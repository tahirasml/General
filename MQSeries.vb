Option Strict Off

Imports System.Globalization

Public Class MQSeries
    Inherits ErrorClass

    Shared Function strip(ByRef strMsg As String, ByVal nChars As Integer) As String
        Dim RetStr As String
        If strMsg Is Nothing Then Return Nothing
        If strMsg.Length < 1 Then Return Nothing
        'If Len(strMsg) < nChars Then strip = "": Exit Function
        If strMsg.Length <= nChars Then
            RetStr = strMsg
            strMsg = ""
        Else
            RetStr = Left(strMsg, nChars)
            strMsg = Right(strMsg, strMsg.Length - nChars)
        End If
        Return RetStr
    End Function

    Shared Function IsValidMid(ByVal MsgID As String) As Boolean
        If MsgID Is Nothing Then Return False
        If MsgID.Trim = "" Then Return False
        If MsgID.Length <> 12 Then Return False
        Return True
    End Function


#Region "Declaration Section"

    Public Enum enumWaitTime As Integer
        TENTH = 100
        HALF = 500
        ONE = 1000
        TWO = 2000
        THREE = 3000
        FOUR = 4000
        FIVE = 5000
    End Enum

    
    Public Const MQCC_OK As Integer = 0
   
    Public Const MQGMO_WAIT As Integer = 1
    Public Const MQGMO_NO_SYNCPOINT As Integer = 4
    
    Public Const MQMO_MATCH_MSG_ID As Integer = 1
    Public Const MQMO_MATCH_CORREL_ID As Integer = 2
   
    Public Const MQOO_INPUT_AS_Q_DEF As Integer = 1

    Public Const MQOO_OUTPUT As Integer = 16
    
    Public Const MQPMO_NO_SYNCPOINT As Integer = 4
   
   
    Public Const MQRC_NO_MSG_AVAILABLE As Integer = 2033
    
    Public Const MQRC_NOT_OPEN As Integer = 6125

    Private Seconds As enumWaitTime

    Private Const CUSTOMERFOUND As Integer = 1
    Private Const MONTHCOUNTDEPOSITS As Integer = 12
    Private Const MONTHCOUNTWITHDRAWALS As Integer = 12
    Private Const MT_BANK_TEST As Integer = 65536
    Private Const MT_BANK_REQUEST As Integer = 65537
    Private Const MT_BANK_REPLY As Integer = 65538
    Private Const BANK_TEST_RESPONSE As String = "This is a bank test response."

    'Formats'
    Private Const MQFMT_NONE As String = "        "
    Private Const MQFMT_ADMIN As String = "MQADMIN "
    Private Const MQFMT_CHANNEL_COMPLETED As String = "MQCHCOM "
    Private Const MQFMT_CICS As String = "MQCICS  "
    Private Const MQFMT_COMMAND_1 As String = "MQCMD1  "
    Private Const MQFMT_COMMAND_2 As String = "MQCMD2  "
    Private Const MQFMT_DEAD_LETTER_HEADER As String = "MQDEAD  "
    Private Const MQFMT_DIST_HEADER As String = "MQHDIST "
    Private Const MQFMT_EVENT As String = "MQEVENT "
    Private Const MQFMT_IMS As String = "MQIMS   "
    Private Const MQFMT_IMS_VAR_STRING As String = "MQIMSVS "
    Private Const MQFMT_MD_EXTENSION As String = "MQHMDE  "
    Private Const MQFMT_PCF As String = "MQPCF   "
    Private Const MQFMT_REF_MSG_HEADER As String = "MQHREF  "
    Private Const MQFMT_RF_HEADER As String = "MQHRF   "
    Private Const MQFMT_RF_HEADER_2 As String = "MQHRF2  "
    Private Const MQFMT_STRING As String = "MQSTR   "
    Private Const MQFMT_TRIGGER As String = "MQTRIG  "
    Private Const MQFMT_WORK_INFO_HEADER As String = "MQHWIH  "
    Private Const MQFMT_XMIT_Q_HEADER As String = "MQXMIT  "

    'Encoding'
    Private Const DefEnc_OS2 As Integer = 546
    Private Const DefEnc_DOS As Integer = 546
    Private Const DefEnc_Windows As Integer = 546
    Private Const DefEnc_MicroFocusCOBOL As Integer = 17
    Private Const DefEnc_OpenVMS As Integer = 273
    Private Const DefEnc_MVSESA As Integer = 785
    Private Const DefEnc_OS400 As Integer = 273
    Private Const DefEnc_Tandem As Integer = 273
    Private Const DefEnc_UNIX As Integer = 273

    Private m_MQSess As Object '* session object
    Private m_QMgr As Object '* queue manager object
    Private m_InputQueue As Object          '* input queue object
    Private m_OutputQueue As Object            '* output queue object
    Private m_PutMsg As Object           '* message object for put
    Private m_GetMsg As Object                 '* message object for get
    Private m_PutOptions As Object   '* get message options
    Private m_GetOptions As Object   '* put message options

    Private m_WksID As String = "SAIBW000"
    Private m_BrnCde As String = "0101"
    Private m_MsgSrc As String
    Private m_MsgDst As String
    Private m_EnvSrc As String
    Private m_EnvDst As String
    Private m_SentMsgID As String
    Private m_SendRetries As Integer = 3
    Private m_GetRetries As Integer = 3
    Private m_ReSendDelay As Integer = 3000
    Private m_DoWait As Boolean = True
    Private m_TmeOut As Integer = 2000
    Private m_LoggedUser As String
    Private m_Connected As Boolean
    Private m_QueueManager As String
    Private m_ReadQueue As String
    Private m_WriteQueue As String
    Private m_ReplyToQueue As String = ""

    Public Structure SibHdrStruct
        'SubStruct:MsgHdr	Size:256
        Dim MsgProCde As String
        Dim MsgMid As String
        Dim MsgStp As String
        'SubStruct:MsgOrg	Size:10
        Dim MsgSrc As String
        Dim MsgSrcEnv As String
        Dim MsgSrcRsv As String
        Dim MsgUsrIde As String
        'SubStruct:MsgDst	Size:9
        Dim MsgTgt As String
        Dim MsgTgtEnv As String
        Dim MsgTgtRsv As String
        'SubStruct:MsgResCde	Size:10
        Dim MsgResVal As String
        Dim MsgActCde As String
        Dim MsgSysCon As String
        Dim Rsv1b As String
        Dim MsgResInd As String
        Dim SibResCde As String
        Dim SifRefNum As String
        'SubStruct:SibEnvLoc	Size:90
        Dim SibEnvUnt As String    '3
        Dim SibOrgCls As String     '3
        'Dim SibKeyLoc As String    '84
        Dim SibKeyCls As String '3x
        Dim SibCus As String '6n
        Dim SibAcc As String '13n
        Dim SibVar As String '20x

        Dim MsiDat As String '10x
        Dim Rsv21 As String  '10x
        Dim CstVerTab As String  '12n
        Dim SibCusSeg As String '1x
        Dim Rsv2 As String  '9x
        '**************************************************************************************************
        'SubStruct:SibNonRep	Size:50
        Dim IceBrnNum As String '4x
        Dim LstNonDupMid As String '12x
        Dim IceTryFin As String '1x
        Dim Rsv415 As String '2x
        Dim AskMidRef As String '12x	MID of the corresponding ASK message
        Dim IceCapCde As String '2x	    ICE system feature capability code
        Dim AutUsrIde As String '10x
        Dim AutPacVal As String '6x
        Dim SibNrdTyp As String '1x
        Dim Rsv3 As String   '4x

        Public Function PackHeader(ByVal BaseClass As MQSeries) As String
            Dim RetStr As String
            Try
                IceCapCde = "02"
                With BaseClass
                    RetStr = .PackString(MsgProCde, 6)
                    RetStr &= .PackString(MsgMid, 12)
                    RetStr &= .PackString(MsgStp, 14)
                    RetStr &= .PackString(MsgSrc, 4)
                    RetStr &= .PackString(MsgSrcEnv, 3)
                    RetStr &= .PackString(MsgSrcRsv, 3)
                    RetStr &= .PackString(MsgUsrIde, 32)
                    RetStr &= .PackString(MsgTgt, 4)
                    RetStr &= .PackString(MsgTgtEnv, 3)
                    RetStr &= .PackString(MsgTgtRsv, 2)
                    RetStr &= .PackString(MsgResVal, 3)
                    RetStr &= .PackString(MsgActCde, 2)
                    RetStr &= .PackString(MsgSysCon, 3)
                    RetStr &= .PackString(Rsv1b, 1)
                    RetStr &= .PackString(MsgResInd, 1)
                    RetStr &= .PackString(SibResCde, 7)
                    RetStr &= .PackString(SifRefNum, 12)
                    RetStr &= .PackString(SibEnvUnt, 3)
                    RetStr &= .PackString(SibOrgCls, 3)
                    ' SibKeyLoc, 84
                    RetStr &= .PackString(SibKeyCls, 3)
                    RetStr &= .PackString(SibCus, 6)
                    RetStr &= .PackString(SibAcc, 13)
                    RetStr &= .PackString(SibVar, 20)

                    RetStr &= .PackString(MsiDat, 10)
                    RetStr &= .PackString(Rsv21, 10)
                    RetStr &= .PackString(CstVerTab, 12)
                    RetStr &= .PackString(SibCusSeg, 1)
                    RetStr &= .PackString(Rsv2, 9)
 
                    RetStr &= .PackString(BaseClass.m_BrnCde, 4)
                    RetStr &= .PackString(BaseClass.m_SentMsgID, 12)
                    RetStr &= .PackString(IceTryFin, 1)
                    RetStr &= .PackString(Rsv415, 2)
                    RetStr &= .PackString(AskMidRef, 12)
                    RetStr &= .PackString(IceCapCde, 2)
                    RetStr &= .PackString(AutUsrIde, 10)
                    RetStr &= .PackString(AutPacVal, 6)
                    RetStr &= .PackString("I", 1)  'RetStr &= .PackString(.SibNrdTyp, 1)
                    'rsv3
                    RetStr &= .PackString(Rsv3, 4)
                End With
            Catch
                RetStr = Space(256)
            End Try
            Return RetStr
        End Function

        Public Sub ReadHeader(ByVal HdrStr As String)
            'SibHdr	    256-->	Bank Header (includes BankAway Header)

            'BwyMsgHdr	93 -->	BankAway Message header (structure follows)
            MsgProCde = strip(HdrStr, 6)
            MsgMid = strip(HdrStr, 12)
            MsgStp = strip(HdrStr, 14)

            'MsgOrg	    10x-->	Message sender
            MsgSrc = strip(HdrStr, 4)
            MsgSrcEnv = strip(HdrStr, 3)
            MsgSrcRsv = strip(HdrStr, 3)

            MsgUsrIde = strip(HdrStr, 32)

            'MsgDst	    9x-->	Message destination
            MsgTgt = strip(HdrStr, 4)
            MsgTgtEnv = strip(HdrStr, 3)
            MsgTgtRsv = strip(HdrStr, 2)

            'MsgResCde	10-->	Result code
            MsgResVal = strip(HdrStr, 3)
            MsgActCde = strip(HdrStr, 2)
            MsgSysCon = strip(HdrStr, 3)
            Rsv1b = strip(HdrStr, 1)
            MsgResInd = strip(HdrStr, 1)

            SibResCde = strip(HdrStr, 7)
            SifRefNum = strip(HdrStr, 12)

            'SibEnvLoc	90-->	Environment and Locus (Structure follows)
            SibEnvUnt = strip(HdrStr, 3)
            SibOrgCls = strip(HdrStr, 3)

            'SibKeyLoc	84-->	Key Locus values (structure follows)
            SibKeyCls = strip(HdrStr, 3)
            SibCus = strip(HdrStr, 6)
            SibAcc = strip(HdrStr, 13)
            SibVar = strip(HdrStr, 20)

            MsiDat = strip(HdrStr, 10)
            Rsv21 = strip(HdrStr, 10)
            CstVerTab = strip(HdrStr, 12)
            SibCusSeg = strip(HdrStr, 1)
            Rsv2 = strip(HdrStr, 9)

            'SibNonRep	50x-->	Non-repudiation data (refer to the structures)
            IceBrnNum = strip(HdrStr, 4)
            LstNonDupMid = strip(HdrStr, 12)
            IceTryFin = strip(HdrStr, 1)
            Rsv415 = strip(HdrStr, 2)
            AskMidRef = strip(HdrStr, 12)
            IceCapCde = strip(HdrStr, 2)
            AutUsrIde = strip(HdrStr, 10)
            AutPacVal = strip(HdrStr, 6)
            SibNrdTyp = strip(HdrStr, 1)

            Rsv3 = strip(HdrStr, 4)
        End Sub
    End Structure

    Private Structure SibHdrStruct83
        'SubStruct:MsgHdr	Size:83
        Dim MsgProCde As String '6x
        Dim MsgMid As String '12x
        Dim MsgStp As String '14x
        Dim MsgOrg As String '10x
        Dim MsgUsrIde As String '32x
        Dim MsgDst As String '9x
    End Structure

    Private Structure SibHdrStruct93
        'SubStruct:MsgHdr	Size:93
        Dim MsgProCde As String '6x
        Dim MsgMid As String '12x
        Dim MsgStp As String '14x
        Dim MsgOrg As String '10x
        Dim MsgUsrIde As String '32x
        Dim MsgDst As String '9x
        'MsgResCde	10
        Dim MsgResVal As String '3n
        Dim MsgActCde As String '2x
        Dim MsgSysCon As String '3x
        Dim Rsv1 As String  '2x
    End Structure

#End Region

    'Public Section

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the currently logged on user ID for MQ Series messages    ''' 
    ''' </summary>
    ''' <remarks>
    ''' This property will not return the current Windows User! It just returns whatever was set in it before     ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property LoggedUser() As String
        Set(ByVal Value As String)
            m_LoggedUser = Value
        End Set
        Get
            LoggedUser = m_LoggedUser
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Returns the connection status of the MQ Series class with the QueueManager    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public ReadOnly Property Connected() As Boolean
        Get
            Connected = m_Connected
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the source system name for the SAIB message header    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property SrcSysName() As String
        Get
            SrcSysName = m_MsgSrc
        End Get
        Set(ByVal Value As String)
            m_MsgSrc = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the Timeout period before a message retrieval is considered failed    ''' 
    ''' </summary>
    ''' <remarks>
    ''' If DoWait is set to False, this value has no effect    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property TmeOut() As Integer
        Get
            TmeOut = m_TmeOut
        End Get
        Set(ByVal Value As Integer)
            m_TmeOut = Value
            m_GetOptions.WaitInterval = m_TmeOut
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets The user's branch code    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	27/07/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property BrnCde() As String
        Get
            BrnCde = m_BrnCde
        End Get
        Set(ByVal Value As String)
            m_BrnCde = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets whether message retrieval blocks the current thread with the specified TmeOut    ''' 
    ''' </summary>
    ''' <remarks>
    ''' if Set to False, GetMessage will returns immediatly regardless if a message was found or not.
    ''' The application must provide logic to handle the message retrieval retries if desired so.   ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property DoWait() As Boolean
        Get
            DoWait = m_DoWait
        End Get
        Set(ByVal Value As Boolean)
            m_DoWait = Value
            m_GetOptions.Options = MQGMO_NO_SYNCPOINT
            If m_DoWait Then m_GetOptions.Options += MQGMO_WAIT
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the the destination system name for the SAIB message header    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property DstSysName() As String
        Get
            DstSysName = m_MsgDst
        End Get
        Set(ByVal Value As String)
            m_MsgDst = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the the source environment name for the SAIB message header    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property SrcEnvName() As String
        Get
            SrcEnvName = m_EnvSrc
        End Get
        Set(ByVal Value As String)
            m_EnvSrc = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the message send retry count    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	233/04/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property SendRetries() As Integer
        Get
            SendRetries = m_SendRetries
        End Get
        Set(ByVal Value As Integer)
            m_SendRetries = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the message send retry count    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	233/04/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property GetRetries() As Integer
        Get
            GetRetries = m_GetRetries
        End Get
        Set(ByVal Value As Integer)
            m_GetRetries = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the delay between each message resend    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	23/04/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property ResendDelay() As Integer
        Get
            ResendDelay = m_ReSendDelay
        End Get
        Set(ByVal Value As Integer)
            m_ReSendDelay = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets or sets the destination environment name for the SAIB message header    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property DstEnvName() As String
        Get
            DstEnvName = m_EnvDst
        End Get
        Set(ByVal Value As String)
            m_EnvDst = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the last MessageID (MID) sent with the SAIB message header    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	06/09/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public ReadOnly Property SentMsgID() As String
        Get
            Return m_SentMsgID
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the last MessageID (MID) sent with the SAIB message header    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	06/09/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Property ReplyToQueue() As String
        Get
            Return m_ReplyToQueue
        End Get
        Set(ByVal Value As String)
            m_ReplyToQueue = Value
        End Set
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of the MQSeries class    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Before accessing any methods, Connect must be called with the desired Queue name     ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Sub New()
        Try
            'm_MQSess = New MQAX200.MQSession
            m_MQSess = CreateObject("MQAX200.MQSession")
        Catch
            If (m_MQSess Is Nothing) Then
                m_ErrNum = &H80006666
                m_ErrDsc = "Could not create an MQ Series session"
            Else
                m_ErrNum = m_MQSess.ReasonCode
                m_ErrDsc = m_MQSess.ReasonName
            End If
        End Try
        m_WksID = Trim(Environment.GetEnvironmentVariable("COMPUTERNAME"))
    End Sub


    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of the MQSeries class and connects to the specified queue manager name provided    ''' 
    ''' </summary>
    ''' <param name="QueueManagerName"></param>
    ''' <remarks>
    ''' If connection failed, MQSeries.ErrNum would be a non-zero value, otherwise, it is set to zero    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------

    Public Sub New(ByVal QueueManagerName As String)
        Try
            m_QueueManager = QueueManagerName
            'm_MQSess = New MQAX200.MQSession
            m_MQSess = CreateObject("MQAX200.MQSession")
            m_QMgr = m_MQSess.AccessQueueManager(QueueManagerName)
        Catch
            If (m_MQSess Is Nothing) Then
                m_ErrNum = &H80006666
                m_ErrDsc = "Could not create an MQ Series session"
            Else
                m_ErrNum = m_MQSess.ReasonCode
                m_ErrDsc = m_MQSess.ReasonName
            End If
        End Try
        m_WksID = Trim(Environment.GetEnvironmentVariable("COMPUTERNAME"))
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Connects to the specified QueueManagerName and open the specified Write and Read Queues    ''' 
    ''' </summary>
    ''' <param name="QueueManagerName"></param>
    ''' <param name="WriteQueueName"></param>
    ''' <param name="ReadQueueName"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' If connection failed, MQSeries.ErrNum would be a non-zero value, otherwise, it is set to zero    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function Connect(ByVal QueueManagerName As String, ByVal WriteQueueName As String, ByVal ReadQueueName As String) As Boolean
        Try
            m_QueueManager = QueueManagerName
            'm_MQSess = New MQAX200.MQSession
            m_QMgr = m_MQSess.AccessQueueManager(QueueManagerName)
            Return Connect(WriteQueueName, ReadQueueName)
        Catch
            If (m_MQSess Is Nothing) Then
                m_ErrNum = &H80006666
                m_ErrDsc = "Could not create an MQ Series session"
            Else
                m_ErrNum = m_MQSess.ReasonCode
                m_ErrDsc = m_MQSess.ReasonName
            End If
            m_Connected = False
            Return False
        End Try
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Connects to Queue Manager name given to New(QueueName) and open the Read and Write queues provided here   ''' 
    ''' </summary>
    ''' <param name="WriteQueueName"></param>
    ''' <param name="ReadQueueName"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' You must use the full Connect version if you initialized the MQ Series with New()    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function Connect(ByVal WriteQueueName As String, ByVal ReadQueueName As String) As Boolean
        Dim RetValue As Boolean
        m_ReadQueue = ReadQueueName
        m_WriteQueue = WriteQueueName
        If (m_ReplyToQueue = "") Then m_ReplyToQueue = ReadQueueName
        Try
            m_InputQueue = m_QMgr.AccessQueue(ReadQueueName, MQOO_INPUT_AS_Q_DEF)
            m_GetOptions = m_MQSess.AccessGetMessageOptions()
            m_GetOptions.Options = MQGMO_NO_SYNCPOINT
            m_GetOptions.MatchOptions = MQMO_MATCH_MSG_ID + MQMO_MATCH_CORREL_ID
            If m_DoWait Then m_GetOptions.Options += MQGMO_WAIT
            m_GetOptions.WaitInterval = m_TmeOut
            m_OutputQueue = m_QMgr.AccessQueue(WriteQueueName, MQOO_OUTPUT)
            m_PutOptions = m_MQSess.AccessPutMessageOptions()
            m_PutOptions.Options = MQPMO_NO_SYNCPOINT
            'm_PutOptions.UserID = m_LoggedUser
            RetValue = True
            m_ErrNum = 0
            m_ErrDsc = ""
        Catch ex As Exception
            If (m_MQSess Is Nothing) Then
                m_ErrNum = &H80006666
                m_ErrDsc = "Could not create an MQ Series session"
            Else
                If m_MQSess.CompletionCode <> MQCC_OK Then
                    m_ErrNum = m_MQSess.ReasonCode
                    m_ErrDsc = m_MQSess.ReasonName
                End If
            End If
            RetValue = False
        End Try

        m_Connected = RetValue
        Return RetValue

    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' ReConnects to Queue Manager name given to New(QueueName) and Connect and open the Read and Write queues provided ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' You must use the full Connect version if you initialized the MQ Series with New()    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	23/04/2005	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function ReConnect() As Boolean
        Return Connect(m_WriteQueue, m_ReadQueue)
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Disconnect from the Queue and flushes the data    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' To Reconnect, you can simply call the Connect(String,String) function    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function Disconnect() As Boolean
        m_Connected = False
        m_InputQueue = Nothing
        m_OutputQueue = Nothing
        m_GetOptions = Nothing
        m_PutOptions = Nothing
        Return True
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Reads a message from the Queue    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' The Message is not return to the caller, use ReadMessage to obtain the Message
    ''' Check MQSeries.ErrNum for success code. If succeeded, return value will be zero    ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function GetMessage(ByRef MsgObj As Object, Optional ByVal MsgID As String = "", Optional ByVal bWait As Boolean = False) As Integer

        If Not m_Connected Then
            m_ErrDsc = "Not Connected to Queue!"
            m_ErrNum = &H80001234
            Return Nothing
        End If

        Dim my_GetOptions As New Object
        Dim my_MQSess As New Object
        Dim RetVal As Integer
        my_GetOptions = m_GetOptions
        my_MQSess = m_MQSess
        my_GetOptions.Options = MQGMO_NO_SYNCPOINT
        If bWait Then my_GetOptions.Options += MQGMO_WAIT

        ''DoWait = bWait

        MsgObj = my_MQSess.AccessMessage()
        'GetMsg.Encoding = 546 'DefEnc_Windows = 546
        'GetMsg.Format = MQFMT_STRING
        my_MQSess.ExceptionThreshold = 3              '* process GetMsg errors in line
        If IsValidMid(MsgID) Then
            If MsgID.Length = 12 Then
                'm_GetMsg.MessageId = "12345678901234567890ABCD"
                MsgObj.MessageId = MsgID & MsgID  '24 characters
            End If
        End If
        m_InputQueue.Get(MsgObj, my_GetOptions)

        my_MQSess.ExceptionThreshold = 2

        m_ErrNum = my_MQSess.ReasonCode
        m_ErrDsc = my_MQSess.ReasonName
        RetVal = my_MQSess.ReasonCode
        ''Dim RetStr As String
        ''If my_MQSess.ReasonCode <> MQRC_NO_MSG_AVAILABLE Then
        ''    Dim rc As Char()
        ''    ReDim rc(MsgObj.DataLength - 1)
        ''    For i As Integer = 0 To rc.GetUpperBound(0)
        ''        rc(i) = Chr(MsgObj.ReadUnsignedByte())
        ''    Next
        ''    RetStr = rc
        ''    m_ErrNum = 0
        ''    m_ErrDsc = ""
        ''    Return RetStr
        ''End If

        my_GetOptions = Nothing
        my_MQSess = Nothing

        Return RetVal
    End Function


    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Returns a string represenation of the message read by MQSeries    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function ReadMessageByByte(ByRef MsgObj As Object) As String
        'Old function, reads one byte at time and attempts a transaltion
        'Clients linked to use ReadMessage will automatically use the newer and faster
        'function ReadMessageFast instead of this old implementation
        Dim RetStr As String
        Dim rc As Char()
        ReDim rc(MsgObj.DataLength - 1)
        For i As Integer = 0 To rc.Length - 1
            rc(i) = Chr(MsgObj.ReadUnsignedByte())
        Next
        RetStr = rc
        m_ErrNum = 0
        m_ErrDsc = ""
        Return RetStr
        'End If
        MsgObj = Nothing
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Returns a string represenation of the message read by MQSeries    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function ReadMessage(ByRef MsgObj As Object) As String
        'Provided for compatibility only
        Return ReadMessageFast(MsgObj)
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Returns a string represenation of the message read by MQSeries    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function ReadMessageFast(ByRef MsgObj As Object) As String
        Dim RetStr As String
        'If m_MQSess.ReasonCode <> MQRC_NO_MSG_AVAILABLE Then
        MsgObj.CharacterSet = 1256
        ' If attempted to read the whole DataLength using ReadString, then an exception will be raised
        'the reason behind this behavior remains unknown at this point.
        ' So a work around is to read DataLength-1, then read a single unsigned byte (which will be the
        'remaining byte in the buffer and cast it as a character and append it to the string.
        ' This workaround proved to have insignificant impact on performance, and thus it is accepted.
        RetStr = MsgObj.ReadString(MsgObj.DataLength - 1)
        RetStr &= Chr(MsgObj.ReadUnsignedByte())
        m_ErrNum = 0
        m_ErrDsc = ""
        Return RetStr
        'End If
        MsgObj = Nothing
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Function that builds an MQ message to be sent, but does not send it out, it is
    ''' used for logging purposes.
    ''' </summary>
    ''' <param name="MsgCode"></param>
    ''' <param name="MsgData"></param>
    ''' <param name="MsgID"></param>
    ''' <param name="SibOrgCls"></param>
    ''' <param name="CusNum"></param>
    ''' <param name="AccNum"></param>
    ''' <param name="CrdNum"></param>
    ''' <param name="ResVal"></param>
    ''' <param name="ActCde"></param>
    ''' <param name="SysCon"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[u910krzi]	03/03/2007	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function PrepareMessage(ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String, ByVal SibOrgCls As String, _
                                ByVal CusNum As String, ByVal AccNum As String, ByVal CrdNum As String, ByVal ResVal As String, ByVal ActCde As String, _
                                ByVal SysCon As String, ByVal IceTryFin As String, ByVal AutUsrIde As String, ByVal AutPacVal As String, _
                                ByVal AskMidRef As String) As String
        Dim TxtSndHeader, TxtMsgBody As String
        Try
            TxtSndHeader = BuildSndHeaderEx(MsgID, MsgCode, m_MsgSrc, m_EnvSrc, m_MsgDst, m_EnvDst, SibOrgCls, CusNum, AccNum, CrdNum, ResVal, ActCde, _
                                            SysCon, IceTryFin, AutUsrIde, AutPacVal, AskMidRef)
            TxtMsgBody = TxtSndHeader & MsgData
        Catch ex As Exception
            TxtMsgBody = Nothing
        End Try
        Return TxtMsgBody
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Write a message onto the Queue    ''' 
    ''' </summary>
    ''' <param name="MsgCode"></param>
    ''' <param name="MsgData"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Check MQSeries.ErrNum for success code. If succeeded, return value will be zero     ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function PutMessage(ByVal MsgCode As String, ByVal MsgData As String, Optional ByVal MsgID As String = "", _
                                Optional ByVal SibOrgCls As String = "GEN") As Boolean
        Return PutMessageEx(MsgCode, MsgData, MsgID, SibOrgCls, "", "", "")
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Overloaded function, used to maintain compatiblity with previous code
    ''' </summary>
    ''' <param name="MsgCode"></param>
    ''' <param name="MsgData"></param>
    ''' <param name="MsgID"></param>
    ''' <param name="SibOrgCls"></param>
    ''' <param name="CusNum"></param>
    ''' <param name="AccNum"></param>
    ''' <param name="CrdNum"></param>
    ''' <param name="ResVal"></param>
    ''' <param name="ActCde"></param>
    ''' <param name="SysCon"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[u910krzi]	03/03/2007	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function PutMessageEx(ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String, ByVal SibOrgCls As String, _
                                ByVal CusNum As String, ByVal AccNum As String, ByVal CrdNum As String, Optional ByVal ResVal As String = "000", _
                                Optional ByVal ActCde As String = "00", Optional ByVal SysCon As String = "000") As Boolean
        Dim dummy As String = Nothing
        Return PutMessageEx(MsgCode, MsgData, MsgID, SibOrgCls, CusNum, AccNum, CrdNum, dummy, ResVal, ActCde, SysCon)
    End Function


    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Write a message onto the Queue    ''' 
    ''' </summary>
    ''' <param name="MsgCode"></param>
    ''' <param name="MsgData"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Check MQSeries.ErrNum for success code. If succeeded, return value will be zero     ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function PutMessageEx(ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String, ByVal SibOrgCls As String, _
                                ByVal CusNum As String, ByVal AccNum As String, ByVal CrdNum As String, ByRef MsgDump As String, ByVal ResVal As String, _
                                ByVal ActCde As String, ByVal SysCon As String) As Boolean
        Return PutMessageFinancial(MsgCode, MsgData, MsgID, SibOrgCls, CusNum, AccNum, CrdNum, MsgDump, ResVal, ActCde, SysCon, "", "", "")
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Write a message onto the Queue    ''' 
    ''' </summary>
    ''' <param name="MsgCode"></param>
    ''' <param name="MsgData"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Check MQSeries.ErrNum for success code. If succeeded, return value will be zero     ''' 
    ''' </remarks>
    ''' <history>
    ''' 	[U910KRZI]	05/05/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function PutMessageFinancial(ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String, ByVal SibOrgCls As String, _
                                ByVal CusNum As String, ByVal AccNum As String, ByVal CrdNum As String, ByRef MsgDump As String, ByVal ResVal As String, _
                                ByVal ActCde As String, ByVal SysCon As String, ByVal IceTryFin As String, ByVal AutUsrIde As String, ByVal AutPacVal As String) As Boolean
        Dim my_PutMsg As Object
        Dim RetVal As Boolean = False

        If Not m_Connected Then
            m_ErrDsc = "Not Connected to Queue!"
            m_ErrNum = MQRC_NOT_OPEN
            Return False
        End If

        Dim TxtSndHeader As String
        Dim TxtMsgBody As String

        Try

            TxtSndHeader = BuildSndHeaderEx(MsgID, MsgCode, m_MsgSrc, m_EnvSrc, m_MsgDst, m_EnvDst, SibOrgCls, _
                                            CusNum, AccNum, CrdNum, ResVal, ActCde, SysCon, IceTryFin, AutUsrIde, AutPacVal)
            TxtMsgBody = TxtSndHeader & MsgData

            my_PutMsg = m_MQSess.AccessMessage
            my_PutMsg.ReplyToQueueManagerName = m_QueueManager '"MQ.GOLF"
            my_PutMsg.ReplyToQueueName = m_ReplyToQueue
            my_PutMsg.Encoding = 420 'DefEnc_Windows = 546
            my_PutMsg.Format = MQFMT_NONE 'MQFMT_STRING
            my_PutMsg.CharacterSet = 1256
            my_PutMsg.Expiry = CInt((m_TmeOut - 1000) / 100) '' Value is set to m_TmeOut - 1 sec
            my_PutMsg.MessageId = Nothing
            If IsValidMid(MsgID) Then
                If MsgID.Length = 12 Then
                    'm_PutMsg.MessageId = "12345678901234567890ABCD"
                    my_PutMsg.MessageId = MsgID & MsgID '24 characters
                End If
            End If

            my_PutMsg.WriteString(TxtMsgBody)
            MsgDump = TxtMsgBody
            m_OutputQueue.Put(my_PutMsg, m_PutOptions)
            m_SentMsgID = my_PutMsg.MessageId
            RetVal = True
        Catch ex As Exception
            RetVal = False
        End Try
        m_ErrNum = m_MQSess.ReasonCode
        m_ErrDsc = m_MQSess.ReasonName
        Return RetVal
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Function that posts a pre-prepared message (made using PrepareMessage function)
    ''' to the host.
    ''' </summary>
    ''' <param name="MsgID"></param>
    ''' <param name="MsgBody"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[u910krzi]	03/03/2007	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Function PutMessagePrepared(ByVal MsgID As String, ByVal MsgBody As String) As Boolean
        Dim my_PutMsg As Object
        Dim RetVal As Boolean = False

        If Not m_Connected Then
            m_ErrDsc = "Not Connected to Queue!"
            m_ErrNum = MQRC_NOT_OPEN
            Return False
        End If


        Try
            my_PutMsg = m_MQSess.AccessMessage
            my_PutMsg.ReplyToQueueManagerName = m_QueueManager '"MQ.GOLF"
            my_PutMsg.ReplyToQueueName = m_ReplyToQueue
            my_PutMsg.Encoding = 420 'DefEnc_Windows = 546
            my_PutMsg.Format = MQFMT_NONE 'MQFMT_STRING
            my_PutMsg.CharacterSet = 1256
            my_PutMsg.Expiry = CInt((m_TmeOut - 1000) / 100) '' Value is set to m_TmeOut - 1 sec
            my_PutMsg.MessageId = Nothing
            If IsValidMid(MsgID) Then
                If MsgID.Length = 12 Then
                    'm_PutMsg.MessageId = "12345678901234567890ABCD"
                    my_PutMsg.MessageId = MsgID & MsgID '24 characters
                End If
            End If

            my_PutMsg.WriteString(MsgBody)
            m_OutputQueue.Put(my_PutMsg, m_PutOptions)
            m_SentMsgID = my_PutMsg.MessageId
            RetVal = True
        Catch ex As Exception
            RetVal = False
        End Try
        m_ErrNum = m_MQSess.ReasonCode
        m_ErrDsc = m_MQSess.ReasonName
        Return RetVal
    End Function

    Public Function PutMessageRaw1(ByVal MsgData As String, Optional ByVal MsgID As String = "", Optional ByVal ReplyQueue As String = "", Optional ByVal CorlID As String = "") As Boolean
        Dim my_PutMsg As Object
        Dim RetVal As Boolean = False

        If Not m_Connected Then
            m_ErrDsc = "Not Connected to Queue!"
            m_ErrNum = MQRC_NOT_OPEN
            Return False
        End If

        Try
            my_PutMsg = m_MQSess.AccessMessage
            my_PutMsg.ReplyToQueueManagerName = m_QueueManager '"MQ.GOLF"
            my_PutMsg.ReplyToQueueName = CStr(IIf(ReplyQueue = "", m_ReplyToQueue, ReplyQueue))
            my_PutMsg.Encoding = 420 'DefEnc_Windows = 546
            my_PutMsg.Format = MQFMT_NONE
            my_PutMsg.CharacterSet = 1256
            my_PutMsg.Expiry = CInt((m_TmeOut - 1000) / 100) '' Value is set to m_TmeOut - 1 sec
            my_PutMsg.MessageId = Nothing
            If IsValidMid(MsgID) Then
                If MsgID.Length = 12 Then
                    'm_PutMsg.MessageId = "12345678901234567890ABCD"
                    my_PutMsg.MessageId = MsgID & MsgID '24 characters
                End If
            End If

            If Trim(CorlID) = "" Then
                my_PutMsg.CorrelationId = MsgID
            Else
                my_PutMsg.CorrelationId = PackString(CorlID, 24)
            End If

            my_PutMsg.WriteString(MsgData)
            m_OutputQueue.Put(my_PutMsg, m_PutOptions)
            m_SentMsgID = my_PutMsg.MessageId
            RetVal = True
        Catch ex As Exception
            RetVal = False
        End Try
        m_ErrNum = m_MQSess.ReasonCode
        m_ErrDsc = m_MQSess.ReasonName
        Return RetVal
    End Function

    Public Function PutMessageInQueueEx(ByVal WriteQueueName As String, _
                                ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String, ByVal SibOrgCls As String, _
                                ByVal CusNum As String, ByVal AccNum As String, ByVal CrdNum As String, Optional ByVal ResVal As String = "000", _
                                Optional ByVal ActCde As String = "00", Optional ByVal SysCon As String = "000", _
                                Optional ByVal ReplyQueue As String = "", Optional ByVal CorlID As String = "") As Boolean
        Dim TxtSndHeader As String
        Dim TxtMsgBody As String

        TxtSndHeader = BuildSndHeaderEx(MsgID, MsgCode, m_MsgSrc, m_EnvSrc, m_MsgDst, m_EnvDst, SibOrgCls, CusNum, AccNum, CrdNum, ResVal, ActCde, SysCon)
        TxtMsgBody = TxtSndHeader & MsgData

        Return PutMessageInQueue(WriteQueueName, TxtMsgBody, MsgID, ReplyQueue, CorlID)
    End Function

    Public Function PutMessageInQueue(ByVal WriteQueueName As String, ByVal MsgData As String, _
                                                    Optional ByVal MsgID As String = "", Optional ByVal ReplyQueue As String = "", _
                                                    Optional ByVal CorlID As String = "") As Boolean
        Dim lOutQueue As Object
        Dim lPutOptions As Object
        Dim my_PutMsg As Object
        Dim RetVal As Boolean = False

        Try
            lOutQueue = m_QMgr.AccessQueue(WriteQueueName, MQOO_OUTPUT)
            lPutOptions = m_MQSess.AccessPutMessageOptions()
            lPutOptions.Options = MQPMO_NO_SYNCPOINT
            my_PutMsg = m_MQSess.AccessMessage
            my_PutMsg.ReplyToQueueManagerName = m_QueueManager '"MQ.GOLF"
            my_PutMsg.ReplyToQueueName = CStr(IIf(ReplyQueue = "", m_ReplyToQueue, ReplyQueue))
            my_PutMsg.Encoding = 420 'DefEnc_Windows = 546
            my_PutMsg.Format = MQFMT_NONE
            my_PutMsg.CharacterSet = 1256
            my_PutMsg.Expiry = CInt((m_TmeOut - 1000) / 100) '' Value is set to m_TmeOut - 1 sec
            my_PutMsg.MessageId = Nothing
            If IsValidMid(MsgID) Then
                If MsgID.Length = 12 Then
                    'm_PutMsg.MessageId = "12345678901234567890ABCD"
                    my_PutMsg.MessageId = MsgID & MsgID '24 characters
                Else
                    my_PutMsg.MessageId = PackString(MsgID, 24)
                End If
            End If

            If Trim(CorlID) = "" Then
                my_PutMsg.CorrelationId = PackString(MsgID, 24)
            Else
                my_PutMsg.CorrelationId = PackString(CorlID, 24)
            End If

            my_PutMsg.WriteString(MsgData)
            lOutQueue.Put(my_PutMsg, lPutOptions)
            m_SentMsgID = my_PutMsg.MessageId
            RetVal = True
        Catch ex As Exception
            RetVal = False
        End Try
        m_ErrNum = m_MQSess.ReasonCode
        m_ErrDsc = m_MQSess.ReasonName
        Return RetVal
    End Function

    Public Function PutMessage83(ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String) As Boolean

        Dim my_PutMsg As Object
        Dim RetVal As Boolean = False

        If Not m_Connected Then
            m_ErrDsc = "Not Connected to Queue!"
            m_ErrNum = MQRC_NOT_OPEN
            Return False
        End If

        Dim TxtSndHeader As String
        Dim TxtMsgBody As String

        Try
            TxtSndHeader = BuildSndHeader83(MsgID, MsgCode, m_MsgSrc, m_MsgDst)
            TxtMsgBody = TxtSndHeader & MsgData


            my_PutMsg = m_MQSess.AccessMessage
            my_PutMsg.ReplyToQueueManagerName = m_QueueManager '"MQ.GOLF"
            my_PutMsg.ReplyToQueueName = m_ReplyToQueue
            my_PutMsg.Encoding = 420 'DefEnc_Windows = 546
            my_PutMsg.Format = MQFMT_NONE
            my_PutMsg.CharacterSet = 1256
            my_PutMsg.Expiry = CInt((m_TmeOut - 1000) / 100) '' Value is set to m_TmeOut - 1 sec
            my_PutMsg.MessageId = Nothing
            If IsValidMid(MsgID) Then
                If MsgID.Length = 12 Then
                    'm_PutMsg.MessageId = "12345678901234567890ABCD"
                    my_PutMsg.MessageId = MsgID & MsgID '24 characters
                End If
            End If

            my_PutMsg.WriteString(TxtMsgBody)
            m_OutputQueue.Put(my_PutMsg, m_PutOptions)
            m_SentMsgID = my_PutMsg.MessageId
            RetVal = True
        Catch ex As Exception
            RetVal = False
        End Try
        m_ErrNum = m_MQSess.ReasonCode
        m_ErrDsc = m_MQSess.ReasonName
        Return RetVal
    End Function

    Public Function PutMessage93(ByVal MsgCode As String, ByVal MsgData As String, ByVal MsgID As String, ByVal ResVal As String, _
                                ByVal ActCde As String, ByVal SysCon As String) As Boolean

        Dim my_PutMsg As Object
        Dim RetVal As Boolean = False

        If Not m_Connected Then
            m_ErrDsc = "Not Connected to Queue!"
            m_ErrNum = MQRC_NOT_OPEN
            Return False
        End If

        Dim TxtSndHeader As String
        Dim TxtMsgBody As String

        Try
            TxtSndHeader = BuildSndHeader93(MsgID, MsgCode, m_MsgSrc, m_MsgDst, ResVal, ActCde, SysCon)
            TxtMsgBody = TxtSndHeader & MsgData


            my_PutMsg = m_MQSess.AccessMessage
            my_PutMsg.ReplyToQueueManagerName = m_QueueManager '"MQ.GOLF"
            my_PutMsg.ReplyToQueueName = m_ReplyToQueue
            my_PutMsg.Encoding = 420 'DefEnc_Windows = 546
            my_PutMsg.Format = MQFMT_NONE
            my_PutMsg.CharacterSet = 1256
            my_PutMsg.Expiry = CInt((m_TmeOut - 1000) / 100) '' Value is set to m_TmeOut - 1 sec
            my_PutMsg.MessageId = Nothing
            If IsValidMid(MsgID) Then
                If MsgID.Length = 12 Then
                    'm_PutMsg.MessageId = "12345678901234567890ABCD"
                    my_PutMsg.MessageId = MsgID & MsgID '24 characters
                End If
            End If

            my_PutMsg.WriteString(TxtMsgBody)
            m_OutputQueue.Put(my_PutMsg, m_PutOptions)
            m_SentMsgID = my_PutMsg.MessageId
            RetVal = True
        Catch ex As Exception
            RetVal = False
        End Try
        m_ErrNum = m_MQSess.ReasonCode
        m_ErrDsc = m_MQSess.ReasonName
        Return RetVal
    End Function

    Public Function PackString(ByVal prmStr As String, ByVal prmLen As Integer) As String
        Dim RetStr As Char()
        Dim i As Integer

        If prmStr Is Nothing Then Return Space(prmLen)
        If prmStr.Length >= prmLen Then Return Left(prmStr, prmLen)

        ReDim RetStr(prmLen - 1)
        For i = 0 To prmStr.Length - 1
            RetStr(i) = prmStr.Chars(i)
        Next
        For i = prmStr.Length To prmLen - 1
            RetStr(i) = " "
        Next
        Return RetStr
    End Function

    Private Function PackHeader123(ByVal SibHeader As SibHdrStruct) As String
        Dim RetStr As String
        Try
            With SibHeader
                RetStr = PackString(.MsgProCde, 6)
                RetStr += PackString(.MsgMid, 12)
                RetStr += PackString(.MsgStp, 14)
                RetStr += PackString(.MsgSrc, 4)
                RetStr += PackString(.MsgSrcEnv, 3)
                RetStr += PackString(.MsgSrcRsv, 3)
                RetStr += PackString(.MsgUsrIde, 32)
                RetStr += PackString(.MsgTgt, 4)
                RetStr += PackString(.MsgTgtEnv, 3)
                RetStr += PackString(.MsgTgtRsv, 2)
                RetStr += PackString(.MsgResVal, 3)
                RetStr += PackString(.MsgActCde, 2)
                RetStr += PackString(.MsgSysCon, 3)
                RetStr += PackString(.Rsv1b, 1)
                RetStr += PackString(.MsgResInd, 1)
                RetStr += PackString(.SibResCde, 7)
                RetStr += PackString(.SifRefNum, 12)
                RetStr += PackString(.SibEnvUnt, 3)
                RetStr += PackString(.SibOrgCls, 3)
                ' SibKeyLoc, 84
                RetStr += PackString(.SibKeyCls, 3)
                RetStr += PackString(.SibCus, 6)
                RetStr += PackString(.SibAcc, 13)
                RetStr += PackString(.SibVar, 20)
                '---RetStr += PackString(.SibDea, 20)
                '---RetStr += PackString(.SibRetIde, 12)
                '---RetStr += PackString(.SibTrmIde, 4)
                '---RetStr += PackString(.SibCrdNum, 16)
                RetStr += PackString(.MsiDat, 10)
                RetStr += PackString(.Rsv21, 10)
                RetStr += PackString(.CstVerTab, 12)
                RetStr += PackString(.SibCusSeg, 1)
                RetStr += PackString(.Rsv2, 9)
                'SubStruct:SibNonRep	Size:50
                'RetStr += PackString("0303", 4) 
                RetStr += PackString(m_BrnCde, 4)
                RetStr += PackString(.Rsv415, 45)
                RetStr += PackString("I", 1)  'RetStr += PackString(.SibNrdTyp, 1)
                'rsv3
                RetStr += PackString(.Rsv3, 4)
            End With
        Catch
            RetStr = Space(Len(SibHeader))
        End Try
        Return RetStr

    End Function

    Private Function PackHeader83(ByVal SibHeader As SibHdrStruct83) As String
        Dim RetStr As String
        Try
            With SibHeader
                RetStr = PackString(.MsgProCde, 6)
                RetStr += PackString(.MsgMid, 12)
                RetStr += PackString(.MsgStp, 14)
                RetStr += PackString(.MsgOrg, 10)
                RetStr += PackString(.MsgUsrIde, 32)
                RetStr += PackString(.MsgDst, 9)
            End With
        Catch
            RetStr = Space(Len(SibHeader))
        End Try
        Return RetStr
    End Function

    Private Function PackHeader93(ByVal SibHeader As SibHdrStruct93) As String
        Dim RetStr As String
        Try
            With SibHeader
                RetStr = PackString(.MsgProCde, 6)
                RetStr += PackString(.MsgMid, 12)
                RetStr += PackString(.MsgStp, 14)
                RetStr += PackString(.MsgOrg, 10)
                RetStr += PackString(.MsgUsrIde, 32)
                RetStr += PackString(.MsgDst, 9)
                RetStr += PackString(.MsgResVal, 3)
                RetStr += PackString(.MsgActCde, 2)
                RetStr += PackString(.MsgSysCon, 3)
                RetStr += PackString(.Rsv1, 2)
            End With
        Catch
            RetStr = Space(Len(SibHeader))
        End Try
        Return RetStr
    End Function

    Private Function ToHexID(ByVal DecID As String) As String
        Dim tt As String = "6963653A"
        For i As Integer = 0 To DecID.Length - 1
            tt += "3" & DecID.Chars(i)
        Next
        Return tt
    End Function

    Private Function US_FormatDate(ByVal pDtpDate As DateTime, ByVal pFormatString As String) As String
        Dim MyResult As String
        Dim OldCultInfo, NewCultInfo As CultureInfo
        '* Use English US locale
        OldCultInfo = System.Threading.Thread.CurrentThread.CurrentCulture
        NewCultInfo = New CultureInfo("en-US", False)
        System.Threading.Thread.CurrentThread.CurrentCulture = NewCultInfo

        MyResult = Format(pDtpDate, pFormatString)
        System.Threading.Thread.CurrentThread.CurrentCulture = OldCultInfo
        NewCultInfo = Nothing
        Return MyResult
    End Function

    Private Function BuildSndHeader(ByVal MsgId As String, ByVal pTrxCde As String, ByVal pMsgSrc As String, _
                                    ByVal pSrcEnv As String, ByVal pMsgDst As String, ByVal pDstEnv As String, _
                                    Optional ByVal SibOrgCls As String = "GEN") As String
        Return BuildSndHeaderEx(MsgId, pTrxCde, pMsgSrc, pSrcEnv, pMsgDst, pDstEnv, SibOrgCls, "", "", "", "000", "00", "000")
    End Function

    Private Function BuildSndHeaderEx(ByVal MsgId As String, ByVal pTrxCde As String, ByVal pMsgSrc As String, _
                                    ByVal pSrcEnv As String, ByVal pMsgDst As String, ByVal pDstEnv As String, _
                                    ByVal SibOrgCls As String, ByVal CusNum As String, ByVal AccNum As String, ByVal CrdNum As String, _
                                    ByVal ResVal As String, ByVal ActCde As String, ByVal SysCon As String, _
                                    Optional ByVal IceTryFin As String = "", _
                                    Optional ByVal AutUsrIde As String = "", Optional ByVal AutPacVal As String = "", _
                                    Optional ByVal AskMidRef As String = "") As String
        Dim SibHdr As New SibHdrStruct
        'Dim TmpStr As String
        Try
            ErrClr()
            With SibHdr
                .MsgProCde = pTrxCde
                'TmpStr = "000000000000" & Replace(CStr(CLng(Now.ToOADate)), ".", "") & Replace(CStr(VB6.Format(CDbl(VB.Timer()), "#0.000")), ".", "") '& "000000000000"
                .MsgMid = MsgId 'Right(TmpStr, 12)
                .MsgStp = US_FormatDate(Now, "yyyyMMddHHmmss") '& Format(Timer - Int(Timer), ".000")
                .MsgSrc = pMsgSrc
                .MsgSrcEnv = pSrcEnv
                .MsgSrcRsv = "   "
                .MsgUsrIde = m_LoggedUser & ";" & m_LoggedUser
                .MsgTgt = pMsgDst
                .MsgTgtEnv = pDstEnv
                .MsgTgtRsv = "  "
                .MsgResVal = ResVal
                .MsgActCde = ActCde
                .MsgSysCon = SysCon
                .SibOrgCls = SibOrgCls
                .SibKeyCls = SibOrgCls
                .SibCus = CusNum
                .SibAcc = AccNum
                .SibVar = "    " & CrdNum
                '.SibCrdNum = CrdNum
                .IceTryFin = IceTryFin
                .AutUsrIde = AutUsrIde
                .AutPacVal = AutPacVal
                .AskMidRef = AskMidRef
            End With
            Return (SibHdr.PackHeader(Me))
        Catch ex As Exception
            m_ErrNum = &H80001234
            m_ErrDsc = ex.Message
        End Try
        Return (SibHdr.PackHeader(Me))
    End Function

    Private Function BuildSndHeader83(ByVal MsgId As String, ByVal pTrxCde As String, ByVal pMsgSrc As String, _
                                    ByVal pMsgDst As String) As String
        Dim SibHdr As New SibHdrStruct83
        ' Dim TmpStr As String
        Try
            ErrClr()
            With SibHdr
                .MsgProCde = pTrxCde
                'TmpStr = "000000000000" & Replace(CStr(CLng(Now.ToOADate)), ".", "") & Replace(CStr(VB6.Format(CDbl(VB.Timer()), "#0.000")), ".", "") '& "000000000000"
                .MsgMid = MsgId 'Right(TmpStr, 12)
                .MsgStp = US_FormatDate(Now, "yyyyMMddHHmmss") '& Format(Timer - Int(Timer), ".000")
                .MsgOrg = pMsgSrc
                .MsgUsrIde = m_LoggedUser & ";" & m_LoggedUser
                .MsgDst = pMsgDst
            End With
            Return (PackHeader83(SibHdr))
        Catch ex As Exception
            m_ErrNum = &H80001234
            m_ErrDsc = ex.Message
        End Try
        Return (PackHeader83(SibHdr))
    End Function

    Private Function BuildSndHeader93(ByVal MsgId As String, ByVal pTrxCde As String, ByVal pMsgSrc As String, _
                                    ByVal pMsgDst As String, ByVal ResVal As String, ByVal ActCde As String, _
                                    ByVal SysCon As String) As String
        Dim SibHdr As New SibHdrStruct93
        'Dim TmpStr As String
        Try
            ErrClr()
            With SibHdr
                .MsgProCde = pTrxCde
                'TmpStr = "000000000000" & Replace(CStr(CLng(Now.ToOADate)), ".", "") & Replace(CStr(VB6.Format(CDbl(VB.Timer()), "#0.000")), ".", "") '& "000000000000"
                .MsgMid = MsgId 'Right(TmpStr, 12)
                .MsgStp = US_FormatDate(Now, "yyyyMMddHHmmss") '& Format(Timer - Int(Timer), ".000")
                .MsgOrg = pMsgSrc
                .MsgUsrIde = m_LoggedUser & ";" & m_LoggedUser
                .MsgDst = pMsgDst
                .MsgResVal = ResVal
                .MsgActCde = ActCde
                .MsgSysCon = SysCon
                .Rsv1 = Space(100)
            End With
            Return (PackHeader93(SibHdr))
        Catch ex As Exception
            m_ErrNum = &H80001234
            m_ErrDsc = ex.Message
        End Try
        Return (PackHeader93(SibHdr))
    End Function

End Class
