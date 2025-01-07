Imports INIClass
Imports SIBL0100.Util
Imports SIBL0100
Imports TDXL0100.INIParameters.Statement


''' <summary>
''' This class is used to load the .ini settings
''' </summary>
''' <remarks></remarks>
Public Class INIParameters

    Private Shared m_singlton As INIParameters

    Public Glob As [Global]
    Public Stm As Statement


    Private _Ini As IniFileClass '.ini object

    Private _errorList As New Generic.SortedList(Of String, String) 'Error list in the ini
    Private _paramsValues As New Generic.SortedList(Of String, String) 'read parameters values

    Public ReadOnly Property ParamsValues As Generic.SortedList(Of String, String)
        Get
            Return _paramsValues
        End Get
    End Property


    Public Shared ReadOnly Property Singlton() As INIParameters
        Get
            If m_singlton Is Nothing Then
                m_singlton = New INIParameters
            End If
            Return m_singlton
        End Get
    End Property

    Public Structure [Global]
        Dim LogMaxSize, ErrRtyCount, ErrRtySleep, LogEvtLevel As Integer 'Fixed
        Dim MaxFleInLst As Integer 'Fixed
        Dim AlrEmlTo, AlrEmlCc, AlrEmlFlag, AlrAidPath As String 'Fixed
        Dim SMTPServer, SMTPPort, FromEmail As String
        Dim InpFleRetain, RptFleRetain As Integer 'Fixed
        Dim StaFleRetain As Integer 'Fixed

        Dim InputFileExt As String 'Fixed
        Dim FTPCdeTyp As String 'Fixed
        Dim DBProvider As String 'Fixed
        Dim WorkPoolSleep As Integer 'Fixed
        Dim DBOleFilePath As String 'Fixed
        Dim DBSQLCeFilePath As String 'Fixed
        Dim DBExtractSQLConnectionstring As String 'Fixed

        Dim MaxFilesPerProc As Integer 'Fixed
        Dim UpdateUIEveryFile As Integer 'Fixed
        Dim UploadErrorTrailTimes As Integer 'Fixed
        Dim StartUpMode As Integer 'Fixed
        Dim ErrorRetryCount As Integer
        Dim gSleepTime As Integer 'Sleep time when trying to upload file.
        Dim gIntYieldTime As Integer 'The period in milliseconds that MAXP0100 sleeps between processing/error retries.

        Dim RPT_BAR_CDE_FNT_SZE_1 As String 'Fixed
        Dim RPT_BAR_CDE_FNT_SZE_2 As String 'Fixed

        Dim CheckDriveInfo As Boolean
        Dim MinimumDriveSize As Integer
        Dim MaximumFolderFiles As Integer
        Dim ShowBalances As Boolean
        Dim startCustomerID As Integer
        Dim BatchSize As Integer
        Dim OnlyStaff As Boolean
        Dim ExtractType As Integer 'Extract type: 0 for both, 1 for travel only,2 for shopping only
    End Structure

    ''' <summary>
    ''' General statement properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure Statement

        Dim WorkDirectory As String 'Fixed
        Dim InpFlePath, OutDirRoot, FilArcPath, FleBadPath As String 'Fixed
        Dim InputFileExt As String 'Fixed

        Dim StmSubFld As String 'Fixed

        Dim RPT_Max_Usage As Integer 'Fixed

        'Dim PrnFlag As Boolean 'Fixed
        'Dim PrnTarget As String 'Fixed

        Dim TmpPath As String 'Fixed


        Dim PDF As PDFProperties 'Fixed


        Public Structure PDFProperties
            Dim Title, Subject, Author, Producer, Application, PdfCrtPath, CrtPwd As String
            Dim SecFlag, SgnFlag, PDFPrpFlag As Boolean
        End Structure

    End Structure

#Region " me._Ini Functions "

    ''' <summary>
    ''' Checks the .ini value if it is null or not, is its a file or directory, and checks their path
    ''' </summary>
    ''' <param name="param_"></param>
    ''' <param name="val_"></param>
    ''' <param name="isCheckPath_"></param>
    ''' <param name="isFile_"></param>
    ''' <remarks></remarks>
    Sub checkIniParameter(ByVal param_ As String, ByVal val_ As String, Optional ByVal isCheckPath_ As Boolean = False, Optional ByVal isFile_ As Boolean = False, Optional ByVal isCheckDirectoryWritability_ As Boolean = False)

        If Not _Ini.isValueFound(val_) Then
            _errorList.Item(param_) = String.Empty
            Exit Sub
        End If

        _paramsValues.Item(param_) = val_

        If isCheckPath_ Then
            If isFile_ Then
                If _Ini.isValueFound(val_) AndAlso Not IO.File.Exists(val_) Then _errorList.Item(param_) = param_ & ": File does not exist."
            Else
                If Not IO.Directory.Exists(val_) Then
                    _errorList.Item(param_) = param_ & ": Directory does not exist."
                ElseIf Not SIBL0100.UFolder.isFolderValid(val_) Then
                    _errorList.Item(param_) = param_ & ": Directory has exceeded the number of files/directories it has."
                ElseIf isCheckDirectoryWritability_ Then
                    If Not UFolder.isDirectoryWritable(val_) Then
                        '_errorList.Item(param_) = param_ & ": No write permision is allowed for directory, please give write permision."
                    End If

                End If
            End If
        End If
    End Sub


    ''' <summary>
    ''' Read the ini file and assign their values
    ''' </summary>
    ''' <param name="iniFilePath">The complete path where me._Ini file exists.</param>
    ''' <returns>Return TRUE if me._Ini read sucessfully, or return FALSE incase of failure.</returns>
    ''' <remarks>
    ''' Created : 2010-12-19 DK
    ''' Mofified: 2011-02-06 DK
    ''' Errors: make defualt values for all me._Ini properties
    ''' </remarks>
    Public Function ReadINIFile(ByVal iniFilePath As String) As Boolean
        If Not IO.File.Exists(iniFilePath) Then
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "me._Ini file could not be found: " & iniFilePath, , , ShowPopUpAlert:=True)
            Return False
        End If

        Dim dStart As DateTime

        Try
            Me._Ini = New IniFileClass(iniFilePath)

            INIParameters.Singlton.Glob = New [Global]
            INIParameters.Singlton.Stm = New Statement

            ServicesUti.Services.Singleton.AppInstance.Unit = Me._Ini.GetValueWithDefault("Tdx.Run.Unit", "GLOBALS", String.Empty).Trim
            checkIniParameter("Tdx.Run.Unit", ServicesUti.Services.Singleton.AppInstance.Unit)

            dStart = Now
            getParamsGlobal()
            Logger.LoggerClass.Singleton.LogInfo(0, "ReadINIFile -> getParamsGlobal. TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)
            '--------------------------------------------


            dStart = Now
            getParamsDB()
            Logger.LoggerClass.Singleton.LogInfo(0, "ReadINIFile -> getParamsDB. TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)

            dStart = Now
            getParamsSTMT()
            Logger.LoggerClass.Singleton.LogInfo(0, "ReadINIFile -> getParamsSTMT. TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)

            dStart = Now
            getParmasSTMP_PDF()
            Logger.LoggerClass.Singleton.LogInfo(0, "ReadINIFile -> getParmasSTMP_PDF. TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)



            dStart = Now
            getParamsDrive()
            Logger.LoggerClass.Singleton.LogInfo(0, "ReadINIFile -> getParamsDrive. TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)


            Me._Ini = Nothing

            If _errorList.Count <= 0 Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "Initialization of .Ini file completed successfully.")
                Return True
            Else
                setIniParametersInfo()
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "The following parameter(s) are not set in the .me._Ini file, or the paths are not found:")
                For Each sItem As String In _errorList.Values
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, sItem)
                Next
                Return False
            End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Failed to read data from the me._Ini file: {0} Error: {1}", iniFilePath, ex.Message), , ex, ShowPopUpAlert:=False)
            Return False
        Finally
            Me._Ini = Nothing
        End Try
    End Function

    ''' <summary>
    ''' retrieve the global parameters values from .ini file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub getParamsGlobal()
        Dim tmp As String = String.Empty
        INIParameters.Singlton.Glob.LogMaxSize = Convert.ToInt32(Me._Ini.GetValueWithDefault("Log.Max.Size", "GLOBALS", "524288").Trim)
        INIParameters.Singlton.Glob.LogEvtLevel = Convert.ToInt32(Me._Ini.GetValueWithDefault("Log.Evt.Level", "GLOBALS", "1").Trim)
        INIParameters.Singlton.Glob.ErrRtyCount = Convert.ToInt32(Me._Ini.GetValueWithDefault("Err.Rty.Count", "GLOBALS", "3").Trim)
        INIParameters.Singlton.Glob.ErrRtySleep = Convert.ToInt32(Me._Ini.GetValueWithDefault("Err.Rty.Sleep", "GLOBALS", "500").Trim)
        INIParameters.Singlton.Glob.StartUpMode = Convert.ToInt32(Me._Ini.GetValueWithDefault("StartupMode", "GLOBALS", "0").Trim)
        INIParameters.Singlton.Glob.MaxFleInLst = Convert.ToInt32(Me._Ini.GetValueWithDefault("Max.Fle.In.Lst", "GLOBALS", "250").Trim)

        INIParameters.Singlton.Glob.AlrEmlTo = Me._Ini.GetValueWithDefault("Alr.Eml.To", "GLOBALS", String.Empty).Trim
        checkIniParameter("Alr.Eml.To", INIParameters.Singlton.Glob.AlrEmlTo)

        INIParameters.Singlton.Glob.AlrEmlCc = Me._Ini.GetValueWithDefault("Alr.Eml.Cc", "GLOBALS", String.Empty).Trim
        checkIniParameter("Alr.Eml.Cc", INIParameters.Singlton.Glob.AlrEmlCc)

        INIParameters.Singlton.Glob.AlrEmlFlag = Me._Ini.GetValueWithDefault("Alr.Eml.Flag", "GLOBALS", "Y").Trim.ToUpper

        INIParameters.Singlton.Glob.AlrAidPath = Me._Ini.GetValueWithDefault("Alr.Aid.Path", "GLOBALS", String.Empty).Trim
        checkIniParameter("Alr.Aid.Path", INIParameters.Singlton.Glob.AlrAidPath, True, False, True)

        INIParameters.Singlton.Glob.FromEmail = Me._Ini.GetValueWithDefault("FromEmail", "GLOBALS", String.Empty).Trim
        checkIniParameter("FromEmail", INIParameters.Singlton.Glob.FromEmail)

        INIParameters.Singlton.Glob.SMTPServer = Me._Ini.GetValueWithDefault("SMTPhost", "GLOBALS", String.Empty).Trim
        checkIniParameter("SMTPhost", INIParameters.Singlton.Glob.SMTPServer, False, False, False)

        INIParameters.Singlton.Glob.SMTPPort = Me._Ini.GetValueWithDefault("SMTPPort", "GLOBALS", String.Empty).Trim
        checkIniParameter("SMTPPort", INIParameters.Singlton.Glob.SMTPPort)

        INIParameters.Singlton.Glob.InputFileExt = Me._Ini.GetValueWithDefault("Inp.Fle.Mask", "GLOBALS", ".pdf").Trim.ToLower
        checkIniParameter("Inp.Fle.Mask", INIParameters.Singlton.Glob.InputFileExt, False, False)

        INIParameters.Singlton.Glob.InpFleRetain = CInt(Me._Ini.GetValueWithDefault("Inp.Fle.Retain", "GLOBALS", "-1").Trim) 'this should have a defualt value
        If INIParameters.Singlton.Glob.InpFleRetain <= 0 Then _errorList.Item("Inp.Fle.Retain") = String.Empty

        INIParameters.Singlton.Glob.RptFleRetain = CInt(Me._Ini.GetValueWithDefault("Rpt.Fle.Retain", "GLOBALS", "-1").Trim) 'this should have a defualt value
        If INIParameters.Singlton.Glob.RptFleRetain <= 0 Then _errorList.Item("Rpt.Fle.Retain") = String.Empty

        INIParameters.Singlton.Glob.RPT_BAR_CDE_FNT_SZE_1 = Me._Ini.GetValueWithDefault("RPT.BAR.CDE.FNT.SZE.1", "GLOBALS", "").Trim
        INIParameters.Singlton.Glob.RPT_BAR_CDE_FNT_SZE_2 = Me._Ini.GetValueWithDefault("RPT.BAR.CDE.FNT.SZE.2", "GLOBALS", "").Trim
        INIParameters.Singlton.Glob.StaFleRetain = getParameterInteger("Sta.Fle.Retain", "GLOBALS", "-1")
        INIParameters.Singlton.Glob.MaxFilesPerProc = getParameterInteger("Max.Files.Per.Proc", "GLOBALS", "")
        INIParameters.Singlton.Glob.UpdateUIEveryFile = getParameterInteger("Update.UI.Every.File", "GLOBALS", "-1")
        INIParameters.Singlton.Glob.ErrorRetryCount = getParameterInteger("Error.Retry.Count", "GLOBALS", "")
        INIParameters.Singlton.Glob.ShowBalances = getBooleanParameter("Statment.Show.Balance", "GLOBALS", "F")
        INIParameters.Singlton.Glob.OnlyStaff = getBooleanParameter("Export.Staff", "GLOBALS", "F")
        INIParameters.Singlton.Glob.startCustomerID = getParameterInteger("Start.Customer.ID", "GLOBALS", "1")
        INIParameters.Singlton.Glob.BatchSize = getParameterInteger("Batch.Size", "GLOBALS", "2000")
        INIParameters.Singlton.Glob.ExtractType = getParameterInteger("Extract.Type", "GLOBALS", "0")


    End Sub


    ''' <summary>
    ''' retrieve the database parameters values from .ini file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub getParamsDB()
        Dim sTemp As String
        Dim sDecrypt As String

        INIParameters.Singlton.Glob.DBProvider = Me._Ini.GetValueWithDefault("DB.Provider", "GLOBALS", "SQLCeServer").Trim
        checkIniParameter("DB.Provider", INIParameters.Singlton.Glob.DBProvider, False, False, False)

        Dim a As New Encrypt

        sTemp = Me._Ini.GetValueWithDefault("ExtractConnectionString", "GLOBALS", "").Trim
        'sDecrypt = a.Encrypt(sTemp, "postilian")
        INIParameters.Singlton.Glob.DBExtractSQLConnectionstring = a.Decrypt(sTemp, "postilian")
        checkIniParameter("ExtractConnectionString", sTemp, False, True, False)


        'sTemp = Me._Ini.GetValueWithDefault("PostilianConnectionString", "GLOBALS", "").Trim
        'INIParameters.Singlton.Glob.DBPostilionSQLConnectionstring = sTemp
        'checkIniParameter("PostilianConnectionString", sTemp, False, True, False)

        sTemp = Me._Ini.GetValueWithDefault("DB.Ole.File.Path", "GLOBALS", "").Trim
        INIParameters.Singlton.Glob.DBOleFilePath = sTemp
        checkIniParameter("DB.Ole.File.Path", sTemp, False, True, False)
        If IO.File.Exists(sTemp) Then
            Dim oInf As IO.FileInfo = New IO.FileInfo(sTemp)
            If oInf.IsReadOnly Then
                _errorList.Item("DB.Ole.File.Path") = String.Format("The file '{0}' Can't be read only.", sTemp)
            End If
        End If
    End Sub

    ''' <summary>
    ''' retrieve the drive parameters values from .ini file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub getParamsDrive()

        Dim sTagName As String = String.Empty
        sTagName = "GLOBALS"

        INIParameters.Singlton.Glob.CheckDriveInfo = getBooleanParameter("Check.Drive.Info", sTagName, "F")
        If INIParameters.Singlton.Glob.CheckDriveInfo = False Then Exit Sub
        INIParameters.Singlton.Glob.MinimumDriveSize = getParameterInteger("Minimum.Drive.Size", sTagName, "")
        INIParameters.Singlton.Glob.MaximumFolderFiles = getParameterInteger("Maximum.Folder.Files", sTagName, "")


    End Sub


    ''' <summary>
    ''' retrieve the statement parameters values from .ini file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub getParamsSTMT()
        Dim i As Integer

        INIParameters.Singlton.Stm.WorkDirectory = Me._Ini.GetValueWithDefault("Vdx.Wrk.Path", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Vdx.Wrk.Path", INIParameters.Singlton.Stm.WorkDirectory, True, False, True)

        INIParameters.Singlton.Stm.InpFlePath = Me._Ini.GetValueWithDefault("Inp.Fle.Path", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Inp.Fle.Path", INIParameters.Singlton.Stm.InpFlePath, True, False, True)

        INIParameters.Singlton.Stm.OutDirRoot = Me._Ini.GetValueWithDefault("Out.Dir.Root", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Out.Dir.Root", INIParameters.Singlton.Stm.OutDirRoot, True, False, True)


        INIParameters.Singlton.Stm.StmSubFld = Me._Ini.GetValueWithDefault("Stm.Sub.Fld", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Stm.Sub.Fld", INIParameters.Singlton.Stm.StmSubFld)

        INIParameters.Singlton.Stm.RPT_Max_Usage = getParameterInteger("Obj.Max.Usage", "STATEMENTS", "0")

        INIParameters.Singlton.Stm.InputFileExt = Me._Ini.GetValueWithDefault("Inp.Fle.Mask", "STATEMENTS", ".dat").Trim.ToLower
        checkIniParameter("Inp.Fle.Mask", INIParameters.Singlton.Stm.InputFileExt, False, False)

        INIParameters.Singlton.Stm.FilArcPath = Me._Ini.GetValueWithDefault("Fle.Arc.Path", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Fle.Arc.Path", INIParameters.Singlton.Stm.FilArcPath, True, False, True)


        INIParameters.Singlton.Stm.FleBadPath = Me._Ini.GetValueWithDefault("Fle.Bad.Path", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Fle.Bad.Path", INIParameters.Singlton.Stm.FleBadPath, True, False, True)

        INIParameters.Singlton.Stm.TmpPath = Me._Ini.GetValueWithDefault("Tmp.Path", "STATEMENTS", String.Empty).Trim
        checkIniParameter("Tmp.Path", INIParameters.Singlton.Stm.TmpPath, True, False, True)

    End Sub

    ''' <summary>
    ''' retrieve the statement pdf parameters values from .ini file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub getParmasSTMP_PDF()
        With INIParameters.Singlton.Stm.PDF
            .Title = Me._Ini.GetValueWithDefault("PDF.Title", "STATEMENTS", "Electronic statement of account BBBB-CCCCCC-SSS").Trim

            .Subject = Me._Ini.GetValueWithDefault("PDF.Subject", "STATEMENTS", "Electronic Customer Documents").Trim

            .Author = Me._Ini.GetValueWithDefault("PDF.Author", "STATEMENTS", "The Saudi Investment Bank").Trim

            .Producer = Me._Ini.GetValueWithDefault("PDF.Producer", "STATEMENTS", "SAIB").Trim

            .Application = Me._Ini.GetValueWithDefault("PDF.Application", "STATEMENTS", "MDOX").Trim


            .SecFlag = Me._Ini.flagToBooleanYesNo(Me._Ini.GetValueWithDefault("PDF.Sec.Flag", "STATEMENTS", "Y"))
            .SgnFlag = Me._Ini.flagToBooleanYesNo(Me._Ini.GetValueWithDefault("PDF.Sgn.Flag", "STATEMENTS", "Y"))
            '.PDFPrpFlag = me._Ini.flagToBoolean(me._Ini.GetValueWithDefault("PDF.Prp.Flag", "STATEMENTS", "Y"))


            .PdfCrtPath = Me._Ini.GetValueWithDefault("PDF.Crt.Path", "STATEMENTS", String.Empty).Trim
            .CrtPwd = Me._Ini.GetValueWithDefault("PDF.Crt.Pwd", "STATEMENTS", String.Empty).Trim

            If .SgnFlag Then
                checkIniParameter("PDF.Crt.Path", .PdfCrtPath, True, True)
                checkIniParameter("PDF.Crt.Pwd", .CrtPwd)
            End If
        End With
    End Sub


    ''' <summary>
    ''' checks the value of the parameter for true and false values
    ''' </summary>
    ''' <param name="param_"></param>
    ''' <param name="sSection"></param>
    ''' <param name="Default_"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function flagToBooleanTrueFalse(param_ As String, ByVal sSection As String, Optional Default_ As String = "F") As Boolean
        Dim sVal As String

        sVal = Me._Ini.GetValueWithDefault(param_, sSection, Default_)

        Me._paramsValues.Item(param_) = sVal

        If String.IsNullOrEmpty(sVal) OrElse sVal.Trim = String.Empty Then
            If Default_.Trim.ToUpper = "F" Or Default_.Trim.ToUpper = "FALSE" Then
                Return False
            End If
        End If

        If sVal.Trim.ToUpper = "F" Or sVal.Trim.ToUpper = "FALSE" Then
            Return False
        Else
            Return True
        End If

    End Function

    ''' <summary>
    ''' checks the value of the parameter for true and false values
    ''' </summary>
    ''' <param name="sParam"></param>
    ''' <param name="sSection"></param>
    ''' <param name="sDefualt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getBooleanParameter(ByVal sParam As String, ByVal sSection As String, ByVal sDefualt As String) As Boolean
        Dim tmp As String
        Dim bRet As Boolean = False

        tmp = Me._Ini.GetValueWithDefault(sParam, sSection, sDefualt).Trim.ToUpper
        checkIniParameter(sParam, tmp.ToString, False, False)

        If tmp.ToString = "T" OrElse tmp.ToString = "TRUE" Then
            bRet = True
        Else
            bRet = False
        End If

        Return bRet
    End Function

    ''' <summary>
    ''' checks the value of the parameter for No and Yes values
    ''' </summary>
    ''' <param name="sParam"></param>
    ''' <param name="sSection"></param>
    ''' <param name="sDefualt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getBooleanParameterYesNo(ByVal sParam As String, ByVal sSection As String, ByVal sDefualt As String) As Boolean
        Dim tmp As String
        Dim bRet As Boolean = False

        tmp = Me._Ini.GetValueWithDefault(sParam, sSection, sDefualt).Trim.ToUpper
        checkIniParameter(sParam, tmp.ToString, False, False)

        If tmp.ToString = "Y" OrElse tmp.ToString = "YES" Then
            bRet = True
        Else
            bRet = False
        End If

        Return bRet
    End Function

    ''' <summary>
    ''' checks the value of the parameter for integer values
    ''' </summary>
    ''' <param name="sParam"></param>
    ''' <param name="sSection"></param>
    ''' <param name="sDefualt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getParameterInteger(ByVal sParam As String, ByVal sSection As String, ByVal sDefualt As String) As Integer
        Dim iVal As Integer
        Dim tmpStr As String

        tmpStr = Me._Ini.GetValueWithDefault(sParam, sSection, sDefualt).Trim
        checkIniParameter(sParam, tmpStr, False, False, False)

        If Not String.IsNullOrEmpty(tmpStr) Then
            If Not Integer.TryParse(tmpStr, iVal) Then
                _errorList.Item(sParam) = String.Format("The parameter '{0}' is not an integer value.", sParam)
            End If
        End If

        Return iVal
    End Function


    ''' <summary>
    ''' if an .Ini parameter is required the its collected with its description
    ''' </summary>
    ''' <remarks></remarks>
    Sub setIniParametersInfo()
        Dim errorInfo As SortedList
        errorInfo = getIniParametersInfo()

        Try

            For Each sItem As String In errorInfo.Keys
                If _errorList.Keys.Contains(sItem) Then
                    If Not _errorList.Item(sItem) Is Nothing AndAlso Not _errorList.Item(sItem).ToString.Trim = String.Empty Then
                        _errorList.Item(sItem) = String.Format("{0}: {1} -> {2}", sItem, errorInfo.Item(sItem))
                    Else
                        _errorList.Item(sItem) = String.Format("{0}: {1} -> Data problem.", sItem, errorInfo.Item(sItem))
                    End If
                End If
            Next

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Error is getting me._Ini error list.", , ex, ShowPopUpAlert:=False)
        End Try

    End Sub

    ''' <summary>
    ''' Sets the description for each me._Ini parameter
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Function getIniParametersInfo() As SortedList
        Dim iniParametersInfo As New SortedList


        iniParametersInfo.Item("Fle.Xfr.Enabled") = "Decide if the Named ftp site is enabled or not.(True/False)"
        iniParametersInfo.Item("Ftp.Tgt.Ip") = "IP of Named ftp site"
        iniParametersInfo.Item("Ftp.Tgt.Port") = "Port of Named ftp site"
        iniParametersInfo.Item("Ftp.Log.User") = "User of Named ftp site"
        iniParametersInfo.Item("Ftp.Log.Password") = "Password of Named ftp site"
        iniParametersInfo.Item("Ftp.Tgt.Path") = "Path of the target in the Named ftp site"
        iniParametersInfo.Item("Fle.Lvl.Statistics") = "Statistics logging level of Named ftp site"
        iniParametersInfo.Item("Wrk.Pol.Sleep") = "Sleep time in seconds, before each Que pooling of Named ftp site"



        iniParametersInfo.Item("Max.Files.Per.Proc") = "Maximum files per process thread."
        iniParametersInfo.Item("Error.Retry.Count") = "Trial times after an error has occured."




        iniParametersInfo.Item("Tdx.Run.Unit") = "Unit definition"
        iniParametersInfo.Item("Log.Max.Size") = "Log file maximum size in kb after which it is switched a new one"
        iniParametersInfo.Item("Err.Rty.Count") = "Number of times the system retries on critical errors"
        iniParametersInfo.Item("Err.Rty.Sleep") = "The period in ms that the system sleeps between retries on errors"
        iniParametersInfo.Item("Alr.Eml.To") = "The email address(s) to which critical errors are notified"
        iniParametersInfo.Item("Alr.Eml.Cc") = "The email address(s) to which critical errors are copied"
        iniParametersInfo.Item("Alr.Eml.Flag") = "Controls (Y/N) if alert emails are to be sent"
        iniParametersInfo.Item("Alr.Aid.Path") = "The location where alert AIDS (email) files are placed"
        iniParametersInfo.Item("Log.Evt.Level") = "Logging level used, which may have a value from 0 to 3 (default is 1)"
        iniParametersInfo.Item("Inp.Fle.Retain") = "The retention period (in days) for input files archived in Fle.Arc.Path."
        iniParametersInfo.Item("Rpt.Fle.Retain") = "The retention period (in days) for report files saved in the log directory."

        iniParametersInfo.Item("Fle.Arc.Path") = "The directory where input files are archived after processing"
        iniParametersInfo.Item("Fle.Bad.Path") = "The directory where bad input files are archived after having errors in them."


        iniParametersInfo.Item("PDF.Title") = "Title of the PDF document (Set to 'Electronic Statement of account BBB-CCCCCC-SSS')"
        iniParametersInfo.Item("PDF.Subject") = "Subject of the PDF document (Set to 'Electronic Customer Documents')"
        iniParametersInfo.Item("PDF.Author") = "Author of the PDF document (Set to 'The Saudi Investment Bank')"
        iniParametersInfo.Item("PDF.Producer") = "Producer of the PDF document (Set to 'SAIB')"
        iniParametersInfo.Item("PDF.Application") = "Application producing the PDF document (Set to 'MDOX')"
        iniParametersInfo.Item("PDF.Crt.Path") = "Location of the certificate file to use in signing the PDF document"
        iniParametersInfo.Item("PDF.Crt.Pwd") = "The password of the certificate used for signing the PDF document"
        iniParametersInfo.Item("PDF.Sec.Flag") = "Flag (Y/N) to indicate if the PDF document is to be secured"
        iniParametersInfo.Item("PDF.Sgn.Flag") = "Flag (Y/N) to indicate if the PDF document is to be signed"
        'iniParametersInfo.Item("PDF.Prp.Flag") = "Flag (Y/N) to indicate if the PDF document properties is to be set ot not"

        iniParametersInfo.Item("Sta.Fle.Retain") = "Statistics File retain time."

        '''''

        iniParametersInfo.Item("FTP.Upload.Enabled") = "Enables or disables the Uploading of files as a hole."
        iniParametersInfo.Item("FTP.Targets") = "FTP upload connection names"
        iniParametersInfo.Item("Restart.On.Each.File") = "Makes MDOX restarts after each file has been processed."

        iniParametersInfo.Item("FTP.Timeout") = "FTP Time out for each connection."
        iniParametersInfo.Item("FTP.Keep.Alive") = "Keep the FTP Connection Alive (True/False)."
        iniParametersInfo.Item("FTP.Connection.Type.Passive") = "FTP Connection Type ( Passive/Active )."
        iniParametersInfo.Item("FTP.Cde.Typ") = "The Code used to execute the FTP functionality(Internal/Enhanced)"
        iniParametersInfo.Item("Upload.Error.Trail.Times") = "Maximum upload trial failures per file before sending an eMail about it."
        iniParametersInfo.Item("FTP.Tgt.Path.Complement") = "The Complementary Path"



        iniParametersInfo.Item("Inp.Fle.Path") = "Input files path"
        iniParametersInfo.Item("Vdx.Wrk.Path") = "Work files path"
        iniParametersInfo.Item("Out.Dir.Root") = "The root (parent) path where generated statement files are placed"
        iniParametersInfo.Item("Inp.Fle.Mask") = "The extension of the input files"

        iniParametersInfo.Item("Stm.Sub.Fld") = "The sub-folder of the output"

        iniParametersInfo.Item("DB.Provider") = "The database provider type to be used (SQLCeServer/OleDB)"

        iniParametersInfo.Item("DB.Ole.File.Path") = "The database file to be used (OLE)"
        iniParametersInfo.Item("DB.SQLCe.File.Path") = "The database file to be used (SQLServer)"
        iniParametersInfo.Item("Update.UI.Every.File") = "UI update every n files."

        iniParametersInfo.Item("Check.Drive.Info") = "Enables the check the Drive free space and the folders number of files [True/False]"
        iniParametersInfo.Item("Minimum.Drive.Size") = "Minimum drive free space size"
        iniParametersInfo.Item("Maximum.Folder.Files") = "Maximum number of files a directory can have."

        Return iniParametersInfo
    End Function

    ''' <summary>
    ''' Write the assigned values to the .Ini file.
    ''' </summary>
    ''' <param name="iniFilePath">The complete path where me._Ini file exists.</param>
    ''' <returns>Return TRUE if me._Ini written sucessfully, or return FALSE incase of failure.</returns>
    ''' <remarks>
    ''' Created : 2010-12-19 DK
    ''' Mofified: 2011-02-06 DK
    ''' </remarks>
    Public Function WriteINIFile(ByVal iniFilePath As String) As Boolean
        Try
            Dim write_Ini As New IniFileClass(iniFilePath)
            Dim i As Integer

            With INIParameters.Singlton
                With .Glob
                    write_Ini.SetValue("Log.Max.Size", .LogMaxSize.ToString, "GLOBALS")
                    write_Ini.SetValue("Err.Rty.Count", .ErrRtyCount.ToString, "GLOBALS")
                    write_Ini.SetValue("Err.Rty.Sleep", .ErrRtySleep.ToString, "GLOBALS")
                    write_Ini.SetValue("Alr.Eml.To", .AlrEmlTo, "GLOBALS")
                    write_Ini.SetValue("Alr.Eml.Cc", .AlrEmlCc, "GLOBALS")
                    write_Ini.SetValue("Alr.Eml.Flag", .AlrEmlFlag, "GLOBALS")
                    write_Ini.SetValue("Alr.Aid.Path", .AlrAidPath, "GLOBALS")
                    write_Ini.SetValue("Log.Evt.Level", .LogEvtLevel.ToString, "GLOBALS")
                    write_Ini.SetValue("Inp.Fle.Retain", .InpFleRetain.ToString, "GLOBALS")
                    write_Ini.SetValue("Rpt.Fle.Retain", .RptFleRetain.ToString, "GLOBALS")
                End With

                With .Stm
                    write_Ini.SetValue("Inp.Fle.Path", .InpFlePath, "STATEMENTS")
                    write_Ini.SetValue("Out.Dir.Root", .OutDirRoot, "STATEMENTS")
                    write_Ini.SetValue("Fle.Arc.Path", .FilArcPath, "STATEMENTS")
                    write_Ini.SetValue("Fle.Bad.Path", .FleBadPath, "STATEMENTS")
                    write_Ini.SetValue("Tmp.Path", .TmpPath, "STATEMENTS")

                    With .PDF
                        write_Ini.SetValue("PDF.Title", .Title, "STATEMENTS")
                        write_Ini.SetValue("PDF.Subject", .Subject, "STATEMENTS")
                        write_Ini.SetValue("PDF.Author", .Author, "STATEMENTS")
                        write_Ini.SetValue("PDF.Producer", .Producer, "STATEMENTS")
                        write_Ini.SetValue("PDF.Application", .Application, "STATEMENTS")
                        write_Ini.SetValue("PDF.Crt.Path", .PdfCrtPath, "STATEMENTS")
                        write_Ini.SetValue("PDF.Crt.Pwd", .CrtPwd, "STATEMENTS")
                        write_Ini.SetValue("PDF.Sec.Flag", IIf(.SecFlag, "Y", "N").ToString, "STATEMENTS")
                        write_Ini.SetValue("PDF.Sgn.Flag", IIf(.SgnFlag, "Y", "N").ToString, "STATEMENTS")
                    End With
                End With
            End With

            write_Ini = Nothing
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "write_Ini has been updated successfully.")
            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Failed to update the write_Ini file.", , ex, ShowPopUpAlert:=True)
            Return False
        End Try
    End Function

#End Region
End Class

