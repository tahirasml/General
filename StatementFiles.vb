Imports System.IO
Imports System.Threading
Imports SIBL0100.Util
Imports CRP0100
Imports TDXL0100.INIParameters.Statement
Imports VDXExtractDB
Imports System.IO.File
Imports System.Linq
Imports System.Globalization

''' <summary>
''' This class collects the .dat input files and creates an object of clsStatements for each file, then asks it to generate the Statements in it.
''' This class will pass each .dat file to the clsStatement to process it and generate .pdf from it,
''' and insert a record for each .pdf generated into the fileStats table, and if this statement is
''' supposed to go to CIB, or RIB, it will create a record for each site into fileUpload, and fileQue tables.
''' </summary>
''' <remarks></remarks>
Public Class StatementFiles

#Region " Fields "

    Private m_currentStatement As clsStatement ' current running Statement
    Private iOverAllFileCount As Integer = 0 'Files counter


    Private _IsCanceledJobs As Boolean = False ' if this is set to true, jobs will be canceled
    Private _IsCanceledJobsSyncLock As New Object 'This is for thread safety of filed _IsCanceledJobs


    Private _StatementsLog As StatementFilesLog 'Database logger for each input file
    Private _UIControl As IStatementsForm 'UI notifier


    Private _InputFilePath As String 'the Input file path
    Private _WorkDirectoryPath As String 'Work directory, which will be a temporary location for work.
    Private _OutputDirectoryPath As String 'the place for .pdf output folder
    Private _ArchiveDirectoryPath As String 'finished .dat files will be archived to this folder
    Private _BadFilesDirectoryPath As String 'bad .dat files that have problems into them, will be sent to this folder

    Private _isMoreFiles As Boolean = False 'set to true if there are more files in the input or work folder
    Private _isProcessFiles_Finished As Boolean = False 'indicates that the processed files are finished

    Private _inputFileExtension As String 'extension of the input files '.dat'
    Public Event InputFileChanged As SIBL0100.ProgressTrack.CountChangedEventHandler

    Private m_DBFileNo As Integer = 0 'database file number in order to save the last database number
    Private m_DBExecutionNo As Integer = 0 'database file execution number in order to resume processing the file with same execution number

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

    Public ReadOnly Property StatementsLog() As StatementFilesLog
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

    Public Property currentStatement() As clsStatement
        Get
            Return m_currentStatement
        End Get
        Set(ByVal Value As clsStatement)
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
#End Region

    Public Class FilesList 'The list of files in the directories
        Public AllFiles() As String 'All files
        Public SkippedFiles As New Collections.ArrayList 'Only skipped files
        Public FailedFiles As New Collections.ArrayList 'Only Failed files
    End Class

    ''' <summary>
    ''' constructor for the StatementsFiles, that will initialize its fields
    ''' </summary>
    ''' <param name="InputFilePath_"></param>
    ''' <param name="WorkDirectoryPath_"></param>
    ''' <param name="OutputDirectoryPath_"></param>
    ''' <param name="ArchiveDirectoryPath_"></param>
    ''' <param name="BadFilesDirectoryPath_"></param>
    ''' <param name="inputFileExtension_"></param>
    ''' <remarks></remarks>
    Public Sub New(UIControl_ As IStatementsForm, InputFilePath_ As String, WorkDirectoryPath_ As String, OutputDirectoryPath_ As String, ArchiveDirectoryPath_ As String, BadFilesDirectoryPath_ As String _
                   , inputFileExtension_ As String)

        _UIControl = UIControl_
        With FilesProcessSummary
            .CurrentCusNum = 0
            .EndTime = Nothing
            .Failed = 0
            .Process = 0
            .Skip = 0
            .StartTime = Nothing
            .Total = 0

        End With

        'creates the statements loger to db
        _StatementsLog = New StatementFilesLog(ServicesUti.AppPaths.singleton.LogFilePath, ServicesUti.Services.Singleton.C_AppName)

        Me._isMoreFiles = False 'Defualt to false
        Me._isProcessFiles_Finished = False 'Defualt to false

        Me._InputFilePath = InputFilePath_
        Me._WorkDirectoryPath = WorkDirectoryPath_
        Me._OutputDirectoryPath = OutputDirectoryPath_
        Me._ArchiveDirectoryPath = ArchiveDirectoryPath_
        Me._BadFilesDirectoryPath = BadFilesDirectoryPath_
        Me._inputFileExtension = inputFileExtension_
    End Sub

#Region "Events"

    ''' <summary>
    ''' This updates the grid ui
    ''' </summary>
    ''' <param name="status_"></param>
    ''' <remarks></remarks>
    ''' <Later>
    ''' This should be an event
    '''</Later>
    Public Sub Grid_UpdateRow(Optional ByVal status_ As String = "")
        'Me._UIControl.Grid_UpdateRow(status_, m_currentStatement)
       

    End Sub

    ''' <summary>
    ''' This will make a new row in the grid
    ''' </summary>
    ''' <param name="image_index"></param>
    ''' <param name="item_title"></param>
    ''' <param name="subitem_titles"></param>
    ''' <remarks></remarks>
    ''' <Later>
    ''' This should be an event
    '''</Later>
    Public Sub Grid_MakeNewRow(ByVal image_index As Integer, ByVal item_title As String, ByVal subitem_titles() As String)
        'Me._UIControl.Grid_MakeNewRow(image_index, item_title, subitem_titles)
    End Sub

    ''' <summary>
    ''' This will call a function to show a dialog for the user, to select one of its options
    ''' </summary>
    ''' <param name="fileProcessStartDate_"></param>
    ''' <param name="totalNumberOfStatements_"></param>
    ''' <param name="CurrentStatementNumber_"></param>
    ''' <param name="workingFile_"></param>
    ''' <param name="sPassedError_"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function AskUser(ByVal fileProcessStartDate_ As Date, ByVal totalNumberOfStatements_ As Integer, ByVal CurrentStatementNumber_ As Integer, ByVal workingFile_ As String, _
                     ByVal sPassedError_ As String) As frmFileErrorB.enumFileErrorUserSelection
        Dim retVal As frmFileErrorB.enumFileErrorUserSelection
        retVal = Me._UIControl.AskUser(fileProcessStartDate_, totalNumberOfStatements_, CurrentStatementNumber_, workingFile_, sPassedError_)
        Return retVal

    End Function

#End Region

    ''' <summary>
    ''' Starts the processing of .dat files
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub StartProcess()
        Dim isRetryVisible As Boolean = False
        Dim isThereFilesInInputFolder As Boolean = False
        Dim isThereFilesInWorkFolder As Boolean = False

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            Dim oFiles As New FilesList

            _StatementsLog.start()
            Me._isMoreFiles = False

            IsCanceledJobs = False
            iOverAllFileCount = 0
            RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", 0, 0, 0, True))

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "Processing of statement files started - " & Now.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), )
            Me.FilesProcessSummary.OverAllFileNo = getMaxFileNo()
            Me.FilesProcessSummary.ExecutionNo = getMaxExecutionNo() + 1

            With Me.FilesProcessSummary
                Me.FilesProcessSummary.StartTime = Now
                .CurrentCusNum = 0
                .Total = 0
                .Skip = 0
                .Failed = 0

                'Start by processing .dat files in 'Work Folder'
                oFiles = getFiles(Me._WorkDirectoryPath)

                If oFiles.AllFiles.Length > 0 Then
                    isThereFilesInWorkFolder = oFiles.AllFiles.Length > 0

                    RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", oFiles.AllFiles.Length, 0, 0, False))
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("[work] directory [{0}]: Processing ({1}) files in the [{2}] with ext [{3}].", Me._WorkDirectoryPath, oFiles.AllFiles.Length, Me._WorkDirectoryPath.ToUpper, _inputFileExtension))

                    If IsCanceledJobs Then Exit Sub
                    processFiles(oFiles, Me._WorkDirectoryPath)
                    .EndTime = Now
                    If IsCanceledJobs Then Exit Sub
                Else
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("[work] directory [{0}]: does not have files with ext [{1}].", Me._WorkDirectoryPath, _inputFileExtension))
                End If

                If Me.IsCanceledJobs Then
                    Exit Sub
                End If

                'Second by processing .dat files in 'Input Folder'
                oFiles = getFiles(Me._InputFilePath)

                If oFiles.AllFiles.Length > 0 Then
                    isThereFilesInInputFolder = oFiles.AllFiles.Length > 0
                    'prgBarFiles.Maximum += oFiles.AllFiles.Length
                    RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", oFiles.AllFiles.Length, 0, 0, False))
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("[input] directory [{0}]: Processing ({1}) files in the [{2}] with ext [{3}].", Me._InputFilePath, oFiles.AllFiles.Length, Me._InputFilePath.ToUpper, _inputFileExtension))

                    processFiles(oFiles, Me._InputFilePath)
                    .EndTime = Now
                    If IsCanceledJobs Then Exit Sub
                Else
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("[input] directory [{0}]: does not have files with ext [{1}].", Me._InputFilePath, _inputFileExtension))
                End If

            End With

        Catch ex As ThreadInterruptedException
            Debug.WriteLine("ThreadInterrupted")
        Catch ex As ThreadAbortException
            Debug.WriteLine("Thread Abort")
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An Unknown error has occurred while processing.", "Unknown Error : " & ex.Message, ex, , , True)
            Me._UIControl.SetState(UIState.Cancel)
        Finally
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("5Processing of statement files completed - {0} - {1}", Now.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), UDate.getTimeDiffrence(Me.FilesProcessSummary.StartTime)))

            If Not isRetryVisible Then
                Me._UIControl.SetState(UIState.ReadyOnAllThreadsFinished)
            End If

            Me.FilesProcessSummary.EndTime = Now
            IsCanceledJobs = True
            RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", 0, 0, 0, False))

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, _StatementsLog.logEndProcess())

            If isThereFilesInInputFolder = True OrElse isThereFilesInWorkFolder = True Then
                Me._isMoreFiles = True
            End If

            Me._isProcessFiles_Finished = True
        End Try
        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "----------------------------------------- ")
    End Sub

   

    ''' <summary>
    ''' Gets the files in path_
    ''' </summary>
    ''' <param name="path_"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getFiles(ByVal path_ As String) As FilesList
        'Dim oFiles As New FilesList
        Dim AllFolderFiles() As String
        Dim sArr As New Collections.ArrayList

        Dim oFiles As New FilesList
        Dim lFileSource As String = ""
        Dim oDirInfo As System.IO.DirectoryInfo
        Dim oInfo As System.IO.FileInfo
        Dim SearchExtension As String
        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            lFileSource = Trim(path_)
            If Not (lFileSource.EndsWith("\")) Then lFileSource = lFileSource & "\"

            oDirInfo = New System.IO.DirectoryInfo(lFileSource)
            SearchExtension = "*" & _inputFileExtension

            AllFolderFiles = Directory.GetFiles(path_)
            For Each sFile As String In AllFolderFiles
                If IO.Path.GetExtension(sFile).ToLower = _inputFileExtension Then
                    sArr.Add(sFile)
                Else
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, String.Format("[{0}] skipped because of bad file name, its extension is not '{1}'.", sFile, _inputFileExtension), , LogLevel:=1)
                End If
            Next

            If sArr.Count = 0 Then
                oFiles.AllFiles = DirectCast(sArr.ToArray(GetType(String)), String())
                Return oFiles
            End If
            sArr.Clear()
            oInfo = oDirInfo.GetFiles(SearchExtension).OrderByDescending(Function(p) p.LastWriteTime).First()
            'AllFolderFiles = Directory.GetFiles(path_).OrderByDescending(Function(p) p.LastWriteTime).First()
            If Not oInfo Is Nothing Then
                Dim FileName As String
                FileName = oInfo.Name
                If IO.Path.GetExtension(FileName).ToLower = _inputFileExtension Then
                    'Check file naming
                    sArr.Add(oInfo.FullName)
                Else
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, String.Format("[{0}] skipped because of bad file name, its extension is not '{1}'.", FileName, _inputFileExtension), , LogLevel:=1)
                End If
            End If
            'For Each sFile As String In AllFolderFiles

            'Next

            oFiles.AllFiles = DirectCast(sArr.ToArray(GetType(String)), String())

            'Me.FilesProcessSummary.Total += oFiles.AllFiles.Length

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while getFiles.", "Unknown Error : " & ex.Message, ex, , , True)
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
    ''' <param name="oFiles"></param>
    ''' <param name="sCurrentDirectory_"></param>
    ''' <remarks></remarks>
    Sub processFiles(ByVal oFiles As FilesList, sCurrentDirectory_ As String)
        Dim isRetryVisible As Boolean = False
        Dim isOK_ProcessStatements As Boolean = False

        Dim OutDirRoot_Unit As String = String.Empty
        Dim timeStart As DateTime = Nothing
        Dim timeEnd As DateTime = Nothing

        Dim fileLines As Integer
        Dim filePages As Integer
        Dim lsFilePath As String
        Dim tmpCurrentFileParentNo As Integer
        Dim tmpExecutionNo As Integer


        Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 9)

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            OutDirRoot_Unit = IO.Path.Combine(Me._OutputDirectoryPath, ServicesUti.Services.Singleton.AppInstance.Unit)


            With Me.FilesProcessSummary

                For Each lsFilePath In oFiles.AllFiles
                    If Me.IsCanceledJobs Then
                        Exit Sub
                    End If

                    timeStart = Now
                    .OverAllFileNo += 1
                    .CurrentCusNum += 1
                    iOverAllFileCount += 1
                    fileLines = 0
                    filePages = 0

                    RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", oFiles.AllFiles.Length, iOverAllFileCount, 0, False))

                    Me.FileIndex = .CurrentCusNum - 1

                    m_currentStatement = New clsStatement(Me, sCurrentDirectory_, Me._WorkDirectoryPath, Me._ArchiveDirectoryPath, Me._BadFilesDirectoryPath _
                                                          , Me._inputFileExtension)


                    Me.StatementsLog.fileStats_Insert(.OverAllFileNo, .ExecutionNo, sCurrentDirectory_, lsFilePath, timeStart, "Start Process")
                    tmpExecutionNo = .ExecutionNo
                    ' we will delete this record in case of resume file
                    DBFileExecutionNumber = .ExecutionNo
                    'Dim OldCultInfo, NewCultInfo As CultureInfo
                    'OldCultInfo = System.Threading.Thread.CurrentThread.CurrentCulture
                    'NewCultInfo = New CultureInfo("ar-SA", False)
                    'System.Threading.Thread.CurrentThread.CurrentCulture = NewCultInfo
                    With m_currentStatement
                        .isExport = Me.isExport
                        .OutDirRoot_Unit = OutDirRoot_Unit
                        .FilePath = lsFilePath
                        .FileState = clsStatement.enumFileState.OK

                        _StatementsLog.log(IO.Path.GetDirectoryName(.FilePath), IO.Path.GetFileName(.FilePath), Date.Now, 0)
                        isOK_ProcessStatements = .ProcessStatements()

                        timeEnd = Now
                        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, _StatementsLog.log(IO.Path.GetDirectoryName(.FilePath), IO.Path.GetFileName(.FilePath) _
                                             , .StatementSummary.StartTime, .StatementSummary.FileTotalLines))

                    End With
                    If Not isOK_ProcessStatements Then
                        If m_currentStatement.FileState = clsStatement.enumFileState.Skipped Then
                            oFiles.SkippedFiles.Add(lsFilePath)

                        ElseIf m_currentStatement.FileState = clsStatement.enumFileState.Failed Then

                            oFiles.FailedFiles.Add(lsFilePath)
                            Me.StatementsLog.fileStats_DeleteAll(tmpCurrentFileParentNo, tmpExecutionNo, True)
                        ElseIf m_currentStatement.StatmentStatus = frmFileErrorB.enumFileErrorUserSelection.Abort Then

                        End If

                        If m_currentStatement.StatmentStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError Then
                            IsCanceledJobs = True
                        End If

                    Else
                        Me.StatementsLog.fileStats_Update(.OverAllFileNo, m_currentStatement.StartDate, m_currentStatement.EndDate, m_currentStatement.StatementSummary.Total, "", "Completed Successfully")

                    End If

                    'With Me.m_statementFiles.FilesProcessSummary
                    '    Me.m_statementFiles.StatementsLog.fileStats_Update(.OverAllFileNo, currentStatmentStart, Now, _CurrentStatementLines, "", "")
                    'End With
                    If IsCanceledJobs Then
                        If m_currentStatement.StatmentStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError Then
                            Me._UIControl.SetState(UIState.StopTotaly)
                            Exit Sub
                        Else
                            isRetryVisible = True
                            Me._UIControl.SetState(UIState.Cancel)
                        End If
                    End If

                    Me.FilesProcessSummary.Add(m_currentStatement.StatementSummary)
                    'System.Threading.Thread.CurrentThread.CurrentCulture = OldCultInfo
                    m_currentStatement.Dispose()
                    m_currentStatement = Nothing

                    GC.Collect()
                Next

            End With
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An Unknown error has occurred while processing.", "Unknown Error : " & ex.Message, ex)
        Finally
            If Not IsCanceledJobs Then ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("6Processing of statement files completed - {0}", Now))
        End Try

    End Sub

End Class

''' <summary>
''' This will insert/update table records for each .dat file being processed and each .pdf statement file being generated.
''' </summary>
''' <remarks></remarks>
Public Class StatementFilesLog
    Implements IDisposable

    Private _fileDate As Date
    Private _path As String
    Private _filePrefix As String
    'Private _FileWriter As StreamWriter = Nothing
    Private lockObject As New Object

    Private _totalStartDate As DateTime
    Private _totalEndDate As DateTime
    Private _totalNumberOfCustomers As Long = 0
    Private _totalNumberOfRecords As Long = 0
    Private _LogEntrySequence As Integer = 0

    Const C_Sp As String = vbTab 'Seperator

    Sub New(path_ As String, filePrefix_ As String)
        _fileDate = Date.Now
        _path = path_
        _filePrefix = filePrefix_

        OpenLog()
        start()
    End Sub

    ''' <summary>
    ''' Inits the local fields
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub start()
        _totalStartDate = Date.Now
        _totalEndDate = Nothing
        _totalNumberOfCustomers = 0
        _totalNumberOfRecords = 0
    End Sub

    ''' <summary>
    ''' Insert a record for each .dat file
    ''' </summary>
    ''' <param name="fileNo_"></param>
    ''' <param name="executionNo_"></param>
    ''' <param name="directory_"></param>
    ''' <param name="stmtFile_"></param>
    ''' <param name="Start_"></param>
    ''' <remarks></remarks>
    Public Sub fileStats_Insert(fileNo_ As Integer, executionNo_ As Integer, directory_ As String, stmtFile_ As String, Start_ As DateTime, ProcessStatus_ As String)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        oDBProvider.fileStats_Insert(fileNo_, executionNo_, directory_, stmtFile_, Start_, ProcessStatus_)
    End Sub

    ''' <summary>
    ''' Delete a record for resuming .dat file
    ''' </summary>
    ''' <param name="fileNo_"></param>
    ''' <param name="executionNo_"></param>
    ''' <remarks></remarks>
    Public Sub fileStats_DeleteAll(fileNo_ As Integer, executionNo_ As Integer, deleteParent_ As Boolean)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        'oDBProvider.fileStats_DeleteAll(fileNo_, executionNo_, deleteParent_)
    End Sub
    ''' <summary>
    ''' update a record for each .dat file, with data after finishing from it
    ''' </summary>
    ''' <param name="fileNo_"></param>
    ''' <param name="Start_"></param>
    ''' <param name="end_"></param>
    ''' <param name="countRecords_"></param>
    ''' <param name="err_"></param>
    ''' <remarks></remarks>
    Public Sub fileStats_Update(fileNo_ As Integer, Start_ As DateTime, end_ As DateTime, countRecords_ As Long, err_ As String, ProcessStatus_ As String)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        oDBProvider.fileStats_Update(fileNo_, Start_, end_, countRecords_, err_, ProcessStatus_)
    End Sub

    Public Sub fileStats_UpdateStatus(fileNo_ As Integer, Start_ As DateTime, end_ As DateTime, ProcessStatus_ As String)
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        oDBProvider.fileStats_UpdateStatus(fileNo_, Start_, end_, ProcessStatus_)
    End Sub

    ''' <summary>
    ''' insert a record for each customer
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub fileStats_InsertHostRecord(ByRef TravelCardRecord As Structures.CusStm_Struct)

        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        Dim records(23) As String
        Dim j As Integer
        'Dim TravelCardDetails() As String
        'Dim ShoppingCardDetails() As String

        records(0) = TravelCardRecord.SHD.StatementNumber.Trim
        records(1) = TravelCardRecord.SHD.CusNum.Trim
        records(2) = TravelCardRecord.SHD.CutSeg.Trim
        records(3) = TravelCardRecord.SHD.CutLng.Trim
        records(4) = TravelCardRecord.SHD.CusArTit.Trim
        records(5) = TravelCardRecord.SHD.CusArNam1.Trim
        records(6) = TravelCardRecord.SHD.CusArNam2.Trim
        records(7) = TravelCardRecord.SHD.CusArNam3.Trim
        records(8) = TravelCardRecord.SHD.CusArNam4.Trim
        records(9) = TravelCardRecord.SHD.CusEnTit.Trim
        records(10) = TravelCardRecord.SHD.CusEnNam1.Trim
        records(11) = TravelCardRecord.SHD.CusEnNam2.Trim
        records(12) = TravelCardRecord.SHD.CusEnNam3.Trim
        records(13) = TravelCardRecord.SHD.CusEnNam4.Trim
        records(14) = TravelCardRecord.SHD.CusArAddLin1.Trim
        records(15) = TravelCardRecord.SHD.CusArAddLin2.Trim
        records(16) = TravelCardRecord.SHD.CusArAddLin3.Trim
        records(17) = TravelCardRecord.SHD.CusArAddLin4.Trim
        records(18) = TravelCardRecord.SHD.CusEnAddLin1.Trim
        records(19) = TravelCardRecord.SHD.CusEnAddLin2.Trim
        records(20) = TravelCardRecord.SHD.CusEnAddLin3.Trim
        records(21) = TravelCardRecord.SHD.CusEnAddLin4.Trim
        records(22) = TravelCardRecord.SHD.NoOfTrvWal.Trim
        records(23) = TravelCardRecord.SHD.NoOfShpWal.Trim
        For j = 0 To records.Length - 1
            records(j) = records(j).Replace("'", "''")
        Next

        'TravelCardRecord.SHD.TvlWalDet
        oDBProvider.InsertHostRecord(records)

        If TravelCardRecord.SHD.NoOfTrvWal.Trim <> "" AndAlso IsNumeric(TravelCardRecord.SHD.NoOfTrvWal) AndAlso CInt(TravelCardRecord.SHD.NoOfTrvWal) > 0 Then
            For i As Integer = 0 To CInt(TravelCardRecord.SHD.NoOfTrvWal) - 1
                With TravelCardRecord.SHD.TvlWalDet(i)
                    .CurInWal = .CurInWal.Trim
                    If .CurInWal.Trim.EndsWith(",") Then .CurInWal = .CurInWal.Substring(0, .CurInWal.Length - 1)
                    .WalNikNam = .WalNikNam.Replace("'", "''")
                    oDBProvider.InsertWalletDetails(.WalId, .WalNikNam, .CreLim, 0, records(1), .CurInWal)

                    'TravelCardDetails = .CurInWal.Split(CChar(","))
                    'For j As Integer = 0 To TravelCardDetails.Length - 1
                    '    oDBProvider.InsertWalletCardsDetails(.WalId, TravelCardDetails(j).Trim, 0, records(1))
                    'Next
                End With

            Next

        End If

        If TravelCardRecord.SHD.NoOfShpWal.Trim <> "" AndAlso IsNumeric(TravelCardRecord.SHD.NoOfShpWal) AndAlso CInt(TravelCardRecord.SHD.NoOfShpWal) > 0 Then
            For i As Integer = 0 To 0
                With TravelCardRecord.SHD.ShpWalDet(i)
                    .ShpCurInWal = .ShpCurInWal.Trim
                    If .ShpCurInWal.Trim.EndsWith(",") Then .ShpCurInWal = .ShpCurInWal.Substring(0, .ShpCurInWal.Length - 1)
                    .ShpWalNikNam = .ShpWalNikNam.Replace("'", "''")
                    oDBProvider.InsertWalletDetails(.ShpWalId, .ShpWalNikNam, .ShpCreLim, 1, records(1), .ShpCurInWal)


                    'ShoppingCardDetails = .CurInWal.Split(CChar(","))
                    'For j As Integer = 0 To ShoppingCardDetails.Length - 1
                    '    oDBProvider.InsertWalletCardsDetails(.WalId, ShoppingCardDetails(j).Trim, 1, records(1))
                    'Next
                End With

            Next

        End If
        'AndAlso IsNumeric(TravelCardRecord.SHD.NoOfTrvWal)

    End Sub


    Public Function log(directory_ As String, sFile_ As String, Start_ As DateTime, countRecords_ As Long) As String

        Dim sData As String = String.Empty
        Dim TotalTime_ As String
        Dim sIn_Out As String = ""
        Dim End_ As DateTime = Now

        _totalNumberOfCustomers += countRecords_


        TotalTime_ = UDate.getTimeDiffrence(Start_, End_)

        Monitor.Enter(lockObject)
        Try
            _LogEntrySequence += 1
            sData = String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", C_Sp, _LogEntrySequence, directory_, sFile_, Start_.ToString(ServicesUti.GlobalVars.C_IO_DateFormat) _
                                  , End_.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), TotalTime_, countRecords_.ToString())

            'If _FileWriter Is Nothing Then
            '    OpenLog()
            'End If
            '_FileWriter.WriteLine(sData)
            '_FileWriter.Flush()
        Catch ex As Exception
            'consume
        End Try

        Monitor.Exit(lockObject)

        Return sData
    End Function


    Public Function logEndProcess() As String
        Dim sData As String = String.Empty
        Dim TotalTime_ As String

        _totalEndDate = Date.Now

        TotalTime_ = UDate.getTimeDiffrence(_totalStartDate, _totalEndDate)

        Monitor.Enter(lockObject)
        Try

            sData = String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}", C_Sp, _LogEntrySequence + 1, "Out", "Total Execution Of all Files (" & _totalNumberOfCustomers & ")", "", _totalStartDate.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), _totalEndDate.ToString(ServicesUti.GlobalVars.C_IO_DateFormat), TotalTime_ _
                                  , _totalNumberOfCustomers, _totalNumberOfRecords)

            'If _FileWriter Is Nothing Then
            '    OpenLog()
            'End If
            '_FileWriter.WriteLine(sData)
            '_FileWriter.Flush()
        Catch ex As Exception
            Debug.WriteLine("")
            'consume
        End Try

        Monitor.Exit(lockObject)
        Return sData
    End Function

    Private Sub OpenLog()
        Try
            'Dim sLogPath As String
            'sLogPath = IO.Path.Combine(_path, _filePrefix & String.Format("_OutputList_{0:yyyy-MM-dd hh-mm-ss}.csv", _fileDate))
            '_FileWriter = New StreamWriter(sLogPath, True)
            '_FileWriter.WriteLine(C_Header)
            '_FileWriter.Flush()
        Catch ex As Exception
            '_FileWriter = Nothing

        End Try
    End Sub

    Private Function CloseLog() As Boolean
        Monitor.Enter(lockObject)
        Try
            'If Not (_FileWriter Is Nothing) Then
            '    _FileWriter.Close()
            '    _FileWriter = Nothing
            'End If
        Catch ex As Exception
            '"Error: " & ex.Message
        End Try
        Monitor.Exit(lockObject)
    End Function


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
