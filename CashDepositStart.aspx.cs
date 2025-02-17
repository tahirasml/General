using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CashDrawerManagement.MessageDefinitions.CashDeposit;
using Common.Definitions.AccountsManagement;
using Common.Definitions.CashDrawerManagement;
using IAL.CashDrawerModule.MessageDefinitions.Common;
using IAL.MessageDefinitions.Common.Extended;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.Common.ObjectModel;
using VeriBranch.WebApplication.Constants;
using VeriBranch.WebApplication.FormatUtilities;
using VeriBranch.WebApplication.ObjectModel;
using VeriBranch.WebApplication.UIProcess;
using System.Web.UI;
using VeriBranch.WebApplication.Helpers;

/// <summary>
/// Start page for the Cash Deposit transaction
/// </summary>
public partial class CashDepositStart : VeriBranchTransactionStartBasePage<VPCashDepositTransactionRequest, VPCashDepositTransactionResponse>
{
    /// <summary>
    /// Gets the name of the transaction. The transaction name is used to fetch charges against the transaction.
    /// </summary>
    /// <value>
    /// The name of the transaction.
    /// </value>
    public override string TransactionName
    {
        get { return TransactionNameContants.CASH_DEPOSIT; }
    }

    protected override string BeneficiaryName
    {
        get
        {
            VPCashDepositTransactionRequest request = RequestData;

            if (request != null && request.IALCashDepositRequest != null
                && request.IALCashDepositRequest.DepositDetail != null
                && request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail != null
                && !request.IALCashDepositRequest.DepositDetail.IsDepositedByCustomer)
            {
                return request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail.PerformerName;
            }
            else
            {
                return string.Empty;
            }

        }
    }

    protected override string BeneficiaryID
    {
        get
        {
            VPCashDepositTransactionRequest request = RequestData;

            if (request != null && request.IALCashDepositRequest != null
                && request.IALCashDepositRequest.DepositDetail != null
                && request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail != null
                && request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail.CustomerIdentification != null
                && !request.IALCashDepositRequest.DepositDetail.IsDepositedByCustomer)
            {
                return request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail.CustomerIdentification.IdentityDocNumber;
            }
            else
            {
                return string.Empty;
            }

        }
    }

    /// <summary>
    /// Gets the ExchangeRateChecked from ctlForwardToReferCardControl
    /// </summary>
    /// <value>
    /// The ExchangeRateChecked of the transaction.
    /// </value>
    protected override bool ExchangeRateChecked
    {
        get { return ctlForwardToReferCardControl.ExchangeRateChecked; }
    }

    /// <summary>
    /// Gets the OverDrawnChecked from ctlForwardToReferCardControl
    /// </summary>
    /// <value>
    /// The OverDrawnChecked of the transaction.
    /// </value>
    protected override bool OverDrawnChecked
    {
        get { return ctlForwardToReferCardControl.OverDrawnChecked; }
    }

    /// <summary>
    /// Gets the ValueDateChecked from ctlForwardToReferCardControl
    /// </summary>
    /// <value>
    /// The ValueDateChecked of the transaction.
    /// </value>
    protected override bool ValueDateChecked
    {
        get { return ctlForwardToReferCardControl.ValueDateChecked; }
    }

    /// <summary>
    /// Gets the ReferCardRemarks from ctlForwardToReferCardControl
    /// </summary>
    /// <value>
    /// The ReferCardRemarks of the transaction.
    /// </value>
    protected override string ReferCardRemarks
    {
        get { return ctlForwardToReferCardControl.Remarks; }
    }
 
    /// <summary>
    /// The helper method which will call the VBL for this transaction.
    /// </summary>
    protected override TransactionOperation Operation
    {
        get { return CashDrawerHelper.DoCashDepositTransaction; }
    }

    /// <summary>
    /// Set the initial state of the request before any changes are made to the request object.
    /// The initial request is compared with the actual request object to find if particular fields are changed and CRM case needs to be opened.
    /// </summary>
    /// <returns></returns>
    protected override VPCashDepositTransactionRequest SetInitialRequest()
    {
        return new VPCashDepositTransactionRequest
            {
            IALCashDepositRequest = new VpCashDepositRequest
                {
                FinancialData = new VpFinancialData
                {
                    ChargesDetail = ctlChargesAndOffersTabs.FetchedTransactionCharges
                }
            }
        };
    }

    protected override string AdviceDocumentResourceKey
    {
        get { return VpConstants.AdviceDocuments.CashDeposit; }
    }

    protected override string ReceiptDocumentResourceKey
    {
        get { return VpConstants.ReceiptDocuments.CashDeposit; }
    }

    /// <summary>
    /// Creates the request object based on the inputs provided by the user.
    /// </summary>
    /// <param name="existingRequest"></param>
    /// <returns></returns>
    protected override VPCashDepositTransactionRequest GetRequestForConfirmation(VPCashDepositTransactionRequest existingRequest)
    {
        var request = existingRequest;

        var customLogList = new CustomLogEntryList();

        //var charges = ctlChargesAndOffersTabs.TransactionCharges;

        VpDrawerInfo _CashDrawerInfo = null;
        if (ctlCurrencyDenominations.DataSource as CurrencyDenominationList != null)
        {
            _CashDrawerInfo = new VpDrawerInfo();
            _CashDrawerInfo.AccountInfo = new VpAccount();
            _CashDrawerInfo.AccountInfo.AccountNumber = hidCashDrawerAccountNo.Value;
            _CashDrawerInfo.AccountInfo.CurrencyCode = ctlDepositCurrency.SelectedValue;
            _CashDrawerInfo.AccountInfo.BranchCode = SafePageController.Performer.Agent.BranchCode;
            _CashDrawerInfo.DenominationDetail = new VpDenominationDetail();
            _CashDrawerInfo.DenominationDetail.CurrencyCode = ctlDepositCurrency.SelectedValue;
            _CashDrawerInfo.DenominationDetail.Denomination = (ctlCurrencyDenominations.DataSource as CurrencyDenominationList).GetVPDenominationDetail();
        }

        VpDrawerInfoExtended _TCRDrawerInfo = null;
        if (ctlTCRDenominations.DataSource as CurrencyDenominationList != null)
        {
            _TCRDrawerInfo = new VpDrawerInfoExtended();
            _TCRDrawerInfo.DeviceName = ctlTCRDenominations.TCRName;
            _TCRDrawerInfo.AccountInfo = new VpAccount();
            _TCRDrawerInfo.AccountInfo.AccountNumber = ctlTCRDenominations.AccountNumber;
            _TCRDrawerInfo.AccountInfo.CurrencyCode = ctlDepositCurrency.SelectedValue;
            _TCRDrawerInfo.AccountInfo.BranchCode = SafePageController.Performer.Agent.BranchCode;
            _TCRDrawerInfo.DenominationDetail = new VpDenominationDetail();
            _TCRDrawerInfo.DenominationDetail.CurrencyCode = ctlDepositCurrency.SelectedValue;
            _TCRDrawerInfo.DenominationDetail.Denomination = (ctlTCRDenominations.DataSource as CurrencyDenominationList).GetVPDenominationDetail();
            _TCRDrawerInfo.Amount = ctlTCRDenominations.Amount;
        }

        request.IALCashDepositRequest = new VpCashDepositRequest();
        request.IALCashDepositRequest.DepositDetail = new VpDepositDetail();
        request.IALCashDepositRequest.DepositDetail.PurposeOfDeposit = ddlDepositPurpose.SelectedValue;
        request.IALCashDepositRequest.DepositDetail.DepositAmount = ctlTotalDepositAmountValue.Amount;
        request.IALCashDepositRequest.DepositDetail.DepositCurrencycode = ctlDepositCurrency.SelectedValue;
        request.IALCashDepositRequest.DepositDetail.ExchangeRate = ctlExchangeRate.ExchangeRate;
        request.IALCashDepositRequest.DepositDetail.Narration1 = ctlNarrations.Narration1;
        request.IALCashDepositRequest.DepositDetail.Narration2 = ctlNarrations.Narration2;
        request.IALCashDepositRequest.DepositDetail.SourceOfFunds = ddlSourceOfFunds.SelectedValue;
        request.IALCashDepositRequest.DepositDetail.IsDepositedByCustomer = ctlThirdPartyDetails.IsSelf;
        request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail = ctlThirdPartyDetails.ThirdPartyDetails.ToIAL();
        request.IALCashDepositRequest.CashDrawerInfo = _CashDrawerInfo;
        request.IALCashDepositRequest.TCRDrawerInfo = _TCRDrawerInfo;
        request.IALCashDepositRequest.CIF = SafePageController.TransactionHeader.User.CifNo;
        request.IALCashDepositRequest.CustomerAccount = new VpAccount();
        request.IALCashDepositRequest.CustomerAccount.AccountNumber = ctlAccountNumber.SelectedValue;
        request.IALCashDepositRequest.CustomerAccount.BranchCode = hidBranchCode.Value;
        request.IALCashDepositRequest.CustomerAccount.CurrencyCode = hidAccountCurrency.Value;
        request.IALCashDepositRequest.FinancialData = new VpFinancialData();
        request.IALCashDepositRequest.FinancialData.ChargesDetail = ctlChargesAndOffersTabs.TransactionCharges;
        request.IALCashDepositRequest.FinancialData.ExchangeRates = ctlExchangeRate.ExchangeRate;
        request.IALCashDepositRequest.FinancialData.ValueDate = ctlValueDate.ChosenDate.ToIALFormat();
        request.IALCashDepositRequest.FinancialData.BuyAmount = lblAmountAccountCCYValue.Amount;
        request.IALCashDepositRequest.FinancialData.SellAmount = SellAmount;

        if(IsAskCall)
        {
            request.IALCashDepositRequest.DepositDetail.IsDepositedByCustomer = true;
        }

        FillChargeAccountNumber(request.GetIALRequest().FinancialData.ChargesDetail, ctlAccountNumber.SelectedValue);
        //request.IALCashDepositRequest.FinancialData.ChargesDetail.Where(c=> string.IsNullOrEmpty(c.ChargeAccountNumber)).ToList().ForEach(c => c.ChargeAccountNumber = ctlAccountNumber.SelectedValue);

        request.EntitlementCode = ctlChargesAndOffersTabs.SelectedEntitlementCode;
        if (request.IALCashDepositRequest != null)
        {
            customLogList.AddLogEntry(VpPageControllerConstants.Account.ChargesAccountCurrency, ctlChargesAndOffersTabs.AccountCurrency);
        }

        customLogList.AddLogEntry(VpPageControllerConstants.Transaction.PerformerType, ((int)ctlThirdPartyDetails.PerformerType));
        customLogList.AddLogEntry(VpPageControllerConstants.Account.AmountInWords, ctlAmountInWords.Text);
        customLogList.AddLogEntry(VpPageControllerConstants.Account.QueryType, hidQueryType.Value);
        customLogList.AddLogEntry(VpPageControllerConstants.Account.AccountHolderName, hidAccountTitle.Value);
        customLogList.AddLogEntry(VpPageControllerConstants.Transaction.CurrencyName, ctlDepositCurrency.Text);
        customLogList.AddLogEntry(VpPageControllerConstants.Transaction.SourceOfFunds, ddlSourceOfFunds.SelectedItem.Text);
        customLogList.AddLogEntry(VpPageControllerConstants.Transaction.DepositPurpose, ddlDepositPurpose.SelectedItem.Text);
        request.CustomLogEntryList = customLogList;

                

        return request;
    }

    /// <summary>
    /// This method is called on (!PostBack) to set initial values on the UI controls,
    /// usually when coming back from the Confirm Page.
    /// </summary>
    protected override void SetUIFromState()
    {
        var request = SafePageController.RequestData as VPCashDepositTransactionRequest;

        if (request != null)
        {
            var list = new CustomLogEntryList();
            if (request.CustomLogEntryList != null)
                list = request.CustomLogEntryList;

            ddlSourceOfFunds.SelectedValue = request.IALCashDepositRequest.DepositDetail.SourceOfFunds;
            ddlDepositPurpose.SelectedValue = request.IALCashDepositRequest.DepositDetail.PurposeOfDeposit;
            ctlNarrations.Narration1 = request.IALCashDepositRequest.DepositDetail.Narration1;
            ctlNarrations.Narration2 = request.IALCashDepositRequest.DepositDetail.Narration2;
            //ctlNarrations.Narration3 = request.IALCashDepositRequest.DepositDetail.Narration3;
            //ctlNarrations.Narration4 = request.IALCashDepositRequest.DepositDetail.Narration4;
            ctlNarrations.Description = request.IALCashDepositRequest.DepositDetail.PurposeOfDeposit;

            ctlThirdPartyDetails.ThirdPartyDetails = new VPThirdPartyDetails(request.IALCashDepositRequest.DepositDetail.ThirdPartyDetail, list.GetLogValue<int>(VpPageControllerConstants.Transaction.PerformerType));

            ctlValueDate.ChosenDate = request.IALCashDepositRequest.FinancialData.ValueDate.DateFromIALFormat();
            ctlValueDate.ReadOnly = false;

            ctlAccountNumber.SelectedValue = request.IALCashDepositRequest.CustomerAccount.AccountNumber;

            lblAccountBranchValue.Text = ServicesHelper.GetBranchNameByCode(request.IALCashDepositRequest.CustomerAccount.BranchCode);

            hidAccountCurrency.Value = request.IALCashDepositRequest.CustomerAccount.CurrencyCode;
            hidBranchCode.Value = request.IALCashDepositRequest.CustomerAccount.BranchCode;
            hidQueryType.Value = list.GetLogValue(VpPageControllerConstants.Account.QueryType);
            hidAccountTitle.Value = list.GetLogValue(VpPageControllerConstants.Account.AccountHolderName);
            lblAccountNameValue.Text = list.GetLogValue(VpPageControllerConstants.Account.AccountHolderName);

            hidCashDrawerAccountNo.Value = request.IALCashDepositRequest.CashDrawerInfo.AccountInfo.AccountNumber;

            ctlDepositCurrency.SelectedValue = request.IALCashDepositRequest.DepositDetail.DepositCurrencycode;
            hidDepositCurrency.Value = request.IALCashDepositRequest.DepositDetail.DepositCurrencycode;

            if (request.IALCashDepositRequest.CustomerAccount.CurrencyCode != request.IALCashDepositRequest.DepositDetail.DepositCurrencycode)
            {
                ctlExchangeRate.Enabled = true;
            }

            ctlTotalDepositAmountValue.CurrencyCode = request.IALCashDepositRequest.DepositDetail.DepositCurrencycode;
            ctlTotalDepositAmountValue.Amount = request.IALCashDepositRequest.DepositDetail.DepositAmount;

            ctlDepositAmount.Enabled = true;
            ctlDepositAmount.CurrencyName = request.IALCashDepositRequest.DepositDetail.DepositCurrencycode;
            ctlDepositAmount.Amount = request.IALCashDepositRequest.DepositDetail.DepositAmount;

            ctlExchangeRate.SetExchangeRate(request.IALCashDepositRequest.DepositDetail.DepositCurrencycode, request.IALCashDepositRequest.CustomerAccount.CurrencyCode, request.IALCashDepositRequest.FinancialData.ExchangeRates);

            var currencyDenominationList = CashDrawerHelper.GetCurrencyDenominations(request.IALCashDepositRequest.DepositDetail.DepositCurrencycode);
            ctlCurrencyDenominations.CurrencyCode = request.IALCashDepositRequest.DepositDetail.DepositCurrencycode;
            ctlCurrencyDenominations.DataSource = CurrencyDenominationList.GetFilledCurrencyDenominationList(request.IALCashDepositRequest.CashDrawerInfo.DenominationDetail.Denomination, currencyDenominationList);
            ctlCurrencyDenominations.DataBind();

            ctlTCRDenominations.CurrencyCode = request.IALCashDepositRequest.DepositDetail.DepositCurrencycode; //TODO - to check if DepositCurrency should be assigned or currency from the TCRDrawerInfo.AccountInfo object
            if (request.IALCashDepositRequest.TCRDrawerInfo != null)
            {
                ctlTCRDenominations.AccountNumber = request.IALCashDepositRequest.TCRDrawerInfo.AccountInfo.AccountNumber;
                var tcrDenominationList = CashDrawerHelper.GetCurrencyDenominations(request.IALCashDepositRequest.DepositDetail.DepositCurrencycode);
                ctlTCRDenominations.DataSource = CurrencyDenominationList.GetFilledCurrencyDenominationList(request.IALCashDepositRequest.TCRDrawerInfo.DenominationDetail.Denomination, tcrDenominationList);
                ctlTCRDenominations.DataBind();
            }

            lblExchangeRate.Enabled = true;
            lblDepositAmount.Enabled = true;
            ctlCurrencyDenominations.Enabled = true;
            lblTCRDepositAmount.Enabled = true;
            ctlTCRDenominations.Enabled = true;

            ctlAmountInWords.Text = list.GetLogValue(VpPageControllerConstants.Account.AmountInWords);

            lblAmountAccountCCYValue.CurrencyCode = request.IALCashDepositRequest.CustomerAccount.CurrencyCode;
            lblAmountAccountCCYValue.Amount = request.IALCashDepositRequest.FinancialData.BuyAmount;
            lblAmountAccountCCYValue.Visible = request.IALCashDepositRequest.CustomerAccount.CurrencyCode != request.IALCashDepositRequest.DepositDetail.DepositCurrencycode;

            //ctlChargesAndOffersTabs.ChargeSource = request.IALCashDepositRequest.ChargesDetail.ChargeSource;
            //ctlChargesAndOffersTabs.AccountNumber = request.IALCashDepositRequest.ChargesDetail.ChargeAccountNumber;
            ctlChargesAndOffersTabs.TransactionCharges = request.IALCashDepositRequest.FinancialData.ChargesDetail;
            ctlChargesAndOffersTabs.SelectedEntitlementCode = request.EntitlementCode;

            if (request.ReferCardApprovalFlag != null)
            {
                ctlForwardToReferCardControl.SetControlFromReferCardApprovalFlags(request.ReferCardApprovalFlag);
            }
        }

    
    }

    /// <summary>
    /// Code to be executed when the page is requested
    /// </summary>
    protected override void DoPageAction()
    {
     
       var isReferCardApproved = SafePageController.GetStateValue("ReferCardApproved");
       var isComplianceApproved = SafePageController.GetStateValue("ComplianceApproved");
       LoadSourceOfFunds();
       LoadDepositOfPurpose();
       //For approved transactiopns.
       if ((isReferCardApproved != null && isReferCardApproved.ToString() == "true") ||
           (isComplianceApproved != null && isComplianceApproved.ToString() == "true"))
       {

           //Account details inquiry needs to be called for internal accounts. 
           //It will allow IAL to cache the customer profile inquiry response from account details inquiry.
           var Approved_AccountNumber = SafePageController.GetStateValue("Approved_AccountNumber");
           if (Approved_AccountNumber != null && !string.IsNullOrEmpty(Approved_AccountNumber.ToString()))
           {
               AccountHelper.GetAccountDetail(VpPageControllerConstants.Account.CASA.ToString(), Approved_AccountNumber.ToString());
           }

           ctlAccountNumber.DataTable = AccountHelper.GetAccountListDataTable(true, VpPageControllerConstants.Account.CASA.ToString());
           ctlDepositCurrency.DataTable = CashDrawerHelper.GetCashDrawerCurrenciesDataTable();

       }
       else
       {
           ctlAccountNumber.DataTable = AccountHelper.GetAccountListDataTable(false, VpPageControllerConstants.Account.CASA.ToString());
           ctlDepositCurrency.DataTable = CashDrawerHelper.GetCashDrawerCurrenciesDataTable();
       }

           
        
    }


    private void LoadDepositOfPurpose()
    {
        ddlDepositPurpose.Items.Clear();



        ddlDepositPurpose.Items.Add(new ListItem(ResourceHelper.Resources[VPResourceConstants.IValidationConstant.PLEASE_SELECT], string.Empty));
        foreach (var name in Enum.GetNames(typeof(PurposeOfDeposit)))
        {
            ddlDepositPurpose.Items.Add(new ListItem(ResourceHelper.Resources[string.Format("PurposeOfDeposit.{0}.Text", name)], name));
        }



    }
    private void LoadSourceOfFunds()
    {
        ddlSourceOfFunds.Items.Clear();



        ddlSourceOfFunds.Items.Add(new ListItem(ResourceHelper.Resources[VPResourceConstants.IValidationConstant.PLEASE_SELECT], string.Empty));
        foreach (var name in Enum.GetNames(typeof(SourceOfFunds)))
        {
            ddlSourceOfFunds.Items.Add(new ListItem(ResourceHelper.Resources[string.Format("SourceOfFunds.{0}.Text", name)], name));
        }



    }

    /// <summary>
    /// Fired when credit account is selected for the cash deposit
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ctlAccountNumber_OnItemSelected(object sender, LookupControlEventArgs e)
    {
        try
        {
            lblAccountBranchValue.Text = string.Empty;
            hidAccountCurrency.Value = string.Empty;

            if (e != null && e.table != null && e.table.Rows.Count > 0)
            {
                var row = e.table.Rows[0];
                var accountNumber = row[VpPageControllerConstants.Account.AccountNumber.ToString()] as string;

                lblAccountBranchValue.Text = row[VpPageControllerConstants.Account.BranchName.ToString()] as string;
                hidBranchCode.Value = row[VpPageControllerConstants.Account.BranchCode.ToString()] as string;
                hidQueryType.Value = row[VpPageControllerConstants.Account.QueryType.ToString()] as string;
                hidAccountCurrency.Value = row[VpPageControllerConstants.Account.Currency.ToString()] as string;

                hidAccountTitle.Value = row[VpPageControllerConstants.Account.AccountTitle.ToString()] as string;
                lblAccountNameValue.Text = row[VpPageControllerConstants.Account.AccountTitle.ToString()] as string;

                //if (!Convert.ToBoolean((row[VpPageControllerConstants.Account.IsCreditAllowed.ToString()])))
                //{
                //    SetErrorDisplay(GetLocalResource("cvValidateCreditOnlyAccounts.ErrorMessage"));
                //}
            }

            SetExchangeRate();

            SetAmountAccountCCY();
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }


    /// <summary>
    /// Sets the credit amount for the credit account after applying the exchange rate.
    /// </summary>
    private void SetAmountAccountCCY()
    {
        lblAmountAccountCCYValue.Reset();

        if (ctlExchangeRate.HasValue && hidAccountCurrency.Value != ctlDepositAmount.SelectedCurrencyName)
        {
            lblAmountAccountCCYValue.Amount = ctlExchangeRate.ExchangeRate * (ctlCurrencyDenominations.Amount + ctlTCRDenominations.Amount);
            lblAmountAccountCCYValue.CurrencyCode = hidAccountCurrency.Value;
        }
    }

    /// <summary>
    /// Fired when deposit currency is selected by the user.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ctlDepositCurrency_OnItemSelected(object sender, LookupControlEventArgs e)
    {
        try
        {
            hidCashDrawerAccountNo.Value = string.Empty;
            hidDepositCurrency.Value = string.Empty;
            if (e != null && e.table != null && e.table.Rows.Count > 0)
            {
                var ccode = e.table.Rows[0][VpConstants.Services.CurrencyColumnISOCurrencyCode] as string;
                hidDepositCurrency.Value = ccode;
                hidCashDrawerAccountNo.Value = e.table.Rows[0][VpConstants.Services.CashDrawerAccountNumber] as string;

                ctlCurrencyDenominations.CurrencyCode = ccode;

                SetCurrencyDenominations(hidDepositCurrency.Value);

                lblExchangeRate.Enabled = true;
                lblDepositAmount.Enabled = true;
                ctlExchangeRate.Enabled = true;
                ctlCurrencyDenominations.Enabled = true;

                lblTCRDepositAmount.Enabled = true;
                ctlTCRDenominations.Enabled = true;
                ctlTCRDenominations.CurrencyCode = ccode;
                ctlDepositAmount.Enabled = true;
                ctlDepositAmount.CurrencyName = ccode;
            }

            SetExchangeRate();

            ctlAmountInWords.Reset();
            lblAmountAccountCCYValue.Reset();
            ctlTotalDepositAmountValue.Reset();
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    /// <summary>
    /// Set the currency code on the denomination list control.
    /// </summary>
    /// <param name="currencyCode"></param>
    private void SetCurrencyDenominations(string currencyCode)
    {
        var denominations = CashDrawerHelper.GetCurrencyDenominations(currencyCode);

        ctlCurrencyDenominations.DataSource = denominations;
        ctlCurrencyDenominations.DataBind();
    }

    /// <summary>
    /// Sets the exchange rate between the two currencies (deposit currency and credit account currency)
    /// </summary>
    private void SetExchangeRate()
    {
        string sourceCurrency = hidDepositCurrency.Value;
        string destinationCurrency = hidAccountCurrency.Value;

        //SetRate();

        if (!string.IsNullOrEmpty(destinationCurrency) && !string.IsNullOrEmpty(sourceCurrency))
        {
            if (sourceCurrency == destinationCurrency)
            {
            }
            else
            {
            }

            OverrideExRate_CheckedChanged(null, null);
        }
    }

    /// <summary>
    /// Sets the exchange rate between the two currencies (deposit currency and credit account currency)
    /// </summary>
    private void SetRate()
    {
        string sourceCurrency = hidDepositCurrency.Value;
        string destinationCurrency = hidAccountCurrency.Value;

        ctlExchangeRate.SetExchangeRate(sourceCurrency, destinationCurrency, null);
    }

    /// <summary>
    /// Validates if Credit transaction is allowed on the selected credit account.
    /// </summary>
    /// <param name="accountNumber"></param>
    /// <param name="queryType"></param>
    /// <returns></returns>
    bool ValidateCreditOnlyAccounts(string accountNumber, string queryType)
    {
        var AccountList = AccountHelper.GetAccountListDataTable();
        var account = AccountList.Select(string.Format("{0} = '{1}'", VpPageControllerConstants.Account.AccountNumber.ToString(), accountNumber))[0];
        return Convert.ToBoolean(account[VpPageControllerConstants.Account.IsCreditAllowed.ToString()]);
    }

    /// <summary>
    /// Handles the ServerValidate event of cvValidateCreditOnlyAccounts
    /// </summary>
    /// <param name="source"></param>
    /// <param name="args"></param>
    protected void cvValidateCreditOnlyAccounts_ServerValidate(object source, ServerValidateEventArgs args)
    {
        try
        {
            args.IsValid = false;

            //if (IsPostBackComingFromNextButton())
            //{
                var accountNumber = ctlAccountNumber.SelectedValue;
                var queryType = hidQueryType.Value;

                if (!string.IsNullOrEmpty(accountNumber) && !string.IsNullOrEmpty(queryType))
                {
                    args.IsValid = ValidateCreditOnlyAccounts(accountNumber, queryType);

                    if (!args.IsValid)
                        SetErrorDisplay(GetLocalResource("cvValidateCreditOnlyAccounts.ErrorMessage"));
                }
            //}
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    /// <summary>
    /// Handles the ServerValidate event of cvValidateDepositCurrencies
    /// </summary>
    /// <param name="source"></param>
    /// <param name="args"></param>
    protected void cvValidateDepositCurrencies_ServerValidate(object source, ServerValidateEventArgs args)
    {
        try
        {
            args.IsValid = ctlTCRDenominations.CurrencyCode == ctlCurrencyDenominations.CurrencyCode || string.IsNullOrEmpty(ctlTCRDenominations.CurrencyCode) || string.IsNullOrEmpty(ctlCurrencyDenominations.CurrencyCode);

            if (!args.IsValid)
            {
                SetErrorDisplay(GetLocalResource("cvValidateDepositCurrencies.ErrorMessage"));
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    /// <summary>
    /// Handles the CheckedChanged event of rbOverrideExRateYes and rbOverrideExRateNo radio buttons
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OverrideExRate_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            ctlExchangeRate.Enabled = true;
            SetRate();
            SetAmountAccountCCY();
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    protected override string OnExternalCallReceived(VpFinancialData financialData)
    {
        ctlValueDate.ChosenDate = Convert.ToDateTime(financialData.ValueDate.DateFromIALFormat());
        ctlValueDate.ReadOnly = false;
        ctlChargesAndOffersTabs.TransactionCharges = financialData.ChargesDetail;
        lblAmountAccountCCYValue.CurrencyCode = hidAccountCurrency.Value;
        lblAmountAccountCCYValue.Amount = financialData.BuyAmount;
        lblAmountAccountCCYValue.Visible = hidAccountCurrency.Value != ctlDepositCurrency.SelectedValue;
        ctlExchangeRate.SetExchangeRate(RequestData.IALCashDepositRequest.DepositDetail.DepositCurrencycode, RequestData.IALCashDepositRequest.CustomerAccount.CurrencyCode, financialData.ExchangeRates);
        return string.Empty;
        //return string.Format("onExRateChanged({0});", financialData.ExchangeRates);
    }

    protected override string GetLoggedInCustomerAccount()
    {
        return RequestData.IALCashDepositRequest.CustomerAccount.AccountNumber;
    }

    protected override bool IsSignatureVerificationRequired()
    {
        var list = new CustomLogEntryList();
        if (RequestData.CustomLogEntryList != null)
        {
            list = RequestData.CustomLogEntryList;

            var performerType = list.GetLogValue<int>(VpPageControllerConstants.Transaction.PerformerType);
            return performerType == (int)VpConstants.TransactionPerformerType.Self || performerType == (int)VpConstants.TransactionPerformerType.Representative;
        }


        return true;
    }

    protected override void CheckIfReferCardApprovalRequired()
    {
          //For refer card approved transactiopns.
          if (IsReferCardApproved)
          {
              ctlForwardToReferCardControl.Enabled = false;

          }
          else
          {
              if (!VpConfigurationManager.IsAskFieldChangeEnabled)
              {
                  ctlValueDate.ReadOnly = true;
                  ctlExchangeRate.Enabled = false;
              }

              ctlForwardToReferCardControl.EnableForwardToReferCard = true;

              if (IsExchangeRateApprovalEnforced)
              {
                  btnNavigationControl.btnReferCardVisible = true;
                  ctlExchangeRate.Enabled = false;

                  ctlForwardToReferCardControl.ExchangeRateCheckEnforced = true;
                  
              }
              
          }

    }

    protected override void DisableAllControls()
    {
      
        //disable all controls on pageEnableForwardToReferCard
        ctlAccountNumber.Enabled = false;
        ctlDepositCurrency.Enabled = false;

        ctlValueDate.ReadOnly = true;
        ctlExchangeRate.Enabled = false;
        ctlDepositAmount.Enabled = false;
        lblExchangeRate.Enabled = false;
        lblDepositAmount.Enabled = false;
        ctlCurrencyDenominations.Enabled = false;
        lblTCRDepositAmount.Enabled = false;
        ctlTCRDenominations.Enabled = false;
        
        ddlSourceOfFunds.Enabled = false;
        ddlDepositPurpose.Enabled = false;
        ctlThirdPartyDetails.Enabled = false;
        ctlNarrations.Enabled = false;
        ctlChargesAndOffersTabs.Enabled = false;

        ctlForwardToReferCardControl.Enabled = false;

        //Equivlent amount will be set based on exhange rate.
        //SetAmountAccountCCY();

    }

    protected override void ForwardToReferCardClicked(bool isChecked)
    {

        if (ctlExchangeRate.ExchangeRate != 1)
        {
            ctlForwardToReferCardControl.EnableExchangeRate = isChecked;
        }

        if (isChecked)
        {
            ctlValueDate.ReadOnly = isChecked;
            ctlExchangeRate.Enabled = !isChecked;
        }
        else if (VpConfigurationManager.IsAskFieldChangeEnabled)
        {
            ctlValueDate.ReadOnly = isChecked;
            ctlExchangeRate.Enabled = !isChecked;
        }

        ctlAccountNumber.Enabled = !isChecked;
        ctlDepositCurrency.Enabled = !isChecked;

        ctlDepositAmount.Enabled = !isChecked;
        lblExchangeRate.Enabled = !isChecked;
        lblDepositAmount.Enabled = !isChecked;
        ctlCurrencyDenominations.Enabled = !isChecked;
        lblTCRDepositAmount.Enabled = !isChecked;
        ctlTCRDenominations.Enabled = !isChecked;

        ddlSourceOfFunds.Enabled = !isChecked;
        ctlThirdPartyDetails.Enabled = !isChecked;
        ctlNarrations.Enabled = !isChecked;
        ctlChargesAndOffersTabs.Enabled = !isChecked;

        btnNavigationControl.btnReferCardVisible = isChecked;
        

    }

    protected override void EnableBackButton(string backURL)
    {
        //Enable Back Button for refer card request
        btnNavigationControl.BackButton.Visible = true;
        btnNavigationControl.BackUrl = backURL;

    }
    

}
