using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Common.Definitions.ServicesManagement;
using VeriBranch.Framework.Definitions;
using VeriBranch.WebApplication.CustomControls;
using VeriBranch.WebApplication.Helpers;
using VeriBranch.WebApplication.UIProcess;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.Constants;
using Common.Definitions.TransactionUtilityManagement;
using VeriBranch.WebApplication.ObjectModel;
using VeriBranch.WebApplication.Exceptions;
using Common.Definitions.AccountsManagement;
using AccountManagement.MessageDefinitions.GetAccountDetail;
using Common.Definitions.UserManagement;
using CustomerManagement.MessageDefinitions.GetCustomerInfo;
using IAL.VpExternalCallBase.CoreApi.Interfaces;
using VeriBranch.WebApplication.FormatUtilities;
using FundsModule.MessageDefinitions.PaymentStatusInquiry;
using Common.Definitions.FundsTransfer;


public partial class LocalOperations_WaitingForApprovalTransactionsList : VeriBranchTransactionCommonBasePage
{
    
    CultureInfo ci = new CultureInfo(HelperBase.EnglishCulture);

    //protected delegate ExtendedResponseTransactionData<ResponseTransactionData> TransactionOperation(RequestTransactionData request, OperationStageEnumeration stage);

    //protected abstract TransactionOperation Operation { get; }

    protected override void GetStateFromUI()
    {
        grdTransactionList.SaveStateEvent += GridView_SaveStateEvent;
        grdTransactionList.LoadStateEvent += GridView_LoadStateEvent;
    }

    protected override void SetUIFromState()
    {
    }

    protected override void DoPageAction()
    {
        LoadStatus();
        //LoadChannels();
        LoadTransactionNames();

        LoadAprovalDepartment();

        if (SafePageController.QueryStrings["TxnName"] == TransactionNameContants.REVERTED_TRANSFERS)
        {
            grdTransactionList.Columns[10].Visible = true;
            ctlFromDate.ChosenDate = DateTime.Today.AddDays(-7);
        }

        grdTransactionList.SaveStateEvent += GridView_SaveStateEvent;
        grdTransactionList.LoadStateEvent += GridView_LoadStateEvent;

        HandlePostBack(btnDisplay);

        txtPerformerIdentity.Text = SafePageController.Performer.User.UserName;

        try
        {
            if (Request.UrlReferrer.AbsolutePath.EndsWith("Start.aspx"))
            {
               

                var request = SafePageController.GetProfileValue("VpOnlineTransactionListInquiryRequest") as VpOnlineTransactionListInquiryRequest;
                
                if (request != null)
                {
                    //Set Login user
                    SafePageController.RequestData = null;

                    VPCustomerProfileInquiryResponse customerProfile =null;
                    if (request.Header.User.CifNo != VpConfigurationParameters.GetGenericParameter("NonCustomerCIF"))
                    {
                        var helper = new UserHelper();
                        helper.LoginToken = request.Header.LoginTokenKey;
                        customerProfile = helper.GetCustomerProfile(request.Header.User.CifNo);
                        if (!customerProfile.IsSuccess || customerProfile.IALCustomerInfoResponse == null)
                            throw new VPSystemException("LocalOperations_WaitingForApprovalTransactionsList", VPExceptionConstants.CUSTOMER_PROFILE_NOT_FOUND);

                        //Initialize Page controller's User (Customer)
                        InititializePageControllerForCustomer(request.Header.User, customerProfile.IALCustomerInfoResponse);
                    }
                    else
                    {
                        //Initialize Page controller's User (NonCustomerCIF)
                        InititializePageControllerForCustomer(request.Header.User, null);
                    }
                  


                    if (request.TransactionName != null)
                    {
                        var txnName = request.TransactionName.FirstOrDefault();
                        if (!string.IsNullOrEmpty(txnName) && ddlTransactionName.Items.FindByValue(txnName) != null)
                            ddlTransactionName.SelectedValue = txnName;
                    }

                    if (request.ApprovalDepartment != null)
                    {
                        var approvalDepartment = request.ApprovalDepartment;
                        if (!string.IsNullOrEmpty(approvalDepartment.ToString()) && ddlApprovalDepartment.Items.FindByValue(approvalDepartment.ToString()) != null)
                            ddlApprovalDepartment.SelectedValue = approvalDepartment.ToString();
                    }

                    ctlFromDate.ChosenDate = request.BeginDate;
                    ctlToDate.ChosenDate = request.EndDate;
                    
                    txtTransactionReferenceNumber.Text = request.TransactionReferenceNumber;
                    //txtHostReferenceNumber.Text = request.HostReferenceNumber;
                    txtPerformerIdentity.Text = request.PerformerIdentity;

                    //Only approved request can proceed with next step
                    if (request.TransactionStatus != null && request.TransactionStatus.Count > 2)
                        ddlTransactionStatus.SelectedValue = TransactionStatusEnum.All.ToString();
                    else
                        ddlTransactionStatus.SelectedValue = TransactionStatusEnum.Approved.ToString();

                    txtAccountNumber.Text = request.LoggedInAccountNumber;
                    cbOverDrawn.Checked = request.RC_IsOverDrawnRequested;
                    cbValueDate.Checked = request.RC_IsNewValueDateRequested;
                    cbExchangeRate.Checked = request.RC_IsNewExchangeRateRequested;
                    ctlAmountFrom.Amount = request.FromAmount;
                    ctlAmountTo.Amount = request.ToAmount;

                    FetchTransactions();

                    //Hide branch status
                    tblBranchStatus.Visible = false;
                    BranchStatusStep.Visible = false;

                    //Show Inquire Button only for refer card
                    if (request.ApprovalDepartment == ApprovalDepartmentEnum.ReferCard ||
                        request.ApprovalDepartment == ApprovalDepartmentEnum.All)
                        btnInquireStatus.Visible = true;
                    else
                        btnInquireStatus.Visible = false;

                    btnCancelRequest.Visible = false;

                    ddlTransactionStatus.Enabled = Convert.ToBoolean(SafePageController.GetProfileValue("ddlTransactionStatus_Enabled"));
                    ddlTransactionName.Enabled = Convert.ToBoolean(SafePageController.GetProfileValue("ddlTransactionName_Enabled"));
                }
            }
        }
        catch (Exception)
        {
        }
    }

    //private void LoadChannels()
    //{
    //    ddlChannel.Items.Clear();

    //    ddlChannel.Items.Add(new ListItem(GetGlobalResource(VPResourceConstants.ICommon.ALL), string.Empty));

    //    ddlChannel.Items.Add(new ListItem(GetGlobalResource(VpConstants.Services.ChannelResourceKey + ChannelTypeEnum.Branch + ".DisplayName"), ChannelTypeEnum.Branch.ToString()));
    //    //ddlChannel.Items.Add(new ListItem(GetGlobalResource(VpConstants.Services.ChannelResourceKey + ChannelTypeEnum.CallCenter + ".DisplayName"), ChannelTypeEnum.CallCenter.ToString()));
    //    //ddlChannel.Items.Add(new ListItem(GetGlobalResource(VpConstants.Services.ChannelResourceKey + ChannelTypeEnum.Internet + ".DisplayName"), ChannelTypeEnum.Internet.ToString()));
    //    //ddlChannel.Items.Add(new ListItem(GetGlobalResource(VpConstants.Services.ChannelResourceKey + ChannelTypeEnum.Mobile + ".DisplayName"), ChannelTypeEnum.Mobile.ToString()));
    //    //ddlChannel.Items.Add(new ListItem(GetGlobalResource(VpConstants.Services.ChannelResourceKey + ChannelTypeEnum.Kiosk + ".DisplayName"), ChannelTypeEnum.Kiosk.ToString()));

    //    //set the current channel as default
    //    ddlChannel.Items.FindByValue(ApplicationController.Channel.ToString()).Selected = true;
    //}

    private void LoadStatus()
    {
        ddlTransactionStatus.Items.Clear();

        ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VPResourceConstants.ICommon.ALL), TransactionStatusEnum.All.ToString()));

        ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.ApprovalPending), TransactionStatusEnum.ApprovalPending.ToString()));
        ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.Approved), TransactionStatusEnum.Approved.ToString()));
        ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.Rejected), TransactionStatusEnum.Rejected.ToString()));
        ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.Expired), TransactionStatusEnum.Expired.ToString()));
        ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.ApprovalCancelled), TransactionStatusEnum.ApprovalCancelled.ToString()));
        //ddlTransactionStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.Failed), TransactionStatusEnum.Failed.ToString()));
        
        var defaultStatus = Request["TxnStatus"];
        if (!string.IsNullOrEmpty(defaultStatus))
        {
            var item = ddlTransactionStatus.Items.FindByValue(defaultStatus);

            if (item != null)
            {
                item.Selected = true;
                ddlTransactionStatus.Enabled = false;
            }
        }
        else
        {
            //Operation stage is passed as RequestForApproval. It will only return request for approval operations
            ddlTransactionStatus.Items.FindByValue(TransactionStatusEnum.All.ToString()).Selected = true;
        }
    }

    private void LoadAprovalDepartment()
    {
        ddlApprovalDepartment.Items.Clear();

        ddlApprovalDepartment.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.ApprovalDepartmentResourceKey + ApprovalDepartmentEnum.All), ApprovalDepartmentEnum.All.ToString()));
        ddlApprovalDepartment.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.ApprovalDepartmentResourceKey + ApprovalDepartmentEnum.Compliance), ApprovalDepartmentEnum.Compliance.ToString()));
        ddlApprovalDepartment.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.ApprovalDepartmentResourceKey + ApprovalDepartmentEnum.ReferCard), ApprovalDepartmentEnum.ReferCard.ToString()));
        ddlApprovalDepartment.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.ApprovalDepartmentResourceKey + ApprovalDepartmentEnum.CallCenter), ApprovalDepartmentEnum.CallCenter.ToString()));
        
        //For IPS Branch Inquiry
        //ddlApprovalDepartment.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.ApprovalDepartmentResourceKey + ApprovalDepartmentEnum.IPS), ApprovalDepartmentEnum.IPS.ToString()));
                
        //Select default department
        ddlApprovalDepartment.Items.FindByValue(ApprovalDepartmentEnum.All.ToString()).Selected = true;

    }

    private void LoadTransactionNames()
    {
        ddlTransactionName.Items.Clear();

        var transactionNames = new[] { TransactionNameContants.CASH_DEPOSIT
            ,TransactionNameContants.CASH_CENTER_CASH_DEPOSIT
            , TransactionNameContants.CASH_WITHDRAWAL
            , TransactionNameContants.DENOMINATION_EXCHANGE
            , TransactionNameContants.CHEQUE_DEPOSIT
            , TransactionNameContants.CHEQUE_WITHDRAWAL
            , TransactionNameContants.TRANSFER_WITHIN_OWN_ACCOUNTS
            , TransactionNameContants.TRANSFER_TO_3RD_PARTY_IN_SAME_BANK
            , TransactionNameContants.OFFICIAL_CHEQUE_DEPOSIT
            , TransactionNameContants.ISSUE_OFFICIAL_CHEQUE
            , TransactionNameContants.OFFICIAL_CHEQUE_WITHDRAWAL
            , TransactionNameContants.PERFORM_REVERSAL_OPERATION

            , TransactionNameContants.DOMESTIC_TRANSFER
            , TransactionNameContants.INTERNATIONAL_TRANSFER
            , TransactionNameContants.LOCAL_CHEQUE_CLEARING
            , TransactionNameContants.INTERNATIONAL_CHEQUE_COLLECTION

            , TransactionNameContants.CAPITALMARKETTRANSFER_TRANSACTION
            , TransactionNameContants.IPO_SUBSCRIPTION
            , TransactionNameContants.DISHONORED_CHEQUE
            , TransactionNameContants.CURRENCY_EXCHANGE

            ,TransactionNameContants.BILL_PAYMENT
            ,TransactionNameContants.BILL_PAYMENT_BY_CASH
            ,TransactionNameContants.PREPAID_BILL_PAYMENT
            ,TransactionNameContants.PREPAID_BILL_PAYMENT_BY_CASH
            ,TransactionNameContants.MOI_BILL_PAYMENT_BY_ACCOUNT
            ,TransactionNameContants.MOI_BILL_PAYMENT_BY_CASH
            ,TransactionNameContants.RECONCILE_PAYMENT

            ,TransactionNameContants.REFUND_PAYOUT
            ,TransactionNameContants.PAYMENT_REVERSAL
            ,TransactionNameContants.REFUND_REQUEST

            ,TransactionNameContants.ITM_CASH_DEPOSIT
            ,TransactionNameContants.ITM_CASH_WITHDRAWAL
            ,TransactionNameContants.ITM_CHEQUE_DEPOSIT
            ,TransactionNameContants.ITM_CHEQUE_WITHDRAWAL
            ,TransactionNameContants.ITM_OFFICIAL_CHEQUE_WITHDRAWAL
            ,TransactionNameContants.ITM_TRANSFER_TO_3RD_PARTY_IN_SAME_BANK
            ,TransactionNameContants.ITM_MOI_BILL_PAYMENT_BY_ACCOUNT
            ,TransactionNameContants.ITM_PREPAID_BILL_PAYMENT
            ,TransactionNameContants.ITM_BILL_PAYMENT

        };

        ddlTransactionName.Items.Add(new ListItem(GetGlobalResource(VPResourceConstants.ICommon.ALL), string.Empty));

        if (SafePageController.QueryStrings["TxnName"] == TransactionNameContants.REVERTED_TRANSFERS)
        {
            ddlTransactionName.Items.Clear();
            transactionNames = new[]
                {
                    TransactionNameContants.DOMESTIC_TRANSFER
                    , TransactionNameContants.INTERNATIONAL_TRANSFER
                };
        }

        if (SafePageController.QueryStrings["TxnName"] == TransactionNameContants.DISHONORED_CHEQUES_INQUIRY)
        {
            ddlTransactionName.Items.Clear();
            transactionNames = new[]
                {
                    TransactionNameContants.DISHONORED_CHEQUE
                };
        }

        if (SafePageController.QueryStrings["TxnName"] == TransactionNameContants.IPO_RI_INQUIRY_TRANSACTION)
        {
            ddlTransactionName.Items.Clear();
            transactionNames = new[]
                {
                    TransactionNameContants.IPO_SUBSCRIPTION
                };
        }

        foreach (var name in transactionNames)
        {
            ddlTransactionName.Items.Add(new ListItem(GetGlobalResource(string.Format("{0}.TxnDisplayName", name)), name));
        }
    }

    protected void btnDisplay_Click(object sender, EventArgs e)
    {
        FetchTransactions();

        //Hide branch status
        tblBranchStatus.Visible = false;
        BranchStatusStep.Visible = false;
        
        //Show Inquire Button only for refer card
        var approvalDepartment = (ApprovalDepartmentEnum)Enum.Parse(typeof(ApprovalDepartmentEnum), ddlApprovalDepartment.SelectedValue, true);
        if(approvalDepartment == ApprovalDepartmentEnum.All ||
            approvalDepartment == ApprovalDepartmentEnum.ReferCard)
            btnInquireStatus.Visible = true;
        else
            btnInquireStatus.Visible = false;

        btnCancelRequest.Visible = false;

    }


    protected void btnInquireStatus_Click(object sender, EventArgs e)
    {
        int channelId = (int)(ChannelTypeEnum.Branch + 1);

        var maxRecordCount = 250;   //UI default value
        Int32.TryParse(VpConfigurationParameters.GetGenericParameter("InquiryTxnListMaxRecordCount"), out maxRecordCount);

        var txnName = TransactionNameContants.INQUIRY_TRANSACTION;

        var operationStages = new List<OperationStageEnumeration>();
        operationStages.Add(OperationStageEnumeration.RequestForApproval);
        //For IPS Inquiry  Branch
        //operationStages.Add(OperationStageEnumeration.CloseCrmCase);

        var transactionStatus = new List<TransactionStatusEnum>();
        transactionStatus.Add(TransactionStatusEnum.ApprovalPending);
        transactionStatus.Add(TransactionStatusEnum.Approved);

        var request = new VpOnlineTransactionListInquiryRequest
        {
            BeginDate = ctlFromDate.ChosenDate,
            EndDate = ctlToDate.ChosenDate,
            TransactionStatus = transactionStatus,
            ChannelID = channelId,
            FromAmount = Convert.ToInt64(ctlAmountFrom.Amount),
            ToAmount = Convert.ToInt64(ctlAmountTo.Amount),
            //UserID = SafePageController.TransactionHeader.User.UserId,
            CIF = SafePageController.Customer.User.CifNo == VpConfigurationParameters.GetGenericParameter("NonCustomerCIF") ? null : SafePageController.Customer.User.CifNo,
            TransactionReferenceNumber = txtTransactionReferenceNumber.Text.Trim(),
            LoggedInAccountNumber = txtAccountNumber.Text.Trim(),
            PerformerIdentity = txtPerformerIdentity.Text.Trim(),
            LastTransactionsCount = maxRecordCount,
            PriviledgeDepth = GetPrivilegeDepth(txnName),
            OperationStage = operationStages,
            RC_IsOverDrawnRequested = cbOverDrawn.Checked,
            RC_IsNewValueDateRequested = cbValueDate.Checked,
            RC_IsNewExchangeRateRequested = cbExchangeRate.Checked,
            ApprovalDepartment = ApprovalDepartmentEnum.ReferCard
        };



        if (!string.IsNullOrEmpty(ddlTransactionName.SelectedValue))
            request.TransactionName = new List<string>() { ddlTransactionName.SelectedValue };

        var response = ServicesHelper.DoOnlineTransactionListInquiry(request);

        var referenceNumbers = new List<string>();

        if (response != null && response.IsSuccess && response.UserTransactionList != null && response.UserTransactionList.Count > 0)
        {
            foreach (UserTransaction txn in response.UserTransactionList.UserTransactions)
            {
                //Inquire status for transactions which were sent to refer card
                if (txn.ApprovalDepartment == ApprovalDepartmentEnum.ReferCard)
                    referenceNumbers.Add(txn.TransactionReferenceNumber);
            }

            var inquireStatusResponse = InquireStatus(referenceNumbers);

            //If required display any message

            FetchTransactions();

            //Hide branch status
            tblBranchStatus.Visible = false;
            BranchStatusStep.Visible = false;
            btnCancelRequest.Visible = false;

        }

        /*For IPS Branch Release
         * //Fetch IPS Statuses


         var IPStransactionStatus = new List<TransactionStatusEnum>();
         IPStransactionStatus.Add(TransactionStatusEnum.ApprovalPending);


         var IPSrequest = new VpOnlineTransactionListInquiryRequest
         {
             BeginDate = ctlFromDate.ChosenDate,
             EndDate = ctlToDate.ChosenDate,
             TransactionStatus = IPStransactionStatus,
             ChannelID = channelId,
             FromAmount = Convert.ToInt64(ctlAmountFrom.Amount),
             ToAmount = Convert.ToInt64(ctlAmountTo.Amount),
             //UserID = SafePageController.TransactionHeader.User.UserId,
             CIF = SafePageController.Customer.User.CifNo == VpConfigurationParameters.GetGenericParameter("NonCustomerCIF") ? null : SafePageController.Customer.User.CifNo,
             TransactionReferenceNumber = txtTransactionReferenceNumber.Text.Trim(),
             LoggedInAccountNumber = txtAccountNumber.Text.Trim(),
             PerformerIdentity = txtPerformerIdentity.Text.Trim(),
             LastTransactionsCount = maxRecordCount,
             PriviledgeDepth = GetPrivilegeDepth(txnName),
             OperationStage = operationStages,
             ApprovalDepartment = ApprovalDepartmentEnum.IPS
         };



         if (!string.IsNullOrEmpty(ddlTransactionName.SelectedValue))
             request.TransactionName = new List<string>() { ddlTransactionName.SelectedValue };

         var IPSresponse = ServicesHelper.DoOnlineTransactionListInquiry(IPSrequest);
         string OrgInsIde = string.Empty; 
        
         var IPSreferenceNumbers = new List<string>();
         bool IsChecked = false;
         foreach (GridViewRow row in grdTransactionList.Rows)
         {
             RadioButton rb = (RadioButton)row.FindControl("btnBranchStatus");
             if (rb.Checked)
             {
                 IsChecked = true;
                 var approvalDepartment = row.FindControl("litApprovalDepartment") as Literal;
                 var litReferenceNumber = row.FindControl("litReferenceNumber") as Literal;
                 var SelectedRow = IPSresponse.UserTransactionList.UserTransactions.Where(x => x.TransactionReferenceNumber == litReferenceNumber.Text);
                 OrgInsIde = SelectedRow.FirstOrDefault().LogFields.Reserved1;
                 var inquireStatusResponse = InquireIPSStatus(OrgInsIde, litReferenceNumber.Text);

                 if (inquireStatusResponse != null && inquireStatusResponse.VblResponse != null &&
                     inquireStatusResponse.VblResponse.IALPaymentStatusInquiryResponse != null && inquireStatusResponse.VblResponse.IALPaymentStatusInquiryResponse.MsgPstSts != null)
                 {
                     var MsgStatus = inquireStatusResponse.VblResponse.IALPaymentStatusInquiryResponse.MsgPstSts;
                     if (MsgStatus.ToLower() == "post")
                     {
                         var msg = GetGlobalResource("IPSTransactionPostedMessage");
                         AlertModal(msg);
                     }
                     else if (MsgStatus.ToLower() == "rjct" || MsgStatus.ToLower() == "canc")
                     {
                         var msg = GetGlobalResource("IPSTransactionRejectMessage");
                         AlertModal(msg);
                     }
                     else if (MsgStatus.ToLower() == "wpost")
                     {
                         var msg = GetGlobalResource("IPSTransactionWPostMessage");
                         AlertModal(msg);
                     }
                    
                 }
                
             }
           

         }
         if (!IsChecked)
         {
             if (IPSresponse != null && IPSresponse.IsSuccess && IPSresponse.UserTransactionList != null && IPSresponse.UserTransactionList.Count > 0)
             {
                 OrgInsIde = IPSresponse.UserTransactionList.UserTransactions.FirstOrDefault().LogFields.Reserved1;
                 foreach (UserTransaction txn in IPSresponse.UserTransactionList.UserTransactions)
                 {
                     //Inquire status for transactions which were sent to IPS card
                     if (txn.ApprovalDepartment == ApprovalDepartmentEnum.IPS && txn.TransactionStatus == TransactionStatusEnum.ApprovalPending)
                     {
                         referenceNumbers.Add(txn.TransactionReferenceNumber);

                     }
                 }

                 //If required display any message

                 FetchTransactions();

                 //Hide branch status
                 tblBranchStatus.Visible = false;
                 BranchStatusStep.Visible = false;
                 btnCancelRequest.Visible = false;
             }
         }
        
         */

    

    }

    protected void btnCancelRequest_Click(object sender, EventArgs e)
    {

        foreach (GridViewRow row in grdTransactionList.Rows)
        {
            RadioButton rb = (RadioButton)row.FindControl("btnBranchStatus");
            if (rb.Checked)
            {
                var litRC_ReferenceNumber = row.FindControl("litRC_ReferenceNumber") as Literal;
                var litReferenceNumber = row.FindControl("litReferenceNumber") as Literal;

                if (string.IsNullOrEmpty(litRC_ReferenceNumber.Text))
                {
                    AlertModal("Invalid RC reference number.");
                }
                else
                {
                    var request = new VPReferCardCancelRequest
                     {
                         RC_ReferenceNumber = litRC_ReferenceNumber.Text,
                         Original_ReferenceNumber = litReferenceNumber.Text,
                         Action = ReferCardActionEnum.Cancelled
                     };

                    var response = ServicesHelper.DoReferCardCancelOperation(request);

                    if (response != null && response.IsSuccess)
                    {
                        FetchTransactions();

                        btnCancelRequest.Visible = false;
                    }
                    else
                    {
                        var errorMessage = "Failed to cancel Refer Card request. ";

                        if (response.Footer != null && response.Footer.Result != null && response.Footer.Result.Errors != null && response.Footer.Result.Errors.Count > 0)
                            errorMessage += response.Footer.Result.Errors[0].Description;

                        AlertModal(errorMessage);
                    }
                }


                
            }
        }
    }
    
    private void FetchTransactions()
    {
        int channelId = (int)(ChannelTypeEnum.Branch +1);

        var maxRecordCount = 250;   //UI default value
        Int32.TryParse(VpConfigurationParameters.GetGenericParameter("InquiryTxnListMaxRecordCount"), out maxRecordCount);

        var txnName = TransactionNameContants.INQUIRY_TRANSACTION;
      
        var operationStages = new List<OperationStageEnumeration>();
        operationStages.Add(OperationStageEnumeration.RequestForApproval);
        /*For IPS Branch
         operationStages.Add(OperationStageEnumeration.CloseCrmCase);
         */

        var selectedValue = (TransactionStatusEnum)Enum.Parse(typeof(TransactionStatusEnum), ddlTransactionStatus.SelectedValue, true);

        var approvalDepartment = (ApprovalDepartmentEnum)Enum.Parse(typeof(ApprovalDepartmentEnum), ddlApprovalDepartment.SelectedValue, true);

        var transactionStatus = new List<TransactionStatusEnum>();
        transactionStatus.Add(selectedValue);

        //Only below status needs to be fetched for approval requests
        if (selectedValue == TransactionStatusEnum.All)
        {
            transactionStatus.Remove(TransactionStatusEnum.All);
            transactionStatus.Add(TransactionStatusEnum.ApprovalCancelled);
            transactionStatus.Add(TransactionStatusEnum.ApprovalPending);
            transactionStatus.Add(TransactionStatusEnum.Approved);
            transactionStatus.Add(TransactionStatusEnum.Rejected);
            transactionStatus.Add(TransactionStatusEnum.Expired);
            transactionStatus.Add(TransactionStatusEnum.Executed);
        }

        if(selectedValue == TransactionStatusEnum.Approved)
        {
            transactionStatus.Add(TransactionStatusEnum.Executed);
        }
        
        var request = new VpOnlineTransactionListInquiryRequest
        {
            BeginDate = ctlFromDate.ChosenDate,
            EndDate = ctlToDate.ChosenDate,
            TransactionStatus = transactionStatus,
            ChannelID = channelId,
            FromAmount = Convert.ToInt64(ctlAmountFrom.Amount),
            ToAmount = Convert.ToInt64(ctlAmountTo.Amount),
            //UserID = SafePageController.TransactionHeader.User.UserId,
            CIF = SafePageController.Customer.User.CifNo == VpConfigurationParameters.GetGenericParameter("NonCustomerCIF") ? null : SafePageController.Customer.User.CifNo,
            TransactionReferenceNumber = txtTransactionReferenceNumber.Text.Trim(),
            LoggedInAccountNumber = txtAccountNumber.Text.Trim(),
            PerformerIdentity = txtPerformerIdentity.Text.Trim(),
            LastTransactionsCount = maxRecordCount,
            PriviledgeDepth = GetPrivilegeDepth(txnName),
            OperationStage = operationStages,
            RC_IsOverDrawnRequested = cbOverDrawn.Checked,
            RC_IsNewValueDateRequested = cbValueDate.Checked,
            RC_IsNewExchangeRateRequested = cbExchangeRate.Checked,
            ApprovalDepartment = approvalDepartment

        };



        if (!string.IsNullOrEmpty(ddlTransactionName.SelectedValue))
            request.TransactionName = new List<string>() { ddlTransactionName.SelectedValue };

        var response = ServicesHelper.DoOnlineTransactionListInquiry(request);
        SafePageController.SetProfileValue("VpOnlineTransactionListInquiryRequest", request);
        grdTransactionList.DataSource = null;
        if (response != null && response.IsSuccess && response.UserTransactionList != null)
            grdTransactionList.DataSource = response.UserTransactionList.UserTransactions;
        else
        {
            if (ResponseHasErrorMessage(response))
            {
                AlertModal(GetErrorMessageFromResponse(response));
            }
        }
        grdTransactionList.DataBind();
    }

    private void LoadBranchTransactionDetails(string referenceNumber, bool RC_IsOverDrawnRequested, bool RC_IsNewValueDateRequested, bool RC_IsNewExchangeRateRequested, string RC_ApproverID, string RC_RefNumber, string approverComments)
    {
        int channelId = (int)(ChannelTypeEnum.Branch + 1);

        var maxRecordCount = 250;   //UI default value
        Int32.TryParse(VpConfigurationParameters.GetGenericParameter("InquiryTxnListMaxRecordCount"), out maxRecordCount);

        var txnName = TransactionNameContants.INQUIRY_TRANSACTION;

        var operationStages = new List<OperationStageEnumeration>();
        operationStages.Add(OperationStageEnumeration.RequestForApproval);
        operationStages.Add(OperationStageEnumeration.RejectApprovalRequest);
        operationStages.Add(OperationStageEnumeration.ExpireApprovalRequest);
        operationStages.Add(OperationStageEnumeration.Execute);
        operationStages.Add(OperationStageEnumeration.CloseCrmCase);

        var transactionStatus = new List<TransactionStatusEnum>();
        transactionStatus.Add(TransactionStatusEnum.All);

        var request = new VpOnlineTransactionListInquiryRequest
        {
            BeginDate = ctlFromDate.ChosenDate,
            EndDate = ctlToDate.ChosenDate,
            TransactionStatus = transactionStatus,
            ChannelID = channelId,
            FromAmount = 0,
            ToAmount = 0,
            //UserID = SafePageController.TransactionHeader.User.UserId,
            CIF = SafePageController.Customer.User.CifNo == VpConfigurationParameters.GetGenericParameter("NonCustomerCIF") ? null : SafePageController.Customer.User.CifNo,
            TransactionReferenceNumber = referenceNumber,
            LoggedInAccountNumber = txtAccountNumber.Text.Trim(),
            PerformerIdentity = txtPerformerIdentity.Text.Trim(),
            LastTransactionsCount = maxRecordCount,
            PriviledgeDepth = GetPrivilegeDepth(txnName),
            OperationStage = operationStages,
            ApprovalDepartment = ApprovalDepartmentEnum.All,
            RC_IsOverDrawnRequested = RC_IsOverDrawnRequested,
            RC_IsNewValueDateRequested = RC_IsNewValueDateRequested,
            RC_IsNewExchangeRateRequested = RC_IsNewExchangeRateRequested
            
            
        };


        var response = ServicesHelper.DoOnlineTransactionListInquiry(request);

        if (response != null && response.IsSuccess && response.UserTransactionList != null && response.UserTransactionList.UserTransactions != null && response.UserTransactionList.Count > 0)
        {
            var userTransaction = response.UserTransactionList.UserTransactions[0];

            var transactionName = userTransaction.TransactionName;
            txt_BS_TransactionName.Text = GetGlobalResource(string.Format("{0}.TxnDisplayName", transactionName));
            if (transactionName == TransactionNameContants.PERFORM_REVERSAL_OPERATION && !string.IsNullOrEmpty(userTransaction.OriginalTransactionName))
            {
                var originalTxnName = userTransaction.OriginalTransactionName;
                txt_BS_TransactionName.Text = string.Format("{0} - {1}", GetGlobalResource(string.Format("{0}.TxnDisplayName", transactionName)), GetGlobalResource(string.Format("{0}.TxnDisplayName", originalTxnName)));
            }
                        
            txt_BS_ReferenceNumber.Text = userTransaction.TransactionReferenceNumber;

            //For RequestForApproval stage, no need to show data as transactionDate should show only branch execution datetime
            if (userTransaction.OperationStage != OperationStageEnumeration.RequestForApproval)
            {
                txt_BS_TransactionDate.Text = userTransaction.TransactionDate.ToString(VpConstants.Dates.DateTimeFormat, ci);
            }
            else
            {
                txt_BS_TransactionDate.Text = "-";
            }


            txt_BS_TransactionAmount.Text = ServicesHelper.GetAmountInCurrencyFormat(userTransaction.TransactionAmount.Value, userTransaction.TransactionCurrency, true);
            txt_BS_ExchangeRate.Text = userTransaction.ExchangeRate.ToString();

             if (!string.IsNullOrEmpty(userTransaction.ValueDate))
             {
                 var date = DateTime.ParseExact(userTransaction.ValueDate, VpConstants.Dates.IALDateFormat, null);
                 txt_BS_ValueDate.Text = date.ToString(VpConstants.Dates.DateFormat, ci);
                                 
             }
             else
             {
                txt_BS_ValueDate.Text = "-";
              }

            //For RequestForApproval stage, no need to show data as transactionstatus should show only branch transactions status
             if (userTransaction.OperationStage != OperationStageEnumeration.RequestForApproval)
             {
                 txt_BS_TransactionStatus.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + userTransaction.TransactionStatus);
             }
             else
             {
                 txt_BS_TransactionStatus.Text = "-";
             }

            txt_BS_HostReferenceNumber.Text = userTransaction.HostReferenceNumber;

            txt_BS_AccountNumber.Text = userTransaction.AccountNumber;


            txt_BS_TellerID.Text = userTransaction.PerformerIdentity;

            txt_BS_ApproverIdentity.Text = userTransaction.ApproverIdentity;

            txt_BS_SecondaryApproverIdentity.Text = userTransaction.SecondaryApproverIdentity;

            txt_BS_ApprovalDepartmentUserID.Text = RC_ApproverID;

            txt_BS_RC_RefNumber.Text = RC_RefNumber;

            //Transaction Type
            cb_BS_OverDrawn.Checked = userTransaction.RC_IsOverDrawnRequested;
            cb_BS_ExchangeRate.Checked = userTransaction.RC_IsNewExchangeRateRequested;
            cb_BS_ValueDate.Checked = userTransaction.RC_IsNewValueDateRequested;

            //RejectReason
            if (userTransaction.LogFields != null)
            {
                txt_BS_RejectReason.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RejectReasonResourceKey + userTransaction.LogFields.Reserved9);
                txt_BS_CallReferenceNumber.Text = userTransaction.LogFields.Reserved7;
            }

            //Reason
            txt_BS_Reason.Text = approverComments;

            //Show branch status
            tblBranchStatus.Visible = true;
            BranchStatusStep.Visible = true;

            if (userTransaction.TransactionStatus == TransactionStatusEnum.ApprovalPending && userTransaction.ApprovalDepartment == ApprovalDepartmentEnum.ReferCard)
            {
                btnCancelRequest.Visible = true;
            }
            else
            {
                btnCancelRequest.Visible = false;
            }

            //txt_Branch_Remarks
            txt_Branch_Remarks.Text = userTransaction.LogFields == null ? string.Empty : userTransaction.LogFields.Reserved10;

        }
        else
        {
            if (ResponseHasErrorMessage(response))
            {
                AlertModal(GetErrorMessageFromResponse(response));
            }
            else
            {

                //Hide branch status
                tblBranchStatus.Visible = false;
                BranchStatusStep.Visible = false;

                AlertModal(GetLocalResource("TransactionPendingBranch.ErrorMessage"));
            }
        }
        
    }

    private VPGenerateRequestResponseForRefNumberResponse VpGenerateRequestResponseForRefNumberResponse(string referenceNumber,  ApprovalDepartmentEnum approvalDept)
    {
        VPGenerateRequestResponseForRefNumberResponse response = null;
        if (approvalDept == ApprovalDepartmentEnum.IPS)
        {
            
            response = TransactionUtilitiesHelper.DoGenerateObjectsForReferenceNumber(new VPGenerateRequestResponseForRefNumberRequest
            {
                InquiryType = InquiryTypeEnum.RequestAndResponse,
                TransactionReferenceNumber = referenceNumber,
                TransactionStage = OperationStageEnumeration.Execute
            });
        }
        else
        {
            response = TransactionUtilitiesHelper.DoGenerateObjectsForReferenceNumber(new VPGenerateRequestResponseForRefNumberRequest
            {
                InquiryType = InquiryTypeEnum.RequestAndResponse,
                TransactionReferenceNumber = referenceNumber,
                TransactionStage = OperationStageEnumeration.RequestForApproval
            });
        }

        return response;
    }

    private ApprovalFlagsDictionary GetApprovalFlagsReversedDictionary(IEnumerable<VPApprovalActionInformation> approvalActionInformationList)
    {
        var dict = new ApprovalFlagsDictionary();

        foreach (var actionInformation in approvalActionInformationList)
        {
            var approvalFlag = new ApprovalFlag(actionInformation);
            if (string.IsNullOrEmpty(approvalFlag.Code))
                continue;

            if (!dict.ContainsKey(approvalFlag.Category))
                dict.Add(approvalFlag.Category, new ApprovalFlagList());

            dict[approvalFlag.Category].Add(approvalFlag);
        }

        return dict;
    }

    private void InititializePageControllerForCustomer(VPBLUser userInfo, VpGetCustomerInfoResponse customerProfile)
    {

        SafePageController.Customer.User.UserName = userInfo.UserName;
        SafePageController.Customer.User.TPinVerified = userInfo.TPinVerified;

        SafePageController.Customer.IsAuthenticated = true;
        SafePageController.Customer.UserInfo = new UserInfo();

        //save performer
        var performer = SafePageController.Customer.User.PerformerName;
        var tpinVerified = SafePageController.Customer.User.TPinVerified;

        //set the user from login
        SafePageController.Customer.User = userInfo;
        SafePageController.Customer.User.PerformerName = performer;
        SafePageController.Customer.User.TPinVerified = tpinVerified;

        SafePageController.Customer.UserInfo.CustomerFirstName = userInfo.FirstName;
        SafePageController.Customer.UserInfo.CustomerLastName = userInfo.LastName;
        SafePageController.Customer.UserInfo.CustomerName = userInfo.CustomerName;
        SafePageController.Customer.UserInfo.CustomerNumber = userInfo.CifNo;

        SafePageController.Customer.CustomerInfo = customerProfile;
    }

    public CheckAndUpdateReferCardInquiryResponse InquireStatus(List<string> referenceNumbers)
    {
        var request = new CheckAndUpdateReferCardInquiryRequest();
        request.ReferenceNumbers = referenceNumbers;
        
        return ServicesHelper.DoCheckAndUpdateReferCardOperation(request);
    }

    public ExtendedResponseTransactionData<VPPaymentStatusInquiryResponse> InquireIPSStatus(string OrgInsIde, string ReferenceNumber)
    {
        var request = new VPPaymentStatusInquiryRequest();
        request.IALPaymentStatusInquiryRequest = new PaymentStatusInquiryRequest();
        request.IALPaymentStatusInquiryRequest.OrgInsIde = OrgInsIde;
        request.ReferenceNumber = ReferenceNumber;

        return FundsHelper.PaymentStatusInquiryOperation(request);
    }

    public CheckAndUpdateReferCardInquiryResponse InquireAll(int NoOfDays)
    {
        var request = new CheckAndUpdateReferCardInquiryRequest
        {
            NoOfDays = NoOfDays 
        };
        return ServicesHelper.DoCheckAndUpdateReferCardOperation(request);
    }

    protected void rbSelector_CheckedChanged(object sender, System.EventArgs e)
    {   
        //Set the new selected row
        RadioButton rb = (RadioButton)sender;
        GridViewRow row = (GridViewRow)rb.NamingContainer;

        ////For Cancel Request, save the litRC_ReferenceNumber in hidRC_ReferenceNumber
        //var hidRC_ReferenceNumber = row.FindControl("hidRC_ReferenceNumber") as HiddenField;
        //var litRC_ReferenceNumber = row.FindControl("litRC_ReferenceNumber") as Literal;
        //hidRC_ReferenceNumber.Value = litRC_ReferenceNumber.Text;


        var litReferenceNumber = row.FindControl("litReferenceNumber") as Literal;
        var litRC_ReferenceNumber = row.FindControl("litRC_ReferenceNumber") as Literal;
        
        var hidRC_IsOverDrawnRequested = row.FindControl("hidRC_IsOverDrawnRequested") as HiddenField;
        var hidRC_IsNewValueDateRequested = row.FindControl("hidRC_IsNewValueDateRequested") as HiddenField;
        var hidRC_IsNewExchangeRateRequested = row.FindControl("hidRC_IsNewExchangeRateRequested") as HiddenField;
        var hidRC_ApproverID = row.FindControl("hidRC_ApproverID") as HiddenField;
        var hid_ApproverComments = row.FindControl("hid_ApproverComments") as HiddenField;
        
        var RC_IsOverDrawnRequested = Convert.ToBoolean(hidRC_IsOverDrawnRequested.Value);
        var RC_IsNewValueDateRequested = Convert.ToBoolean(hidRC_IsNewValueDateRequested.Value);
        var RC_IsNewExchangeRateRequested = Convert.ToBoolean(hidRC_IsNewExchangeRateRequested.Value);

        if (litReferenceNumber != null)
        {
            LoadBranchTransactionDetails(litReferenceNumber.Text, RC_IsOverDrawnRequested, RC_IsNewValueDateRequested, RC_IsNewExchangeRateRequested, hidRC_ApproverID.Value, litRC_ReferenceNumber.Text, hid_ApproverComments.Value);

        }
        else
        {
            AlertModal("Transaction reference number is not found.");

            //Hide branch status
            tblBranchStatus.Visible = false;
            BranchStatusStep.Visible = false;
        }

    }

    protected void grdTransactionList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var userTransaction = ((UserTransaction)e.Row.DataItem);
                var litChannel = e.Row.FindControl("litChannel") as Literal;
                if (litChannel != null)
                {
                    var channel = Enum.Parse(typeof(ChannelTypeEnum), (userTransaction.Channel - 1).ToString());
                    litChannel.Text = GetGlobalResource(string.Format("Channel.{0}.DisplayName", channel));
                }

                var litBranch = e.Row.FindControl("litBranch") as Literal;
                if (litBranch != null)
                {
                    litBranch.Text = ServicesHelper.GetBranchNameByCode(userTransaction.PerformerBranchCode, UserLanguage);
                }

                var litApprovalDepartment = e.Row.FindControl("litApprovalDepartment") as Literal;
                if (litApprovalDepartment != null)
                {
                    var approvaldepartment = userTransaction.ApprovalDepartment.ToString();
                    litApprovalDepartment.Text = GetGlobalResource(string.Format("ApprovalDepartment.{0}.DisplayName", approvaldepartment)); 
                }

                var hid_ApprovalDepartment = e.Row.FindControl("hid_ApprovalDepartment") as HiddenField;
                if (hid_ApprovalDepartment != null)
                {
                    hid_ApprovalDepartment.Value = userTransaction.ApprovalDepartment.ToString();
                }

                var litTransactionDate = e.Row.FindControl("litTransactionDate") as Literal;
                if (litTransactionDate != null)
                {
                    litTransactionDate.Text = userTransaction.TransactionDate.ToString(VpConstants.Dates.DateTimeFormat, ci);
                }

                var litReceiveDate = e.Row.FindControl("litReceiveDate") as Literal;
                if (litReceiveDate != null)
                {
                    if (userTransaction.RC_ReceivedDateTime != DateTime.MinValue)
                    {
                        litReceiveDate.Text = userTransaction.RC_ReceivedDateTime.ToString(VpConstants.Dates.DateTimeFormat, ci);
                    }
                    else
                    {
                        litReceiveDate.Text = "-";
                    }

                }

                var litValueDate = e.Row.FindControl("litValueDate") as Literal;
                if (litValueDate != null && !string.IsNullOrEmpty(userTransaction.ValueDate))
                {
                    var date = DateTime.ParseExact(userTransaction.ValueDate, VpConstants.Dates.IALDateFormat, null);
                    litValueDate.Text = date.ToString(VpConstants.Dates.DateFormat, ci);
                }

                var litSpecialValueDate = e.Row.FindControl("litSpecialValueDate") as Literal;
                if (litSpecialValueDate != null)
                {
                    if (userTransaction.SpecialValueDate != DateTime.MinValue)
                    {
                        litSpecialValueDate.Text = userTransaction.SpecialValueDate.ToString(VpConstants.Dates.DateFormat, ci);
                    }
                    else
                    {
                        litSpecialValueDate.Text = "-";
                    }
                    
                }
                //litTransactionAmount
                var litTransactionAmount = e.Row.FindControl("litTransactionAmount") as Literal;
                if (litTransactionAmount != null)
                {

                    litTransactionAmount.Text = ServicesHelper.GetAmountInCurrencyFormat(userTransaction.TransactionAmount.Value , userTransaction.TransactionCurrency, true);
                }

                var litSpecialExchangeRate = e.Row.FindControl("litSpecialExchangeRate") as Literal;
                if (litSpecialExchangeRate != null)
                {
                    if (userTransaction.SpecialExchangeRate != 0)
                    {
                        litSpecialExchangeRate.Text = userTransaction.SpecialExchangeRate.ToString().Trim('0');
                    }
                    else
                    {
                        litSpecialExchangeRate.Text = "-";
                    }

                }

                var litExpiryDateTime = e.Row.FindControl("litExpiryDateTime") as Literal;
                if (litExpiryDateTime != null)
                {
                    if (userTransaction.RC_ExpiryDateTime != DateTime.MinValue)
                    {
                        litExpiryDateTime.Text = userTransaction.RC_ExpiryDateTime.ToString(VpConstants.Dates.DateTimeFormat, ci);
                    }
                    else
                    {
                        litExpiryDateTime.Text = "-";
                    }

                }


                var litTransactionDescription = e.Row.FindControl("litTransactionDescription") as Literal;
                if (litTransactionDescription != null)
                {
                    var txnName = userTransaction.TransactionName;
                    litTransactionDescription.Text = GetGlobalResource(string.Format("{0}.TxnDisplayName", txnName));
                    if (txnName == TransactionNameContants.PERFORM_REVERSAL_OPERATION && !string.IsNullOrEmpty(userTransaction.OriginalTransactionName))
                    {
                        var originalTxnName = userTransaction.OriginalTransactionName;
                        litTransactionDescription.Text = string.Format("{0} - {1}", GetGlobalResource(string.Format("{0}.TxnDisplayName", txnName)), GetGlobalResource(string.Format("{0}.TxnDisplayName", originalTxnName)));
                    }
                }

                var litReferenceNumber = e.Row.FindControl("litReferenceNumber") as Literal;
                if (litReferenceNumber != null)
                {
                    litReferenceNumber.Text = userTransaction.TransactionReferenceNumber;
                }

                var litAccountNumber = e.Row.FindControl("litAccountNumber") as Literal;
                if (litAccountNumber != null)
                {
                    litAccountNumber.Text = userTransaction.AccountNumber;
                }
                

                var litTransactionStatus = e.Row.FindControl("litTransactionStatus") as Literal;
                if (litTransactionStatus != null)
                {
                    var status = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + userTransaction.TransactionStatus);
                    
                    switch (userTransaction.TransactionStatus)
                    {
                        case TransactionStatusEnum.ApprovalPending: status = "<span style='color:orange'>" + status + "</span>";
                        break;

                        case TransactionStatusEnum.Approved: status = "<span style='color:green'>" + status + "</span>";
                        break;

                        case TransactionStatusEnum.Rejected: status = "<span style='color:red'>" + status + "</span>";
                        break;

                       //For Executed transactions shpow "Approved" transaction status
                        case TransactionStatusEnum.Executed: status = "<span style='color:green'>" + GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + TransactionStatusEnum.Approved) +"</span>";
                        break;

                    }
                    litTransactionStatus.Text = status;
                }


                //litRC_ReferenceNumber

                var litRC_ReferenceNumber = e.Row.FindControl("litRC_ReferenceNumber") as Literal;
                if (litRC_ReferenceNumber != null)
                {
                    litRC_ReferenceNumber.Text = userTransaction.RC_ReferenceNumber;
                }


                //litBranchTxnStatus
                var litBranchTxnStatus = e.Row.FindControl("litBranchTxnStatus") as Literal;


                if (litBranchTxnStatus != null)
                {
                    var status = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.TransactionStatusResourceKey + userTransaction.BranchTransactionStatus);

                    if (userTransaction.BranchTransactionStatus == TransactionStatusEnum.Unknown || (userTransaction.ApprovalDepartment == ApprovalDepartmentEnum.IPS && userTransaction.TransactionStatus == TransactionStatusEnum.ApprovalPending))
                    {
                        litBranchTxnStatus.Text = "-";
                    }
                    else
                    {
                        litBranchTxnStatus.Text = status;
                    }
                }


                //var btnResolveDoubtful = e.Row.FindControl("btnResolvePending") as VBImageButton;
                //if (btnResolveDoubtful != null)
                //    btnResolveDoubtful.Visible = userTransaction.TransactionStatus == TransactionStatusEnum.ApprovalPending;

                var btnResume = e.Row.FindControl("btnDetails") as VBImageButton;
                if (btnResume != null)
                {
                    btnResume.Visible = userTransaction.TransactionStatus == TransactionStatusEnum.Approved;
                }
                //In case if the transaction is IPS then allow user to see the details of transaction only in pending case.
                if (btnResume != null && userTransaction.ApprovalDepartment == ApprovalDepartmentEnum.IPS && userTransaction.TransactionStatus == TransactionStatusEnum.Completed)
                {
                    btnResume.Visible = true ;
                }
                //hidden fields
                var hidRC_IsOverDrawnRequested = e.Row.FindControl("hidRC_IsOverDrawnRequested") as HiddenField;
                var hidRC_IsNewValueDateRequested = e.Row.FindControl("hidRC_IsNewValueDateRequested") as HiddenField;
                var hidRC_IsNewExchangeRateRequested = e.Row.FindControl("hidRC_IsNewExchangeRateRequested") as HiddenField;
                var hidRC_ApproverID = e.Row.FindControl("hidRC_ApproverID") as HiddenField;
                var hid_ApproverComments = e.Row.FindControl("hid_ApproverComments") as HiddenField;

                var hid_Reserved1 = e.Row.FindControl("hid_Reserved1") as HiddenField;
                var hid_Reserved2 = e.Row.FindControl("hid_Reserved2") as HiddenField;
                var hid_Reserved3 = e.Row.FindControl("hid_Reserved3") as HiddenField;
                var hid_Reserved4 = e.Row.FindControl("hid_Reserved4") as HiddenField;
                var hid_Reserved5 = e.Row.FindControl("hid_Reserved5") as HiddenField;
                var hid_Reserved6 = e.Row.FindControl("hid_Reserved6") as HiddenField;
                var hid_Reserved7 = e.Row.FindControl("hid_Reserved7") as HiddenField;
                var hid_Reserved8 = e.Row.FindControl("hid_Reserved8") as HiddenField;
                var hid_Reserved9 = e.Row.FindControl("hid_Reserved9") as HiddenField;
                var hid_Reserved10= e.Row.FindControl("hid_Reserved10") as HiddenField;

                hidRC_IsOverDrawnRequested.Value = userTransaction.RC_IsOverDrawnRequested.ToString();
                hidRC_IsNewValueDateRequested.Value = userTransaction.RC_IsNewValueDateRequested.ToString();
                hidRC_IsNewExchangeRateRequested.Value = userTransaction.RC_IsNewExchangeRateRequested.ToString();
                hidRC_ApproverID.Value = userTransaction.RC_ApproverID.ToString();

                if (userTransaction.LogFields != null)
                {
                    hid_ApproverComments.Value = userTransaction.LogFields.Reserved3;

                    hid_Reserved1.Value = userTransaction.LogFields.Reserved1;
                    hid_Reserved2.Value = userTransaction.LogFields.Reserved2;
                    hid_Reserved3.Value = userTransaction.LogFields.Reserved3;
                    hid_Reserved4.Value = userTransaction.LogFields.Reserved4;
                    hid_Reserved5.Value = userTransaction.LogFields.Reserved5;
                    hid_Reserved6.Value = userTransaction.LogFields.Reserved6;
                    hid_Reserved7.Value = userTransaction.LogFields.Reserved7;
                    hid_Reserved8.Value = userTransaction.LogFields.Reserved8;
                    hid_Reserved9.Value = userTransaction.LogFields.Reserved9;
                    hid_Reserved10.Value = userTransaction.LogFields.Reserved10;
                }
                
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    protected void grdTransactionList_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {

            //details
            if (e.CommandName.ToLower() == "details")
            {

                SafePageController.SetProfileValue("ddlTransactionStatus_Enabled", ddlTransactionStatus.Enabled);
                SafePageController.SetProfileValue("ddlTransactionName_Enabled", ddlTransactionName.Enabled);
                var referenceNumber = e.CommandArgument.ToString();

                Literal litSpecialExchangeRate = null;
                Literal litSpecialValueDate = null;
                Literal litAccountNumber = null;
                Literal litExpiryDateTime = null;

                HiddenField hid_ApprovalDepartment = null;
                HiddenField hid_Reserved1 = null;
                HiddenField hid_Reserved2 = null;
                HiddenField hid_Reserved3 = null;
                HiddenField hid_Reserved4 = null;
                HiddenField hid_Reserved5 = null;
                HiddenField hid_Reserved6 = null;
                HiddenField hid_Reserved7 = null;
                HiddenField hid_Reserved8 = null;
                HiddenField hid_Reserved9 = null;
                HiddenField hid_Reserved10 = null;

                foreach (GridViewRow row in grdTransactionList.Rows)
                {
                    var litReferenceNumber = row.FindControl("litReferenceNumber") as Literal;

                    if (referenceNumber == litReferenceNumber.Text)
                    {
                        
                        litSpecialExchangeRate = row.FindControl("litSpecialExchangeRate") as Literal;
                        litSpecialValueDate = row.FindControl("litSpecialValueDate") as Literal;
                        litAccountNumber = row.FindControl("litAccountNumber") as Literal;
                        litExpiryDateTime = row.FindControl("litExpiryDateTime") as Literal;

                        hid_ApprovalDepartment = row.FindControl("hid_ApprovalDepartment") as HiddenField;
                        hid_Reserved1 = row.FindControl("hid_Reserved1") as HiddenField;
                        hid_Reserved2 = row.FindControl("hid_Reserved2") as HiddenField;
                        hid_Reserved3 = row.FindControl("hid_Reserved3") as HiddenField;
                        hid_Reserved4 = row.FindControl("hid_Reserved4") as HiddenField;
                        hid_Reserved5 = row.FindControl("hid_Reserved5") as HiddenField;
                        hid_Reserved6 = row.FindControl("hid_Reserved6") as HiddenField;
                        hid_Reserved7 = row.FindControl("hid_Reserved7") as HiddenField;
                        hid_Reserved8 = row.FindControl("hid_Reserved8") as HiddenField;
                        hid_Reserved9 = row.FindControl("hid_Reserved9") as HiddenField;
                        hid_Reserved10 = row.FindControl("hid_Reserved10") as HiddenField;
                        
                        break;
                    }
                    
                }


                //Check If request is Expired. 
                 var expiryDateTime = DateTime.MinValue;

                 if (litExpiryDateTime != null && !string.IsNullOrEmpty(litExpiryDateTime.Text) && litExpiryDateTime.Text != "-")
                 {
                    expiryDateTime = Convert.ToDateTime(litExpiryDateTime.Text);
                 }


                 if (expiryDateTime != DateTime.MinValue && expiryDateTime < DateTime.Now)
                 {
                     var request = new VPReferCardCancelRequest
                     {
                         Original_ReferenceNumber = referenceNumber,
                         Action = ReferCardActionEnum.Expired
                     };

                     var response = ServicesHelper.DoReferCardCancelOperation(request);

                     var msg = GetGlobalResource("ReferCardRequestExpiredMessage");
                     AlertModal(msg);

                     FetchTransactions();

                     return;
                 }


                 if (hid_ApprovalDepartment == null || string.IsNullOrEmpty(hid_ApprovalDepartment.Value))
                    throw new Exception("Invalid approval department.");

                //if (litAccountNumber == null || string.IsNullOrEmpty(litAccountNumber.Text))
                //    throw new Exception("Invalid Account Number.");

                 var approvalDeparment = (ApprovalDepartmentEnum)Enum.Parse(typeof(ApprovalDepartmentEnum), hid_ApprovalDepartment.Value, true);

                //For Refer card always check status before processing any request
                if (approvalDeparment == ApprovalDepartmentEnum.ReferCard)
                {
                    var referenceNumbers = new List<string>();
                    referenceNumbers.Add(referenceNumber);
                    var inquireStatusResponse = InquireStatus(referenceNumbers);

                    if (inquireStatusResponse != null && inquireStatusResponse.IsSuccess && inquireStatusResponse.ReferCardOperations != null)
                    {
                        var operation = inquireStatusResponse.ReferCardOperations.FirstOrDefault();
                        if (operation != null &&
                                (
                                operation.Status == TransactionStatusEnum.ApprovalCancelled || 
                                operation.Status == TransactionStatusEnum.Rejected || 
                                operation.Status == TransactionStatusEnum.Expired  
                                )
                            )
                        {
                            var msg = GetGlobalResource("ReferCardRequestCancelMessage");
                            AlertModal(msg);
                            FetchTransactions();

                            return;

                        }
                    }
                }


                if (!string.IsNullOrEmpty(referenceNumber))
                {
                    SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber, referenceNumber);

                    var response = VpGenerateRequestResponseForRefNumberResponse(referenceNumber, approvalDeparment);

                    if (response.IsSuccess)
                    {
                        
                        SafePageController.RequestData = response.RequestItem;
                        
                        //Set SafePageController

                        var userHelper = new UserHelper();
                        userHelper.LoginToken = SafePageController.RequestData.Header.LoginTokenKey;
                        var customerProfile = userHelper.GetCustomerProfile(SafePageController.RequestData.Header, SafePageController.RequestData.Header.User.CifNo);
                        if (!customerProfile.IsSuccess || customerProfile.IALCustomerInfoResponse == null)
                            throw new VPSystemException("LocalOperations_WaitingForApprovalTransactionsList", VPExceptionConstants.CUSTOMER_PROFILE_NOT_FOUND);
                        
                        //Initialize Page controller's User (Customer)
                        InititializePageControllerForCustomer(SafePageController.RequestData.Header.User, customerProfile.IALCustomerInfoResponse);

                        //set Approved flag for department
                        if (approvalDeparment == ApprovalDepartmentEnum.ReferCard)
                        {
                            SafePageController.SetStateValue("ReferCardApproved", "true");
                        }
                        else if (approvalDeparment == ApprovalDepartmentEnum.CallCenter)
                        {
                            SafePageController.SetStateValue("CallCenterApproved", "true");

                        }
                        else if (approvalDeparment == ApprovalDepartmentEnum.Compliance)
                        {
                            SafePageController.SetStateValue("ComplianceApproved", "true");

                        }

                        var list = new CustomLogEntryList();
                        list = SafePageController.RequestData.CustomLogEntryList;

                        //Check if request customer log entry contains ReferCardApproved = true then set flag in SafePageController
                        var isReferCardApproved = list.GetLogValue("ReferCardApproved");
                        if (isReferCardApproved != null && isReferCardApproved.ToString().ToLower() == "true")
                            SafePageController.SetStateValue("ReferCardApproved", "true");

                        //Check if request customer log entry contains ComplianceApproved = true then set flag in SafePageController
                        var isComplianceApproved = list.GetLogValue("ComplianceApproved");
                        if (isComplianceApproved != null && isComplianceApproved.ToString().ToLower() == "true")
                            SafePageController.SetStateValue("ComplianceApproved", "true");

                        //Check if request customer log entry contains CVUApproved = true then set flag in SafePageController
                        var isCallCenterApproved = list.GetLogValue("CallCenterApproved");
                        if (isCallCenterApproved != null && isCallCenterApproved.ToString().ToLower() == "true")
                            SafePageController.SetStateValue("CallCenterApproved", "true");


                        //this account number will be used to re-login from start page
                        SafePageController.SetStateValue("Approved_AccountNumber", litAccountNumber.Text);

                        //this will  update account list table cache
                        AccountHelper.GetAccountListDataTable(true, VpPageControllerConstants.Account.CASA.ToString());

                        //SafePageController.RequestData.TransactionActions = GetApprovalActions(response.RequestItem);

                        //loop through the controls to get the approval actions
                        //foreach (var approvalControl in ApprovalControls)
                        //    approvalControl.SetSelectedApprovalFlag();
                        if (approvalDeparment != ApprovalDepartmentEnum.IPS)
                        {
                            if (response != null && response.ResponseItem != null && response.ResponseItem.Footer != null && response.ResponseItem.Footer.TransactionInformation != null)
                            {
                                //set original transaction parameters
                                TransactionInformation = response.ResponseItem.Footer.TransactionInformation;
                                SafePageController.SetStateValue("TransactionName", response.ResponseItem.Footer.TransactionInformation.Name);

                                if (response.ResponseItem.Footer.TransactionInformation.TransactionSettings != null)
                                    SafePageController.SignatureVerificationFlagActual = response.ResponseItem.Footer.TransactionInformation.TransactionSettings.IsSignatureVerificationRequired;

                                //set original transaction approval actions
                                var approvalFlags = response.ResponseItem.Footer.TransactionInformation.ApprovalActionInformationList;

                                if (approvalFlags != null)
                                {
                                    var dictionary = GetApprovalFlagsReversedDictionary(approvalFlags);
                                    if (dictionary != null)
                                        ApprovalFlagsDictionary = dictionary;
                                }


                                //Set special exchange rate and value date
                                if (SafePageController.RequestData is IIALRequest)
                                {
                                    var ialRequest = (SafePageController.RequestData as IIALRequest).GetCommonIALRequest();

                                    if (ialRequest is IFinancialData)
                                    {
                                        var iFinancialDataRequest = ialRequest as IFinancialData;
                                        if (iFinancialDataRequest.FinancialData != null)
                                        {
                                            if (litSpecialExchangeRate != null && !string.IsNullOrEmpty(litSpecialExchangeRate.Text) && litSpecialExchangeRate.Text != "-")
                                            {
                                                iFinancialDataRequest.FinancialData.ExchangeRates = Convert.ToDecimal(litSpecialExchangeRate.Text);
                                            }

                                            if (litSpecialValueDate != null && !string.IsNullOrEmpty(litSpecialValueDate.Text) && litSpecialValueDate.Text != "-")
                                            {
                                                iFinancialDataRequest.FinancialData.ValueDate = iFinancialDataRequest.FinancialData.ValueDateBuy = Convert.ToDateTime(litSpecialValueDate.Text).ToIALFormat();
                                            }

                                        }
                                    }
                                }

                                //set reserved fields in request data
                                if (SafePageController.RequestData != null)
                                {
                                    var requestData = SafePageController.RequestData;

                                    if (requestData.LogFields == null)
                                        requestData.LogFields = new LogField();

                                    requestData.LogFields.Reserved1 = hid_Reserved1.Value;
                                    requestData.LogFields.Reserved2 = hid_Reserved2.Value;
                                    requestData.LogFields.Reserved3 = hid_Reserved3.Value;
                                    requestData.LogFields.Reserved4 = hid_Reserved4.Value;
                                    requestData.LogFields.Reserved5 = hid_Reserved5.Value;
                                    requestData.LogFields.Reserved6 = hid_Reserved6.Value;
                                    requestData.LogFields.Reserved7 = hid_Reserved7.Value;
                                    requestData.LogFields.Reserved8 = hid_Reserved8.Value;
                                    requestData.LogFields.Reserved9 = hid_Reserved9.Value;
                                    requestData.LogFields.Reserved10 = hid_Reserved10.Value;

                                }

                                //Resolve start page url

                                var detailsInquiryresponse = ServicesHelper.DoOnlineTransactionDetailsInquiry(referenceNumber);

                                var startUrl = GetTransactionUrl(detailsInquiryresponse.UserTransactionDetails.OperationCode);

                                SafePageController.SetStateValue(VpPageControllerConstants.Transaction.OperationCode, detailsInquiryresponse.UserTransactionDetails.OperationCode);
                                SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionDescription, detailsInquiryresponse.UserTransactionDetails.TransactionDescription);

                                SafePageController.PageControllerRedirect(startUrl);



                                //SafePageController.PageControllerRedirect("~/RemoteOperations/Cash/CashDeposit/CashDepositStart.aspx");
                            }

                           
                            else
                            {
                                var msg = "Failed to fetch orginal transaction information. Reference Number : " + referenceNumber;
                                SetWarningDisplay(msg, msg);
                            }
                        }
                            //In case the transaction is IPS then open execute page
                        else
                        {
                            SafePageController.PageControllerRedirect("TransactionDetails.aspx");
                        }
                    }

                    else
                    {
                        var msg = "Failed to fetch orginal transaction information. Reference Number : " + referenceNumber;
                        SetWarningDisplay(msg, msg);
                    }



                }
            }
            //
            else if (e.CommandName.ToLower() == "inquirestatus")
            {
                var referenceNumber = e.CommandArgument.ToString();

                if (!string.IsNullOrEmpty(referenceNumber))
                {
                    var referenceNumbers = new List<string>();
                    referenceNumbers.Add(referenceNumber);

                    var response = InquireStatus(referenceNumbers);

                    if (response.IsSuccess && response.ReferCardOperations != null)
                    {
                        var txnResponse = response.ReferCardOperations.FirstOrDefault(r => r.ReferenceNumber == referenceNumber);
                        if (txnResponse != null)
                        {
                            //FetchTransactions();
                            if (txnResponse.Status == TransactionStatusEnum.Approved)
                                AlertModal(GetGlobalResource(VPResourceConstants.TRANSACTION_APPROVED));
                            else if (txnResponse.Status == TransactionStatusEnum.ApprovalPending)
                                AlertModal(GetGlobalResource(VPResourceConstants.APPROVAL_PENDING));
                            else if (txnResponse.Status == TransactionStatusEnum.Rejected)
                                AlertModal(GetGlobalResource(VPResourceConstants.TRANSACTION_REJECTED));
                            else if (txnResponse.Status == TransactionStatusEnum.Disapproved)
                                AlertModal(GetGlobalResource(VPResourceConstants.TRANSACTION_REJECTED));
                            else if (txnResponse.Status == TransactionStatusEnum.Cancelled)
                                AlertModal(GetGlobalResource(VPResourceConstants.TRANSACTION_REJECTED));
                            else if(true)
                                AlertModal("Invalide transaction status received.");

                        }
                    }
                    else
                    {
                        //AlertModalError("");//TODO
                    }
                }
            }

        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
            SetErrorDisplayDefault();
        }
    }

  
}
 
