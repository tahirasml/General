
Imports sib = SIBL0100
Imports CRP0100
Imports SIBL0100.Util
Imports TDXL0100.INIParameters.Statement
Imports VDXExtractDB

Public Class TDOXMain



#Region " Variables "

    'Private Shared _singleton As frmMain 'main instance of the form
    'Private objLock As String = "Lock"
    'Private objLock2 As String = "Lock"
    'Private _locker As String = "Locker"

    Private _workerProcessFiles As System.ComponentModel.BackgroundWorker 'worker for proccessing files on a diffrent thread
    Private _workerAppChecks As System.ComponentModel.BackgroundWorker 'worker for proccessing app checks on a diffrent thread
    Private _lastFileNumber As Integer = 0 'last file number

    Private WithEvents _statementFiles As StatementFilesCMD
    Private WithEvents _PosStatementFiles As POSStatementFilesCMD
    Private WithEvents _PosShoppingStatementFiles As POSShoppingStatementFilesCMD
    Private _isFormLoaded As Boolean = False 'Flag idicates if the form UI is fully loaded

    Private _messagesCount As Integer = 0 'Number of messages that has been shown till now

    Private _logedMessages As New System.Text.StringBuilder("") 'leged messages to be shown on the form

    Private _filesList As New System.Collections.ArrayList


    Private _isAppChecksOK As Boolean 'True if there is no errors in all apps checks
    Private _isExecutionCanceled As Boolean = False 'True if user cancels execution

    Private _oAppChecks As AppChecks 'app checks object

    Private _PDFMetaData As New pdfLib.PDFMetaData
    'Private _UIMaxFileList As Integer = 0 'maximum file list

    Private _isRestartIsDone As Boolean = False 'True if restart has finished

    'Private _countExecutionClicked As Integer = 0

    Private lMonth As String
    Private lYear As String
#End Region

#Region " Properties "


    Public ReadOnly Property WorkerProcessFiles As System.ComponentModel.BackgroundWorker
        Get
            Return _workerProcessFiles
        End Get
    End Property

    Public ReadOnly Property WorkerAppChecks As System.ComponentModel.BackgroundWorker
        Get
            Return _workerAppChecks
        End Get
    End Property

    Public ReadOnly Property oAppChecks As AppChecks
        Get
            Return _oAppChecks
        End Get
    End Property

   
#End Region

    Public Sub New()

    End Sub

    Public Sub startTDX()


        Try

            ServicesUti.Uti.ShowAlertsOnScreen = New ServicesUti.Uti.ShowAlertsOnScreenDelegate(AddressOf ShowAlertsOnScreen)

            Dim dt, dtBegin, dtEnd As DateTime

            dt = DateTime.Now.AddMonths(-1)
            dtBegin = dt.AddDays(-(dt.Day - 1))
            dtEnd = dtBegin.AddMonths(1).AddDays(-1)

            lYear = CStr(Year(dtEnd))
            lMonth = Format(Month(dtEnd), "00")


            _isFormLoaded = True

            ''Start the AppChecks object
            _oAppChecks = New AppChecks(" ")

            _workerAppChecks = New System.ComponentModel.BackgroundWorker
            AddHandler _workerAppChecks.DoWork, AddressOf workerAppChecks_DoWork_applicationChecks
            AddHandler _workerAppChecks.RunWorkerCompleted, AddressOf workerAppChecks_RunWorkerCompleted
            _workerAppChecks.RunWorkerAsync()


            'Execute()
        Catch ex As Exception
            ServicesUti.Services.Singleton.AppInstance.ModalMessageBox(String.Format("Error in loading form.{0}{1}", vbCrLf, ex.Message), MessageBoxButtons.OK, , MessageBoxIcon.Error, "")
        End Try
    End Sub

    ''' <summary>
    ''' begin the appchecks
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Sub workerAppChecks_DoWork_applicationChecks(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        e.Result = Me._oAppChecks.checks()
    End Sub

    ''' <summary>
    ''' After appchecks is finished, this function is called
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub workerAppChecks_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) ' Handles mmmm.RunWorkerCompleted
        _isAppChecksOK = CBool(e.Result)
        If _isAppChecksOK Then
            Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider()
            Dim lRow As DataRow
            oDBProvider.tblSequence_Init()
            gCurrency = New DataSet
            If oDBProvider.execute("Select * from Currency", gCurrency) > 0 Then
                ReDim gCurrencyList(gCurrency.Tables(0).Rows.Count - 1)
                For i As Integer = 0 To gCurrency.Tables(0).Rows.Count - 1
                    lRow = gCurrency.Tables(0).Rows(i)
                    gCurrencyList(i).CcyCde = lRow(0).ToString.Trim
                    gCurrencyList(i).CcyNum = lRow(1).ToString.Trim
                    gCurrencyList(i).CcyDscEng = lRow(2).ToString.Trim
                    gCurrencyList(i).CcyDscAra = lRow(3).ToString.Trim
                    gCurrencyList(i).CcyExp = CInt(lRow(4))
                Next

            End If



            VDXDatabase.Singleton.init(INIParameters.Singlton.Stm.FilArcPath)

            AppFiles.Singleton.init(INIParameters.Singlton.Stm.FilArcPath, INIParameters.Singlton.Glob.InpFleRetain, INIParameters.Singlton.Glob.RptFleRetain, INIParameters.Singlton.Glob.StaFleRetain, INIParameters.Singlton.Glob.AlrAidPath, ServicesUti.AppPaths.singleton.LogFilePath)
            AppFiles.Singleton.CleanFiles()
            Dim arguments As String() = Environment.GetCommandLineArgs()
            If arguments.Length = 3 Then
                If arguments(1).ToUpper = "CUSTOMERINFO" Then
                   
                ElseIf arguments(1).ToUpper = "CUSTOMEREXTRACT" Then
                   
                End If
                If arguments(2).ToUpper = "AUTO" Then

                    Execute()
                End If
            End If

        End If


    End Sub

    


    Public Sub Execute()
        Try
            Dim lMonth As String = ""
            Dim lYear As String = ""

            '_isExecutionCanceled = False
            PublicVars.isProcessFiles_PDF_Finished = False
            _lastFileNumber = 0


            initPdfMeta()
            Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider()
            Dim lRow As DataRow
            Dim lStatementMonth As DataSet
            lStatementMonth = New DataSet
            If oDBProvider.getStatementMonth(lMonth, lYear, lStatementMonth) > 0 Then
                lRow = lStatementMonth.Tables(0).Rows(0)
                'gLastMonthEndString = lRow(0).ToString.Trim
                gCurrentMonthEnd = Convert.ToDateTime(lRow(5))
                'gCurrentMonthStart = Convert.ToDateTime(lRow(2))
                gLastMonthEnd = Convert.ToDateTime(lRow(5))
            End If

            _statementFiles = New StatementFilesCMD(INIParameters.Singlton.Stm.InpFlePath, INIParameters.Singlton.Stm.WorkDirectory _
                                                  , INIParameters.Singlton.Stm.OutDirRoot, INIParameters.Singlton.Stm.FilArcPath, INIParameters.Singlton.Stm.FleBadPath _
                                                  , INIParameters.Singlton.Stm.InputFileExt)

            _PosStatementFiles = New POSStatementFilesCMD(INIParameters.Singlton.Stm.OutDirRoot, INIParameters.Singlton.Stm.TmpPath, Me._PDFMetaData, INIParameters.Singlton.Stm.RPT_Max_Usage, INIParameters.Singlton.Stm.StmSubFld)
            _PosStatementFiles.StatementMonth = lMonth
            _PosStatementFiles.StatementYear = lYear


            '_PosShoppingStatementFiles
            _PosShoppingStatementFiles = New POSShoppingStatementFilesCMD(INIParameters.Singlton.Stm.OutDirRoot, INIParameters.Singlton.Stm.TmpPath, Me._PDFMetaData, INIParameters.Singlton.Stm.RPT_Max_Usage, INIParameters.Singlton.Stm.StmSubFld)
            _PosShoppingStatementFiles.StatementMonth = lMonth
            _PosShoppingStatementFiles.StatementYear = lYear
           

            'Start the processing of .dat files
            Me._workerProcessFiles = New System.ComponentModel.BackgroundWorker ' New Thread(New ThreadStart(AddressOf m_statementFiles.StartProcess))
            Me._workerProcessFiles.WorkerSupportsCancellation = True
            Me._workerProcessFiles.WorkerReportsProgress = True
            AddHandler Me._workerProcessFiles.DoWork, AddressOf workerProcessFiles_DoWork
            AddHandler Me._workerProcessFiles.RunWorkerCompleted, AddressOf workerProcessFiles_RunWorkerCompleted

         

            Me._workerProcessFiles.RunWorkerAsync()


        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try

    End Sub

    ''' <summary>
    ''' init pdf meta data from the .ini file
    ''' </summary>
    ''' <remarks></remarks>
    Sub initPdfMeta()
        Dim PDFMain As PDFProperties
        PDFMain = INIParameters.Singlton.Stm.PDF

        With _PDFMetaData
            .Title = PDFMain.Title
            .Subject = PDFMain.Subject
            .Author = PDFMain.Author
            .Application = PDFMain.Application
            .Producer = PDFMain.Producer
            .Company = "The Saudi Investment Bank (SAIB)"
            .PdfCertificatePath = PDFMain.PdfCrtPath
            .PdfCertificatePassword = PDFMain.CrtPwd

            .isSecurePDF = PDFMain.SecFlag
            .isSignPDF = PDFMain.SgnFlag
            .isSetPDFProperties = PDFMain.PDFPrpFlag
            .encryptPassword = "40940.3615575347"


            .SigningReason = "Signing"

            .Strength = pdfLib.EncyptionStrength.e128_bit_AES
            .CanPrint = 1
            .CanCopy = 1

            .CanChange = 0
            .CanAddNotes = 0
            .CanFillFields = 0
            .CanCopyAccess = 0
            .CanAssemble = 0
            .CanPrintFull = 0

        End With
    End Sub

    ''' <summary>
    ''' starts the .dat files processing
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Sub workerProcessFiles_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
      

        If INIParameters.Singlton.Glob.ExtractType = 0 Then
            Me._PosStatementFiles.StartProcess()
            Me._PosShoppingStatementFiles.StartProcess()
        ElseIf INIParameters.Singlton.Glob.ExtractType = 1 Then
            Me._PosStatementFiles.StartProcess()
        Else
            Me._PosShoppingStatementFiles.StartProcess()
        End If



    End Sub

    ''' <summary>
    ''' gets called when the .dat files processing is finished
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub workerProcessFiles_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)

        PublicVars.isProcessFiles_PDF_Finished = True
        finishedAllWork()
    End Sub

    ''' <summary>
    ''' Called when either the ftp upload is finished or the .dat processing is finished
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub finishedAllWork()
        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started.")


            If Me._statementFiles Is Nothing Then Logger.LoggerClass.Singleton.LogError(0, String.Format("[{0}] is null.", "_statementFiles"))

            If Not _statementFiles.currentStatement Is Nothing Then
                _statementFiles.IsCanceledJobs = True
            End If
            If Not _PosStatementFiles.currentStatement Is Nothing Then
                _PosStatementFiles.IsCanceledJobs = True
            End If
            If Not _PosShoppingStatementFiles.currentStatement Is Nothing Then
                _PosShoppingStatementFiles.IsCanceledJobs = True
            End If
           

            Exit Sub

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try

    End Sub

   
    Private Sub logTextMessage(sMessage_ As String)

        Console.WriteLine(sMessage_)
    End Sub

    ''' <summary>
    ''' logs message
    ''' </summary>
    ''' <param name="LogMessage"></param>
    ''' <param name="MaxLines"></param>
    ''' <remarks></remarks>
    Public Sub ShowAlertsOnScreen(ByVal LogMessage As String, ByVal MaxLines As Integer)

        Try

            If _messagesCount > 200 Then
                _logedMessages.Clear()
                _logedMessages = New System.Text.StringBuilder("")
                _messagesCount = 0
            End If
            logTextMessage(LogMessage)
            _messagesCount += 1

        Catch ex As Exception

        End Try
    End Sub


End Class
