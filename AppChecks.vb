Imports System.IO
Imports sib = SIBL0100
Imports CRP0100
Imports SIBL0100.Util
Imports VDXExtractDB
Imports ServicesUti
Imports Microsoft.Win32

''' <summary>
''' Application main Checks
''' It will be doing checks on the .ini parameters in its own worker thread, by checking existence of files, existence of folders, correctness of values(Boolean, integers, strings).
''' It will check the main database existence, and connectivity, and the template database also.
''' It will check for the number of files in some folders, so they don’t exceed windows limits, and check if the current hard disk has enough free space.
''' 
''' It will check for the existence of the quickpdf .dlls, and crystal reports registry keys and that its installed.
''' It will extract resources of the .rpt templates for .pdf statements.
''' It will check for the FTP connections, and their connectivity and the existence of the main folders into them.
''' </summary>
''' <remarks></remarks>
Public Class AppChecks


    Private _text As String 'UI header text

    Private _sDBProvider As String 'DB Provider
    Private _sDBOleFilePath As String 'Ole DB Provider
    'Private _sDBSQLCeFilePath As String 'SQL Ce DB Provider
    Private _sDBSQLConnectionString As String 'SQL Server DB Provider

    Public Sub New(text_ As String)
        _text = text_
    End Sub

    ''' <summary>
    ''' Set the properties for the app checks
    ''' </summary>
    ''' <param name="iniDBProvider_"></param>
    ''' <remarks></remarks>
    Public Sub setProperties(iniDBProvider_ As String, sDBSQLConnectionString_ As String, sDBOleFilePath_ As String)
        Me._sDBProvider = iniDBProvider_.ToUpper
        Me._sDBSQLConnectionString = sDBSQLConnectionString_
        Me._sDBOleFilePath = sDBOleFilePath_
    End Sub

    ''' <summary>
    ''' Start the checks
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function checks() As Boolean
        Dim isEnableExecution As Boolean = True
        Dim dStart As DateTime


        Try

            Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)

            'Check .ini existence
            If Not File.Exists(ServicesUti.AppPaths.singleton.AppIni) Then
                'ServicesUti.Services.Singleton.AppInstance.HandleError(0, "Couldn't find the configuration file (TDXF0100.ini) in the application directory. Can’t continue further, kindly verify installation.", "configuration file (MDXF0100.ini)")
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Couldn't find the configuration file (TDXF0100.ini) in the application directory. Can’t continue further, kindly verify installation.", ShowPopUpAlert:=True)
                isEnableExecution = False
                Return isEnableExecution
            Else
                'Read ini
                If Not INIParameters.Singlton.ReadINIFile(ServicesUti.AppPaths.singleton.AppIni) = True Then
                    'ServicesUti.Services.Singleton.AppInstance.HandleError(0, "There are errors in the configuration file (TDXF0100.ini). Can’t continue further, kindly verify installation.", "configuration file (MDXF0100.ini)")
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "There are errors in the configuration file (TDXF0100.ini). Can’t continue further, kindly verify installation. Fix and Rerun application.", ShowPopUpAlert:=True)
                    'Exit Sub
                    isEnableExecution = False
                    Return isEnableExecution
                End If
            End If


            setProperties(INIParameters.Singlton.Glob.DBProvider, INIParameters.Singlton.Glob.DBExtractSQLConnectionstring, INIParameters.Singlton.Glob.DBOleFilePath)
            'initialize the msgbox props
            SIBL0100.UMsgBox.Singleton.init(INIParameters.Singlton.Glob.LogEvtLevel, frmMain.Singleton, INIParameters.Singlton.Stm.WorkDirectory, INIParameters.Singlton.Glob.AlrAidPath, INIParameters.Singlton.Glob.AlrEmlTo, INIParameters.Singlton.Glob.AlrEmlCc)

            'set the quickPdf .dll path
            If pdfLib.QuickPDF.dllFolderPath Is Nothing Then
                pdfLib.QuickPDF.dllFolderPath = Application.StartupPath & "\Resources"
            End If

            'Extract .rpt template resources for pdf statements
            dStart = Now
            extractResources()
            Logger.LoggerClass.Singleton.LogInfo(0, "extractResources -> TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)

            'Check that crystal reports is installed
            If Not sib.Utils.checkAssemblyInGAC("CrystalDecisions.ReportSource.dll") Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "It looks like you did not install Crystal Reports, please install it first then re-run the application.", ShowPopUpAlert:=True)
                isEnableExecution = False
            Else
                'Checks the registry keys for crystal reports
                If Not isCR_ExportToPDF() Then
                    isEnableExecution = False
                End If
            End If

            'If Not isQuickPdfInstalled() Then
            '    isEnableExecution = False
            'End If


            Dim sFile As String
            sFile = pdfLib.QuickPDF.isDllFound() 'check if the dll is found

            If Not String.IsNullOrEmpty(sFile) Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Error could not find the QuickPDF dll file(s): {0}.", vbCrLf & sFile), ShowPopUpAlert:=True)
                isEnableExecution = False
                Return isEnableExecution
            End If


            'Check DB props
            dStart = Now
            checkDB(isEnableExecution)
            Logger.LoggerClass.Singleton.LogInfo(0, "applicationCheckDB -> TimeSpan: " & UDate.getTimeDiffrence(dStart), 6)
            If Not isEnableExecution Then
                Return isEnableExecution
            End If
            'Check drive settings
            If INIParameters.Singlton.Glob.CheckDriveInfo Then
                checkFolderSizes(isEnableExecution)
                If Not isEnableExecution Then
                    Return isEnableExecution
                End If
            End If

            Dim sBuilder As New System.Text.StringBuilder("")
            For Each oKeyVal As System.Collections.Generic.KeyValuePair(Of String, String) In INIParameters.Singlton.ParamsValues
                Dim sVal As String
                sVal = oKeyVal.Value
                If String.IsNullOrEmpty(sVal) Then
                    sVal = ""
                End If
                sBuilder.Append(String.Format(", {0}={1}", oKeyVal.Key, sVal))
            Next
            Dim sParams As String
            sParams = sBuilder.ToString
            If sParams.Length > 0 Then
                Logger.LoggerClass.Singleton.LogInfo(0, "Read Params:", 6)
                Logger.LoggerClass.Singleton.LogInfo(0, sParams.Substring(1), 6)
            End If

        Catch ex As System.Threading.ThreadAbortException
            'sunk
        Catch ex As Exception
            ServicesUti.Services.Singleton.AppInstance.ModalMessageBox(String.Format("Error in extractResources.{0}{1}", vbCrLf, ex.Message), MessageBoxButtons.OK, , MessageBoxIcon.Error, Me._text)
        End Try

        Return isEnableExecution
    End Function

    ''' <summary>
    ''' Check the folder empty space size
    ''' </summary>
    ''' <param name="isEnableExecution"></param>
    ''' <remarks></remarks>
    Sub checkFolderSizes(ByRef isEnableExecution As Boolean)
        Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)

        If isEnableExecution Then
            Dim sTmpDrive As String
            Dim iDriveSizeGB As Long

            'check HDD empty space
            sTmpDrive = IO.Path.GetPathRoot(INIParameters.Singlton.Stm.FilArcPath).ToLower 'Get the Archive drive

            iDriveSizeGB = sib.UFolder.getDriveFreeSpace(sTmpDrive)
            iDriveSizeGB = iDriveSizeGB \ 1000000000

            If iDriveSizeGB <= INIParameters.Singlton.Glob.MinimumDriveSize Then
                isEnableExecution = False
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Drive {0} is almost full, please change drive or clear it.", sTmpDrive), ShowPopUpAlert:=True)
                Return
            End If

            'check folder max files
            checkFolderMaximumFiles(isEnableExecution, INIParameters.Singlton.Stm.FilArcPath)
            checkFolderMaximumFiles(isEnableExecution, INIParameters.Singlton.Glob.AlrAidPath)
            checkFolderMaximumFiles(isEnableExecution, AppPaths.singleton.LogFilePath)
            checkFolderMaximumFiles(isEnableExecution, INIParameters.Singlton.Stm.FleBadPath)
        End If
    End Sub

    ''' <summary>
    ''' Checks if the folder has reached its maximum number of files
    ''' </summary>
    ''' <param name="isEnableExecution"></param>
    ''' <param name="sPath_"></param>
    ''' <remarks></remarks>
    Private Shared Sub checkFolderMaximumFiles(ByRef isEnableExecution As Boolean, ByVal sPath_ As String)
        Dim iArchiveFolderFiles As Long
        iArchiveFolderFiles = sib.UFolder.getFileCount(sPath_, "*.*", SearchOption.TopDirectoryOnly)

        If iArchiveFolderFiles >= INIParameters.Singlton.Glob.MaximumFolderFiles Then
            isEnableExecution = False
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Folder {0} has {1} files in it, and this will affect the system, please fix.", sPath_, iArchiveFolderFiles), ShowPopUpAlert:=True)
            Return
        End If
    End Sub

    ''' <summary>
    ''' Extracts the .rpt templates for use into the creation of pdf reports.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub extractResources()

        Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)

        Const SummaryTemplateName As String = "VDXT0300.rpt"

        Dim sRptName As String
        Dim sTemplatePath As String = INIParameters.Singlton.Stm.TmpPath


        For iInd As Integer = 0 To 0
            sRptName = String.Format(POSStatementFiles.C_TCSTReportTemplateName, "AR", iInd.ToString, "01")
            extractInternalResource(sRptName, IO.Path.Combine(sTemplatePath, sRptName))
            sRptName = String.Format(POSStatementFiles.C_SCSTReportTemplateName, "AR", iInd.ToString, "01")
            extractInternalResource(sRptName, IO.Path.Combine(sTemplatePath, sRptName))
        Next
        'VDXF0110.STMT.GENSTM.AR.0.01.WICN
        'En, WICN
        For iInd As Integer = 0 To 0
            sRptName = String.Format(POSStatementFiles.C_TCSTReportTemplateName, "EN", iInd.ToString, "01")
            extractInternalResource(sRptName, IO.Path.Combine(sTemplatePath, sRptName))
            sRptName = String.Format(POSStatementFiles.C_SCSTReportTemplateName, "EN", iInd.ToString, "01")
            extractInternalResource(sRptName, IO.Path.Combine(sTemplatePath, sRptName))
        Next

        'extractInternalResource(INIParameters.Statement.TmpPgs, IO.Path.Combine(sTemplatePath, INIParameters.Statement.TmpPgs))
        extractInternalResource(SummaryTemplateName, IO.Path.Combine(sTemplatePath, SummaryTemplateName))
    End Sub

    Private Function ReadRegistry(pkey As String, psubkey As String) As String
        Dim KeyValue As String = ""
        Dim baseKey As RegistryKey
        Dim regkey As RegistryKey

        baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, baseKey.ToString, String.Empty, , LogLevel:=1)
        regkey = baseKey.OpenSubKey(pkey)
        If regkey IsNot Nothing Then KeyValue = CStr(regkey.GetValue(psubkey))

        Return KeyValue
    End Function
    ''' <summary>
    ''' Checks for the existence of specific registry keys for the work of crystal reports
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function isCR_ExportToPDF() As Boolean
        ' From the bellow links, to make Crystal Reports v13 or CRVS2010 for VS 2010 that exports correct pdf font size:
        ' Add two DWORD keys and set their values as 1:
        ' Collapse | Copy Code
        ' HKEY_CURRENT_USER\Software\SAP BusinessObjects\Crystal Reports for .NET Framework 4.0\Crystal Reports\Export\PDF\ForceLargerFonts
        ' HKEY_LOCAL_MACHINE\SOFTWARE\SAP BusinessObjects\Crystal Reports for .NET Framework 4.0\Crystal Reports\Export\Pdf\ForceLargerFonts
        ' http://www.idautomation.com/kb/exporting_to_pdf.html[^]
        ' and apply what mr "Ludek Uher" said:
        ' http://forums.sdn.sap.com/thread.jspa?threadID=1943543&tstart=45#10218949[^]
        ' and apply also what mr "Ken Low" said:
        ' http://forums.sdn.sap.com/thread.jspa?threadID=1933131[^]


        Dim sKey As String = String.Empty
        Dim sVal As String = String.Empty
        Dim sKeysArr(1) As String
        Dim iInd As Integer
        Dim sSubKey As String = String.Empty
        Dim bResult As Boolean = True
        Dim sMsgError As String = String.Empty

        Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)
        Try
            sKeysArr(0) = INIParameters.Singlton.Glob.RPT_BAR_CDE_FNT_SZE_1
            sKeysArr(1) = INIParameters.Singlton.Glob.RPT_BAR_CDE_FNT_SZE_2
            'check Install
            For iInd = 0 To sKeysArr.Length - 1
                If String.IsNullOrEmpty(sKeysArr(iInd)) Then
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, String.Format("Registry key not set in ini: RPT.BAR.CDE.FNT.SZE"), String.Empty, , LogLevel:=1, ShowPopUpAlert:=True)
                    Continue For
                End If
                sKey = sKeysArr(iInd).Trim
                sSubKey = sKey.Substring(sKey.LastIndexOf("\") + 1)
                sKey = sKey.Substring(0, sKey.LastIndexOf("\"))
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, sKey, String.Empty)
               
                If iInd = 1 Then
#If PLATFORM = "x86" Then
                    If Environment.Is64BitOperatingSystem Then
                        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "64", "Application run on 64 Bit machine", , LogLevel:=1)
                        sVal = ReadRegistry("SOFTWARE\SAP BusinessObjects\Crystal Reports for .NET Framework 4.0\Crystal Reports\Export\Pdf", "ForceLargerFonts")
                    Else
                        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "32", "Application run on 32 Bit machine", , LogLevel:=1)
                        sVal = CStr(My.Computer.Registry.GetValue(sKey, sSubKey, String.Empty))
                    End If

#Else
                sVal = CStr(My.Computer.Registry.GetValue(sKey, sSubKey, String.Empty))
#End If

                Else
                    sVal = CStr(My.Computer.Registry.GetValue(sKey, sSubKey, String.Empty))
                End If


                If String.IsNullOrEmpty(sVal) Then
                    sMsgError &= String.Format(iInd + 1 & ". Registry key not found, or could not be read: {0}\{1}", sKey, sSubKey) & vbCrLf & vbCrLf
                    bResult = False
                    Continue For
                End If

                If sVal <> "1" Then
                    sMsgError &= String.Format(iInd + 1 & ". Registry key does not equal 1: {0}\{1}", sKey, sSubKey) & vbCrLf
                    bResult = False
                    Continue For
                End If
            Next

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, String.Format("Error: Registry key not found: {0}\{1}", sKey, sSubKey), String.Empty, ex, LogLevel:=1, ShowPopUpAlert:=True)
            bResult = False
        End Try

        If bResult = False Then
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, String.Format("Errors in Registry key(s):" & vbCrLf & vbCrLf & "{0}", sMsgError), String.Empty, , LogLevel:=1, ShowPopUpAlert:=True)
        End If
        '#If DEBUG Then
        '        bResult = True
        '#End If
        Return bResult

    End Function

    ''' <summary>
    ''' Checks if quick pdf is installed
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function isQuickPdfInstalled() As Boolean
        Dim sKey As String = String.Empty
        Dim sVal As String = String.Empty
        Const C_QuickPdfInstalled As String = "HKEY_CLASSES_ROOT\QuickPDFAX.PDFLibrary\Clsid" '
        Dim sKeysArr(0) As String
        Dim iInd As Integer
        Dim bResult As Boolean = True

        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)
            sKeysArr(0) = C_QuickPdfInstalled
            'check Install
            For iInd = 0 To sKeysArr.Length - 1
                sKey = sKeysArr(iInd)

                'sVal = RegKeyMainNode.GetValue(Nothing).ToString
                sVal = My.Computer.Registry.GetValue(sKey, Nothing, String.Empty).ToString

                If sVal Is Nothing OrElse sVal.Trim = String.Empty Then
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "QuickPdf is not installed, because Registry key not found: " & sKey, String.Empty, , LogLevel:=1, ShowPopUpAlert:=True)
                    bResult = False
                End If
            Next

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "QuickPdf is not installed, because Registry key not found: " & sKey, String.Empty, ex, LogLevel:=1, ShowPopUpAlert:=True)
            bResult = False
        End Try

        Return bResult

    End Function

    ''' <summary>
    ''' Checks the existence of the DB files, and the possiblity to connect to them, and that they are not read only
    ''' </summary>
    ''' <param name="isEnableExecution"></param>
    ''' <remarks></remarks>
    Public Sub checkDB(ByRef isEnableExecution As Boolean)


        Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)
        If _sDBProvider = DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.SQLCeServer).ToUpper Then
            DBProvider.singleton.DBType = DBProvider.enumDBProviderType.SQLCeServer
        ElseIf _sDBProvider = DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.OleDB).ToUpper Then
            DBProvider.singleton.DBType = DBProvider.enumDBProviderType.OleDB
        ElseIf _sDBProvider = DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.SQLServer).ToUpper Then
            DBProvider.singleton.DBType = DBProvider.enumDBProviderType.SQLServer
        Else
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Error you have two options for paramater 'DB.Provider' and they are({0}/{1})." _
                        , DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.SQLCeServer) _
                        , DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.OleDB) _
                         ), ShowPopUpAlert:=True)
            isEnableExecution = False

        End If

        'Logger.LoggerClass.Singleton.LogInfo(0, "Started.", 6)
        ' If _sDBProvider = DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.SQLCeServer).ToUpper Then
        '    DBProvider.singleton.DBType = DBProvider.enumDBProviderType.SQLCeServer
        'ElseIf _sDBProvider = DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.OleDB).ToUpper Then
        '    DBProvider.singleton.DBType = DBProvider.enumDBProviderType.OleDB
        'ElseIf _sDBProvider = DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.SQLServer).ToUpper Then
        '    DBProvider.singleton.DBType = DBProvider.enumDBProviderType.SQLServer
        'Else
        '    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Error you have two options for paramater 'DB.Provider' and they are({0}/{1})." _
        '                , DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.SQLCeServer) _
        '                , DBProvider.enumDBProviderType.GetName(GetType(DBProvider.enumDBProviderType), DBProvider.enumDBProviderType.OleDB) _
        '                 ), ShowPopUpAlert:=True)
        '    isEnableExecution = False

        'End If

        If Not IO.File.Exists(_sDBOleFilePath) Then
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Error could not find local db file [{0}] for mdb.", _sDBOleFilePath), ShowPopUpAlert:=True)
            isEnableExecution = False
        End If

        If isEnableExecution Then

            DBProvider.singleton.init(INIParameters.Singlton.Glob.DBExtractSQLConnectionstring, PublicVars.C_DB_Version)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("The database provider to be used is [{0}].", _sDBProvider), ShowPopUpAlert:=False)

            If Not IO.File.Exists(PublicVars.getDBTemplatePath()) Then
                isEnableExecution = False
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Error could not find template DB {0}.", PublicVars.getDBTemplatePath()), ShowPopUpAlert:=True)
            End If
        End If

    End Sub


End Class
