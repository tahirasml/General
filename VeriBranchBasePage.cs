using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Common.Definitions.SecurityManagement;
using Common.Definitions.TransactionUtilityManagement;
using Common.Definitions.UserManagement;
using VeriBranch.Framework.Definitions;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.Constants;
using VeriBranch.WebApplication.Diagnostics.PerformanceCounters;
using VeriBranch.WebApplication.Exceptions;
using VeriBranch.WebApplication.Helpers;
using VeriBranch.WebApplication.ObjectModel;

namespace VeriBranch.WebApplication.UIProcess
{
    /// <summary>
    /// All web pages are overridden in this class.
    /// </summary>
    public abstract class VeriBranchBasePage : VeriBranchCommonBasePage
    {
        const string ReportTimestamp = "ddMMMyyyy HHmmss";
        protected static readonly CultureInfo Ci = new CultureInfo(HelperBase.EnglishCulture);

        private const string SET_CONTROLLER_STATE = "VeriBranchBasePage.SetControllerState";
        private const string CHECK_QUERY_STRING_PARAMETERS = "VeriBranchBasePage.CheckQueryStringParams";
        private const string PROCESS_EXCEPTION = "VeriBranchBasePage.ProcessException";
        private const string PROCESS_VP_EXCEPTION = "VeriBranchBasePage.ProcessVpException";
        private const string NORMALIZE_CURRENT_REQUEST_URL = "VeribranchBasePage.NormalizeCurrentRequestUrlForPageAccessCheck";
        private const string GET_REQUEST_URL = "VeribranchBasePage.GetRequestUrl";
        private const string ENSURE_ROLE_PAGE_ACCESS_INSTANCE = "VeriBranchBasePage.EnsureRolePageAccessInstance";
        private const string CHECK_USER_ACCESS_RIGHT = "VeribranchBasePage.CheckUserAccessRight";
        private const string GET_ROLE_PAGE_ACCESS = "VeriBranchBasePage.GetRolePageAccess";
        private const string READ_ROLE_AUTH_REFRESH_RATE = "VeriBranchBasePage.ReadPageRoleAuthRefreshRate";
        private const string SET_EXCEPTION_LOGGING_TICKET = "VeriBranchBasePage.SetExceptionLoggingTicket";
        private const string PROCESS_EXCEPTION_FROM_OUTSIDE = "VeriBranchBasePage.ProcessExceptionFromOutside";
        private const string PROCESS_VP_BUSINESS_EXCEPTION = "VeriBranchBasePage.ProcessVpBusinessException";
        private const string SET_DIV_BACKGROUND = "VeriBranchBasePage.SetDivBackground";

        /// <summary>
        /// Gets the current culture.
        /// </summary>
        /// <value>
        /// The current culture.
        /// </value>
        protected string CurrentCulture { get; private set; }

        /// <summary>
        /// Gets or sets if browser history back button is enabled
        /// </summary>
        /// <value>
        /// <c>true</c> browser history back button is enabled, otherwise <c>false</c>.
        /// </value>
        protected bool EnableBrowserHistoryBackButton { get; set; }

        /// <summary>
        /// Gets the default theme.
        /// </summary>
        /// <value>
        /// The default theme.
        /// </value>
        protected string DefaultTheme { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request comes from menu.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the request comes from menu, otherwise <c>false</c>.
        /// </value>
        protected bool ComesFromMenu
        {
            get { return Request.QueryString[VpConstants.Common.VpStateResetConstant] != null; }
        }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        protected ResourceHelper Resources
        {
            get
            {
                return ResourceHelper.Resources;
            }
        }

        protected virtual void SetTransactionInfo()
        {
        }

        public virtual void SetEntitlements(List<VPEntitlement> entitlements)
        {
            var ctlChargesControl = ChargesControl;

            if (ctlChargesControl != null)
            {
                ctlChargesControl.LoadEntitlements(entitlements);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VeriBranchBasePage" /> class.
        /// </summary>
        protected VeriBranchBasePage()
        {

        }

        protected virtual void LoadTransactionProcessSteps() { }

        protected virtual void SetProcessSteps() { }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            EnableBrowserHistoryBackButton = false;

            try
            {
                DefaultTheme = VpConfigurationManager.GetConfigurationParameter(FrontOfficeConstants.Frontoffice_UITheme);
                CurrentCulture = VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.LANGUAGE);
            }
            catch (Exception ex)
            {
                ProcessExceptionFromOutside(ex);
            }
            finally
            {
                DisableHistoryBack();
            }

            Response.Expires = MINUS_ONE;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            try
            {
                //Old call not used anymore because VBL will do this check internally for each transaction
                //IsSessionForcedLogout();

                //check query string params for the hack
                //CheckQueryStringParams();

                SafePageController.SetProfileValue("TemplateLoad", SafePageController.QueryStrings["TemplateLoad"]);
                var isTemplate = (SafePageController.GetProfileValue("TemplateLoad") != null);
                if (isTemplate)
                {
                    var language = UserLanguage;
                    //reset previous values
                    SafePageController.ResetProfile();
                    UserLanguage = language;
                    SafePageController.SetProfileValue("TemplateLoad", Request.QueryString["TemplateLoad"]);
                    var _NavigationControl = NavigationControl;
                    if (_NavigationControl != null)
                        _NavigationControl.Visible = false;
                }

                //check user right to access page!
                if (!isTemplate)
                    CheckUserAccessRight();

                InitializePageController();

                SetEmptyHeaderLabel();
                ResetWarningDisplay();

                if (!isTemplate)
                    CheckSessionHijacking();

                RegisterPrinterActiveX();

                if (Page.IsPostBack)
                {
                    GetStateFromUI();

                    if (IsPostBackComingFromNextButton())
                        OnNextButtonClicked();
                }
                else
                {
                    SetControllerState();

                    if (!isTemplate || Page.ToString() == "ASP.mainpage_aspx")
                    {
                        DoPageAction();
                        SetUIFromState();
                        SetTransactionInfo();
                    }

                    LoadTransactionProcessSteps();
                    SetProcessSteps();

                    if (!isTemplate)
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUI2", "if(typeof(ignoreFirstFocusScript)=='undefined') {setTimeout(function () {var ctl = $(\":input:visible:not([type='radio']):not([readonly='readonly']):not([disabled='disabled']),select:visible:not([readonly='readonly']):not([disabled='disabled'])\");if(ctl!=null && ctl.length>0){for(i=0;i<ctl.length;i++){if($(ctl[i]).attr('skipfocus') != '1'){ctl[i].focus();$(ctl[i]).select();break;}}}},1000);}", true);
                }

                //set the page action to the proper page when we are using Server.Transfer instead of Response.Redirect
                if (!VpAppConfigurationManager.UseResponseRedirect && Page.Form != null)
                    Page.Form.Action = Request.CurrentExecutionFilePath;

                LocalizePageContent();

                if (!isTemplate)
                    EnableVblValidator();

            }
            catch (ThreadAbortException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                ProcessExceptionFromOutside(ex);
            }
            finally
            {
                DisableHistoryBack();
            }
        }

        protected virtual void RegisterPrinterActiveX() { }

        protected virtual void OnNextButtonClicked() { }

        protected virtual void EnableVblValidator()
        {
        }

        //Old call not used anymore because VBL will do this check internally for each transaction
        ///// <summary>
        ///// Determines whether [is session forced logout].
        ///// </summary>
        //private void IsSessionForcedLogout()
        //{
        //    try
        //    {
        //        if (UserHelper.IsSessionForcedLogout())
        //            Server.Transfer(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.LOGOUT_PAGEURL).ToString());
        //    }
        //    catch (ThreadAbortException)
        //    {
        //        //do nothing
        //    }
        //}

        /// <summary>
        /// Disables the history back.
        /// </summary>
        private void DisableHistoryBack()
        {
            if (!EnableBrowserHistoryBackButton)
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), VpBasePageConstants.DISABLE_BACK_BUTTON_SCRIPT_IDENTIFIER, VpBasePageConstants.DISABLE_BACK_BUTTON_SCRIPT);
        }

        /// <summary>
        /// Sets the state of the controller.
        /// </summary>
        private void SetControllerState()
        {
            if (string.IsNullOrEmpty(Request.AppRelativeCurrentExecutionFilePath))
                throw new VPSystemException(SET_CONTROLLER_STATE, VpBasePageConstants.APP_PATH_IS_NULL_OR_EMPTY);

            string appPath = Request.AppRelativeCurrentExecutionFilePath.ToLower();
            if (appPath.Contains(VPResourceConstants.ICommon.StartPageSuffix))
                SafePageController.SetControllerState(PageController.ControllerState.Started);
            else if (appPath.Contains(VPResourceConstants.ICommon.ExecutePageSuffix))
                SafePageController.SetControllerState(PageController.ControllerState.Executed);

            if (ComesFromMenu) //comes from menu case 
            {
                SafePageController.ResetState();
                SafePageController.RequestData = null;
            }
        }

        /// <summary>
        /// Checks the session hijacking.
        /// </summary>
        private void CheckSessionHijacking()
        {
            try
            {
                if (SafePageController == null)
                    Server.Transfer(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.LOGOUT_PAGEURL).ToString());

                //if page controller user info section is null and page request is error page ignore check
                if ((SafePageController.Customer.User == null) && (Page.Request.Url.AbsolutePath.Contains(VpBasePageConstants.ERRORPAGE_STRING)))
                    return;

                if (SafePageController.GetProfileValue(VeribranchBasePageConstants.IS_USER_AUTHENTICATED) == null)
                    return;

                var currentLoginIP = GetCurrentLoginIP();
                var currentloginAgent = GetCurrentLoginAgent();
                string currentIP = (Sha1EncryptedData)System.Web.HttpContext.Current.Request.UserHostAddress;
                string currentUserAgent = Convert.ToString(System.Web.HttpContext.Current.Request.UserAgent);

                if (currentLoginIP != currentIP || currentloginAgent != currentUserAgent)
                    Server.Transfer(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.LOGOUT_PAGEURL).ToString());
            }
            catch (ThreadAbortException)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Gets the current logged in agent.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentLoginAgent()
        {
            string currentloginAgent = string.Empty;

            object loginUserAgent = SafePageController.GetProfileValue(FrontOfficeConstants.LoginUserAgent);
            if (loginUserAgent != null)
                currentloginAgent = Convert.ToString(loginUserAgent);

            return currentloginAgent;
        }

        /// <summary>
        /// Gets the current login IP.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentLoginIP()
        {
            string currentloginIP = string.Empty;

            object loginIps = SafePageController.GetProfileValue(FrontOfficeConstants.LoginIps);
            if (loginIps != null)
                currentloginIP = (Sha1EncryptedData)loginIps;

            return currentloginIP;
        }

        /// <summary>
        /// Checks the query string params.
        /// </summary>
        private void CheckQueryStringParams()
        {
            Regex regex = new Regex(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.QUERYSTRING_REGEXPRESSION).ToString());
            foreach (string key in Request.QueryString.AllKeys)
            {
                string value = Request.QueryString[key];
                if (regex.IsMatch(value))
                    throw new VPSystemException(CHECK_QUERY_STRING_PARAMETERS, string.Format(VpBasePageConstants.INVALID_QUERY_STRING_PARAMETERS_TEMPLATE, Request.RawUrl));
            }
        }

        #region Exception Handling

        /// <summary>
        /// On .aspx pages event based operatiosn needs be surrounded with try catch and in 
        /// catch ProcessExceptionFromOutside needs to be called for creating ticket number
        /// </summary>
        /// <param name="ex">
        /// VPSystemException, VPBusinessException or Exception
        /// </param>
        public void ProcessExceptionFromOutside(Exception ex)
        {
            if (ex == null)
                throw new VPException(PROCESS_EXCEPTION_FROM_OUTSIDE, VPExceptionConstants.EXCEPTION_NULL);

            //if ((ex as VPThirdPartyBusinessException) != null)
            //    ProcessVpThirdPartyBusinessException((VPThirdPartyBusinessException)ex);
            else if ((ex as VPBusinessException) != null)
                ProcessVpBusinessException((VPBusinessException)ex);
            else if ((ex as VPSystemException) != null)
                ProcessVpSystemException((VPSystemException)ex);
            else if ((ex as VPException) != null)
                ProcessVpException((VPException)ex);
            else
                ProcessException(ex);
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void ProcessException(Exception ex)
        {
            if (ex == null)
                throw new VPException(PROCESS_EXCEPTION, VPExceptionConstants.EXCEPTION_NULL);

            CheckSessionTimeOut();
            SafePageController.LoggingTicket = ApplicationInitializer.LogException(PROCESS_EXCEPTION, ex).ToString();
            SetExceptionHelpLink(ex);

            SafePageController.PageControllerRedirect(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.ERROR_PAGEURL).ToString());
        }

        /// <summary>
        /// Processes the vp system exception.
        /// </summary>
        /// <param name="ex">The exception</param>
        private void ProcessVpSystemException(VPSystemException ex)
        {
            ProcessVpException(ex);
        }

        /// <summary>
        /// Processes the vp exception.
        /// </summary>
        /// <param name="ex">The exception</param>
        private void ProcessVpException(VPException ex)
        {
            if (ex == null)
                throw new VPException(PROCESS_VP_EXCEPTION, VPExceptionConstants.EXCEPTION_NULL);

            SafePageController.LoggingTicket = ApplicationInitializer.LogException(PROCESS_VP_EXCEPTION, ex).ToString();
            SetExceptionHelpLink(ex);
            SafePageController.ErrorMessage = ex.Message;

            //for fill by developer Exception information
            LoadExceptionInformation(ex);

            VeriBranchPerformanceCounter.Increase(CumulativeCounterTypes.VpSystemExceptionsTotal);
            if (ex.Message == IExceptions.UserDoesntHaveAccessRight.ToString())
            {
                SafePageController.SetStateValue(VpBasePageConstants.WARNING_DISPLAY_MESSAGE, ResourceHelper.DoGlobalizationResourcesInquiry(IExceptions.UserDoesntHaveAccessToThisPage)); //VPException.GetPresentationMessage(IExceptions.UserDoesntHaveAccessToThisPage));
                SafePageController.PageControllerRedirect(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.WARNING_PAGEURL).ToString());
            }
            else
            {
                SafePageController.PageControllerRedirect(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.ERROR_PAGEURL).ToString());
            }
        }

        /// <summary>
        /// Processes the vp third party business exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        //private void ProcessVpThirdPartyBusinessException(VPThirdPartyBusinessException ex)
        //{
        //    ProcessVpBusinessException(ex);
        //}

        /// <summary>
        /// Processes the vp business exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void ProcessVpBusinessException(VPBusinessException ex)
        {
            if (ex == null)
                throw new VPException(PROCESS_VP_BUSINESS_EXCEPTION, VPExceptionConstants.EXCEPTION_NULL);

            LoadExceptionInformation(ex);   //for developer full Exception information
            SetExceptionLoggingTicket(ex);  // Loggin ticket
            SetExceptionHelpLink(ex);       // help link

            VeriBranchPerformanceCounter.Increase(CumulativeCounterTypes.VpBusinessExceptionsTotal);
        }

        #endregion

        /// <summary>
        /// Checks the session time out.
        /// </summary>
        private void CheckSessionTimeOut()
        {
            if (SafePageController == null || SafePageController.Customer.User == null)
                Server.Transfer(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.SESSION_TIMEOUT_PAGEURL).ToString());
        }

        /// <summary>
        /// Gets the current module id.
        /// </summary>
        /// <returns></returns>
        //public static long GetCurrentModuleId()
        //{
        //    RolePageAccess rolePageAccess = EnsureRolePageAccessInstance();
        //    return rolePageAccess.GetModuleIdByUrl(NormalizeCurrentRequestUrlForPageAccessCheck());
        //}

        /// <summary>
        /// Normalizes the current request URL for page access check.
        /// </summary>
        /// <returns></returns>
        public static string NormalizeCurrentRequestUrlForPageAccessCheck()
        {
            string applicationPath = HttpContext.Current.Request.ApplicationPath;
            if (string.IsNullOrEmpty(applicationPath))
                throw new VPSystemException(NORMALIZE_CURRENT_REQUEST_URL, VpBasePageConstants.APPLICATION_PATH_IS_NULL_OR_EMPTY);

            return GetRequestUrl(applicationPath);
        }

        /// <summary>
        /// Gets the request URL.
        /// </summary>
        /// <param name="applicationPath">The application path.</param>
        /// <returns></returns>
        private static string GetRequestUrl(string applicationPath)
        {
            string absolutePath = HttpContext.Current.Request.Url.AbsolutePath;
            if (absolutePath.Length < applicationPath.Length + 1)
                throw new VPSystemException(GET_REQUEST_URL, VpBasePageConstants.ABSOLUTE_PATH_IS_NOT_ENOUGTH_LENGTH);

            return absolutePath.Substring(applicationPath.Length + 1).ToLower();
        }

        /// <summary>
        /// Ensures the role page access instance.
        /// </summary>
        /// <returns></returns>
        public RolePageAccess EnsureRolePageAccessInstance()
        {
            RolePageAccess rolePageAccess = (RolePageAccess)HttpContext.Current.Application[RolePageAccess.cRolePageAccessKey];

            if (rolePageAccess == null || rolePageAccess.HasExpired || !rolePageAccess.IsPageModuleIDsLoaded)
            {
                rolePageAccess = GetRolePageAccess();
                if (rolePageAccess == null)
                    throw new VPSystemException(ENSURE_ROLE_PAGE_ACCESS_INSTANCE, VpBasePageConstants.ROLE_PAGE_ACCESS_NULL);

                HttpContext.Current.Application[RolePageAccess.cRolePageAccessKey] = rolePageAccess;
            }

            return rolePageAccess;
        }

        /// <summary>
        /// Checks the user access right.
        /// </summary>
        private void CheckUserAccessRight()
        {
            //if channel is Branch or Call Center, don't check for roles
            if (ApplicationController.Channel == ChannelTypeEnum.Branch ||
                ApplicationController.Channel == ChannelTypeEnum.CallCenter)
            {
                //just check for session timeout
                if (SafePageController.Customer.IsAuthenticated)
                {
                    if (!(SafePageController.Performer.Agent != null && SafePageController.Performer.Agent.IsFrontOffice && SafePageController.Performer.Agent.Roles != null))
                        CheckSessionTimeOut();
                }

                return;
            }

            string applicationPath = Request.ApplicationPath;
            if (string.IsNullOrEmpty(applicationPath))
                throw new VPSystemException(CHECK_USER_ACCESS_RIGHT, VpBasePageConstants.APP_PATH_IS_NULL_OR_EMPTY);

            RolePageAccess rolePageAccess = EnsureRolePageAccessInstance();

            string url = string.Empty;
            if (applicationPath.Length == 1)
                url = rolePageAccess.ToLower(
                    //Request.Url.AbsolutePath
                    Request.CurrentExecutionFilePath
                    .Substring(applicationPath.Length));
            else
                url = rolePageAccess.ToLower(
                    //Request.Url.AbsolutePath
                    Request.CurrentExecutionFilePath
                    .Substring(applicationPath.Length + 1));

            if (SafePageController.Customer.IsAuthenticated)
            {
                if (SafePageController.Performer.Agent != null && SafePageController.Performer.Agent.IsFrontOffice && SafePageController.Performer.Agent.Roles != null)
                {
                    //Check rights after user identified (has logged in for FrontOfficeAgent)
                    bool doesAgentHaveAccess = DoesAgentHaveAccess(rolePageAccess, url);
                    if (!doesAgentHaveAccess)
                        throw new VPBusinessException(CHECK_USER_ACCESS_RIGHT, IExceptions.UserDoesntHaveAccessRight);
                }
                else
                {
                    CheckSessionTimeOut();

                    //Check rights after user identified (has logged in)
                    if (!rolePageAccess.HasRight(SafePageController.Customer.User, SafePageController.Customer.User.RoleId, url, LoginToken))
                    {
                        SafePageController.SetStateValue(VpBasePageConstants.WARNING_DISPLAY_MESSAGE, ResourceHelper.DoGlobalizationResourcesInquiry(IExceptions.UserDoesntHaveAccessRight));
                        SafePageController.PageControllerRedirect(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.WARNING_PAGEURL).ToString());
                    }
                }
            }
            else
            {
                //User = 0 and Role = UserRoleConstants.UNDEFINED(0) needs to able to access Login page
                if (!rolePageAccess.HasRight(null, UserRoleConstants.UNDEFINED, url, LoginToken))
                    throw new VPBusinessException(CHECK_USER_ACCESS_RIGHT, IExceptions.UserDoesntHaveAccessRight);
            }
        }

        /// <summary>
        /// Tells if the agent have access to the given url
        /// </summary>
        /// <param name="rolePageAccess">The role page access.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private bool DoesAgentHaveAccess(RolePageAccess rolePageAccess, string url)
        {
            return SafePageController.Performer.Agent.Roles.Any(agentRole => rolePageAccess.HasRight(null, agentRole, url, LoginToken));
        }

        /// <summary>
        /// Gets the role page access.
        /// </summary>
        /// <returns></returns>
        private RolePageAccess GetRolePageAccess()
        {
            VPModuleRoleInquiryResponse pagesAllowed = null;
            VPModulePageInquiryResponse modulePages = null;

            try
            {
                pagesAllowed = UserHelper.GetApplicationModulesAllowed();
                modulePages = UserHelper.GetApplicationModulePages();
            }
            catch (Exception ex)
            {
                throw new Exception(GET_ROLE_PAGE_ACCESS, ex);
            }

            TimeSpan pageRoleAuthRefreshRate = ReadPageRoleAuthRefreshRate();
            return new RolePageAccess(pagesAllowed, modulePages, pageRoleAuthRefreshRate);
        }

        /// <summary>
        /// Reads the page role auth refresh rate param.
        /// </summary>
        /// <returns></returns>
        private TimeSpan ReadPageRoleAuthRefreshRate()
        {
            try
            {
                string pageRoleAuthRefreshSeconds = VpConfigurationParameters.GetSpesificParameter(VpBasePageConstants.PAGE_ROLE_AUTH_REFRESH_SECONDS);
                return new TimeSpan(0, 0, Convert.ToInt32(pageRoleAuthRefreshSeconds));
            }
            catch (VPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new VPSystemException(READ_ROLE_AUTH_REFRESH_RATE, ex);
            }
        }

        /// <summary>
        /// Sets the exception logging ticket.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void SetExceptionLoggingTicket(VPBusinessException ex)
        {
            SafePageController.LoggingTicket = ApplicationInitializer.LogException(SET_EXCEPTION_LOGGING_TICKET, ex).ToString();
            SafePageController.ExceptionReferenceNumber = SafePageController.LoggingTicket;
        }

        /// <summary>
        /// Sets the exception help link.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void SetExceptionHelpLink(Exception ex)
        {
            //if (SafePageController.RequestHeader != null && SafePageController.RequestHeader.UniqueKey != null)
            //    ex.HelpLink += string.Format("({0})", SafePageController.RequestHeader.UniqueKey);
            //else
            ex.HelpLink = SafePageController.LoggingTicket;
        }

        /// <summary>
        /// Loads the exception information.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public virtual void LoadExceptionInformation(VPException ex)
        {
            //this method must be absract but it has been added this class later.
        }

        /// <summary>
        /// Sets the empty header label.
        /// </summary>
        public void SetEmptyHeaderLabel()
        {
            SetHeaderLabel(string.Empty);
        }

        protected string ReversalReferenceNumber
        {
            get
            {
                return Request.QueryString["ReversalReferenceNumber"];
            }
        }

        protected bool ForReversal
        {
            get
            {
                return Request.QueryString["ForReversal"] == "1";
            }
        }

        protected bool IsApprovalRequest
        {
            get
            {
                return Request.QueryString["IsApprovalRequest"] == "1";
            }
        }

        protected bool AdviceData
        {
            get
            {
                return Request.QueryString["AdviceData"] == "1";
            }
        }
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!IsPostBack)
            {
                if (ForReversal || IsApprovalRequest)
                {
                    Page.MasterPageFile = "~/MasterPage/Popup.master";
                    if (Page.Master is VeriBranchCommonMasterPage)
                    {
                        if (Page is IExecutePage)
                        {
                            (Page.Master as VeriBranchCommonMasterPage).SetForReversal();

                            var _NavigationControl = NavigationControl;
                            if (_NavigationControl != null)
                                _NavigationControl.Visible = false;
                        }
                    }
                }

                (new ControlHideTabHelper() { LoginToken = LoginToken }).ConfigureHideAndTab(this);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "decorateUIOnpartialpostback", "if(typeof(decorateUI) == 'function') decorateUI();", true);

            if (!IsPostBack)
            {
                (new ShortCutHelper() { LoginToken = LoginToken }).ConfigureShortCutKeys(this);
            }
        }

        /// <summary>
        /// Gets the referrer.
        /// </summary>
        /// <returns></returns>
        private Uri GetReferrer()
        {
            return Request.UrlReferrer;
        }

        /// <summary>
        /// Checks the URL referrer.
        /// </summary>
        protected virtual void CheckUrlReferrer()
        {
            // No referer check for Front Office
            if (SafePageController != null && SafePageController.Performer.Agent != null)
                return;

            //If the call is an ajax call
            if (Request.Headers[VpBasePageConstants.XMICROSOFT_AJAX] != null)
                return;

            Uri referrerUri = GetReferrer();
            string referredUriString = referrerUri.Host.ToLowerInvariant();

            string discardedHostConfiguration = VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.DISCARDED_HOSTS_FOR_CHECK_URL);
            if (string.IsNullOrEmpty(discardedHostConfiguration))
                return;

            string[] discardedHosts = discardedHostConfiguration.Split(',');
            if (discardedHosts.ToList().Contains(referredUriString))
                return;

            if (referredUriString != Request.Url.Host.ToLowerInvariant() || !referrerUri.Port.Equals(Request.Url.Port))
            {
                SafePageController.SetStateValue(VeribranchBasePageConstants.WARNING_DISPLAY_MESSAGE, GetLocalResource(VpBasePageConstants.SESSION_EXPIRED_MSG));
                SafePageController.PageControllerRedirect(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.SESSION_TIMEOUT_PAGEURL));
            }
        }

        /// <summary>
        /// Sets the header logo.
        /// </summary>
        protected sealed override void SetHeaderLogo()
        {
            if (Master == null)
                return;

            SetHeaderBackground();
            SetAnnouncementBackground();
        }

        #region Div Backgrounds

        /// <summary>
        /// Sets the header background.
        /// </summary>
        private void SetHeaderBackground()
        {
            if (Master == null)
                return;

            // If div not found, return:
            HtmlContainerControl headerBackground = Master.FindControl(VpBasePageConstants.HEADER_BACKGROUND) as HtmlContainerControl;
            if (headerBackground == null)
                return;

            // Set div background
            var imagePath = GetThemeBasedImagePath();
            SetDivBackground(headerBackground, imagePath);
        }

        /// <summary>
        /// Gets the theme based image path.
        /// </summary>
        /// <returns></returns>
        private string GetThemeBasedImagePath()
        {
            string imagePath = string.Empty;

            // Get theme-based image path:
            if (SafePageController.Customer.User != null && Page.Theme != DefaultTheme)
                imagePath = string.Format(VpConfigurationParameters.GetGenericParameter(VpBasePageConstants.HEADER_IMAGEURL), Page.Theme);
            else
                imagePath = GetGlobalResource(VpBasePageConstants.IMAGES_URLS, VpBasePageConstants.HEADER);

            return Request.ApplicationPath + imagePath;
        }

        /// <summary>
        /// Sets the announcement background.
        /// </summary>
        private void SetAnnouncementBackground()
        {
            if (Master == null)
                return;

            HtmlContainerControl announcementDiv = Master.FindControl(VpBasePageConstants.ANNOUNCEMENT_DIV) as HtmlContainerControl;
            if (announcementDiv == null)
                return;

            string imagePath = Request.ApplicationPath + GetGlobalResource(VpBasePageConstants.IMAGES_URLS, VpBasePageConstants.SPOTLIGHT);
            SetDivBackground(announcementDiv, imagePath);
        }

        /// <summary>
        /// Sets the div background.
        /// </summary>
        /// <param name="div">The div.</param>
        /// <param name="imagePath">The image path.</param>
        private void SetDivBackground(HtmlContainerControl div, string imagePath)
        {
            if (div == null)
                throw new VPSystemException(SET_DIV_BACKGROUND, VPExceptionConstants.DIV_NULL);

            if (string.IsNullOrEmpty(imagePath))
                throw new VPSystemException(SET_DIV_BACKGROUND, VPExceptionConstants.IMAGE_PATH_NULL_OR_EMPTY);

            string value = string.Format("url({0})", imagePath);
            div.Style.Add(VpBasePageConstants.BACKGROUND_IMAGE, value);
        }

        #endregion

        #region Resources & Messages

        ///// <summary>
        ///// Gets the local resource value by key.
        ///// Return the key if the value is not found.
        ///// </summary>
        ///// <param name="key">The key.</param>
        ///// <returns></returns>
        //protected string GetLocalResource(string key)
        //{
        //    if (string.IsNullOrEmpty(key))
        //        return string.Empty;

        //    try
        //    {
        //        object value = GetLocalResourceObject(key);
        //        if (value == null)
        //            return key;

        //        return value.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.LogException(ex);
        //        return key;
        //    }
        //}

        ///// <summary>
        ///// Gets the error message.
        ///// </summary>
        ///// <param name="response">The response.</param>
        ///// <returns></returns>
        //protected string GetErrorMessage(ResponseTransactionData response)
        //{
        //    string errorMsg = string.Empty;

        //    if (response != null && response.Footer != null && response.Footer.Result != null && response.Footer.Result.Warnings.Warning != null)
        //    {
        //        foreach (ResultInstance result in response.Footer.Result.Warnings.Warning)
        //            errorMsg += result.Description + Environment.NewLine;
        //    }

        //    return errorMsg;
        //}

        /// <summary>
        /// Sets the control property value.
        /// </summary>
        /// <param name="controlName">Name of the control.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void SetControlPropertyValue(string controlName, string propertyName, bool value)
        {
            if (Master == null || string.IsNullOrEmpty(controlName) || string.IsNullOrEmpty(propertyName))
                return;

            Control contentPanel = Master.FindControl(controlName);
            if (contentPanel == null)
                return;

            PropertyInfo property = contentPanel.GetType().GetProperty(propertyName);
            if (property == null)
                return;

            property.SetValue(contentPanel, value, null);
        }

        #endregion

        public void HandlePostBack(Button btn)
        {
            if (!string.IsNullOrEmpty(btn.Attributes["onclick"]))
            {
                return;
            }
            #region Postback handler
            //SOLUTION 2: CHANGE THE BUTTON SUBMIT JS CODE SO THAT IT
            //DISABLES WHEN THE FORM IS VALID, PRIOR TO SUBMISSION

            if (btn.CausesValidation)
            {
                //build the JS
                StringBuilder sb = new StringBuilder();
                sb.Append(" if (typeof(Page_ClientValidate) == 'function') { ");

                //var popup = "$(\"#validation_dialog\").dialog({title: \"Validation Error!\",modal: true,resizable: false,buttons: {Close: function () {$(this).dialog('close');}});";


                //if client-side does not validate, stop (this supports validation groups)
                //BUGFIX: must save, then restore the page validation / submission state, otherwise
                //when the validation fails, it prevents the FIRST autopostback from other controls
                sb.Append("var oldPage_IsValid = Page_IsValid; var oldPage_BlockSubmit = Page_BlockSubmit;");
                sb.Append("if (Page_ClientValidate('" + btn.ValidationGroup + "') == false) {");
                sb.Append(" Page_IsValid = oldPage_IsValid; Page_BlockSubmit = oldPage_BlockSubmit; showSummary" + btn.ValidationGroup + "(); return false; }} ");

                //change button text and disable it
                sb.AppendFormat("this.value = '{0}';", GetGlobalResource("Controls/NavigationButtonControl.ascx", "btnNextNavigation.Processing.Text"));
                sb.Append("this.disabled = true;");

                //insert the call to the framework JS code that does the postback of the form in the client
                //The default code generated by ASP (WebForm_DoPostbackWithOptions) will not 
                //submit because the button is disabled (this is new in 2.0)
                sb.Append(ClientScript.GetPostBackEventReference(btn, null) + ";");

                sb.Append("if(typeof(showOverlay) != 'undefined') {showOverlay();}");

                //BUGFIX: MUST RETURN AFTER THIS, OTHERWISE IF THE BUTTON HAS UseSubmitBehavior=false
                //THEN ONE CLICK WILL IN FACT CAUSE 2 SUBMITS, DEFEATING THE WHOLE PURPOSE
                sb.Append("return true;");

                var script = string.Empty;
                if (!string.IsNullOrEmpty(btn.Attributes["onclick"]))
                    script = btn.Attributes["onclick"];

                script += sb.ToString();

                btn.Attributes.Add("onclick", script);
            }
            else
            {
                //build the JS
                StringBuilder sb = new StringBuilder();

                //change button text and disable it
                sb.AppendFormat("this.value = '{0}';", GetGlobalResource("Controls/NavigationButtonControl.ascx", "btnNextNavigation.Processing.Text"));
                sb.Append("this.disabled = true;");

                //insert the call to the framework JS code that does the postback of the form in the client
                //The default code generated by ASP (WebForm_DoPostbackWithOptions) will not 
                //submit because the button is disabled (this is new in 2.0)
                sb.Append(ClientScript.GetPostBackEventReference(btn, null) + ";");

                sb.Append("if(typeof(showOverlay) != 'undefined') {showOverlay();}");

                //BUGFIX: MUST RETURN AFTER THIS, OTHERWISE IF THE BUTTON HAS UseSubmitBehavior=false
                //THEN ONE CLICK WILL IN FACT CAUSE 2 SUBMITS, DEFEATING THE WHOLE PURPOSE
                sb.Append("return true;");

                btn.Attributes.Add("onclick", sb.ToString());
            }

            #endregion
        }

        protected void GridView_SaveStateEvent(object obj)
        {
            SafePageController.SetStateValue(ClientID + "_DataSource", obj);
        }

        protected object GridView_LoadStateEvent()
        {
            return SafePageController.GetStateValue(ClientID + "_DataSource");
        }

        [WebMethod]
        [Obsolete("Use javascript method getAmountCurrencyFormat defined in AmountToWords.js")]
        public static string GetAmountCurrencyFormat(string amount, string currency, bool appendCurrencyCode, string loginToken)
        {
            try
            {
                var helper = new ServicesHelper();
                helper.LoginToken = loginToken;

                decimal result = 0;
                if (decimal.TryParse(amount, out result))
                    return helper.GetAmountInCurrencyFormat(result, currency, appendCurrencyCode);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
            }
            return string.Empty;
        }

        [WebMethod]
        public static string UpdateTransactionNotes(string referenceNumber, string notes)
        {
            var helper = new ServicesHelper {LoginToken = ""};

            return helper.UpdateTransactionNotes(referenceNumber, notes) ? "1" : "0";
        }

        [WebMethod]
        public static string LogManualRecoveryTCROperations(string amount, string currencyCode, string identifier)
        {
            string exceptionTicketNumber = string.Empty;
            try
            {
                string message = "Transaction having reference number:[" + identifier + "] , amount:[" + amount + "] , currency code:[" + currencyCode + "] is marked for manual recovery.";
                exceptionTicketNumber = LogManager.LogException("LogManualRecoveryTCROperations", message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
            }
            return exceptionTicketNumber;
        }

        [WebMethod]
        public static string VerifyOTPAndApproveTransaction(string otp, string approvalType, string transactionName, string crmCaseNumber, string transactionReferenceNumber, string nonStpTransactionID, string loginToken, bool isRetry)
        {
            var errorString = string.Empty;

            try
            {
                var request = new VPVerifyOTPOperationRequest();
                request.TransactionName = transactionName;
                request.OTPHash = otp;
                request.IsRetry = isRetry;

                OTPApprovalType otpType;
                Enum.TryParse(approvalType, true, out otpType);

                request.OTPType = otpType;
                request.ApproveCrmCaseIfVerified = otpType == OTPApprovalType.Manager;

                request.CrmCaseNumber = crmCaseNumber;
                request.TransactionReferenceNumber = transactionReferenceNumber;
                request.NonStpTransactionID = long.Parse(nonStpTransactionID);

                var helper = new SecurityHelper();
                helper.LoginToken = loginToken;

                var response = helper.VerifyOTPOperation(request);
                if (response != null && response.IsSuccess)
                    errorString = "";
                else
                {
                    if (response.Footer != null && response.Footer.Result != null && response.Footer.Result.Error != null)
                    {
                        if (response.Footer.Result.Error.IsDoubtfulTxn)
                            errorString = string.Format("D|{0}", response.Footer.Result.Error.Description);

                        else if (!response.IsVerified)
                            errorString = string.Format("O|{0}", response.Footer.Result.Error.Description);
                        else
                            errorString = response.Footer.Result.Error.Description;
                    }
                    else
                    {
                        errorString = "Unable to validate OTP.";    //TODO - fetch resource value from globalization.
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                errorString = ex.Message;
            }

            return errorString;
        }

        [WebMethod]
        public static string GetAuthorizers(string strTxnAmount, string txnName, string loginToken)
        {
            var result = new StringBuilder();
            decimal txnAmount = 0;
            decimal.TryParse(strTxnAmount, out txnAmount);
            var queueUsersResponse = new UserHelper() { LoginToken = loginToken }.DoBranchQueueUsersInqury(txnName);
            var pageController = PageController.GetCurrent(loginToken);
            if (queueUsersResponse.IsSuccess)
            {
                if (queueUsersResponse.QueueUsers != null)
                {
                    foreach (
                        var user in
                            queueUsersResponse.QueueUsers.Where(user => user.AuthorizerLimit >= txnAmount && (true && user.DomainName != pageController.TransactionHeader.PerformerIdentity)).ToList())
                    {
                        result.AppendFormat("{0}~{1}#{2}#{3}|", user.FullName, user.MobilePhone, user.DomainName, user.InternalEmail);
                    }

                    //Set BranchQueueUsersInqury in page controller. QueueUsers should be validated in SendOTP method.
                    pageController.SetProfileValue("BranchQueueUsersInqury", queueUsersResponse);
                }
                if(result.ToString() == string.Empty)
                    result.Append("e");
            }
            else
            {
                result.Append("x");
            }
            return result.ToString();
        }

        [WebMethod]
        public static string CancelCrmCase(string crmCaseNumber, string nonStpTransactionID, string loginToken)
        {
            var errorString = string.Empty;

            try
            {
                var request = new VPCloseCrmCaseRequest();
                request.CrmCaseNumber = crmCaseNumber;
                request.NonStpTransactionID = long.Parse(nonStpTransactionID);
                request.CloseCaseAction = CloseCRMCaseOperationAction.Cancel;

                var helper = new TransactionUtilitiesHelper();
                helper.LoginToken = loginToken;

                var response = helper.DoCloseCrmCaseOperation(request);
                if (response != null && response.IsSuccess)
                    errorString = "";
                else
                {
                    if (response.Footer != null && response.Footer.Result != null && response.Footer.Result.Error != null)
                        errorString = response.Footer.Result.Error.Description;
                    else
                    {
                        errorString = "Unable to cancel transaction.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                errorString = ex.Message;
            }

            return errorString;
        }

        [WebMethod]
        public static string FailCrmCase(string crmCaseNumber, string nonStpTransactionID, string loginToken)
        {
            var errorString = string.Empty;

            try
            {
                var request = new VPCloseCrmCaseRequest();
                request.CrmCaseNumber = crmCaseNumber;
                request.NonStpTransactionID = long.Parse(nonStpTransactionID);
                request.CloseCaseAction = CloseCRMCaseOperationAction.Fail;

                var helper = new TransactionUtilitiesHelper();
                helper.LoginToken = loginToken;

                var response = helper.DoCloseCrmCaseOperation(request);
                if (response != null && response.IsSuccess)
                    errorString = "";
                else
                {
                    if (response.Footer != null && response.Footer.Result != null && response.Footer.Result.Error != null)
                        errorString = response.Footer.Result.Error.Description;
                    else
                    {
                        errorString = "Unable to close transaction.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                errorString = ex.Message;
            }

            return errorString;
        }

        [WebMethod]
        public static string SendScannedDocumentsToCRM(string transactionReferenceNumber, string strDocType, string strDocTypeName, string loginToken, bool convertToPdf)
        {
            //scan the local upload folder to get the saved files and proceed to upload
            var returnMessage = "Upload document failed. Please close the dialog and try again";

            //TraceManager.Write("Entered SendScannedDocumentsToCRM");
            //TraceManager.Write(string.Format("transactionReferenceNumber: {0}", transactionReferenceNumber));
            //TraceManager.Write(string.Format("strDocType: {0}", transactionReferenceNumber));
            //TraceManager.Write(string.Format("strDocTypeName: {0}", transactionReferenceNumber));
            //TraceManager.Write(string.Format("loginToken: {0}", transactionReferenceNumber));
            //TraceManager.Write(string.Format("convertToPdf: {0}", transactionReferenceNumber));

            var uploadDirectory = string.Format("{0}\\Scanner\\UploadedImages\\{1}", HostingEnvironment.MapPath("~"), transactionReferenceNumber);
            try
            {

                //TraceManager.Write(string.Format("uploadDirectory: {0}", uploadDirectory));

                var userHelper = new UserHelper();
                userHelper.LoginToken = loginToken;

                var documentHelper = new DocumentHelper();
                documentHelper.LoginToken = loginToken;

                var reportHelper = new ReportHelper() { LoginToken = loginToken };

                var controller = PageController.GetCurrent(loginToken);

                var txnRequest = controller.GetStateValue("OriginalTransactionRequest") as RequestTransactionData ??
                                 controller.RequestData;

                var txnResponse = controller.GetStateValue("OriginalTransactionResponse") as ResponseTransactionData ??
                                  controller.GetStateValue("response") as ResponseTransactionData;

                //TraceManager.Write(string.Format("Before entering foreach on Directory.GetFiles(uploadDirectory)"));

                foreach (var fullFileName in Directory.GetFiles(uploadDirectory))
                {
                    //TraceManager.Write(string.Format("Entered foreach for fullFileName: {0}", fullFileName));

                    var fileName = Path.GetFileName(fullFileName);

                    //TraceManager.Write(string.Format("just filename: {0}", fileName));

                    byte[] fileData;
                    if (convertToPdf)
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";
                        try
                        {
                            fileData = documentHelper.ConvertImageToPdf(fullFileName);
                        }
                        catch (Exception)
                        {
                            fileData = File.ReadAllBytes(fullFileName);
                        }
                    }
                    else
                    {
                        fileData = File.ReadAllBytes(fullFileName);
                    }

                    var blob = new Blob()
                    {
                        FileName = string.Format("{0}-{1}-{2}{3}", transactionReferenceNumber, strDocType, DateTime.Now.ToString(ReportTimestamp, Ci), Path.GetExtension(fileName)),
                        Data = fileData,
                    };

                    //TraceManager.Write(string.Format("Creating request object VPSPDocumentUploadOperationRequest..."));
                    var request = new VPSPDocumentUploadOperationRequest()
                    {
                        FileForUpload = new List<Blob>() { blob },
                        TransactionReferenceNumber = transactionReferenceNumber,
                        FieldInformationList = reportHelper.GetSpDocumentFieldInformation(txnRequest, txnResponse)
                    };

                    //SAVE to sharepoint
                    //TraceManager.Write(string.Format("Before calling DoDocumentUploadToCrmOperation..."));
                    var response = userHelper.DoDocumentUploadToCrmOperation(request);
                    //TraceManager.Write(string.Format("After calling DoDocumentUploadToCrmOperation..."));

                    //TraceManager.Write(string.Format("response.IsSuccess: {0}", response.IsSuccess));

                    if (response != null && response.IsSuccess)
                        returnMessage = "OK";
                    else if (response != null && response.Footer != null && response.Footer.Result != null && response.Footer.Result.Errors != null && response.Footer.Result.Errors.FirstOrDefault() != null && !string.IsNullOrEmpty(response.Footer.Result.Errors.FirstOrDefault().Description))
                    {
                        returnMessage = response.Footer.Result.Errors.FirstOrDefault().Description;
                        
                        //TraceManager.Write(string.Format("response.Footer.Result.Errors.FirstOrDefault().Description: {0}", response.Footer.Result.Errors.FirstOrDefault().Description));
                    }

                    //remove uploaded file
                    //TraceManager.Write(string.Format("Before deleting {0}...", fullFileName));
                    File.Delete(fullFileName);
                    //TraceManager.Write(string.Format("After deleting {0}...", fullFileName));
                }

                //cleanup directory after upload
                //TraceManager.Write(string.Format("Before deleting directory {0}...", uploadDirectory));
                Directory.Delete(uploadDirectory);
                //TraceManager.Write(string.Format("After deleting directory {0}...", uploadDirectory));
            }
            catch (Exception ex)
            {
                if (Directory.Exists(uploadDirectory))
                {
                    foreach (var fullFileName in Directory.GetFiles(uploadDirectory))
                    {
                        if (File.Exists(fullFileName))
                        {
                            File.Delete(fullFileName);
                        }
                    }
                    Directory.Delete(uploadDirectory);
                }

                //TraceManager.Write(string.Format("Entered catch (Exception ex)"));
                //TraceManager.Write(string.Format("{0}", ex.ToString()));
                returnMessage = "Upload document failed. Please close the dialog and try again.";
                LogManager.LogToEventViewer(ex);
                LogManager.LogException(ex);

            }

            //TraceManager.Write(string.Format("Returned value: {0}", string.Format("{0}|{1}|{2}", returnMessage, strDocType, strDocTypeName)));
            return string.Format("{0}|{1}|{2}", returnMessage, strDocType, strDocTypeName);
        }

        [WebMethod]
        public static string SendOTP(string approvalType, string mobileNumber, string email, string domainName, string transactionName, string transactionReferenceNumber, string loginToken, Dictionary<string,object> keyValuePairs)
        {
            var errorString = string.Empty;

            try
            {
                //Verifiy if mobileNumber and email belongs to correct domainName
                var pageController = PageController.GetCurrent(loginToken);
                var queueUsersResponse = pageController.GetProfileValue("BranchQueueUsersInqury") as VPBranchQueueUsersInquiryResponse;

                if (queueUsersResponse != null && queueUsersResponse.QueueUsers != null)
                {

                    foreach (
                             var user in
                                 queueUsersResponse.QueueUsers.Where(user => user.DomainName != pageController.TransactionHeader.PerformerIdentity).ToList())
                    {

                        if (user.DomainName == domainName)
                        {
                            if (
                                (user.MobilePhone != null && user.MobilePhone != mobileNumber) ||
                                 (user.InternalEmail != null && user.InternalEmail != email)
                                )
                            {
                                errorString = "User Mobile Number or Email address do not match with the provided data. User validation failed.";
                                return errorString;
                            }
                            break;
                        }
                    }
                }


                var request = new VPSendOTPOperationRequest();
                request.MobileNumber = mobileNumber;
                request.EmailAddress = email;
                request.TransactionName = transactionName;

                OTPApprovalType otpType;
                Enum.TryParse(approvalType, true, out otpType);

                request.UpdateTransactionOTPInfo = otpType == OTPApprovalType.Manager;
                request.OTPType = otpType;

                if (otpType == OTPApprovalType.Customer && keyValuePairs != null)
                {
                    request.CustomLogEntryList = keyValuePairs.Select(vpLogEntry => new VPLogEntry {Key = vpLogEntry.Key, Value = vpLogEntry.Value}).ToArray();
                }

                request.TransactionReferenceNumber = transactionReferenceNumber;
                request.RecieverId = domainName;

                var helper = new SecurityHelper();
                helper.LoginToken = loginToken;

                var response = helper.SendOTPOperation(request);
                if (response != null && response.IsSuccess)
                    errorString = "";
                else
                {
                    if (response.Footer != null && response.Footer.Result != null && response.Footer.Result.Error != null)
                        errorString = response.Footer.Result.Error.Description;
                    else
                    {
                        errorString = "Unable to send OTP.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                errorString = ex.Message;
            }

            return errorString;
        }
    }
}
