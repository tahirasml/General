using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using IAL.MessageDefinitions.Common.Extended;
using IAL.VPExternalCallBase.CoreApi.MessageDefinitions;
using IAL.VpExternalCallBase.CoreApi.Interfaces;
using VeriBranch.Framework.Definitions;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.Constants;
using VeriBranch.WebApplication.Exceptions;
using VeriBranch.WebApplication.FormatUtilities;
using VeriBranch.WebApplication.Helpers;
using VeriBranch.WebApplication.ObjectModel;
using Common.Definitions.UserManagement;
using CustomerManagement.MessageDefinitions.RetailAMLCheck;

namespace VeriBranch.WebApplication.UIProcess
{
    public abstract class VeriBranchTransactionStartBasePage<T, R> : VeriBranchTransactionCommonBasePage<T, R>, IStartPage, ICallbackEventHandler
        where T : RequestTransactionData, new()
        where R : ResponseTransactionData, new()
    {
        private const string SET_START_TRANSACTION_INFO = "VeriBranchStartBasePage.SetStartTransactionInfo";
        private const string CV_VBL_ON_SERVER_VALIDATE = "VeriBranchStartBasePage.CvVblOnServerValidate";

        private string _returnValue = string.Empty;

        protected string IALMsgResponseCode = string.Empty;

        protected virtual bool IsOralInstructionEnabled { get { return false; } }

        protected virtual string BeneficiaryName  { get { return string.Empty ; } }

        protected virtual string BeneficiaryID { get { return string.Empty; } }

        protected virtual decimal CVUApprovalAmount { get { return 0; } }

        protected bool IsOverDrawn
        {
            get 
            {
                if (string.IsNullOrEmpty(IALMsgResponseCode) && SafePageController.GetStateValue("IALMsgResponseCode") != null)
                {
                    IALMsgResponseCode = SafePageController.GetStateValue("IALMsgResponseCode").ToString();
                }

                return VpConfigurationManager.IsOverDrawnEnabled && !string.IsNullOrEmpty(IALMsgResponseCode) && VpConfigurationParameters.GetGenericParameter("IAL.InsufficientFundsMsgResponseCode").Split('|').Contains(IALMsgResponseCode);  
            }
        }

        //If false then allow user send change exchange rates and continue with local manager approval
        //If true then user has to send approval request to Refer Card.
        protected bool IsExchangeRateApprovalEnforced
        {
            get
            {
                return VpConfigurationManager.IsExchangeRateApprovalEnforced && IsExchangeRateChanged;
            }
        }

        protected bool IsValueDateApprovalEnforced
        {
            get
            {
                return VpConfigurationManager.IsValueDateApprovalEnforced && IsValueDateChanged;
            }
        }

        protected bool IsReferCardApproved
        {
            get
            {
                var isReferCardApproved = SafePageController.GetStateValue("ReferCardApproved");
                if (isReferCardApproved != null && isReferCardApproved.ToString().ToLower() == "true")
                    return true;

                return false;
            }
        }
        
     
        protected bool IsComplianceApproved
        {
            get
            {
                var isComplianceApproved = SafePageController.GetStateValue("ComplianceApproved");
                if (isComplianceApproved != null && isComplianceApproved.ToString().ToLower() == "true")
                    return true;

                return false;
            }
        }

        public abstract string TransactionName { get; }
        protected abstract TransactionOperation Operation { get; }

        protected abstract T GetRequestForConfirmation(T existingRequest);
        protected virtual T GetRequestForStart() { return new T(); }
        protected abstract T SetInitialRequest();

        //TODO - later, not urgent
        //resource keys for advice and receipt already moved to VpConstants (VpConstants.AdviceDocuments and VpConstants.ReceiptDocuments)
        protected virtual bool CheckDocsIfExists { get { return true; } }
        protected abstract string AdviceDocumentResourceKey { get; }
        protected abstract string ReceiptDocumentResourceKey { get; }

        protected virtual bool ExchangeRateChecked { get { return false; }}
        protected virtual bool OverDrawnChecked { get { return false; }}
        protected virtual bool ValueDateChecked { get { return false; }}
        protected virtual string ReferCardRemarks { get; set; }

        protected virtual string OnExternalCallReceived(VpResponseData ialResponseData)
        {
            var ifinancialData = (ialResponseData as IFinancialData);

            if (ifinancialData != null)
            {
                return OnExternalCallReceived(ifinancialData.FinancialData);
            }

            return string.Empty;
        }

        protected virtual string OnExternalCallReceived(VpFinancialData financialData)
        {
            return string.Empty;
        }

        protected virtual void CheckIfReferCardApprovalRequired()
        { }

        protected virtual void DisableAllControls() { }

        protected virtual void AfterComplianceApproval() {

            DisableAllControls();
        
        }

        protected virtual void AfterCallCenterApproval()
        {

            var ctlOralInstructionControl = OralInstructionControl;
            if (ctlOralInstructionControl != null)
            {
                ctlOralInstructionControl.Checked = true;

                if(RequestData != null && RequestData.LogFields != null)
                {
                    ctlOralInstructionControl.CallCenterReferenceNumber = RequestData.LogFields.Reserved7;
                    ctlOralInstructionControl.Remarks = RequestData.LogFields.Reserved8;
                }
            }



            DisableAllControls();

        }

        protected virtual void EnableBackButton(string backURL) { }

        private AskControlsDict AskControlValues
        {
            get
            {
                var dict = SafePageController.GetStateValue("AskControlValues") as AskControlsDict;
                if (dict == null)
                {
                    dict = new AskControlsDict();
                    SafePageController.SetStateValue("AskControlValues", dict);
                }

                return dict;
            }
        }

        private AskControlsDict AskOutputControlValues
        {
            get
            {
                var dict = SafePageController.GetStateValue("AskOutputControlValues") as AskControlsDict;
                if (dict == null)
                {
                    dict = new AskControlsDict();
                    SafePageController.SetStateValue("AskOutputControlValues", dict);
                }

                return dict;
            }
        }

        protected virtual bool IgnoreFinancialData {
            get { return false; }
        }

        protected virtual bool IgnoreCaseOverride
        {
            get { return false; }
        }

        protected bool IsCallCenterApproved
        {
            get
            {
                var isCallCenterApproved = SafePageController.GetStateValue("CallCenterApproved");
                if (isCallCenterApproved != null && isCallCenterApproved.ToString().ToLower() == "true")
                    return true;

                return false;
            }
        }

        public bool IsOralInstructionAvailableForCustomer
        {
            get
            {
                if (SafePageController.Customer != null &&
                    SafePageController.Customer.CustomerInfo != null &&
                    SafePageController.Customer.CustomerInfo.CustomerSpecialDetail != null)
                {
                    return SafePageController.Customer.CustomerInfo.CustomerSpecialDetail.PhoneInstFlag ||
                           SafePageController.Customer.CustomerInfo.CustomerSpecialDetail.FaxInstFlag ||
                           SafePageController.Customer.CustomerInfo.CustomerSpecialDetail.EmailInstFlag;
                }

                return false;
            }

        }
        

        public bool IsAskCall
        {
            get
            {
                if (SafePageController.GetStateValue("IsAskCall") == null)
                    return false;

                return bool.Parse(SafePageController.GetStateValue("IsAskCall").ToString());
            }
            private set
            {
                SafePageController.SetStateValue("IsAskCall", value);
            }
        }

        public bool IsAskPageEnabled
        {
            get { return (AskControls != null && AskControls.Count > 0); }
        }

        public bool IsAskAlreadyCalled
        {
            get
            {
                if (SafePageController.GetStateValue("IsAskAlreadyCalled") == null)
                    return false;

                return bool.Parse(SafePageController.GetStateValue("IsAskAlreadyCalled").ToString());
            }
            set
            {
                SafePageController.SetStateValue("IsAskAlreadyCalled", value);
            }
        }

        public override bool ShowScanPage
        {
            get
            {
                return false;   //do not show scan page on the start page.
            }
        }

        void CheckDocsExistence()
        {
            if (CheckDocsIfExists)
            {
                bool adviceOK = true;
                bool receiptOK = true;
                if (!string.IsNullOrEmpty(AdviceDocumentResourceKey))
                {
                    var adviceDocTemplateName = GetGlobalResource(VpConstants.GlobalResource.OpenXmlReports, AdviceDocumentResourceKey);
                    var adviceDocTemplateFileName = Server.MapPath(VpUserControlConstants.VpNavigationControlConstant.OPEN_XML_REPORTS_FOLDER + adviceDocTemplateName);
                    adviceOK = File.Exists(adviceDocTemplateFileName);
                }

                if (!string.IsNullOrEmpty(ReceiptDocumentResourceKey))
                {
                    var receiptDocTemplateName = GetGlobalResource(VpConstants.GlobalResource.OpenXmlReports, ReceiptDocumentResourceKey);
                    var receiptDocTemplateFileName = Server.MapPath(VpUserControlConstants.VpNavigationControlConstant.OPEN_XML_REPORTS_FOLDER + receiptDocTemplateName);
                    receiptOK = File.Exists(receiptDocTemplateFileName);
                }

                var msgs = new List<string>();

                if (!adviceOK)
                    msgs.Add(GetGlobalResource("AdviceDocumentTemplateMissing"));

                if (!receiptOK)
                    msgs.Add(GetGlobalResource("ReceiptDocumentTemplateMissing"));

                if (msgs.Count > 0)
                {
                    SetWarningDisplay(string.Join("<BR/>", msgs));
                }
            }
        }

        protected override void SetTransactionInfo()
        {
            InitializeRequestData();
            RequestData.TransactionActions = null;

            var referenceNumber = (string)SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
            var entitlements = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.Entitlement) as List<VPEntitlement>;
            var offersId = PageType + VpPageControllerConstants.Transaction.Offers.ToString();
            var offers = SafePageController.GetStateValue(offersId) as List<VPDynamicOffer>;

            VPApprovalActionInformation[] approvalFlags = null;

            if (string.IsNullOrEmpty(referenceNumber))
            {
                var requestForStart = GetRequestForStart();
                ExtendedResponseTransactionData<R> extendedResponse = SecureExecuteOperation(requestForStart, OperationStageEnumeration.Start);
                if (extendedResponse == null || extendedResponse.VblResponse == null)
                    throw new VPSystemException(SET_START_TRANSACTION_INFO, VPExceptionConstants.TRANSACTION_RESPONSE_NULL);

                var response = extendedResponse.VblResponse;


                if (response.Footer != null)
                {
                    if (response.Footer.Result != null && response.Footer.Result.Error != null && !string.IsNullOrEmpty(response.Footer.Result.Error.Code))
                    {
                        if (!response.IsSuccess)
                        {
                            AlertModalButtonArray(GetGlobalResource("AlertModalTitleError"), string.Format("{0}: {1}", response.Footer.Result.Error.Code, response.Footer.Result.Error.Description), "[{ text: getOKButtonText(), click: function () { $(this).dialog('close'); redirectToTransactionPad(); } }]");
                            return;
                        }

                        if (response.Footer.Result.PhoneVerification != null)
                        {
                            if (response.Footer.Result.PhoneVerification.ManualVerificationRequired != ManuelVerificationType.NotVerified)
                            {
                                //Javascript trigger for Manual Verification
                                var jsTriggerFormatForManualVerification = ServicesHelper.DoConfigurationParameterInquiry("TriggerScriptFormatForManualVerification");
                                if (!string.IsNullOrEmpty(jsTriggerFormatForManualVerification))
                                {
                                    var jsTriggerScriptManualVerification = string.Format(jsTriggerFormatForManualVerification, ((int)response.Footer.Result.PhoneVerification.ManualVerificationRequired), requestForStart.Header.ExternalTokenKey);
                                    Page.ClientScript.RegisterStartupScript(this.GetType(), "TriggerScriptManualVerification", jsTriggerScriptManualVerification, true);
                                }
                            }

                            if (response.Footer.Result.PhoneVerification.TPinRequired)
                            {
                                //Javascript trigger for TPin Validation
                                var jsTriggerFormatForTPinValidation = ServicesHelper.DoConfigurationParameterInquiry("TriggerScriptFormatForTPinValidation");
                                if (!string.IsNullOrEmpty(jsTriggerFormatForTPinValidation))
                                {
                                    var jsTriggerScriptTPinValidation = string.Format(jsTriggerFormatForTPinValidation, ((int)response.Footer.Result.PhoneVerification.ManualVerificationRequired), requestForStart.Header.ExternalTokenKey);
                                    Page.ClientScript.RegisterStartupScript(this.GetType(), "TriggerScriptTPinValidaton", jsTriggerScriptTPinValidation, true);
                                }
                            }
                        }
                    }

                    if (response.Footer.VPDynamicOffers != null)
                    {
                        offers = response.Footer.VPDynamicOffers;
                        SafePageController.SetStateValue(offersId, response.Footer.VPDynamicOffers);
                    }

                    if (response.Footer.VPEntitlements != null)
                    {
                        entitlements = response.Footer.VPEntitlements;
                        SafePageController.SetStateValue(VpPageControllerConstants.Transaction.Entitlement, response.Footer.VPEntitlements);
                    }

                    if (response.Footer.TransactionInformation != null)
                    {
                        TransactionInformation = response.Footer.TransactionInformation;
                        SafePageController.SignatureVerificationFlagActual = response.Footer.TransactionInformation.TransactionSettings.IsSignatureVerificationRequired;
                    }

                    if (response.Footer.TransactionInformation != null && response.Footer.TransactionInformation.ApprovalActionInformationList != null && response.Footer.TransactionInformation.ApprovalActionInformationList.Length > 0)
                    {
                        approvalFlags = response.Footer.TransactionInformation.ApprovalActionInformationList;
                    }

                    if (response.Footer.VPDocumentPackages != null)
                    {
                        //LogMissingDocumentCodes(response.Footer.VPDocumentPackages);
                        SafePageController.SetStateValue(VpPageControllerConstants.Transaction.DocumentPackage, response.Footer.VPDocumentPackages);
                    }

                    if (response.Footer.DocumentTypes != null)
                    {
                        response.Footer.DocumentTypes.Sort((type1, type2) => type1.Name.CompareTo(type2.Name));
                        SafePageController.SetStateValue(VpPageControllerConstants.Transaction.DocumentTypes, response.Footer.DocumentTypes);
                    }

                    if (response.Footer.VPCheckListQuestion != null)
                    {
                        SafePageController.SetStateValue(VpPageControllerConstants.Transaction.Checklist, response.Footer.VPCheckListQuestion);
                    }

                    if (!string.IsNullOrEmpty(response.Footer.AlertMessage))
                    {

                        var alertTitle = GetGlobalResource("AlertModalTitle");
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "Alert", "alertModal('" + alertTitle + "','" + response.Footer.AlertMessage + "', getOKButtonText(),0);", true);
            
                    }
                }

                referenceNumber = response.ReferenceNumber;

                SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber, referenceNumber);

                PrePopulateControls();


            }

            if (!string.IsNullOrEmpty(referenceNumber))
                SetTransactionReferenceNumber(referenceNumber);

            SetOfferBox(offers);

            if (entitlements != null)
                SetEntitlements(entitlements);

            if (approvalFlags != null)
            {
                var dictionary = GetApprovalFlagsReversedDictionary(approvalFlags);
                if (dictionary != null)
                    ApprovalFlagsDictionary = dictionary;
            }

            InitialRequest = SetInitialRequest();
            CheckDocsExistence();
            
            //For approval department 'Approved' transactiopns.
            if (IsReferCardApproved || IsComplianceApproved || IsCallCenterApproved)
            {

                if (IsReferCardApproved)
                {
                    DisableAllControls();
                }

                if (IsCallCenterApproved)
                {
                    AfterCallCenterApproval();
                }

                if (IsComplianceApproved)
                {
                    AfterComplianceApproval();
                }
                
                var StartPageUrlReferrer = SafePageController.GetStateValue("StartPageUrlReferrer");
                if (StartPageUrlReferrer == null || string.IsNullOrEmpty(StartPageUrlReferrer.ToString()))
                {
                    var backURL = string.Format("{0}{1}", Request.UrlReferrer.AbsolutePath, Request.UrlReferrer.Query);
                    EnableBackButton(backURL);
                    SafePageController.SetStateValue("StartPageUrlReferrer", backURL);
                }
                else
                {   
                    EnableBackButton(StartPageUrlReferrer.ToString());
                }

                //showRejectApprovalRequestButton
                if (!ClientScript.IsStartupScriptRegistered(this.GetType(), "showRejectApprovalRequestButton"))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "showRejectApprovalRequestButton", "showRejectApprovalRequestButton();", true);
                }
            }

            //If Start page is reffered from confirm page
            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.AbsolutePath) && Request.UrlReferrer.AbsolutePath.EndsWith("Confirm.aspx"))
            {
                CheckIfReferCardControlRequired();
            }



        }

        private void LogMissingDocumentCodes(IEnumerable<VPDocumentPackage> vpDocumentPackages)
        {
            try
            {
                foreach (VPDocumentPackage package in vpDocumentPackages)
                {
                    foreach (VPDocumentPackageContents packageContents in package.DocumentPackageContents)
                    {
                        if (packageContents.DocumentCategory.DocumentType == null || packageContents.DocumentCategory.DocumentType.Length == 0)
                        {
                            LogManager.LogException("LogMissingDocumentCodes", string.Format("No document codes under {0} [{1}]", packageContents.DocumentCategory.Name, packageContents.DocumentCategory.Code));
                        }
                    }
                }
            }
            catch (Exception)
            {
                //continue
            }
        }

        private void PrePopulateControls()
        {
            foreach (var control in PrepopulateControls)
            {
                if (control != null && !string.IsNullOrEmpty(control.PrePopulatedValueName) && !string.IsNullOrEmpty(SafePageController.QueryStrings[control.PrePopulatedValueName]))
                {
                    control.PopulateValue(SafePageController.QueryStrings[control.PrePopulatedValueName]);
                }
            }
        }

        protected sealed override void LoadTransactionProcessSteps()
        {
            if (File.Exists(Server.MapPath(VpConstants.PageControllerConstants.TRANSACTION_PROCESS_STEPS_FILE)))
            {
                var xDoc = XDocument.Load(Server.MapPath(VpConstants.PageControllerConstants.TRANSACTION_PROCESS_STEPS_FILE));

                var steps = xDoc.Descendants("Transaction").Where(e => e.Attribute("Name").Value == TransactionName).Descendants("Steps").Descendants("Step").ToList();

                SafePageController.SetStateValue(VpApplicationControllerConstants.Transactions.TxnProcessSteps, steps);
            }
        }

        private ExtendedResponseTransactionData<R> SecureExecuteOperation(T request, OperationStageEnumeration stage)
        {
            SafePageController.SetStateValue(VpPageControllerConstants.Transaction.Operation, Operation);
            return SecureExecute.Execute((t, s) => Operation.Invoke(t, s), request, stage);
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

        protected override void EnableVblValidator()
        {
            if (PageType == PageType.Start)
            {
                NavigationControl.AskButtonClicked += SendAskCall;
                NavigationControl.NextButtonClicked += SaveTransactionRequest;
                NavigationControl.ReferCardButtonClicked += SendReferCardCall;
                NavigationControl.RejectApprovalRequestButtonClicked += RejectApprovalRequest;
                NavigationControl.ComplianceButtonClicked += SendComplianceCall;
                NavigationControl.CallCenterButtonClicked += SendCallCenterApprovalCall;

                if (FowardToReferCardControl != null)
                {
                    FowardToReferCardControl.ForwardToReferCardClicked += ForwardToReferCardClicked;
                }

                if (OralInstructionControl != null)
                {
                    OralInstructionControl.OralInstructionClicked += OralInstructionClicked;
                }

            }
        }

        private bool ProcessResponseInternal(ExtendedResponseTransactionData<R> extendedResponse)
        {
            bool isValid = false;

            if (extendedResponse != null && extendedResponse.VblResponse != null)
            {
                var response = extendedResponse.VblResponse;

                isValid = true;

                var requiresCaseOpening = (!TransactionInformation.IsStpTransaction)
                                          || extendedResponse.VblResponse.Footer.RuleVerifications.RequiresApproval
                                          || extendedResponse.VblResponse.Footer.RuleVerifications.HostRequiresApproval
                                          || extendedResponse.VblResponse.Footer.RuleVerifications.LimitApprovalRequired;

                SafePageController.ResetState(VpPageControllerConstants.Transaction.CrmCaseOpenReason);
                SafePageController.ResetState(VpPageControllerConstants.Transaction.OpensCrmCase);

                //Below code need to refactored to allow multiple approval actions 
                if (extendedResponse.VblResponse.Footer.RuleVerifications.HostRequiresApproval && !IgnoreCaseOverride)
                {
                    var approvalFlagList = ApprovalFlagsDictionary["HostRequiresApproval"];
                    if (approvalFlagList == null || approvalFlagList.Count == 0)
                        throw new VPSystemException(this.ToString(), "HostRequiresApproval approval category is not defined");

                    var category = approvalFlagList[0].Category;
                    var approvalAction = new VPApprovalAction() { ActionCode = approvalFlagList[0].Code, Remarks = extendedResponse.VblResponse.Footer.RuleVerifications.Reason };
                    UpdateApprovalActions(RequestData, approvalAction, category);
                }

                if (extendedResponse.VblResponse.Footer.RuleVerifications.LimitApprovalRequired && !IgnoreCaseOverride)
                {
                    var approvalFlagList = ApprovalFlagsDictionary["LimitApproval"];
                    if (approvalFlagList == null || approvalFlagList.Count == 0)
                        throw new VPSystemException(this.ToString(), "Limit approval category is not defined.");

                    var category = approvalFlagList[0].Category;
                    var approvalAction = new VPApprovalAction() { ActionCode = approvalFlagList[0].Code, Remarks = extendedResponse.VblResponse.Footer.RuleVerifications.Reason };
                    UpdateApprovalActions(RequestData, approvalAction, category);
                }

                if (extendedResponse.VblResponse.Footer.RuleVerifications.OralInstructionApproval && !IgnoreCaseOverride)
                {
                    var approvalFlagList = ApprovalFlagsDictionary["OralInstructionApproval"];
                    if (approvalFlagList == null || approvalFlagList.Count == 0)
                        throw new VPSystemException(this.ToString(), "Oral Instruction approval category is not defined.");

                    var category = approvalFlagList[0].Category;
                    var approvalAction = new VPApprovalAction() { ActionCode = approvalFlagList[0].Code, Remarks = extendedResponse.VblResponse.Footer.RuleVerifications.Reason };
                    UpdateApprovalActions(RequestData, approvalAction, category);
                }

                if (requiresCaseOpening && !IgnoreCaseOverride)
                {
                    var message = new List<string>();

                    if (extendedResponse.VblResponse.Footer.RuleVerifications.RequiresApproval)
                        if (!string.IsNullOrEmpty(extendedResponse.VblResponse.Footer.RuleVerifications.Reason))
                        {
                            message.AddRange(extendedResponse.VblResponse.Footer.RuleVerifications.Reason.Split(new string[] { "<BR/>", "<br/>" }, StringSplitOptions.RemoveEmptyEntries));
                        }

                    if (message.Count == 0)
                        //TODO - message to come from globalization resources
                        message.Add(GetGlobalResource("TransactionRequiresApprovalMsg"));

                    SafePageController.SetStateValue(VpPageControllerConstants.Transaction.OpensCrmCase, requiresCaseOpening);
                    SafePageController.SetStateValue(VpPageControllerConstants.Transaction.CrmCaseOpenReason, message);
                }

                if (!response.IsSuccess)
                {
                    if (!string.IsNullOrEmpty(SafePageController.ErrorMessage))
                    {
                        SetErrorDisplay(SafePageController.ErrorMessage);
                        isValid = false;
                    }
                    else if (!string.IsNullOrEmpty(SafePageController.WarningMessage))
                    {
                        SafePageController.SetStateValue(VpPageControllerConstants.UI.WarningMessage, SafePageController.WarningMessage);
                    }
                }

                if (isValid)
                {
                    SafePageController.SetStateValue("TransactionName", TransactionName);
                    SetConfirmTransactionInfo(response);
                    ProcessResponse(extendedResponse);
                }
            }

            if (extendedResponse != null && extendedResponse.IalResponse != null && extendedResponse.IalResponse.Header != null)
            {
                this.IALMsgResponseCode = extendedResponse.IalResponse.Header.MsgResponseCode;
                SafePageController.SetStateValue("IALMsgResponseCode", extendedResponse.IalResponse.Header.MsgResponseCode);
            }
            
            return isValid;
        }

        protected virtual void ProcessResponse(ExtendedResponseTransactionData<R> extendedResponse)
        {
        }


        protected virtual void OnAskCallCompleted()
        {
        }

        protected virtual void OnAMLCallCompleted()
        { 

        
        }
        protected virtual string AfterAskCall(ExtendedResponseTransactionData<R> extendedResponse)
        {
            string returnMessage = string.Empty;
            
            //Since Refer card approval is the last step in approval work flow steps.
            //Therefore, if transaction has passed refer card approval step then no need to check compliance and CVU approval steps. 
            if (IsReferCardApproved)
                return returnMessage;

            //Check If Compliance department approval is required
            returnMessage = CheckIfComplianceApprovalRequired();

            //Check If CallCenter Approval Required
            CheckIfCallCenterApprovalRequired();

            if (!string.IsNullOrEmpty(returnMessage))
                return returnMessage;
            
            //Check If CVU approval is required
            //returnMessage = CheckIfCVUApprovalIsRequired();

             return returnMessage;
        }

        private void CheckIfCallCenterApprovalRequired()
        {
            //If Transaction is already passed CallCenter then return
            if (IsCallCenterApproved)
                return ;

            if (IsOralInstructionEnabled && IsOralInstructionAvailableForCustomer )
            {     
                var message = GetGlobalResource("OralInstuctionAlert");
                
                //SafePageController.SetStateValue(VpPageControllerConstants.UI.AlertMessage, message);
                var alertTitle = GetGlobalResource("AlertModalTitle");
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Alert", "alertModal('" + alertTitle + "','" + message.Replace("'", "") + "', getOKButtonText(),0);", true);
                
            }

        }

        private string CheckIfComplianceApprovalRequired()
        {
            string returnMessage = string.Empty;

            //If Transaction is already passed compliance then return
            if (IsComplianceApproved)
                return returnMessage;

            //call ANTS
            if (VpConfigurationManager.IsANTSValidationEnabled && (!string.IsNullOrEmpty(BeneficiaryName) || !string.IsNullOrEmpty(BeneficiaryID)))
            {
                //Prepare AMLCheckInquiryRequest
                var userHelper = new UserHelper();
                userHelper.LoginToken = RequestData.Header.LoginTokenKey;

                var AMLCheckInquiryRequest = new VpRetailAMLCheckInquiryRequest()
                {
                    IALRetailAMLCheckRequest = new VpRetailAMLCheckRequest(),

                };

                if (!string.IsNullOrEmpty(BeneficiaryName) && !string.IsNullOrEmpty(BeneficiaryID))
                {
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.SearchType = IAL_AML_SearchType.NAAN.ToString();
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.SearchKey = BeneficiaryID;
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.FindName = BeneficiaryName;
                }
                else if (!string.IsNullOrEmpty(BeneficiaryName))
                {
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.SearchType = IAL_AML_SearchType.ANON.ToString();
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.FindName = BeneficiaryName;
                }
                else if (!string.IsNullOrEmpty(BeneficiaryID))
                {
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.SearchType = IAL_AML_SearchType.NATL.ToString();
                    AMLCheckInquiryRequest.IALRetailAMLCheckRequest.SearchKey = BeneficiaryID;
                }


                var response = userHelper.DoAMLCheckInquiry(AMLCheckInquiryRequest);

                if (response != null && response.IsSuccess && response.IALRetailAMLCheckResponse != null)
                {
                    if (response.IALRetailAMLCheckResponse.HitValue >= VpConfigurationManager.ANTSThresholdValue)
                    {
                        var message = GetGlobalResource("ComplianceThresholdMessage");
                        returnMessage = string.Format("javascript:showComplianceButton();hideOverlay();alertModal(getAlertTitleText(), '{0}', getOKButtonText());", message);

                            //set AML fields to pass for Compliance request
                            SafePageController.SetStateValue("AMLScore", response.IALRetailAMLCheckResponse.HitValue);
                            SafePageController.SetStateValue("AMLBeneficiaryName", BeneficiaryName);
                            SafePageController.SetStateValue("AMLBeneficiaryID", BeneficiaryID);

                            var serializedData = Veripark.CustomSerializer.VpSerializer.Serialize(response.IALRetailAMLCheckResponse);
                            var serializedString = serializedData.GetSerializedObject();

                            SafePageController.SetStateValue("AMLInquiryResponse", serializedString);
                                                        
                            OnAMLCallCompleted();
                        }
                    else if (!string.IsNullOrEmpty(BeneficiaryID) && response.IALRetailAMLCheckResponse.SearchedItems != null && response.IALRetailAMLCheckResponse.SearchedItems.Count > 0)
                    {
                        var found = false;
                        var hitValue = 0;
                        //string searchedBeneficiaryName = string.Empty;


                        foreach (var item in response.IALRetailAMLCheckResponse.SearchedItems)
                        {
                            if (item.HitValue >= VpConfigurationManager.ANTSThresholdValue)
                            {
                                found = true;
                                hitValue = item.HitValue;
                                //searchedBeneficiaryName = string.IsNullOrEmpty(item.NameEN) ? item.Name_LS : item.NameEN;
                                break;
                            }


                        }

                        if (found)
                        {
                            var message = GetGlobalResource("ComplianceThresholdMessage");
                            returnMessage = string.Format("javascript:showComplianceButton();hideOverlay();alertModal(getAlertTitleText(), '{0}', getOKButtonText());", message);

                            //set AML fields to pass for Compliance request
                            SafePageController.SetStateValue("AMLScore", hitValue);
                            SafePageController.SetStateValue("AMLBeneficiaryID", BeneficiaryID);

                            OnAMLCallCompleted();
                        }
                    }

                    //If AML test passes
                    else
                    {
                        //set AML fields to pass for Compliance request
                        SafePageController.SetStateValue("AMLScore", null);
                        SafePageController.SetStateValue("AMLBeneficiaryName", null);
                        SafePageController.SetStateValue("AMLBeneficiaryID", null);

                        OnAMLCallCompleted();

                    }
                }
                else
                {
                    var message = "Failed to receive ANTS response from host.";

                    if (response != null && response.Footer != null && response.Footer.Result != null && response.Footer.Result.Error != null && !string.IsNullOrEmpty(response.Footer.Result.Error.Description))
                    {
                        message = response.Footer.Result.Error.Description;
                    }

                    returnMessage = string.Format("javascript:onAskFieldChanged();hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", message);

                }


            }

            return returnMessage;
                    }
              
        //private string CheckIfCVUApprovalIsRequired()
        //{
        //    string returnMessage = string.Empty;

        //    //If Transaction is already passed CVU approval then return
        //    if (IsCVUApproved)
        //        return returnMessage;

        //    //check if CVU enabled
        //    if (VpConfigurationManager.IsCVUApprovalEnabled && (CVUApprovalAmount >= VpConfigurationManager.CVUThresholdValue))
        //    {
        //        var message = GetGlobalResource("CVUThresholdMessage");
        //        returnMessage = string.Format("javascript:showCVUButton();hideOverlay();alertModal(getAlertTitleText(), '{0}', getOKButtonText());", message);

        //    }

        //    return returnMessage;
        //}

        protected virtual void OnReferCardCallCompleted()
        {
            DisableAllControls();
        }

        protected virtual void OnComplianceCallCompleted()
        {
            DisableAllControls();
        }

        protected virtual void OnCallCenterApprovalRequestCompleted()
        {
            DisableAllControls();
        }

        protected virtual void OnRejectApprovalRequestCompleted()
        {
            DisableAllControls();
        }
        

        private ExtendedResponseTransactionData<R> SendTransactionRequestForConfirmation()
        {
            CheckCustomerAndAgentForNulls();

            //todo: Noman debug and see
            //InitialRequest = SetInitialRequest();

            //T existingRequest = new T();
            //if (isAsk)
            //    existingRequest = InitializeRequestData();
            T existingRequest = InitializeRequestData();
            if (!string.IsNullOrEmpty(SafePageController.QueryStrings["AccountNumber"]))
                SafePageController.TransactionHeader.User.LoggedInAccountNumber = SafePageController.QueryStrings["AccountNumber"];
            var request = GetRequestForConfirmation(existingRequest);
            //request.IsAsk = !isAsk;

            var refNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
            if (refNumber != null)
                request.ReferenceNumber = refNumber.ToString();

            request.TransactionActions = GetApprovalActions(request);

            //loop through the controls to get the approval actions
            foreach (var approvalControl in ApprovalControls)
                approvalControl.SetSelectedApprovalFlag();

            //if (isAsk)
            //    SafePageController.RequestData = request;
            SafePageController.RequestData = request;

            if (request is IIALRequest)
            {
                var ialRequest = (request as IIALRequest).GetCommonIALRequest();

                if (ialRequest is IFinancialData)
                {
                    var iFinancialDataRequest = ialRequest as IFinancialData;
                    if (iFinancialDataRequest.FinancialData != null)
                    {
                        //if (request.IsAsk)
                        //{
                        //    //Do not send FinancialData in ask1
                        //    iFinancialDataRequest.FinancialData = null;
                        //}
                        //else
                        //{
                        //put total charges in customLgEntry for reporting purpose.
                        if (iFinancialDataRequest.FinancialData.ChargesDetail != null)
                        {
                            iFinancialDataRequest.FinancialData.ExchangeRatesLCY = ExchangeRatesLCY;
                            if (request.CustomLogEntryList == null)
                                request.CustomLogEntryList = new VPLogEntry[2];

                            var list = request.CustomLogEntryList.ToList();
                            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TotalCharges.ToString(), Value = iFinancialDataRequest.FinancialData.ChargesDetail.Sum(c => c.ChargeAmountLCY) + iFinancialDataRequest.FinancialData.AmountVAT });
                            if (iFinancialDataRequest.FinancialData.ExchangeRatesLCY != 0)
                            {
                                list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TotalChargesInSourceCurrency.ToString(), Value = ReportHelper.GetTotalCharges(iFinancialDataRequest.FinancialData) });

                            }
                            request.CustomLogEntryList = list.ToArray();
                        }
                        //}
                    }
                }
            }

            if (IsCallCenterApproved)
            {
                SetCustomLogEntry(request, "CallCenterApproved", true.ToString());
            }

            if (IsReferCardApproved)
            {
                SetCustomLogEntry(request, "ReferCardApproved", true.ToString());
            }

            var extendedResponse = SecureExecuteOperation(request as T, OperationStageEnumeration.Confirm);

            if ((extendedResponse == null || extendedResponse.VblResponse == null) && PageType == PageType.Start)
                throw new VPSystemException(CV_VBL_ON_SERVER_VALIDATE, VPExceptionConstants.RESPONSE_NULL);

            return extendedResponse;
        }

        private ExtendedResponseTransactionData<R> SendApprovalRequestToReferCard()
        {
            CheckCustomerAndAgentForNulls();

          T existingRequest = InitializeRequestData();
            if (!string.IsNullOrEmpty(SafePageController.QueryStrings["AccountNumber"]))
                SafePageController.TransactionHeader.User.LoggedInAccountNumber = SafePageController.QueryStrings["AccountNumber"];
            var request = GetRequestForConfirmation(existingRequest);

            //set refer card approval flags
            request.ReferCardApprovalFlag = new VeriBranch.Framework.Definitions.ReferCardApprovalFlags();
            request.ReferCardApprovalFlag.RequestExchangeRate = ExchangeRateChecked;
            request.ReferCardApprovalFlag.RequestOverDrawn = OverDrawnChecked;

            //For LOCAL_CHEQUE_CLEARING ValueDateChecked represents RequestClearingDate
            if (TransactionName == TransactionNameContants.LOCAL_CHEQUE_CLEARING)
            {
                request.ReferCardApprovalFlag.RequestClearingDate = ValueDateChecked;
            }
            else
            {
                request.ReferCardApprovalFlag.RequestValueDateChange = ValueDateChecked;
            }

            request.ReferCardApprovalFlag.Remarks = ReferCardRemarks;

            //ApprovalDepartment
            request.ApprovalDepartment = ApprovalDepartmentEnum.ReferCard;

            //Set ComplianceApproved in customm log entry
            if (IsComplianceApproved)
            {
                SetCustomLogEntry(request, "ComplianceApproved", true.ToString());
            }

            //Set CallCenterApproved in customm log entry
            if (IsCallCenterApproved)
            {
                SetCustomLogEntry(request, "CallCenterApproved", true.ToString());
            }

            
            var refNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
            if (refNumber != null)
                request.ReferenceNumber = refNumber.ToString();

            //approval actions are not required for refer card
            //request.TransactionActions = GetApprovalActions(request);

            //loop through the controls to get the approval actions
            //foreach (var approvalControl in ApprovalControls)
            //    approvalControl.SetSelectedApprovalFlag();

            //if (isAsk)
            //    SafePageController.RequestData = request;
            SafePageController.RequestData = request;

            if (request is IIALRequest)
            {
                var ialRequest = (request as IIALRequest).GetCommonIALRequest();

                if (ialRequest is IFinancialData)
                {
                    var iFinancialDataRequest = ialRequest as IFinancialData;
                    if (iFinancialDataRequest.FinancialData != null)
                    {
                        //if (request.IsAsk)
                        //{
                        //    //Do not send FinancialData in ask1
                        //    iFinancialDataRequest.FinancialData = null;
                        //}
                        //else
                        //{
                        //put total charges in customLgEntry for reporting purpose.
                        if (iFinancialDataRequest.FinancialData.ChargesDetail != null)
                        {
                            iFinancialDataRequest.FinancialData.ExchangeRatesLCY = ExchangeRatesLCY;
                            if (request.CustomLogEntryList == null)
                                request.CustomLogEntryList = new VPLogEntry[2];

                            var list = request.CustomLogEntryList.ToList();
                            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TotalCharges.ToString(), Value = iFinancialDataRequest.FinancialData.ChargesDetail.Sum(c => c.ChargeAmountLCY) + iFinancialDataRequest.FinancialData.AmountVAT });
                            if (iFinancialDataRequest.FinancialData.ExchangeRatesLCY != 0)
                            {
                                list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TotalChargesInSourceCurrency.ToString(), Value = ReportHelper.GetTotalCharges(iFinancialDataRequest.FinancialData) });

                            }
                            request.CustomLogEntryList = list.ToArray();
                        }
                        //}
                    }
                }
            }

            var extendedResponse = SecureExecuteOperation(request as T, OperationStageEnumeration.RequestForApproval);

            if ((extendedResponse == null || extendedResponse.VblResponse == null) && PageType == PageType.Start)
                throw new VPSystemException(CV_VBL_ON_SERVER_VALIDATE, VPExceptionConstants.RESPONSE_NULL);

            return extendedResponse;
        }

        private ExtendedResponseTransactionData<R> SendRejectApprovalRequest()
        {
            CheckCustomerAndAgentForNulls();

            T existingRequest = InitializeRequestData();
            if (!string.IsNullOrEmpty(SafePageController.QueryStrings["AccountNumber"]))
                SafePageController.TransactionHeader.User.LoggedInAccountNumber = SafePageController.QueryStrings["AccountNumber"];
            var request = GetRequestForConfirmation(existingRequest);

      
            var refNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
            if (refNumber != null)
                request.ReferenceNumber = refNumber.ToString();

            SafePageController.RequestData = request;
            request.ApprovalDepartment = ApprovalDepartmentEnum.ReferCard;
            
            var extendedResponse = SecureExecuteOperation(request as T, OperationStageEnumeration.RejectApprovalRequest);

            if ((extendedResponse == null || extendedResponse.VblResponse == null) && PageType == PageType.Start)
                throw new VPSystemException(CV_VBL_ON_SERVER_VALIDATE, VPExceptionConstants.RESPONSE_NULL);

            return extendedResponse;
        }

        private ExtendedResponseTransactionData<R> SendComplianceRequest()
        {
            CheckCustomerAndAgentForNulls();

            T existingRequest = InitializeRequestData();
            if (!string.IsNullOrEmpty(SafePageController.QueryStrings["AccountNumber"]))
                SafePageController.TransactionHeader.User.LoggedInAccountNumber = SafePageController.QueryStrings["AccountNumber"];
            var request = GetRequestForConfirmation(existingRequest);


            var refNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
            if (refNumber != null)
                request.ReferenceNumber = refNumber.ToString();

            SafePageController.RequestData = request;
            request.ApprovalDepartment = ApprovalDepartmentEnum.Compliance;

            //set Compliance fields in CustomLogEntryList
            if (request.CustomLogEntryList == null)
                request.CustomLogEntryList = new VPLogEntry[3];

            var list = request.CustomLogEntryList.ToList();
            var AMLScore = SafePageController.GetStateValue("AMLScore") == null ? 0 : Convert.ToInt32(SafePageController.GetStateValue("AMLScore"));
            var AMLBeneficiaryName = SafePageController.GetStateValue("AMLBeneficiaryName") == null ? string.Empty : SafePageController.GetStateValue("AMLBeneficiaryName").ToString();
            var AMLBeneficiaryID = SafePageController.GetStateValue("AMLBeneficiaryID") == null ? string.Empty : SafePageController.GetStateValue("AMLBeneficiaryID").ToString();
            var AMLInquiryResponse = SafePageController.GetStateValue("AMLInquiryResponse") == null ? string.Empty : SafePageController.GetStateValue("AMLInquiryResponse").ToString();


            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.AMLScore.ToString(), Value = AMLScore });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.AMLBeneficiaryName.ToString(), Value = AMLBeneficiaryName });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.AMLBeneficiaryID.ToString(), Value = AMLBeneficiaryID });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.AMLInquiryResponse.ToString(), Value = AMLInquiryResponse });
            

            request.CustomLogEntryList = list.ToArray();

            var extendedResponse = SecureExecuteOperation(request as T, OperationStageEnumeration.RequestForApproval);

            if ((extendedResponse == null || extendedResponse.VblResponse == null) && PageType == PageType.Start)
                throw new VPSystemException(CV_VBL_ON_SERVER_VALIDATE, VPExceptionConstants.RESPONSE_NULL);

            return extendedResponse;
        }

        private ExtendedResponseTransactionData<R> SendCallCenterApprovalRequest()
        {
            CheckCustomerAndAgentForNulls();

            T existingRequest = InitializeRequestData();
            if (!string.IsNullOrEmpty(SafePageController.QueryStrings["AccountNumber"]))
                SafePageController.TransactionHeader.User.LoggedInAccountNumber = SafePageController.QueryStrings["AccountNumber"];
            var request = GetRequestForConfirmation(existingRequest);
            
            var refNumber = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);
            if (refNumber != null)
                request.ReferenceNumber = refNumber.ToString();

            //Set ComplianceApproved in customm log entry
            if (IsComplianceApproved)
            {
                SetCustomLogEntry(request, "ComplianceApproved", true.ToString());
            }

            //Set ReferCardApproved in customm log entry
            if (IsReferCardApproved)
            {
                SetCustomLogEntry(request, "ReferCardApproved", true.ToString());
            }

            SafePageController.RequestData = request;
            request.ApprovalDepartment = ApprovalDepartmentEnum.CallCenter;
            
            var extendedResponse = SecureExecuteOperation(request as T, OperationStageEnumeration.RequestForApproval);

            if ((extendedResponse == null || extendedResponse.VblResponse == null) && PageType == PageType.Start)
                throw new VPSystemException(CV_VBL_ON_SERVER_VALIDATE, VPExceptionConstants.RESPONSE_NULL);

            return extendedResponse;
        }

        private void SetCustomLogEntry(T request, string key, string value)
        {
            //If CustomLogEntryList is null create new entry
           if (request.CustomLogEntryList == null)
               request.CustomLogEntryList = new VPLogEntry[2];

           //If Key does not exist in CustomLogEntryList
           if (request.CustomLogEntryList != null &&
               request.CustomLogEntryList.FirstOrDefault(e => e.Key == key) == null)
           {
               var list = request.CustomLogEntryList.ToList();
               list.Add(new VPLogEntry { Key = key, Value = value });
           
               request.CustomLogEntryList = list.ToArray();
           }

        }

        void CheckCustomerAndAgentForNulls()
        {
            var msg = string.Empty;
            if (SafePageController == null)
            {
                msg = "SafePageController is NULL";
            }
            else if (SafePageController.Performer == null)
            {
                msg = "SafePageController.Performer is NULL";
            }
            else if (SafePageController.Performer.User == null)
            {
                msg = "SafePageController.Performer.User is NULL";
            }
            else if (SafePageController.Performer.Agent == null)
            {
                msg = "SafePageController.Performer.Agent is NULL";
            }
            else if (SafePageController.Customer == null)
            {
                msg = "SafePageController.Customer is NULL";
            }
            else if (SafePageController.Customer.User == null)
            {
                msg = "SafePageController.Customer.User is NULL";
            }
            else if (SafePageController.Customer.UserInfo == null)
            {
                msg = "SafePageController.Customer.UserInfo is NULL";
            }
            else if (SafePageController.Customer.CustomerInfo == null)
            {
                msg = "SafePageController.Customer.CustomerInfo is NULL";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                SafePageController.ErrorMessage = msg;
                throw new VPBusinessException("CheckCustomerAndAgentForNulls", msg);
            }
        }

        private void SetConfirmTransactionInfo(R response)
        {
            string offersId = PageType.Confirm + VpPageControllerConstants.Transaction.Offers.ToString();
            var offers = SafePageController.GetStateValue(offersId) as List<VPDynamicOffer>;
            if (offers != null)
                return;

            if (response == null || response.Footer == null || response.Footer.VPDynamicOffers == null)
                return;

            SafePageController.SetStateValue(offersId, response.Footer.VPDynamicOffers);
        }

        private VPApprovalAction[] GetApprovalActions(T request)
        {
            var dict = new Dictionary<string, string>();
            if (ApprovalFlagsDictionary != null)
            {

                foreach (var approvalFlag in ApprovalFlagsDictionary)
                {
                    foreach (var flag in approvalFlag.Value)
                    {
                        if (flag.Fields == null || flag.Fields.Count == 0)
                            continue;

                        //look into the fields of the request if something is changed
                        foreach (var field in flag.Fields)
                        {
                            var originalValue = GetInitialRequestValue(field);
                            var requestValue = GetRequestValue(request, field);
                            if (!originalValue.Equals(requestValue))
                            {
                                if (!dict.ContainsKey(flag.Code))
                                    dict.Add(flag.Code, string.Empty);

                                if (!dict[flag.Code].Contains(field))
                                {
                                    if (dict[flag.Code].Length > 0)
                                        dict[flag.Code] += ",";

                                    dict[flag.Code] += field;
                                }
                            }
                        }
                    }
                }
            }

            return dict.Select(pair => new VPApprovalAction() { ActionCode = pair.Key, FieldSet = pair.Value }).ToArray();
        }
        

        private object GetInitialRequestValue(string approvalFlagField)
        {
            return GetRequestValue(InitialRequest, approvalFlagField);
        }

        private object GetRequestValue(T request, string approvalFlagField)
        {
            return GetNestedPropertyValue(request, approvalFlagField);
        }

        private object GetNestedPropertyValue(object obj, string property)
        {
            if (string.IsNullOrEmpty(property))
                return string.Empty;

            var propertyNames = property.Split('.');

            foreach (var p in propertyNames)
            {
                if (obj == null)
                    return string.Empty;

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(p);
                if (info == null)
                    return string.Empty;

                obj = info.GetValue(obj, null);

            }

            return obj;
        }

        public bool HighlightAskFields
        {
            get
            {
                var parameter = VpConfigurationParameters.GetGenericParameter("HighlightAskFields");
                bool temp = false;
                if (!string.IsNullOrEmpty(parameter))
                {
                    bool.TryParse(parameter.ToLower(), out temp);
                }
                return temp;
            }
        }

        private void OnTextChanged()
        {
            OnTextChanged(Page.Controls);
        }

        private void OnTextChanged(ControlCollection coll)
        {
            foreach (Control control in coll)
            {
                var textBox = control as TextBox;
                if (textBox != null && textBox is IAskControl)
                {
                    var onAskFieldChanged = "if (typeof onAskTextBoxChanged === 'function') onAskTextBoxChanged(this); if (typeof DisableReferCard === 'function') DisableReferCard(); if (typeof DisableOralInstruction === 'function') DisableOralInstruction();";

                    var onkeyup = textBox.Attributes["onkeyup"];
                    if (string.IsNullOrEmpty(onkeyup))
                        onkeyup = "javascipt: ";

                    if (onkeyup.IndexOf(onAskFieldChanged) < 0)
                        onkeyup += onAskFieldChanged;

                    textBox.Attributes["onkeyup"] = onkeyup;

                    var onPaste = textBox.Attributes["onpaste"];
                    if (string.IsNullOrEmpty(onPaste))
                        onPaste = "javascipt: ";

                    if (onPaste.IndexOf(onAskFieldChanged) < 0)
                        onPaste += onAskFieldChanged;

                    textBox.Attributes["onpaste"] = onPaste;

                    var onCut = textBox.Attributes["oncut"];
                    if (string.IsNullOrEmpty(onCut))
                        onCut = "javascipt: ";

                    if (onCut.IndexOf(onAskFieldChanged) < 0)
                        onCut += onAskFieldChanged;

                    textBox.Attributes["oncut"] = onCut;
                }

                var dropDownList = control as DropDownList;
                if (dropDownList != null)
                {
                    var onAskFieldChanged = "if (typeof onAskTextBoxChanged === 'function') onAskTextBoxChanged(this); if (typeof DisableReferCard === 'function') DisableReferCard(); if (typeof DisableOralInstruction === 'function') DisableOralInstruction();";
                    var onchange = dropDownList.Attributes["onchange"];
                    if (string.IsNullOrEmpty(onchange))
                        onchange = "javascipt: ";

                    if (onchange.IndexOf(onAskFieldChanged) < 0)
                        onchange += onAskFieldChanged;

                    dropDownList.Attributes["onchange"] = onchange;
                }

                var gridView = control as GridView;
                if (gridView != null)
                {
                    foreach (GridViewRow gridRow in gridView.Rows)
                        OnTextChanged(gridRow.Controls);
                }

                if (control.HasControls())
                    OnTextChanged(control.Controls);
            }
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Page is IStartPage)
                OnTextChanged();


            //if (!IsPostBackComingFromNextButton())
            InitializeAskCall();

            if (HighlightAskFields)
                HighlightAskControls();
        }

        private void HighlightAskControls()
        {
            var controls = AskControls;

            if (controls != null)
            {
                foreach (var control in controls)
                {
                    control.Highlight();
                }
            }
        }

        protected override void OnPreLoad(EventArgs e)
        {
            //TraceLog
            LogManager.TraceLog("OnPreLoad", "Entered Method");

            base.OnPreLoad(e);

            //if (!IsPostBack)
            //(new AskMappingHelper() { LoginToken = LoginToken }).ConfigureAskFlags(this);


            if (!IsAskPageEnabled)
                return;

            //set the output fields as to be populated by the ajax callback event
            foreach (var outputControl in AskOutputControls)
                outputControl.IsDataFetchDisabled = true;
        }

        private void InitializeAskCall()
        {
            if (!IsAskPageEnabled)
                return;

            //set the ajax call
            var cbReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "serverCallBack", "context");
            var callbackScript = "function callServer(arg, context) { " + cbReference + "; }";
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "callServer", callbackScript, true);

            //set the ajax callback
            var script = "function serverCallBack(value) { eval(value); }";
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "callbackServer", script, true);
        }

        public void RaiseCallbackEvent(string eventArgument)
        {
            if (!IsAskPageEnabled)
                return;

            if (IsPostBackComingFromNextButton())
                return;

            var sendAskCall = false;
            var valuesDict = eventArgument.Split(';');
            foreach (var valuesItem in valuesDict)
            {
                if (string.IsNullOrEmpty(valuesItem))
                    continue;

                if (valuesItem.Contains("SendAskCall"))
                {
                    sendAskCall = true;
                    continue;
                }

                var values = valuesItem.Split('=');
                var clientIds = values[0].Split(':');

                var parentClientId = string.Empty;
                var clientId = string.Empty;
                if (clientIds.Length == 2)
                {
                    parentClientId = clientIds[0];
                    clientId = clientIds[1];
                }
                else
                {
                    clientId = values[0];
                }

                var value = values[1];

                if (string.IsNullOrEmpty(clientId))
                    continue;

                if (SetAskControlValue(parentClientId, clientId, value))
                {
                    //a value is changed
                    _returnValue += "onAskFieldChanged();";
                    IsAskAlreadyCalled = false;
                }
            }

            if (sendAskCall)
                SendAskCall();
            else if (!Page.IsCallback)
            {
                // in case we are not sending the ask call there is no additional scripts to be added to _returnValue
                ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
            }
        }

        private void SendAskCall()
        {
            var extendedResponse = SendTransactionRequestForConfirmation();
            OnAskCallCompleted();
            
            ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUI2", "if(typeof(ignoreFirstFocusScript)=='undefined') {setTimeout(function () {var ctl = $(\":input:visible:not([type='radio']):not([readonly='readonly']):not([disabled='disabled']),select:visible:not([readonly='readonly']):not([disabled='disabled'])\");if(ctl!=null && ctl.length>0){for(i=0;i<ctl.length;i++){if($(ctl[i]).attr('skipfocus') != '1'){ctl[i].focus();$(ctl[i]).select();break;}}}},300);}", true);
            if (!extendedResponse.VblResponse.IsSuccess)
            {
                if (extendedResponse.VblResponse.Footer.Result != null && extendedResponse.VblResponse.Footer.Result.Error != null && !string.IsNullOrEmpty(extendedResponse.VblResponse.Footer.Result.Error.Description))
                    _returnValue = string.Format("javascript:onAskFieldChanged();hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", extendedResponse.VblResponse.Footer.Result.Error.Description.Replace("'", ""));

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return;
            }

            //To Call any host function after ask call
            var message = AfterAskCall(extendedResponse);
            if (!string.IsNullOrEmpty(message))
            {
                _returnValue = message;

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                //Process financial data received in ask reponse
                ProcessFinancialData(extendedResponse);
                return;

            }

            _returnValue += "hideOverlay(); onAskComplete();";
            IsAskAlreadyCalled = true;
             
            if (!Page.IsCallback)
            {
                // in case we are sending the ask call additional scripts are added to _returnValue
                ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
            }

            if (!IgnoreFinancialData && !ProcessFinancialData(extendedResponse))
                return;
            
            SafePageController.SetStateValue("ExtendedResponse", extendedResponse);
            ProcessResponseInternal(extendedResponse);
            DisplayConfirmWarningMessage();

            CheckIfReferCardControlRequired();


        }

        protected void CheckIfReferCardControlRequired()
        {

            //isForwardToRefercard approval required
            if (VpConfigurationManager.IsReferCardEnabled && VpConfigurationManager.IsBranchEnabledForReferCard)
            {
                CheckIfReferCardApprovalRequired();
            }

            //For refer card approved transactiopns disable all controls
            if (IsReferCardApproved)
            {
                DisableAllControls();
            }
            
        }

        protected virtual void ForwardToReferCardClicked(bool isChecked)
        {
           
        }

        protected virtual void OralInstructionClicked(bool isChecked)
        {

        }

        private void SendReferCardCall()
        {
            var extendedResponse = SendApprovalRequestToReferCard();

             string msg = string.Empty;
            
            //CheckIfReferCardApprovalRequired();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUI2", "if(typeof(ignoreFirstFocusScript)=='undefined') {setTimeout(function () {var ctl = $(\":input:visible:not([type='radio']):not([readonly='readonly']):not([disabled='disabled']),select:visible:not([readonly='readonly']):not([disabled='disabled'])\");if(ctl!=null && ctl.length>0){for(i=0;i<ctl.length;i++){if($(ctl[i]).attr('skipfocus') != '1'){ctl[i].focus();$(ctl[i]).select();break;}}}},300);}", true);
            if (!extendedResponse.VblResponse.IsSuccess)
            {
                if (extendedResponse.VblResponse.Footer.Result != null && extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:hideOverlay();onReferCardCallFailed();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", extendedResponse.VblResponse.Footer.Result.Error.Description);

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return;
            }
            //success case
            else
            {
                msg = GetGlobalResource("ReferCardRequestSuccessMessage");
                //todo: get success message from globalizat
                _returnValue = string.Format("javascript:hideOverlay();alertModal(getSuccessTitleText(), '{0}', getOKButtonText());", msg);

                OnReferCardCallCompleted();
            }

            //Set Information display
            SetInformationDisplay(msg);

            _returnValue += "onReferCardCallComplete();";

      

            if (!Page.IsCallback)
            {
                // in case we are sending the ask call additional scripts are added to _returnValue
                ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
            }

            
        }

        private void RejectApprovalRequest()
        {
            var extendedResponse = SendRejectApprovalRequest();

            string msg = string.Empty;
       
            OnRejectApprovalRequestCompleted();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUI2", "if(typeof(ignoreFirstFocusScript)=='undefined') {setTimeout(function () {var ctl = $(\":input:visible:not([type='radio']):not([readonly='readonly']):not([disabled='disabled']),select:visible:not([readonly='readonly']):not([disabled='disabled'])\");if(ctl!=null && ctl.length>0){for(i=0;i<ctl.length;i++){if($(ctl[i]).attr('skipfocus') != '1'){ctl[i].focus();$(ctl[i]).select();break;}}}},300);}", true);
            if (!extendedResponse.VblResponse.IsSuccess)
            {
                if (extendedResponse.VblResponse.Footer.Result != null && extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", extendedResponse.VblResponse.Footer.Result.Error.Description);

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return;
            }
            //success case
            else
            {
                msg = GetGlobalResource("ApprovalRequestRejectSuccessMessage");
                //todo: get success message from globalizat
                _returnValue = string.Format("javascript:hideOverlay();alertModal(getSuccessTitleText(), '{0}', getOKButtonText());", msg);
            }

            //Set Information display
            SetInformationDisplay(msg);

            _returnValue += "onRejectApprovalRequestCallComplete();";



            if (!Page.IsCallback)
            {
                // in case we are sending the ask call additional scripts are added to _returnValue
                ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
            }


        }

        private void SendComplianceCall()
        {
            var extendedResponse = SendComplianceRequest();

            string msg = string.Empty;

            OnComplianceCallCompleted();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUI2", "if(typeof(ignoreFirstFocusScript)=='undefined') {setTimeout(function () {var ctl = $(\":input:visible:not([type='radio']):not([readonly='readonly']):not([disabled='disabled']),select:visible:not([readonly='readonly']):not([disabled='disabled'])\");if(ctl!=null && ctl.length>0){for(i=0;i<ctl.length;i++){if($(ctl[i]).attr('skipfocus') != '1'){ctl[i].focus();$(ctl[i]).select();break;}}}},300);}", true);
            if (!extendedResponse.VblResponse.IsSuccess)
            {
                if (extendedResponse.VblResponse.Footer.Result != null && extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", extendedResponse.VblResponse.Footer.Result.Error.Description);

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return;
            }
            //success case
            else
            {
                msg = GetGlobalResource("ComplianceSuccessMessage");
                //todo: get success message from globalizat
                _returnValue = string.Format("javascript:hideOverlay();alertModal(getSuccessTitleText(), '{0}', getOKButtonText());", msg);
            }

            //Set Information display
            SetInformationDisplay(msg);

            _returnValue += "showOnlyCloseButton();";



            if (!Page.IsCallback)
            {
                // in case we are sending the ask call additional scripts are added to _returnValue
                ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
            }


        }

        private void SendCallCenterApprovalCall()
        {
            var extendedResponse = SendCallCenterApprovalRequest();

            string msg = string.Empty;

            OnCallCenterApprovalRequestCompleted();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUI2", "if(typeof(ignoreFirstFocusScript)=='undefined') {setTimeout(function () {var ctl = $(\":input:visible:not([type='radio']):not([readonly='readonly']):not([disabled='disabled']),select:visible:not([readonly='readonly']):not([disabled='disabled'])\");if(ctl!=null && ctl.length>0){for(i=0;i<ctl.length;i++){if($(ctl[i]).attr('skipfocus') != '1'){ctl[i].focus();$(ctl[i]).select();break;}}}},300);}", true);
            if (!extendedResponse.VblResponse.IsSuccess)
            {
                if (extendedResponse.VblResponse.Footer.Result != null && extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", extendedResponse.VblResponse.Footer.Result.Error.Description);

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return;
            }
            //success case
            else
            {
                msg = GetGlobalResource("CallCenterSuccessMessage");
                //todo: get success message from globalizat
                _returnValue = string.Format("javascript:hideOverlay();alertModal(getSuccessTitleText(), '{0}', getOKButtonText());", msg);

                var ctlOralInstructionControl = OralInstructionControl;
                if (ctlOralInstructionControl != null)
                {
                    ctlOralInstructionControl.Checked = true;
                    ctlOralInstructionControl.Enabled = false;
                }
            }

            //Set Information display
            SetInformationDisplay(msg);

            _returnValue += "showOnlyCloseButton();";



            if (!Page.IsCallback)
            {
                // in case we are sending the ask call additional scripts are added to _returnValue
                ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
            }


        }

        private void SaveTransactionRequest()
        {
            CheckCustomerAndAgentForNulls();

            T existingRequest = InitializeRequestData();
            RequestData = GetRequestForConfirmation(existingRequest);

            var extendedResponse = SafePageController.GetStateValue("ExtendedResponse") as ExtendedResponseTransactionData<R>;
            ProcessResponseInternal(extendedResponse);


            List<VPLogEntry> list;
            if (RequestData is IIALRequest)
            {
                var ialRequest = (RequestData as IIALRequest).GetCommonIALRequest();

                if (ialRequest is IFinancialData)
                {
                    var iFinancialDataRequest = ialRequest as IFinancialData;
                    if (iFinancialDataRequest.FinancialData != null)
                    {
                        iFinancialDataRequest.FinancialData.ExchangeRatesLCY = ExchangeRatesLCY;
                        iFinancialDataRequest.FinancialData.TotalDebitAmount = TotalDebitAmount;

                        //put total charges in customLgEntry for reporting purpose.
                        if (iFinancialDataRequest.FinancialData.ChargesDetail != null)
                        {
                            list = RequestData.CustomLogEntryList == null ? new List<VPLogEntry>() : RequestData.CustomLogEntryList.ToList();

                            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TotalCharges.ToString(), Value = iFinancialDataRequest.FinancialData.ChargesDetail.Sum(c => c.ChargeAmountLCY) + iFinancialDataRequest.FinancialData.AmountVAT  });
                            if (iFinancialDataRequest.FinancialData.ExchangeRatesLCY != 0)
                            {
                                list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TotalChargesInSourceCurrency.ToString(), Value = ReportHelper.GetTotalCharges(iFinancialDataRequest.FinancialData) });
                            }

                            RequestData.CustomLogEntryList = list.ToArray();
                        }
                    }

                }
            }

            list = RequestData.CustomLogEntryList == null ? new List<VPLogEntry>() : RequestData.CustomLogEntryList.ToList();

            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.CustomerSegment.ToString(), Value = SafePageController.Customer.CustomerInfo.CustomerSegment });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.CustomerSubSegment.ToString(), Value = SafePageController.Customer.CustomerInfo.CustomerSubSegment });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.CustomerName.ToString(), Value = GetCustomerName() });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.CustomerPrefferedLanguage.ToString(), Value = SafePageController.Customer.CustomerInfo.CustomerPrefferedLanguage });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.IdentityIssuingCountry.ToString(), Value = SafePageController.Customer.CustomerInfo.CustomerIdentification == null ? string.Empty : SafePageController.Customer.CustomerInfo.CustomerIdentification.IdentityIssuingCountry });
            

            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TransactionAmount.ToString(), Value = TransactionAmount });
            list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.TransactionCurrency.ToString(), Value = TransactionCurrency });

            list.RemoveAll(i => i.Key == VpPageControllerConstants.Transaction.CrmCaseOpenReason.ToString());

            var caseOpenReasons = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.CrmCaseOpenReason) as List<string>;
            if (caseOpenReasons != null)
            {
                list.Add(new VPLogEntry { Key = VpPageControllerConstants.Transaction.CrmCaseOpenReason.ToString(), Value = string.Join("\n", caseOpenReasons) });
            }

            RequestData.CustomLogEntryList = list.ToArray();
            
            SafePageController.SetStateValue("TransactionName", TransactionName);
            SafePageController.TransactionHeader.User.LoggedInAccountNumber = GetLoggedInCustomerAccount();
            TransactionInformation.TransactionSettings.IsSignatureVerificationRequired = SafePageController.SignatureVerificationFlagActual && IsSignatureVerificationRequired();
        }

        protected virtual string GetCustomerName()
        {
            return ReportHelper.GetCustomerName();
        }

        protected virtual bool IsSignatureVerificationRequired()
        {
            return true;
        }

        protected virtual string GetLoggedInCustomerAccount()
        {
            return string.Empty;
        }

        protected virtual bool ProcessFinancialData(ExtendedResponseTransactionData<R> extendedResponse)
        {
            var ifinancialData = (extendedResponse.IalResponse as IFinancialData);
            if (ifinancialData == null)
            {
                if (extendedResponse.VblResponse.Footer.Result != null &&
                    extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:onAskFieldChanged();hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", "IAL response doean not implement IFinancialData");

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return false;
            }

            var financialData = ifinancialData.FinancialData;
            if (financialData == null)
            {
                if (extendedResponse.VblResponse.Footer.Result != null &&
                    extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:onAskFieldChanged();hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", "Invalid Financial Data");

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return false;
            }

            var chargesDetail = financialData.ChargesDetail;
            if (chargesDetail == null)
            {
                if (extendedResponse.VblResponse.Footer.Result != null &&
                    extendedResponse.VblResponse.Footer.Result.Error != null)
                    _returnValue = string.Format("javascript:onAskFieldChanged();hideOverlay();alertModal(getErrorTitleText(), '{0}', getOKButtonText());", "Invalid Financial Data");

                if (!Page.IsCallback)
                {
                    // in case we are sending the ask call additional scripts are added to _returnValue
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "syncCallBack1", _returnValue, true);
                }

                return false;
            }

            decimal exchangeRate = financialData.ExchangeRates;

            ExchangeRatesLCY = financialData.ExchangeRatesLCY;

            //set the response
            if (AskOutputControls != null && AskOutputControls.Count != 0)
            {
                //set charges
                var chargesControl = AskOutputControls.GetControl(AskOutputFieldTypesEnum.Charges);
                if (chargesControl != null)
                {
                    // var value = chargeValue;
                    _returnValue += chargesControl.GetJavascriptSetValueFunction(chargesDetail);

                    decimal charges = chargesDetail.Sum(d => d.ChargeAmountLCY);
                    SetAskOutputControlValue(chargesControl, charges.ToString());
                    InitialRequest = SetInitialRequest();
                }

                //set exchange rate
                var exchangeRatesControl = AskOutputControls.GetControl(AskOutputFieldTypesEnum.ExchangeRates);
                if (exchangeRatesControl != null)
                {
                    var value = exchangeRate.ToString();
                    _returnValue += exchangeRatesControl.GetJavascriptSetValueFunction(value);
                    SetAskOutputControlValue(exchangeRatesControl, value);
                    InitialRequest = SetInitialRequest();
                }

                //set value date
                var valueDateControl = AskOutputControls.GetControl(AskOutputFieldTypesEnum.ValueDate);
                if (valueDateControl != null && !string.IsNullOrEmpty(financialData.ValueDate))
                {
                    var value = financialData.ValueDate;
                    _returnValue += valueDateControl.GetJavascriptSetValueFunction(value);
                    SetAskOutputControlValue(valueDateControl, value);
                    InitialRequest = SetInitialRequest();
                }
            }

            //assign values
            _returnValue += OnExternalCallReceived(extendedResponse.IalResponse);

            SellAmount = financialData.SellAmount;
            BuyAmount = financialData.BuyAmount;
            TotalDebitAmount = financialData.TotalDebitAmount;

            //AskExchangeRate should be Set only once in Ask1 call
            if (AskExchangeRate == 0)
                AskExchangeRate = financialData.ExchangeRates;
                        
            ExchangeRate = financialData.ExchangeRates;

            //For Refer Card, check if exchange rates is modifed after Ask1
            IsExchangeRateChanged = AskExchangeRate != ExchangeRate;

            //AskValueDate should be Set only once in Ask1 call
            if (string.IsNullOrEmpty(AskValueDate))
                AskValueDate = financialData.ValueDate;
            
            ValueDate = financialData.ValueDate;

            //For Refer Card, check if Value Date is modifed after Ask1
            IsValueDateChanged = AskValueDate != ValueDate;
                        

            return true;
        }

        public string GetCallbackResult()
        {
            return _returnValue;
        }

        private bool SetAskControlValue(string parentClientId, string clientId, string value)
        {
            var valueChanged = false;

            var clentIdForCheck = clientId;
            if (!string.IsNullOrEmpty(parentClientId))
                clentIdForCheck = parentClientId;

            var control = AskControls.GetControl(clentIdForCheck);
            if (control == null)
                return false;

            var isValidValue = control.ValidateAskValue(clientId, value);
            var item = new AskControlsDictItem
            {
                ControlClientID = clientId,
                ControlValue = value,
            };

            if (!AskControlValues.ContainsKey(item.ControlClientID))
            {
                if (isValidValue)
                {
                    valueChanged = true;
                    AskControlValues.Add(item.ControlClientID, item);
                }
            }
            else
            {
                if (isValidValue)
                {
                    valueChanged = (AskControlValues[item.ControlClientID].ControlValue != item.ControlValue);
                    AskControlValues[item.ControlClientID] = item;
                }
                else
                {
                    AskControlValues.Remove(item.ControlClientID);
                }
            }

            return valueChanged;
        }

        private void SetAskOutputControlValue(IAskOutputControl control, string value)
        {
            var item = new AskControlsDictItem
            {
                ControlClientID = control.ClientID,
                ControlValue = value,
            };

            if (!AskOutputControlValues.ContainsKey(item.ControlClientID))
            {
                AskOutputControlValues.Add(item.ControlClientID, item);
            }
            else
            {
                AskOutputControlValues[item.ControlClientID] = item;
            }
        }

        public string GetAskControlValue(string clientId)
        {
            var value = string.Empty;
            if (AskControlValues.ContainsKey(clientId))
                value = AskControlValues[clientId].ControlValue;

            return value;
        }

        public string GetAskOutputControlValue(string clientId)
        {
            var value = string.Empty;
            if (AskOutputControlValues.ContainsKey(clientId))
                value = AskOutputControlValues[clientId].ControlValue;

            return value;
        }

        public decimal ExchangeRatesLCY
        {
            get
            {
                var value = SafePageController.GetStateValue("ExchangeRatesLCY");

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue("ExchangeRatesLCY", value);
            }
        }

        public decimal ExchangeRate
        {
            get
            {
                var value = SafePageController.GetStateValue("ExchangeRate");

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue("ExchangeRate", value);
            }
        }

        public decimal AskExchangeRate
        {
            get
            {
                var value = SafePageController.GetStateValue("AskExchangeRate");

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue("AskExchangeRate", value);
            }
        }

        public bool IsExchangeRateChanged
        {
            get
            {
                var value = SafePageController.GetStateValue("IsExchangeRateChanged");

                if (value != null)
                    return (bool)value;
                return false;
            }
            set
            {
                SafePageController.SetStateValue("IsExchangeRateChanged", value);
            }
        }

        public decimal SellAmount
        {
            get
            {
                var value = SafePageController.GetStateValue("SellAmount");

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue("SellAmount", value);
            }
        }

        public decimal BuyAmount
        {
            get
            {
                var value = SafePageController.GetStateValue("BuyAmount");

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue("BuyAmount", value);
            }
        }
        public decimal TotalDebitAmount
        {
            get
            {
                var value = SafePageController.GetStateValue("TotalDebitAmount");

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue("TotalDebitAmount", value);
            }
        }

        public string ValueDate
        {
            get
            {
                var value = SafePageController.GetStateValue("ValueDate");

                if (value != null)
                    return (string)value;
                return DateTime.Today.ToIALFormat();
            }
            set
            {
                SafePageController.SetStateValue("ValueDate", value);
            }
        }

        public string AskValueDate
        {
            get
            {
                var value = SafePageController.GetStateValue("AskValueDate");

                if (value != null)
                    return (string)value;
                return string.Empty;
            }
            set
            {
                SafePageController.SetStateValue("AskValueDate", value);
            }
        }

        public bool IsValueDateChanged
        {
            get
            {
                var value = SafePageController.GetStateValue("IsValueDateChanged");

                if (value != null)
                    return (bool)value;
                return false;
            }
            set
            {
                SafePageController.SetStateValue("IsValueDateChanged", value);
            }
        }

        public decimal TransactionAmount
        {
            get
            {
                var value = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionAmount);

                if (value != null)
                    return (decimal)value;
                return 0;
            }
            set
            {
                SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionAmount, value);
            }
        }

        public string TransactionCurrency
        {
            get
            {
                var value = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionCurrency);

                if (value != null)
                    return (string)value;
                return string.Empty;
            }
            set
            {
                SafePageController.SetStateValue(VpPageControllerConstants.Transaction.TransactionCurrency, value);
            }
        }

        public void FillChargeAccountNumber(List<VpChargesDetail> chargesDetails, string accountNumber)
        {
            if (chargesDetails != null)
            {
                try
                {
                    chargesDetails.Where(c => c != null && string.IsNullOrEmpty(c.ChargeAccountNumber)).ToList().ForEach(c => c.ChargeAccountNumber = accountNumber);
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex);
                }
            }
        }
    }
}
