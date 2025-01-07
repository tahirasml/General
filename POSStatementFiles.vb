Imports System.IO
Imports System.Threading
Imports SIBL0100.Util
Imports CRP0100
Imports TDXL0100.INIParameters.Statement
Imports VDXExtractDB

''' <summary>
''' This class collects the .dat input files and creates an object of clsStatements for each file, then asks it to generate the Statements in it.
''' This class will pass each .dat file to the clsStatement to process it and generate .pdf from it,
''' and insert a record for each .pdf generated into the fileStats table, and if this statement is
''' supposed to go to CIB, or RIB, it will create a record for each site into fileUpload, and fileQue tables.
''' </summary>
''' <remarks></remarks>
Public Class POSStatementFiles

#Region " Fields "

    Private m_currentStatement As POSclsStatement ' current running Statement
    Private iOverAllFileCount As Integer = 0 'Files counter


    Private _IsCanceledJobs As Boolean = False ' if this is set to true, jobs will be canceled
    Private _IsCanceledJobsSyncLock As New Object 'This is for thread safety of filed _IsCanceledJobs


    Public isShowReportPreview As Boolean = False 'this will show the report view of the current generated report
    Public _rptViewer As CCrystalReport = Nothing 'main report viewer for all Statements

    Private _StatementsLog As POSStatementFilesLog 'Database logger for each input file

    Private _reports As ReportCollection 'Templates for Statement Reports

    Private _UIControl As IStatementsForm 'UI notifier
    Private _OutputDirectoryPath As String 'the place for .pdf output folder

    Private _isMoreFiles As Boolean = False 'set to true if there are more files in the input or work folder
    Private _isProcessFiles_Finished As Boolean = False 'indicates that the processed files are finished
    Private _templatePath As String 'path for the .pdf templates
    Private _PDFMetaData As New pdfLib.PDFMetaData 'pdf main metadata

    Private _report_Max_Usage As Integer = 1 'maximum use of a report object, this filed is for a work around for a bug in crystal reports, that when an app uses a report for hundreds of times, memory leaks, so this will dispose of the report after its created n times.
    Private _subFolder As String 'Last folder name in the statemet folder heirarchy
    Public Event InputFileChanged As SIBL0100.ProgressTrack.CountChangedEventHandler

    Private m_DBFileNo As Integer = 0 'database file number in order to save the last database number
    Private m_DBExecutionNo As Integer = 0 'database file execution number in order to resume processing the file with same execution number
    Private m_Month As String
    Private m_Year As String
    Private CurrentBatchCount As Integer = 0



#Region "Constants"

    Public Const C_TCSTReportTemplateName As String = "VDXF0110.TCST.GENSTM.{0}.{1}.{2}.rpt" '0:Lang, 1:Rank, 2:Version,  ' Report naming convinsion
    Public Const C_SCSTReportTemplateName As String = "VDXF0110.SCST.GENSTM.{0}.{1}.{2}.rpt" '0:Lang, 1:Rank, 2:Version,  ' Report naming convinsion
    Public Const C_ReportSummaryTemplateName As String = "VDXT0300.rpt" ' Summery Report naming convinsion


#End Region

#End Region

#Region " Properties "

    Public Property FilesProcessSummary As New ProcessSummary 'The over all process summery of all processed .dat files

    Public Property FileIndex As Integer = 0 '.dat file index
    Public Property isExport As Boolean = True 'allow exporting

    Public Property isMoreFiles As Boolean
        Get
            Return _isMoreFiles
        End Get
        Set(value As Boolean)

        End Set
    End Property

    Public ReadOnly Property isProcessFiles_Finished As Boolean
        Get
            Return _isProcessFiles_Finished
        End Get
    End Property

    Public ReadOnly Property StatementsLog() As POSStatementFilesLog
        Get
            Return _StatementsLog
        End Get
    End Property


    Public Property IsCanceledJobs() As Boolean
        Get
            SyncLock (_IsCanceledJobsSyncLock)
                Return _IsCanceledJobs
            End SyncLock
        End Get
        Set(ByVal Value As Boolean)
            SyncLock (_IsCanceledJobsSyncLock)
                _IsCanceledJobs = Value
            End SyncLock
        End Set
    End Property

    Public Property currentStatement() As POSclsStatement
        Get
            Return m_currentStatement
        End Get
        Set(ByVal Value As POSclsStatement)
            m_currentStatement = Value

        End Set
    End Property

    Public Property DBFileNumber() As Integer
        Get
            Return m_DBFileNo
        End Get
        Set(value As Integer)
            m_DBFileNo = value
        End Set

    End Property

    Public Property DBFileExecutionNumber() As Integer
        Get
            Return m_DBExecutionNo

        End Get
        Set(value As Integer)
            m_DBExecutionNo = value
        End Set

    End Property

    Public Property StatementMonth() As String
        Get
            Return m_Month

        End Get
        Set(value As String)
            m_Month = value
        End Set

    End Property

    Public Property StatementYear() As String
        Get
            Return m_Year

        End Get
        Set(value As String)
            m_Year = value
        End Set

    End Property
#End Region

    Public Class CustomerList 'The list of files in the directories
        Public AllCustomers() As Structures.CusDBStm_Hdr 'All files
        Public count As Integer
        Public SkippedCustomers As New Collections.ArrayList 'Only skipped files
        Public FailedCustomers As New Collections.ArrayList 'Only Failed files
    End Class

    ''' <summary>
    ''' constructor for the StatementsFiles, that will initialize its fields
    ''' </summary>
    ''' <param name="OutputDirectoryPath_"></param>
    ''' <param name="templatePath_"></param>
    ''' <param name="PDFMetaData_"></param>
    ''' <param name="report_Max_Usage_"></param>
    ''' <param name="subField_"></param>
    ''' <remarks></remarks>
    Public Sub New(UIControl_ As IStatementsForm, OutputDirectoryPath_ As String, templatePath_ As String _
                   , PDFMetaData_ As pdfLib.PDFMetaData _
                   , report_Max_Usage_ As Integer, subField_ As String)

        _UIControl = UIControl_
        With FilesProcessSummary
            '.CurrentFileNum = 0
            .EndTime = Nothing
            .Failed = 0
            .Process = 0
            .Skip = 0
            .StartTime = Nothing
            .Total = 0

        End With

        'creates the statements loger to db
        _StatementsLog = New POSStatementFilesLog(ServicesUti.AppPaths.singleton.LogFilePath, ServicesUti.Services.Singleton.C_AppName)

        Me._isMoreFiles = False 'Defualt to false
        Me._isProcessFiles_Finished = False 'Defualt to false

        Me._OutputDirectoryPath = OutputDirectoryPath_
        Me._templatePath = templatePath_
        Me._PDFMetaData = PDFMetaData_

        Me._report_Max_Usage = report_Max_Usage_
        Me._subFolder = subField_

    End Sub

#Region "Events"


    Public Sub Grid_MakeNewRow(ByVal status_ As String)
        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, status_, LogLevel:=3)
    End Sub

    ''' <summary>
    ''' This will call a function to show a dialog for the user, to select one of its options
    ''' </summary>
    ''' <param name="fileProcessStartDate_"></param>
    ''' <param name="totalNumberOfStatements_"></param>
    ''' <param name="CurrentStatementNumber_"></param>
    ''' <param name="sPassedError_"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function AskUser(ByVal fileProcessStartDate_ As Date, ByVal totalNumberOfStatements_ As Integer, ByVal CurrentStatementNumber_ As Integer, _
                     ByVal sPassedError_ As String) As frmFileErrorB.enumFileErrorUserSelection
        Dim retVal As frmFileErrorB.enumFileErrorUserSelection
        retVal = Me._UIControl.AskUser(fileProcessStartDate_, totalNumberOfStatements_, CurrentStatementNumber_, "", sPassedError_)
        Return retVal

    End Function

#End Region

    ''' <summary>
    ''' Starts the processing of .dat files
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub StartProcess()
        Dim isRetryVisible As Boolean = False
        Dim isThereCustomers As Boolean = False
        Dim startIdx As Integer = 1
        Dim CurrentIdx As Integer = 0

        Dim AllCustomersCount As Integer = 0
        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            Dim cusList As New CustomerList

            _StatementsLog.start()
            Me._isMoreFiles = False

            IsCanceledJobs = False
            iOverAllFileCount = 0
            RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", 0, 0, 0, True))

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "Processing of statement files started - " & Now.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), )

            If Not initCrystalReportViewer() Then 'Inits the crystal report object
                Exit Sub
            End If
            _reports = getReportTemplates()

            Me.FilesProcessSummary.OverAllFileNo = getMaxFileNo()
            Me.FilesProcessSummary.ExecutionNo = getMaxExecutionNo() + 1

            Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
            AllCustomersCount = oDBProvider.getCustomerCount(INIParameters.Singlton.Glob.OnlyStaff)
            Dim LastId As Integer = 1
            Me.StatementsLog.CustStats_get(m_Month, m_Year, LastId, 0) ' 0 for travel extract last ID

            Me.FilesProcessSummary.Total = AllCustomersCount
            RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", AllCustomersCount, 0, 0, False))
            cusList = getCustomers(LastId, INIParameters.Singlton.Glob.BatchSize, startIdx)
            isThereCustomers = cusList.AllCustomers.Length > 0
            'CurrentBatchCount += cusList.count
            Me.FilesProcessSummary.StartTime = Now
            Me.FilesProcessSummary.Skip = 0
            Me.FilesProcessSummary.Failed = 0
            While isThereCustomers
                With Me.FilesProcessSummary
                    If cusList.AllCustomers.Length > 0 Then
                        processCustomers(cusList)
                        .EndTime = Now
                        If IsCanceledJobs Then Exit Sub
                    Else
                        'ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("[input] directory [{0}]: does not have files with ext [{1}].", Me._InputFilePath, _inputFileExtension))
                    End If
                    'startIdx += 1
                    cusList = getCustomers(CurrentBatchCount, INIParameters.Singlton.Glob.BatchSize, startIdx)

                    isThereCustomers = cusList.count > 0
                    'CurrentBatchCount += cusList.count
                End With

            End While


        Catch ex As ThreadInterruptedException
            Debug.WriteLine("ThreadInterrupted")
        Catch ex As ThreadAbortException
            Debug.WriteLine("Thread Abort")
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An Unknown error has occurred while processing.", "Unknown Error : " & ex.Message, ex, , , True)
            Me._UIControl.SetState(UIState.Cancel)
        Finally
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("Processing of travel card statements completed - {0} - {1}", Now.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), UDate.getTimeDiffrence(Me.FilesProcessSummary.StartTime)))

            If Not isRetryVisible Then
                Me._UIControl.SetState(UIState.ReadyOnAllThreadsFinished)
            End If

            Me.FilesProcessSummary.EndTime = Now
            IsCanceledJobs = True
            RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", 0, 0, 0, False))

            gCrystalViewer.Dispose()
            gCrystalViewer = Nothing


            If isThereCustomers = True Then
                Me._isMoreFiles = True
            End If

            Me._isProcessFiles_Finished = True
        End Try
        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "----------------------------------------- ")
    End Sub

    ''' <summary>
    ''' return a collection of report templates
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getReportTemplates() As ReportCollection
        Dim oRptColec As New ReportCollection(gCrystalViewer)
        Dim sRptName As String
        Dim iMax As Integer = 0

        iMax = Me._report_Max_Usage
        'Ar, WICN
        '        This is for travel card:
        '       VDXF0110.TCST.GENSTM.{0}.{1}.{2}.rpt

        '        This is for shopping card:
        '       VDXF0110.TCST.GENSTM.{0}.{1}.{2}.rpt
        For iInd As Integer = 0 To 0
            sRptName = String.Format(C_TCSTReportTemplateName, "AR", iInd.ToString, "01")
            oRptColec.addReport(sRptName, IO.Path.Combine(_templatePath, sRptName), iMax)
            sRptName = String.Format(C_SCSTReportTemplateName, "AR", iInd.ToString, "01")
            oRptColec.addReport(sRptName, IO.Path.Combine(_templatePath, sRptName), iMax)
        Next
        'VDXF0110.STMT.GENSTM.AR.0.01.WICN
        'En, WICN
        For iInd As Integer = 0 To 0
            sRptName = String.Format(C_TCSTReportTemplateName, "EN", iInd.ToString, "01")
            oRptColec.addReport(sRptName, IO.Path.Combine(_templatePath, sRptName), iMax)
            sRptName = String.Format(C_SCSTReportTemplateName, "EN", iInd.ToString, "01")
            oRptColec.addReport(sRptName, IO.Path.Combine(_templatePath, sRptName), iMax)
        Next

        sRptName = POSStatementFiles.C_ReportSummaryTemplateName
        oRptColec.addReport(sRptName, IO.Path.Combine(_templatePath, sRptName), iMax)


        Return oRptColec
    End Function

    ''' <summary>
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getCustomers(startIdx As Integer, PatchSize As Integer, PatchId As Integer) As CustomerList
        Dim oFiles As New CustomerList
        Dim sArr As New Collections.ArrayList
        Dim count As Integer
        Dim lRow As DataRow
        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider

            'Me.FilesProcessSummary.Total += oDBProvider.getCustomerCount()

            Dim Records As New DataSet
            Dim Record As New Structures.CusDBStm_Hdr
            count = oDBProvider.CustomerInfoExtract(startIdx, PatchSize, PatchId, Records)
            If count > 0 Then
                ReDim oFiles.AllCustomers(count - 1)
                oFiles.count = count
                For i As Integer = 0 To Records.Tables(0).Rows.Count - 1
                    lRow = Records.Tables(0).Rows(i)

                    With Record
                        .Clear()
                        .StatementNumber = lRow(0).ToString.Trim
                        .CusNum = lRow(1).ToString.Trim
                        .CusSeg = lRow(2).ToString.Trim
                        .CusLng = lRow(3).ToString.Trim
                        .CusArTit = lRow(4).ToString.Trim
                        .CusArNam1 = lRow(5).ToString.Trim
                        .CusArNam2 = lRow(6).ToString.Trim
                        .CusArNam3 = lRow(7).ToString.Trim
                        .CusArNam4 = lRow(8).ToString.Trim
                        .CusEnTit = lRow(9).ToString.Trim
                        .CusEnNam1 = lRow(10).ToString.Trim
                        .CusEnNam2 = lRow(11).ToString.Trim
                        .CusEnNam3 = lRow(12).ToString.Trim
                        .CusEnNam4 = lRow(13).ToString.Trim
                        .CusArAddLin1 = lRow(14).ToString.Trim
                        .CusArAddLin2 = lRow(15).ToString.Trim
                        .CusArAddLin3 = lRow(16).ToString.Trim
                        .CusArAddLin4 = lRow(17).ToString.Trim
                        .CusEnAddLin1 = lRow(18).ToString.Trim
                        .CusEnAddLin2 = lRow(19).ToString.Trim
                        .CusEnAddLin3 = lRow(20).ToString.Trim
                        .CusEnAddLin4 = lRow(21).ToString.Trim
                        .NoOfTrvWal = lRow(22).ToString.Trim
                        .NoOfShpWal = lRow(23).ToString.Trim
                        .CusID = CInt(lRow(24))
                    End With
                    oFiles.AllCustomers(i) = Record
                Next
            Else
                ReDim oFiles.AllCustomers(0)
                oFiles.count = 0
            End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while getCustomers.", "Unknown Error : " & ex.Message, ex, , , True)
        End Try

        Return oFiles
    End Function

    ''' <summary>
    ''' Get the maximum file number reached till now
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getMaxFileNo() As Integer
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        Return oDBProvider.getMaxFileNo()

    End Function

    ''' <summary>
    ''' Get the maximum execution number reached till now
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getMaxExecutionNo() As Integer
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        Dim iCurrentExecutionNo As Integer = 0

        iCurrentExecutionNo = oDBProvider.getMaxExecutionNo()
        oDBProvider.tblSequence_Update_fileStatsExecutionNo()

        Return iCurrentExecutionNo
    End Function

    ''' <summary>
    ''' starts creating .pdf from .dat files
    ''' </summary>
    ''' <remarks></remarks>
    Sub processCustomers(ByVal Customers As CustomerList)
        Dim isRetryVisible As Boolean = False
        Dim isOK_ProcessStatements As Boolean = False


        Dim OutDirRoot_Unit As String = String.Empty
        Dim timeStart As DateTime = Nothing
        Dim timeEnd As DateTime = Nothing
        Dim fileLines As Integer
        Dim filePages As Integer
        Dim lsCustomerRecord As Structures.CusDBStm_Hdr
        Dim tmpExecutionNo As Integer


        Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 9)

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)
            OutDirRoot_Unit = IO.Path.Combine(Me._OutputDirectoryPath, ServicesUti.Services.Singleton.AppInstance.Unit)

            With Me.FilesProcessSummary


                For Each lsCustomerRecord In Customers.AllCustomers
                    If Me.IsCanceledJobs Then
                        Exit Sub
                    End If

                    timeStart = Now
                    .OverAllFileNo += 1

                    iOverAllFileCount += 1
                    fileLines = 0
                    filePages = 0
                    CurrentBatchCount = lsCustomerRecord.CusID

                    RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", Me.FilesProcessSummary.Total, iOverAllFileCount, 0, False))

                    'Me.FileIndex = .CurrentFileNum - 1
                    If lsCustomerRecord.NoOfTrvWal.Trim <> "" AndAlso CInt(lsCustomerRecord.NoOfTrvWal.Trim) > 0 Then
                        m_currentStatement = New POSclsStatement(Me, Me._PDFMetaData, Me._report_Max_Usage, Me._subFolder)
                        Me.StatementsLog.CustStats_Update(Me.m_Month, Me.m_Year, lsCustomerRecord.CusID, lsCustomerRecord.CusNum, 0)
                        'm_currentStatement.StartDate

                        tmpExecutionNo = .ExecutionNo
                        ' we will delete this record in case of resume file
                        With m_currentStatement
                            .isExport = Me.isExport
                            .OutDirRoot_Unit = OutDirRoot_Unit
                            .FileState = POSclsStatement.enumFileState.OK
                            .Reports = Me._reports
                            .Customer = lsCustomerRecord
#If WalletSplit Then
                            isOK_ProcessStatements = .ProcessStatements_WalletSplit()
#Else
                            isOK_ProcessStatements = .ProcessStatements_AllWallets

#End If

                            timeEnd = Now
                        End With


                        If IsCanceledJobs Then
                            If m_currentStatement.StatmentStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError Then
                                Me._UIControl.SetState(UIState.StopTotaly)
                                Exit Sub
                            Else
                                isRetryVisible = True
                                Me._UIControl.SetState(UIState.Cancel)
                            End If
                        End If

                        'Me.FilesProcessSummary.Add(m_currentStatement.StatementSummary)
                        m_currentStatement.Dispose()
                        m_currentStatement = Nothing
                    End If
                    

                Next

            End With
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An Unknown error has occurred while processing.", "Unknown Error : " & ex.Message, ex)
        Finally
            If Not IsCanceledJobs Then ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("Processing travel card statements batch completed - {0}", Now))
        End Try

    End Sub

End Class

''' <summary>
''' This will insert/update table records for each .dat file being processed and each .pdf statement file being generated.
''' </summary>
''' <remarks></remarks>
Public Class POSStatementFilesLog
    Implements IDisposable

    Private _fileDate As Date
    Private _path As String
    Private _filePrefix As String
    'Private _FileWriter As StreamWriter = Nothing
    Private lockObject As New Object

    Private _totalStartDate As DateTime
    Private _totalEndDate As DateTime
    Private _totalNumberOfStatements As Long = 0
    Private _totalNumberOfRecords As Long = 0
    Private _LogEntrySequence As Integer = 0


    Private _Execution_PagesOfAllStatements As Long = 0

    Const C_Sp As String = vbTab 'Seperator

    Sub New(path_ As String, filePrefix_ As String)
        _fileDate = Date.Now
        _path = path_
        _filePrefix = filePrefix_

        start()
    End Sub

    ''' <summary>
    ''' Inits the local fields
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub start()
        _totalStartDate = Date.Now
        _totalEndDate = Nothing
        _totalNumberOfStatements = 0
        _totalNumberOfRecords = 0
    End Sub

    Public Sub CustStats_Update(Month As String, Year As String, CusID As Integer, CusNum As String, ExtractType As Integer)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        oDBProvider.CustStats_Update(Month, Year, CusID, CusNum, ExtractType)
    End Sub

    Public Sub CustStats_get(Month As String, Year As String, ByRef CusID As Integer, ExtractType As Integer)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        oDBProvider.CustStats_get(Month, Year, CusID, ExtractType)
    End Sub
   
    Public Sub fileStats_UpdateStatus(fileNo_ As Integer, Start_ As DateTime, end_ As DateTime, ProcessStatus_ As String)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        oDBProvider.fileStats_UpdateStatus(fileNo_, Start_, end_, ProcessStatus_)
    End Sub

   

    'Protected Overrides Sub Finalize()
    '    'CloseLog()
    '    MyBase.Finalize()
    'End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        'If disposing Then
        '    If _FileWriter IsNot Nothing Then
        '        _FileWriter.Dispose()
        '        _FileWriter = Nothing
        '    End If
        'End If
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

End Class
