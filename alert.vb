Imports System.Threading
Imports System.Globalization
Imports SIBL0100
Imports System.Data.SqlClient


Public Class frmAlertServices
    Inherits System.Windows.Forms.Form

    Public CusNum As String
    Public MsgRspDat As M708031_AlertServices_Struct
    Dim MsgSndDat As M708040_AlertServices_Struct
    Public m_CusDat As frmCustomer.CusDta_Struct
    Private m_TrackChanges As New UI.ControlUtil.ContainerStateTracker
    Friend WithEvents txtEmailAddressOld As System.Windows.Forms.TextBox
    Friend WithEvents cmbPreferedLanguageOld As System.Windows.Forms.ComboBox
    Friend WithEvents cmbStatusOld As System.Windows.Forms.ComboBox
    Private connectionString As String = ""
    Friend WithEvents txtMobilePhoneOld As System.Windows.Forms.TextBox
    Friend WithEvents txtMobilePhone2Old As System.Windows.Forms.ComboBox
    Friend WithEvents lblOldRegistrationStatus As System.Windows.Forms.Label
    Friend WithEvents lblOldMobilePhone As System.Windows.Forms.Label
    Friend WithEvents lblOldEmailAddress As System.Windows.Forms.Label
    Friend WithEvents lblOldPreferedLanguage As System.Windows.Forms.Label
    Friend WithEvents btnReject As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Private Result As DialogResult = DialogResult.Cancel

#Region " Windows Form Designer generated code "

    Public Sub New(ByRef CusDat As frmCustomer.CusDta_Struct)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()
        If AppInstance.IsLoggedOn Then
            If AppInstance.Unit = "PRD" Then
                connectionString = EncryptionHelper.Decrypt("F5o4Y/Jnk7D3aY1piSxM/d8654Msi4xgx/cStAlMWiAumKsjPP7BZxiPPVAyn9ShFo3ZRBcgmRksegqmMh7PVOX4U7VLcsO2vyUIFhWKPaqOro0nyDp425hqSilKHAbFxH8tK4UHptbYIfMKgQYqMfRSFeEtt3WD27I2/Y8+rI0=")
            ElseIf AppInstance.Unit = "QRD" Then
                connectionString = EncryptionHelper.Decrypt("F5o4Y/Jnk7D3aY1piSxM/cvU9VhwDX1XsyXQ1O8NOe0+5VduLWL5UzqFca2S/+w2zSM1Aobv7hofEUFp/jzptHXVJyn4DwWyYgjkYX2GmzAsLUi7u9XNQEP47N0cbZ+2FeSUr77QWhOOiIT6REz7EqBjweyc2L2LveR4ws0rDuU=")
            Else
                connectionString = EncryptionHelper.Decrypt("F5o4Y/Jnk7D3aY1piSxM/d5RLald5MVB6tDTaP40KxOWURcP8m5Hr/u6IDmcKwpMZWbiRP7TxvSFjDSfhduGBmdbZLCHOhFp+F+X415bYWLHpa+FFnQzSrjPZ3lUJ7Bz")
            End If
        End If
        'Add any initialization after the InitializeComponent() call
        m_CusDat = CusDat
        AddHandler Me.txtMobilePhone.KeyPress, AddressOf UI.Controls.Control.Event_Text_PhoneNumber_KeyPress
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents btnPrint As System.Windows.Forms.Button
    Friend WithEvents btnModify As System.Windows.Forms.Button
    Friend WithEvents btnPreferences As System.Windows.Forms.Button
    Friend WithEvents txtLastModification As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents cmbPreferedLanguage As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtEmailAddress As System.Windows.Forms.TextBox
    Friend WithEvents txtMobilePhone As System.Windows.Forms.TextBox
    Friend WithEvents chkAlertEmail As System.Windows.Forms.CheckBox
    Friend WithEvents chkAlertMobile As System.Windows.Forms.CheckBox
    Friend WithEvents txtCusNum As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents cmbStatus As System.Windows.Forms.ComboBox
    Friend WithEvents btnSubmit As System.Windows.Forms.Button
    Friend WithEvents pnlButtons As System.Windows.Forms.Panel
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents txtCusNam As System.Windows.Forms.TextBox
    Friend WithEvents chkEmlAdd_UpdAll As System.Windows.Forms.CheckBox
    Friend WithEvents chkMobPhn_UpdAll As System.Windows.Forms.CheckBox
    Friend WithEvents grpAlerts As System.Windows.Forms.GroupBox
    Friend WithEvents btnTstMob As System.Windows.Forms.Button
    Friend WithEvents btnTstEml As System.Windows.Forms.Button
    Friend WithEvents btnTstMsg As System.Windows.Forms.Button
    Friend WithEvents txtMobilePhone2 As System.Windows.Forms.ComboBox

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.btnPrint = New System.Windows.Forms.Button()
        Me.btnSubmit = New System.Windows.Forms.Button()
        Me.btnModify = New System.Windows.Forms.Button()
        Me.btnPreferences = New System.Windows.Forms.Button()
        Me.txtLastModification = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.cmbPreferedLanguage = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtEmailAddress = New System.Windows.Forms.TextBox()
        Me.txtMobilePhone = New System.Windows.Forms.TextBox()
        Me.chkAlertEmail = New System.Windows.Forms.CheckBox()
        Me.chkAlertMobile = New System.Windows.Forms.CheckBox()
        Me.txtCusNum = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbStatus = New System.Windows.Forms.ComboBox()
        Me.pnlButtons = New System.Windows.Forms.Panel()
        Me.btnTstMsg = New System.Windows.Forms.Button()
        Me.btnReject = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.grpAlerts = New System.Windows.Forms.GroupBox()
        Me.lblOldPreferedLanguage = New System.Windows.Forms.Label()
        Me.lblOldEmailAddress = New System.Windows.Forms.Label()
        Me.lblOldMobilePhone = New System.Windows.Forms.Label()
        Me.txtMobilePhoneOld = New System.Windows.Forms.TextBox()
        Me.txtEmailAddressOld = New System.Windows.Forms.TextBox()
        Me.cmbPreferedLanguageOld = New System.Windows.Forms.ComboBox()
        Me.txtMobilePhone2 = New System.Windows.Forms.ComboBox()
        Me.btnTstEml = New System.Windows.Forms.Button()
        Me.btnTstMob = New System.Windows.Forms.Button()
        Me.chkMobPhn_UpdAll = New System.Windows.Forms.CheckBox()
        Me.chkEmlAdd_UpdAll = New System.Windows.Forms.CheckBox()
        Me.txtMobilePhone2Old = New System.Windows.Forms.ComboBox()
        Me.txtCusNam = New System.Windows.Forms.TextBox()
        Me.cmbStatusOld = New System.Windows.Forms.ComboBox()
        Me.lblOldRegistrationStatus = New System.Windows.Forms.Label()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.pnlButtons.SuspendLayout()
        Me.grpAlerts.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(360, 0)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(80, 23)
        Me.btnClose.TabIndex = 4
        Me.btnClose.Text = "&Close"
        '
        'btnPrint
        '
        Me.btnPrint.Location = New System.Drawing.Point(272, 0)
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.Size = New System.Drawing.Size(80, 23)
        Me.btnPrint.TabIndex = 3
        Me.btnPrint.Text = "&Print"
        '
        'btnSubmit
        '
        Me.btnSubmit.Location = New System.Drawing.Point(90, 0)
        Me.btnSubmit.Name = "btnSubmit"
        Me.btnSubmit.Size = New System.Drawing.Size(80, 23)
        Me.btnSubmit.TabIndex = 1
        Me.btnSubmit.Text = "&Submit"
        '
        'btnModify
        '
        Me.btnModify.Location = New System.Drawing.Point(0, 0)
        Me.btnModify.Name = "btnModify"
        Me.btnModify.Size = New System.Drawing.Size(80, 23)
        Me.btnModify.TabIndex = 0
        Me.btnModify.Text = "&Modify"
        '
        'btnPreferences
        '
        Me.btnPreferences.Location = New System.Drawing.Point(176, 0)
        Me.btnPreferences.Name = "btnPreferences"
        Me.btnPreferences.Size = New System.Drawing.Size(88, 23)
        Me.btnPreferences.TabIndex = 2
        Me.btnPreferences.Text = "P&references..."
        '
        'txtLastModification
        '
        Me.txtLastModification.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtLastModification.Location = New System.Drawing.Point(112, 283)
        Me.txtLastModification.Name = "txtLastModification"
        Me.txtLastModification.ReadOnly = True
        Me.txtLastModification.Size = New System.Drawing.Size(400, 20)
        Me.txtLastModification.TabIndex = 6
        '
        'Label10
        '
        Me.Label10.Location = New System.Drawing.Point(16, 282)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(104, 16)
        Me.Label10.TabIndex = 57
        Me.Label10.Text = "Last Modification"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'cmbPreferedLanguage
        '
        Me.cmbPreferedLanguage.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.cmbPreferedLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPreferedLanguage.Items.AddRange(New Object() {"Arabic", "English"})
        Me.cmbPreferedLanguage.Location = New System.Drawing.Point(104, 140)
        Me.cmbPreferedLanguage.Name = "cmbPreferedLanguage"
        Me.cmbPreferedLanguage.Size = New System.Drawing.Size(121, 21)
        Me.cmbPreferedLanguage.TabIndex = 5
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(6, 141)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(104, 16)
        Me.Label6.TabIndex = 55
        Me.Label6.Text = "Prefered Language"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(264, 24)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(136, 16)
        Me.Label5.TabIndex = 54
        Me.Label5.Text = "Saudi Arabian mobile only"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.BottomRight
        '
        'txtEmailAddress
        '
        Me.txtEmailAddress.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtEmailAddress.Location = New System.Drawing.Point(104, 82)
        Me.txtEmailAddress.Name = "txtEmailAddress"
        Me.txtEmailAddress.Size = New System.Drawing.Size(296, 20)
        Me.txtEmailAddress.TabIndex = 3
        '
        'txtMobilePhone
        '
        Me.txtMobilePhone.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtMobilePhone.Location = New System.Drawing.Point(104, 24)
        Me.txtMobilePhone.Name = "txtMobilePhone"
        Me.txtMobilePhone.Size = New System.Drawing.Size(144, 20)
        Me.txtMobilePhone.TabIndex = 0
        '
        'chkAlertEmail
        '
        Me.chkAlertEmail.Location = New System.Drawing.Point(8, 84)
        Me.chkAlertEmail.Name = "chkAlertEmail"
        Me.chkAlertEmail.Size = New System.Drawing.Size(112, 16)
        Me.chkAlertEmail.TabIndex = 2
        Me.chkAlertEmail.Text = "Email address"
        '
        'chkAlertMobile
        '
        Me.chkAlertMobile.Location = New System.Drawing.Point(8, 26)
        Me.chkAlertMobile.Name = "chkAlertMobile"
        Me.chkAlertMobile.Size = New System.Drawing.Size(104, 16)
        Me.chkAlertMobile.TabIndex = 0
        Me.chkAlertMobile.Text = "Mobile Phone"
        '
        'txtCusNum
        '
        Me.txtCusNum.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtCusNum.Location = New System.Drawing.Point(112, 8)
        Me.txtCusNum.Name = "txtCusNum"
        Me.txtCusNum.Size = New System.Drawing.Size(48, 20)
        Me.txtCusNum.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(16, 8)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(104, 17)
        Me.Label1.TabIndex = 64
        Me.Label1.Text = "Customer Number"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(15, 36)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(104, 16)
        Me.Label2.TabIndex = 67
        Me.Label2.Text = "Registration Status"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'cmbStatus
        '
        Me.cmbStatus.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbStatus.Items.AddRange(New Object() {"Registered in the service", "Deregistered from the service"})
        Me.cmbStatus.Location = New System.Drawing.Point(113, 35)
        Me.cmbStatus.Name = "cmbStatus"
        Me.cmbStatus.Size = New System.Drawing.Size(232, 21)
        Me.cmbStatus.TabIndex = 3
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnTstMsg)
        Me.pnlButtons.Controls.Add(Me.btnPreferences)
        Me.pnlButtons.Controls.Add(Me.btnSubmit)
        Me.pnlButtons.Controls.Add(Me.btnPrint)
        Me.pnlButtons.Controls.Add(Me.btnClose)
        Me.pnlButtons.Controls.Add(Me.btnReject)
        Me.pnlButtons.Controls.Add(Me.btnModify)
        Me.pnlButtons.Controls.Add(Me.btnSave)
        Me.pnlButtons.Location = New System.Drawing.Point(8, 329)
        Me.pnlButtons.Name = "pnlButtons"
        Me.pnlButtons.Size = New System.Drawing.Size(592, 24)
        Me.pnlButtons.TabIndex = 0
        '
        'btnTstMsg
        '
        Me.btnTstMsg.Location = New System.Drawing.Point(448, 0)
        Me.btnTstMsg.Name = "btnTstMsg"
        Me.btnTstMsg.Size = New System.Drawing.Size(144, 24)
        Me.btnTstMsg.TabIndex = 5
        Me.btnTstMsg.Text = "Recent Test Messages ..."
        '
        'btnReject
        '
        Me.btnReject.Location = New System.Drawing.Point(0, 1)
        Me.btnReject.Name = "btnReject"
        Me.btnReject.Size = New System.Drawing.Size(80, 23)
        Me.btnReject.TabIndex = 6
        Me.btnReject.Text = "Reject"
        '
        'btnSave
        '
        Me.btnSave.Enabled = False
        Me.btnSave.Location = New System.Drawing.Point(89, 1)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(80, 23)
        Me.btnSave.TabIndex = 7
        Me.btnSave.Text = "Save"
        '
        'grpAlerts
        '
        Me.grpAlerts.Controls.Add(Me.lblOldPreferedLanguage)
        Me.grpAlerts.Controls.Add(Me.lblOldEmailAddress)
        Me.grpAlerts.Controls.Add(Me.lblOldMobilePhone)
        Me.grpAlerts.Controls.Add(Me.txtMobilePhoneOld)
        Me.grpAlerts.Controls.Add(Me.txtEmailAddressOld)
        Me.grpAlerts.Controls.Add(Me.cmbPreferedLanguageOld)
        Me.grpAlerts.Controls.Add(Me.txtMobilePhone)
        Me.grpAlerts.Controls.Add(Me.txtMobilePhone2)
        Me.grpAlerts.Controls.Add(Me.btnTstEml)
        Me.grpAlerts.Controls.Add(Me.btnTstMob)
        Me.grpAlerts.Controls.Add(Me.Label5)
        Me.grpAlerts.Controls.Add(Me.chkAlertMobile)
        Me.grpAlerts.Controls.Add(Me.txtEmailAddress)
        Me.grpAlerts.Controls.Add(Me.chkAlertEmail)
        Me.grpAlerts.Controls.Add(Me.chkMobPhn_UpdAll)
        Me.grpAlerts.Controls.Add(Me.chkEmlAdd_UpdAll)
        Me.grpAlerts.Controls.Add(Me.cmbPreferedLanguage)
        Me.grpAlerts.Controls.Add(Me.Label6)
        Me.grpAlerts.Controls.Add(Me.txtMobilePhone2Old)
        Me.grpAlerts.Location = New System.Drawing.Point(8, 86)
        Me.grpAlerts.Name = "grpAlerts"
        Me.grpAlerts.Size = New System.Drawing.Size(538, 193)
        Me.grpAlerts.TabIndex = 4
        Me.grpAlerts.TabStop = False
        Me.grpAlerts.Text = "Send alerts to"
        '
        'lblOldPreferedLanguage
        '
        Me.lblOldPreferedLanguage.ForeColor = System.Drawing.Color.Black
        Me.lblOldPreferedLanguage.Location = New System.Drawing.Point(37, 168)
        Me.lblOldPreferedLanguage.Name = "lblOldPreferedLanguage"
        Me.lblOldPreferedLanguage.Size = New System.Drawing.Size(61, 16)
        Me.lblOldPreferedLanguage.TabIndex = 71
        Me.lblOldPreferedLanguage.Text = "Old Value"
        Me.lblOldPreferedLanguage.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'lblOldEmailAddress
        '
        Me.lblOldEmailAddress.ForeColor = System.Drawing.Color.Black
        Me.lblOldEmailAddress.Location = New System.Drawing.Point(40, 107)
        Me.lblOldEmailAddress.Name = "lblOldEmailAddress"
        Me.lblOldEmailAddress.Size = New System.Drawing.Size(58, 16)
        Me.lblOldEmailAddress.TabIndex = 70
        Me.lblOldEmailAddress.Text = "Old Value"
        Me.lblOldEmailAddress.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'lblOldMobilePhone
        '
        Me.lblOldMobilePhone.ForeColor = System.Drawing.Color.Black
        Me.lblOldMobilePhone.Location = New System.Drawing.Point(40, 52)
        Me.lblOldMobilePhone.Name = "lblOldMobilePhone"
        Me.lblOldMobilePhone.Size = New System.Drawing.Size(58, 16)
        Me.lblOldMobilePhone.TabIndex = 69
        Me.lblOldMobilePhone.Text = "Old Value"
        Me.lblOldMobilePhone.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'txtMobilePhoneOld
        '
        Me.txtMobilePhoneOld.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtMobilePhoneOld.ForeColor = System.Drawing.Color.Black
        Me.txtMobilePhoneOld.Location = New System.Drawing.Point(104, 51)
        Me.txtMobilePhoneOld.Name = "txtMobilePhoneOld"
        Me.txtMobilePhoneOld.ReadOnly = True
        Me.txtMobilePhoneOld.Size = New System.Drawing.Size(144, 20)
        Me.txtMobilePhoneOld.TabIndex = 58
        '
        'txtEmailAddressOld
        '
        Me.txtEmailAddressOld.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtEmailAddressOld.ForeColor = System.Drawing.Color.Black
        Me.txtEmailAddressOld.Location = New System.Drawing.Point(105, 107)
        Me.txtEmailAddressOld.Name = "txtEmailAddressOld"
        Me.txtEmailAddressOld.ReadOnly = True
        Me.txtEmailAddressOld.Size = New System.Drawing.Size(296, 20)
        Me.txtEmailAddressOld.TabIndex = 57
        '
        'cmbPreferedLanguageOld
        '
        Me.cmbPreferedLanguageOld.BackColor = System.Drawing.Color.Red
        Me.cmbPreferedLanguageOld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPreferedLanguageOld.ForeColor = System.Drawing.Color.Red
        Me.cmbPreferedLanguageOld.Items.AddRange(New Object() {"Arabic", "English"})
        Me.cmbPreferedLanguageOld.Location = New System.Drawing.Point(104, 166)
        Me.cmbPreferedLanguageOld.Name = "cmbPreferedLanguageOld"
        Me.cmbPreferedLanguageOld.Size = New System.Drawing.Size(121, 21)
        Me.cmbPreferedLanguageOld.TabIndex = 56
        '
        'txtMobilePhone2
        '
        Me.txtMobilePhone2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtMobilePhone2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.txtMobilePhone2.Enabled = False
        Me.txtMobilePhone2.ForeColor = System.Drawing.Color.Black
        Me.txtMobilePhone2.Location = New System.Drawing.Point(107, 24)
        Me.txtMobilePhone2.Name = "txtMobilePhone2"
        Me.txtMobilePhone2.Size = New System.Drawing.Size(160, 21)
        Me.txtMobilePhone2.TabIndex = 1
        '
        'btnTstEml
        '
        Me.btnTstEml.Location = New System.Drawing.Point(408, 56)
        Me.btnTstEml.Name = "btnTstEml"
        Me.btnTstEml.Size = New System.Drawing.Size(104, 24)
        Me.btnTstEml.TabIndex = 4
        Me.btnTstEml.Text = "Test Message"
        '
        'btnTstMob
        '
        Me.btnTstMob.Location = New System.Drawing.Point(408, 24)
        Me.btnTstMob.Name = "btnTstMob"
        Me.btnTstMob.Size = New System.Drawing.Size(104, 24)
        Me.btnTstMob.TabIndex = 2
        Me.btnTstMob.Text = "Test Message"
        '
        'chkMobPhn_UpdAll
        '
        Me.chkMobPhn_UpdAll.Location = New System.Drawing.Point(412, 85)
        Me.chkMobPhn_UpdAll.Name = "chkMobPhn_UpdAll"
        Me.chkMobPhn_UpdAll.Size = New System.Drawing.Size(96, 16)
        Me.chkMobPhn_UpdAll.TabIndex = 6
        Me.chkMobPhn_UpdAll.Text = "Update mobile phone number in all channels"
        Me.chkMobPhn_UpdAll.Visible = False
        '
        'chkEmlAdd_UpdAll
        '
        Me.chkEmlAdd_UpdAll.Location = New System.Drawing.Point(412, 101)
        Me.chkEmlAdd_UpdAll.Name = "chkEmlAdd_UpdAll"
        Me.chkEmlAdd_UpdAll.Size = New System.Drawing.Size(96, 16)
        Me.chkEmlAdd_UpdAll.TabIndex = 7
        Me.chkEmlAdd_UpdAll.Text = "Update e-mail address in all channels"
        Me.chkEmlAdd_UpdAll.Visible = False
        '
        'txtMobilePhone2Old
        '
        Me.txtMobilePhone2Old.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtMobilePhone2Old.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.txtMobilePhone2Old.Enabled = False
        Me.txtMobilePhone2Old.ForeColor = System.Drawing.Color.Red
        Me.txtMobilePhone2Old.Location = New System.Drawing.Point(108, 51)
        Me.txtMobilePhone2Old.Name = "txtMobilePhone2Old"
        Me.txtMobilePhone2Old.Size = New System.Drawing.Size(160, 21)
        Me.txtMobilePhone2Old.TabIndex = 59
        '
        'txtCusNam
        '
        Me.txtCusNam.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtCusNam.Location = New System.Drawing.Point(160, 8)
        Me.txtCusNam.Name = "txtCusNam"
        Me.txtCusNam.Size = New System.Drawing.Size(352, 20)
        Me.txtCusNam.TabIndex = 2
        '
        'cmbStatusOld
        '
        Me.cmbStatusOld.BackColor = System.Drawing.Color.Red
        Me.cmbStatusOld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbStatusOld.ForeColor = System.Drawing.Color.Red
        Me.cmbStatusOld.Items.AddRange(New Object() {"Registered in the service", "Deregistered from the service"})
        Me.cmbStatusOld.Location = New System.Drawing.Point(112, 62)
        Me.cmbStatusOld.Name = "cmbStatusOld"
        Me.cmbStatusOld.Size = New System.Drawing.Size(232, 21)
        Me.cmbStatusOld.TabIndex = 56
        '
        'lblOldRegistrationStatus
        '
        Me.lblOldRegistrationStatus.ForeColor = System.Drawing.Color.Black
        Me.lblOldRegistrationStatus.Location = New System.Drawing.Point(48, 62)
        Me.lblOldRegistrationStatus.Name = "lblOldRegistrationStatus"
        Me.lblOldRegistrationStatus.Size = New System.Drawing.Size(58, 16)
        Me.lblOldRegistrationStatus.TabIndex = 68
        Me.lblOldRegistrationStatus.Text = "Old Value"
        Me.lblOldRegistrationStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'lblMessage
        '
        Me.lblMessage.ForeColor = System.Drawing.Color.Red
        Me.lblMessage.Location = New System.Drawing.Point(18, 307)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New System.Drawing.Size(455, 16)
        Me.lblMessage.TabIndex = 69
        Me.lblMessage.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'frmAlertServices
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(623, 367)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.cmbStatusOld)
        Me.Controls.Add(Me.txtCusNam)
        Me.Controls.Add(Me.txtCusNum)
        Me.Controls.Add(Me.txtLastModification)
        Me.Controls.Add(Me.grpAlerts)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.cmbStatus)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.lblOldRegistrationStatus)
        Me.Controls.Add(Me.Label2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAlertServices"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Alert Service Maintenance"
        Me.pnlButtons.ResumeLayout(False)
        Me.grpAlerts.ResumeLayout(False)
        Me.grpAlerts.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private Sub btnPreferences_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPreferences.Click
        Dim frm As New frmAlertServices_Preferences(m_CusDat)

        Try
            Dim MsgID As String
            Dim MsgRspDat As M708051_GetAlertServicePreferences_Struct
            ShowBusyIcon(True)
            MsgID = SendMessage_GetAlertServicePreferences(m_CusDat.CusDtl.CusInf.CusNum, "", "")
            If Not (IsValidMid(MsgID)) Then
                HandleError(&H81000167, "Get Alert Service Preferences: Could not send message to the host", "Host Connection")
                Exit Try
            Else
                If Not (GetMessage_GetAlertServicePreferences(MsgID, MsgRspDat)) Then
                    Exit Try
                End If
            End If
            frm.MsgRspDat = MsgRspDat
            'As per mike, customer should always be able to modify regardless of registration status
            'frm.btnModify.Enabled = (cmbStatus.SelectedIndex = 0)
            frm.ShowData(True)
            frm.ShowDialog(Me)
            Dim tmp As M708051_GetAlertServicePreferences_Struct
            tmp = frm.MsgRspDat
            Me.MsgRspDat.TabRowVer = tmp.AltRowVer
        Catch ex As Exception
            HandleError(&H81000168, "Could not view alert service preferences", "ICE Error")
        End Try
        ShowBusyIcon()
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        If m_TrackChanges.isChanged Then
            Dim Ans As DialogResult
            Ans = MessageBox.Show("You have not submitted your changes." & vbCrLf & "Are you sure you want to cancel?", "Submit your changes", _
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If Ans = DialogResult.No Then
                Return
            End If
        Else
            If (btnClose.Text = "&Close") Then Me.Close()
        End If
        btnClose.Text = "&Close"
        Me.m_TrackChanges.RemoveAllControls()
        'Me.Close()
    End Sub

    Private Sub frmAlertServices_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Me.DialogResult = Me.Result
        If (Not (AppInstance.gLoggingOff)) Then
            Dim result As DialogResult = MessageBox.Show("Are you sure you want to close the Alert Screen?", "Cancel Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                Return
            Else
                e.Cancel = True
            End If
        End If
    End Sub

    Private Function GenerateAlertTransactionReference() As String
        Return "ALERT" & DateTime.Now.ToString("yyyyMMddHHmmss") & New Random().Next(100, 999).ToString()
    End Function
    Private Function GetTransactionReference() As String
        Dim transactionRef As String = String.Empty
        Dim sql As String = "SELECT TOP 1 AlertTrxRef FROM AlertServiceChanges WHERE CusNum = @CusNum AND Status = 'Pending'"

        Using conn As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@CusNum", txtCusNum.Text)
                conn.Open()
                transactionRef = cmd.ExecuteScalar().ToString()
            End Using
        End Using

        Return transactionRef
    End Function
    Private Sub EnableNewFields(ByVal enable As Boolean)
        ' Enable or disable new fields based on the role
        txtMobilePhone.Enabled = enable
        txtEmailAddress.Enabled = enable
        cmbPreferedLanguage.Enabled = enable
        cmbStatus.Enabled = enable

        ' You can add additional controls here if necessary
    End Sub

    Private Sub btnModify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnModify.Click
        If Not IceUserAut.IceAutLst(458) Then
            HandleError(&H81000040, "User Operation Denied: Insufficient Authority (Update Enrolment)")
            ModalMsgBox("User Operation Denied: Insufficient authority to update enrolment", MsgBoxStyle.Exclamation, "ICE Security")
            Return
        End If
        ' Check if there are any pending changes for the current customer
        If CheckPendingChanges(txtCusNum.Text) Then
            MessageBox.Show("There are already pending changes for this customer. You cannot modify it again.",
                            "Pending Changes", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return ' Exit the event to prevent modification
        End If

        ' Allow the Maker to proceed with modification if no pending changes
        EnableNewFields(True)
        btnClose.Text = "&Cancel"
        AppInstance.SetAllControlsState(Me, False, New Control() {pnlButtons, txtCusNam, txtCusNum, txtLastModification, btnTstMob, btnTstEml})
        btnModify.Enabled = False
        btnSubmit.Enabled = False
        btnSave.Enabled = True
        btnPreferences.Enabled = False
        btnPrint.Enabled = False
        btnTstMob.Enabled = False
        btnTstEml.Enabled = False
        Me.CancelButton = Nothing
        Me.m_TrackChanges.RecordState()
    End Sub

    Private Function CheckPendingChanges(ByVal customerNumber As String) As Boolean
        Dim sql As String = "SELECT COUNT(1) FROM AlertServiceChanges WHERE CusNum = @CusNum AND Status = 'Pending'"
        Using conn As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@CusNum", customerNumber)
                conn.Open()

                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count > 0 ' Return True if there are pending changes, otherwise False
            End Using
        End Using
    End Function

    Private Sub btnSubmit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSubmit.Click

        If Not IceUserAut.IceAutLst(459) Then

            HandleError(&H81000040, "User Operation Denied: Insufficient Authority (Submit Updated Enrolment)")

            ModalMsgBox("User Operation Denied: Insufficient authority to submit update enrolment", MsgBoxStyle.Exclamation, "ICE Security")

            Return

        End If

        ' Show confirmation dialog before submitting changes

        Dim result As DialogResult = MessageBox.Show("Are you sure you want to submit the changes?", "Confirm Submission", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then

            ApproveChanges()

        End If
        btnModify.Enabled = False
        lblOldEmailAddress.Visible = False
        lblOldMobilePhone.Visible = False
        lblOldPreferedLanguage.Visible = False
        lblOldRegistrationStatus.Visible = False
        txtMobilePhoneOld.Visible = False
        txtEmailAddressOld.Visible = False
        cmbPreferedLanguageOld.Visible = False
        cmbStatusOld.Visible = False
        txtMobilePhone2Old.Visible = False

    End Sub
    Private Sub ApproveChanges()

        ' Fetch the transaction reference

        Dim transactionRef As String = GetTransactionReference()

        Dim checkerUserId As String = AppInstance.LoggedUser

        Dim isSuccess As Boolean = SubmitToHost()

        If isSuccess Then

            ' Update the record in the database to 'Approved'

            Dim sql As String = "UPDATE AlertServiceChanges SET Status = 'Approved', ApprovedDate = GETDATE(), CheckerID = @CheckerID WHERE AlertTrxRef = @AlertTrxRef"

            Using conn As New SqlConnection(connectionString)

                Using cmd As New SqlCommand(sql, conn)

                    cmd.Parameters.AddWithValue("@AlertTrxRef", transactionRef)

                    cmd.Parameters.AddWithValue("@CheckerID", checkerUserId)

                    conn.Open()

                    cmd.ExecuteNonQuery()

                End Using

            End Using

            ' Notify the Checker that the changes have been approved

            MessageBox.Show("Changes approved and submitted to the host.", "Alert Service", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Re-fetch the updated data

            LoadNewValues(txtCusNum.Text)

            btnReject.Enabled = False

            btnSubmit.Enabled = False

        End If

    End Sub


    Private Function SubmitToHost() As Boolean

        Try

            ' If focus abort is triggered, exit the function

            If m_TrackChanges.AbortFocus Then Return False

            ' Check user authorization for registration and de-registration

            Dim AltRegFlg As String = If(cmbStatus.SelectedIndex = 0, "Y", "N")

            If Not (IceUserAut.IceAutLst(459)) And (AltRegFlg = "Y") And (MsgRspDat.AltRegFlg <> AltRegFlg) Then

                HandleError(&H81000040, "User Operation Denied: Insufficient Authority (Register SMS Banking)")

                ModalMsgBox("User Operation Denied: Insufficient authority to register in SMS Banking", MsgBoxStyle.Exclamation, "ICE Security")

                Return False

            End If

            If Not (IceUserAut.IceAutLst(459)) And (AltRegFlg = "N") And (MsgRspDat.AltRegFlg <> AltRegFlg) Then

                HandleError(&H81000040, "User Operation Denied: Insufficient Authority (De-Register SMS Banking)")

                ModalMsgBox("User Operation Denied: Insufficient authority to de-register in SMS Banking", MsgBoxStyle.Exclamation, "ICE Security")

                Return False

            End If

            ' Validate email address if provided

            If (chkAlertEmail.Checked Or txtEmailAddress.Text.Trim <> MsgRspDat.AltEmlAdd.Trim) And txtEmailAddress.Text.Trim <> "" Then

                If Not SIBL0100.SIBI0100.IsValidEmail(txtEmailAddress.Text.Trim) Then

                    MessageBox.Show("Email address [" & txtEmailAddress.Text.Trim & "] is invalid.", "Email Message", MessageBoxButtons.OK, MessageBoxIcon.Error)

                    txtEmailAddress.Focus()

                    txtEmailAddress.SelectAll()

                    Return False

                End If

            End If

            ' Validate mobile number if provided

            If (chkAlertMobile.Checked Or txtMobilePhone.Text.Trim <> MsgRspDat.AltTelNum.Trim) And txtMobilePhone.Text.Trim <> "" Then

                If Not TELI0100.Phone.IsValidPhone(txtMobilePhone.Text.Trim, "N", "Y", "Y", True) Then

                    MessageBox.Show("Mobile number [" & txtMobilePhone.Text.Trim & "] is invalid.", "Mobile Number", MessageBoxButtons.OK, MessageBoxIcon.Error)

                    txtMobilePhone.Focus()

                    txtMobilePhone.SelectAll()

                    Return False

                End If

            End If

            ' Ensure a phone or email is selected for registration

            'If AltRegFlg = "Y" And Not (chkAlertMobile.Checked Or chkAlertEmail.Checked) Then

            '    MessageBox.Show("You must select a phone and/or an email to register in the service.", "Alert Registration", MessageBoxButtons.OK, MessageBoxIcon.Error)

            '    Return False

            'End If

            ' Show busy icon and disable the form

            ShowBusyIcon(True)

            Me.Enabled = False

            ' Prepare data for submission

            With MsgSndDat

                .AltPatDat = If(MsgRspDat.TabRowVer = "000000000000", "NNNNNA", MsgRspDat.AltPatDat)

                .CusNum = txtCusNum.Text

                .ReqComCon = "N"

                .UpdComEml = If(chkEmlAdd_UpdAll.Checked, "Y", "N")

                .UpdComMob = If(chkMobPhn_UpdAll.Checked, "Y", "N")

                .TabRowVer = MsgRspDat.TabRowVer

                .AltRegFlg = AltRegFlg

                .AltLngFlg = If(cmbPreferedLanguage.SelectedIndex = 0, "AR", "EN")

                .AltEmlFlg = If(chkAlertEmail.Checked, "Y", "N")

                .AltEmlAdd = txtEmailAddress.Text.Trim

                .AltMobFlg = If(chkAlertMobile.Checked, "Y", "N")

                .AltTelNum = txtMobilePhone.Text.Trim

            End With

            ' Submit data to host

            Dim MsgID As String = SendMessage_ModifyAlertServices(MsgSndDat)

            If Not IsValidMid(MsgID) OrElse Not GetMessage_ModifyAlertServices(MsgID, Nothing) Then

                MessageBox.Show("Unable to submit data to the host.", "Alert Services", MessageBoxButtons.OK, MessageBoxIcon.Error)

                Return False

            End If

            ' Fetch updated data from host

            MsgID = SendMessage_GetAlertServices(txtCusNum.Text, "", "N")

            If Not IsValidMid(MsgID) OrElse Not GetMessage_GetAlertServices(MsgID, MsgRspDat) Then

                HandleError(&H81000165, "Get Alert Services: Could not send message to the host", "Host Connection")

                Return False

            End If

            ' Update customer data

            With m_CusDat.CusDtl.ALT

                .TabRowVer = MsgRspDat.TabRowVer

                .AltTelNum = MsgRspDat.AltTelNum

                .AltMobFlg = MsgRspDat.AltMobFlg

                .AltEmlAdd = MsgRspDat.AltEmlAdd

                .AltEmlFlg = MsgRspDat.AltEmlFlg

                .AltPatDat = MsgRspDat.AltPatDat

                .AltLngFlg = MsgRspDat.AltLngFlg

                .AltRegFlg = MsgRspDat.AltRegFlg

                .UsrStpDat = MsgRspDat.UsrStpDat

            End With

            ' Clear controls and show data

            Me.m_TrackChanges.RemoveAllControls()

            ShowData()

            ' Enable form and hide busy icon

            Me.Enabled = True

            ShowBusyIcon()

            Return True

        Catch ex As Exception

            btnSubmit.Enabled = True

            Me.Enabled = True

            ShowBusyIcon()

            Return False

        End Try

    End Function

    Private Sub LoadNewValues(ByVal customerNumber As String)
        ' Define the SQL query to get old and new values for the customer
        Dim sql As String = "SELECT  NewMobilePhone,  NewEmailAddress, " &
                            " NewPreferredLanguage, NewStatus " &
                            "FROM AlertServiceChanges WHERE CusNum = @CusNum AND Status = 'Approved'"

        ' Establish a connection to the database
        Using conn As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(sql, conn)
                ' Add the customer number as a parameter to the SQL query
                cmd.Parameters.AddWithValue("@CusNum", customerNumber)

                ' Open the database connection
                conn.Open()

                ' Execute the query and read the results
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' Populate the form fields with the old and new values
                        txtMobilePhoneOld.Text = ""
                        txtMobilePhone.Text = reader("NewMobilePhone").ToString()

                        txtEmailAddressOld.Text = ""
                        txtEmailAddress.Text = reader("NewEmailAddress").ToString()

                        cmbPreferedLanguageOld.SelectedItem = -1
                        cmbPreferedLanguage.SelectedItem = reader("NewPreferredLanguage").ToString()

                        cmbStatusOld.SelectedItem = -1
                        cmbStatus.SelectedItem = reader("NewStatus").ToString()
                    Else
                        ' Handle case where no pending changes are found
                        lblMessage.Text = "No pending changes found for this customer."
                    End If
                End Using
            End Using
        End Using
    End Sub
    Private Sub LoadOldValues(ByVal customerNumber As String)
        ' Define the SQL query to get old and new values for the customer
        Dim sql As String = "SELECT OldMobilePhone, NewMobilePhone, OldEmailAddress, NewEmailAddress, " &
                            "OldPreferredLanguage, NewPreferredLanguage, OldStatus, NewStatus, MobileFlag, EmailFlag " &
                            "FROM AlertServiceChanges WHERE CusNum = @CusNum AND Status = 'Pending'"

        ' Establish a connection to the database
        Using conn As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(sql, conn)
                ' Add the customer number as a parameter to the SQL query
                cmd.Parameters.AddWithValue("@CusNum", customerNumber)

                ' Open the database connection
                conn.Open()

                ' Execute the query and read the results
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' Populate the form fields with the old and new values
                        txtMobilePhoneOld.Text = reader("OldMobilePhone").ToString()
                        txtMobilePhone.Text = reader("NewMobilePhone").ToString()

                        txtEmailAddressOld.Text = reader("OldEmailAddress").ToString()
                        txtEmailAddress.Text = reader("NewEmailAddress").ToString()

                        cmbPreferedLanguageOld.SelectedItem = reader("OldPreferredLanguage").ToString()
                        cmbPreferedLanguage.SelectedItem = reader("NewPreferredLanguage").ToString()

                        cmbStatusOld.SelectedItem = reader("OldStatus").ToString()
                        cmbStatus.SelectedItem = reader("NewStatus").ToString()

                        chkAlertMobile.Checked = (reader("MobileFlag").ToString() = "Y")
                        chkAlertEmail.Checked = (reader("EmailFlag").ToString() = "Y")
                    Else
                        ' Handle case where no pending changes are found
                        lblMessage.Text = "No pending changes found for this customer."
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadPendingRequest(ByVal trxRef As String)
        ' Query the database to get the details of the selected transaction reference
        Dim sql As String = "SELECT * FROM AlertServiceChanges WHERE AlertTrxRef = @AlertTrxRef AND Status = 'Pending'"

        Using conn As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@AlertTrxRef", trxRef)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' Load both old and new values into the form fields
                        txtMobilePhoneOld.Text = reader("OldMobilePhone").ToString()
                        txtMobilePhone.Text = reader("NewMobilePhone").ToString()

                        txtEmailAddressOld.Text = reader("OldEmailAddress").ToString()
                        txtEmailAddress.Text = reader("NewEmailAddress").ToString()

                        cmbPreferedLanguageOld.SelectedItem = reader("OldPreferredLanguage").ToString()
                        cmbPreferedLanguage.SelectedItem = reader("NewPreferredLanguage").ToString()

                        cmbStatusOld.SelectedItem = reader("OldStatus").ToString()
                        cmbStatus.SelectedItem = reader("NewStatus").ToString()

                        chkAlertMobile.Checked = (reader("MobileFlag").ToString() = "Y")
                        chkAlertEmail.Checked = (reader("EmailFlag").ToString() = "Y")

                        ' Enable approval/rejection buttons for the Checker
                        btnSubmit.Text = "Submit"
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub RejectChanges()

        ' Fetch the transaction reference

        Dim transactionRef As String = GetTransactionReference()

        Dim checkerUserId As String = AppInstance.LoggedUser

        ' Update the record in the database to 'Rejected'

        Dim sql As String = "UPDATE AlertServiceChanges SET Status = 'Rejected', RejectedDate = GETDATE(), CheckerID = @CheckerID WHERE AlertTrxRef = @AlertTrxRef"

        Using conn As New SqlConnection(connectionString)

            Using cmd As New SqlCommand(sql, conn)

                cmd.Parameters.AddWithValue("@AlertTrxRef", transactionRef)

                cmd.Parameters.AddWithValue("@CheckerID", checkerUserId)

                conn.Open()

                cmd.ExecuteNonQuery()

            End Using

        End Using

        ' Notify the Checker that the changes have been rejected

        MessageBox.Show("Changes have been rejected.", "Alert Service", MessageBoxButtons.OK, MessageBoxIcon.Information)

        btnReject.Enabled = False

        btnSubmit.Enabled = False

    End Sub

    Private Sub SaveMakerChanges()
        Try

            ' Collect old and new values

            Dim oldMobilePhone As String = If(String.IsNullOrEmpty(txtMobilePhoneOld.Text), "N/A", txtMobilePhoneOld.Text)

            Dim newMobilePhone As String = If(String.IsNullOrEmpty(txtMobilePhone.Text), "N/A", txtMobilePhone.Text)

            Dim oldEmailAddress As String = If(String.IsNullOrEmpty(txtEmailAddressOld.Text), "N/A", txtEmailAddressOld.Text)

            Dim newEmailAddress As String = If(String.IsNullOrEmpty(txtEmailAddress.Text), "N/A", txtEmailAddress.Text)

            Dim oldLanguage As String = If(cmbPreferedLanguageOld.SelectedItem Is Nothing, "N/A", cmbPreferedLanguageOld.SelectedItem.ToString())

            Dim newLanguage As String = If(cmbPreferedLanguage.SelectedItem Is Nothing, "N/A", cmbPreferedLanguage.SelectedItem.ToString())

            Dim oldStatus As String = If(cmbStatusOld.SelectedItem Is Nothing, "N/A", cmbStatusOld.SelectedItem.ToString())

            Dim newStatus As String = If(cmbStatus.SelectedItem Is Nothing, "N/A", cmbStatus.SelectedItem.ToString())

            Dim mobileFlag As String = If(chkAlertMobile.Checked, "Y", "N")

            Dim EmailFlag As String = If(chkAlertEmail.Checked, "Y", "N")


            ' Generate a unique transaction reference
            Dim transactionRef As String = GenerateAlertTransactionReference()
            Dim makerUserId As String = AppInstance.LoggedUser
            ' Insert into the database
            Dim sql As String = "INSERT INTO AlertServiceChanges (AlertTrxRef, CusNum, OldMobilePhone, NewMobilePhone, OldEmailAddress, NewEmailAddress, " &
                                "OldPreferredLanguage, NewPreferredLanguage, OldStatus, NewStatus, MobileFlag , EmailFlag , ChangeType, Status, MakerID) " &
                                "VALUES (@AlertTrxRef, @CusNum, @OldMobilePhone, @NewMobilePhone, @OldEmailAddress, @NewEmailAddress, @OldPreferredLanguage, " &
                                "@NewPreferredLanguage, @OldStatus, @NewStatus, @MobileFlag, @EmailFlag, 'Modify', 'Pending', @MakerID)"

            Using conn As New SqlConnection(connectionString)
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@AlertTrxRef", transactionRef)
                    cmd.Parameters.AddWithValue("@CusNum", txtCusNum.Text)
                    cmd.Parameters.AddWithValue("@OldMobilePhone", oldMobilePhone)
                    cmd.Parameters.AddWithValue("@NewMobilePhone", newMobilePhone)
                    cmd.Parameters.AddWithValue("@OldEmailAddress", oldEmailAddress)
                    cmd.Parameters.AddWithValue("@NewEmailAddress", newEmailAddress)
                    cmd.Parameters.AddWithValue("@OldPreferredLanguage", oldLanguage)
                    cmd.Parameters.AddWithValue("@NewPreferredLanguage", newLanguage)
                    cmd.Parameters.AddWithValue("@OldStatus", oldStatus)
                    cmd.Parameters.AddWithValue("@NewStatus", newStatus)
                    cmd.Parameters.AddWithValue("@MobileFlag", mobileFlag)
                    cmd.Parameters.AddWithValue("@EmailFlag", EmailFlag)
                    cmd.Parameters.AddWithValue("@MakerID", makerUserId)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            ' Notify the Maker that the changes are saved and awaiting approval
            MessageBox.Show("Changes saved. Waiting for checker approval.", "Alert Service", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("An error occurred:", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try
    End Sub


    Public Sub ShowData()

        ' Map customer data to UI fields

        txtCusNum.Text = m_CusDat.CusDtl.CusInf.CusNum

        txtCusNam.Text = m_CusDat.CusItm.CusNam

        ' Store and map old and new Email Addresses

        txtEmailAddressOld.Text = MsgRspDat.AltEmlAdd.Trim ' Old Email Address

        txtEmailAddress.Text = MsgRspDat.AltEmlAdd.Trim    ' New Email Address

        ' Fill mobile phone number fields with old and new values

        txtMobilePhoneOld.Text = MsgRspDat.AltTelNum.Trim   ' Old Mobile Phone

        txtMobilePhone.Text = MsgRspDat.AltTelNum.Trim      ' New Mobile Phone

        ' Handle second mobile phone number with old value tracking

        'txtMobilePhone2Old.Text = If(txtMobilePhone2.Items.Count > 0, txtMobilePhone2.Items(0).ToString(), "") ' Old Mobile Phone 2

        txtMobilePhone2.Items.Clear()
        txtMobilePhone2Old.Items.Clear()

        If (MsgRspDat.AltTelNum.Trim <> "") Then

            txtMobilePhone2.Items.Add(MsgRspDat.AltTelNum.Trim) ' Add current number to the dropdown
            txtMobilePhone2Old.Items.Add(MsgRspDat.AltTelNum.Trim)
        End If

        If Not m_CusDat.CusCmb.PrsConDat.TelMob Is Nothing Then

            Dim st As String = m_CusDat.CusCmb.PrsConDat.TelMob.Substring(0, 25).Trim

            If (st <> "") AndAlso (st <> MsgRspDat.AltTelNum.Trim) Then

                txtMobilePhone2.Items.Add(st) ' Add additional phone number
                txtMobilePhone2Old.Items.Add(st)
            End If

        End If

        ' Map last modification date

        txtLastModification.Text = AppInstance.FormatUsrStpDat(MsgRspDat.UsrStpDat.Trim)

        ' Map alert preferences

        chkAlertEmail.Checked = (MsgRspDat.AltEmlFlg = "Y")

        chkAlertMobile.Checked = (MsgRspDat.AltMobFlg = "Y")

        ' Track old and new status

        cmbStatusOld.SelectedIndex = CInt(IIf(MsgRspDat.AltRegFlg = "Y", 0, 1)) ' Old Status

        cmbStatus.SelectedIndex = CInt(IIf(MsgRspDat.AltRegFlg = "Y", 0, 1)) ' New Status

        ' Track old and new preferred language

        cmbPreferedLanguageOld.SelectedIndex = CInt(IIf(MsgRspDat.AltLngFlg = "EN", 1, 0)) ' Old Language

        cmbPreferedLanguage.SelectedIndex = CInt(IIf(MsgRspDat.AltLngFlg = "EN", 1, 0)) ' New Language

        ' Enable or disable preferences button

        btnPreferences.Enabled = Not (HideCustomerFinData(m_CusDat, "2"))

        ' Enable alerts group if registered

        grpAlerts.Enabled = (cmbStatus.SelectedIndex = 0)

        ' Reset checkboxes for updating all

        chkMobPhn_UpdAll.Checked = False

        chkEmlAdd_UpdAll.Checked = False

        ' Manage control states

        AppInstance.SetAllControlsState(CType(Me, Control), True, New Control() {pnlButtons, txtCusNum, txtCusNam, txtLastModification, btnTstMob, btnTstEml})

        ' Enable or disable buttons

        btnModify.Enabled = True

        btnSubmit.Enabled = False

        btnPrint.Enabled = True

        btnTstMob.Enabled = True

        btnTstEml.Enabled = True

        ' Track changes for maker-checker functionality

        Me.m_TrackChanges.TrackControls(Me)

    End Sub

    Private Sub frmAlertServices_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If Not IceUserAut.IceAutLst(458) And IceUserAut.IceAutLst(458) And IceUserAut.IceAutLst(290) Then
            HandleError(&H81000040, "User Operation Denied: Insufficient Authority (Update Enrolment)")
            ModalMsgBox("User Operation Denied: Insufficient authority to update enrolment", MsgBoxStyle.Exclamation, "ICE Security")
            Me.Close()
            Return
        End If
        ' Preserve the existing control state logic
        AppInstance.SetAllControlsState(CType(Me, Control), True, New Control() {pnlButtons, btnTstMob, btnTstEml})

        If IceUserAut.IceAutLst(458) Or IceUserAut.IceAutLst(290) Then
            ' If the user is Maker, load the Maker view
            SetupMakerView()
        ElseIf IceUserAut.IceAutLst(459) Then
            ' If the user is Checker, load the Checker view and fetch pending changes
            SetupCheckerView()
            LoadPendingChangesForCustomer(txtCusNum.Text)
        End If

    End Sub

    Private Sub LoadPendingChangesForCustomer(ByVal customerNumber As String)

        Dim sql As String = "SELECT * FROM AlertServiceChanges WHERE CusNum = @CusNum AND Status = 'Pending'"

        Using conn As New SqlConnection(connectionString)

            Using cmd As New SqlCommand(sql, conn)

                cmd.Parameters.AddWithValue("@CusNum", customerNumber.Trim())

                conn.Open()

                Using reader As SqlDataReader = cmd.ExecuteReader()

                    If reader.Read() Then

                        ' Map old values to 'Old' fields

                        txtMobilePhoneOld.Text = reader("OldMobilePhone").ToString()

                        txtEmailAddressOld.Text = reader("OldEmailAddress").ToString()

                        cmbPreferedLanguageOld.SelectedItem = reader("OldPreferredLanguage").ToString()

                        cmbStatusOld.SelectedItem = reader("OldStatus").ToString()

                        ' Map new values to the regular fields

                        txtMobilePhone.Text = reader("NewMobilePhone").ToString()

                        txtEmailAddress.Text = reader("NewEmailAddress").ToString()

                        cmbPreferedLanguage.SelectedItem = reader("NewPreferredLanguage").ToString()

                        cmbStatus.SelectedItem = reader("NewStatus").ToString()

                        chkAlertMobile.Checked = (reader("MobileFlag").ToString() = "Y")

                        chkAlertEmail.Checked = (reader("EmailFlag").ToString() = "Y")

                        ' Add arrows if values differ
                        AddArrowIfDifferent(cmbStatusOld, cmbStatus, 350, 62)
                        AddArrowIfDifferent(txtMobilePhoneOld, txtMobilePhone, 280, 135)
                        AddArrowIfDifferent(txtEmailAddressOld, txtEmailAddress, 420, 193)
                        AddArrowIfDifferent(cmbPreferedLanguageOld, cmbPreferedLanguage, 238, 250)
                    Else
                        btnReject.Enabled = False
                        btnSubmit.Enabled = False
                        btnPrint.Enabled = False
                        btnPreferences.Enabled = False
                        lblOldEmailAddress.Visible = False
                        lblOldMobilePhone.Visible = False
                        lblOldPreferedLanguage.Visible = False
                        lblOldRegistrationStatus.Visible = False
                        txtMobilePhoneOld.Visible = False
                        txtEmailAddressOld.Visible = False
                        cmbPreferedLanguageOld.Visible = False
                        cmbStatusOld.Visible = False
                        txtMobilePhone2Old.Visible = False
                        lblMessage.Text = "No Pending Changes Found for this Customer."

                        'btnTstEml.Enabled = False
                        'btnTstMob.Enabled = False
                        'btnTstMsg.Enabled = False
                    End If

                End Using

            End Using

        End Using

    End Sub

    Private Sub AddArrowIfDifferent(ByVal oldControl As Control, ByVal newControl As Control, ByVal xPosition As Integer, ByVal yPosition As Integer)


        If oldControl.Text <> newControl.Text Then

            ' Remove any existing arrow to avoid duplicates

            RemoveExistingArrow(oldControl)

            ' Create a new PictureBox for the arrow icon

            Dim arrowIcon As New PictureBox()

            arrowIcon.SizeMode = PictureBoxSizeMode.StretchImage

            arrowIcon.Size = New Size(20, 20) ' Adjust size as needed

            ' Set the arrow icon from Resources or a file path

            arrowIcon.Image = My.Resources.Arrow

            ' Set the position of the PictureBox

            arrowIcon.Location = New Point(xPosition, yPosition)

            ' Attach the arrow PictureBox to the old control's Tag property for cleanup

            oldControl.Tag = arrowIcon

            ' Add the PictureBox to the form

            Me.Controls.Add(arrowIcon)

            arrowIcon.BringToFront()

        End If

    End Sub

    ' Method to remove any existing arrow icon from the control's Tag property

    Private Sub RemoveExistingArrow(ByVal control As Control)

        If control.Tag IsNot Nothing AndAlso TypeOf control.Tag Is PictureBox Then

            Dim existingArrow As PictureBox = CType(control.Tag, PictureBox)

            Me.Controls.Remove(existingArrow)

            control.Tag = Nothing

        End If

    End Sub

    Private Sub SetupMakerView()
        ' Show only the fields that the maker can modify
        txtMobilePhone.Visible = True
        txtEmailAddress.Visible = True
        cmbPreferedLanguage.Visible = True
        cmbStatus.Visible = True
        ' Hide the old value fields
        txtMobilePhoneOld.Visible = False
        txtMobilePhone2Old.Visible = False
        txtEmailAddressOld.Visible = False
        cmbPreferedLanguageOld.Visible = False
        cmbStatusOld.Visible = False
        lblOldRegistrationStatus.Visible = False
        lblOldMobilePhone.Visible = False
        lblOldEmailAddress.Visible = False
        lblOldPreferedLanguage.Visible = False
        chkAlertEmail.Checked = False
        chkAlertMobile.Checked = False
        btnPrint.Enabled = False
        ' Set button text for Maker
        btnSave.Visible = True
        btnSubmit.Visible = False ' Hide Save button for Checker
        btnReject.Visible = False
        btnModify.Visible = True ' Hide Modify button for Maker
        ' Enable fields for Maker to modify
        EnableNewFields(True)
    End Sub
    Private Sub SetupCheckerView()
        ' Show both old and new fields
        txtMobilePhone.Visible = True
        txtEmailAddress.Visible = True
        cmbPreferedLanguage.Visible = True
        cmbStatus.Visible = True

        txtMobilePhoneOld.Visible = True
        txtEmailAddressOld.Visible = True
        txtMobilePhone2Old.Visible = True
        cmbPreferedLanguageOld.Visible = True
        cmbStatusOld.Visible = True

        lblOldRegistrationStatus.Visible = True
        lblOldMobilePhone.Visible = True
        lblOldEmailAddress.Visible = True
        lblOldPreferedLanguage.Visible = True
        ' Set button text for Checker
        btnPrint.Enabled = False
        btnPreferences.Enabled = False
        btnTstEml.Enabled = False
        btnTstMob.Enabled = False
        btnTstMsg.Enabled = False
        btnSave.Visible = False ' Hide Save button for Checker
        btnSubmit.Visible = True
        btnModify.Visible = False ' Hide Modify button for Checker
        btnReject.Visible = True
        btnSubmit.Enabled = True
        ' Disable new fields, as Checker should only view them
        EnableNewFields(False)
    End Sub


    Private Class AltThreadData_Struct
        Public LastMsgID As String
        Public Unit As String
        Public ObjKey As String
        Public CusNum As String
        Public CusNam As String
        Public IbkUsrIde As String
        Public IbkCrpIde As String
        'Public EnrInfTable As CoreSys_EnrInf
        Public AltSrv As M708031_AlertServices_Struct
        Public CusDat As frmCustomer.CusDta_Struct

        Public Sub PrintAlertThread()
            Dim ChnIdx As Integer
            Dim WordDoc As Object
            Dim i, j, Offset As Integer
            Dim Str1, Str2, Str3, StrTemp, StrPrf As String
            Try
                AppInstance.EnterPrintBlock()
                WinWord.StartWord()
                If (WinWord.SelectPrinter) Then
                    Dim savFile As String
                    ShowPrintFormEx(AppInstance.PrintThreads.Item(ObjKey))
                    savFile = Me.Unit & ".ICET0250.AlertService." & Me.CusNum & "." & AppInstance.IceFormatDate(Now, "ddMMMyyyy.") & _
                                AppInstance.IceFormatTime(Now, "HHmmss.") & Me.LastMsgID & ".doc"
                    AppInstance.Logger.LogInfo(&H80000000, "Printing " & savFile)
                    gfrmPrint.Printer = WinWord.SelectedPrinter
                    gfrmPrint.Title = ("Printing Alert Service Report")
                    gfrmPrint.Status = ("Starting Word Engine...")
                    gfrmPrint.FullName = savFile
                    savFile = ICEI0100.IcePaths.DefInstance.VersionsPath & "\" & savFile
                    If ChnIdx >= 0 Then
                        WordDoc = WinWord.CreateTemplateEx(ICEI0100.IcePaths.DefInstance.TemplatePath, "\ICET0250.DOT", _
                                    "Alert Service", &H81000175, WinWord.enumDocType.EmbededWord)
                        If (WordDoc Is Nothing) Then
                            HandleError(&H80005016, "Could not print Alert Service report. Check log file for details!", "Print Alert Service")
                            Exit Try
                        End If
                        gfrmPrint.Status = ("Updating Channel Enrolment")
                        WinWord.SetDocProp(WordDoc, "PrtRefNum", Me.Unit & "-" & Me.LastMsgID)
                        WinWord.SetDocProp(WordDoc, "CusNam", Me.CusNam)
                        WinWord.SetDocProp(WordDoc, "CusNum", Me.CusNum)
                        WinWord.SetDocProp(WordDoc, "RegSta", CStr(IIf(AltSrv.AltRegFlg = "Y", "Registered in the service", "Deregistered from the service")))

                        With AltSrv
                            'Clear the flags in the document if not set in the data
                            If .AltMobFlg <> "Y" Then
                                WinWord.SetDocProp(WordDoc, "FlgMob", "O")
                            End If
                            If .AltEmlFlg <> "Y" Then
                                WinWord.SetDocProp(WordDoc, "FlgEml", "O")
                            End If
                            'Set the rest of the fields
                            WinWord.SetDocProp(WordDoc, "MobPhn", .AltTelNum)
                            WinWord.SetDocProp(WordDoc, "EmlAdd", .AltEmlAdd)
                            WinWord.SetDocProp(WordDoc, "PrfLng", CStr(IIf(.AltLngFlg = "AR", "Arabic", "English")))
                            WinWord.SetDocProp(WordDoc, "LstMod", AppInstance.FormatUsrStpDat(.UsrStpDat.Trim))
                            WinWord.SetDocProp(WordDoc, "PrtUsr", AppInstance.LoggedUser & " on " & Environment.MachineName)
                        End With
                        gfrmPrint.Status = ("Printing Alert Service Report...")
                        WinWord.UpdateDocFields(WordDoc)

                        WinWord.SaveWordDoc(WordDoc, savFile)
                        WinWord.PrintWordDoc(WordDoc, False)
                        MarkFileForDeletion(savFile)
                        WinWord.CloseWordDoc(WordDoc)
                    End If
                End If
            Catch ex As ThreadAbortException
                ShowStatusText("Printing Aborted.")
            Catch ex As Exception
                HandleError(&H81000176, "Error while printing ALert Preferences Report! " & ex.Message, "ICE Critical Error")
            Finally
                If Not (WordDoc Is Nothing) Then WinWord.CloseWordDoc(WordDoc)
                WinWord.StopWord()
                HidePrintForm()
                'Clean up the thread references
                AppInstance.PrintThreads.Remove(ObjKey)
                AppInstance.ExitPrintBlock()
            End Try
        End Sub
    End Class

    Public Sub PrintAlertService(ByVal CusDat As frmCustomer.CusDta_Struct)
        Static Seq As Integer = 0
        Dim ChnThreaddata As New AltThreadData_Struct
        If (Printing.PrinterSettings.InstalledPrinters.Count() = 0) Then
            HandleError(&H80007005, "Could not print alert preferences report as there are no printers installed!", "Report Printing Error")
            Return
        End If
        Dim i As Integer
        ShowBusyIcon(True)
        '''Thread.VolatileWrite(AppInstance.gIcePrinting, 1)
        Dim PrnObj As New ICEI0100.AppInstanceClass.PrintThreadObject_stuct
        Dim ObjKey As String = AppInstance.LastMsgID & Format(Now, "HHmmss") & Seq
        Seq += 1

        With ChnThreaddata
            .Unit = AppInstance.Unit
            .ObjKey = ObjKey
            .LastMsgID = AppInstance.LastMsgID
            .CusNum = CusDat.CusItm.CusNum
            .CusNam = CusDat.CusItm.CusNam
            .IbkUsrIde = CusDat.CusItm.IbkUsrIde
            .IbkCrpIde = CusDat.CusItm.IbkCrpIde
            .AltSrv = MsgRspDat
            .CusDat = m_CusDat
        End With

        With PrnObj
            .ObjKey = ObjKey
            .CreateStamp = Now
            .PrintObject = ChnThreaddata
            .PrintThread = New Thread(AddressOf ChnThreaddata.PrintAlertThread) 'xx'
            .PrintThread.CurrentCulture = New CultureInfo("en-US")
            SetApartmentState_MTA(.PrintThread)
            'AppInstance.gIcePrintThread = PrintThread
            AppInstance.PrintThreads.Add(PrnObj, ObjKey)
            CreatePrintForm()
            Application.DoEvents()
            .PrintThread.Start()
        End With
        ShowBusyIcon(False)
    End Sub

    Private Sub btnPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrint.Click
        'Check customer sensitivity level
        If (m_CusDat.CusDtl.CusInf.CusSnsLev.Trim <> "") AndAlso (m_CusDat.CusDtl.CusInf.CusSnsLev.Trim > "0") Then
            'This is a sensitive customer
            If Not (IceUserAut.IceAutLst(290)) Then
                HandleError(&H81000207, "User Operation Denied: Insufficient Authority (Print data for sensitive customers)")
                ModalMessageBox("User Operation Denied: Insufficient Authority (Print data for sensitive customers)", , , MessageBoxIcon.Exclamation, "ICE Security")
                Me.Close()
                Return
            End If
        End If

        PrintAlertService(m_CusDat)
    End Sub

    Private Sub txtEmailAddress_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtEmailAddress.Leave
        m_TrackChanges.SetAbortSubmission(CType(sender, Control), False)
        If m_TrackChanges.isChanged(CType(sender, Control)) Then
            If (txtEmailAddress.Text.Trim <> "" And (Not (SIBL0100.SIBI0100.IsValidEmailEx(txtEmailAddress.Text.Trim)))) Then
                MessageBox.Show("Email address [" & txtEmailAddress.Text.Trim & "] is invalid.", "Email Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtEmailAddress.Focus()
                txtEmailAddress.SelectAll()
                m_TrackChanges.SetAbortSubmission(CType(sender, Control), True)
            End If
        End If
    End Sub

    Private Sub txtEmailAddress_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEmailAddress.KeyPress
        If e.KeyChar = vbBack Then Return 'Key accpted
        If (Asc(e.KeyChar) <= 32 Or Asc(e.KeyChar) >= 127 Or ("!#$%^&*()=+{}[]|\;:'/?>,< """).IndexOf(e.KeyChar) >= 0) Then
            'reject the character
            '''Beep()
            MessageBox.Show("Invalid character entered in email address.", "Input Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            e.Handled = True
        End If
    End Sub

    Private Sub txtMobilePhone_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMobilePhone.Leave
        Dim tel As New TELI0100.Phone
        Dim oLnd As String = Space(10)
        Dim oAre As String = Space(10)
        Dim oNum As String = Space(35)
        Dim oExt As String = Space(10)
        Dim oStr As String = Space(100)
        Dim txt As TextBox = CType(sender, TextBox)
        m_TrackChanges.SetAbortSubmission(txt, False)
        If m_TrackChanges.isChanged(txt) Then
            If (txt.Text.Trim = "") OrElse (tel.CheckPhoneEx(txt.Text, CChar("N"), CChar("Y"), CChar("Y"), oLnd, oAre, oNum, oExt, oStr) = 0) Then
                txt.Text = oStr.Trim
            Else
                MessageBox.Show("Mobile phone [" & txt.Text & "] is invalid.", "Input Validation", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txt.Focus()
                txt.SelectAll()
                m_TrackChanges.SetAbortSubmission(txt, True)
            End If
        End If
    End Sub

    Private Sub cmbStatus_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbStatus.SelectedIndexChanged
        grpAlerts.Enabled = cmbStatus.SelectedIndex = 0
        'Reset values if the user selected to derigister (in case some changes were made)
        If (m_TrackChanges.isChanged(grpAlerts.Controls) And cmbStatus.SelectedIndex = 1) Then
            m_TrackChanges.RestoreState(grpAlerts.Controls)
        End If
    End Sub

    Private Sub btnTstMob_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTstMob.Click
        If ModalMessageBox("About to send a test message to the customer." & vbCrLf & "Are you sure you want to continue?", MessageBoxButtons.YesNo, _
                           MessageBoxDefaultButton.Button2, MessageBoxIcon.Question, "Customer Alert") <> DialogResult.Yes Then
            Return
        End If
        If Not frmAlertSendMessage.SendTestMessage_SMS(txtCusNum.Text, txtMobilePhone.Text.Trim) Then
            txtMobilePhone.Focus()
            txtMobilePhone.SelectAll()
        End If
    End Sub

    Private Sub btnTstEml_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTstEml.Click
        If ModalMessageBox("About to send a test message to the customer." & vbCrLf & "Are you sure you want to continue?", MessageBoxButtons.YesNo, _
                           MessageBoxDefaultButton.Button2, MessageBoxIcon.Question, "Customer Alert") <> DialogResult.Yes Then
            Return
        End If
        If Not frmAlertSendMessage.SendTestMessage_Email(txtCusNum.Text, txtEmailAddress.Text.Trim) Then
            txtEmailAddress.Focus()
            txtEmailAddress.SelectAll()
        End If
    End Sub

    Private Sub btnTstMsg_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTstMsg.Click
        frmAlertSentMessages.RecentTestMessages(Me, txtCusNum.Text, txtCusNam.Text, "", "")
    End Sub

    Private Sub txtMobilePhone2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMobilePhone2.SelectedIndexChanged
        If txtMobilePhone2.SelectedIndex < 0 Then Return
        txtMobilePhone.Text = CStr(txtMobilePhone2.SelectedItem)
        txtMobilePhone2.SelectedIndex = -1
    End Sub

    Private Sub btnReject_Click(sender As Object, e As EventArgs) Handles btnReject.Click

        If Not (IceUserAut.IceAutLst(459)) Then

            HandleError(&H81000207, "User Operation Denied: Insufficient Authority (Reject Enrolment)")

            ModalMessageBox("User Operation Denied: Insufficient Authority (Reject Enrolment)", , , MessageBoxIcon.Exclamation, "ICE Security")

            Return

        End If

        ' Show confirmation dialog before rejecting changes

        Dim result As DialogResult = MessageBox.Show("Are you sure you want to reject the changes?", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then

            RejectChanges()

        End If

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not (IceUserAut.IceAutLst(458)) Then
            HandleError(&H81000207, "User Operation Denied: Insufficient Authority (Save Updated Enrolment)")
            ModalMessageBox("User Operation Denied: Insufficient Authority (Save Updated Enrolment)", , , MessageBoxIcon.Exclamation, "ICE Security")
            Return
        End If
        If txtEmailAddressOld.Text <> txtEmailAddress.Text Then
            If Not chkAlertEmail.Checked Then
                MessageBox.Show("You must select email checkbox to update the email address.", "Alert Registration Email", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
        End If
        If txtMobilePhone.Text <> txtMobilePhoneOld.Text Then
            If Not chkAlertMobile.Checked Then
                MessageBox.Show("You must select mobile phone checkbox to update the mobile phone.", "Alert Registration Mobile", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
        End If
        'If txtEmailAddressOld.Text <> txtEmailAddress.Text Or txtMobilePhone.Text <> txtMobilePhoneOld.Text Then
        '    If Not (chkAlertMobile.Checked v chkAlertEmail.Checked) Then
        '        MessageBox.Show("You must select a phone and/or an email to register in the service.", "Alert Registration", MessageBoxButtons.OK, MessageBoxIcon.Error)
        '        Return
        '    End If
        'End If

        ' Check if all the old and new values are the same
        If txtEmailAddressOld.Text = txtEmailAddress.Text AndAlso
           txtMobilePhoneOld.Text = txtMobilePhone.Text AndAlso
           cmbStatusOld.SelectedIndex = cmbStatus.SelectedIndex AndAlso
           cmbPreferedLanguageOld.SelectedIndex = cmbPreferedLanguage.SelectedIndex Then

            ' If nothing is changed, show a message to the user
            MessageBox.Show("No changes detected for this customer.", "Alert Service", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Enable the modify button (if needed)
            btnModify.Enabled = True

            ' Exit the method to prevent further operations
            Return
        End If

        ' Save functionality for Maker
        SaveMakerChanges()
        btnModify.Enabled = False
        btnSave.Enabled = False
    End Sub
End Class
