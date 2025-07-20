using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Common.Definitions.FundsTransfer;
using Common.Definitions.ServicesManagement;
using Common.Definitions.TransactionUtilityManagement;
using FundsModule.MessageDefinitions.ApproveFundsTransfer;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.FormatUtilities;
using VeriBranch.Framework.Definitions;
using VeriBranch.WebApplication.Helpers;
using VeriBranch.WebApplication.UIProcess;
using VeriBranch.WebApplication.Constants;
using VeriBranch.WebApplication.ObjectModel;
using System.Web.UI;

public partial class CVUTransactionDetails : VeriBranchTransactionCommonBasePage
{
    private const string NAVIGATION_CLIENT_CLICK_EVENT = "onclick";
    CultureInfo ci = new CultureInfo(HelperBase.EnglishCulture);

    protected override void DoPageAction()
    {
        SecureVoidExecute.Execute(FetchTransactionDetails);
        ifrTransactionDetails.Attributes.Add("onload", "iframeOnLoad(this);");
        ctlNavigationButton.BackUrl = string.Format("{0}{1}", Request.UrlReferrer.AbsolutePath, Request.UrlReferrer.Query);
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        var authorizeMsg = GetLocalResource("AuthorizeConfirmMessage");
        var revertMsg = GetLocalResource("RevertConfirmMessage");
        var resetAndAuthorizeMsg = GetLocalResource("ResetAndAuthorizeConfirmMessage");

        ctlNavigationButton.AuthorizeButton.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, "if (confirm('" + authorizeMsg + "')) {ClickButton('" + ctlNavigationButton.AuthorizeButton.ClientID + "');} else {return;} ");
        ctlNavigationButton.RejectButton.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, "if (confirm('" + revertMsg + "')) {ClickButton('" + ctlNavigationButton.RejectButton.ClientID + "');} else {return;} ");
        ctlNavigationButton.Reserved1Button.Attributes.Add(NAVIGATION_CLIENT_CLICK_EVENT, "if (confirm('" + resetAndAuthorizeMsg + "')) {ClickButton('" + ctlNavigationButton.Reserved1Button.ClientID + "');} else {return;} ");
       
     
    }

    protected override void GetStateFromUI()
    {
    }

    protected override void SetUIFromState()
    {
    }

    protected override void LocalizePageContent()
    {
    }

    string HostReferenceNumber
    {
        get { return ViewState["HostReferenceNumber"] as string; }
        set { ViewState["HostReferenceNumber"] = value; }
    }

    protected string SvsExternalUrl
    {
        get
        {
            return VpConfigurationParameters.GetGenericParameter("SVSExternalUrl");
        }
    }

    protected void ctlNavigationButton_SignatureButtonClicked()
    {
        FetchTransactionDetails();
        
    }

    private void FetchTransactionDetails()
    {
        var referenceNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber) as string;

        if (!string.IsNullOrEmpty(referenceNumber))
        {
            var response = ServicesHelper.DoOnlineTransactionDetailsInquiry(referenceNumber, true);

            var startUrl = GetTransactionUrl(response.UserTransactionDetails.OperationCode);
            ctlNavigationButton.ModifyUrl = startUrl;

            SetHeaderLabel(GetPageHeaderForUrl(startUrl));

            SecureVoidExecute.Execute(SetTransactionDetails, response);
            SetTransactionReferenceNumber(response.UserTransactionDetails.TransactionReferenceNumber);
            SafePageController.SetStateValue(VpPageControllerConstants.Transaction.OperationCode, response.UserTransactionDetails.OperationCode);
            SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionDescription, response.UserTransactionDetails.TransactionDescription);
            var startPageUrl = GetTransactionUrl(response.UserTransactionDetails.OperationCode);
            //If transaction start contains Start.aspx or List.aspx, then replace the URL with Execute.aspx
            //List.aspx was added after introduction of SADAD payments, since SADAD payments had first page as List.aspx
            if (startPageUrl.Contains("Start.aspx"))
                startPageUrl = startPageUrl.Replace("Start.aspx", "Execute.aspx");
            else if (startPageUrl.Contains("List.aspx"))
                startPageUrl = startPageUrl.Replace("List.aspx", "Execute.aspx");
            var confirmPageUrl = "http://" + Request.Url.Authority + ResolveUrl(startPageUrl);
            confirmPageUrl = AddQueryStringToUrl(confirmPageUrl, "ForReversal", "1");
            confirmPageUrl = AddQueryStringToUrl(confirmPageUrl, "ReversalReferenceNumber",
                                                 response.UserTransactionDetails.TransactionReferenceNumber);
            confirmPageUrl = confirmPageUrl.Replace("http://" + Request.Url.Authority, "__hostname__");
            //ifrTransactionDetails.Attributes.Add("src", confirmPageUrl);
            var script = string.Format("var txnUrl = '{1}';var ifrTD = document.getElementById('{0}'); if(ifrTD) {{setTimeout(function(){{showOverlay();ifrTD.src=txnUrl.replace('__hostname__', location.protocol + '//' + location.host);}}, 100);}}", ifrTransactionDetails.ClientID, confirmPageUrl);
            ScriptManager.RegisterStartupScript(this, GetType(), "showOverlay", script, true);
        }
    }

    //private List<TransactionLogs> GetTransactionLogs(VpTransactionLogEntry[] transactionLogEntryList)
    //{
    //    var logs = new List<TransactionLogs>();

    //    transactionLogEntryList = transactionLogEntryList.Where(l => l.IsLogTableField).ToArray();

    //    for (var i = 0; i < transactionLogEntryList.Length; i += 3)
    //    {
    //        var log = new TransactionLogs();

    //        log.Label1 = transactionLogEntryList[i].FieldDisplayNameLS;
    //        log.Value1 = transactionLogEntryList[i].Value.ToString();

    //        if (i + 1 < transactionLogEntryList.Length)
    //        {
    //            log.Label2 = transactionLogEntryList[i + 1].FieldDisplayNameLS;
    //            log.Value2 = transactionLogEntryList[i + 1].Value.ToString();

    //            if (i + 2 < transactionLogEntryList.Length)
    //            {
    //                log.Label3 = transactionLogEntryList[i + 2].FieldDisplayNameLS;
    //                log.Value3 = transactionLogEntryList[i + 2].Value.ToString();
    //            }
    //        }

    //        logs.Add(log);
    //    }

    //    return logs;
    //}

    private CVUStatusEnum cvuStatus
    {
        set { ViewState["cvuStatus"] = value; }
        get
        {
            if (ViewState["cvuStatus"] != null)
            {
                return ((CVUStatusEnum) Enum.Parse(typeof (CVUStatusEnum), ViewState["cvuStatus"].ToString(), true));
            }
            return CVUStatusEnum.Unassigned;
        }
    }
    private RiskStatusEnum riskStatus
    {
        set { ViewState["riskStatus"] = value; }
        get
        {
            if (ViewState["riskStatus"] != null)
            {
                return ((RiskStatusEnum)Enum.Parse(typeof(RiskStatusEnum), ViewState["riskStatus"].ToString(), true));
            }
            return RiskStatusEnum.Unassigned;
        }
    }

    private void SetTransactionDetails(VPOnlineTransactionDetailsInquiryResponse response)
    {
        cvuStatus = response.UserTransactionDetails.CVUStatus;
        riskStatus = response.UserTransactionDetails.RiskStatus;
        pnlComments.Visible = false;

        txtComments.Text = string.Empty;

        lblCustomerNameValue.Text = response.UserTransactionDetails.CustomerName;
        lblCustomerSegmentValue.Text = response.UserTransactionDetails.CustomerSegment;
        lblCustomerRankValue.Text = response.UserTransactionDetails.CustomerRank;

        if (response.UserTransactionDetails.TransactionName == TransactionNameContants.CHEQUE_DEPOSIT)
            ctlNavigationButton.AccountNumber = string.Empty;
        else
        ctlNavigationButton.AccountNumber = response.UserTransactionDetails.AccountNumber;

        ctlNavigationButton.AuthorizeButtonVisible =
            ctlNavigationButton.RejectButtonVisible =
            ctlNavigationButton.AcceptRejectionButtonVisible =
            ctlNavigationButton.ResubmitButtonVisible =
            ctlNavigationButton.Reserved1ButtonVisible = false;


        lblReferenceNoValue.Text = response.UserTransactionDetails.TransactionReferenceNumber;
        if (response.UserTransactionDetails.TransactionDate != DateTime.MinValue)
            lblTransactionDateValue.Text = response.UserTransactionDetails.TransactionDate.ToString(VpConstants.Dates.DateTimeFormat, Ci);
        lblTransactionTypeValue.Text = GetGlobalResource(string.Format("{0}.TxnDisplayName", response.UserTransactionDetails.TransactionName));

        HostReferenceNumber = response.UserTransactionDetails.HostReferenceNumber;
        lblHostReferenceNumberValue.Text = response.UserTransactionDetails.HostReferenceNumber;
        lblOverrideReasonValue.Text = response.UserTransactionDetails.Info3;


        var channel = Enum.Parse(typeof(ChannelTypeEnum), (response.UserTransactionDetails.Channel - 1).ToString());
        lblChannelValue.Text = GetGlobalResource(string.Format("Channel.{0}.DisplayName", channel));

        lblCVUStatusValue.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + response.UserTransactionDetails.CVUStatus);

        if (response.UserTransactionDetails.CVUAuditLogs != null)
        {
            //response.UserTransactionDetails.CVUAuditLogs.Sort((a, b) => ((a.UpdatedOn < b.UpdatedOn) ? 0 : 1));
            grdCVUAuditLog.DataSource = response.UserTransactionDetails.CVUAuditLogs;
            grdCVUAuditLog.DataBind();
        }
        else
        {
            grdCVUAuditLog.Visible = false;
        }

        ctlNavigationButton.ShowScanPage = true;

        if (response.UserTransactionDetails.CVUStatus == CVUStatusEnum.Resubmitted
            || response.UserTransactionDetails.CVUStatus == CVUStatusEnum.RevertedToChecker
            || response.UserTransactionDetails.CVUStatus == CVUStatusEnum.Submitted
            || response.UserTransactionDetails.CVUStatus == CVUStatusEnum.ApprovedByChecker)
        {
            rfvtxtComments.Enabled = true;
            rfvddlRiskStatusValue.Enabled = true;
            rfvddlRiskStatusValue.InitialValue = RiskStatusEnum.Unassigned.ToString();

            if (response.UserTransactionDetails.IsCallCenterApproved != null && response.UserTransactionDetails.IsCallCenterApproved == true)
            {
                cbOriginalDocReceived.Enabled = true;
            }
            ctlNavigationButton.AcceptRejectionButton.ValidationGroup = rfvtxtComments.ValidationGroup;
            ctlNavigationButton.AcceptRejectionButton.CausesValidation = true;


            ddlRiskStatusValue.Visible = true;

            ddlRiskStatusValue.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + RiskStatusEnum.Unassigned), RiskStatusEnum.Unassigned.ToString()));
            ddlRiskStatusValue.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + RiskStatusEnum.Low), RiskStatusEnum.Low.ToString()));
            ddlRiskStatusValue.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + RiskStatusEnum.Medium), RiskStatusEnum.Medium.ToString()));
            ddlRiskStatusValue.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + RiskStatusEnum.High), RiskStatusEnum.High.ToString()));

            var item = ddlRiskStatusValue.Items.FindByValue(response.UserTransactionDetails.RiskStatus.ToString());

            CVUErrorTypesInquiryResponse CVUErrorTypeResponse = ServicesHelper.GetCVUErrorTypesInquiry();

            if (CVUErrorTypeResponse != null && CVUErrorTypeResponse.CVUErrorTypes != null)
            {
                foreach (var CVUErrorType in CVUErrorTypeResponse.CVUErrorTypes)
                {
                    ddlErrorType.Items.Add(new ListItem(SafePageController.UserLanguage == HelperBase.ArabicCulture ? CVUErrorType.DescriptionAR : CVUErrorType.DescriptionEN, CVUErrorType.Code));
                }

                ddlErrorType.Enabled = true;

            }

        

            if (item != null)
            {
                item.Selected = true;
            }
        }
        else
        {
            lblRiskStatusValue.Visible = true;
            lblRiskStatusValue.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + response.UserTransactionDetails.RiskStatus);
        }


        if (HasPrivilege(TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS) && response.UserTransactionDetails.CVUStatus == CVUStatusEnum.RevertedToBranch && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS)
        {
            pnlComments.Visible = true;
            ctlNavigationButton.ResubmitButtonVisible = true;
        }
        else if (HasPrivilege(TransactionNameContants.CVU_BRANCH_TELLER_BATCHES)
             && (response.UserTransactionDetails.CVUStatus == CVUStatusEnum.Submitted)
             && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.CVU_BRANCH_TELLER_BATCHES
            )
        {
            pnlComments.Visible = true;
            ctlNavigationButton.AuthorizeButtonVisible = true;
        }
        else if (HasPrivilege(TransactionNameContants.CHECKER_CVU_REVERTED_TRANSACTIONS) &&
            (response.UserTransactionDetails.CVUStatus == CVUStatusEnum.RevertedToChecker)
            && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.CHECKER_CVU_REVERTED_TRANSACTIONS
            )
        {
            pnlComments.Visible = true;
            ctlNavigationButton.AuthorizeButtonVisible = ctlNavigationButton.AcceptRejectionButtonVisible = true;
        }
        else if (HasPrivilege(TransactionNameContants.CVU_AUTHORIZE_BRANCH_TELLER_BATCHES) &&
            (response.UserTransactionDetails.CVUStatus == CVUStatusEnum.ApprovedByChecker || response.UserTransactionDetails.CVUStatus == CVUStatusEnum.Resubmitted) && 
            SafePageController.QueryStrings["TxnName"] == TransactionNameContants.CVU_AUTHORIZE_BRANCH_TELLER_BATCHES)
        {
            pnlComments.Visible = true;
            ctlNavigationButton.AuthorizeButtonVisible = ctlNavigationButton.RejectButtonVisible =  ctlNavigationButton.Reserved1ButtonVisible = true;
            ctlNavigationButton.Reserved1ButtonText = GetLocalResource("Reserved1Button.Text");
        }

    }

    //private void LoadCVUErrorType()
    //{
        
    //    CVUErrorTypesInquiryResponse response = ServicesHelper.GetCVUErrorTypesInquiry();

    //    re
        
    //}

    private VPGenerateRequestResponseForRefNumberResponse VpGenerateRequestResponseForRefNumberResponse(string referenceNumber)
    {
        var response = TransactionUtilitiesHelper.DoGenerateObjectsForReferenceNumber(new VPGenerateRequestResponseForRefNumberRequest
                    {
                        InquiryType = InquiryTypeEnum.RequestAndResponse,
                        TransactionReferenceNumber = referenceNumber,
                        TransactionStage = OperationStageEnumeration.CloseCrmCase
                    });

        if (!response.IsSuccess)
        {
            response = TransactionUtilitiesHelper.DoGenerateObjectsForReferenceNumber(new VPGenerateRequestResponseForRefNumberRequest
            {
                InquiryType = InquiryTypeEnum.RequestAndResponse,
                TransactionReferenceNumber = referenceNumber,
                TransactionStage = OperationStageEnumeration.Execute
            });
        }

        return response;
    }

    protected void ctlNavigationButton_ReceiptButtonClicked()
    {
        try
        {
            var referenceNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber) as string;

            var response = VpGenerateRequestResponseForRefNumberResponse(referenceNumber);

            if (response.RequestItem is VPPerformReversalOperationRequest && response.ResponseItem is VPPerformReversalOperationResponse)
            {
                var original = VpGenerateRequestResponseForRefNumberResponse((response.ResponseItem as VPPerformReversalOperationResponse).TxnReferenceNumber);
                SecureVoidExecute.Execute(ReportHelper.PrepareReprintReversalReportData, original.RequestItem, original.ResponseItem, response.RequestItem as VPPerformReversalOperationRequest, response.ResponseItem as VPPerformReversalOperationResponse);

                SafePageController.SetStateValue("OriginalTransactionRequest", response.RequestItem);
                SafePageController.SetStateValue("OriginalTransactionResponse", response.ResponseItem);
            }
            else
            {
                SecureVoidExecute.Execute(ReportHelper.PrepareReprintReportData, response.RequestItem, response.ResponseItem);
            }



            FetchTransactionDetails();
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    protected void ctlNavigationButton_AcceptRejectionButtonClicked()
    {
        bool? originalDocReceived = null;

        if (cbOriginalDocReceived.Enabled)
            originalDocReceived = cbOriginalDocReceived.Checked;

        var response = GetApproveResponse(CVUStatusEnum.RevertedToBranch, ddlErrorType.SelectedValue, originalDocReceived, txtComments.Text.Trim(), ((RiskStatusEnum)Enum.Parse(typeof(RiskStatusEnum), ddlRiskStatusValue.SelectedValue, true)));

        if (response != null && response.VblResponse != null && response.VblResponse.IsSuccess)
        {
            SetInformationDisplay(GetLocalResource("TxnRevertedToBranchSuccessfully"));
        }
        else
        {
            AlertModal(GetErrorMessageFromResponse(response.VblResponse));
        }
        FetchTransactionDetails();
    }

    protected void ctlNavigationButton_ResubmitButtonClicked()
    {
        var response = GetApproveResponse(CVUStatusEnum.Resubmitted, ddlErrorType.SelectedValue, null, txtComments.Text.Trim());

        if (response != null && response.VblResponse != null && response.VblResponse.IsSuccess)
        {
            SetInformationDisplay(GetLocalResource("TxnResubmittedSuccessfully"));
        }
        else
        {
            AlertModal(GetErrorMessageFromResponse(response.VblResponse));
        }

        FetchTransactionDetails();
    }

    protected void ctlNavigationButton_RejectButtonClicked()
    {
        var newRiskStatus = ((RiskStatusEnum)Enum.Parse(typeof(RiskStatusEnum), ddlRiskStatusValue.SelectedValue, true));

        bool? originalDocReceived = null;

        if (cbOriginalDocReceived.Enabled)
            originalDocReceived = cbOriginalDocReceived.Checked;

        var response = GetApproveResponse(CVUStatusEnum.RevertedToBranch, ddlErrorType.SelectedValue, originalDocReceived, txtComments.Text.Trim(), newRiskStatus);

        if (response != null && response.VblResponse != null && response.VblResponse.IsSuccess)
        {
            SetInformationDisplay(GetLocalResource("TxnRevertedToBranchSuccessfully"));
        }
        else
        {
            AlertModal(GetErrorMessageFromResponse(response.VblResponse));
        }

        FetchTransactionDetails();
    }

    protected void ctlNavigationButton_AuthorizeButtonClicked()
    {
        var newRiskStatus = ((RiskStatusEnum)Enum.Parse(typeof(RiskStatusEnum), ddlRiskStatusValue.SelectedValue, true));
        var newCVUStatus = cvuStatus == CVUStatusEnum.ApprovedByChecker || cvuStatus == CVUStatusEnum.Resubmitted ? CVUStatusEnum.Completed : CVUStatusEnum.ApprovedByChecker;

        bool? originalDocReceived = null;

        if (cbOriginalDocReceived.Enabled)
            originalDocReceived = cbOriginalDocReceived.Checked;


        var response = GetApproveResponse(newCVUStatus, ddlErrorType.SelectedValue, originalDocReceived, txtComments.Text, newRiskStatus);

        if (response != null && response.VblResponse != null && response.VblResponse.IsSuccess)
        {
            SetInformationDisplay(GetLocalResource("TxnApprovedSuccessfully"));
        }
        else
        {
            AlertModal(GetErrorMessageFromResponse(response.VblResponse));
        }
        FetchTransactionDetails();
    }

    protected void ctlNavigationButton_ReservedButtonClicked()
    {
        var newRiskStatus = ((RiskStatusEnum)Enum.Parse(typeof(RiskStatusEnum), ddlRiskStatusValue.SelectedValue, true));
        var newCVUStatus = cvuStatus == CVUStatusEnum.ApprovedByChecker || cvuStatus == CVUStatusEnum.Resubmitted ? CVUStatusEnum.Completed : CVUStatusEnum.ApprovedByChecker;

        bool? originalDocReceived = null;

        if (cbOriginalDocReceived.Enabled)
            originalDocReceived = cbOriginalDocReceived.Checked;

        var response = GetApproveResponse(newCVUStatus, ddlErrorType.SelectedValue, originalDocReceived, txtComments.Text, newRiskStatus, true);

        if (response != null && response.VblResponse != null && response.VblResponse.IsSuccess)
        {
            SetInformationDisplay(GetLocalResource("TxnApprovedSuccessfully"));
        }
        else
        {
            AlertModal(GetErrorMessageFromResponse(response.VblResponse));
        }
        FetchTransactionDetails();
    }

    ExtendedResponseTransactionData<VPUpdateTransactionForCVUApprovalOperationResponse> GetApproveResponse(CVUStatusEnum action, string CVUErrorType, Nullable<bool> IsOriginalDocumentReceived, string comments = "", RiskStatusEnum riskStatus = RiskStatusEnum.Unassigned, bool resetHistory = false)
    {
        var referenceNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber) as string;
        var request = new VPUpdateTransactionForCVUApprovalOperationRequest
            {
                CVUStatus = action,
                CVURemarks = comments,
                TransactionReferenceNumber = referenceNumber,
                TransactionRiskStatus = riskStatus,
                CVUErrorType = CVUErrorType,
                ResetHistory = resetHistory,
                IsOriginalDocumentReceived = IsOriginalDocumentReceived
            };

        return ServicesHelper.DoUpdateTransactionForCVUAppoval(request);
    }

    protected void grdCVUAuditLog_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var cvuDetails = ((CVUAuditLog)e.Row.DataItem);

                var litCVUStatus = e.Row.FindControl("litCVUStatus") as Literal;
                if (litCVUStatus != null)
                {
                    litCVUStatus.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + cvuDetails.CVUStatus);
                }


                var litCVUErrorType = e.Row.FindControl("litCVUErrorType") as Literal;
                if (litCVUErrorType != null)
                {
                    litCVUErrorType.Text = SafePageController.UserLanguage == HelperBase.ArabicCulture ? cvuDetails.CVUErrorTypeAR : cvuDetails.CVUErrorTypeEN;
                }

                var litRiskStatus = e.Row.FindControl("litRiskStatus") as Literal;
                if (litRiskStatus != null)
                {
                    litRiskStatus.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + cvuDetails.RiskStatus);
                }

                var litIsOriginalDocumentReceived = e.Row.FindControl("litIsOriginalDocumentReceived") as Literal;
                if (litIsOriginalDocumentReceived != null)
                {
                    if (cvuDetails.IsOriginalDocumentReceived != null)
                    {
                        litIsOriginalDocumentReceived.Text = Convert.ToBoolean(cvuDetails.IsOriginalDocumentReceived) ? GetGlobalResource("Yes.Text") : GetGlobalResource("No.Text");
                    }
                    
                }

                

                var litUpdatedOn = e.Row.FindControl("litUpdatedOn") as Literal;
                if (litUpdatedOn != null && cvuDetails.UpdatedOn != DateTime.MinValue)
                {
                    litUpdatedOn.Text = cvuDetails.UpdatedOn.ToString(VpConstants.Dates.DateTimeFormat, ci);
                }

            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }
}
