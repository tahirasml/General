Imports INIClass
Imports System.Drawing.Printing
Imports System.Reflection
Imports System.IO
Imports CrystalDecisions.Windows.Forms
Imports SIBL0100.SQL
Imports SIBL0100.Util
Imports ServicesUti
Imports SIBL0100
Imports System.Globalization
''' <summary>
''' The module contains the Global variables and functions, used within application
''' </summary>
''' <remarks>
''' Created : 2010-12-18
''' Modified: 2011-02-05
''' Modified: 2011-02-12
''' </remarks>
Public Module PublicVars

#Region " Fields, Variables & Constants"

    Public gLastErrorMsg As String = String.Empty 'Last Error 


    Public gUserSelection As frmFileErrorB.enumFileErrorUserSelection 'User selection

    Public gCrystalViewer As CrystalReportViewer = Nothing 'Globale object for crystal report

    Public C_DBName As String = "dbGrid.mdb" '.mdb name

    'Public isMDOX_Restarted As Boolean = False 'True if the application has restarted
    'Public isMDOX_IsMoreFiles As Boolean = False 'True if there are more .dat files, that need processing

    Public Const C_Args_Restart As String = "Restart" 'the restart command argument, that is passed to the app when being run.
    Public Const C_Args_IsMoreFiles As String = "IsMoreFiles" 'the is more files command argument, that is passed to the app when being run.
    Public Const C_DB_Version As String = "2.0.0" 'Current db version
    Public gCurrency As DataSet
    Public gCurrencyList() As Structures.CurrencyList
    'Public gLastMonthEndString As String
    Public gCurrentMonthEnd As Date
    'Public gCurrentMonthStart As Date
    Public gLastMonthEnd As Date
    Friend isProcessFiles_PDF_Finished As Boolean = False 'true when the generation of .pdf files has finished

#End Region

#Region " Project init "

    ''' <summary>
    ''' first entry for the application
    ''' it sets the visauls and initializes the logging, then open the main form
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <STAThread()> _
    Public Sub Main()

        Dim arguments As String() = Environment.GetCommandLineArgs()
        Dim vdx As New TDOXMain
        If Not init() Then
            Exit Sub
        End If

        gCurrency = New DataSet

        'If arguments.Length = 4 Then
        '    If arguments(1).ToUpper = "CUSTOMERINFO" Then

        '    ElseIf arguments(1).ToUpper = "CUSTOMEREXTRACT" Then

        '    End If
        '    If arguments(2).ToUpper = "AUTO" Then

        '        'Execute()
        '    End If


        '    'If arguments(3).ToUpper = "Win" Then
        '    '    Application.EnableVisualStyles()
        '    '    Application.SetCompatibleTextRenderingDefault(False)

        '    '    'Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        '    '    System.Threading.Thread.CurrentThread.Name = "Main"
        '    '    Application.Run(New TDOXMainForm)
        '    'Else

        '    '    vdx.startTDX()
        '    'End If
        'Else
        '    vdx.startTDX()
        'End If

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        'Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        System.Threading.Thread.CurrentThread.Name = "Main"
        'Application.Run(New TDOXMainForm)
        Application.Run(New frmMain)




    End Sub

    ''' <summary>
    ''' connects to the main exception handlers, and initilize the log
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function init() As Boolean

        Try
            'connectMainExceptions()

            If Not File.Exists(ServicesUti.AppPaths.singleton.AppIni) Then
                MsgBox(String.Format("Error, Could not run application, becuase .ini file not found. [{0}]", ServicesUti.AppPaths.singleton.AppIni), MsgBoxStyle.Critical)
                'Console.WriteLine(String.Format("Error, Could not run application, becuase .ini file not found. [{0}]", ServicesUti.AppPaths.singleton.AppIni))
                Return False
            End If

            If Not ServicesUti.AppPaths.singleton.load() Then
                Return False
            End If

            If Not InitializeLogFile(AppPaths.singleton.LogFilePath) Then
                Return False
            End If


            ServicesUti.Services.Singleton.init()

            Return True
        Catch ex As Exception
            'If Not gLogger Is Nothing Then Logger.LoggerClass.Singleton.LogError(0, "Error in running form: " & ex.Message)
            If Not ServicesUti.Services.Singleton.AppInstance Is Nothing Then
                ServicesUti.Services.Singleton.AppInstance.HandleError(0, String.Format("Error in running form.{0}{1}", vbCrLf, ex.Message), "Error", ex)
            Else
                Windows.Forms.MessageBox.Show(String.Format("Error in running form.{0}{1}", vbCrLf, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If
            Debug.Write(ex.Message)
            Return False
        End Try

    End Function


    Public Function initCMD() As Boolean

        Try
            'connectMainExceptions()

            If Not File.Exists(ServicesUti.AppPaths.singleton.AppIni) Then
                'MsgBox(String.Format("Error, Could not run application, becuase .ini file not found. [{0}]", ServicesUti.AppPaths.singleton.AppIni), MsgBoxStyle.Critical)
                Console.WriteLine(String.Format("Error, Could not run application, becuase .ini file not found. [{0}]", ServicesUti.AppPaths.singleton.AppIni))
                Return False
            End If

            If Not ServicesUti.AppPaths.singleton.load() Then
                Return False
            End If

            If Not InitializeLogFile(AppPaths.singleton.LogFilePath) Then
                Return False
            End If


            ServicesUti.Services.Singleton.init()

            Return True
        Catch ex As Exception
            'If Not gLogger Is Nothing Then Logger.LoggerClass.Singleton.LogError(0, "Error in running form: " & ex.Message)
            If Not ServicesUti.Services.Singleton.AppInstance Is Nothing Then
                ServicesUti.Services.Singleton.AppInstance.HandleError(0, String.Format("Error in running form.{0}{1}", vbCrLf, ex.Message), "Error", ex)
            Else
                Windows.Forms.MessageBox.Show(String.Format("Error in running form.{0}{1}", vbCrLf, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If
            Debug.Write(ex.Message)
            Return False
        End Try

    End Function

    Public Function getCcyExp(ByVal CcyCde As String) As Integer
        Dim idx As Integer
        Dim st As Integer
        idx = LocateCcyCde(Trim(CcyCde))
        If idx >= 0 Then
            st = gCurrencyList(idx).CcyExp
            Return st
        End If
        Return 0
    End Function

    Public Function DecodeCcyCde(ByVal CcyCde As String, Optional ByVal Arabic As Boolean = False) As String
        Dim idx As Integer
        Dim st As String
        idx = LocateCcyCde(Trim(CcyCde))
        If idx >= 0 Then
            st = gCurrencyList(idx).CcyDscEng
            If (st = "") Or Arabic Then st = gCurrencyList(idx).CcyDscAra
            Return st
        End If
        Return ""
    End Function

    Public Function LocateCcyCde(ByVal CcyCde As String) As Integer
        Dim i, c As Integer
        If (gCurrencyList Is Nothing) Then Return -1
        c = 0
        For i = 0 To gCurrencyList.Length - 1
            If (gCurrencyList(i).CcyCde = CcyCde) Or (gCurrencyList(i).CcyNum = CcyCde) Then Return c
            c += 1
        Next
        Return -1
    End Function

    ''' <summary>
    ''' returns the ole db path
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getDBOlePath() As String
        Return String.Format("{0}", INIParameters.Singlton.Glob.DBOleFilePath)
    End Function

    ''' <summary>
    ''' returns the ole db connection
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getOleDBProviderConnection() As SIBL0100.SQL.SQLOleDB
        Dim sCon As String = PublicVars.getDBOlePath

        Logger.LoggerClass.Singleton.LogInfo(0, "Getting new OleDBConnection, con=" & sCon, 6)

        Dim oleProvider As New SIBL0100.SQL.SQLOleDB(sCon, "")
        Return oleProvider
    End Function
    ''' <summary>
    ''' returns the sql ce connection
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getSQLCeDBProviderConnection() As System.Data.SqlServerCe.SqlCeConnection
        Dim sCon As String = String.Format("Data Source={0}", INIParameters.Singlton.Glob.DBSQLCeFilePath)
        Logger.LoggerClass.Singleton.LogInfo(0, "Getting new SqlCeConnection, con=" & sCon, 6)
        Dim oConn As New System.Data.SqlServerCe.SqlCeConnection(sCon)

        Return oConn
    End Function

    ''' <summary>
    ''' path of the db template
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getDBTemplatePath() As String
        If INIParameters.Singlton.Glob.DBProvider.ToUpper = "OLEDB" Then
            Return Application.StartupPath & "\Resources\dbGrid_Template.mdb"
        Else
            Return Application.StartupPath & "\Resources\dbStat_Template.sdf"
        End If

    End Function


    ''' <summary>
    ''' inits the crystal report viewer
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function initCrystalReportViewer() As Boolean
        Try

            'If gCrystalViewer Is Nothing Then
            '      gCrystalViewer = New CrystalReportViewer
            'End If

            If Not gCrystalViewer Is Nothing Then
                gCrystalViewer.CloseView(Nothing)
                gCrystalViewer.Dispose()
                gCrystalViewer = Nothing
            End If
            gCrystalViewer = New CrystalReportViewer

            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error: " & ex.Message, ShowPopUpAlert:=True, p_exception:=ex)
        End Try

        Return False
    End Function

    ''' <summary>
    ''' return the app version
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getAppVersion() As String
        Return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
    End Function
#End Region

#Region " Structures "
    ''' <summary>
    ''' Collects statical information of all the files processed
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PostilianExtractSummary
        Public StartTime As Date, EndTime As Date
        Public Total As Integer = 0 'Total processed records
        Public FinalSummary As New PostilianCustomerSummary 'Last summery

        Public Sub Add(ByVal Summary As PostilianCustomerSummary) 'Add statement stats to the process stats
            With FinalSummary
                .TotalShopping = .TotalShopping + Summary.TotalShopping
                .TotalTravel = .TotalTravel + Summary.TotalTravel
            End With
        End Sub

        ''' <summary>
        ''' Duration from started date till end date.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Duration() As String
            Return UDate.getTimeDiffrence(StartTime, EndTime)
        End Function
    End Class
    ''' <summary>
    ''' Collects statical information of all the files processed
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProcessSummary
        Public StartTime As Date, EndTime As Date
        Public Total As Integer = 0 'Total processed records
        Public Skip As Integer = 0 'Skipped records
        Public Process As Integer = 0
        Public Failed As Integer = 0 'Failes records
        Public CurrentCusNum As Integer = 0 'Current file number

        Public FinalSummary As New StatementSummary 'Last summery
        Private _overAllFileNo As Integer = 0
        Private _currentFileParentNo As Integer = 0
        Private _executionNo As Integer = 0 'Current execution number
        Private _currentFileChildNo As Integer = 0

        Public Property ExecutionNo As Integer
            Get
                Return _executionNo
            End Get
            Set(value As Integer)
                If _executionNo = value Then
                    Return
                End If
                _executionNo = value
            End Set
        End Property

        Public Property OverAllFileNo As Integer
            Get
                Return _overAllFileNo
            End Get
            Set(value As Integer)
                _overAllFileNo = value
            End Set
        End Property

        Public Sub Add(ByVal Summary As StatementSummary) 'Add statement stats to the process stats
            With FinalSummary
                .Total = .Total + Summary.Total
            End With
        End Sub

        ''' <summary>
        ''' Duration from started date till end date.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Duration() As String
            Return UDate.getTimeDiffrence(StartTime, EndTime)
        End Function
    End Class

    ''' <summary>
    ''' statement stats
    ''' </summary>
    ''' <remarks></remarks>
    Public Class StatementSummary
        Public StartTime As Date, EndTime As Date
        Public Total As Long = 0 'Total statements in file
        Public FileSize As Double = 0 'File Size in MG
        Public FileTotalLines As Long = 0 ' 

        Public Function Duration() As String
            Return UDate.getTimeDiffrence(StartTime, EndTime)
        End Function
    End Class

    ''' <summary>
    ''' statement stats
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PostilianCustomerSummary
        Public StartTime As Date, EndTime As Date
        Public TotalTravel As Long = 0 'Total statements in file
        Public TotalShopping As Long = 0 'Total statements in file

        Public Function Duration() As String
            Return UDate.getTimeDiffrence(StartTime, EndTime)
        End Function
    End Class
#End Region

#Region " General Function "

    ''' <summary>
    ''' Add List of installed printers to the Combo Box.
    ''' </summary>
    ''' <param name="CmbBox">Combo Box Name where list is required to be populated.</param>
    ''' <param name="DefaultPrint">Specify the name of the default printer name.</param>
    ''' <remarks>
    ''' Created : 2010-12-19 DK
    ''' </remarks>
    Public Sub FillListofInstalledPrinters(ByRef CmbBox As ComboBox, ByVal DefaultPrint As String)
        Dim InstalledPrinter As String
        Dim i As Integer = 0
        Try
            For Each InstalledPrinter In PrinterSettings.InstalledPrinters
                CmbBox.Items.Add(InstalledPrinter)
                If DefaultPrint = InstalledPrinter Then
                    CmbBox.SelectedIndex = i
                End If
                i = i + 1
            Next
        Catch ex As Exception
            ServicesUti.Services.Singleton.AppInstance.HandleError(&H89010000, "Initializing of installed printer(s) failed. " & ex.Message, "VDX Critical Error")
        End Try
    End Sub

    Public Function USFormatDate(ByVal dt As Date, ByVal fs As String) As String
        Dim st As String
        Dim OldCultInfo, NewCultInfo As CultureInfo
        OldCultInfo = System.Threading.Thread.CurrentThread.CurrentCulture
        Try
            NewCultInfo = New CultureInfo("en-US", False)
            System.Threading.Thread.CurrentThread.CurrentCulture = NewCultInfo
            st = Format(dt, fs)
        Catch
            st = "******"
        Finally
            System.Threading.Thread.CurrentThread.CurrentCulture = OldCultInfo
        End Try
        Return st
    End Function

   

#End Region

#Region " Logging & Alerts"

    ''' <summary>
    ''' Create/Append log file on the specified path
    ''' </summary>
    ''' <param name="logFolderPath">The log folder path</param>
    ''' <remarks>
    ''' Created : 2010-12-19 DK
    ''' </remarks>
    Public Function InitializeLogFile(ByVal logFolderPath As String) As Boolean
        Try


            Try
                If Directory.Exists(logFolderPath) = False Then
                    Directory.CreateDirectory(logFolderPath)
                End If
            Catch ex As Exception
                If Directory.Exists(logFolderPath) = False Then
                    MessageBox.Show(String.Format("Could not access/create Log Folder: [{0}]", logFolderPath))
                    Return False
                End If
            End Try

            If Not UFolder.isDirectoryWritable(logFolderPath) Then
                MessageBox.Show(String.Format("Log Folder: No write permision is allowed for folder: [{0}]", logFolderPath))
            End If
           
            Logger.LoggerClass.Singleton.init(ServicesUti.Services.Singleton.C_AppName & "0100", logFolderPath, Environment.UserName.ToUpper)

            Dim Ini As New IniFileClass(AppPaths.singleton.AppIni)

            With INIParameters.Singlton.Glob
                .LogEvtLevel = Convert.ToInt32(Ini.GetValue("Log.Evt.Level", "GLOBALS", "1").Trim)
                .LogMaxSize = Convert.ToInt32(Ini.GetValue("Log.Max.Size", "GLOBALS", "524288").Trim)
            End With

            With Logger.LoggerClass.Singleton
                .LogLevel = INIParameters.Singlton.Glob.LogEvtLevel
                .MaxSize = INIParameters.Singlton.Glob.LogMaxSize
                .MaxCount = Convert.ToInt32("9999")
                .WriteToLogFile = True
                .Suffix = ServicesUti.Services.Singleton.C_AppName & "0100."
            End With

            Logger.SQLLogger.Singleton.init("sql_DB", logFolderPath, Environment.UserName.ToUpper)

            With Logger.SQLLogger.Singleton
                .LogLevel = INIParameters.Singlton.Glob.LogEvtLevel
                .MaxSize = INIParameters.Singlton.Glob.LogMaxSize
                .MaxCount = Convert.ToInt32("9999")
                .WriteToLogFile = True
                .Suffix = String.Format("sql_{0}0100.", ServicesUti.Services.Singleton.C_AppName)
            End With

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "___________________________________________________________________________________________________________________", ShowOn:=False)
            'EventHandler(&H89010000, SIBL0100.EventType.Information, "Starting VDXP0100", ShowOn:=False)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("Starting {0} - {1}", [Assembly].GetExecutingAssembly().GetName().Name, [Assembly].GetExecutingAssembly().GetName().Version.ToString), ShowOn:=False)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "Application is compiled at " & IIf(SIBL0100.URuntime.is64bit(), "x64", "x86").ToString, ShowOn:=True)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "Logging Services Started.", ShowOn:=True)
            Logger.SQLLogger.Singleton.log(SIBL0100.EventType.Information, Now, "_________________________________________________________________________", "_________________________________________________________________________", 2, True, 1)

            Return True
        Catch ex As Exception
            MessageBox.Show(String.Format("Error in initializing Log: [{0}]", ex.Message))
            Return False
        End Try

    End Function

#End Region

    ''' <summary>
    ''' connects the main exceptions to their app handlers
    ''' </summary>
    ''' <remarks></remarks>
    Sub connectMainExceptions()
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)

        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf CurrentDomain_UnhandledException
        AddHandler Application.ThreadException, AddressOf Application_ThreadException

    End Sub

    ''' <summary>
    ''' Handles the domain Unhandled Exception
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub CurrentDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Try
            Dim ex As Exception = CType(e.ExceptionObject, Exception)
            Dim errorMsg As String = "An application error occurred. Please contact the adminstrator " & _
                "with the following information:" & ControlChars.Lf & ControlChars.Lf

            ' Since we can't prevent the app from terminating, log this to the event log. 
            If (Not EventLog.SourceExists("ThreadException")) Then
                EventLog.CreateEventSource("ThreadException", ServicesUti.Services.Singleton.C_AppName)
            End If

            errorMsg &= ex.Message & ControlChars.Lf & ControlChars.Lf & _
                "Stack Trace:" & ControlChars.Lf & ex.StackTrace

            ' Create an EventLog instance and assign its source. 
            Dim myLog As New EventLog()
            myLog.Source = "ThreadException"
            myLog.WriteEntry(errorMsg)

            MessageBox.Show(errorMsg, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)
        Catch exc As Exception
            Try
                MessageBox.Show("Fatal Non-UI Error", "Fatal Non-UI Error. Could not write the error to the event log. " & _
                    "Reason: " & exc.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Finally
                Application.Exit()
            End Try
        End Try
    End Sub

    ''' <summary>
    ''' Handles the application Thread Exception 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub Application_ThreadException(sender As Object, e As System.Threading.ThreadExceptionEventArgs)
        Dim result As System.Windows.Forms.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Try
            result = ShowThreadExceptionDialog("Windows Forms Error", e.Exception)
        Catch
            Try
                MessageBox.Show("Fatal Windows Forms Error", _
                    "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop)
            Finally
                Application.Exit()
            End Try
        End Try

        ' Exits the program when the user clicks Abort. 
        If result = DialogResult.Abort Then
            Application.Exit()
        End If
    End Sub

    ''' <summary>
    ''' Shows msg box for the thread dialog
    ''' </summary>
    ''' <param name="title"></param>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ShowThreadExceptionDialog(ByVal title As String, ByVal e As Exception) As DialogResult
        Dim errorMsg As String = "An application error occurred. Please contact the adminstrator " & _
                                 "with the following information:" & ControlChars.Lf & ControlChars.Lf

        errorMsg = errorMsg & e.Message & ControlChars.Lf & _
                     ControlChars.Lf & "Stack Trace:" & ControlChars.Lf & e.StackTrace

        Return MessageBox.Show(errorMsg, title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop)
    End Function


End Module

