using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;
using IAL.Common.Exceptions;
using VeriBranch.WebApplication.CustomControls;
using VeriBranch.WebApplication.UIProcess;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.Constants;
using System.Collections.Generic;
using VeriBranch.WebApplication.ObjectModel;
using System.Text;


public partial class NavigationButtonControl : VeriBranchBaseControl, IVpNavigationControl
{
    private const string NAVIGATION_CLIENT_CLICK_EVENT = "onclick";
    private const string VALIDATE_REQUEST_OBJECT = "Request";
    private const string NAVIGATION_NEXT_RESOURCE_CONFIRM = "navigationButtonControllerNextButton.Confirm.Text";
    private const string NAVIGATION_HOME_URL_RESOURCE = "MainPageUrlResource";
    //private const int REPORT_FONT_SIZE = 18;
    //private const int REPORT_IMAGE_HEIGHT = 120;
    //private const int REPORT_IMAGE_WIDTH = 390;
    //private const string REPORT_TIMESTAMP = "ddMMMyyyy HHmmss";
    //private const string REPORT_HEADER = "Content-Disposition";
    //private const string REPORT_CONTENT_TYPE = "application/octet-stream";

    public delegate void BackButtonClickedDelegate();
    public delegate void ModifyButtonClickedDelegate();
    public delegate void ReceiptButtonClickedDelegate();
    public delegate void PrintButtonClickedDelegate();
    public delegate void ScanButtonClickedDelegate();
    public delegate void PrintChequeButtonClickedDelegate();
    
    
    //public delegate void NextButtonClickedDelegate();

    public delegate void Reserved1ButtonClickedDelegate();
    public delegate void AuthorizeButtonClickedDelegate();
    public delegate void RejectButtonClickedDelegate();
    public delegate void ResubmitButtonClickedDelegate();
    public delegate void AcceptRejectionButtonClickedDelegate();
    
    public event BackButtonClickedDelegate BackButtonClicked;
    public event ModifyButtonClickedDelegate ModifyButtonClicked;
    public event ReceiptButtonClickedDelegate ReceiptButtonClicked;
    public event PrintButtonClickedDelegate PrintButtonClicked;
    public event ScanButtonClickedDelegate ScanButtonClicked;
    public event NextButtonClickedDelegate NextButtonClicked;
    public event AskButtonClickedDelegate AskButtonClicked;
    public event ReferCardButtonClickedDelegate ReferCardButtonClicked;
    public event RejectApprovalRequestButtonClickedDelegate RejectApprovalRequestButtonClicked;
    public event ComplianceButtonClickedDelegate ComplianceButtonClicked;
    public event CallCenterButtonClickedDelegate CallCenterButtonClicked;
    public event PrintChequeButtonClickedDelegate PrintChequeButtonClicked;

    public event Reserved1ButtonClickedDelegate Reserved1ButtonClicked;
    public event AuthorizeButtonClickedDelegate AuthorizeButtonClicked;
    public event RejectButtonClickedDelegate RejectButtonClicked;
    public event ResubmitButtonClickedDelegate ResubmitButtonClicked;
    public event AcceptRejectionButtonClickedDelegate AcceptRejectionButtonClicked;
    

    public string Reserved1ButtonText
    {
        set
        {
            btnReserved1.Text = value;
        }
    }

    /// <summary>
    /// Get or Set the Next Button Text
    /// </summary>
    public string NextButtonText
    {
        set
        {
            SafePageController.SetStateValue(ID + "_NextButtonText", value);
            btnNextNavigation.Text = value;
        }
        get
        {
            object str = SafePageController.GetStateValue(ID + "_NextButtonText");
            if (str != null)
                return str.ToString();
            return null;
        }
    }

    public string RejectButtonText
    {
        set
        {
            SafePageController.SetStateValue(ID + "_RejectButtonText", value);
            btnReject.Text = value;
        }
        get
        {
            object str = SafePageController.GetStateValue(ID + "_RejectButtonText");
            if (str != null)
                return str.ToString();
            return null;
        }
    }

    public string PrintButtonText
    {
        set
        {
            btnPrintNavigation.Text = value;
        }
    }

    public string PrintChequeClientID
    {
        get
        {
            return btnPrintCheque.ClientID;
        }
    }

    public string PrintChequeButtonText
    {
        set
        {
            btnPrintCheque.Text = value;
        }
    }
    /// <summary>
    /// Get or Set Accept Rejection Button Control, this requires to change the control properties from page
    /// </summary>
    public VBButton AcceptRejectionButton
    {
        get { return btnAcceptRejection; }
        set { btnAcceptRejection = value; }
    }

    /// <summary>
    /// Get or Set Back Button Control, this requires to change the control properties from page
    /// </summary>
    public VBButton BackButton
    {
        get { return btnBackNavigation; }
        set { btnBackNavigation = value; }
    }
    /// <summary>
    /// Get or Set Next Button Control, this requires to change the control properties from page
    /// </summary>
    public VBButton NextButton
    {
        get { return btnNextNavigation; }
        set { btnNextNavigation = value; }
    }

    public VBButton AskButton
    {
        get { return btnSendAskCall; }
      
    }
    /// <summary>
    /// Get or Set Cancel Button, this requires to change the control properties from page
    /// </summary>
    public VBButton CancelButton
    {
        get { return btnCancelNavigation; }
        set { btnCancelNavigation = value; }
    }
    /// <summary>
    /// Get or Set Close Button, this requires to change the control properties from page
    /// </summary>
    public VBButton CloseButton
    {
        get { return btnClose; }
        set { btnClose = value; }
    }

    /// <summary>
    /// Get or Set Authorize Button, this requires to change the control properties from page
    /// </summary>
    public VBButton AuthorizeButton
    {
        get { return btnAuthorize; }
        set { btnAuthorize = value; }
    }

    /// <summary>
    /// Get or Set Reject Button, this requires to change the control properties from page
    /// </summary>
    public VBButton RejectButton
    {
        get { return btnReject; }
        set { btnReject = value; }
    }

    /// <summary>
    /// Get or Set Reserved1 Button, this requires to change the control properties from page
    /// </summary>
    public VBButton Reserved1Button
    {
        get { return btnReserved1; }
        set { btnReserved1 = value; }
    }

    bool fConfirmButtonEnable = true;
    /// <summary>
    /// Get or Set to check Confirm button is enabled or disable
    /// </summary>
    public bool ConfirmButtonEnable
    {
        get { return fConfirmButtonEnable; }
        set { fConfirmButtonEnable = value; }
    }
    /// <summary>
    /// Get or Set the cause validation of Next Button
    /// </summary>
    public bool NextButtonCausesValidation
    {
        get { return btnNextNavigation.CausesValidation; }
        set { btnNextNavigation.CausesValidation = value; }
    }
    /// <summary>
    /// Get or Set the cause validation of Back Button
    /// </summary>
    public bool BackButtonCausesValidation
    {
        get { return btnBackNavigation.CausesValidation; }
        set { btnBackNavigation.CausesValidation = value; }
    }
    /// <summary>
    /// Get or Set to check the visibilty of scan button
    /// </summary>
    public bool ScanButtonVisible
    {
        get { return btnScanNavigation.Visible; }
        set { btnScanNavigation.Visible = pnlScan.Visible = value; }
    }

    /// <summary>
    /// Get or Set to check the visibilty of scan button
    /// </summary>
    public bool PrintChequeButtonVisible
    {
        get { return btnPrintCheque.Visible; }
        set { btnPrintCheque.Visible = value; }
    }

    public bool ChecklistButtonVisible
    {
        get { return btnPopupChecklist.Visible; }
        set { btnPopupChecklist.Visible = pnlChecklist.Visible = value; }
    }

    public bool DocumentsButtonVisible
    {
        get { return btnPopupDocuments.Visible; }
        set { btnPopupDocuments.Visible = pnlDocuments.Visible = value; }
    }
    
    /// <summary>
    /// Get or Set to check the back button visibility
    /// </summary>
    public bool BackButtonVisible
    {
        get { return btnBackNavigation.Visible; }
        set { btnBackNavigation.Visible = value; }
    }
    /// <summary>
    /// Get or Set to check the visiblity of modify button
    /// </summary>
    public bool ModifyButtonVisible
    {
        get { return btnModifyNavigation.Visible; }
        set { btnModifyNavigation.Visible = value; }
    }
    /// <summary>
    /// Get or Set to check the visibility of cancel button
    /// </summary>
    public bool CancelButtonVisible
    {
        get { return btnCancelNavigation.Visible; }
        set { btnCancelNavigation.Visible = value; }
    }
    /// <summary>
    /// Get or Set to check the visibility of close button
    /// </summary>
    public bool CloseButtonVisible
    {
        get { return btnClose.Visible; }
        set { btnClose.Visible = value; }
    }
    /// <summary>
    /// Get or Set to check the visibility of next button
    /// </summary>
    public bool NextButtonVisible
    {
        get { return btnNextNavigation.Visible; }
        set { btnNextNavigation.Visible = value; }
    }
    /// <summary>
    /// Get or Set to check the visibilty of receipt button
    /// </summary>
    public bool ReciptButtonVisible
    {
        get { return btnReciptNavigation.Visible; }
        set { btnReciptNavigation.Visible = value; }
    }

    public bool Reserved1ButtonVisible
    {
        get { return btnReserved1.Visible; }
        set { btnReserved1.Visible = value; }
    }

    public bool btnSendAskCallVisible
    {
        get { return btnSendAskCall.Visible; }
        set { btnSendAskCall.Visible = value; }
    }

    public bool btnReferCardVisible
    {
        get { return btnReferCardCall.Visible; }
        set
        {
            btnReferCardCall.Visible = value;
            NextButtonVisible = !value;
        }
    }

    public bool btnCallCenterVisible
    {
        get { return btnCallCenter.Visible; }
        set
        {
            btnCallCenter.Visible = value;
        }
    }

    

    public bool AuthorizeButtonVisible
    {
        get { return btnAuthorize.Visible; }
        set { btnAuthorize.Visible = value; }
    }

    public bool RejectButtonVisible
    {
        get { return btnReject.Visible; }
        set { btnReject.Visible = value; }
    }

    public bool ResubmitButtonVisible
    {
        get { return btnResubmit.Visible; }
        set { btnResubmit.Visible = value; }
    }

    public bool AcceptRejectionButtonVisible
    {
        get { return btnAcceptRejection.Visible; }
        set { btnAcceptRejection.Visible = value; }
    }

    public string AuthorizeButtonText
    {
        set { btnAuthorize.Text = value; }
    }

    public string AcceptRejectionButtonText
    {
        set { btnAcceptRejection.Text = value; }
    }

    public bool SignatureButtonVisible
    {
        get { return btnSignature.Visible; }
        set { btnSignature.Visible = value; }
    }

    public string ReceiptButtonText
    {
        set { btnReciptNavigation.Text = value; }
    }

    /// <summary>
    /// Get or Set to check the visibility of print button
    /// </summary>
    public bool PrintAcknowledgementVisible
    {
        get { return btnPrintNavigation.Visible; }
        set { btnPrintNavigation.Visible = value; }
    }
    /// <summary>
    /// Set client click event for print button
    /// </summary>
    public string PrintAcknowledgeButtonClientClick
    {
        set { btnPrintNavigation.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, value); }
    }
    /// <summary>
    /// Set client click event for receipt button
    /// </summary>
    public string ReciptButtonClientClick
    {
        set { btnReciptNavigation.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, value); }
    }
    /// <summary>
    /// Get or Set to check is confirmation valid
    /// </summary>
    public bool IsConfirmationValid
    {
        get;
        set;
    }
    /// <summary>
    /// Get or Set the ScanUrl to go to the scanning page
    /// </summary>
    private string fScanUrlDefault = "~/Scanner/ScanDocs.aspx";
    public string ScanUrl
    {
        get { return fScanUrlDefault; }
        set { fScanUrlDefault = value; }
    }
    /// <summary>
    /// Get or Set the ScanNextURL to go forward 1 step
    /// </summary>
    public string NextUrl
    {
        get
        {
            return ViewState["NextUrl"] as string;
        }
        set
        {
            ViewState["NextUrl"] = value;
        }
    }
    /// <summary>
    /// Get or Set the BackUrl to go back 1 step
    /// </summary>
    public string BackUrl
    {
        get
        {
            return ViewState["BackUrl"] as string;
        }
        set
        {
            ViewState["BackUrl"] = value;
        }
    }
    /// <summary>
    /// Get or Set the modify page url
    /// </summary>
    public string ModifyUrl
    {
        get
        {
            return ViewState["ModifyUrl"] as string;
        }
        set
        {
            ViewState["ModifyUrl"] = value;
        }
    }
    /// <summary>
    /// Get or Set the CancelUrl for cancelation action
    /// </summary>
    public string CancelUrl
    {
        get
        {
            return ViewState["CancelUrl"] as string;
        }
        set
        {
            ViewState["CancelUrl"] = value;
        }
    }
    /// <summary>
    /// Get or Set the receipturl
    /// </summary>
    public string ReceiptUrl
    {
        get
        {
            return ViewState["ReceiptUrl"] as string;
        }
        set
        {
            ViewState["ReceiptUrl"] = value;
        }
    }
    /// <summary>
    /// Get or Set the focus on back button
    /// </summary>
    public bool SetFocusAtBackButton
    {
        get;
        set;
    }
    /// <summary>
    /// Get or Set the focus on cancel button
    /// </summary>
    public bool SetFocusAtCancelButton
    {
        get;
        set;
    }

    /// <summary>
    /// Navigation Record
    /// </summary>
    public void NavigationRecord()
    {
        if (Request == null)
            throw new VPSystemException(VALIDATE_REQUEST_OBJECT, VPExceptionConstants.REQUEST_NULL);

        bool isConfirmPage = false;
        bool isConfirmGeneralPage = false;
        bool isScanPage = false;
        bool isChecklistPage = false;

        if (Request.AppRelativeCurrentExecutionFilePath != null)
        {
            isConfirmPage = Request.AppRelativeCurrentExecutionFilePath.IndexOf(VpUserControlConstants.VpNavigationControlConstant.CONFIRM_URL, StringComparison.Ordinal) > 0;
            isConfirmGeneralPage = Request.AppRelativeCurrentExecutionFilePath.IndexOf(VpUserControlConstants.VpNavigationControlConstant.CONFIRM_GENERAL_URL, StringComparison.Ordinal) > 0;
            isScanPage = Request.AppRelativeCurrentExecutionFilePath.IndexOf(VpUserControlConstants.VpNavigationControlConstant.SCAN_URL, StringComparison.Ordinal) > 0;
            isChecklistPage = Request.AppRelativeCurrentExecutionFilePath.IndexOf(VpUserControlConstants.VpNavigationControlConstant.CHECKLIST_URL, StringComparison.Ordinal) > 0;
        }
        if (isConfirmPage)
            btnNextNavigation.Text = GetLocalResource("navigationButtonControllerExecuteButton.Text");
        //if (isConfirmPage || isConfirmGeneralPage || isScanPage || isChecklistPage)
        //{
        //    if (ConfirmButtonEnable)
        //    {
        //        grdReasons.DataSource = GetEmptyReason();
        //        grdReasons.DataBind();

        //        if (
        //            (isConfirmPage && !BasePage.ShowScanPage && !BasePage.ShowChecklist)
        //            ||
        //            (isScanPage && !BasePage.ShowChecklist)
        //            ||
        //            isChecklistPage
        //            )
        //        {
        //            if (string.IsNullOrEmpty(NextButtonText))
        //                btnNextNavigation.Text = GetLocalResource(NAVIGATION_NEXT_RESOURCE_CONFIRM);
        //        }
        //        else
        //        {
        //            btnDummyExecute.Text = GetLocalResource("navigationButtonControllerNextButton.Text");
        //            btnConfirmNextNavigation.Text = GetLocalResource("navigationButtonControllerNextButton.Text");
        //        }

        //        if (isScanPage || isChecklistPage)
        //        {
        //            mpeExecute.Enabled = btnNextNavigation.Visible;
        //            pnlConfirmExecution.Visible = btnNextNavigation.Visible;
        //            btnNextNavigation.Style.Add(HtmlTextWriterStyle.Display, "none");
        //            btnDummyExecute.Visible = mpeExecute.Enabled;
        //            btnDummyExecute.OnClientClick = "if(Page_ClientValidate(\"\")){gatherReasons();return false;}else {return false;}";
        //        }

        //        hidCaseOpenMsgHeading.Value = GetGlobalResource(VPResourceConstants.CASE_GENERATE_MSG_PRE_EXECUTE);
        //        ctlCaseOpenMessage.NotificationType = VpUserControlConstants.NotificationDisplay.NotificationType.Warning;

        //        var crmCaseOpenReason = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.CrmCaseOpenReason) as List<string>;
        //        var checklistCaseOpenReasons = SafePageController.GetStateValue(VpPageControllerConstants.PageController.ChecklistCaseOpenReasons);
        //        var documentCaseOpenReasons = SafePageController.GetStateValue(VpPageControllerConstants.PageController.DocumentCaseOpenReasons);

        //        if ( checklistCaseOpenReasons != null || documentCaseOpenReasons != null)
        //        {
        //            mpeExecute.Enabled = btnNextNavigation.Visible;
        //            pnlConfirmExecution.Visible = btnNextNavigation.Visible;
        //            btnNextNavigation.Style.Add(HtmlTextWriterStyle.Display, "none");
        //            btnDummyExecute.Visible = mpeExecute.Enabled;
        //            btnDummyExecute.OnClientClick = "if(Page_ClientValidate(\"\")){gatherReasons();return false;}else {return false;}";



        //            var script = new StringBuilder();
        //            script.AppendLine("function getReasons() {");
        //            script.AppendFormat("var checklistItemsReasons = '{0}';", checklistCaseOpenReasons);
        //            script.AppendFormat("var uploadDocumentReasons = '{0}';", documentCaseOpenReasons);
        //            script.AppendLine("var arrchecklistItemsReasons = checklistItemsReasons.split('||');");
        //            script.AppendLine("var arrUploadDocumentReasons = uploadDocumentReasons.split('||');");
        //            script.AppendLine("var combined = arrUploadDocumentReasons.concat(arrchecklistItemsReasons).unique();");
        //            script.AppendLine("return combined;");
        //            script.AppendLine("}");

        //            ScriptManager.RegisterStartupScript(this, GetType(), "getReasons", script.ToString(), true);

        //        }
        //        hidCaseOpenReasons.Value = string.Empty;
        //        if (crmCaseOpenReason != null && crmCaseOpenReason.Count > 0)
        //        {
        //            foreach (var msg in crmCaseOpenReason)
        //            {
        //                hidCaseOpenReasons.Value += string.Format("|{0}", msg);
        //            }
        //        }

        //    }
        //}


        if (Page.IsPostBack)
        {
            if (IsPostBackComingFromModifyButton())
            {
                if (ModifyButtonClicked != null)
                    ModifyButtonClicked();

                if (ModifyUrl == null)
                    throw new VPSystemException("NavigationButtonControl.btnModifyNavigation_Click", "ModifyUrl is null");

                SafePageController.PageControllerRedirect(ModifyUrl);
            }
            if (IsPostBackComingFromBackButton())
            {
                if (BackButtonClicked != null)
                    BackButtonClicked();

                if (BackUrl == null)
                    throw new VPSystemException("NavigationButtonControl.btnBackNavigation_Click", "BackUrl is null");

                SafePageController.PageControllerRedirect(BackUrl);
            }
            else if (IsPostBackComingFromScanButton())
            {
                Page.Validate();

                foreach (var validator in Page.Validators)
                {
                    if (!((IValidator)validator).IsValid)
                    {

                    }
                }


                if (Page.IsValid)
                {
                    if (ScanButtonClicked != null)
                        ScanButtonClicked();

                    SetConfirmPageState();

                    SafePageController.PageControllerRedirect(ScanUrl + "?AppNo=" + (SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber) as string) + "&CIF=" + (SafePageController.Customer.User.CifNo));
                }
            }
        }
    }
    /// <summary>
    /// Set confirmation page in page controller
    /// </summary>
    public void SetConfirmPageState()
    {
        if (Request.AppRelativeCurrentExecutionFilePath != null && Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains(VPResourceConstants.ICommon.ConfirmPageSuffix))
            SafePageController.SetControllerState(PageController.ControllerState.Confirmed);
    }
    /// <summary>
    /// Before cancel, reset state in page controller and reset transfer
    /// </summary>
    public void PrepareToCancelPage()
    {
        SafePageController.ResetState();
    }
    /// <summary>
    /// Redirect to cancel page , if no url provide for cancel it will redirect to main page
    /// </summary>
    public void RedirectToCancelPage()
    {
        SafePageController.PageControllerRedirect(CancelUrl ?? NAVIGATION_HOME_URL_RESOURCE);
    }
    protected override void PageLoad()
    {
        if (!string.IsNullOrEmpty(ReceiptUrl))
            HandlePostBack(btnReciptNavigation);

        NavigationRecord();

        if (SetFocusAtBackButton)
            btnBackNavigation.Focus();
        else if (SetFocusAtCancelButton)
            btnCancelNavigation.Focus();

        HandlePostBack(btnNextNavigation);
        HandlePostBack(btnBackNavigation);
        HandlePostBack(btnModifyNavigation);
        HandlePostBack(btnCancelNavigation);
        HandlePostBack(btnScanNavigation);
        HandlePostBack(btnConfirmNextNavigation);
        HandlePostBack(btnSendAskCall);
        HandlePostBack(btnReferCardCall);
        HandlePostBack(btnRejectApprovalRequest);
        HandlePostBack(btnComplianceCall);
        HandlePostBack(btnCallCenter);

        HandlePostBack(btnAuthorize);
        HandlePostBack(btnReject);
        HandlePostBack(btnResubmit);
        HandlePostBack(btnAcceptRejection);
        HandlePostBack(btnReserved1);
        
        if (!ScanPageVisibilityOverridden)
            mpeScan.Enabled = pnlScan.Visible = btnPopupScan.Visible = BasePage.ShowScanPage;

        mpeChecklist.Enabled = pnlChecklist.Visible = btnPopupChecklist.Visible = BasePage.ShowChecklist;

        if (!IsPostBack)
        {
            btnPopupScan.Attributes.Add("onclick", btnPopupScan.ClientID + string.Format("_showScan('{0}'); return false;", LoginToken));
            btnPopupChecklist.Attributes.Add("onclick", btnPopupChecklist.ClientID + string.Format("_showChecklist('{0}'); return false;", LoginToken));
            btnPopupDocuments.Attributes.Add("onclick", btnPopupDocuments.ClientID + string.Format("_showDocuments('{0}'); return false;", LoginToken));
        }

        //if (IsAskEnabled && !AskButtonCustomHandled)
        if (Page is IStartPage)
        {
            btnSendAskCall.Visible = true;
            btnClose.Visible = true;
            btnRejectApprovalRequest.Visible = true;
            btnComplianceCall.Visible = true;
            
            //keep buttons hidden
            btnClose.Style.Add(HtmlTextWriterStyle.Display, "none");
            btnRejectApprovalRequest.Style.Add(HtmlTextWriterStyle.Display, "none");
            btnComplianceCall.Style.Add(HtmlTextWriterStyle.Display, "none");
            
            if (!IsAskAlreadyCalled)
            {
                btnNextNavigation.Style.Add(HtmlTextWriterStyle.Display, "none");
                btnSendAskCall.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
                
            }
            else
            {
                btnNextNavigation.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
                btnSendAskCall.Style.Add(HtmlTextWriterStyle.Display, "none");
                
            }


        }

        if (Page is IStartPage || Page is IConfirmPage)
        {
            var onClick = btnNextNavigation.Attributes["onclick"];
            if (string.IsNullOrEmpty(onClick))
                onClick = "javascript: ";
            else
                onClick = onClick.Replace("javascript:", "");

            onClick = "javascript: if (typeof(OlivettiDeviceCheckPaperInit) == 'function') { OlivettiDeviceCheckPaperInit();  try { if (typeof(OlivettiDevice) != undefined && OlivettiDevice.IsOlivettiExists == true && OlivettiDevice.IsPaperInserted == false) return false; } catch(err) {}}" + onClick;
            btnNextNavigation.Attributes["onclick"] = onClick;
        }

        if(Page is IExecutePage)
        {
            if(PrintChequeButtonVisible)
            {
                var onClick = btnPrintCheque.Attributes["onclick"];
                onClick = string.IsNullOrEmpty(onClick) ? "javascript: " : onClick.Replace("javascript:", "");

                onClick = "javascript: if (typeof(OlivettiDeviceCheckPaperInit) == 'function') { OlivettiDeviceCheckPaperInit();  try { if (typeof(OlivettiDevice) != undefined && OlivettiDevice.IsOlivettiExists == true && OlivettiDevice.IsPaperInserted == false) return false; } catch(err) {}}" + onClick;
                btnPrintCheque.Attributes["onclick"] = onClick;
            }
        }

        //Page.Form.DefaultButton = btnDefaultDummy.UniqueID;
        //SetButtonVisibility();

        var currentPageType = pageType;
        if(currentPageType == PageType.Confirm)
            btnPrintNavigation.Visible = BasePage.PrintAdvice;

    }

    public bool ShowScanPage
    {
        set
        {
            mpeScan.Enabled = pnlScan.Visible = btnPopupScan.Visible = value;
            ScanPageVisibilityOverridden = true;
        }
    }
    public bool ScanPageVisibilityOverridden { get; set; }

    public bool AskButtonCustomHandled { set; get; }

    private void SetButtonVisibility()
    {
        var nextPageType = NextPageType;
        var currentPageType = pageType;
        var previousPageType = PreviousPageType;

        //hide all buttons
        btnNextNavigation.Visible =
            btnBackNavigation.Visible =
            btnModifyNavigation.Visible =
            btnCancelNavigation.Visible =
            btnScanNavigation.Visible =
            btnPrintNavigation.Visible =
            btnConfirmNextNavigation.Visible = false;

        //based on current and next page types, show the buttons

        switch (currentPageType)
        {
            case PageType.Confirm:
                btnCancelNavigation.Visible = true;
                if (BasePage.PrintAdvice)
                    btnPrintNavigation.Visible = true;
                break;
        }

        switch (nextPageType)
        {
            case PageType.Confirm:
                btnCancelNavigation.Visible =
                    btnNextNavigation.Visible = true;
                break;

            case PageType.Scan:
                btnScanNavigation.Visible = true;
                break;

            case PageType.Execute:
                btnNextNavigation.Visible = true;
                break;
        }


    }
    protected void btnBackNavigation_Click(object sender, EventArgs e)
    {
        if (BackButtonClicked != null)
            BackButtonClicked();
        if (BackUrl == null)
            throw new VPSystemException("NavigationButtonControl.btnBackNavigation_Click", "BackUrl is null");

        SafePageController.PageControllerRedirect(BackUrl);
    }

    protected void btnModifyNavigation_Click(object sender, EventArgs e)
    {
        SafePageController.PageControllerRedirect(ModifyUrl ?? NAVIGATION_HOME_URL_RESOURCE);
    }
    protected void btnCancelNavigation_Click(object sender, EventArgs e)
    {
        PrepareToCancelPage();
        RedirectToCancelPage();
    }
    protected void btnReciptNavigation_Click(object sender, EventArgs e)
    {
        if (ReceiptButtonClicked != null)
            ReceiptButtonClicked();

        GenerateReport("ADV", "Advice");
    }

    public void PrintReceipt()
    {
        btnReciptNavigation_Click(null, null);
    }
    public void PrintAdvice()
    {
        btnPrintNavigation_Click(null, null);
    }

    protected void btnPrintNavigation_Click(object sender, EventArgs e)
    {
        if (PrintButtonClicked != null)
            PrintButtonClicked();

        GenerateReport(string.Empty, string.Empty);
    }
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        btnCancelNavigation.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, string.Format(VpUserControlConstants.VpNavigationControlConstant.JAVASCRIPT_CONFIRM_FUNCTION, GetGlobalResource(VPResourceConstants.CUSTOM_EXIT_MESSAGE), VPConfigurationManager.GetConfigurationParameter(VPResourceConstants.CLOSE_BA_WINDOW_SCRIPT)));
        btnClose.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, string.Format(VpUserControlConstants.VpNavigationControlConstant.JAVASCRIPT_CONFIRM_FUNCTION, GetGlobalResource(VPResourceConstants.CUSTOM_CLOSE_MESSAGE), VPConfigurationManager.GetConfigurationParameter(VPResourceConstants.CLOSE_BA_WINDOW_SCRIPT)));
    }

    protected void NextButton_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            SetConfirmPageState();

            if (NextButtonClicked != null)
                NextButtonClicked();

            if (NextUrl == null)
                throw new VPSystemException("NavigationButtonControl.NextButton_Click", "NextUrl is null");

            SafePageController.PageControllerRedirect(NextUrl);
        }
        else
        {
            foreach (var validator in Page.Validators)
            {
                if (!((IValidator)validator).IsValid)
                {
                    //to check what validator is failing (and not showing any error message on the page)
                }
            }

        }
    }

    protected void AskButton_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            if (AskButtonClicked != null)
                AskButtonClicked();
        }
        else
        {
            foreach (var validator in Page.Validators)
            {
                if (!((IValidator)validator).IsValid)
                {
                    //to check what validator is failing (and not showing any error message on the page)
                }
            }
        }
    }

    protected void ReferCardButton_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            if (ReferCardButtonClicked != null)
                ReferCardButtonClicked();
        }
        else
        {
            foreach (var validator in Page.Validators)
            {
                if (!((IValidator)validator).IsValid)
                {
                    //to check what validator is failing (and not showing any error message on the page)
                }
            }
        }
    }

    protected void ComplianceButton_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            if (ComplianceButtonClicked != null)
                ComplianceButtonClicked();
        }
        else
        {
            foreach (var validator in Page.Validators)
            {
                if (!((IValidator)validator).IsValid)
                {
                    //to check what validator is failing (and not showing any error message on the page)
                }
            }
        }
    }

    protected void CallCenterButton_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            if (CallCenterButtonClicked != null)
                CallCenterButtonClicked();
        }
        else
        {
            foreach (var validator in Page.Validators)
            {
                if (!((IValidator)validator).IsValid)
                {
                    //to check what validator is failing (and not showing any error message on the page)
                }
            }
        }
    }

    protected void RejectApprovalRequestButton_Click(object sender, EventArgs e)
    {
       
       if (RejectApprovalRequestButtonClicked != null)
            RejectApprovalRequestButtonClicked();
       
    }

    protected void RetryButton_Click(object sender, EventArgs e)
    {
        btnRetry.Style.Add(HtmlTextWriterStyle.Display, "");
        (BasePage as IExecutePage).RepostTransaction();

    }

    private void GenerateReport(string strDocType, string strDocTypeName, bool scan, bool isDualScan)
    {
        try
        {
            var basePage = Page as VeriBranchTransactionCommonBasePage;
            if (basePage != null)
            {
                //print
                basePage.AutoPrint(VPConfigurationManager.PrintingType, strDocType, strDocTypeName, scan, isDualScan);
            }
        }
        catch (Exception ex)
        {
        }
    }
    private void GenerateReport(string strDocType, string strDocTypeName)
    {
        try
        {
            var basePage = Page as VeriBranchTransactionCommonBasePage;
            if (basePage != null)
            {
                //print
                basePage.AutoPrint(VPConfigurationManager.PrintingType, strDocType, strDocTypeName, false);
            }
        }
        catch (Exception ex)
        {
        }
    }

    public string AccountNumber
    {
        get
        {
            if (SafePageController.GetStateValue("NavigationButtonControl.AccountNo") != null)
                return (string)SafePageController.GetStateValue("NavigationButtonControl.AccountNo");

            return string.Empty;
        }
        set
        {
            SafePageController.SetStateValue("NavigationButtonControl.AccountNo", value);

            var url = SvsExternalUrl;
            if (!string.IsNullOrEmpty(url))
            {
                btnSignature.OnClientClick = string.Format("openSVS('{0}'); return false;", url.Replace("#AccountNumber#", AccountNumber));
            }

        }
    }

    public string ReceiptButtonClientID
    {
        get
        {
            return btnReciptNavigation.ClientID;
        }
    }

    public string BackButtonClientID
    {
        get
        {
            return btnBackNavigation.ClientID;
        }
    }

    public string NextButtonClientID
    {
        get
        {
            return btnNextNavigation.ClientID;
        }
    }

    public string ScanButtonClientID
    {
        get
        {
            return btnScanNavigation.ClientID;
        }
    }

    public string AdviceButtonClientID
    {
        get
        {
            return btnPrintNavigation.ClientID;
        }
    }

    private DataTable GetEmptyReason()
    {
        var dt = new DataTable();
        dt.Columns.Add("Reason");

        dt.Rows.Add("11");

        return dt;
    }

    IEnumerable<XElement> Steps
    {
        get
        {
            return SafePageController.GetStateValue(VpApplicationControllerConstants.Transactions.TxnProcessSteps) as List<XElement>;
        }
    }
    XElement GetCurrentNode()
    {
        return Steps.SingleOrDefault(s => Page.ResolveUrl(s.Attribute("url").Value) == Request.CurrentExecutionFilePath);
    }
    public string GetNextUrl()
    {
        var currentNode = GetCurrentNode();
        if (currentNode != null)
        {
            var nextNode = GetNextNode(Steps, currentNode);
            if (nextNode != null)
            {
                return nextNode.Attribute("url").Value;
            }
        }
        return string.Empty;
    }
    XElement GetNextNode(IEnumerable<XElement> list, XElement current)
    {
        XElement next = null;
        var index = 1;
        var xElements = list as XElement[] ?? list.ToArray();
        foreach (var element in xElements)
        {
            if (element.Attribute("url") == current.Attribute("url") && xElements.Length > index)
                next = xElements.ElementAt(index);
            else
                index++;
        }

        return next;
    }
    XElement GetPreviousNode(IEnumerable<XElement> list, XElement current)
    {
        XElement next = null;
        var xElements = list as XElement[] ?? list.ToArray();

        for (var i = 1; i < xElements.Length; i++)
        {
            if (xElements.ElementAt(i).Attribute("url") == current.Attribute("url"))
                next = xElements.ElementAt(i - 1);
        }

        return next;
    }
    public string GetBackUrl()
    {
        var currentNode = GetCurrentNode();
        if (currentNode != null)
        {
            var nextNode = GetPreviousNode(Steps, currentNode);
            if (nextNode != null)
            {
                return nextNode.Attribute("url").Value;
            }
        }
        return string.Empty;
    }
    PageType NextPageType
    {
        get
        {
            return GetPageByType(GetNextUrl());
        }
    }
    PageType PreviousPageType
    {
        get
        {
            return GetPageByType(GetBackUrl());
        }
    }
    private static PageType GetPageByType(string nextUrl)
    {
        if (!string.IsNullOrEmpty(nextUrl))
        {
            var isConfirmPage = nextUrl.IndexOf(VpUserControlConstants.VpNavigationControlConstant.CONFIRM_URL, StringComparison.Ordinal) > 0;
            if (isConfirmPage) return PageType.Confirm;
            var isScanPage = nextUrl.IndexOf(VpUserControlConstants.VpNavigationControlConstant.SCAN_URL, StringComparison.Ordinal) > 0;
            if (isScanPage) return PageType.Scan;
            var isChecklistPage = nextUrl.IndexOf(VpUserControlConstants.VpNavigationControlConstant.CHECKLIST_URL, StringComparison.Ordinal) > 0;
            if (isChecklistPage) return PageType.Checklist;
            var isExecutePage = nextUrl.IndexOf(VpUserControlConstants.VpNavigationControlConstant.EXECUTE_URL, StringComparison.Ordinal) > 0;
            if (isExecutePage) return PageType.Execute;
        }
        return PageType.None;
    }
    PageType pageType
    {
        get
        {
            var basePage = BasePage as VeriBranchTransactionCommonBasePage;
            return basePage != null ? basePage.PageType : PageType.None;
        }
    }

    protected string AppNo
    {
        get
        {
            return (string)SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
        }
    }

    protected string CifNo
    {
        get
        {
            if (SafePageController.Customer != null && SafePageController.Customer.User != null)
                return SafePageController.Customer.User.CifNo;
            return string.Empty;
        }
    }

    protected string NoButtonText
    {
        get { return GetGlobalResource(VPResourceConstants.ICommon.NO); }
    }

    protected string YesButtonText
    {
        get { return GetGlobalResource(VPResourceConstants.ICommon.YES); }
    }

    protected string RepostTransactionMessage
    {
        get { return GetLocalResource("RepostTransactionMessage"); }
    }

    protected void btnPrintCheque_Click(object sender, EventArgs e)
    {
        if (PrintChequeButtonClicked != null)
            PrintChequeButtonClicked();

        GenerateReport("CHQ", "Cheque", true, BasePage.DualScanForCheque);
    }

    protected void hidScanOption_ValueChanged(object sender, EventArgs e)
    {
        if (ReceiptButtonClicked != null)
            ReceiptButtonClicked();

        GenerateReport("ADV", "Advice", hidScanOption.Value == "1", BasePage.DualScanForReceipt);

        hidScanOption.Value = string.Empty;
    }

    protected void btnReserved1_Click(object sender, EventArgs e)
    {
        if (Reserved1ButtonClicked != null)
            Reserved1ButtonClicked();
    }

    protected void btnAuthorize_Click(object sender, EventArgs e)
    {
        if (AuthorizeButtonClicked != null)
            AuthorizeButtonClicked();
    }
    
    protected string SvsExternalUrl
    {
        get
        {
            return VpConfigurationParameters.GetGenericParameter("SVSExternalUrl");
        }
    }

    protected void btnReject_Click(object sender, EventArgs e)
    {
        if (RejectButtonClicked != null)
            RejectButtonClicked();
    }

    protected void btnResubmit_Click(object sender, EventArgs e)
    {
        if (ResubmitButtonClicked != null)
            ResubmitButtonClicked();
    }

    protected void btnAcceptRejection_Click(object sender, EventArgs e)
    {
        if (AcceptRejectionButtonClicked != null)
            AcceptRejectionButtonClicked();
    }

    /// <summary>
    /// Getter Setter to enable / disable the Print button
    /// </summary>
    public bool PrintAcknowledgementEnable
    {
        get { return btnPrintNavigation.Enabled; }
        set { btnPrintNavigation.Enabled = value; }
    }
}
