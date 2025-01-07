Imports sib = SIBL0100
Imports CRP0100
Imports SIBL0100.Util
Imports TDXL0100.INIParameters.Statement
Imports VDXExtractDB

''' <summary>
''' Main form of the application
''' The main form opens after creating a separate worker thread for checking for several things like database connection and ftp connections, and fills the main parameters from the ini file and extracting .rpt reports for generating the .pdf statements, because this step might take several seconds and it will make the app look hanged, it was made into a separate worker thread. If there is no errors in this step the execute button will be enabled.
''' When the user clicks the “execute” button, several worker threads are created:
''' “_StatementFiles” that will be the one who will generate the .pdf statements.
''' “_progressLabel” it will make a circulating bar, that will make the user feel that it’s working.
''' The main form will cater for displaying the number of currently generating .pdf statements, and shows the currently uploaded ones to their respective sites like: CIB, RIB.
''' </summary>
''' <remarks></remarks>
Public Class frmMain
    Implements IStatementsForm


#Region " Variables "

    Private Shared _singleton As frmMain 'main instance of the form
    Private objLock As String = "Lock"
    Private objLock2 As String = "Lock"
    Private _locker As String = "Locker"

    Private _workerProcessFiles As System.ComponentModel.BackgroundWorker 'worker for proccessing files on a diffrent thread

    Private _lastFileNumber As Integer = 0 'last file number

    Private WithEvents _statementFiles As StatementFiles
    Private WithEvents _PosStatementFiles As POSStatementFiles
    Private WithEvents _PosShoppingStatementFiles As POSShoppingStatementFiles
    Private _isFormLoaded As Boolean = False 'Flag idicates if the form UI is fully loaded

    Private _messagesCount As Integer = 0 'Number of messages that has been shown till now

    Private _logedMessages As New System.Text.StringBuilder("") 'leged messages to be shown on the form

    Private _filesList As New System.Collections.ArrayList

    Private _workerAppChecks As System.ComponentModel.BackgroundWorker 'worker for proccessing app checks on a diffrent thread
    Private _isAppChecksOK As Boolean 'True if there is no errors in all apps checks
    Private _isExecutionCanceled As Boolean = False 'True if user cancels execution

    Private _oAppChecks As AppChecks 'app checks object

    Private _PDFMetaData As New pdfLib.PDFMetaData
    Private _UIMaxFileList As Integer = 0 'maximum file list

    Private _isRestartIsDone As Boolean = False 'True if restart has finished

    Private _progressLabel As sib.UI.Controls.CProgressLabel 'Progress label object
    Private _progressPOSLabel As sib.UI.Controls.CProgressLabel 'Progress label object
    Private _countExecutionClicked As Integer = 0

#End Region

#Region " Properties "

    'main instance of the form
    Public Shared ReadOnly Property Singleton() As frmMain
        Get
            If _singleton Is Nothing Then
                _singleton = New frmMain
            End If
            Return _singleton
        End Get
    End Property

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

    'Public ReadOnly Property progressLabel As sib.UI.Controls.CProgressLabel
    '    Get
    '        Return _progressLabel
    '    End Get
    'End Property

    'Public ReadOnly Property progressPOSLabel As sib.UI.Controls.CProgressLabel
    '    Get
    '        Return _progressPOSLabel
    '    End Get
    'End Property
#End Region

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If _singleton Is Nothing Then _singleton = Me
    End Sub

    ''' <summary>
    ''' Load UI with correct list columns and sets the required delegates for threading
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the 'DbGridDataSet.StatementFile' table. You can move, or remove it, as needed.
        Me.StatementFileTableAdapter1.Fill(Me.DbGridDataSet.StatementFile)

        Try
            ServicesUti.Services.Singleton.AppInstance.LoadLastPosition(Me)
            SIBL0100.UMsgBox.Singleton.frmMain = Me
            ServicesUti.Services.Singleton.setFrmMain(Me)
            ServicesUti.Uti.ShowAlertsOnScreen = New ServicesUti.Uti.ShowAlertsOnScreenDelegate(AddressOf ShowAlertsOnScreen)
            Me.Text &= " v" & getAppVersion()

            Dim dt, dtBegin, dtEnd As DateTime

            dt = DateTime.Now.AddMonths(-1)
            dtBegin = dt.AddDays(-(dt.Day - 1))
            dtEnd = dtBegin.AddMonths(1).AddDays(-1)
         
            cmbYear.Text = CStr(Year(dtEnd))
            cmbMonth.Text = Format(Month(dtEnd), "00")

            'frmMain.Singleton = Me
#If DEBUG Then
            Me.Text &= " Debug Mode"
            chkSkip.Checked = True
            chkGenSkip.Checked = False

#End If

            _isFormLoaded = True

            ''Start the AppChecks object
            _oAppChecks = New AppChecks(Me.Text)

            _workerAppChecks = New System.ComponentModel.BackgroundWorker
            AddHandler _workerAppChecks.DoWork, AddressOf workerAppChecks_DoWork_applicationChecks
            AddHandler _workerAppChecks.RunWorkerCompleted, AddressOf workerAppChecks_RunWorkerCompleted
            _workerAppChecks.RunWorkerAsync()


            'Setting Delegates

            Me.mylogTextMessage = New logTextMessageDelegate(AddressOf logTextMessage)
            Me.mySetState = New SetStateDelegate(AddressOf SetState)

            showTestLog()

            'For Each column As DataGridViewColumn In Me.grid.Columns
            '    column.SortMode = DataGridViewColumnSortMode.NotSortable
            'Next

            _progressLabel = New sib.UI.Controls.CProgressLabel(Me.lblProgress, 0.5)
            _progressPOSLabel = New sib.UI.Controls.CProgressLabel(Me.lblPOSProgress, 0.5)
            'lblPOSProgress
            'Execute()

            showTestLog()
        Catch ex As Exception
            ServicesUti.Services.Singleton.AppInstance.ModalMessageBox(String.Format("Error in loading form.{0}{1}", vbCrLf, ex.Message), MessageBoxButtons.OK, , MessageBoxIcon.Error, Me.Text)
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


            _UIMaxFileList = INIParameters.Singlton.Glob.MaxFleInLst
#If PLATFORM = "x64" Then
            Me.Text &= " 64 Bit"
#End If
            SetState(UIState.Ready)

            VDXDatabase.Singleton.init(INIParameters.Singlton.Stm.FilArcPath)

            AppFiles.Singleton.init(INIParameters.Singlton.Stm.FilArcPath, INIParameters.Singlton.Glob.InpFleRetain, INIParameters.Singlton.Glob.RptFleRetain, INIParameters.Singlton.Glob.StaFleRetain, INIParameters.Singlton.Glob.AlrAidPath, ServicesUti.AppPaths.singleton.LogFilePath)
            AppFiles.Singleton.CleanFiles()
            Dim arguments As String() = Environment.GetCommandLineArgs()
            If arguments.Length = 3 Then
                If arguments(1).ToUpper = "CUSTOMERINFO" Then
                    chkSkip.Checked = False
                    chkGenSkip.Checked = True
                ElseIf arguments(1).ToUpper = "CUSTOMEREXTRACT" Then
                    chkSkip.Checked = True
                    chkGenSkip.Checked = False
                End If
                If arguments(2).ToUpper = "AUTO" Then
                    Me.Text &= " AUTO"
                    Execute()
                End If
            End If

        End If


    End Sub

    ''' <summary>
    ''' Disposes threads on closing form
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmMain_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try

            If Me._workerProcessFiles IsNot Nothing Then
                If Me._workerProcessFiles.IsBusy Then
                    ServicesUti.Services.Singleton.AppInstance.ModalMessageBox("Sorry you can't close the window till the process is finished or canceled.", MessageBoxButtons.OK, , MessageBoxIcon.Information, Me.Text)
                    e.Cancel = True
                    Exit Sub
                End If
            End If

            Me._progressLabel.cancel()
            'Me._progressPOSLabel.cancel()
            If Me._workerProcessFiles IsNot Nothing Then
                If Me._workerProcessFiles.IsBusy Then
                    Try : Me._workerProcessFiles.CancelAsync() : Catch ex As Exception : End Try
                End If
            End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error in closing form.", ShowPopUpAlert:=True, LogEvent:=True, LogLevel:=1, ShowOn:=True)
        End Try

    End Sub

#Region " Screen Controls Handling "

    Delegate Sub SetStateDelegate(state_ As UIState)
    Public mySetState As SetStateDelegate

    Public Sub SetStateThreadSafe(state_ As UIState) Implements IStatementsForm.SetState
        Me.Invoke(Me.mySetState, state_)
    End Sub

    Private Sub SetState(state_ As UIState)
        SyncLock objLock2

            Try
                Select Case state_
                    Case UIState.Processing
                        Me.btnExecute.Enabled = False
                        Me.btnCancel.Enabled = True
                        Me.btnClose.Enabled = False

                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnExecute, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnCancel, "Enabled", True)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnClose, "Enabled", False)
                    Case UIState.Cancel
                        Me.btnExecute.Enabled = True
                        Me.btnCancel.Enabled = False
                        Me.btnClose.Enabled = True

                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnExecute, "Enabled", True)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnCancel, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnClose, "Enabled", True)

                    Case UIState.StopTotaly

                        Me.btnExecute.Enabled = False
                        Me.btnCancel.Enabled = False
                        Me.btnClear.Enabled = False
                        Me.btnClose.Enabled = True

                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnExecute, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnCancel, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnClear, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnClose, "Enabled", True)

                    Case UIState.Ready
                        Me.btnExecute.Enabled = True
                        Me.btnCancel.Enabled = False
                        Me.btnClose.Enabled = True

                        sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnExecute, "Enabled", True)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnCancel, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnClose, "Enabled", True)

                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnDatabase, "Enabled", True)

                        Me._progressLabel.cancel()
                        'Me._progressPOSLabel.cancel()

                    Case UIState.ReadyOnAllThreadsFinished

                        If Me._workerProcessFiles.IsBusy Then
                            Exit Sub
                        End If
                        'If Me._oFileUpload.IsBusy Then
                        '    Exit Sub
                        'End If

                        Me.btnExecute.Enabled = True
                        Me.btnCancel.Enabled = False
                        Me.btnClose.Enabled = True

                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnExecute, "Enabled", True)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnCancel, "Enabled", False)
                        'sib.UI.Controls.Control.SetControlPropertyAsync(Me.btnClose, "Enabled", True)

                End Select

            Catch ex As Exception
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
            End Try
        End SyncLock

    End Sub

#End Region

#Region " Functions "

    

    ''' <summary>
    ''' Updates the statement count entry in the file grid
    ''' </summary>
    ''' <param name="status_"></param>
    ''' <param name="statement_"></param>
    ''' <remarks></remarks>
    Private Sub Grid_UpdateRow(status_ As String, statement_ As clsStatement)
        SyncLock _locker

            Try
                Dim row As dsGridDataSet.StatementFileRow
                Dim rows() As System.Data.DataRow
                rows = Me.DsGridDataSet.StatementFile.Select("idNo=" & Me._lastFileNumber)
                If rows.Length <= 0 Then
                    Exit Sub
                End If
                row = DirectCast(rows(0), dsGridDataSet.StatementFileRow)

                If statement_.CurrentStatement >= 998 Then
                    Debug.WriteLine("")
                End If

                If statement_.CurrentStatement <= statement_.TotalCustomers Then
                    row.CustomerNo = String.Format("{0:##,#0}/{1:##,#0}", statement_.CurrentStatement, statement_.TotalCustomers)
                End If

                'row.lines = statement_.fileReachedLine.ToString("##,#0") 'ArrayItemNum & "/" & TotalLines

                row.totalTime = UDate.getTimeDiffrence(statement_.StartDate, Date.Now)

                If status_ <> String.Empty Then
                    Dim dNow As Date
                    dNow = Date.Now

                    row.endTime = dNow.ToString(ServicesUti.GlobalVars.C_UI_DateFormat)

                End If
                'BindingContext(Me.grid.DataSource).Current 
            Catch ex As Exception
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
            End Try
        End SyncLock

    End Sub

    ''' <summary>
    ''' Updates the statement count entry in the file grid
    ''' </summary>
    ''' <param name="status_"></param>
    ''' <param name="statement_"></param>
    ''' <remarks></remarks>
    Private Sub Grid_POSUpdateRowTravel(status_ As String, statement_ As POSclsStatement)
        SyncLock _locker

            Try
                Dim row As dsGridDataSet.StatementFileRow
                Dim rows() As System.Data.DataRow
                rows = Me.DsGridDataSet.StatementFile.Select("idNo=" & Me._lastFileNumber)
                If rows.Length <= 0 Then
                    Exit Sub
                End If
                row = DirectCast(rows(0), dsGridDataSet.StatementFileRow)

                If statement_.CurrentStatement >= 998 Then
                    Debug.WriteLine("")
                End If

                row.totalTime = UDate.getTimeDiffrence(statement_.StartDate, Date.Now)
                Dim dNow As Date
                dNow = Date.Now

                row.endTime = dNow.ToString(ServicesUti.GlobalVars.C_UI_DateFormat)
                'BindingContext(Me.grid.DataSource).Current
            Catch ex As Exception
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
            End Try
        End SyncLock

    End Sub

    Private Sub Grid_POSUpdateRowShopping(status_ As String, statement_ As POSShoppingclsStatement)
        SyncLock _locker

            Try
                Dim row As dsGridDataSet.StatementFileRow
                Dim rows() As System.Data.DataRow
                rows = Me.DsGridDataSet.StatementFile.Select("idNo=" & Me._lastFileNumber)
                If rows.Length <= 0 Then
                    Exit Sub
                End If
                row = DirectCast(rows(0), dsGridDataSet.StatementFileRow)

                If statement_.CurrentStatement >= 998 Then
                    Debug.WriteLine("")
                End If

                row.totalTime = UDate.getTimeDiffrence(statement_.StartDate, Date.Now)
                Dim dNow As Date
                dNow = Date.Now

                row.endTime = dNow.ToString(ServicesUti.GlobalVars.C_UI_DateFormat)
            Catch ex As Exception
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
            End Try
        End SyncLock

    End Sub
    ''' <summary>
    ''' Delegate for poping up a msg for the user
    ''' </summary>
    ''' <param name="fileProcessStartDate_"></param>
    ''' <param name="totalNumberOfStatements_"></param>
    ''' <param name="CurrentStatementNumber_"></param>
    ''' <param name="workingFile_"></param>
    ''' <param name="sPassedError_"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Delegate Function AskUserDelegate(ByVal fileProcessStartDate_ As Date, ByVal totalNumberOfStatements_ As Integer, ByVal CurrentStatementNumber_ As Integer, ByVal workingFile_ As String, _
                    ByVal sPassedError_ As String) As frmFileErrorB.enumFileErrorUserSelection
    Public myAskUser As AskUserDelegate

    Public Function AskUser_ThreadSafe(ByVal fileProcessStartDate_ As Date, ByVal totalNumberOfStatements_ As Integer, ByVal CurrentStatementNumber_ As Integer, ByVal workingFile_ As String, _
                 ByVal sPassedError_ As String) As frmFileErrorB.enumFileErrorUserSelection Implements IStatementsForm.AskUser
        Return DirectCast(Me.Invoke(Me.myAskUser, fileProcessStartDate_, totalNumberOfStatements_, CurrentStatementNumber_, workingFile_, sPassedError_), frmFileErrorB.enumFileErrorUserSelection)
    End Function

    ''' <summary>
    ''' Asks the user what to do when an error occures
    ''' </summary>
    ''' <param name="fileProcessStartDate_"></param>
    ''' <param name="totalNumberOfStatements_"></param>
    ''' <param name="CurrentStatementNumber_"></param>
    ''' <param name="workingFile_"></param>
    ''' <param name="sPassedError_"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function AskUser(ByVal fileProcessStartDate_ As Date, ByVal totalNumberOfStatements_ As Integer, ByVal CurrentStatementNumber_ As Integer, ByVal workingFile_ As String, _
                    ByVal sPassedError_ As String) As frmFileErrorB.enumFileErrorUserSelection
        Try

            Using frmAsk As New frmFileErrorB(fileProcessStartDate_, totalNumberOfStatements_, CurrentStatementNumber_, workingFile_, sPassedError_)
                frmAsk.ShowDialog(frmMain.Singleton)
            End Using

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try

        Return gUserSelection
    End Function

#End Region

    ''' <summary>
    ''' calls the execute method
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnExecute_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExecute.Click
        Execute()
    End Sub

    ''' <summary>
    ''' begin the process of statement creation from .dat files
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Execute()
        Try
            Dim lMonth As String = ""
            Dim lYear As String = ""

            '_isExecutionCanceled = False
            PublicVars.isProcessFiles_PDF_Finished = False
            _lastFileNumber = 0
            SetState(UIState.Processing)
            clearControls()
            Me.DsGridDataSet.StatementFile.Rows.Clear()
            Me._filesList.Clear()
            Me.prgBarFiles.Value = 0

            initPdfMeta()
            Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider()
            Dim lRow As DataRow
            Dim lStatementMonth As DataSet
            lStatementMonth = New DataSet
            If oDBProvider.getStatementMonth(cmbMonth.Text, cmbYear.Text, lStatementMonth) > 0 Then
                lRow = lStatementMonth.Tables(0).Rows(0)
                'gLastMonthEndString = lRow(0).ToString.Trim
                gCurrentMonthEnd = Convert.ToDateTime(lRow(5))
                'gCurrentMonthStart = Convert.ToDateTime(lRow(2))
                gLastMonthEnd = Convert.ToDateTime(lRow(5))
            End If

            _statementFiles = New StatementFiles(Me, INIParameters.Singlton.Stm.InpFlePath, INIParameters.Singlton.Stm.WorkDirectory _
                                                  , INIParameters.Singlton.Stm.OutDirRoot, INIParameters.Singlton.Stm.FilArcPath, INIParameters.Singlton.Stm.FleBadPath _
                                                  , INIParameters.Singlton.Stm.InputFileExt)
            _countExecutionClicked += 1
            _PosStatementFiles = New POSStatementFiles(Me, INIParameters.Singlton.Stm.OutDirRoot, INIParameters.Singlton.Stm.TmpPath, Me._PDFMetaData, INIParameters.Singlton.Stm.RPT_Max_Usage, INIParameters.Singlton.Stm.StmSubFld)
            _PosStatementFiles.StatementMonth = cmbMonth.Text
            _PosStatementFiles.StatementYear = cmbYear.Text

            _countExecutionClicked += 1

            '_PosShoppingStatementFiles
            _PosShoppingStatementFiles = New POSShoppingStatementFiles(Me, INIParameters.Singlton.Stm.OutDirRoot, INIParameters.Singlton.Stm.TmpPath, Me._PDFMetaData, INIParameters.Singlton.Stm.RPT_Max_Usage, INIParameters.Singlton.Stm.StmSubFld)
            _PosShoppingStatementFiles.StatementMonth = cmbMonth.Text
            _PosShoppingStatementFiles.StatementYear = cmbYear.Text
            '_postilianExtract = New PostilianExtract(Me, "", "")
#If DEBUG Then
            Me._statementFiles.isExport = True
            Me._PosShoppingStatementFiles.isExport = True
            Me._PosStatementFiles.isExport = True

            '_statementFiles.isShowReportPreview = True
#End If

            'Me._progressLabel.start()


            'Start the processing of .dat files
            Me._workerProcessFiles = New System.ComponentModel.BackgroundWorker ' New Thread(New ThreadStart(AddressOf m_statementFiles.StartProcess))
            Me._workerProcessFiles.WorkerSupportsCancellation = True
            Me._workerProcessFiles.WorkerReportsProgress = True
            AddHandler Me._workerProcessFiles.DoWork, AddressOf workerProcessFiles_DoWork
            AddHandler Me._workerProcessFiles.RunWorkerCompleted, AddressOf workerProcessFiles_RunWorkerCompleted

            AddHandler Me._statementFiles.InputFileChanged, AddressOf statementFiles_InputFileChanged
            AddHandler Me._PosStatementFiles.InputFileChanged, AddressOf POSstatementFiles_InputFileChanged
            AddHandler Me._PosShoppingStatementFiles.InputFileChanged, AddressOf POSstatementFiles_InputFileChanged

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
        If Not chkSkip.Checked Then
            Me._progressLabel.start()
            Me._statementFiles.StartProcess()
        End If

        If Not chkGenSkip.Checked Then
            'Me._progressPOSLabel.start()
            If INIParameters.Singlton.Glob.ExtractType = 0 Then
                Me._PosStatementFiles.StartProcess()
                Me._PosShoppingStatementFiles.StartProcess()
            ElseIf INIParameters.Singlton.Glob.ExtractType = 1 Then
                Me._PosStatementFiles.StartProcess()
            Else
                Me._PosShoppingStatementFiles.StartProcess()
            End If

        End If

        'Me._postilianExtract.StartProcess()
        'e.Result = applicationChecks()
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
    ''' called when the file upload is finished
    ''' </summary>
    ''' <param name="sender_"></param>
    ''' <param name="progress_"></param>
    ''' <remarks></remarks>
    Private Sub FileUpload_FinishedFileUpload(ByVal sender_ As Object, progress_ As SIBL0100.ProgressTrack)

        finishedAllWork()
    End Sub

    ''' <summary>
    ''' Called on every change or step happening to the .dat file
    ''' </summary>
    ''' <param name="sender_"></param>
    ''' <param name="progress_"></param>
    ''' <remarks></remarks>
    Private Sub statementFiles_InputFileChanged(ByVal sender_ As Object, progress_ As SIBL0100.ProgressTrack)
        If progress_.TotalCount < progress_.CurrentCount Then Exit Sub
        SyncLock objLock2

            If Me.prgBarFiles.Maximum <> progress_.TotalCount Then sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.prgBarFiles, "Maximum", progress_.TotalCount) ' Me.prgBarFiles.Maximum = progress_.TotalCount

            sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.prgBarFiles, "Value", progress_.CurrentCount)

            If progress_.TotalCount = 0 Then
                sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.lblFilPrs, "Text", "")
            Else
                sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.lblFilPrs, "Text", String.Format("{0:##,#0}/{1:##,#0}", progress_.CurrentCount, progress_.TotalCount))
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Called on every change or step happening to the .dat file
    ''' </summary>
    ''' <param name="sender_"></param>
    ''' <param name="progress_"></param>
    ''' <remarks></remarks>
    Private Sub POSstatementFiles_InputFileChanged(ByVal sender_ As Object, progress_ As SIBL0100.ProgressTrack)
        If progress_.TotalCount < progress_.CurrentCount Then Exit Sub
        SyncLock objLock2

            If Me.POSprgBarFiles.Maximum <> progress_.TotalCount Then sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.POSprgBarFiles, "Maximum", progress_.TotalCount)

            sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.POSprgBarFiles, "Value", progress_.CurrentCount)

            If progress_.TotalCount = 0 Then
                sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.lblPOSFilPrs, "Text", "")
            Else
                sib.UI.Controls.Control.SetControlPropertyAsync(frmMain.Singleton.lblPOSFilPrs, "Text", String.Format("{0:##,#0}/{1:##,#0}", progress_.CurrentCount, progress_.TotalCount))
            End If
        End SyncLock
    End Sub
    ''' <summary>
    ''' closes the frm main
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    ''' <summary>
    ''' Called when either the ftp upload is finished or the .dat processing is finished
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub finishedAllWork()
        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started.")


            If Me._statementFiles Is Nothing Then Logger.LoggerClass.Singleton.LogError(0, String.Format("[{0}] is null.", "_statementFiles"))
            If Me._progressLabel Is Nothing Then Logger.LoggerClass.Singleton.LogError(0, String.Format("[{0}] is null.", "_progressLabel"))
            If Me._progressPOSLabel Is Nothing Then Logger.LoggerClass.Singleton.LogError(0, String.Format("[{0}] is null.", "_progressPOSLabel"))

            If Not _statementFiles.currentStatement Is Nothing Then
                _statementFiles.IsCanceledJobs = True
            End If
            If Not _PosStatementFiles.currentStatement Is Nothing Then
                _PosStatementFiles.IsCanceledJobs = True
            End If
            If Not _PosShoppingStatementFiles.currentStatement Is Nothing Then
                _PosShoppingStatementFiles.IsCanceledJobs = True
            End If
            SetState(UIState.Cancel)

            Me._progressLabel.cancel()
            'Me._progressPOSLabel.cancel()

            Exit Sub

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try

    End Sub

    ''' <summary>
    ''' calls cancelExecution
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        cancelExecution()
    End Sub

    ''' <summary>
    ''' Cancels the current job
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub cancelExecution()
        Try
            If Not Me._workerProcessFiles Is Nothing AndAlso Me._workerProcessFiles.IsBusy Then
                Dim Res As DialogResult
                Res = ServicesUti.Services.Singleton.AppInstance.ModalMessageBox("The processs is still running..." & vbCrLf & _
                                            "Do you want to cancel and discard the process?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2, MessageBoxIcon.Information, _
                                              "Cancel " & Me.Text)
                If Res = MsgBoxResult.No Then Exit Sub
            End If

            _isExecutionCanceled = True
            If Not _statementFiles.currentStatement Is Nothing Then
                _statementFiles.IsCanceledJobs = True
            End If

            If Not _PosStatementFiles.currentStatement Is Nothing Then
                _PosStatementFiles.IsCanceledJobs = True
            End If

            If Not _PosShoppingStatementFiles.currentStatement Is Nothing Then
                _PosShoppingStatementFiles.IsCanceledJobs = True
            End If

            Me._progressLabel.cancel()
            'Me._progressPOSLabel.cancel()
            SetState(UIState.Cancel)
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try
    End Sub

    Private Sub frmMain_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        If (ServicesUti.Services.Singleton.AppInstance.gAppBusy And Not (ServicesUti.Services.Singleton.AppInstance.gLoggingOff)) Then e.Cancel = True : Return

        _singleton = Nothing

        Try

            If Not Me._workerProcessFiles Is Nothing Then Me._workerProcessFiles.CancelAsync()
            Me._progressLabel.cancel()
            'Me._progressPOSLabel.cancel()
            Me._workerProcessFiles = Nothing

            ServicesUti.Services.Singleton.AppInstance.SaveLastPosition(Me)

        Catch ex As Exception
#If DEBUG Then
            MsgBox(ex.ToString)
#End If


        End Try
    End Sub

    ''' <summary>
    ''' calls the clearControls
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
        clearControls()
    End Sub

    ''' <summary>
    ''' clears the UI controls
    ''' </summary>
    ''' <remarks></remarks>
    Sub clearControls()
        Try

            If Me.btnExecute.Enabled = True Then
                Me.DsGridDataSet.StatementFile.Rows.Clear()
                Me.prgBarFiles.Value = 0

            End If

            If _logedMessages IsNot Nothing Then _logedMessages.Clear()
            _logedMessages = New System.Text.StringBuilder("")
            _messagesCount = 0

            Me.txtMsgLog.Clear()
            Me.lblFilPrs.Text = String.Empty
            Me.lblProgress.Text = String.Empty
            Me.lblPOSFilPrs.Text = String.Empty
            Me.lblPOSProgress.Text = String.Empty
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try

    End Sub

    Delegate Sub previewReportDelegate(oCrystalRep As CCrystalReport, dataSource_ As Object)
    Public mypreviewReport As previewReportDelegate

   

    'Private Sub btnLog_Click(sender As System.Object, e As System.EventArgs) Handles btnLog.Click
    '    showTestLog()
    'End Sub

    ''' <summary>
    ''' Shows the text log message box or hides it
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub showTestLog()
        Try
            If Me.txtMsgLog.Visible Then
                'Hide Log
                Me.txtMsgLog.Visible = False
                Me.txtMsgLog.Dock = DockStyle.None
                Me.txtMsgLog.Text = String.Empty
                'Me.lstViewStatus.Dock = DockStyle.Fill
                'Me.grid.Dock = DockStyle.Fill
                'Me.pnlGrids.Visible = True
                'pnlTop.Dock = DockStyle.Fill
                btnLog.Text = "Show Log"
            Else
                'Show Log
                'Me.lstViewStatus.Dock = DockStyle.Top
                'Me.pnlGrids.Visible = False
                'Me.grid.Dock = DockStyle.None
                Me.txtMsgLog.Text = _logedMessages.ToString
                Me.txtMsgLog.Dock = DockStyle.Fill
                Me.txtMsgLog.Visible = True
                'pnlTop.Dock = DockStyle.Top
                btnLog.Text = "Hide Log"
            End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error in showTestLog.", String.Empty, ex, LogLevel:=1, ShowPopUpAlert:=False)
        End Try
    End Sub

    Delegate Sub logTextMessageDelegate(ByVal sMessage_ As String)
    Public mylogTextMessage As logTextMessageDelegate

    Public Sub logTextMessage_ThreadSafe(sMessage_ As String) Implements IStatementsForm.logTextMessage
        Me.Invoke(Me.mylogTextMessage, sMessage_)
    End Sub

    ''' <summary>
    ''' Logs the message into the collectection of messages and shows it if pnltxtMsgLog is visible
    ''' </summary>
    ''' <param name="sMessage_"></param>
    ''' <remarks></remarks>
    Private Sub logTextMessage(sMessage_ As String)
        SyncLock (objLock)
            _logedMessages.Append(vbCrLf & sMessage_)

            If Me.txtMsgLog.Visible Then
                Try
                    txtMsgLog.Text = _logedMessages.ToString
                    'SyncLock objLock2
                    '    sib.UI.Controls.Control.SetControlPropertyAsync(txtMsgLog, "Text", _logedMessages.ToString)
                    'End SyncLock
                Catch ex As Exception
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while ShowAlertsOnScreen." & ex.Message, String.Empty, ex, LogLevel:=1, ShowPopUpAlert:=True, ShowOn:=False)
                End Try
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' logs message
    ''' </summary>
    ''' <param name="LogMessage"></param>
    ''' <param name="MaxLines"></param>
    ''' <remarks></remarks>
    Public Sub ShowAlertsOnScreen(ByVal LogMessage As String, ByVal MaxLines As Integer)
        'Dim lsLogMsg As String
        'If m_isFormLoaded = False Then
        '    m_collectedMsgsBeforeLoadingForm &= vbCrLf & LogMessage
        '    Exit Sub
        'End If
        'If m_collectedMsgsBeforeLoadingForm <> String.Empty Then
        '    LogMessage = m_collectedMsgsBeforeLoadingForm & vbCrLf & LogMessage
        '    m_collectedMsgsBeforeLoadingForm = String.Empty
        'End If

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


    Private Sub pnltxtMsgLog_Resize(sender As Object, e As EventArgs) Handles pnltxtMsgLog.Resize

        pnlPdfGenerate.Size = New Drawing.Size(pnlPdfGenerate.Size.Width, pnltxtMsgLog.Size.Height)

    End Sub

End Class
