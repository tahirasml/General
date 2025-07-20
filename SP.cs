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
using System.Data;

public partial class CVUTransactionsList : VeriBranchTransactionCommonBasePage
{
    CultureInfo ci = new CultureInfo(HelperBase.EnglishCulture);

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
        var txnName = SafePageController.QueryStrings["TxnName"];

        if (txnName == TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS && !ServicesHelper.IsBranchEnabledForCVUWorkflow)
        {
            SafePageController.PageControllerRedirect("~/InternalOperations/TransactionPad.aspx");
        }

        var branch = "";
        DataTable branches = new DataTable();
        branches.Columns.Add("BranchCode");
        branches.Columns.Add("BranchName");

        try
        {
            branch = ServicesHelper.GetBranchNameByCode(SafePageController.Performer.User.BranchCode);
            var nr = branches.NewRow();
            nr["BranchCode"] = SafePageController.Performer.User.BranchCode;
            nr["BranchName"] = branch;
            branches.Rows.Add(nr);
            if (txnName == TransactionNameContants.CVU_AUTHORIZE_BRANCH_TELLER_BATCHES)
            {
                branches = ServicesHelper.DoUserBranchHierarchyInquiry(GetPrivilegeDepth(txnName));
            }
        }
        catch (Exception)
        {

        }
        ctlBranchLookup.DataTable = branches;

        //if (branches.Rows.Count == 1 && txnName == TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS)
        if (branches.Rows.Count == 1 && (txnName == TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS || txnName == TransactionNameContants.CVU_BRANCH_TELLER_BATCHES))
        {
            //ctlBranchLookup.PopulateValue(branches.Rows[0][VpPageControllerConstants.Branch.BranchCode.ToString()].ToString());
            ctlBranchLookup.PopulateValue(SafePageController.Performer.User.BranchCode);
            ctlBranchLookup.Enabled = false;
        }


        ResetBranchUsers();
        LoadCVUStatus();
        LoadTransactionNames();

        ctlFromDate.ChosenDate = DateTime.Today.AddDays(-7);
        ctlToDate.ChosenDate = DateTime.Today;

        grdTransactionList.SaveStateEvent += GridView_SaveStateEvent;
        grdTransactionList.LoadStateEvent += GridView_LoadStateEvent;

        HandlePostBack(btnDisplay);



        try
        {
            if (Request.UrlReferrer.AbsolutePath.EndsWith("CVUTransactionDetails.aspx"))
            {
                var request = SafePageController.GetProfileValue("VpOnlineTransactionListInquiryRequest") as VpOnlineTransactionListInquiryRequest;

                if (request != null)
                {
                    if (request.TransactionName != null)
                    {
                        var selectedTxnName = request.TransactionName.FirstOrDefault();
                        if (!string.IsNullOrEmpty(selectedTxnName) && ddlTransactionName.Items.FindByValue(selectedTxnName) != null)
                            ddlTransactionName.SelectedValue = selectedTxnName;
                    }
                    ctlFromDate.ChosenDate = request.BeginDate;
                    ctlToDate.ChosenDate = request.EndDate;
                    if (request.CVUStatus != null && request.CVUStatus.Count > 0)
                        ddlCVUStatus.SelectedValue = request.CVUStatus.FirstOrDefault().ToString();

                    if (ctlBranchLookup.HasValue(request.PerformerBranchCode))
                        ctlBranchLookup.PopulateValue(request.PerformerBranchCode);

                    if (ctlBranchUsers.HasValue(request.PerformerIdentity))
                        ctlBranchUsers.PopulateValue(request.PerformerIdentity);

                    FetchTransactions();

                    ddlCVUStatus.Enabled = Convert.ToBoolean(SafePageController.GetProfileValue("ddlCVUStatus_Enabled"));
                    ddlTransactionName.Enabled = Convert.ToBoolean(SafePageController.GetProfileValue("ddlTransactionName_Enabled"));
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    protected override void LocalizePageContent()
    {
        var heading = GetGlobalResource("InternalOperations/TransactionPad.aspx", SafePageController.QueryStrings["TxnName"] + ".DisplayName");
        if (!string.IsNullOrEmpty(heading))
            SetHeaderLabel(heading);
        else
        {
            base.LocalizePageContent();

        }
    }

    private void LoadCVUStatus()
    {
        ddlCVUStatus.Items.Clear();

        if (HasPrivilege(TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS) && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.BRANCH_CVU_REVERTED_TRANSACTIONS)
        {
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.RevertedToBranch), CVUStatusEnum.RevertedToBranch.ToString()));
        }
        else if (HasPrivilege(TransactionNameContants.CVU_BRANCH_TELLER_BATCHES) && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.CVU_BRANCH_TELLER_BATCHES)
        {
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.All), CVUStatusEnum.All.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.ApprovedByChecker), CVUStatusEnum.ApprovedByChecker.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.RevertedToBranch), CVUStatusEnum.RevertedToBranch.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.Submitted), CVUStatusEnum.Submitted.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.Resubmitted), CVUStatusEnum.Resubmitted.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.Completed), CVUStatusEnum.Completed.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.ApprovedByBranchCVU), CVUStatusEnum.ApprovedByBranchCVU.ToString()));


            ddlCVUStatus.SelectedValue = CVUStatusEnum.Submitted.ToString();

        }
        else if (HasPrivilege(TransactionNameContants.CHECKER_CVU_REVERTED_TRANSACTIONS) && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.CHECKER_CVU_REVERTED_TRANSACTIONS)
        {
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.RevertedToChecker), CVUStatusEnum.RevertedToChecker.ToString()));
        }
        else if (HasPrivilege(TransactionNameContants.CVU_AUTHORIZE_BRANCH_TELLER_BATCHES) && SafePageController.QueryStrings["TxnName"] == TransactionNameContants.CVU_AUTHORIZE_BRANCH_TELLER_BATCHES)
        {
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.All), CVUStatusEnum.All.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.ApprovedByChecker), CVUStatusEnum.ApprovedByChecker.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.RevertedToBranch), CVUStatusEnum.RevertedToBranch.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.Submitted), CVUStatusEnum.Submitted.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.Resubmitted), CVUStatusEnum.Resubmitted.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.Completed), CVUStatusEnum.Completed.ToString()));
            ddlCVUStatus.Items.Add(new ListItem(GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + CVUStatusEnum.ApprovedByBranchCVU), CVUStatusEnum.ApprovedByBranchCVU.ToString()));

            ddlCVUStatus.SelectedValue = CVUStatusEnum.ApprovedByChecker.ToString();
        }
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
            , TransactionNameContants.CREATE_SARIE_TRANSFER
            , TransactionNameContants.INTERNATIONAL_TRANSFER
            , TransactionNameContants.CREATE_SWIFT_TRANSFER
            , TransactionNameContants.LOCAL_CHEQUE_CLEARING
            , TransactionNameContants.INTERNATIONAL_CHEQUE_COLLECTION

            , TransactionNameContants.CAPITALMARKETTRANSFER_TRANSACTION
            , TransactionNameContants.IPO_SUBSCRIPTION
            , TransactionNameContants.DISHONORED_CHEQUE
            , TransactionNameContants.CURRENCY_EXCHANGE

            ,TransactionNameContants.BILL_PAYMENT
            ,TransactionNameContants.CREATE_BILL_PAYMENT
            ,TransactionNameContants.BILL_PAYMENT_BY_CASH
            ,TransactionNameContants.PREPAID_BILL_PAYMENT
            ,TransactionNameContants.PREPAID_BILL_PAYMENT_BY_CASH
            ,TransactionNameContants.MOI_BILL_PAYMENT_BY_ACCOUNT
            ,TransactionNameContants.CREATE_MOI_PAYMENT
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
            ,TransactionNameContants.POST_MANUAL_CHARGES_TRANSACTION
            ,TransactionNameContants.ACCOUNTHOLD_OPERATION
            ,TransactionNameContants.GOVERNMENT_REVENUE_TRANSFER
        };

        ddlTransactionName.Items.Add(new ListItem(GetGlobalResource(VPResourceConstants.ICommon.ALL), string.Empty));

        foreach (var name in transactionNames)
        {
            ddlTransactionName.Items.Add(new ListItem(GetGlobalResource(string.Format("{0}.TxnDisplayName", name)), name));
        }
    }

    protected void btnDisplay_Click(object sender, EventArgs e)
    {
        FetchTransactions();
    }

    private void FetchTransactions()
    {
        var maxRecordCount = 250;   //UI default value
        Int32.TryParse(VpConfigurationParameters.GetGenericParameter("InquiryTxnListMaxRecordCount"), out maxRecordCount);

        var txnName = SafePageController.QueryStrings["TxnName"];

        var operationStages = new List<OperationStageEnumeration> { OperationStageEnumeration.Execute, OperationStageEnumeration.CloseCrmCase };

        var cvuStatus = new List<CVUStatusEnum>();

        if (ddlCVUStatus.SelectedValue == CVUStatusEnum.All.ToString())
        {
            cvuStatus.Add((CVUStatusEnum.ApprovedByChecker));
            cvuStatus.Add((CVUStatusEnum.RevertedToBranch));
            cvuStatus.Add((CVUStatusEnum.Submitted));
            cvuStatus.Add((CVUStatusEnum.Resubmitted));
            cvuStatus.Add((CVUStatusEnum.Completed));
            cvuStatus.Add((CVUStatusEnum.ApprovedByBranchCVU));

        }
        else
        {
            cvuStatus.Add((CVUStatusEnum)Enum.Parse(typeof(CVUStatusEnum), ddlCVUStatus.SelectedValue, true));
        }

        var request = new VpOnlineTransactionListInquiryRequest
        {
            BeginDate = ctlFromDate.ChosenDate,
            EndDate = ctlToDate.ChosenDate,
            ChannelID = (int)ChannelTypeEnum.Branch + 1,
            CIF = SafePageController.Customer.User.CifNo == VpConfigurationParameters.GetGenericParameter("NonCustomerCIF") ? null : SafePageController.Customer.User.CifNo,
            PerformerIdentity = ctlBranchUsers.SelectedValue,
            LastTransactionsCount = 0,
            PriviledgeDepth = GetPrivilegeDepth(txnName),
            OperationStage = operationStages,
            CVUStatus = cvuStatus,
            PerformerBranchCode = ctlBranchLookup.SelectedValue
        };

        if (!string.IsNullOrEmpty(ddlTransactionName.SelectedValue))
            request.TransactionName = new List<string>() { ddlTransactionName.SelectedValue };

        var response = ServicesHelper.DoOnlineTransactionListInquiry(request);
        SafePageController.SetProfileValue("VpOnlineTransactionListInquiryRequest", request);
        grdTransactionList.DataSource = null;
        grdUsers.DataSource = null;

        if (response != null && response.IsSuccess && response.UserTransactionList != null)
        {
            grdTransactionList.DataSource = response.UserTransactionList.UserTransactions;
            scTransactions.Visible = true;

            //Display Summary if All is selected
            if (ddlCVUStatus.SelectedValue == CVUStatusEnum.All.ToString())
            {
                grdUsers.DataSource = GetTransactionsUserSummary(response.UserTransactionList.UserTransactions);
                pnlUserSummary.Visible = true;
            }
            else
            {
                pnlUserSummary.Visible = false;
            }
        }
        else
        {
            scTransactions.Visible = false;

            if (ResponseHasErrorMessage(response))
            {
                AlertModal(GetErrorMessageFromResponse(response));
            }
        }


        grdTransactionList.DataBind();
        grdUsers.DataBind();
    }

    private object GetTransactionsUserSummary(UserTransaction[] transactions)
    {
        var dt = new DataTable();
        dt.Columns.Add("Type");
        dt.Columns.Add("PerformerBranchCode");
        dt.Columns.Add("BranchTotal");
        dt.Columns.Add("Submitted");
        dt.Columns.Add("Resubmitted");
        dt.Columns.Add("ApprovedByChecker");
        dt.Columns.Add("RevertedToBranch");
        dt.Columns.Add("Completed");

        var branches = transactions.Select(t => t.PerformerBranchCode.ToUpper()).Distinct().ToList();

        foreach (var branch in branches)
        {
            var branchTotal = transactions.Count(t => t.PerformerBranchCode.ToUpper() == branch);
            var submittedTotal = transactions.Where(t => t.CVUStatus == CVUStatusEnum.Submitted).Where(t => t.PerformerBranchCode.ToUpper() == branch).Count();
            var resubmittedTotal = transactions.Where(t => t.CVUStatus == CVUStatusEnum.Resubmitted).Where(t => t.PerformerBranchCode.ToUpper() == branch).Count();
            var approvedByCheckerTotal = transactions.Where(t => t.CVUStatus == CVUStatusEnum.ApprovedByChecker).Where(t => t.PerformerBranchCode.ToUpper() == branch).Count();
            var revertedToBranchTotal = transactions.Where(t => t.CVUStatus == CVUStatusEnum.RevertedToBranch).Where(t => t.PerformerBranchCode.ToUpper() == branch).Count();
            var completedTotal = transactions.Where(t => t.CVUStatus == CVUStatusEnum.Completed).Where(t => t.PerformerBranchCode.ToUpper() == branch).Count();

            var branchName = ServicesHelper.GetBranchNameByCode(branch, UserLanguage);

            dt.Rows.Add("Branch", branchName, branchTotal, submittedTotal, resubmittedTotal, approvedByCheckerTotal, revertedToBranchTotal, completedTotal);
        }

        //Removed option to create batch on branch level.
        //if (tellers.Count > 1)
        //    dt.Rows.Add("B", ServicesHelper.GetBranchNameByCode(SafePageController.Performer.User.BranchCode) , transactions.Count());

        return dt;
    }

    private void ResetBranchUsers()
    {
        var dt = new DataTable();
        dt.Columns.Add("UserName");
        dt.PrimaryKey = new[] { dt.Columns["UserName"] };

        ctlBranchUsers.DataTable = dt;

    }

    protected void grdTransactionList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var userTransaction = ((UserTransaction)e.Row.DataItem);

                var litBranch = e.Row.FindControl("litBranch") as Literal;
                if (litBranch != null)
                {
                    litBranch.Text = ServicesHelper.GetBranchNameByCode(userTransaction.PerformerBranchCode, UserLanguage);
                }

                var litTransactionDate = e.Row.FindControl("litTransactionDate") as Literal;
                if (litTransactionDate != null)
                {
                    litTransactionDate.Text = userTransaction.TransactionDate.ToString(VpConstants.Dates.DateTimeFormat, ci);
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

                var litCVUStatus = e.Row.FindControl("litCVUStatus") as Literal;
                if (litCVUStatus != null)
                {
                    litCVUStatus.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.CVUStatusResourceKey + userTransaction.CVUStatus);
                }

                var litCustomerHasOralAgreement = e.Row.FindControl("litCustomerHasOralAgreement") as Literal;
                if (litCustomerHasOralAgreement != null)
                {
                    litCustomerHasOralAgreement.Text = Convert.ToBoolean(userTransaction.CustomerHasOralAgreement) ? GetGlobalResource("Yes.Text") : GetGlobalResource("No.Text");
                }

                var litOriginalDocReceived = e.Row.FindControl("litOriginalDocReceived") as Literal;
                if (litOriginalDocReceived != null)
                {
                    if (userTransaction.IsOriginalDocumentReceived != null)
                    {
                        if (userTransaction.IsOriginalDocumentReceived == true)
                            litOriginalDocReceived.Text = GetGlobalResource("Yes.Text");
                        else
                            litOriginalDocReceived.Text = GetGlobalResource("No.Text") + " (" + (DateTime.Now - userTransaction.TransactionDate).Days + " " + GetGlobalResource("Days.Text") + ")";
                    }
                }

                var litRiskStatus = e.Row.FindControl("litRiskStatus") as Literal;
                if (litRiskStatus != null)
                {
                    if (userTransaction.RiskStatus != RiskStatusEnum.All && userTransaction.RiskStatus != RiskStatusEnum.Unassigned)
                        litRiskStatus.Text = GetGlobalResource(VpConstants.GlobalResource.CommonEnums, VpConstants.Services.RiskStatusResourceKey + userTransaction.RiskStatus);
                }

                var btnDetails = e.Row.FindControl("btnDetails") as VBImageButton;
                if (btnDetails != null)
                {
                    if (e.Row.RowIndex < 9)
                        btnDetails.AccessKey = (e.Row.RowIndex + 1).ToString();
                    btnDetails.Visible = userTransaction.Channel == ((int)ChannelTypeEnum.Branch + 1);
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
            if (e.CommandName.ToLower() == "details")
            {
                SafePageController.SetProfileValue("ddlCVUStatus_Enabled", ddlCVUStatus.Enabled);
                SafePageController.SetProfileValue("ddlTransactionName_Enabled", ddlTransactionName.Enabled);
                var referenceNumber = e.CommandArgument.ToString();

                if (!string.IsNullOrEmpty(referenceNumber))
                {
                    SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber, referenceNumber);

                    SafePageController.PageControllerRedirect("CVUTransactionDetails.aspx");
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
            SetErrorDisplayDefault();
        }
    }

    protected void ctlBranchLookup_ItemSelected(object sender, LookupControlEventArgs e)
    {
        if (e != null && e.table != null && e.table.Rows.Count > 0)
        {
            var code = e.table.Rows[0][VpPageControllerConstants.Branch.BranchCode.ToString()] as string;

            ctlBranchUsers.DataTable = ServicesHelper.DoUserBranchHierarchyInquiry(code, GetPrivilegeDepth(SafePageController.QueryStrings["TxnName"]));
        }

    }

    protected void ctlBranchLookup_OnSelectionCleared(object sender)
    {
        var txnName = SafePageController.QueryStrings["TxnName"];

        var branches = ServicesHelper.DoUserBranchHierarchyInquiry(GetPrivilegeDepth(txnName));
        ctlBranchLookup.DataTable = branches;

        ResetBranchUsers();

    }


}
