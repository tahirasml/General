Imports System.Threading
Imports System.Globalization
Imports SIBL0100

Public Class frmAlertServices
    Inherits System.Windows.Forms.Form

    Public CusNum As String
    Public MsgRspDat As M708031_AlertServices_Struct
    Dim MsgSndDat As M708040_AlertServices_Struct
    Public m_CusDat As frmCustomer.CusDta_Struct
    Private m_TrackChanges As New UI.ControlUtil.ContainerStateTracker

    Private Result As DialogResult = DialogResult.Cancel

#Region " Windows Form Designer generated code "

    Public Sub New(ByRef CusDat As frmCustomer.CusDta_Struct)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

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
        Me.btnClose = New System.Windows.Forms.Button
        Me.btnPrint = New System.Windows.Forms.Button
        Me.btnSubmit = New System.Windows.Forms.Button
        Me.btnModify = New System.Windows.Forms.Button
        Me.btnPreferences = New System.Windows.Forms.Button
        Me.txtLastModification = New System.Windows.Forms.TextBox
        Me.Label10 = New System.Windows.Forms.Label
        Me.cmbPreferedLanguage = New System.Windows.Forms.ComboBox
        Me.Label6 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtEmailAddress = New System.Windows.Forms.TextBox
        Me.txtMobilePhone = New System.Windows.Forms.TextBox
        Me.chkAlertEmail = New System.Windows.Forms.CheckBox
        Me.chkAlertMobile = New System.Windows.Forms.CheckBox
        Me.txtCusNum = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.cmbStatus = New System.Windows.Forms.ComboBox
        Me.pnlButtons = New System.Windows.Forms.Panel
        Me.btnTstMsg = New System.Windows.Forms.Button
        Me.grpAlerts = New System.Windows.Forms.GroupBox
        Me.btnTstEml = New System.Windows.Forms.Button
        Me.btnTstMob = New System.Windows.Forms.Button
        Me.chkMobPhn_UpdAll = New System.Windows.Forms.CheckBox
        Me.chkEmlAdd_UpdAll = New System.Windows.Forms.CheckBox
        Me.txtCusNam = New System.Windows.Forms.TextBox
        Me.txtMobilePhone2 = New System.Windows.Forms.ComboBox
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
        Me.btnSubmit.Enabled = False
        Me.btnSubmit.Location = New System.Drawing.Point(88, 0)
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
        Me.txtLastModification.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.txtLastModification.Location = New System.Drawing.Point(112, 184)
        Me.txtLastModification.Name = "txtLastModification"
        Me.txtLastModification.ReadOnly = True
        Me.txtLastModification.Size = New System.Drawing.Size(400, 20)
        Me.txtLastModification.TabIndex = 6
        Me.txtLastModification.Text = ""
        '
        'Label10
        '
        Me.Label10.Location = New System.Drawing.Point(16, 184)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(104, 16)
        Me.Label10.TabIndex = 57
        Me.Label10.Text = "Last Modification"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'cmbPreferedLanguage
        '
        Me.cmbPreferedLanguage.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.cmbPreferedLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPreferedLanguage.Items.AddRange(New Object() {"Arabic", "English"})
        Me.cmbPreferedLanguage.Location = New System.Drawing.Point(104, 88)
        Me.cmbPreferedLanguage.Name = "cmbPreferedLanguage"
        Me.cmbPreferedLanguage.Size = New System.Drawing.Size(121, 21)
        Me.cmbPreferedLanguage.TabIndex = 5
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(8, 88)
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
        Me.txtEmailAddress.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.txtEmailAddress.Location = New System.Drawing.Point(104, 56)
        Me.txtEmailAddress.Name = "txtEmailAddress"
        Me.txtEmailAddress.Size = New System.Drawing.Size(296, 20)
        Me.txtEmailAddress.TabIndex = 3
        Me.txtEmailAddress.Text = ""
        '
        'txtMobilePhone
        '
        Me.txtMobilePhone.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.txtMobilePhone.Location = New System.Drawing.Point(104, 24)
        Me.txtMobilePhone.Name = "txtMobilePhone"
        Me.txtMobilePhone.Size = New System.Drawing.Size(144, 20)
        Me.txtMobilePhone.TabIndex = 0
        Me.txtMobilePhone.Text = ""
        '
        'chkAlertEmail
        '
        Me.chkAlertEmail.Location = New System.Drawing.Point(8, 56)
        Me.chkAlertEmail.Name = "chkAlertEmail"
        Me.chkAlertEmail.Size = New System.Drawing.Size(112, 16)
        Me.chkAlertEmail.TabIndex = 2
        Me.chkAlertEmail.Text = "Email address"
        '
        'chkAlertMobile
        '
        Me.chkAlertMobile.Location = New System.Drawing.Point(8, 24)
        Me.chkAlertMobile.Name = "chkAlertMobile"
        Me.chkAlertMobile.Size = New System.Drawing.Size(104, 16)
        Me.chkAlertMobile.TabIndex = 0
        Me.chkAlertMobile.Text = "Mobile Phone"
        '
        'txtCusNum
        '
        Me.txtCusNum.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.txtCusNum.Location = New System.Drawing.Point(112, 8)
        Me.txtCusNum.Name = "txtCusNum"
        Me.txtCusNum.Size = New System.Drawing.Size(48, 20)
        Me.txtCusNum.TabIndex = 1
        Me.txtCusNum.Text = ""
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
        Me.Label2.Location = New System.Drawing.Point(16, 32)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(104, 16)
        Me.Label2.TabIndex = 67
        Me.Label2.Text = "Registration Status"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'cmbStatus
        '
        Me.cmbStatus.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbStatus.Items.AddRange(New Object() {"Registered in the service", "Deregistered from the service"})
        Me.cmbStatus.Location = New System.Drawing.Point(112, 32)
        Me.cmbStatus.Name = "cmbStatus"
        Me.cmbStatus.Size = New System.Drawing.Size(232, 21)
        Me.cmbStatus.TabIndex = 3
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnTstMsg)
        Me.pnlButtons.Controls.Add(Me.btnPreferences)
        Me.pnlButtons.Controls.Add(Me.btnModify)
        Me.pnlButtons.Controls.Add(Me.btnSubmit)
        Me.pnlButtons.Controls.Add(Me.btnPrint)
        Me.pnlButtons.Controls.Add(Me.btnClose)
        Me.pnlButtons.Location = New System.Drawing.Point(8, 216)
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
        'grpAlerts
        '
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
        Me.grpAlerts.Location = New System.Drawing.Point(8, 56)
        Me.grpAlerts.Name = "grpAlerts"
        Me.grpAlerts.Size = New System.Drawing.Size(520, 120)
        Me.grpAlerts.TabIndex = 4
        Me.grpAlerts.TabStop = False
        Me.grpAlerts.Text = "Send alerts to"
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
        Me.chkMobPhn_UpdAll.Location = New System.Drawing.Point(344, 80)
        Me.chkMobPhn_UpdAll.Name = "chkMobPhn_UpdAll"
        Me.chkMobPhn_UpdAll.Size = New System.Drawing.Size(96, 16)
        Me.chkMobPhn_UpdAll.TabIndex = 6
        Me.chkMobPhn_UpdAll.Text = "Update mobile phone number in all channels"
        Me.chkMobPhn_UpdAll.Visible = False
        '
        'chkEmlAdd_UpdAll
        '
        Me.chkEmlAdd_UpdAll.Location = New System.Drawing.Point(344, 96)
        Me.chkEmlAdd_UpdAll.Name = "chkEmlAdd_UpdAll"
        Me.chkEmlAdd_UpdAll.Size = New System.Drawing.Size(96, 16)
        Me.chkEmlAdd_UpdAll.TabIndex = 7
        Me.chkEmlAdd_UpdAll.Text = "Update e-mail address in all channels"
        Me.chkEmlAdd_UpdAll.Visible = False
        '
        'txtCusNam
        '
        Me.txtCusNam.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.txtCusNam.Location = New System.Drawing.Point(160, 8)
        Me.txtCusNam.Name = "txtCusNam"
        Me.txtCusNam.Size = New System.Drawing.Size(352, 20)
        Me.txtCusNam.TabIndex = 2
        Me.txtCusNam.Text = ""
        '
        'txtMobilePhone2
        '
        Me.txtMobilePhone2.BackColor = System.Drawing.Color.FromArgb(CType(255, Byte), CType(255, Byte), CType(192, Byte))
        Me.txtMobilePhone2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.txtMobilePhone2.Enabled = False
        Me.txtMobilePhone2.ForeColor = System.Drawing.Color.Black
        Me.txtMobilePhone2.Location = New System.Drawing.Point(107, 24)
        Me.txtMobilePhone2.Name = "txtMobilePhone2"
        Me.txtMobilePhone2.Size = New System.Drawing.Size(160, 21)
        Me.txtMobilePhone2.TabIndex = 1
        '
        'frmAlertServices
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(610, 248)
        Me.Controls.Add(Me.txtCusNam)
        Me.Controls.Add(Me.txtCusNum)
        Me.Controls.Add(Me.txtLastModification)
        Me.Controls.Add(Me.grpAlerts)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.cmbStatus)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label10)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAlertServices"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Alert Service Maintenance"
        Me.pnlButtons.ResumeLayout(False)
        Me.grpAlerts.ResumeLayout(False)
        Me.ResumeLayout(False)

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
        'If btnSubmit.Enabled Then
        If m_TrackChanges.isChanged Then
            Dim Ans As DialogResult
            Ans = MessageBox.Show("You have not submitted your changes." & vbCrLf & "Are you sure you want to cancel?", "Submit your changes", _
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If Ans = DialogResult.No Then
                Return
            End If
            'Restore original data
            With MsgRspDat
                txtEmailAddress.Text = MsgRspDat.AltEmlAdd.Trim
                txtMobilePhone.Text = MsgRspDat.AltTelNum.Trim
                txtLastModification.Text = MsgRspDat.UsrStpDat.Trim
                chkAlertEmail.Checked = (MsgRspDat.AltEmlFlg = "Y")
                chkAlertMobile.Checked = (MsgRspDat.AltMobFlg = "Y")
                cmbStatus.SelectedIndex = CInt(IIf(MsgRspDat.AltRegFlg = "Y", 1, 0))
            End With
        Else
            If (btnClose.Text = "&Close") Then Me.Close()
        End If
        btnClose.Text = "&Close"
        Me.m_TrackChanges.RemoveAllControls()
        ShowData()
    End Sub

    Private Sub btnModify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnModify.Click
        If (Not IceUserAut.IceAutLst(423)) And (Not IceUserAut.IceAutLst(424)) Then
            HandleError(&H81000040, "User Operation Denied: Insufficient Authority (Update Enrolment)")
            ModalMsgBox("User Operation Denied: Insufficient authority to update enrolment", MsgBoxStyle.Exclamation, "ICE Security")
            Return
        End If
        btnClose.Text = "&Cancel"
        AppInstance.SetAllControlsState(Me, False, New Control() {pnlButtons, txtCusNam, txtCusNum, txtLastModification, btnTstMob, btnTstEml})
        btnModify.Enabled = False
        btnSubmit.Enabled = True
        btnPreferences.Enabled = False
        btnPrint.Enabled = False
        btnTstMob.Enabled = False
        btnTstEml.Enabled = False
        Me.CancelButton = Nothing
        Me.m_TrackChanges.RecordState()
    End Sub

    Private Sub frmAlertServices_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Me.DialogResult = Me.Result
        If (Not (AppInstance.gLoggingOff)) Then
            If btnSubmit.Enabled Then
                Dim Ans As DialogResult
                Ans = MessageBox.Show("You have not submitted your changes." & vbCrLf & "Are you sure you want to close this window?", "Submit your changes", _
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                If Ans = DialogResult.No Then
                    e.Cancel = True
                    Return
                End If
            End If
        End If
    End Sub

    Private Sub btnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubmit.Click
        Try
            'Check if any changes are made, if not, ignore the submission
            'If Not Me.m_TrackChanges.isChanged Then
            '    MessageBox.Show("You have not made any changes" & vbCrLf & "No data will be submitted to the host.", "Alerts Service", MessageBoxButtons.OK, MessageBoxIcon.Information)
            '    ShowData()
            '    Exit Try
            'End If

            If m_TrackChanges.AbortFocus Then Return

            'Check if user is authorized
            Dim AltRegFlg As String = CStr(IIf(cmbStatus.SelectedIndex = 0, "Y", "N"))
            If (Not IceUserAut.IceAutLst(423)) And (AltRegFlg = "Y") And (MsgRspDat.AltRegFlg <> AltRegFlg) Then
                HandleError(&H81000040, "User Operation Denied: Insufficient Authority (Register SMS Banking)")
                ModalMsgBox("User Operation Denied: Insufficient authority to register in SMS Banking", MsgBoxStyle.Exclamation, "ICE Security")
                Return
            End If
            If (Not IceUserAut.IceAutLst(424)) And (AltRegFlg = "N") And (MsgRspDat.AltRegFlg <> AltRegFlg) Then
                HandleError(&H81000040, "User Operation Denied: Insufficient Authority (De-Register SMS Banking)")
                ModalMsgBox("User Operation Denied: Insufficient authority to de-register in SMS Banking", MsgBoxStyle.Exclamation, "ICE Security")
                Return
            End If

            If ((chkAlertEmail.Checked) Or (txtEmailAddress.Text.Trim <> MsgRspDat.AltEmlAdd.Trim)) And (txtEmailAddress.Text.Trim <> "") Then
                If (Not (SIBL0100.SIBI0100.IsValidEmail(txtEmailAddress.Text.Trim))) Then
                    MessageBox.Show("Email address [" & txtEmailAddress.Text.Trim & "] is invalid.", "Email Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    txtEmailAddress.Focus()
                    txtEmailAddress.SelectAll()
                    Exit Try
                End If
            End If
            If (chkAlertMobile.Checked) Or (txtMobilePhone.Text.Trim <> MsgRspDat.AltTelNum.Trim) And (txtMobilePhone.Text.Trim <> "") Then
                If (Not TELI0100.Phone.IsValidPhone(txtMobilePhone.Text.Trim, "N", "Y", "Y", True)) Then
                    MessageBox.Show("Mobile number [" & txtMobilePhone.Text.Trim & "] is invalid.", "Mobile Number", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    txtMobilePhone.Focus()
                    txtMobilePhone.SelectAll()
                    Exit Try
                End If
            End If

            If (AltRegFlg = "Y") And (Not chkAlertMobile.Checked) And (Not chkAlertEmail.Checked) Then
                MessageBox.Show("You must select a phone and/or an email to register in the service.", "Alert Registration", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Try
            End If

            ''''Advise the user if he attempts to change the customer's mobile or email address while the service is deregistered.
            '''If ((chkAlertMobile.Checked And m_TrackChanges.isChanged(txtMobilePhone)) Or (chkAlertEmail.Checked And m_TrackChanges.isChanged(txtEmailAddress))) _
            '''    And cmbStatus.SelectedIndex = 1 Then
            '''    Dim Ans As DialogResult
            '''    Ans = ModalMessageBox("You have requested alerts to be sent to customer," & vbCrLf & _
            '''                    "however the service is currently deregistered." & vbCrLf & _
            '''                    "Please note that no alerts will be delivered" & vbCrLf & _
            '''                    "until the customer registers in the service.", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2, _
            '''                    MessageBoxIcon.Information, "Alerts Service")
            '''    If Ans = DialogResult.No Then Exit Try
            '''End If

            ShowBusyIcon(True)
            Me.Enabled = False
            With MsgSndDat
                If MsgRspDat.TabRowVer = "000000000000" Then
                    .AltPatDat = "NNNNNA"
                Else
                    .AltPatDat = MsgRspDat.AltPatDat
                End If
                .CusNum = txtCusNum.Text
                .ReqComCon = "N"
                .UpdComEml = CStr(IIf(chkEmlAdd_UpdAll.Checked, "Y", "N"))
                .UpdComMob = CStr(IIf(chkMobPhn_UpdAll.Checked, "Y", "N"))
                .TabRowVer = MsgRspDat.TabRowVer
                .AltRegFlg = CStr(IIf(cmbStatus.SelectedIndex = 0, "Y", "N"))
                .AltLngFlg = CStr(IIf(cmbPreferedLanguage.SelectedIndex = 0, "AR", "EN"))
                .AltEmlFlg = CStr(IIf(chkAlertEmail.Checked, "Y", "N"))
                .AltEmlAdd = txtEmailAddress.Text.Trim
                .AltMobFlg = CStr(IIf(chkAlertMobile.Checked, "Y", "N"))
                .AltTelNum = txtMobilePhone.Text.Trim
            End With
            Dim DataOut As M708041_AlertServices_Struct
            btnSubmit.Enabled = False
            Dim MsgID As String
            MsgID = SendMessage_ModifyAlertServices(MsgSndDat)
            If IsValidMid(MsgID) Then
                If Not (GetMessage_ModifyAlertServices(MsgID, DataOut)) Then
                    'MessageBox.Show("Unable to submit data to the host.", "SMS Banking", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Throw New Exception
                End If
            Else
                MessageBox.Show("Unable to submit data to the host.", "Alert Services", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Throw New Exception
            End If
            btnClose.Text = "&Close"
            'Re-obtain the data
            MsgID = SendMessage_GetAlertServices(txtCusNum.Text, "", "N")
            If Not (IsValidMid(MsgID)) Then
                HandleError(&H81000165, "Get Alert Services: Could not send message to the host", "Host Connection")
                Throw New Exception
            Else
                If Not (GetMessage_GetAlertServices(MsgID, MsgRspDat)) Then
                    Throw New Exception
                End If
            End If
            With m_CusDat.CusDtl.ALT
                .ItmTyp = "ALT"
                .TabRowVer = MsgRspDat.TabRowVer  ' 12n	•	Table row version
                .AltTelNum = MsgRspDat.AltTelNum  ' 25x	•	Mobile phone number
                .AltMobFlg = MsgRspDat.AltMobFlg  ' 1x	•	Flag (Y/N) alerts to be sent to the mobile
                .AltEmlAdd = MsgRspDat.AltEmlAdd  ' 50x	•	E-mail address (or spaces)
                .AltEmlFlg = MsgRspDat.AltEmlFlg  ' 1x	•	Flag (Y/N) alerts to be sent to the e-mail address
                .AltPatDat = MsgRspDat.AltPatDat  ' 6x	•	Usage pattern (see ChnPatDat structure)
                .AltLngFlg = MsgRspDat.AltLngFlg  ' 2x	•	Language code (AR=Arabic, EN=English)
                .AltRegFlg = MsgRspDat.AltRegFlg  ' 1x	•	Flag (Y/N) indicating whether registered
                .UsrStpDat = MsgRspDat.UsrStpDat  ' 30x	•	User date and time stamp
            End With
            'gfrmCustomer.GetCustomerEnrInf(gfrmCustomer.CusGrpDat(gfrmCustomer.CurCusDatIdx))
            Me.Result = DialogResult.Retry
            Me.m_TrackChanges.RemoveAllControls()
            ShowData()
        Catch ex As Exception
            btnSubmit.Enabled = True
            '''AppInstance.SetAllControlsState(CType(Me, Control), False, New Control() {pnlButtons, txtCusNum, txtCusNam,txtLastModification})
        End Try
        Me.Enabled = True
        ShowBusyIcon()
    End Sub

    Public Sub ShowData()
        txtCusNum.Text = m_CusDat.CusDtl.CusInf.CusNum
        txtCusNam.Text = m_CusDat.CusItm.CusNam
        txtEmailAddress.Text = MsgRspDat.AltEmlAdd.Trim
        'Fill the mobile phone number and add the existing mobile number, if any, to the drop down list
        txtMobilePhone.Text = MsgRspDat.AltTelNum.Trim
        txtMobilePhone2.Items.Clear()
        If (MsgRspDat.AltTelNum.Trim <> "") Then txtMobilePhone2.Items.Add(MsgRspDat.AltTelNum.Trim)
        '#If DEBUG Then
        '        m_CusDat.CusCmb.PrsConDat.TelMob = "                                          "
        '#End If
        If Not m_CusDat.CusCmb.PrsConDat.TelMob Is Nothing Then

            Dim st As String = m_CusDat.CusCmb.PrsConDat.TelMob.Substring(0, 25).Trim
            If (st <> "") And (st <> MsgRspDat.AltTelNum.Trim) Then txtMobilePhone2.Items.Add(st)
        End If


        txtLastModification.Text = AppInstance.FormatUsrStpDat(MsgRspDat.UsrStpDat.Trim)
        chkAlertEmail.Checked = (MsgRspDat.AltEmlFlg = "Y")
        chkAlertMobile.Checked = (MsgRspDat.AltMobFlg = "Y")
        cmbStatus.SelectedIndex = CInt(IIf(MsgRspDat.AltRegFlg = "Y", 0, 1))
        cmbPreferedLanguage.SelectedIndex = CInt(IIf(MsgRspDat.AltLngFlg = "EN", 1, 0))
        btnPreferences.Enabled = Not (HideCustomerFinData(m_CusDat, "2")) '(MsgRspDat.CusNum <> "000000") '(MsgRspDat.AltRegFlg = "Y") 'True if status is Registered
        grpAlerts.Enabled = (cmbStatus.SelectedIndex = 0)
        chkMobPhn_UpdAll.Checked = False
        chkEmlAdd_UpdAll.Checked = False

        AppInstance.SetAllControlsState(CType(Me, Control), True, New Control() {pnlButtons, txtCusNum, txtCusNam, txtLastModification, btnTstMob, btnTstEml})
        btnModify.Enabled = True
        btnSubmit.Enabled = False
        btnPrint.Enabled = True
        btnTstMob.Enabled = True
        btnTstEml.Enabled = True
        'btnTstMob.Enabled = True
        'btnTstEml.Enabled = True
        'Me.CancelButton = btnClose
        Me.m_TrackChanges.TrackControls(Me)
    End Sub

    Private Sub frmAlertServices_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        AppInstance.SetAllControlsState(CType(Me, Control), True, New Control() {pnlButtons, btnTstMob, btnTstEml})
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

End Class
