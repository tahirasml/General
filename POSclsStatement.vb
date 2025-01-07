Imports System.IO
Imports System.IO.File
Imports System.Text
Imports sib = SIBL0100
Imports CRP0100
Imports SIBL0100.Util
Imports SIBL0100
Imports TDXL0100.INIParameters.Statement
Imports VDXExtractDB
''' <summary>
''' This class will take one .dat input file and generate from it all its internal statements as .pdf files.
''' It will read some of its data into a buffer by the fileQueue class and then it will upload its data into the datasetStm data set to give it to crystal reports for report creation, and from crystal reports, an export to .pdf file is done.
''' </summary>
Public Class POSclsStatement
    Implements IDisposable

#Region " Variables & Constants "

    Private m_DataTravelSet As New DataSetPOS 'Database of the loaded statement

    Private _StatementStatus As frmFileErrorB.enumFileErrorUserSelection = frmFileErrorB.enumFileErrorUserSelection.Normal

    'Files
    Private m_OutputFileName As String = String.Empty 'the name of the output file
    Private m_pdfFilePathFull As String = String.Empty 'the name of the generated .pdf file name
    Private m_pdfFilePathDirectory As String = String.Empty 'the name of the generated .pdf file name
    'Stats
    Private m_StatementRecords() As String 'the loaded statement records from the data file
    Private m_NextRecordSequence As Integer = 0 'The record internal sequence from the statement
    Private m_totalNumberOfStatements As Integer = 0 'The total number of statements
    Private m_TotalCustomers As Integer = 0 'The number of customers in the file
    Private m_TotalPOSTransactions As Integer = 0 'the number of statements in file
    Private m_TotalPOS As Integer = 0 'the number of statements in file

    Private WithEvents m_statementFiles As POSStatementFiles
    Private _reports As ReportCollection 'teport templates list

    Private m_statementStartTime As Date 'The time each statement has started processing


    Private m_CurrentStatementNumber As Integer = 0 'The number of this statement in the file
    Private m_BeginingLineOfCurrentStatement As Integer = 0 'the first line of the Statement thats going to be processed


    Private m_isResumingFile As Boolean = False 'determin if the current file is being resumed from a previous processing operation
    Private m_fileProcessStartDate As Date 'the processing date of the file which might have been dated before in an aborted process
    Private m_fileProcessEndDate As Date 'the processing date of the file which might have been dated before in an aborted process

    Private m_statementDate As Date 'the processing date of the file which might have been dated before in an aborted process
    Private m_balanceDate As Date 'the processing date of the file which might have been dated before in an aborted process

    Private _isCurrentGeneratedPDF_File_Found As Boolean = False
    Private pdf As pdfLib.QuickPDF 'pdf object

    Private _PDFMetaData_current As New pdfLib.PDFMetaData 'current working pdf properties
    Private _PDFMetaData_Passed As pdfLib.PDFMetaData 'passed pdf properties


    '' Statement Structure
    Private m_TravelCusStm As Structures.POSTCCusStm_Struct
    Private m_StatementSummary As New StatementSummary
    Private m_Customer As Structures.CusDBStm_Hdr

    'Private Const C_NumberOfRowsToGC_Collect As Integer = 2000 'Maximum Number of rows before calling the GC.collect
    'Private _currentNumberOfRows_GC_Collect As Integer = 0 'Current number of rows before calling the GC.collect
    Private _report_Max_Usage As Integer = 1 'maximum report usage before disposing
    Private _subFolder As String
    Private lockObject As New Object
    Private m_ForceAddressLeftAlign As Boolean = True

    Public Enum enumFileState
        OK
        Skipped
        Failed
    End Enum

#End Region

#Region " Properties "

    Public Property isExport As Boolean = True 'allow exporting
    Public Property OutDirRoot_Unit As String = String.Empty  'allow exporting

    Public Property FileState As enumFileState = enumFileState.OK ' the state of the file

    Public Property StatmentStatus As frmFileErrorB.enumFileErrorUserSelection
        Get
            Return _StatementStatus
        End Get
        Set(value As frmFileErrorB.enumFileErrorUserSelection)
            _StatementStatus = value
        End Set
    End Property


    Public ReadOnly Property StatementSummary() As StatementSummary
        Get
            Return m_StatementSummary
        End Get
    End Property

    Public ReadOnly Property StartDate As Date
        Get
            Return m_fileProcessStartDate
        End Get

    End Property

    Public ReadOnly Property EndDate As Date
        Get
            Return m_fileProcessEndDate
        End Get
    End Property

    Public Property Reports As ReportCollection
        Get
            Return _reports
        End Get
        Set(value As ReportCollection)
            _reports = value
        End Set
    End Property
    ''' <summary>
    ''' Return the total number of statements avaiable in the statement file.
    ''' </summary>
    ''' <value>Long</value>
    ''' <remarks>
    ''' Created : 2011-02-09 DK
    ''' </remarks>
    Public ReadOnly Property TotalStatements() As Integer
        Get
            Return m_totalNumberOfStatements
        End Get
    End Property

    ''' <summary>
    ''' Return the current number of statement being process.
    ''' </summary>
    ''' <value>Long</value>
    ''' <remarks>
    ''' Created : 2011-02-09 DK
    ''' </remarks>
    Public ReadOnly Property CurrentStatement() As Integer
        Get
            Return m_CurrentStatementNumber
        End Get
    End Property

    Public ReadOnly Property IsResumingFile() As Boolean
        Get
            Return m_isResumingFile
        End Get
    End Property

    Public ReadOnly Property TotalCustomers() As Integer
        Get
            Return m_TotalCustomers
        End Get
    End Property

    Public Property Customer() As Structures.CusDBStm_Hdr
        Get
            Return m_Customer
        End Get
        Set(value As Structures.CusDBStm_Hdr)
            m_Customer = value
        End Set
    End Property

    Public ReadOnly Property CustomerNumber() As String
        Get
            Return m_Customer.CusNum
        End Get
    End Property

#End Region

    Public Sub New(statementFiles_ As POSStatementFiles, PDFMetaData_Passed_ As pdfLib.PDFMetaData _
                   , report_Max_Usage_ As Integer, subField_ As String)

        m_statementFiles = statementFiles_

        Me._PDFMetaData_Passed = PDFMetaData_Passed_
        Me._report_Max_Usage = report_Max_Usage_
        Me._subFolder = subField_
    End Sub

    Private Property Records_CurrentItemNum As Integer

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
        End If
    End Sub

    Protected Overrides Sub Finalize()
        'CloseReport()
        Dispose(False)
    End Sub

#Region " Events "

   
    Private Sub Grid_UpdateRow(Optional ByVal status_ As String = "")
        Try
            Me.m_statementFiles.Grid_MakeNewRow(status_)
        Catch ex As Exception

        End Try
    End Sub

   
    Public Sub Grid_MakeNewRow(Optional ByVal status_ As String = "")


        Try
            Me.m_statementFiles.Grid_MakeNewRow(status_)
        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region " File Handler "


    ''' <summary>
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function readRecords() As Boolean
        Dim sArr As New Collections.ArrayList
        Dim count As Integer
        Dim lRow As DataRow
        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider
        Dim Records As New DataSet
        Dim Record As New Structures.POSSCSTCusWallet
        Dim Trx As New Structures.POSSCSTCusStm_Trx
        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)


            m_TravelCusStm.SHD = Me.m_Customer


            'If m_Customer.CusNum = "002912" Then Stop
            If Me.m_Customer.NoOfTrvWal.Trim <> "" AndAlso IsNumeric(m_Customer.NoOfTrvWal) AndAlso CInt(m_Customer.NoOfTrvWal) > 0 Then
                count = oDBProvider.getCustomerWallets(m_Customer.CusNum, 0, Records)
                If count > 0 Then
                    ReDim m_TravelCusStm.WHD(count - 1)
                    For i As Integer = 0 To Records.Tables(0).Rows.Count - 1
                        lRow = Records.Tables(0).Rows(i)

                        With Record
                            .clear()
                            .CusNum = lRow(0).ToString.Trim
                            .WalId = Convert.ToInt16(lRow(1).ToString.Trim).ToString
                            .WalNikNam = lRow(2).ToString.Trim
                            .CreLim = lRow(3).ToString.Trim
                            .CurInWal = lRow(4).ToString.Trim
                        End With
                        m_TravelCusStm.WHD(i) = Record
                    Next
                End If
                Application.DoEvents()
                Record = Nothing
                count = oDBProvider.TravelCardTransactionsExtract(m_Customer.CusNum, m_statementFiles.StatementMonth, m_statementFiles.StatementYear, Records)

                If count > 0 Then

                    ReDim m_TravelCusStm.TRX(count - 1)
                    For i As Integer = 0 To Records.Tables(0).Rows.Count - 1
                        lRow = Records.Tables(0).Rows(i)

                        With Trx
                            .clear()
                            .CusNum = lRow(0).ToString.Trim
                            .WalId = lRow(1).ToString.Trim
                            .TrxTyp = lRow(2).ToString.Trim
                            .PAN = lRow(3).ToString.Trim
                            .AccountId = lRow(4).ToString.Trim
                            .TransactioType = lRow(5).ToString.Trim
                            .TrxDteGrg = lRow(6).ToString.Trim
                            .TranAmount = lRow(7).ToString.Trim
                            .SettlementAmount = lRow(8).ToString.Trim
                            .TrxIndicator = lRow(9).ToString.Trim
                            .TxnCurrency = lRow(10).ToString.Trim
                            .SettleCurrencyCode = lRow(11).ToString.Trim
                            .TrxDatetime = Convert.ToDateTime(lRow(12))
                            .STAN = lRow(13).ToString.Trim
                            .RRN = lRow(14).ToString.Trim
                            .MCC = lRow(15).ToString.Trim
                            .AuthId = lRow(16).ToString.Trim
                            .MerchantID = lRow(17).ToString.Trim
                            .MerchantNameLOC = lRow(18).ToString.Trim
                            .AB = lRow(19).ToString.Trim
                            .LB = lRow(20).ToString.Trim
                            .FixFee = lRow(21).ToString.Trim
                            .FeePercentage = lRow(22).ToString.Trim
                            .TotalFee = lRow(23).ToString.Trim
                            .TxnISOCurrency = lRow(24).ToString.Trim
                            .SettleISOCurrencyCode = lRow(25).ToString.Trim
                            .TxnCurrencyName = lRow(26).ToString.Trim
                            .SettleCurrencyName = lRow(27).ToString.Trim
                            .PostDatetime = Convert.ToDateTime(lRow(28))
                        End With
                        m_TravelCusStm.TRX(i) = Trx
                    Next
                End If

                Record = Nothing
               
            End If
            m_StatementSummary.EndTime = Now

            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while read Travel card transactions.", "Unknown Error : " & ex.Message, ex, , , True)
            Return False
        Finally
            'Clean memory from old read statement
            sArr.Clear()
            sArr = Nothing
            oDBProvider = Nothing
            Records = Nothing
            Record = Nothing
            Trx = Nothing
        End Try

        Return True
    End Function

#End Region

#Region " Validation Function "


    ''' <summary>
    '''  Check that the Header, Footer, Name and address records are found and only once
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function checkStatementHeaderFooter() As Boolean
        Dim iCountStatementHeader As Integer = 0
        Dim iCountStatementFooter As Integer = 0
        Dim iCountStatementNameAddress As Integer = 0
        Dim sStat As String = "Statement #" & Me.CurrentStatement.ToString("##,#0")

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: checking header and footer of " & sStat), 9)

            Dim sCheckDataAvailability As String = String.Empty

            If m_TravelCusStm.SHD.StatementNumber.Trim = String.Empty Then
                sCheckDataAvailability = ", Statement Number"
            End If

            If m_TravelCusStm.SHD.StatmentDate.Trim = String.Empty Then
                sCheckDataAvailability &= ", Statement Date"
            End If

            If m_TravelCusStm.SHD.CusNum.Trim = String.Empty Then
                sCheckDataAvailability &= ", Customer Number"
            End If

            If sCheckDataAvailability <> String.Empty Then
                sCheckDataAvailability = sCheckDataAvailability.Substring(1)
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, sStat & ", This statement Header record does not have data in field(s) (" & sCheckDataAvailability & ")", ShowOn:=False, LogLevel:=3)
                Return False
            End If
            '---------------------------------
            'Name Arabic Or English

            sCheckDataAvailability = String.Empty

            If (m_TravelCusStm.SHD.CusEnNam1.Trim = String.Empty And m_TravelCusStm.SHD.CusEnNam2.Trim = String.Empty And m_TravelCusStm.SHD.CusEnNam3.Trim = String.Empty And m_TravelCusStm.SHD.CusEnNam4.Trim = String.Empty) Then
                sCheckDataAvailability = ", Customer English Name"
            End If

            If (m_TravelCusStm.SHD.CusArNam1.Trim = String.Empty And m_TravelCusStm.SHD.CusArNam2.Trim = String.Empty And m_TravelCusStm.SHD.CusArNam3.Trim = String.Empty And m_TravelCusStm.SHD.CusArNam4.Trim = String.Empty) Then
                sCheckDataAvailability = ", Customer Arabic Name"
            End If

            If sCheckDataAvailability <> String.Empty Then
                sCheckDataAvailability = sCheckDataAvailability.Substring(1)
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, sStat & ", This customer record does not have valid English or Arabic name (" & sCheckDataAvailability & "), for customer " & m_TravelCusStm.SHD.CusNum, ShowOn:=False, LogLevel:=4)
                Return False
            End If


            Return True

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An exception occurred while processing statement #" & Me.CurrentStatement.ToString("##,#0"), ShowOn:=False)
        End Try
        Return False
    End Function




#End Region

#Region " Statement File Processing "




    ''' <summary>
    ''' Ask user about what to do in case of an error
    ''' </summary>
    ''' <param name="sPassedError_">Error to be showen</param>
    ''' <remarks></remarks>
    Sub AskUser(Optional ByVal sPassedError_ As String = Nothing)
        If Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError Then
            Exit Sub
        End If
        Me._StatementStatus = Me.m_statementFiles.AskUser(m_fileProcessStartDate, m_totalNumberOfStatements, m_CurrentStatementNumber, sPassedError_)
    End Sub



    ''' <summary>
    ''' Checks if the user has canceled the operation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function isUserCanceled() As Boolean
        If Me.m_statementFiles.IsCanceledJobs Then
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, " ->Canceled by user.", LogLevel:=2)
            Grid_UpdateRow("Canceled")
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Cancel
            Return True
        Else

        End If

        Return False
    End Function

    ''' <summary>
    ''' Process a valid statement file, having customer statement of account.
    ''' Process beging by checking file integerity
    ''' then reading the statements one by one
    ''' on each statement an xml data source is made
    ''' then a .pdf report is exported
    ''' </summary>
    ''' <returns></returns>

    Public Function ProcessStatements_WalletSplit() As Boolean
        Dim iStatementNumStart As Integer = 1
        Dim i As Integer
        Dim bisConsumeStatement As Boolean = False
        Dim dConsumeStatement As Date
        Dim iStatmentFirstLineNumber As Integer = 0
        Dim bisCheckStatementHeaderFooter As Boolean = False
        Dim dCheckStatementHeaderFooter As Date
        Dim dGenerateXMLDataSet As Date
        'Me._fileNo = Me.m_statementFiles.FilesProcessSummary.OverAllFileNo

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: ProcessStatements."), 9)
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Normal
FileRestart:
            Me.m_fileProcessStartDate = Date.Now
            Me.m_statementStartTime = Me.m_fileProcessStartDate

            With m_StatementSummary
                .Total = 0

                .StartTime = Me.m_fileProcessStartDate
            End With
            gLastErrorMsg = String.Empty

            Me.FileState = enumFileState.OK
            iStatementNumStart = 1

            Me._PDFMetaData_current.copy(Me._PDFMetaData_Passed)

            'make an entry in the UI for the file
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ", "", , 4, False, False, False)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, " -> Started...", "", , 4, False, False, False)
            Application.DoEvents()
            Grid_MakeNewRow("Travel Card, CIF:" & Me.CustomerNumber & " Time: " & Me.m_fileProcessStartDate.ToString(ServicesUti.GlobalVars.C_UI_DateFormat))
            Application.DoEvents()
            'Return True
            If isUserCanceled() Then Return False

            'm_FileTotalLines = 0
            Records_CurrentItemNum = 0 'Start from the first record if the file is resuming a previous process

            If isUserCanceled() Then Return False

            Dim bRetValue As Boolean = False
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading normally started...", 4)
            Me.m_statementStartTime = Now
            bRetValue = readRecords()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading normally Ended. TimeSpan: " & UDate.getTimeDiffrence(Me.m_statementStartTime), 4)


            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("Processing travel card statement #[{0}], for Customer #[{1}]]", Me.m_CurrentStatementNumber.ToString("##,#0"), m_TravelCusStm.SHD.CusNum), LogLevel:=3, ShowOn:=False)


            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> CheckStatementHeaderFooter, Started...", 4)
            dCheckStatementHeaderFooter = Now
            bisCheckStatementHeaderFooter = checkStatementHeaderFooter()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> CheckStatementHeaderFooter, Ended. TimeSpan: " & UDate.getTimeDiffrence(dCheckStatementHeaderFooter), 4)

            Me.m_statementFiles.FilesProcessSummary.OverAllFileNo += 1
            If m_TravelCusStm.WHD.Length > 0 Then
                For i = 0 To m_TravelCusStm.WHD.Length - 1
                   
                    Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateXMLDataSet, Started...", 4)
                    dGenerateXMLDataSet = Now
                    bRetValue = GenerateTravelCardDataSetSplitWallets(i)
                    'bRetValue = GenerateTravelCardDataSetSplitWallets(Convert.ToInt16(m_TravelCusStm.WHD(i).WalId))
                    Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateXMLDataSet, Ended. TimeSpan: " & UDate.getTimeDiffrence(dGenerateXMLDataSet), 4)



                    'GenerateStatement
                    Dim isGenerateStatement As Boolean = False
                    Dim dGeneratedStatement As Date
                    Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateStatement, Started...", 4)
                    dGeneratedStatement = Now
                    isGenerateStatement = GenerateStatementSingleWallet(Convert.ToInt16(m_TravelCusStm.WHD(i).WalId), m_TravelCusStm.WHD(i).WalNikNam.Trim)
                    Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateStatement, Ended. TimeSpan: " & UDate.getTimeDiffrence(dGeneratedStatement), 4)



                Next

            End If
            Application.DoEvents()
            Me.Records_CurrentItemNum = 0 'Start each new statement from begning of the array
            Me.m_StatementRecords = Nothing
            Me.m_TravelCusStm = Nothing
            GC.Collect()
            GC.WaitForPendingFinalizers()

            If isUserCanceled() Then Return False

            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Full statement Ended. TimeSpan: " & UDate.getTimeDiffrence(m_statementStartTime), 4)
            Me.m_statementStartTime = Now
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully

        Catch ex As OutOfMemoryException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("{0} -> An Unknown exception has occurred while processing file. {1}", "", ex.Message), , ex, ShowOn:=False)
        Finally

            Me.m_fileProcessEndDate = Date.Now
            Me.m_StatementSummary.EndTime = m_fileProcessEndDate

            Me.m_StatementRecords = Nothing
            m_TravelCusStm = Nothing
            GC.Collect()
            Dim sMsg As String = ""
            Dim sOperation As String = String.Empty
            sMsg = String.Format("{0} [%Operation%], Time elapsed {1}, records processed: {2}, statements processed: {3}", "", m_StatementSummary.Duration, "", TotalStatements.ToString("##,#0"))



            If Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully Then
                sOperation = "Completed"

                m_statementFiles.FilesProcessSummary.Process = m_statementFiles.FilesProcessSummary.Process + 1
            ElseIf Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Cancel Then
                sOperation = "Canceled"
                m_statementFiles.FilesProcessSummary.Failed = m_statementFiles.FilesProcessSummary.Failed + 1
            Else
                sOperation = "Failed"
                m_statementFiles.FilesProcessSummary.Failed = m_statementFiles.FilesProcessSummary.Failed + 1
            End If

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, sMsg.Replace("[%Operation%]", sOperation), ShowOn:=False, LogLevel:=3)

        End Try
        Return Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully
    End Function



    ''' <summary>
    ''' Process a valid statement file, having customer statement of account.
    ''' Process beging by checking file integerity
    ''' then reading the statements one by one
    ''' on each statement an xml data source is made
    ''' then a .pdf report is exported
    ''' </summary>
    ''' <returns></returns>

    Public Function ProcessStatements_AllWallets() As Boolean
        Dim iStatementNumStart As Integer = 1

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: ProcessStatements."), 9)
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Normal
FileRestart:
            Me.m_fileProcessStartDate = Date.Now
            Me.m_statementStartTime = Me.m_fileProcessStartDate

            With m_StatementSummary
                .Total = 0

                .StartTime = Me.m_fileProcessStartDate
            End With
            gLastErrorMsg = String.Empty

            Me.FileState = enumFileState.OK
            iStatementNumStart = 1

            Me._PDFMetaData_current.copy(Me._PDFMetaData_Passed)

            'make an entry in the UI for the file
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ", "", , 4, False, False, False)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, " -> Started...", "", , 4, False, False, False)
            Application.DoEvents()

            Grid_MakeNewRow("Travel Card, CIF:" & Me.CustomerNumber & " Time: " & Me.m_fileProcessStartDate.ToString(ServicesUti.GlobalVars.C_UI_DateFormat))
            Application.DoEvents()
            'Return True
            If isUserCanceled() Then Return False

            'm_FileTotalLines = 0
            Records_CurrentItemNum = 0 'Start from the first record if the file is resuming a previous process

            If isUserCanceled() Then Return False

            'Read the first Statement regardless of the file is begining from start or resuming a previous process, so we can read the marketting records
            Dim bRetValue As Boolean = False
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading normally started...", 4)
            Me.m_statementStartTime = Now
            bRetValue = readRecords()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading normally Ended. TimeSpan: " & UDate.getTimeDiffrence(Me.m_statementStartTime), 4)


            Dim currentStatmentStart As DateTime = Date.Now
            Me.m_statementFiles.FilesProcessSummary.OverAllFileNo += 1


            If isUserCanceled() Then Return False


            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("Processing statement #[{0}], for Customer #[{1}]", Me.m_CurrentStatementNumber.ToString("##,#0"), m_TravelCusStm.SHD.CusNum), LogLevel:=3, ShowOn:=False)

            Dim bisCheckStatementHeaderFooter As Boolean = False
            Dim dCheckStatementHeaderFooter As Date
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> CheckStatementHeaderFooter, Started...", 4)
            dCheckStatementHeaderFooter = Now
            bisCheckStatementHeaderFooter = checkStatementHeaderFooter()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> CheckStatementHeaderFooter, Ended. TimeSpan: " & UDate.getTimeDiffrence(dCheckStatementHeaderFooter), 4)

            'Assign data to XML Schema
            If isUserCanceled() Then Return False

            Dim dGenerateXMLDataSet As Date
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateXMLDataSet, Started...", 4)
            dGenerateXMLDataSet = Now
            bRetValue = GenerateTravelCardDataSet()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateXMLDataSet, Ended. TimeSpan: " & UDate.getTimeDiffrence(dGenerateXMLDataSet), 4)


            If isUserCanceled() Then Return False

            'GenerateStatement
            Dim isGenerateStatement As Boolean = False
            Dim dGeneratedStatement As Date
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateStatement, Started...", 4)
            dGeneratedStatement = Now
            isGenerateStatement = GenerateStatement()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> GenerateStatement, Ended. TimeSpan: " & UDate.getTimeDiffrence(dGeneratedStatement), 4)

            If isGenerateStatement Then

                With Me.m_statementFiles.FilesProcessSummary

                End With

            Else

            End If
            Application.DoEvents()
            Me.Records_CurrentItemNum = 0 'Start each new statement from begning of the array
            Me.m_StatementRecords = Nothing
            Me.m_TravelCusStm = Nothing
            GC.Collect()
            GC.WaitForPendingFinalizers()

            If isUserCanceled() Then Return False

            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Full statement Ended. TimeSpan: " & UDate.getTimeDiffrence(m_statementStartTime), 4)

            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully

        Catch ex As OutOfMemoryException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("{0} -> An Unknown exception has occurred while processing file. {1}", "", ex.Message), , ex, ShowOn:=False)
        Finally

            Me.m_fileProcessEndDate = Date.Now
            Me.m_StatementSummary.EndTime = m_fileProcessEndDate

            Me.m_StatementRecords = Nothing
            m_TravelCusStm = Nothing
            GC.Collect()
            Dim sMsg As String = ""
            Dim sOperation As String = String.Empty
            sMsg = String.Format("{0} [%Operation%], Time elapsed {1}, records processed: {2}, statements processed: {3}", "", m_StatementSummary.Duration, "", TotalStatements.ToString("##,#0"))



            If Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully Then
                sOperation = "Completed"

                m_statementFiles.FilesProcessSummary.Process = m_statementFiles.FilesProcessSummary.Process + 1
            ElseIf Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Cancel Then
                sOperation = "Canceled"
                m_statementFiles.FilesProcessSummary.Failed = m_statementFiles.FilesProcessSummary.Failed + 1
            Else
                sOperation = "Failed"
                m_statementFiles.FilesProcessSummary.Failed = m_statementFiles.FilesProcessSummary.Failed + 1
            End If

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, sMsg.Replace("[%Operation%]", sOperation), ShowOn:=False, LogLevel:=3)

        End Try
        Return Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully
    End Function

    ''' <summary>
    ''' This is called when the user selects to skip the file, and leave it in the working folder
    ''' </summary>
    ''' <remarks></remarks>
    Sub processSkipped()
        Grid_UpdateRow("Skipped")
    End Sub

    ''' <summary>
    ''' processes crystal report errors
    ''' </summary>
    ''' <remarks></remarks>
    Sub processCrystalReportsError()
        Grid_UpdateRow("No Resources")
        Logger.LoggerClass.Singleton.LogError(0, "statement -> Error has been raised by Crystal Reports, System must exit and restart again.")

        Dim sMsg As String = "Computer Resources has runned out, the application must quit. " & vbCrLf & "Please close it, and run it again and select [Resume] to continue on the current file."

        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, sMsg, , Nothing, GenerateAIDS:=False)
        ServicesUti.Services.Singleton.AppInstance.ModalMessageBox(sMsg, MessageBoxButtons.OK, , MessageBoxIcon.Error, "MDOX")

    End Sub



#End Region

#Region " Generate Statement "

    ''' <summary>
    ''' Pouplate the data read from the file in the data structure to XML for generating PDF file using Crystal reports.
    ''' </summary>
    ''' <returns>Return TRUE if function executed successfully, and FALSE if failed to process.</returns>
    ''' <remarks>
    ''' </remarks>
    Public Function GenerateTravelCardDataSetSplitWallets(WalletNumber As Integer) As Boolean

        Dim StmHdr() As DataSetPOS.HRDRow
        Dim StmWlc() As DataSetPOS.WalletCardsRow
        Dim CurrInWallet() As String
        'Dim i As Integer
        Dim CurrenctCcyTrx() As Structures.POSSCSTCusStm_Trx
        Dim tmpStmNum As String = String.Empty
        Dim WalletBalance As String
        Dim tmptrxAmunt As String
        Dim startP As DateTime
        Dim StmDate As DateTime
        Dim lBalance As Structures.POSSCSTBalance
        Dim Flgs As String = String.Empty
        Dim lCount As Integer = 0
        Dim AddLine1, AddLine2, AddLine3, AddLine4 As String
        Dim NameLine1, NameLine2 As String
        Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

        Try
            startP = gLastMonthEnd
            StmDate = gCurrentMonthEnd
            Me.m_DataTravelSet.Clear()
            m_ForceAddressLeftAlign = True
            ReDim CurrenctCcyTrx(0)
            With m_TravelCusStm
                Dim sDate As String
                sDate = USFormatDate(StmDate, "yyyy-MM-dd")
                'sDate = ServicesUti.Services.Singleton.AppInstance.IceFormatDate(.ACC.StmDte, "dd MMM yyyy") '.SHD.StatmentDate.Trim '
                ReDim StmWlc(0)
                ReDim StmHdr(0)
                Flgs = "0"
                Flgs = Flgs & USFormatDate(Now, "yyMM")
                Flgs = Flgs & Format(CInt(.SHD.StatementNumber), "0000")


                AddLine1 = .SHD.CusEnAddLin1.Trim
                AddLine2 = .SHD.CusEnAddLin2.Trim
                AddLine3 = .SHD.CusEnAddLin3.Trim
                AddLine4 = .SHD.CusEnAddLin4.Trim
                NameLine1 = ""
                NameLine2 = ""

                If (.SHD.CusArNam1.Trim <> "") Then
                    NameLine1 = .SHD.CusArNam1.Trim & " "
                End If
                If (.SHD.CusArNam2.Trim <> "") Then
                    NameLine1 = NameLine1 & .SHD.CusArNam2.Trim & " "
                End If
                If (.SHD.CusArNam3.Trim <> "") Then
                    NameLine1 = NameLine1 & .SHD.CusArNam3.Trim & " "
                End If
                If (.SHD.CusArNam4.Trim <> "") Then
                    NameLine1 = NameLine1 & .SHD.CusArNam4.Trim
                End If

                '------------

                If (.SHD.CusEnNam1.Trim <> "") Then
                    NameLine2 = .SHD.CusEnNam1.Trim & " "
                End If
                If (.SHD.CusEnNam2.Trim <> "") Then
                    NameLine2 = NameLine2 & .SHD.CusEnNam2.Trim & " "
                End If
                If (.SHD.CusEnNam3.Trim <> "") Then
                    NameLine2 = NameLine2 & .SHD.CusEnNam3.Trim & " "
                End If
                If (.SHD.CusEnNam4.Trim <> "") Then
                    NameLine2 = NameLine2 & .SHD.CusEnNam4.Trim
                End If


                If (.SHD.CusArAddLin1.Trim = "" And .SHD.CusArAddLin2.Trim = "" And .SHD.CusArAddLin3 = "" And .SHD.CusArAddLin4.Trim = "") Then
                    m_TravelCusStm.SHD.CusLng = "E"
                    If (NameLine1.Trim <> "") Then
                        AddLine1 = NameLine1
                        m_TravelCusStm.SHD.CusLng = "A"
                    End If


                End If

                If (.SHD.CusEnAddLin1.Trim = "" And .SHD.CusEnAddLin2.Trim = "" And .SHD.CusEnAddLin3 = "" And .SHD.CusEnAddLin4.Trim = "") Then
                    m_TravelCusStm.SHD.CusLng = "E"
                    AddLine1 = NameLine2
                End If

                If m_TravelCusStm.SHD.CusLng = "A" And (.SHD.CusArAddLin1.Trim <> "" And .SHD.CusArAddLin2.Trim <> "" And .SHD.CusArAddLin3 <> "" And .SHD.CusArAddLin4.Trim <> "") Then
                    AddLine1 = .SHD.CusArAddLin1.Trim
                    AddLine2 = .SHD.CusArAddLin2.Trim
                    AddLine3 = .SHD.CusArAddLin3.Trim
                    AddLine4 = .SHD.CusArAddLin4.Trim
                    m_ForceAddressLeftAlign = False
                End If


                StmHdr(0) = m_DataTravelSet.HRD.AddHRDRow(0, .SHD.CusNum.Trim, sDate, AddLine1, AddLine2, AddLine3, AddLine4, "", Flgs)


                WalletBalance = FormatBalance1(.WHD(WalletNumber).CreLim.Trim, 2, "USD")
                StmWlc(0) = m_DataTravelSet.WalletCards.AddWalletCardsRow(Convert.ToInt16(.WHD(WalletNumber).WalId), StmHdr(0), .WHD(WalletNumber).WalNikNam.Trim, WalletBalance & " USD")

                '----------------------------------------------
                '' Update Header Record
                If .SHD.StatementNumber.Length > 0 And IsNumeric(.SHD.StatementNumber) Then tmpStmNum = CInt(.SHD.StatementNumber).ToString

                If .WHD(WalletNumber).CurInWal.Trim <> "" Then
                    .WHD(WalletNumber).CurInWal = .WHD(WalletNumber).CurInWal.Trim
                    If .WHD(WalletNumber).CurInWal.Trim.EndsWith(",") Then .WHD(WalletNumber).CurInWal = .WHD(WalletNumber).CurInWal.Substring(0, .WHD(WalletNumber).CurInWal.Length - 1)
                    CurrInWallet = .WHD(WalletNumber).CurInWal.Split(CChar(","))
                    For j As Integer = 0 To CurrInWallet.Length - 1
                        If CurrInWallet(j).Trim.Trim <> "" And CurrInWallet(j).Trim <> "999" And CurrInWallet(j).TrimStart.Trim <> "000" Then
                            lBalance = New Structures.POSSCSTBalance
                            lBalance.clear()
                            lCount = 0

                            If GetCcyTransactions(.TRX, CurrenctCcyTrx, CurrInWallet(j).Trim, .WHD(WalletNumber).WalId) Then

                                If Not CurrenctCcyTrx Is Nothing AndAlso CurrenctCcyTrx.Length > 0 Then
                                    For k As Integer = 0 To CurrenctCcyTrx.Length - 1
                                        Dim exp As Integer
                                        exp = 2

                                        If CurrenctCcyTrx(k).TrxIndicator = "CR" Then
                                            tmptrxAmunt = FormatBalance1(CurrenctCcyTrx(k).SettlementAmount, 2, CurrenctCcyTrx(k).SettleCurrencyCode) & " Cr"
                                        Else
                                            tmptrxAmunt = FormatBalance1(CurrenctCcyTrx(k).SettlementAmount, 2, CurrenctCcyTrx(k).SettleCurrencyCode) & " Dr"
                                        End If

                                        m_DataTravelSet.TRX.AddTRXRow(k + 1, StmWlc(0), DecodeCcyCde(CurrenctCcyTrx(k).SettleCurrencyCode) & " Wallet Currency", USFormatDate(CurrenctCcyTrx(k).TrxDatetime, "dd-MMM-yy"), USFormatDate(CurrenctCcyTrx(k).PostDatetime, "dd-MMM-yy"), FormatNarrations(CurrenctCcyTrx(k)), tmptrxAmunt, FormatBalance(CurrenctCcyTrx(k).LB, 2, CurrenctCcyTrx(k).SettleCurrencyCode))
                                        lCount = lCount + 1
                                    Next

                                End If
                            Else
                                If lCount = 0 Then
                                    m_DataTravelSet.TRX.AddTRXRow(0, StmWlc(0), DecodeCcyCde(CurrInWallet(j).Trim) & " Wallet Currency", "No transactions available to show", "", "", "", "")
                                End If
                            End If
                        End If


                    Next
                End If

            End With

            Return True

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error in GenerateTravelCardDataSet.Because an exception happened.", ex.InnerException.ToString)
        Finally

            StmHdr = Nothing
        End Try

        Return False

    End Function

    Public Function GenerateTravelCardDataSet() As Boolean

        Dim StmHdr() As DataSetPOS.HRDRow
        Dim StmWlc() As DataSetPOS.WalletCardsRow
        Dim CurrInWallet() As String
        Dim i As Integer
        Dim CurrenctCcyTrx() As Structures.POSSCSTCusStm_Trx
        Dim tmpStmNum As String = String.Empty
        Dim tmpOpnBal, tmpOpnBalDsc As String
        Dim tmptrxAmunt As String
        Dim startP As DateTime
        Dim StmDate As DateTime
        Dim lBalance As Structures.POSSCSTBalance
        Dim Flgs As String = String.Empty
        Dim lCount As Integer = 0
        Dim AddLine1, AddLine2, AddLine3, AddLine4 As String
        Dim NameLine1, NameLine2 As String
        Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

        Try
            startP = gLastMonthEnd
            StmDate = gCurrentMonthEnd
            Me.m_DataTravelSet.Clear()
            m_ForceAddressLeftAlign = True
            ReDim CurrenctCcyTrx(0)
            With m_TravelCusStm
                Dim sDate As String
                sDate = USFormatDate(StmDate, "yyyy-MM-dd")
                'sDate = ServicesUti.Services.Singleton.AppInstance.IceFormatDate(.ACC.StmDte, "dd MMM yyyy") '.SHD.StatmentDate.Trim '
                ReDim StmWlc(m_TravelCusStm.WHD.Length - 1)
                ReDim StmHdr(m_TravelCusStm.WHD.Length - 1)
                Flgs = "0"
                Flgs = Flgs & USFormatDate(Now, "yyMM")
                Flgs = Flgs & Format(CInt(.SHD.StatementNumber), "0000")


                For i = 0 To m_TravelCusStm.WHD.Length - 1
                    AddLine1 = .SHD.CusEnAddLin1.Trim
                    AddLine2 = .SHD.CusEnAddLin2.Trim
                    AddLine3 = .SHD.CusEnAddLin3.Trim
                    AddLine4 = .SHD.CusEnAddLin4.Trim
                    NameLine1 = ""
                    NameLine2 = ""

                    If (.SHD.CusArNam1.Trim <> "") Then
                        NameLine1 = .SHD.CusArNam1.Trim & " "
                    End If
                    If (.SHD.CusArNam2.Trim <> "") Then
                        NameLine1 = NameLine1 & .SHD.CusArNam2.Trim & " "
                    End If
                    If (.SHD.CusArNam3.Trim <> "") Then
                        NameLine1 = NameLine1 & .SHD.CusArNam3.Trim & " "
                    End If
                    If (.SHD.CusArNam4.Trim <> "") Then
                        NameLine1 = NameLine1 & .SHD.CusArNam4.Trim
                    End If

                    '------------

                    If (.SHD.CusEnNam1.Trim <> "") Then
                        NameLine2 = .SHD.CusEnNam1.Trim & " "
                    End If
                    If (.SHD.CusEnNam2.Trim <> "") Then
                        NameLine2 = NameLine2 & .SHD.CusEnNam2.Trim & " "
                    End If
                    If (.SHD.CusEnNam3.Trim <> "") Then
                        NameLine2 = NameLine2 & .SHD.CusEnNam3.Trim & " "
                    End If
                    If (.SHD.CusEnNam4.Trim <> "") Then
                        NameLine2 = NameLine2 & .SHD.CusEnNam4.Trim
                    End If


                    If (.SHD.CusArAddLin1.Trim = "" And .SHD.CusArAddLin2.Trim = "" And .SHD.CusArAddLin3 = "" And .SHD.CusArAddLin4.Trim = "") Then
                        m_TravelCusStm.SHD.CusLng = "E"
                        If (NameLine1.Trim <> "") Then
                            AddLine1 = NameLine1
                            m_TravelCusStm.SHD.CusLng = "A"
                        End If


                    End If

                    If (.SHD.CusEnAddLin1.Trim = "" And .SHD.CusEnAddLin2.Trim = "" And .SHD.CusEnAddLin3 = "" And .SHD.CusEnAddLin4.Trim = "") Then
                        m_TravelCusStm.SHD.CusLng = "E"
                        AddLine1 = NameLine2
                    End If

                    If m_TravelCusStm.SHD.CusLng = "A" And (.SHD.CusArAddLin1.Trim <> "" And .SHD.CusArAddLin2.Trim <> "" And .SHD.CusArAddLin3 <> "" And .SHD.CusArAddLin4.Trim <> "") Then
                        AddLine1 = .SHD.CusArAddLin1.Trim
                        AddLine2 = .SHD.CusArAddLin2.Trim
                        AddLine3 = .SHD.CusArAddLin3.Trim
                        AddLine4 = .SHD.CusArAddLin4.Trim
                        m_ForceAddressLeftAlign = False
                    End If


                    StmHdr(i) = m_DataTravelSet.HRD.AddHRDRow(i, .SHD.CusNum.Trim, sDate, AddLine1, AddLine2, AddLine3, AddLine4, "", Flgs)

                    'If m_TravelCusStm.SHD.CusLng = "A" Then
                    '    StmHdr(i) = m_DataTravelSet.HRD.AddHRDRow(i, .SHD.CusNum.Trim, sDate, .SHD.CusArAddLin1, .SHD.CusArAddLin2, .SHD.CusArAddLin3, .SHD.CusArAddLin4, "", Flgs)
                    'Else
                    '    StmHdr(i) = m_DataTravelSet.HRD.AddHRDRow(i, .SHD.CusNum.Trim, sDate, .SHD.CusEnAddLin1, .SHD.CusEnAddLin2, .SHD.CusEnAddLin3, .SHD.CusEnAddLin4, "", Flgs)
                    'End If

                Next
                'StmHdr = m_DataSet.HRD.AddHRDRow(1, .SHD.CusNum.Trim, sDate, .SHD.CusEnNam1 & " " & .SHD.CusEnNam2 & " " & .SHD.CusEnNam3 & " " & .SHD.CusEnNam4, .SHD.CusEnAddLin1, .SHD.CusEnAddLin2, .SHD.CusEnAddLin3, .SHD.CusEnAddLin4, .SHD.StatementNumber)
                For i = 0 To m_TravelCusStm.WHD.Length - 1
                    tmpOpnBal = FormatBalance1(.WHD(i).CreLim.Trim, 2, "USD")
                    StmWlc(i) = m_DataTravelSet.WalletCards.AddWalletCardsRow(Convert.ToInt16(.WHD(i).WalId), StmHdr(i), .WHD(i).WalNikNam.Trim, tmpOpnBal & " USD")

                Next
                '----------------------------------------------
                '' Update Header Record
                If .SHD.StatementNumber.Length > 0 And IsNumeric(.SHD.StatementNumber) Then tmpStmNum = CInt(.SHD.StatementNumber).ToString


                For i = 0 To m_TravelCusStm.WHD.Length - 1
                    If .WHD(i).CurInWal.Trim <> "" Then
                        .WHD(i).CurInWal = .WHD(i).CurInWal.Trim
                        If .WHD(i).CurInWal.Trim.EndsWith(",") Then .WHD(i).CurInWal = .WHD(i).CurInWal.Substring(0, .WHD(i).CurInWal.Length - 1)
                        CurrInWallet = .WHD(i).CurInWal.Split(CChar(","))
                        For j As Integer = 0 To CurrInWallet.Length - 1
                            Debug.Print(DecodeCcyCde(CurrInWallet(j).Trim) & " Wallet Currency" & "!!" & CurrInWallet(j).Trim)
                        Next
                        For j As Integer = 0 To CurrInWallet.Length - 1
                            If CurrInWallet(j).Trim.Trim <> "" And CurrInWallet(j).Trim <> "999" And CurrInWallet(j).TrimStart.Trim <> "000" Then
                                lBalance = New Structures.POSSCSTBalance
                                lBalance.clear()
                                lCount = 0
                                If INIParameters.Singlton.Glob.ShowBalances Then
                                    If GetCcyBalance(.Balance, lBalance, CurrInWallet(j).Trim, .WHD(i).WalId) Then
                                        'balance 
                                        ' Balance Brought Forward
                                        tmpOpnBal = FormatBalance1(lBalance.LedgerBalance, 2, CurrInWallet(j).Trim) & " Cr" 'FormatAmount(lBalance.LedgerBalance)
                                        tmpOpnBalDsc = "Balance at end of last statement"
                                        m_DataTravelSet.TRX.AddTRXRow(0, StmWlc(i), DecodeCcyCde(CurrInWallet(j).Trim) & " Wallet Currency", USFormatDate(startP, "dd-MMM-yy"), "", tmpOpnBalDsc, "", tmpOpnBal)
                                        'get currency trx
                                    Else
                                        tmpOpnBal = FormatBalance1("0", 2, CurrInWallet(j).Trim) & " Cr" 'FormatAmount(lBalance.LedgerBalance)
                                        tmpOpnBalDsc = "Balance at end of last statement"
                                        m_DataTravelSet.TRX.AddTRXRow(0, StmWlc(i), DecodeCcyCde(CurrInWallet(j).Trim) & " Wallet Currency", USFormatDate(startP, "dd-MMM-yy"), "", tmpOpnBalDsc, "", tmpOpnBal)
                                    End If
                                End If

                                If GetCcyTransactions(.TRX, CurrenctCcyTrx, CurrInWallet(j).Trim, .WHD(i).WalId) Then

                                    If Not CurrenctCcyTrx Is Nothing AndAlso CurrenctCcyTrx.Length > 0 Then
                                        For k As Integer = 0 To CurrenctCcyTrx.Length - 1
                                            Dim exp As Integer
                                            exp = 2

                                            If CurrenctCcyTrx(k).TrxIndicator = "CR" Then
                                                tmptrxAmunt = FormatBalance1(CurrenctCcyTrx(k).SettlementAmount, 2, CurrenctCcyTrx(k).SettleCurrencyCode) & " Cr"
                                            Else
                                                tmptrxAmunt = FormatBalance1(CurrenctCcyTrx(k).SettlementAmount, 2, CurrenctCcyTrx(k).SettleCurrencyCode) & " Dr"
                                            End If

                                            m_DataTravelSet.TRX.AddTRXRow(k + 1, StmWlc(i), DecodeCcyCde(CurrenctCcyTrx(k).SettleCurrencyCode) & " Wallet Currency", USFormatDate(CurrenctCcyTrx(k).TrxDatetime, "dd-MMM-yy"), USFormatDate(CurrenctCcyTrx(k).PostDatetime, "dd-MMM-yy"), FormatNarrations(CurrenctCcyTrx(k)), tmptrxAmunt, FormatBalance(CurrenctCcyTrx(k).LB, 2, CurrenctCcyTrx(k).SettleCurrencyCode))
                                            lCount = lCount + 1
                                        Next


                                    End If
                                Else
                                    If lCount = 0 Then
                                        m_DataTravelSet.TRX.AddTRXRow(0, StmWlc(i), DecodeCcyCde(CurrInWallet(j).Trim) & " Wallet Currency", "No transactions available to show", "", "", "", "")
                                    End If
                                End If
                            End If


                        Next
                    End If

                Next

            End With

            Return True

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error in GenerateTravelCardDataSet.Because an exception happened.", ex.InnerException.ToString)
        Finally

            StmHdr = Nothing
        End Try

        Return False

    End Function

    Private Function GetCcyBalance(ByRef AllCcyBalances() As Structures.POSSCSTBalance, ByRef CcyBalance As Structures.POSSCSTBalance, CcyCde As String, WalId As String) As Boolean
        Dim i As Integer

        Dim idx As Integer = 0
        If AllCcyBalances Is Nothing Then Return False
        For i = 0 To AllCcyBalances.Length - 1
            If AllCcyBalances(i).Ccy = CcyCde And AllCcyBalances(i).WalId = WalId Then
                CcyBalance = AllCcyBalances(i)
                Return True
            End If
        Next
        Return False
    End Function

    Private Function GetCcyTransactions(ByRef AllTrx() As Structures.POSSCSTCusStm_Trx, ByRef CcyTrx() As Structures.POSSCSTCusStm_Trx, CcyCde As String, WalId As String) As Boolean
        Dim i As Integer
        Dim count As Integer
        Dim idx As Integer = 0
        If AllTrx Is Nothing Then Return False
        For i = 0 To AllTrx.Length - 1
            If AllTrx(i).SettleISOCurrencyCode = CcyCde And AllTrx(i).WalId = WalId Then
                count += 1
            End If
        Next
        If count = 0 Then
            ReDim CcyTrx(0)
            Return False
        Else
            ReDim CcyTrx(count - 1)
            For i = 0 To AllTrx.Length - 1
                If AllTrx(i).SettleISOCurrencyCode = CcyCde And AllTrx(i).WalId = WalId Then
                    CcyTrx(idx) = AllTrx(i)
                    idx += 1
                End If
            Next
            Return True
        End If
    End Function

    Private Function FormatNarrations(ByVal TrxDsc As Structures.POSSCSTCusStm_Trx) As String
        Dim lsTrxDsc As String = String.Empty
        Dim tmpOpnBal As String
        Try
            With TrxDsc
                lsTrxDsc = .TransactioType.Trim
                If .TransactioType.ToUpper = "LOAD" Or .TransactioType.ToUpper = "UNLOAD" Then Return lsTrxDsc

                If .MerchantNameLOC.Trim.Length > 0 Then lsTrxDsc &= vbCrLf & .MerchantNameLOC.Trim
                If .PAN.Trim.Length > 0 Then lsTrxDsc &= vbCrLf & "Card:" & .PAN.Trim
                If .TxnCurrency <> .SettleCurrencyCode Then
                    If .SettlementAmount.Trim.Length > 0 Then lsTrxDsc &= vbCrLf & FormatBalance1(.TranAmount.Trim, 2, .TxnCurrency) & " " & .TxnCurrency
                End If

                If .FixFee.Trim.Length > 0 Then
                    tmpOpnBal = FormatAmount(.TotalFee.Trim)
                    If tmpOpnBal <> "0" Then lsTrxDsc &= vbCrLf & "Fees:" & FormatBalance1(.FixFee.Trim, 2, .SettleCurrencyCode) & " " & .SettleCurrencyCode
                End If

            End With

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while Format Narrations.", String.Empty, ex, LogLevel:=1)
        End Try

        Return lsTrxDsc
    End Function



    ''' <summary>
    ''' Populate the customer statement with the provided XML Dataset.
    ''' </summary>
    ''' <returns>Return TRUE if function executed successfully, FALSE if failed to process.</returns>
    ''' <remarks>
    ''' </remarks>
    Public Overridable Function GenerateStatementSingleWallet(WalletNumber As Integer, WalletName As String) As Boolean
        Dim isExported As Boolean = False
        'Dim iLastPage As Integer = 0
        Dim oCrystalRep As CCrystalReport = Nothing
        Dim rptName As String = String.Empty
        Dim sRank As String = String.Empty
        Dim StmLng As String = "EN"
        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            sRank = "0"
            If m_TravelCusStm.SHD.CusLng.Trim = "" Then m_TravelCusStm.SHD.CusLng = "A"
            If m_TravelCusStm.SHD.CusLng = "A" Then
                StmLng = "AR"
            Else
                StmLng = "EN"
            End If
            'StmLng = "AR"
            ' "VDXF0110.STMT.GENSTM.{0}.{1}.{2}.{3}.rpt" '0:Lang, 1:Rank, 2:Version, 3:WICN/NICN
            rptName = String.Format(POSStatementFiles.C_TCSTReportTemplateName, StmLng, sRank, "01")

            oCrystalRep = Reports.getReport(rptName)

            If oCrystalRep Is Nothing Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "oCrystalRep is null.")
            End If
            If m_DataTravelSet Is Nothing Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "m_DataTravelSet is null.")
            End If
            With oCrystalRep
                .unLoadReport()
                .SetDataSource(m_DataTravelSet)

                m_StatementSummary.Total = m_StatementSummary.Total + 1

            End With
            If Not GenerateOutputPath(WalletNumber, WalletName, "0") Then
                Return False
            End If
            isExported = ExportToPDF(oCrystalRep, m_pdfFilePathFull)
            SetPdfProperties(m_pdfFilePathFull)


            If Not isExported Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Exporting report to file [{0}] did not succeed.", m_pdfFilePathFull), , )

            End If

        Catch ex As CrystalDecisions.CrystalReports.Engine.InternalException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False
        Catch ex As OutOfMemoryException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False
        Catch ex As System.Runtime.InteropServices.COMException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False

        Catch ex As CrystalDecisions.CrystalReports.Engine.LoadSaveReportException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False

        Catch ex As IO.DirectoryNotFoundException
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Could not generate pdf File Path [Out.Dir.Root]\[Mdx.Run.Unit]\[AccNum_FirstPart]\[AccNum]\[Stm.Sub.Fld] [{0}], please don't write any leading or trailing '\'.", m_pdfFilePathDirectory), , DirectCast(ex, Exception), 1, True, True)
            isExported = False

        Catch ex As Exception
            isExported = False
            If ex.Message.Contains("Report Application Server failed") Then
                Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            Else
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0:##,#0} An Unexpected error has occurred, at GenerateStatement.", Me.CustomerNumber), , ex)
            End If
        Finally

        End Try

        Return isExported
    End Function


    ''' <summary>
    ''' The function create the ouput path for the statement to be exported, if the required path doesn't exists it will be created.
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Private Function GenerateOutputPath(WalletNumber As Integer, WalletName As String, Optional FileType As String = "0") As Boolean
        Dim tmpAccNum_FirstPart As String = String.Empty 'first 3 chars
        Dim tmpAccNum_MiddlePart As String = String.Empty ' the 6 digits customer number
        Dim tmpAccNum As String
        Dim Flgs As String = String.Empty
        Dim TmpstmDate As String

        'SyncLock lockObject
        Try

            'This is for travel card:
            'CCCCCC.VDXP0100.TCST.FFFFFFFFFF.YYYYMMDD.Statement.PDF

            'This is for shopping card:
            'CCCCCC.VDXP0100.SCST.FFFFFFFFFF.YYYYMMDD.Statement.PDF
            TmpstmDate = USFormatDate(gCurrentMonthEnd, "yyyyMMdd")
            m_pdfFilePathDirectory = String.Empty
            m_pdfFilePathFull = String.Empty
            tmpAccNum = m_TravelCusStm.SHD.CusNum.Trim
            tmpAccNum_FirstPart = tmpAccNum.Substring(0, 3)
            Flgs = "TFFFFFFFFF"
            If FileType = "0" Then
                m_OutputFileName = String.Format("{0}.VDXP0100.TCST.{1}.{2}.{3}.{4}.Statement.PDF", tmpAccNum, Flgs, TmpstmDate, WalletNumber, WalletName)
            Else
                m_OutputFileName = String.Format("{0}.VDXP0100.SCST.{1}.{2}.{3}.4}.Statement.PDF", tmpAccNum, Flgs, TmpstmDate, WalletNumber, WalletName)
            End If


            'm_pdfFilePath = String.Format("{0}\{1}\Statements\{2}\", Me.OutDirRoot_Unit, tmpAccNum(1), m_CusStm.SHD.StmDte.Substring(0, 4))
            m_pdfFilePathDirectory = String.Format("{0}\{1}\{2}\{3}\", Me.OutDirRoot_Unit, tmpAccNum_FirstPart, tmpAccNum, Me._subFolder) ', m_CusStm.SHD.StmDte.Substring(0, 4)

            If Directory.Exists(m_pdfFilePathDirectory) = False Then
                Directory.CreateDirectory(m_pdfFilePathDirectory)
            End If
            m_pdfFilePathFull = m_pdfFilePathDirectory & m_OutputFileName



        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Could not generate pdf File Path [Out.Dir.Root]\[Mdx.Run.Unit]\[AccNum_FirstPart]\[AccNum]\[Stm.Sub.Fld] [{0}], please don't write any leading or trailing '\'.", m_pdfFilePathDirectory), , ex, 1, True, True)
            Return False
        End Try

        Return True
        'End SyncLock
    End Function


    ''' <summary>
    ''' Populate the customer statement with the provided XML Dataset.
    ''' </summary>
    ''' <returns>Return TRUE if function executed successfully, FALSE if failed to process.</returns>
    ''' <remarks>
    ''' </remarks>
    Public Overridable Function GenerateStatement() As Boolean
        Dim isExported As Boolean = False
        Dim iLastPage As Integer = 0
        Dim oCrystalRep As CCrystalReport = Nothing
        Dim rptName As String = String.Empty
        Dim sRank As String = String.Empty
        Dim StmLng As String = "EN"
        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            sRank = "0"
            If m_TravelCusStm.SHD.CusLng.Trim = "" Then m_TravelCusStm.SHD.CusLng = "A"
            If m_TravelCusStm.SHD.CusLng = "A" Then
                StmLng = "AR"
            Else
                StmLng = "EN"
            End If
            'StmLng = "AR"
            ' "VDXF0110.STMT.GENSTM.{0}.{1}.{2}.{3}.rpt" '0:Lang, 1:Rank, 2:Version, 3:WICN/NICN
            rptName = String.Format(POSStatementFiles.C_TCSTReportTemplateName, StmLng, sRank, "01")

            oCrystalRep = Reports.getReport(rptName)

            If oCrystalRep Is Nothing Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "oCrystalRep is null.")
            End If
            If m_DataTravelSet Is Nothing Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "m_DataTravelSet is null.")
            End If
            With oCrystalRep
                .unLoadReport()
                .SetDataSource(m_DataTravelSet)

                m_StatementSummary.Total = m_StatementSummary.Total + 1

            End With
            If Not GenerateOutputPath() Then
                Return False
            End If
            isExported = ExportToPDF(oCrystalRep, m_pdfFilePathFull)
            SetPdfProperties(m_pdfFilePathFull)


            If Not isExported Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Exporting report to file [{0}] did not succeed.", m_pdfFilePathFull), , )

            End If

        Catch ex As CrystalDecisions.CrystalReports.Engine.InternalException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False
        Catch ex As OutOfMemoryException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False
        Catch ex As System.Runtime.InteropServices.COMException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False

        Catch ex As CrystalDecisions.CrystalReports.Engine.LoadSaveReportException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            isExported = False

        Catch ex As IO.DirectoryNotFoundException
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Could not generate pdf File Path [Out.Dir.Root]\[Mdx.Run.Unit]\[AccNum_FirstPart]\[AccNum]\[Stm.Sub.Fld] [{0}], please don't write any leading or trailing '\'.", m_pdfFilePathDirectory), , DirectCast(ex, Exception), 1, True, True)
            isExported = False

        Catch ex As Exception
            isExported = False
            If ex.Message.Contains("Report Application Server failed") Then
                Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception))
            Else
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0:##,#0} An Unexpected error has occurred, at GenerateStatement.", Me.CustomerNumber), , ex)
            End If
        Finally

        End Try

        Return isExported
    End Function


    ''' <summary>
    ''' The function create the ouput path for the statement to be exported, if the required path doesn't exists it will be created.
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Private Function GenerateOutputPath(Optional FileType As String = "0") As Boolean
        Dim tmpAccNum_FirstPart As String = String.Empty 'first 3 chars
        Dim tmpAccNum_MiddlePart As String = String.Empty ' the 6 digits customer number
        Dim tmpAccNum As String
        Dim Flgs As String = String.Empty
        Dim TmpstmDate As String

        'SyncLock lockObject
        Try

            'This is for travel card:
            'CCCCCC.VDXP0100.TCST.FFFFFFFFFF.YYYYMMDD.Statement.PDF

            'This is for shopping card:
            'CCCCCC.VDXP0100.SCST.FFFFFFFFFF.YYYYMMDD.Statement.PDF
            TmpstmDate = USFormatDate(gCurrentMonthEnd, "yyyyMMdd")
            m_pdfFilePathDirectory = String.Empty
            m_pdfFilePathFull = String.Empty
            tmpAccNum = m_TravelCusStm.SHD.CusNum.Trim
            tmpAccNum_FirstPart = tmpAccNum.Substring(0, 3)
            Flgs = "TFFFFFFFFF"
            If FileType = "0" Then
                m_OutputFileName = String.Format("{0}.VDXP0100.TCST.{1}.{2}.Statement.PDF", tmpAccNum, Flgs, TmpstmDate)
            Else
                m_OutputFileName = String.Format("{0}.VDXP0100.SCST.{1}.{2}.Statement.PDF", tmpAccNum, Flgs, TmpstmDate)
            End If


            'm_pdfFilePath = String.Format("{0}\{1}\Statements\{2}\", Me.OutDirRoot_Unit, tmpAccNum(1), m_CusStm.SHD.StmDte.Substring(0, 4))
            m_pdfFilePathDirectory = String.Format("{0}\{1}\{2}\{3}\", Me.OutDirRoot_Unit, tmpAccNum_FirstPart, tmpAccNum, Me._subFolder) ', m_CusStm.SHD.StmDte.Substring(0, 4)

            If Directory.Exists(m_pdfFilePathDirectory) = False Then
                Directory.CreateDirectory(m_pdfFilePathDirectory)
            End If
            m_pdfFilePathFull = m_pdfFilePathDirectory & m_OutputFileName



        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Could not generate pdf File Path [Out.Dir.Root]\[Mdx.Run.Unit]\[AccNum_FirstPart]\[AccNum]\[Stm.Sub.Fld] [{0}], please don't write any leading or trailing '\'.", m_pdfFilePathDirectory), , ex, 1, True, True)
            Return False
        End Try

        Return True
        'End SyncLock
    End Function



    Public Function FormatBalance(ByVal InStr As String, ByVal exp As Integer, ByVal CcyCde As String, Optional ByVal bCommaSep As Boolean = True) As String
        Dim str As String = String.Empty
        Dim dd As Double
        Dim LeftPad As String = "0000"
        Dim Sign As String = ""
        If ((InStr Is Nothing) OrElse InStr.Trim = "") Then Return Nothing
        Try
            'If the input string is a signed number (such as NS string (with a trailing sign)), then remove the sign,
            'process it, then add the sign as a leading sign
            'If CcyCde = "BHD" Or CcyCde = "JOD" Then
            '    exp = 3
            'End If
            exp = getCcyExp(CcyCde)
            Sign = "Cr"
            If InStr.IndexOf("+") >= 0 Then

                InStr = InStr.Replace("+", "")
            End If
            If InStr.IndexOf("-") >= 0 Then
                Sign = "Dr"
                InStr = InStr.Replace("-", "")
            End If

            If InStr.Length < exp Then
                'InStr &= ".0"
                InStr = LeftPad.Substring(0, exp - InStr.Length) & InStr
            End If
            InStr = InStr.Insert(InStr.Length - exp, ".")
            dd = Val(InStr)
            If bCommaSep Then
                Select Case exp
                    Case 0 : str += Format(dd, "#,#0")
                    Case 1 : str += Format(dd, "#,#0.0")
                    Case 2 : str += Format(dd, "#,#0.00")
                    Case 3 : str += Format(dd, "#,#0.000")
                    Case 4 : str += Format(dd, "#,#0.0000")
                End Select
            Else
                Select Case exp
                    Case 0 : str += Format(dd, "#0")
                    Case 1 : str += Format(dd, "#0.0")
                    Case 2 : str += Format(dd, "#0.00")
                    Case 3 : str += Format(dd, "#0.000")
                    Case 4 : str += Format(dd, "#0.0000")
                End Select
            End If
            Return str & " " & Sign 'Add the sign, if any, as a leading character

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("FormatValString:An error has occurred while FormatAmount. amount [{0}]", InStr), String.Empty, ex, LogLevel:=1)
            Return Nothing
        End Try
    End Function

    Public Function FormatBalance1(ByVal InStr As String, ByVal exp As Integer, ByVal CcyCde As String, Optional ByVal bCommaSep As Boolean = True) As String
        Dim str As String = String.Empty
        Dim dd As Double
        Dim LeftPad As String = "0000"
        Dim Sign As String = ""
        If ((InStr Is Nothing) OrElse InStr.Trim = "") Then Return Nothing
        Try
            'If the input string is a signed number (such as NS string (with a trailing sign)), then remove the sign,
            'process it, then add the sign as a leading sign
            exp = getCcyExp(CcyCde)
            'If CcyCde = "BHD" Or CcyCde = "JOD" Then
            '    exp = 3
            'End If

            If InStr.IndexOf("+") >= 0 Then
                Sign = "Cr"
                InStr = InStr.Replace("+", "")
            End If
            If InStr.IndexOf("-") >= 0 Then
                Sign = "Dr"
                InStr = InStr.Replace("-", "")
            End If

            If InStr.Length < exp Then
                'InStr &= ".0"
                InStr = LeftPad.Substring(0, exp - InStr.Length) & InStr
            End If
            InStr = InStr.Insert(InStr.Length - exp, ".")
            dd = Val(InStr)
            If bCommaSep Then
                Select Case exp
                    Case 0 : str += Format(dd, "#,#0")
                    Case 1 : str += Format(dd, "#,#0.0")
                    Case 2 : str += Format(dd, "#,#0.00")
                    Case 3 : str += Format(dd, "#,#0.000")
                    Case 4 : str += Format(dd, "#,#0.0000")
                End Select
            Else
                Select Case exp
                    Case 0 : str += Format(dd, "#0")
                    Case 1 : str += Format(dd, "#0.0")
                    Case 2 : str += Format(dd, "#0.00")
                    Case 3 : str += Format(dd, "#0.000")
                    Case 4 : str += Format(dd, "#0.0000")
                End Select
            End If
            Return str

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("FormatValString:An error has occurred while FormatAmount. amount [{0}]", InStr), String.Empty, ex, LogLevel:=1)
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Format the Amount with respect of Decimal place and format with ","
    ''' </summary>
    ''' <param name="sAmt">Amount value in string</param>
    ''' <returns>Return formated Amount SAR 1,135.35 - BHD 45.350</returns>
    Private Function FormatAmount(ByVal sAmt As String) As String
        Dim lsAmt As String = String.Empty

        Try
            Dim liNDec As Integer = 0
            If sAmt.Length > 0 Then
                If IsNumeric(sAmt) Then
                    liNDec = InStr(sAmt, ".", CompareMethod.Text)
                    If liNDec <> 0 Then liNDec = sAmt.Length - liNDec
                    lsAmt = CDbl(sAmt).ToString("N" & liNDec)
                End If
            End If

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("An error has occurred while FormatAmount. amount [{0}]", sAmt), String.Empty, ex, LogLevel:=1)
        End Try

        Return lsAmt
    End Function


#End Region

#Region " Statement Post Processing "

    ''' <summary>
    ''' The function will export crystal report to PDF.
    ''' </summary>
    ''' <returns>Return TRUE if function executed successfully, FALSE if failed to process.</returns>
    ''' <remarks>
    ''' Created : 2010-11-09 DK
    ''' Modified: 2011-02-13 DK
    ''' </remarks>
    Private Function ExportToPDF(ByVal oCrystalRep As CCrystalReport, ByVal pdfFilePath As String) As Boolean
        Dim isExported As Boolean = False
        _isCurrentGeneratedPDF_File_Found = False
        Try
            Dim dExportToPDF As Date = Now

            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            With oCrystalRep
                
                If Me.m_ForceAddressLeftAlign Then
                    .AlignLeft("CustomerName1")
                    .AlignLeft("Address11")
                    .AlignLeft("Address21")
                    .AlignLeft("Address31")
                    .AlignLeft("Address41")
                Else
                    .AlignRight("CustomerName1")
                    .AlignRight("Address11")
                    .AlignRight("Address21")
                    .AlignRight("Address31")
                    .AlignRight("Address41")
                End If
                If IO.File.Exists(pdfFilePath) Then
                    _isCurrentGeneratedPDF_File_Found = True
                    Logger.LoggerClass.Singleton.LogInfo(0, String.Format("File [{0}], exist and it will be replaced by the new generated pdf.", pdfFilePath), 4)
                End If
                isExported = .ExportReportTo(CrystalDecisions.[Shared].ExportFormatType.PortableDocFormat, pdfFilePath)

                If isExported Then
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "statement -> Exported to: " & pdfFilePath, String.Empty, , LogLevel:=8, ShowOn:=False)
                    'm_StatementSummary.Archived = m_StatementSummary.Archived + 1
                End If

            End With

            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> ExportToPDF, Ended. TimeSpan: " & UDate.getTimeDiffrence(dExportToPDF), 4)

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while Export To PDF.", String.Empty, ex, LogLevel:=1)
            Throw
        End Try

        Return isExported
    End Function



    ''' <summary>
    ''' Clears the report to empty values
    ''' </summary>
    ''' <param name="m_crSmy"></param>
    ''' <remarks></remarks>
    Private Sub clearReport(ByVal m_crSmy As CCrystalReport)

        With m_crSmy
            .SetFormulaField("InpFil", "''")
            .SetFormulaField("StrTim", "''")
            .SetFormulaField("EndTim", "''")
            .SetFormulaField("TotDur", "''")


            .SetFormulaField("TotRec", "''")

            .SetFormulaField("TotStm", "''")


            .SetFormulaField("PrtStm", "''")

            .SetFormulaField("EmlStm", "''")

            .SetFormulaField("StatmentsExported", "''")

            .SetFormulaField("InpFilSize", "''")

            .SetFormulaField("PrsSta", "''")
            .SetFormulaField("PrsErr", "''")

        End With
    End Sub



    ''' <summary>
    ''' Sets the pdf properties
    ''' </summary>
    ''' <param name="pdfFileName"></param>
    ''' <remarks></remarks>
    Private Sub SetPdfProperties(ByVal pdfFileName As String)

        If pdf Is Nothing OrElse Me.m_CurrentStatementNumber Mod Me._report_Max_Usage = 1 Then
            pdf = Nothing
            pdf = New pdfLib.QuickPDF
        End If


        'If pdf Is Nothing OrElse Me.m_CurrentStatementNumber Mod Me._report_Max_Usage = 1 Then
        '    'pdf = Nothing
        '    pdf = pdfLib.QuickPDF.Singleton
        'End If

        Try
            ''Update MetaData
            With _PDFMetaData_current
                .Title = _PDFMetaData_Passed.Title.Replace("BBBB-CCCCCC-SSS", m_TravelCusStm.SHD.CusNum)
                .fileName = pdfFileName
            End With

            pdf.SecureNSign(_PDFMetaData_current)

            pdf.removeDocument()

        Catch ex As Exception
            Logger.LoggerClass.Singleton.LogError(0, "pdfError: " & pdf.getErrorDesc())
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error in pdfSetProperties: " & ex.Message, "", ex, LogLevel:=1)
            Throw
        End Try
    End Sub

#End Region



End Class
