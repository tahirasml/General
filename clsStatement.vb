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
''' <remarks>
''' </remarks>
Public Class clsStatement
    Implements IDisposable

#Region " Variables & Constants "

    'Private m_DataSet As New DataSetStm 'Database of the loaded statement

    Private _StatementStatus As frmFileErrorB.enumFileErrorUserSelection = frmFileErrorB.enumFileErrorUserSelection.Normal

    'Files
    Private _fileNo As Integer = 0 'current file number
    Private m_FilePath As String = String.Empty ' input data file path
    Private m_fileName As String = String.Empty ' input data file name
    Private m_WorkFilePath As String = String.Empty 'the path of the input file after copying to the work folder
    Private m_WorkFileShadowPath As String = String.Empty 'the path of the shadow file
    Private m_OutputFileName As String = String.Empty 'the name of the output file
    

    Private m_FileStream As StreamReader = Nothing
    Private _fileQue As FileQueue 'file que reader


    'Stats
    Private m_StatementRecords() As String 'the loaded statement records from the data file
    Private m_NextRecordSequence As Integer = 0 'The record internal sequence from the statement
    'Private m_totalNumberOfStatements As Integer = 0 'The total number of statements
    Private m_TotalCustomers As Integer = 0 'The number of customers in the file
    Private WithEvents m_statementFiles As StatementFiles

    Private m_Report_SheetSequence As Integer = 0 'Report sheet sequence -> this is used for generating the barcode number
    Private m_Report_MatchSequence As Integer = 0 'Report number  -> this is used for generating the barcode number
    Private m_Report_TotalPages As Integer = 0 'Report number  -> this is used for generating the barcode number

    Private m_DBExecutionNo As Integer = 0 'database file execution number in order to resume processing the file with same execution number
    Private m_statementStartTime As Date 'The time each statement has started processing

    Private m_CurrentRecordNumber As Integer = 0 'The number of this statement in the file
    Private m_BeginingLineOfCurrentStatement As Integer = 0 'the first line of the Statement thats going to be processed
    Private m_FileTotalLines As Integer = -1 'The total lines of the file, which is incremented each time some lines are read
    Private m_FileTotalPages As Integer = 0 'file pages number
    Private _CurrentStatementLines As Integer = 0 'number of lines of current Statement


    Private m_isResumingFile As Boolean = False 'determin if the current file is being resumed from a previous processing operation
    Private m_fileProcessStartDate As Date 'the processing date of the file which might have been dated before in an aborted process
    Private m_fileProcessEndDate As Date 'the processing date of the file which might have been dated before in an aborted process

    Private m_isMovedToWorkDir As Boolean = False 'Set when the file has moved to the work folder


    '' Statement Structure
    Private m_Statement As Structures.StmFil_Struct
    Private m_CusHostRecord As Structures.CusStm_Struct

    Private m_StatementSummary As New StatementSummary

    Private Const C_NumberOfRowsToGC_Collect As Integer = 2000 'Maximum Number of rows before calling the GC.collect
    Private _currentNumberOfRows_GC_Collect As Integer = 0 'Current number of rows before calling the GC.collect

    Private _InputFilePath As String 'input file path
    Private _WorkDirectoryPath As String 'work path
    Private _ArchiveDirectoryPath As String 'archive path
    Private _BadFilesDirectoryPath As String 'bad files path
    Private _inputFileExtension As String 'input file extension
    Private lockObject As New Object
    'Public Event InputFileChanged As SIBL0100.ProgressTrack.CountChangedEventHandler

    Public Enum enumFileState
        OK
        Skipped
        Failed
    End Enum

    'Record types:
    Private Const C_Record_FileFooter As String = "NrRecords"
    Private Const C_Record_StatementDocType As String = "PP STMT Data"

    Public Const C_TempFileExt As String = ".~tmp"
    Private Const C_FileEncoding As Integer = 1256

#End Region

#Region " Properties "

    Public Property isExport As Boolean = True 'allow exporting
    Public Property OutDirRoot_Unit As String = String.Empty  'allow exporting
    Public Property Records_CurrentItemNum() As Integer 'Return the current Array Item Number.
    Public Property FileState As enumFileState = enumFileState.OK ' the state of the file

    Public ReadOnly Property FileNo As Integer
        Get
            Return _fileNo
        End Get
    End Property

    Public Property StatmentStatus As frmFileErrorB.enumFileErrorUserSelection
        Get
            Return _StatementStatus
        End Get
        Set(value As frmFileErrorB.enumFileErrorUserSelection)
            _StatementStatus = value
        End Set
    End Property

    Public ReadOnly Property FileTotalPages As Integer
        Get
            Return m_FileTotalPages
        End Get
    End Property

    Public Property FilePath() As String
        Get
            Return m_FilePath
        End Get
        Set(ByVal Value As String)
            m_FilePath = Value
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
    Public ReadOnly Property WorkFilePath() As String
        Get
            Return m_WorkFilePath
        End Get
    End Property


    Public ReadOnly Property fileName() As String
        Get
            Return m_fileName
        End Get
    End Property

    Public ReadOnly Property FileNameDisplay() As String
        Get
            Return String.Format("[{0}]", fileName)
        End Get
    End Property

    ''' <summary>
    ''' Return the total number of statements avaiable in the statement file.
    ''' </summary>
    ''' <value>Long</value>
    ''' <remarks>
    ''' </remarks>
    Public ReadOnly Property TotalCustomers() As Integer
        Get
            Return m_TotalCustomers
        End Get
    End Property

    ''' <summary>
    ''' Return the total number of lines in statement file.
    ''' </summary>
    ''' <value>Long</value>
    ''' <remarks>
    ''' </remarks>
    Public ReadOnly Property FileTotalLines() As Integer
        Get
            Return m_FileTotalLines
        End Get
    End Property

    ''' <summary>
    ''' Return the current number of statement being process.
    ''' </summary>
    ''' <value>Long</value>
    ''' <remarks>
    ''' </remarks>
    Public ReadOnly Property CurrentStatement() As Integer
        Get
            Return m_CurrentRecordNumber
        End Get
    End Property


    ''' <summary>
    ''' the reached line in the file
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property fileReachedLine() As Integer
        Get
            If Me.m_StatementRecords Is Nothing Then
                Return Me.m_FileTotalLines + Me.Records_CurrentItemNum + 1
            Else
                Return Me.m_FileTotalLines - Me.m_StatementRecords.Length + Me.Records_CurrentItemNum + 1
            End If
        End Get
    End Property

    Public ReadOnly Property IsResumingFile() As Boolean
        Get
            Return m_isResumingFile
        End Get
    End Property
#End Region

    Public Sub New(statementFiles_ As StatementFiles, InputFilePath_ As String, WorkDirectoryPath_ As String, ArchiveDirectoryPath_ As String, BadFilesDirectoryPath_ As String _
                   , inputFileExtension_ As String)

        m_statementFiles = statementFiles_

        Me._InputFilePath = InputFilePath_
        Me._WorkDirectoryPath = WorkDirectoryPath_
        Me._ArchiveDirectoryPath = ArchiveDirectoryPath_
        Me._BadFilesDirectoryPath = BadFilesDirectoryPath_
        Me._inputFileExtension = inputFileExtension_
    End Sub

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

    ''' <summary>
    ''' Updates the statement count in the UI
    ''' </summary>
    ''' <param name="status_"></param>
    ''' <remarks></remarks>
    Private Sub Grid_UpdateRow(Optional ByVal status_ As String = "")
        Try
            Me.m_statementFiles.Grid_UpdateRow(status_)
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Creates an entry for the file in the UI
    ''' </summary>
    ''' <param name="image_index"></param>
    ''' <param name="item_title"></param>
    ''' <param name="subitem_titles"></param>
    ''' <remarks></remarks>
    Public Sub Grid_MakeNewRow(ByVal image_index As Integer, ByVal item_title As String, ByVal subitem_titles() As String)
        Me.m_statementFiles.Grid_MakeNewRow(image_index, item_title, subitem_titles)
    End Sub

#End Region

#Region " File Handler "

    ''' <summary>
    ''' Reads the file line by line till a statement ending is found and then it saves them in the array StatementRecords starting from the last line reached
    ''' </summary>
    ''' <returns>True is successful</returns>
    ''' <remarks></remarks>
    Function readRecords() As Boolean
        Dim sArr As New Collections.ArrayList
        Dim sLine As String = Nothing
        Try

            Me.m_StatementRecords = Nothing
            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: readRecords."), 9)

            Do
                If _fileQue.isFinished Then
                    _fileQue.fill()
                End If

                sLine = _fileQue.getLine()
                'm_FileCurrentRecord += 1
                sArr.Add(sLine)

            Loop While Not sLine Is Nothing AndAlso Not sLine = String.Empty AndAlso sLine.Substring(0, 9) <> C_Record_FileFooter 'Exit loop on end of file or end of Statement

            'Remove all last empty lines
            While sArr.Count > 0 AndAlso (sArr.Item(sArr.Count - 1) Is Nothing OrElse sArr.Item(sArr.Count - 1).ToString.Trim = String.Empty)
                sArr.RemoveAt(sArr.Count - 1)
            End While
            'sArr.RemoveAt(0)
            'sArr.RemoveAt(sArr.Count - 1)

            Me.m_StatementRecords = DirectCast(sArr.ToArray(GetType(String)), String())
            If Me.m_BeginingLineOfCurrentStatement < Me.m_FileTotalLines Then
                Me.m_BeginingLineOfCurrentStatement = Me.m_FileTotalLines
            End If

            'For Each sRecord As String In Me.m_StatementRecords
            '    If sRecord.Length < 887 Then
            '        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("{0}, This record is not as per the required record length.", sRecord))
            '        Return False
            '    End If
            'Next
            Me.m_FileTotalLines += Me.m_StatementRecords.Length
            _currentNumberOfRows_GC_Collect += Me.m_StatementRecords.Length

            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("[{0}] Could not read file.", WorkFilePath), "Could not read a Statement to memory.", LogLevel:=1, GenerateAIDS:=False)
        Finally
            'Clean memory from old read statement
            sArr.Clear()
            sArr = Nothing

        End Try
        Return False
    End Function

    ''' <summary>
    ''' Reads the file line by line till a statement ending is found and then it saves them in the array StatementRecords starting from the last line reached in the previous aborted operation
    ''' </summary>
    ''' <returns>True is successful</returns>
    ''' <remarks></remarks>
    Function readRecordsResumed() As Boolean
        Dim sArr As New Collections.ArrayList
        Try
            If Not m_FileStream Is Nothing Then
                m_FileStream.Close() 'Close File and start from begining
                m_FileStream.Dispose()
            End If

            m_FileStream = New StreamReader(WorkFilePath, Encoding.GetEncoding(C_FileEncoding))
            _fileQue = New FileQueue(m_FileStream)

            m_FileTotalLines = 0

            Me.m_StatementRecords = Nothing

            'Moving cursor to begining of the last Statement
            _fileQue.discardLines(Me.m_BeginingLineOfCurrentStatement) '- 1

            Me.m_FileTotalLines = Me.m_BeginingLineOfCurrentStatement '- 1
            readRecords()
            If Me.m_BeginingLineOfCurrentStatement = 0 Then
                Me.Records_CurrentItemNum = 1 'Start after the [file header] if the file is being processed from the begining
            End If
            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "[" & WorkFilePath & "] Could not read file.", "Could not read a Statement to memory in getStatementRecordsResumed.", LogLevel:=1, GenerateAIDS:=False)
            Return False
        Finally
            sArr.Clear()
            sArr = Nothing

        End Try
        Return False
    End Function

    ''' <summary>
    ''' Read and return the record from the m_StatementRecords, with the increament of 1.
    ''' </summary>
    ''' <returns>Return the record string, after triming extra spaces.</returns>
    ''' <remarks>
    ''' </remarks>
    Private Function GetRecord() As String
        Try
            Dim lsRec As String

            lsRec = Me.m_StatementRecords(Records_CurrentItemNum)
            Records_CurrentItemNum = Records_CurrentItemNum + 1
            'UpdateFileLinesCount()
            Return lsRec
            'If ValidateRecord() = 0 Then

            'End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0} An error has occurred while retriving record.", fileReachedLine), , ex, FileName:=fileName)
        End Try

        m_StatementSummary.EndTime = Now
        Return String.Empty
    End Function
#End Region

#Region " Validation Function "

    ''' <summary>
    ''' Check that the record type is valid and the line sequence numbering.
    ''' </summary>
    ''' <returns>Return 0 for No Error, 1 for invalid Record Type and 2 for invalid Record Sequence</returns>
    ''' <remarks></remarks>
    Private Function ValidateRecord() As Integer

        Dim liRlt As Integer
        Try

            m_NextRecordSequence = m_NextRecordSequence + 1

            liRlt = m_NextRecordSequence
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0} An Unexpected error has occurred, while validating record.", fileReachedLine), , ex, FileName:=fileName)
            liRlt = -1
        End Try

        Return liRlt
    End Function

    ''' <summary>
    '''  Check that the customer number,name, Name and address details are found 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function checkStatementHeaderFooter() As Boolean

        Dim sStat As String = "Record #" & Me.CurrentStatement.ToString("##,#0")

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: checking header and footer of " & sStat), 9)

            'For Each sRecord As String In Me.m_StatementRecords
            '    If sRecord.Length <> 637 Then
            '        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("{0}, This record is not as per the required record length.", sStat))
            '        Return False
            '    End If
            'Next

            '---------------------------------
            'Customer Data availability

            Dim sCheckDataAvailability As String = String.Empty


            If m_CusHostRecord.SHD.CusNum.Trim = String.Empty Then
                sCheckDataAvailability = ", Customer Number"
            End If

            If m_CusHostRecord.SHD.StatementNumber.Trim = String.Empty Then
                sCheckDataAvailability &= ", Statement Number"
            End If

            If m_CusHostRecord.SHD.NoOfTrvWal.Trim = String.Empty Then
                sCheckDataAvailability &= ", Number of Travel Card Wallets "
            End If

            If m_CusHostRecord.SHD.NoOfShpWal.Trim = String.Empty Then
                sCheckDataAvailability &= ", Number of Shopping Card Wallets "
            End If

            If sCheckDataAvailability <> String.Empty Then
                sCheckDataAvailability = sCheckDataAvailability.Substring(1)
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, sStat & ", This statement Header record does not have data in field(s) (" & m_CusHostRecord.SHD.CusNum & ")")
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, sStat & ", This statement Header record does not have data in field(s) (" & sCheckDataAvailability & ")")
                Return False
            End If

           

            Return True

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An exception occurred while processing statement #" & Me.CurrentStatement.ToString("##,#0"))
        End Try
        Return False
    End Function

#End Region

#Region " General Functions "

    ''' <summary>
    ''' The function will validate the filename according to the standard. UUUYMMDD.TRS 
    ''' </summary>
    ''' <returns>If the filename is valid then return TRUE else FALSE.</returns>
    ''' <remarks>
    ''' </remarks>
    Public Function ValidateFileName() As Boolean
        Dim isOk As Boolean = False
        Dim sTmpFileName As String = String.Empty
        Dim tmpFileName As String = String.Empty
        Dim C_DocSpec As String = String.Format("Check the correct naming 'UUUYMMDD.{0}' found in Document AP2063.", Me._inputFileExtension)

        Try
            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: validating file name " & Me.fileName), 9)
            sTmpFileName = Me.fileName.ToLower
            If sTmpFileName.Substring(sTmpFileName.Length - 4) <> Me._inputFileExtension Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, String.Format("{0} skipped because of bad file name, its extension is not '{1}'.{2}", FileNameDisplay, Me._inputFileExtension, C_DocSpec), , LogLevel:=1)
                Return False
            End If

            If sTmpFileName.Length <> 12 Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " skipped because of bad file name, its length does not equal 30 characters." & C_DocSpec, , LogLevel:=3)
                Return False
            End If

            tmpFileName = sTmpFileName.Replace(Me._inputFileExtension, String.Empty)

            'UUUYMMDD.TRS

            If sTmpFileName.Substring(0, 3).ToLower <> ServicesUti.Services.Singleton.AppInstance.Unit.ToLower Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " skipped because of bad file name, it does not start with 'unit'." & C_DocSpec, , LogLevel:=3)
                Return False
            End If

            If Not IsNumeric(sTmpFileName.Substring(3, 1)) Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " skipped because of bad file name, its first part is not 'Y'." & C_DocSpec, , LogLevel:=3)
                Return False
            End If

            If Not IsNumeric(sTmpFileName.Substring(4, 2)) Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " skipped because of bad file name, its second part is not 'MM'." & C_DocSpec, , LogLevel:=3)
                Return False
            End If


            If Not IsNumeric(sTmpFileName.Substring(6, 2)) Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " skipped because of bad file name, its third part is not 'DD'." & C_DocSpec, , LogLevel:=3)
                Return False
            End If

            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("An error occurred while validating file naming [{0}].", sTmpFileName.ToUpper), , ex, LogLevel:=3)
            Return False
        End Try


    End Function


    ''' <summary>
    ''' The function will check that the source file is in use or not and then move to Work folder for processing.
    ''' </summary>
    ''' <param name="InputFileName">The complete path of the source filename.</param>
    ''' <returns>Return valid file path if moved to work, else return nothing.</returns>
    ''' <remarks>
    ''' </remarks>
    Public Function MoveToWork(ByVal InputFileName As String) As String
        Dim lsWrkFileName As String = String.Empty

        Try
            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: moving file to work folder " & InputFileName), 9)
            InputFileName = InputFileName.ToLower
            lsWrkFileName = IO.Path.Combine(Me._WorkDirectoryPath, Me.fileName)
            Me.m_WorkFileShadowPath = lsWrkFileName & C_TempFileExt
            Me.m_isResumingFile = False

            'if the input file is in the work folder then return it
            If InputFileName.ToLower.IndexOf(Me._WorkDirectoryPath.ToLower) >= 0 Then
                If IO.File.Exists(m_WorkFileShadowPath) AndAlso Me._StatementStatus <> frmFileErrorB.enumFileErrorUserSelection.Restart Then
                    Me.m_isResumingFile = True
                    getValuesOfShadowFile()
                End If
                Return InputFileName
            End If

            '' Delete Existing file from work directory
            If File.Exists(lsWrkFileName) Then File.Delete(lsWrkFileName)

            If File.Exists(InputFileName) Then
                File.Move(InputFileName, lsWrkFileName)
                Me.m_isMovedToWorkDir = True
                If Not File.Exists(lsWrkFileName) Then
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " An error has occurred while moving file to work directory.", String.Empty, , LogLevel:=1, FileName:=InputFileName)
                    lsWrkFileName = String.Empty
                Else
                    createShadowFile(lsWrkFileName)
                End If
            End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, FileNameDisplay & " An error has occurred while moving file to work directory.", String.Empty, ex, LogLevel:=1, FileName:=InputFileName)
            Return String.Empty
        End Try

        Return lsWrkFileName
    End Function

    ''' <summary>
    ''' Creates a shadow file for an input file
    ''' </summary>
    ''' <param name="file_"></param>
    ''' <remarks></remarks>
    Sub createShadowFile(file_ As String)

        If IO.File.Exists(file_) Then Exit Sub

        Try

            Using oWriter As IO.StreamWriter = IO.File.CreateText(file_)
                oWriter.Flush()
            End Using

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("[{0}] An error occurred while making the shadow file.", file_), String.Empty, ex, LogLevel:=1, FileName:=file_)
        End Try


    End Sub

    ''' <summary>
    ''' set the properties in the shadow files
    ''' </summary>
    ''' <param name="Status_"></param>

    Sub setValuesOfShadowFile(ByVal Status_ As String)
        Dim oWriter As IO.StreamWriter
        Dim NumberOfRecordsBeingProcessed_ As Integer = 0
        Const C_ShadowLine As String = "Status={0},ReachedLine={1},DateOfProcessing={2}"

        If Not Me.m_StatementRecords Is Nothing Then NumberOfRecordsBeingProcessed_ = Me.m_StatementRecords.Length

        Try

            oWriter = IO.File.CreateText(Me.m_WorkFileShadowPath)

            oWriter.WriteLine(String.Format(C_ShadowLine, Status_, Me.m_CurrentRecordNumber, Me.m_fileProcessStartDate.ToString(ServicesUti.GlobalVars.C_IO_DateFormat)))
            oWriter.Flush()
            oWriter.Close()
            oWriter.Dispose()
            oWriter = Nothing

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("[{0}] An error occurred while making the shadow file.", m_WorkFileShadowPath), String.Empty, ex, LogLevel:=1, FileName:=m_WorkFileShadowPath)
        End Try

    End Sub

    ''' <summary>
    ''' gets the properties in the shadow files
    ''' </summary>
    ''' <remarks>
    '''     'Status=Error,ReachedLine=43,NumberOfRecordsBeingProcessed=1,skippedStatements=0
    '''</remarks>
    Sub getValuesOfShadowFile()
        Dim oReader As IO.StreamReader
        'Const C_ShadowLine As String = "Status={0},ReachedLine={1},NumberOfRecordsBeingProcessed={2},DateOfProcessing={3},StatementReached={4}"
        Dim sLine As String
        Dim sArrParams() As String
        Dim sArrParam() As String
        Dim sStatus As String = String.Empty
        Dim tmpDateOfProcessing As String

        Try

            Me.m_CurrentRecordNumber = 1

            oReader = IO.File.OpenText(Me.m_WorkFileShadowPath)
            sLine = oReader.ReadLine()
            oReader.Close()
            oReader.Dispose()
            oReader = Nothing

            If sLine Is Nothing OrElse sLine.Trim = String.Empty Then Exit Sub
            sArrParams = Split(sLine, ",", , CompareMethod.Text)


            sArrParam = Split(sArrParams(0), "=", , CompareMethod.Text) : sStatus = sArrParam(1)
            sArrParam = Split(sArrParams(1), "=", , CompareMethod.Text) : Me.m_CurrentRecordNumber = CInt(sArrParam(1)) '- 1
            sArrParam = Split(sArrParams(2), "=", , CompareMethod.Text) : tmpDateOfProcessing = sArrParam(1)
            Me.m_BeginingLineOfCurrentStatement = m_CurrentRecordNumber
            'sArrParam = Split(sArrParams(1), "=", , CompareMethod.Text) : Me.m_BeginingLineOfCurrentStatement = CInt(sArrParam(1))
            'sArrParam = Split(sArrParams(2), "=", , CompareMethod.Text) : tmpNumberOfRecordsBeingProcessed = CInt(sArrParam(1))


        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("[{0}] An error occurred while getting values from shadow file.", m_WorkFileShadowPath), String.Empty, ex, LogLevel:=1, FileName:=m_WorkFileShadowPath)
        End Try

    End Sub

    ''' <summary>
    ''' Deletes the shadow file
    ''' </summary>
    ''' <remarks></remarks>
    Sub deleteShadowFile()
        Try

            If IO.File.Exists(Me.m_WorkFileShadowPath) Then
                IO.File.Delete(Me.m_WorkFileShadowPath)
            End If

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while deleting Shadow File.", String.Empty, ex, LogLevel:=1, GenerateAIDS:=False)
        End Try

    End Sub

#End Region

#Region " Host File Processing "

    ''' <summary>
    ''' Read the entire customer Info.
    ''' </summary>
    ''' <returns>Return TRUE if function executed successfully, and FALSE if failed to process.</returns>
    ''' </remarks>
    Private Function ConsumeHostRecord() As Boolean
        Try
            Dim i As Integer
            Dim sRecord As String
            _CurrentStatementLines = 0

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: consuming Host Record. "), 9)
            sRecord = GetRecord()
            'Logger.LoggerClass.Singleton.LogInfo(0, sRecord)
            If sRecord Is Nothing OrElse sRecord = String.Empty Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0}, Host Record: Not found.", fileReachedLine), , )
                Return False
            End If
            If m_CusHostRecord.SHD.ReadRecord(sRecord) = False Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0}, Host Record: error in reading/parsing the record.", fileReachedLine), , )
                Return False
            End If

            If m_CusHostRecord.SHD.CusNum Is Nothing OrElse sRecord = String.Empty Then
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("Line #{0}, Host Record: error in reading/parsing the record.", fileReachedLine), , )
                Return False
            End If

            Return True
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "Line #" & fileReachedLine & " Error in Statement.", "Because an exception happened.", ex)
            Return False
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Ask user about what to do in case of an error
    ''' </summary>
    ''' <param name="sPassedError_">Error to be showen</param>
    ''' <remarks></remarks>
    Sub AskUser(Optional ByVal sPassedError_ As String = Nothing)
        If Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError Then
            Exit Sub
        End If
        Me._StatementStatus = Me.m_statementFiles.AskUser(m_fileProcessStartDate, m_TotalCustomers, m_CurrentRecordNumber, Me.m_WorkFilePath, sPassedError_)
    End Sub


    ''' <summary>
    ''' Checks if the file is correctly adhering to several rules
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CheckFileIntegrity() As Boolean

        Try
            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: Checking file integrity. "), 9)

            Dim bAccessibility As Boolean = False
            bAccessibility = CheckFileIntegrity_Accessibility()
            If bAccessibility = False Then
                Return False
            End If

            'Moving file to work folder
            m_WorkFilePath = MoveToWork(Me.FilePath)

            'Checking existence in working folder
            If sib.Util.UString.isNullOrEmpty(WorkFilePath) Then
                m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
                Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, Unable to copy input file.")
                Me.FileState = enumFileState.Failed
                Return False
            End If

            m_StatementSummary.FileSize = New IO.FileInfo(WorkFilePath).Length / 1048576

            'Check existence of Header
            Dim sFirstLine As String
            sFirstLine = getFileHeader()


            If sFirstLine Is Nothing OrElse sFirstLine.IndexOf(C_Record_StatementDocType) <> 0 Then
                m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
                Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, Because the first line is not the File Header (" & C_Record_StatementDocType & ").")
                Me.FileState = enumFileState.Failed
                Return False
            End If

            'File Header (Header) - [000]
            If m_Statement.Header.ReadRecord(sFirstLine) = False Then
                m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
                Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, Because the first line (File Header) (" & C_Record_StatementDocType & ") has problems.")
                Me.FileState = enumFileState.Failed
                Return False
            End If

            'File Name	UUUYYMMDD.TRS
            'Where uuu is the creating unit such as PRD (i.e. production)
            'Where YY is the last  two digit of the current year i.e for 2012, the value will be set 12, MM is the current month and DD is day of the month.

            'Check existence of Footer
            Dim sLastLine As String
            sLastLine = getFileFooter()

            If sLastLine Is Nothing OrElse sLastLine.IndexOf(C_Record_FileFooter) <> 0 Then
                m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
                Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, Because last line is not the File Footer (" & C_Record_FileFooter & ").")
                Me.FileState = enumFileState.Failed
                Return False
            End If

            'File Footer (Footer) - [999]
            If m_Statement.Footer.ReadRecord(sLastLine) = False Then
                m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
                Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, Because last line the File Footer (" & C_Record_FileFooter & ") has problems.")
                Me.FileState = enumFileState.Failed
                Return False
            End If

            Try
                Me.m_TotalCustomers = Convert.ToInt32(m_Statement.Footer.NumRec)
            Catch ex As Exception
                m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
                Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, Because there is error in reading the statement count or the customer count in File Footer (" & C_Record_FileFooter & ").")
                Me.FileState = enumFileState.Failed
                Return False
            End Try


            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "Total Statements: " & TotalCustomers.ToString("##,#0") & ", Total Customers: " & Me.m_TotalCustomers.ToString("##,#0"), LogLevel:=3)
        Catch ex As Exception
            m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
            Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, Me.FileNameDisplay & " -> Skipping file, ", "Because an exception happened.", ex)
            Me.FileState = enumFileState.Failed
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Check if the file is accesible and readable
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CheckFileIntegrity_Accessibility() As Boolean

        'Checking existence
        If sib.Util.UString.isNullOrEmpty(Me.FilePath) OrElse Not IO.File.Exists(Me.FilePath) Then
            m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
            Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, Me.FileNameDisplay & " -> Skipping file, Because it was not found. ")
            Me.FileState = enumFileState.Skipped
            Return False
        End If

        'Check file naming
        If Not ValidateFileName() = True Then
            m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
            Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, Me.FileNameDisplay & " -> Skipping file, Because it did not have the correct naming convention. ")
            Me.FileState = enumFileState.Skipped
            Return False
        End If

        'Check reading access of the file
        If Not sib.UFile.CheckExclusiveAccess(Me.FilePath) = True Then
            m_statementFiles.FilesProcessSummary.Skip = m_statementFiles.FilesProcessSummary.Skip + 1
            Grid_MakeNewRow(0, Me.fileName, New String() {"-/-", "-/-", "Skipped"})
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, Me.FileNameDisplay & " -> Skipping file, could not get exclusive access, maybe the file is still in use. Access denied.")
            Me.FileState = enumFileState.Skipped
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Checks if the user has canceled the operation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function isUserCanceled() As Boolean
        If Me.m_statementFiles.IsCanceledJobs Then
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Warning, FileNameDisplay & " ->Canceled by user.", LogLevel:=2)
            setValuesOfShadowFile("Canceled")
            Grid_UpdateRow("Canceled")
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Cancel
            Return True
        Else
            setValuesOfShadowFile("OK")
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
    Public Function ProcessStatements() As Boolean
        Dim iStatementNumStart As Integer = 1
        Me._fileNo = Me.m_statementFiles.FilesProcessSummary.OverAllFileNo

        Try

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: ProcessStatements."), 9)
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Normal
FileRestart:
            Me.m_fileProcessStartDate = Date.Now
            Me.m_statementStartTime = Me.m_fileProcessStartDate
            m_Report_SheetSequence = 0
            m_Report_MatchSequence = 0
            With m_StatementSummary
                .Total = 0
                .StartTime = Me.m_fileProcessStartDate
            End With
            gLastErrorMsg = String.Empty

            Me.FileState = enumFileState.OK
            Me.m_isResumingFile = False
            Me.Records_CurrentItemNum = 0
            Me.m_BeginingLineOfCurrentStatement = 0
            Me.m_FileTotalLines = 0
            Me.m_isMovedToWorkDir = False
            iStatementNumStart = 1
            m_fileName = IO.Path.GetFileName(FilePath)

            If Not m_FileStream Is Nothing Then
                Try
                    m_FileStream.Close()
                    m_FileStream.Dispose()
                    m_FileStream = Nothing
                Catch ex As Exception
                    'consumed
                End Try
            End If

            'make an entry in the UI for the file
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ", LogLevel:=3)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, FileNameDisplay & " -> Started...", LogLevel:=3)
            'RaiseEvent InputFileChanged(Me, New SIBL0100.ProgressTrack("", TotalCustomers, 0, 0, False))
            Grid_MakeNewRow(0, Me.fileName, New String() {"0", "", "Processing...", Me.m_fileProcessStartDate.ToString(ServicesUti.GlobalVars.C_UI_DateFormat)})

            If Not CheckFileIntegrity() Then
                If Me.FileState = enumFileState.Failed Then
                    AskUser()
                    Select Case Me._StatementStatus
                        Case frmFileErrorB.enumFileErrorUserSelection.ResumeFile
                            'Continue normally
                        Case frmFileErrorB.enumFileErrorUserSelection.Skip
                            processSkipped()
                            Return False
                        Case frmFileErrorB.enumFileErrorUserSelection.Restart
                            processRestart()
                            GoTo FileRestart
                        Case frmFileErrorB.enumFileErrorUserSelection.Abort
                            processAbort()
                            Return False
                        Case frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
                            processCrystalReportsError()
                            Return False
                    End Select
                ElseIf Me.FileState = enumFileState.Skipped Then
                    processSkipped()
                    Return False
                End If
            End If
            If isUserCanceled() Then Return False

            'Ask user what to do if a file is found in the working folder
            If Me.m_isResumingFile Then
                AskUser("This file is found in the working directory, so it might have been left from a non finished processing.")
               
                Select Case Me._StatementStatus

                    Case frmFileErrorB.enumFileErrorUserSelection.ResumeFile
                        'Continue normally
                    Case frmFileErrorB.enumFileErrorUserSelection.Skip
                        processSkipped()
                        Return False
                    Case frmFileErrorB.enumFileErrorUserSelection.Restart

                        processRestart(True)
                        GoTo FileRestart
                    Case frmFileErrorB.enumFileErrorUserSelection.Abort
                        processAbort()
                        Return False
                    Case frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
                        processCrystalReportsError()
                        Return False
                End Select
            End If

            m_FileStream = New StreamReader(WorkFilePath, Encoding.GetEncoding(C_FileEncoding))

            If _fileQue Is Nothing Then
                _fileQue = New FileQueue(m_FileStream)
            End If

            m_FileTotalLines = 0
            Records_CurrentItemNum = 0 'Start from the first record if the file is resuming a previous process
            'm_FileCurrentRecord
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, FileNameDisplay & " -> Opened with encoding [" & Encoding.GetEncoding(C_FileEncoding).EncodingName & "]", LogLevel:=3)
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, "---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ", LogLevel:=3)

            If isUserCanceled() Then Return False

            'Read the first Statement regardless of the file is begining from start or resuming a previous process, so we can read the marketting records
            Dim bRetValue As Boolean = False
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading normally started...", 4)
            Me.m_statementStartTime = Now
            bRetValue = readRecords()
            Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading normally Ended. TimeSpan: " & UDate.getTimeDiffrence(Me.m_statementStartTime), 4)

            If Not bRetValue Then
                setValuesOfShadowFile("Error")
                AskUser()
                Select Case Me._StatementStatus
                    Case frmFileErrorB.enumFileErrorUserSelection.ResumeFile
                        'Continue normally
                    Case frmFileErrorB.enumFileErrorUserSelection.Skip
                        processSkipped()
                        Return False
                    Case frmFileErrorB.enumFileErrorUserSelection.Restart
                        processRestart()
                        GoTo FileRestart
                    Case frmFileErrorB.enumFileErrorUserSelection.Abort
                        processAbort()
                        Return False
                    Case frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
                        processCrystalReportsError()
                        Return False
                End Select
            End If

            If isUserCanceled() Then Return False

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("{0} -> Processing started, having [{1:##,#0}] statement(s).", FileNameDisplay, TotalCustomers), LogLevel:=3)

            Me.Records_CurrentItemNum = 1 'Start after the [file header] if the file is being processed from the begining
            If Me.m_isResumingFile Then
                'Me.m_BeginingLineOfCurrentStatement = Me.m_ResumingFileStartLine
                If Me.m_CurrentRecordNumber > 0 Then
                    iStatementNumStart = Me.m_BeginingLineOfCurrentStatement
                    Me.m_CurrentRecordNumber = 0 'Start from the first record if the file is resuming a previous process and it has finished the first statement
                End If

                setValuesOfShadowFile("Resuming") 'Setting the values of the shadow file in the resume state
                Me.m_statementStartTime = Now

                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading In Resume state, started...", 4)
                bRetValue = readRecordsResumed()
                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Reading In Resume state, Ended. TimeSpan: " & UDate.getTimeDiffrence(Me.m_statementStartTime), 4)
                If Not bRetValue Then
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("[{0}] Could not read file.", WorkFilePath), "Could not read file to memory.", LogLevel:=1, GenerateAIDS:=False)
                    setValuesOfShadowFile("Error")
                    AskUser()
                    Select Case Me._StatementStatus
                        Case frmFileErrorB.enumFileErrorUserSelection.ResumeFile
                            'Continue normally
                        Case frmFileErrorB.enumFileErrorUserSelection.Skip
                            processSkipped()
                            Return False
                        Case frmFileErrorB.enumFileErrorUserSelection.Restart
                            processRestart()
                            GoTo FileRestart
                        Case frmFileErrorB.enumFileErrorUserSelection.Abort
                            processAbort()
                            Return False
                        Case frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
                            processCrystalReportsError()
                            Return False
                    End Select
                End If
            End If
            '-------------------------------------------------------------------------------------------------------------
            '-------------------------------------------------------------------------------------------------------------

            For Me.m_CurrentRecordNumber = iStatementNumStart To TotalCustomers
                Dim currentStatmentStart As DateTime = Date.Now
                Me.m_statementFiles.FilesProcessSummary.Total += 1
                m_StatementSummary.Total += 1

                If isUserCanceled() Then Return False

                'ConsumeStatement
                Dim bisConsumeStatement As Boolean = False
                Dim dConsumeStatement As Date
                Dim iStatmentFirstLineNumber As Integer = 0

                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> ConsumeStatement, Started...", 4)
                dConsumeStatement = Now
                bisConsumeStatement = ConsumeHostRecord()
                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> ConsumeStatement, Ended. TimeSpan: " & UDate.getTimeDiffrence(dConsumeStatement), 4)
                iStatmentFirstLineNumber = Me.fileReachedLine - Me.m_StatementRecords.Length
                If iStatmentFirstLineNumber < 0 Then
                    iStatmentFirstLineNumber = Me.fileReachedLine
                End If
                'EventHandler(&H89010000, SIBL0100.EventType.Information, "Statement for account " & m_CusStm.ACC.AccNum, LogLevel:=2)
                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("Processing customer #[{0}], for Rec #[{1}], at line #[{2}]", m_CusHostRecord.SHD.CusNum, Me.m_CurrentRecordNumber.ToString("##,#0"), iStatmentFirstLineNumber.ToString("##,#0")), LogLevel:=3)


                Dim bisCheckStatementHeaderFooter As Boolean = False
                Dim dCheckStatementHeaderFooter As Date
                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> CheckStatementHeaderFooter, Started...", 4)
                dCheckStatementHeaderFooter = Now
                bisCheckStatementHeaderFooter = checkStatementHeaderFooter()
                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> CheckStatementHeaderFooter, Ended. TimeSpan: " & UDate.getTimeDiffrence(dCheckStatementHeaderFooter), 4)

                If isUserCanceled() Then Return False

                m_FileTotalPages += m_Report_TotalPages
                If bisCheckStatementHeaderFooter Then m_statementFiles.StatementsLog.fileStats_InsertHostRecord(Me.m_CusHostRecord)


                Grid_UpdateRow()

                'Me.Records_CurrentItemNum = 0 'Start each new statement from begning of the array
                'Me.m_StatementRecords = Nothing
                Me.m_CusHostRecord = Nothing

                If _currentNumberOfRows_GC_Collect > C_NumberOfRowsToGC_Collect Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()

                    _currentNumberOfRows_GC_Collect = 0
                End If

                If isUserCanceled() Then Return False

                Logger.LoggerClass.Singleton.LogInfo(0, "statement -> Full statement Ended. TimeSpan: " & UDate.getTimeDiffrence(m_statementStartTime), 4)

            Next

StatementsEnd:
            'CloseStatementFile()
            'EventHandler(&H89010000, SIBL0100.EventType.Information, FileName & " Completed, Time elapsed " & gPrsSum.Duration & ", records processed: " & FileTotalLines & ", statements processed: " & TotalStatements)
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully

        Catch ex As OutOfMemoryException
            Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "System is out of resources.", , DirectCast(ex, Exception), FileName:=fileName)
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, String.Format("{0} -> An Unknown exception has occurred while processing file. {1}", FileNameDisplay, ex.Message), , ex, FileName:=fileName)
        Finally

            Me.m_fileProcessEndDate = Date.Now
            Me.m_StatementSummary.EndTime = m_fileProcessEndDate
            Me.m_StatementRecords = Nothing
            m_CusHostRecord = Nothing
            GC.Collect()
            Dim sMsg As String
            Dim sOperation As String = String.Empty
            sMsg = String.Format("{0} [%Operation%], Time elapsed {1}, records processed: {2}, statements processed: {3}", FileNameDisplay, m_StatementSummary.Duration, FileTotalLines.ToString("##,#0"), TotalCustomers.ToString("##,#0"))



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

            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, sMsg.Replace("[%Operation%]", sOperation), LogLevel:=3)

            If Me.StatmentStatus <> frmFileErrorB.enumFileErrorUserSelection.CrystalReportsError Then
                MoveToArchiveOrBad(WorkFilePath, m_FileStream)
            End If

        End Try
        Return Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully
    End Function

    ''' <summary>
    ''' This is called when the user selects to abort the file, and move it to the bad folder
    ''' </summary>
    ''' <remarks></remarks>
    Sub processAbort()
        Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: aborted."), 9)
        Grid_UpdateRow("Aborted")

        Dim oDBProvider As IDBProvider = DBProvider.singleton.getDBProvider

        Dim lDBFileNumber As Integer
        Dim lDBFileExecutionNumber As Integer

        If m_DBExecutionNo = 0 Then
            lDBFileNumber = m_statementFiles.DBFileNumber
            lDBFileExecutionNumber = m_statementFiles.DBFileExecutionNumber
        Else
            lDBFileExecutionNumber = m_DBExecutionNo
            m_statementFiles.StatementsLog.fileStats_DeleteAll(lDBFileNumber, m_DBExecutionNo, False)
        End If

        m_statementFiles.StatementsLog.fileStats_DeleteAll(lDBFileNumber, lDBFileExecutionNumber, False)

        m_statementFiles.StatementsLog.fileStats_UpdateStatus(lDBFileNumber, m_statementFiles.currentStatement.StartDate, Now, "File Has been Aborted")
        'oDBProvider.FileInput_Insert(lDBFileNumber)
        MoveToArchiveOrBad(WorkFilePath, m_FileStream)

    End Sub

    ''' <summary>
    ''' It will make the file path point to its new place which is in the work folder
    ''' </summary>
    ''' <remarks></remarks>
    Sub processRestart(Optional deleteParent_ As Boolean = False)
        Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: restarted."), 9)
        Grid_UpdateRow("Restarted")

        If Me.m_isMovedToWorkDir = True AndAlso Me.m_FilePath.ToLower.IndexOf(Me._InputFilePath.ToLower) >= 0 Then
            If Me.m_FilePath.ToLower.Contains(Me._InputFilePath.ToLower) Then
                Dim sFileFullPath As String
                sFileFullPath = IO.Path.Combine(Me._WorkDirectoryPath, fileName)
                If IO.File.Exists(sFileFullPath) Then
                    Me.m_FilePath = sFileFullPath
                End If


            End If
        End If
        Me._fileQue = Nothing
        Me.m_FileTotalLines = 0

        Me.m_StatementRecords = Nothing
        Me.Records_CurrentItemNum = 1
        If m_DBExecutionNo = 0 Then
            m_statementFiles.StatementsLog.fileStats_DeleteAll(Me.m_statementFiles.DBFileNumber, Me.m_statementFiles.DBFileExecutionNumber, deleteParent_)
        Else
            m_statementFiles.StatementsLog.fileStats_DeleteAll(Me.m_statementFiles.DBFileNumber, m_DBExecutionNo, deleteParent_)
        End If

        Me.m_statementFiles.FilesProcessSummary.OverAllFileNo = Me.m_statementFiles.DBFileNumber
        Me.m_statementFiles.FilesProcessSummary.ExecutionNo = Me.m_statementFiles.DBFileExecutionNumber
    End Sub

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

        ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, sMsg, , Nothing, GenerateAIDS:=False, FileName:=fileName)
        ServicesUti.Services.Singleton.AppInstance.ModalMessageBox(sMsg, MessageBoxButtons.OK, , MessageBoxIcon.Error, "MDOX")

    End Sub

    ''' <summary>
    ''' The function will move the file from work directory to the archive directory if isCompleted_=true else to the Bad directory.
    ''' </summary>
    ''' <param name="WorkFileName">The path and file name of the statement file to be moved.</param>
    ''' <param name="oFile_">the file</param>
    ''' <returns>Return valid file path if moved, else return nothing.</returns>
    ''' <remarks>
    ''' </remarks>
    Public Function MoveToArchiveOrBad(ByVal WorkFileName As String, ByRef oFile_ As IO.StreamReader) As String
        Try
            Dim datFileName As String = Me.fileName
            Dim sDstFilePath As String = String.Empty

            Logger.LoggerClass.Singleton.LogInfo(0, String.Format("Statement: MoveToArchiveOrBad."), 9)

            If Directory.Exists(Me._ArchiveDirectoryPath) = False Then Directory.CreateDirectory(Me._ArchiveDirectoryPath)
            If Directory.Exists(Me._BadFilesDirectoryPath) = False Then Directory.CreateDirectory(Me._BadFilesDirectoryPath)

            If IO.File.Exists(WorkFileName) Then
                If Not oFile_ Is Nothing Then
                    Try
                        oFile_.Close()
                        oFile_.Dispose()
                        oFile_ = Nothing
                    Catch ex As Exception
                        'consumed
                    End Try
                End If

                If Me.isUserCanceled Then
                    Grid_UpdateRow("Canceled")
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("file left at [{0}], because user canceled its operation.", WorkFileName), LogLevel:=3)
                    Return WorkFileName
                End If

                If Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.Skip Then
                    Grid_UpdateRow("Skipped")
                    ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("file left at [{0}], because user Skipped it.", WorkFileName), LogLevel:=3)
                    Return WorkFileName
                End If

                If Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully Then
                    Grid_UpdateRow("Finished")
                    sDstFilePath = IO.Path.Combine(Me._ArchiveDirectoryPath, datFileName)
                    If IO.File.Exists(Me.m_WorkFileShadowPath) Then IO.File.Delete(Me.m_WorkFileShadowPath)
                Else
                    Grid_UpdateRow("Failed")
                    sDstFilePath = IO.Path.Combine(Me._BadFilesDirectoryPath, datFileName)
                End If

                If IO.File.Exists(sDstFilePath) Then IO.File.Delete(sDstFilePath)
                IO.File.Move(WorkFileName, sDstFilePath)

                ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Information, String.Format("file moved to [{0}].", sDstFilePath), LogLevel:=3)

                'Move the shadow file if not completed to the bad folder
                If Not Me._StatementStatus = frmFileErrorB.enumFileErrorUserSelection.CompletedSuccessfully Then
                    If IO.File.Exists(Me.m_WorkFileShadowPath) Then
                        Dim sShadowFileName As String = IO.Path.GetFileName(Me.m_WorkFileShadowPath)
                        Dim sDstshadowPath As String = IO.Path.Combine(Me._BadFilesDirectoryPath, sShadowFileName)
                        If IO.File.Exists(sDstshadowPath) Then IO.File.Delete(sDstshadowPath)
                        File.Move(Me.m_WorkFileShadowPath, sDstshadowPath)
                    End If
                End If

                If IO.File.Exists(sDstFilePath) Then
                    Return sDstFilePath
                End If
            End If
        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while moving/leaving file.", String.Empty, ex, LogLevel:=1, FileName:=WorkFileName)
        End Try
        Return String.Empty
    End Function

#End Region



    ''' <summary>
    ''' Gets the file footer starting from the end of file and going backword.
    '''  'This code is taken from:
    ''' url: http://bytes.com/topic/net/answers/109316-reading-large-text-file-line-line-backwards
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getFileFooter() As String
        Dim i As Integer
        Dim streamTextFile As Stream = Nothing
        Dim strArray() As String
        Dim sBuffer As String = String.Empty

        Try
            Logger.LoggerClass.Singleton.LogInfo(0, "Started", 9)

            streamTextFile = IO.File.OpenRead(Me.m_WorkFilePath)
            streamTextFile.Seek(0, SeekOrigin.End)

            ' the ideal block size is something of a holy grail, I guess. After some
            ' testing of sizes from 1K to 75K, it looked like speed really dropped off
            ' after the mid 60Ks or so. When testing for the minimum time to finish a task,
            ' the number 41K and 42K kept coming up, with occasional 34K thrown in for fun.
            ' There may be an ideal size for each file - I'm not really sure. What I do know
            ' is that over a network drive, this was 7X faster than just doing 'readline' until
            ' the cows come home. On a local drive, it doesn't make as much of a difference
            ' because the file access calls are similar in load to the overhead necessary
            ' when pulling large blocks. But when it is a network drive, the file access gets
            ' slower, so we're much better off minimizing the number of times we access
            ' the file.
            Dim iBlockSize As Integer = 41000
            Dim iFirstElement As Integer = 1

            While streamTextFile.Position > 0
                If streamTextFile.Position <= iBlockSize Then
                    iBlockSize = CInt(streamTextFile.Position)
                    iFirstElement = 0
                End If

                Dim byteArray(iBlockSize - 1) As Byte
                streamTextFile.Seek(-1 * iBlockSize, SeekOrigin.Current)
                streamTextFile.Read(byteArray, 0, byteArray.Length)
                streamTextFile.Seek(-1 * iBlockSize, SeekOrigin.Current)

                strArray = Split(Encoding.GetEncoding(C_FileEncoding).GetString(byteArray), vbCrLf) ' System.Text.Encoding.ASCII
                strArray(strArray.Length - 1) = strArray(strArray.Length - 1) + sBuffer
                For i = strArray.GetUpperBound(0) To iFirstElement Step -1
                    If strArray(i).Trim <> String.Empty Then
                        Return strArray(i).ToString
                    End If
                Next
                sBuffer = strArray(0)
            End While

            streamTextFile.Close()
            streamTextFile.Dispose()
            streamTextFile = Nothing

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while getting File Footer.", String.Empty, ex, LogLevel:=1)
            Return String.Empty
        Finally
            If Not streamTextFile Is Nothing Then
                streamTextFile.Close()
                streamTextFile.Dispose()
                strArray = Nothing
            End If
        End Try

        Return String.Empty
    End Function

    ''' <summary>
    ''' Reads the file header
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getFileHeader() As String
        Dim oFileHeader As StreamReader = Nothing
        Try
            Dim count As Integer = 0
            Dim buffer() As Char

            oFileHeader = New StreamReader(WorkFilePath, Encoding.GetEncoding(C_FileEncoding))

            ReDim buffer(150)

            count = oFileHeader.Read(buffer, 0, 150) '150x header size
            Dim sLine As String = Convert.ToString(buffer).Substring(0, count)

            oFileHeader.Close()
            oFileHeader.Dispose()
            oFileHeader = Nothing

            Return sLine

        Catch ex As Exception
            ServicesUti.Uti.handleMessage(&H89010000, SIBL0100.EventType.Critical, "An error has occurred while getting File Header.", String.Empty, ex, LogLevel:=1)
            Return String.Empty
        Finally
            If Not oFileHeader Is Nothing Then
                oFileHeader.Close()
                oFileHeader.Dispose()
                oFileHeader = Nothing
            End If
        End Try

        Return String.Empty
    End Function

End Class
