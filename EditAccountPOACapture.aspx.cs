using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using VeriBranch.Business.BackOfficeOperations;
using VeriBranch.Business.Managers;
using VeriBranch.Common.Constants;
using VeriBranch.Common.MessageDefinitions;
using VeriBranch.WebApplication.UIProcess;
using System;
using Utilities.ConfigurationUtilities;
using System.Text.RegularExpressions;
using Utilities.Diagnostics;
using Veripark.Data;
using VeriBranch.Common.Exceptions;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Web;

/*[AutoInsertCode:AssemblyVersionInfo]*/


public partial class EditAccountPOACapture : VeriBranchBasePage
{
    private string CaptureSelectedOption
    {
        get
        {
            if (pageController.GetStateValue("CaptureOption") != null)
                return pageController.GetStateValue("CaptureOption").ToString();
            else
                return CaptureOption.Account;
        }
    }

    private static string previousValues = string.Empty;
    /// <summary>
    /// All web pages headers are set.
    /// </summary>
    protected override void LocalizePageContent()
    {
        SetHeaderLabel(GetLocalResourceObject("Header").ToString());

        AddidentityExpiryDatePicker.TextboxWidth = 246;
        MemoText.Width = 250;
        ddlRelationShip.Width = 250;
    }
    /// <summary>
    /// Web pages are getted all information by the state.
    /// </summary>
    protected override void GetStateFromUI()
    {
        //pageController.RequestData = GetSignatoryDetails();
    }

    /// <summary>
    /// The information that are entered by user in the web page are set the state for using other web pages.
    /// </summary>
    protected override void SetUIFromState()
    {
        //SetSignatoryDetails();
    }

    /// <summary>
    /// Functional transaction are made in the web pages.
    /// </summary>
    protected override void DoPageAction()
    {
        try
        {
            //LoadNationalityDDL();
            //LoadIdentityTypeDDL();

            //Search
            ctlIdentityType.TypeDomain = LookupNames.IdentityDocumentType;
            ctlNationality.TypeDomain = LookupNames.NationalityCodes;

            //Add
            AddmessageRow.Visible = false;
            AddctlIdentityType.TypeDomain = LookupNames.IdentityDocumentType;
            AddctlNationality.TypeDomain = LookupNames.NationalityCodes;

            accountNumberRow.Visible = CaptureSelectedOption == CaptureOption.Account;
            lblAccountNumberText.Text = AccounNumber;
            lblAccountNameText.Text = AccounName;

            LoadCorporteDDL();
            LoadDropDownFromEnum(AddchannelDDL, typeof(ChannelTypeEnum));
            LoadDropDownFromEnum(AdduserTypeDDL, typeof(CommonUserType));

            //SignatoryStatusEnum
            LoadDropDownFromEnum(ddlSignatoryStatus, typeof(SignatoryStatusEnum));

            lblWarning.Visible = false;

            var userID = long.Parse(pageController.GetTransferValue("signatoryUserID").ToString());
            SignatureVerificationManager svManager = new SignatureVerificationManager();
            DataTable vbUserTable = svManager.GetUserDetailsByAccountNoUserID(AccounNumber, true, StaggingIDs, userID);

            DataTable dataTable = GetTypeData(true);
            ddlRelationShip.DataSource = dataTable;
            ddlRelationShip.DataTextField = "TypeDataName";
            ddlRelationShip.DataValueField = "TypeDataId";
            ddlRelationShip.DataBind();

            SetSignatoryDetailsfromDT(vbUserTable);
            if (IsCore)
            {
                UpdateEnableStatus(false);
            }
        }
        catch (Exception ex)
        {
            string text = ctlIdentityType.Text;
        }
    }


    public void AddctlIdentityType_CMBSelectChanged(object sender, EventArgs e)
    {
        if (AddctlIdentityType.SelectedValue == TypeDataIDs.IdentityDocumentType_National_Identity)
        {
            AddidentityNumberText.MaxLength = 10;
        }
        else if (AddctlIdentityType.SelectedValue == TypeDataIDs.IdentityDocumentType_Iqama)
        {
            AddidentityNumberText.MaxLength = 10;
        }
        else if (AddctlIdentityType.SelectedValue == TypeDataIDs.IdentityDocumentType_GCC_Identity)
        {
            AddidentityNumberText.MaxLength = 15;
        }
        else
        {
            AddidentityNumberText.MaxLength = 50;
        }

    }


    #region Search


    /// <summary>
    /// Handles the Click event of the btnSearchAccount control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
    protected void btnSearchAccount_Click(object sender, ImageClickEventArgs e)
    {
        try
        {
            SearchAndDisplaySignatories(AccounNumber, identityNumberText.Text, ctlIdentityType.SelectedValue,
                                        ctlNationality.SelectedValue);
        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.btnSearchAccount_Click"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }

    /// <summary>
    /// Query Database to fetch the Signatory based on given parameters
    /// </summary>
    /// <param name="accountNumber">Account Number.</param>
    /// <param name="identityNumber">Identity Number.</param>
    /// /// <param name="identityType">Identity Type.</param>
    /// /// <param name="nationality">Nationality.</param>
    private void SearchAndDisplaySignatories(string accountNumber, string identityNumber, string identityType, string nationality)
    {
        try
        {
            SignatureVerificationManager svManager = new SignatureVerificationManager();
            DataTable dtSignatories = svManager.GetSignatoryForPOA(accountNumber, identityNumber, identityType, nationality, false);
            signatoryGrid.DataSource = dtSignatories;
            signatoryGrid.DataBind();

            trNewUsers.Visible = (dtSignatories.Rows.Count > 0);
        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
            {
                InnerException = ex,
                Origin = "AccountPOACapture.SearchAndDisplaySignatories"
            };
            LogManager.LogException(vpse);
            throw vpse;
        }

    }

    /// <summary>
    /// Gets the no signatory selected MSG.
    /// </summary>
    /// <value>
    /// The no signatory selected MSG.
    /// </value>
    protected string NoSignatorySelectedMsg
    {
        get
        {
            return GetLocalResourceObject("NoSignatorySelectedMsg").ToString();
        }
    }


    /// <summary>
    /// Handles the Click event of the btnSaveAndClose control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
    protected void btnSaveAndClose_Click(object sender, ImageClickEventArgs e)
    {
        try
        {
            SignatureVerificationManager svManager = new SignatureVerificationManager();
            string typeDataID =
                svManager.GetIdentityTypes(TypeDomainIDs.AccountSignatoryRelation,
                                           TypeDataIDs.AccountSignatoryRelation_POA).Rows[0]["TypeDataID"].ToString();
            bool changed = false;
            long corporateID = 0;
            long virtualCorporateID = 0;
            string corporateName = string.Empty;

            if (pageController.GetStateValue("CorporateID") != null)
                corporateID = (long)pageController.GetStateValue("CorporateID");
            if (pageController.GetStateValue("CorporateName") != null)
                corporateName = pageController.GetStateValue("CorporateName").ToString();
            if (pageController.GetStateValue("VirtualCorporateID") != null)
                virtualCorporateID = (long)pageController.GetStateValue("VirtualCorporateID");


            foreach (GridViewRow row in signatoryGrid.Rows)
            {
                GridView grid = (GridView)row.Parent.Parent;
                long userID = (long)grid.DataKeys[row.RowIndex].Value;
                string userFullName = grid.DataKeys[row.RowIndex].Values[1].ToString();
                // DataRow dataRow = (DataRow)row.DataItem;
                CheckBox chkSelected = (CheckBox)row.FindControl("chkSelected");

                if (chkSelected.Checked)
                {
                    //   svManager.SaveUserAccountRelation(AccounNumber, Convert.ToInt64(dataRow["UserID"]), typeDataID);
                    changed = true;
                    bool isNewStagging;
                    svManager.SaveUserAccountRelation(AccounNumber, userID, typeDataID, string.Empty, out isNewStagging);
                    //MakeUpdateCorporateTransaction(userID, userFullName, corporateID, corporateName);
                    MakeUpdateCorporateTransaction(userID, userFullName, virtualCorporateID, corporateName);

                    LogManager.LogAudit("User added - Account Number :" + AccounNumber + " - User ID:" + userID);
                }
            }

            if (changed)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "closeWindow",
                                                    "window.returnValue='1'; window.close(); ", true);
            else
                ScriptManager.RegisterStartupScript(this, this.GetType(), "closeWindow",
                                                    "window.returnValue='0'; window.close(); ", true);

        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.btnSaveAndClose_Click"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }

    private void SaveUserAccountRelation(string accountNumber, long userID)
    {
        SignatureVerificationManager svManager = new SignatureVerificationManager();
        string typeDataID = svManager.GetIdentityTypes(TypeDomainIDs.AccountSignatoryRelation, TypeDataIDs.AccountSignatoryRelation_POA).Rows[0]["TypeDataID"].ToString();

        VpSignatoryCaptureRequest captureRequest =
           ((VpSignatureCaptureProcessRequest)pageController.RequestData).SignatoryCaptureRequest;

        List<long> staggingIDs = new List<long>();
        if (captureRequest.UserAccountRelationStaggingIDs != null)
            staggingIDs = new List<long>(captureRequest.UserAccountRelationStaggingIDs);
        bool isNewStagging;
        long newID = svManager.SaveUserAccountRelation(accountNumber, userID, typeDataID, string.Empty, out isNewStagging);

        if (isNewStagging)
            staggingIDs.Add(newID);

        captureRequest.UserAccountRelationStaggingIDs = staggingIDs.ToArray();
    }

    /// <summary>
    /// Makes the update corporate transaction.
    /// </summary>
    /// <param name="signatoryID">The signatory ID.</param>
    /// <param name="signatoryName">Name of the signatory.</param>
    /// <param name="corporateID">The corporate ID.</param>
    /// <param name="corporateName">Name of the corporate.</param>
    private void MakeUpdateCorporateTransaction(long signatoryID, string signatoryName, long corporateID, string corporateName)
    {
        try
        {
            VpUpdateSignatoryCorporateRequest request = new VpUpdateSignatoryCorporateRequest();
            VpUpdateSignatoryCorporateResponse response = new VpUpdateSignatoryCorporateResponse();
            VpSignatoryCorporateUpdateTransaction transaction = new VpSignatoryCorporateUpdateTransaction();

            request.SignatoryID = signatoryID;
            request.SignatoryName = signatoryName;
            request.CorporateID = corporateID;
            request.CorporateName = corporateName;


            response = (VpUpdateSignatoryCorporateResponse)transaction.Execute(request, pageController.TransactionHeader);

        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.MakeUpdateCorporateTransaction"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }


    #endregion

    #region Add

    /// <summary>
    /// Sets the signatory details.
    /// </summary>
    private void SetSignatoryDetailsfromDT(DataTable userData)
    {
        try
        {
            if (userData.Rows.Count > 0)
            {
                AddctlIdentityType.SelectedValue = userData.Rows[0][6].ToString().Trim();
                AddidentityNumberText.Text = userData.Rows[0][4].ToString();
                if (userData.Rows[0][5] != null && !string.IsNullOrEmpty(userData.Rows[0][5].ToString().Trim()) && userData.Rows[0][5].ToString().Trim() != "00000000")
                {
                    AddidentityExpiryDatePicker.ChosenDate = (DateTime)userData.Rows[0][5];
                }
                else
                {
                    AddidentityExpiryDatePicker.ChosenDate = new DateTime(1900, 01, 01);
                }
                AddctlNationality.SelectedValue = userData.Rows[0][7].ToString().Trim();
                AddfullNameText.Text = userData.Rows[0][3].ToString();
                AdduserNameText.Text = userData.Rows[0][2].ToString();
                MemoText.Text = userData.Rows[0][8].ToString();
                ddlRelationShip.SelectedValue = userData.Rows[0][9].ToString().Trim();

                var signatoryStatus = userData.Rows[0][13] == null || userData.Rows[0][13] == DBNull.Value ? SignatoryStatusEnum.Inactive.ToString() : userData.Rows[0][13].ToString();

                ddlSignatoryStatus.SelectedValue = ((int)(Enum.Parse(typeof(SignatoryStatusEnum), signatoryStatus, true))).ToString();

                txtRemarks.Text = userData.Rows[0][14] == null || userData.Rows[0][14] == DBNull.Value ? string.Empty : userData.Rows[0][14].ToString();
                

                //Saving the current values of the above attributes to be used for case approval
                previousValues = userData.Rows[0][6].ToString().Trim() + ";" +
                    userData.Rows[0][4].ToString() + ";" +
                    ((userData.Rows[0][5] != null && !string.IsNullOrEmpty(userData.Rows[0][5].ToString()) && userData.Rows[0][5].ToString() != "00000000") ? ((DateTime)userData.Rows[0][5]).ToString("yyyy-MM-dd") : "") + ";" +
                    userData.Rows[0][7].ToString().Trim() + ";" +
                    userData.Rows[0][3].ToString() + ";" + userData.Rows[0][8].ToString() + ";" +
                    userData.Rows[0][9].ToString().Trim() + ";" +
                    signatoryStatus;
 

                AddidentityExpiryDatePicker_CMBSelectChanged(null, null);


                
            }
        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "EditAccountPOACapture.SetSignatoryDetailsfromDT"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }

    /// <summary>
    /// Sets the signatory details.
    /// </summary>
    private void SetSignatoryDetails()
    {
        CreateUserRequest request = (CreateUserRequest)pageController.RequestData;
        if (request != null)
        {
            AddcifNoText.Text = request.User.CifNo;
            AddctlIdentityType.SelectedValue = request.IdentityType;
            AddidentityNumberText.Text = request.IdentityNumber;
            AddidentityExpiryDatePicker.ChosenDate = request.IdentityExpiryDate;
            AddctlNationality.SelectedValue = request.Nationality;
            //dateOfBirthDatePicker.ChosenDate = request.User.Birthday;
            AddfullNameText.Text = request.User.Name;
            //AddlastNameText.Text = request.User.LastName;
            AdduserTypeDDL.SelectedValue = ((int)request.User.UserType).ToString();

            if (((CommonUserType)Enum.Parse(typeof(CommonUserType), request.User.UserType.ToString())) == CommonUserType.Corporate)
            {
                trCorporate.Visible = true;
                AddcorporateDDL.SelectedValue = request.User.CorporateID.ToString();
            }
            else
            {
                trCorporate.Visible = false;
            }


            AddemailText.Text = request.User.Email.ToString();
            AddmobileNumberText.PhoneNumber = request.User.MobileNumber.ToString();
            AdddepartmentText.Text = request.User.Department.ToString();
            AdduserNameText.Text = (string)request.User.UserName;
            //AddmemoText.Text = request.Memo.ToString();
        }

    }

    /// <summary>
    /// Gets the signatory details.
    /// </summary>
    /// <returns></returns>
    private VeriBranch.Common.MessageDefinitions.CreateUserRequest GetSignatoryDetails()
    {
        try
        {
            VeriBranch.Common.MessageDefinitions.CreateUserRequest createUserRequest =
                new VeriBranch.Common.MessageDefinitions.CreateUserRequest();
            createUserRequest.User = new User();
            createUserRequest.User.CifNo = AddcifNoText.Text;
            createUserRequest.User.CustomerName = AddfullNameText.Text;//+ " " + AddlastNameText.Text;
            //createUserRequest.User.Birthday = dateOfBirthDatePicker.ChosenDate;
            createUserRequest.User.Name = AddfullNameText.Text.Trim();

            if (AdduserTypeDDL.SelectedItem.Text == Convert.ToString(CommonUserType.Retail))
            {
                createUserRequest.User.CorporateID = 0;
                createUserRequest.User.UserType = CommonUserType.Retail;
                createUserRequest.User.Segment = SignatureVerificationConstants.RetailSegmentID;

                createUserRequest.SignatoryIsCorporate = false;
            }
            else if (AddcorporateDDL.SelectedValue != null)
            {
                createUserRequest.User.CorporateID = Convert.ToInt64(AddcorporateDDL.SelectedValue);
                createUserRequest.User.UserType = CommonUserType.Corporate;
                createUserRequest.User.CorporateName = AddcorporateDDL.SelectedItem.Text;
                createUserRequest.User.Segment = SignatureVerificationConstants.CororateSegmentID;

                createUserRequest.SignatoryIsCorporate = true;
            }

            createUserRequest.User.Email = AddemailText.Text;
            // createUserRequest.User.EmployeeId = employeeID.ToString();
            //createUserRequest.User.LastName = AddlastNameText.Text.Trim();
            createUserRequest.User.MobileNumber = AddmobileNumberText.NumberTextBox.Text;
            createUserRequest.User.Department = AdddepartmentText.Text;

            createUserRequest.User.UserName = (EncryptedData)userName; //userNameText.Text;

            //createUserRequest.AccountList = ConvertToCheckedAccountStatus(AccountHelper.GetAccountList());
            createUserRequest.User.LoginStateTypeId = (int)LoginStateTypeEnum.WaitingNewPasswordSet;
            createUserRequest.User.RoleId = FrontOfficeLoginRoleId;

            createUserRequest.User.ChannelID = Channel.GetChannelId(ChannelTypeEnum.Branch);
            // createUserRequest.EnrolledForIB = false;

            // createUserRequest.Memo = AddmemoText.Text;
            createUserRequest.IdentityNumber = AddidentityNumberText.Text;
            createUserRequest.IdentityExpiryDate = AddidentityExpiryDatePicker.ChosenDate;
            createUserRequest.Nationality = AddctlNationality.SelectedValue;
            createUserRequest.IdentityType = AddctlIdentityType.SelectedValue;

            createUserRequest.IsSignatory = true;

            pageController.SetStateValue("IdentityType", AddctlIdentityType.Text);
            pageController.SetStateValue("Nationality", AddctlNationality.Text);

            return createUserRequest;

        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.GetSignatoryDetails"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the userTypeDDL control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void AdduserTypeDDL_SelectedIndexChanged(Object sender, EventArgs e)
    {
        if (AdduserTypeDDL.SelectedItem.Text == CommonUserType.Retail.ToString())
        {
            trCorporate.Visible = false;
        }
        else
        {
            trCorporate.Visible = true;
        }
    }

    /// <summary>
    /// Loads the corporte DDL.
    /// </summary>
    private void LoadCorporteDDL()
    {
        try
        {
            SignatureVerificationManager svManager = new SignatureVerificationManager();
            DataTable corporateTable = svManager.GetCorporate(null, false, null);
            corporateTable.DefaultView.Sort = "Name";
            AddcorporateDDL.DataSource = corporateTable;
            AddcorporateDDL.DataTextField = "Name";
            AddcorporateDDL.DataValueField = "ID";
            AddcorporateDDL.DataBind();
        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.LoadCorporteDDL"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }

    }

    /// <summary>
    /// Loads the drop down from enum.
    /// </summary>
    /// <param name="dropDownList">The drop down list.</param>
    /// <param name="enumType">Type of the enum.</param>
    private void LoadDropDownFromEnum(DropDownList dropDownList, Type enumType)
    {
        Array valueArray = Enum.GetValues(enumType);
        DataRow newRow = null;
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Value", typeof(int));

        foreach (int value in valueArray)
        {
            newRow = dataTable.NewRow();
            newRow["Name"] = Enum.GetName(enumType, value);
            newRow["Value"] = value;

            dataTable.Rows.Add(newRow);
        }

        dropDownList.DataSource = dataTable;
        dropDownList.DataTextField = "Name";
        dropDownList.DataValueField = "Value";
        dropDownList.DataBind();

    }

    /// <summary>
    /// Handles the ServerValidate event of the cvCanConfirm control.
    /// </summary>
    /// <param name="source">The source of the event.</param>
    /// <param name="args">The <see cref="ServerValidateEventArgs" /> instance containing the event data.</param>
    protected void cvCanConfirm_ServerValidate(object source, ServerValidateEventArgs args)
    {
        try
        {
            if (base.IsPostBackComingFromNextButton())
            {
                string errors = string.Empty;
                args.IsValid = ValidateInputs(out errors);

                if (!args.IsValid)
                {
                    SetWarningDisplay(errors);
                }
            }

        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.cvCanConfirm_ServerValidate"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }

    }

    /// <summary>
    /// Updates the enable status.
    /// </summary>
    /// <param name="enabled">if set to <c>true</c> [enabled].</param>
    private void UpdateEnableStatus(bool enabled)
    {
        //AddcifNoText.Enabled =
        //AdddateOfBirthDatePicker.Enabled =
        //AddfullNameText.Enabled =
            //AddlastNameText.Enabled =
            //AdduserTypeDDL.Enabled =
            //AddcorporateDDL.Enabled =
            //AddemailText.Enabled =
            //AddmobileNumberText.NumberTextBox.Enabled =
            //AdddepartmentText.Enabled =
            //AdduserNameText.Enabled =
        //AddIdentityHijriExpiry.Enabled =
        AddidentityNumberText.Enabled =
        //AddidentityExpiryDatePicker.Enabled =
        AddctlIdentityType.Enabled =
        AddctlNationality.Enabled = enabled;
        // AddmemoText.Enabled = false;

    }

    /// <summary>
    /// Validates the inputs.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns></returns>
    private bool ValidateInputs(out string errors)
    {
        errors = string.Empty;
        bool returnValue = true;
        SignatureVerificationManager svManager = new SignatureVerificationManager();

        try
        {
            //if (svManager.GetUsers(
            //    userName//userNameText.Text
            //    , null, null).Rows.Count > 0)
            //{
            //    errors += GetLocalResourceObject("ErrorMsgUserNameAlreadyExists").ToString();//"Signatory with same name alreay exists";
            //    returnValue = false;
            //}

            //if (svManager.GetUsers(null, null, AddidentityNumberText.Text).Rows.Count > 0)
            //{
            //    errors += GetLocalResourceObject("ErrorMsgIdentityAlreadyExists").ToString();//"Signatory with same identity number alreay exists";
            //    returnValue = false;
            //}


            if (AddctlIdentityType.SelectedValue == TypeDataIDs.IdentityDocumentType_Iqama && !Regex.IsMatch(AddidentityNumberText.Text, VpConfigurationParameters.GetGenericParameter("Iqama.Format")))
            {
                errors += GetLocalResourceObject("AddIqamaFormatMsg").ToString();//"Iqama number is not in required format.";
                returnValue = false;
            }

            if (AddctlIdentityType.SelectedValue == TypeDataIDs.IdentityDocumentType_National_Identity && !Regex.IsMatch(AddidentityNumberText.Text, VpConfigurationParameters.GetGenericParameter("NationalID.Format")))
            {
                errors += GetLocalResourceObject("AddNationalIDFormatMsg").ToString();//"National identity number is not in required format.";
                returnValue = false;
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
            return false;
        }

        return returnValue;
    }

    /// <summary>
    /// Handles the Click event of the btnAddSignatory control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
    protected void btnAddSignatory_Click(object sender, ImageClickEventArgs e)
    {
        try
        {
            lblWarning.Text = string.Empty;
            string errors = string.Empty;
            bool IsValid = ValidateInputs(out errors);
            if (IsValid)
            {
                SignatureVerificationManager svManager = new SignatureVerificationManager();
                long userId = long.Parse(pageController.GetTransferValue("signatoryUserID").ToString());

                bool signatoryStatus = ddlSignatoryStatus.SelectedItem.Text == SignatoryStatusEnum.Active.ToString() ? true : false;

                new Database().ExecuteSPGetDataTable("[sp_Sig_UpdateUserForApproval]"
            , "@UserID", userId, "@AccountNumber", AccounNumber, "@OperationType", "E", "@EditedValues", previousValues, "@IsStagging", true, "@IsEnabled", signatoryStatus, "@Remarks", txtRemarks.Text);

                new Database().ExecuteSPGetDataTable("[sp_Insert_Audit]"
            , "@date", DateTime.Now,
            "@accountNumber", AccounNumber, 
            "@userID", userId,
            "@createdBy",pageController.TransactionHeader.User.PerformerName, 
            "@action", "Signatory Edited",
            "@actionValues", "Signatory New Values: \r\n Signatory Ref No: " + userId 
                            + ", Signatory Name: " + AddfullNameText.Text 
                            + ", Signatory ID: " + AddidentityNumberText.Text 
                            + ", Signatory ID Expiry: " + AddidentityExpiryDatePicker.ChosenDate 
                            + ", Signatory ID Type: " + AddctlIdentityType.SelectedValue 
                            + ", Signatory Nationality: " + AddctlNationality.SelectedValue 
                            + ", Signatory Status: " + ddlSignatoryStatus.SelectedItem.Text
                            + ", Update Reason: " + txtRemarks.Text
            , "@sessionID" ,pageController.GetStateValue("ProcessGuid").ToString()
            , "@caseReference", ""
            , "@caseCreatedBy", ""
            , "@caseStatus",""
            , "@caseHandledBy", "");

                svManager.UpdateUserPerson(AddfullNameText.Text, AddctlNationality.SelectedValue, AddidentityNumberText.Text, AddidentityExpiryDatePicker.ChosenDate, AddctlIdentityType.SelectedValue, userId, MemoText.Text, AccounNumber, ddlRelationShip.SelectedValue);
                //ScriptManager.RegisterStartupScript(this, this.GetType(), "closeWindow", "window.returnValue='1'; window.close(); ", true);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "closeWindow", "window.opener.postMessage(true,'*'); window.close(); ", true);

            }
            else
            {
                lblWarning.Text = errors;
                lblWarning.Visible = true;
                //return;
            }

        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.btnAddSignatory_Click"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }


    private string StaggingIDs
    {
        get
        {
            if (pageController.RequestData is VpSignatureCaptureProcessRequest)
            {
                VpSignatoryCaptureRequest captureRequest = ((VpSignatureCaptureProcessRequest)pageController.RequestData).SignatoryCaptureRequest;
                if (captureRequest.UserAccountRelationStaggingIDs != null)
                    return string.Join(",", captureRequest.UserAccountRelationStaggingIDs);
            }
            return string.Empty;
        }
    }

    private bool IsAccountSignatoryRelationFound(long signatoryID)
    {
        SignatureVerificationManager svManager = new SignatureVerificationManager();
        DataTable signtoriesTable = svManager.GetUserDetailsByAccountNo(AccounNumber, true, StaggingIDs);

        return signtoriesTable == null ? false : signtoriesTable.Select("UserId = " + signatoryID).Length > 0;
    }


    public void AddidentityExpiryDatePicker_CMBSelectChanged(object sender, EventArgs e)
    {
        var datenow = AddidentityExpiryDatePicker.ChosenDate;
        // ClientScript.RegisterStartupScript(GetType(), "id", "setHijriDate('" + datenow.ToString("dd/MM/yyyy") + "')", true);

        try
        {
            string soapRequest = "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">"
              + "<soap:Body>"
                 + "<ConvertGegorianDatetoHijriDate xmlns=\"http://tempuri.org/\">"
                   + "<greDate>" + datenow.ToString("yyyyMMdd") + "</greDate>"
                 + "</ConvertGegorianDatetoHijriDate>"
              + "</soap:Body>"
           + "</soap:Envelope>";
            UTF8Encoding encoding = new UTF8Encoding();

            var requestData = encoding.GetBytes(soapRequest);

            var url = VpConfigurationParameters.GetGenericParameter("DateConversionServiceURL").ToString();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Ssl3;
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            httpRequest.Headers.Add("SOAPAction", "\"http://tempuri.org/ConvertGegorianDatetoHijriDate\"");
            httpRequest.Method = "POST";
            httpRequest.KeepAlive = false;
            httpRequest.ContentType = "text/xml; charset=\"utf-8\"";
            httpRequest.ContentLength = requestData.Length;

            var timeout = Convert.ToInt32(VpConfigurationParameters.GetGenericParameter("DateConversionServiceTimeOut"));
            httpRequest.Timeout = timeout;

            HttpWebResponse httpResponse = null;
            String response = String.Empty;

            httpRequest.GetRequestStream().Write(requestData, 0, requestData.Length);
            httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream baseStream = httpResponse.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(baseStream);
            response = responseStreamReader.ReadToEnd();

            //string val = response.Substring(response.IndexOf("<ConvertGegorianDatetoHijriDateResult>") + 38, response.Length - response.IndexOf("</ConvertGegorianDatetoHijriDateResult>"));
            //val = val.Split(new char[] { '$' })[0];
            responseStreamReader.Close();
            XDocument doc = XDocument.Parse(response);
            var hijriDate = doc.Root.Value;
            DateTime dtHijri = new DateTime();
            bool sucess = false;
            if (!string.IsNullOrEmpty(hijriDate))
            {
                hijriDate = hijriDate.Split(new char[] { '$' })[0];
                sucess = DateTime.TryParseExact(hijriDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtHijri);
            }
            if (sucess)
            {
                AddIdentityHijriExpiry.Text = dtHijri.ToString("dd.MM.yyyy");
            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public void AddidentityHijriExpiryDatePicker_SelectChanged(object sender, EventArgs e)
    {
        try
        {
            DateTime hijriDate = new DateTime();
            bool success = DateTime.TryParseExact(AddIdentityHijriExpiry.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out hijriDate);
            if (success)
            {
                string soapRequest = "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">"
                  + "<soap:Body>"
                     + "<ConvertHijriDatetoGregDate xmlns=\"http://tempuri.org/\">"
                       + "<hijDate>" + hijriDate.ToString("yyyyMMdd") + "</hijDate>"
                     + "</ConvertHijriDatetoGregDate>"
                  + "</soap:Body>"
               + "</soap:Envelope>";
                UTF8Encoding encoding = new UTF8Encoding();

                var requestData = encoding.GetBytes(soapRequest);

                var url = VpConfigurationParameters.GetGenericParameter("DateConversionServiceURL").ToString();
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Ssl3;
                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                
                httpRequest.Headers.Add("SOAPAction", "\"http://tempuri.org/ConvertHijriDatetoGregDate\"");
                httpRequest.Method = "POST";
                httpRequest.KeepAlive = false;
                httpRequest.ContentType = "text/xml; charset=\"utf-8\"";
                httpRequest.ContentLength = requestData.Length;

                var timeout = Convert.ToInt32(VpConfigurationParameters.GetGenericParameter("DateConversionServiceTimeOut"));
                httpRequest.Timeout = timeout;

                HttpWebResponse httpResponse = null;
                String response = String.Empty;

                httpRequest.GetRequestStream().Write(requestData, 0, requestData.Length);
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                Stream baseStream = httpResponse.GetResponseStream();
                StreamReader responseStreamReader = new StreamReader(baseStream);
                response = responseStreamReader.ReadToEnd();
                responseStreamReader.Close();
                XDocument doc = XDocument.Parse(response);
                var gregDate = doc.Root.Value;
                DateTime dtGreg = new DateTime();
                bool sucess = false;
                if (!string.IsNullOrEmpty(gregDate))
                {
                    gregDate = gregDate.Split(new char[] { '$' })[0];
                    sucess = DateTime.TryParseExact(gregDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtGreg);
                }
                if (sucess)
                {
                    AddidentityExpiryDatePicker.ChosenDate = dtGreg;
                }
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// Add Signatory in Database
    /// </summary>
    private long AddSignatory()
    {
        try
        {
            VeriBranch.Common.MessageDefinitions.CreateUserRequest request = GetSignatoryDetails();
            VPEntity personEntity = new VPEntity(VPTables.VpPerson);
            personEntity["FirstName"] = (string)request.User.CustomerName;
            if (request.User.Birthday < DateTime.MaxValue && request.User.Birthday > DateTime.MinValue)
                personEntity["BirthDate"] = request.User.Birthday;

            //BackOffice Corporate Enrollment 13.03.2008 Start
            if (request.User.Name != null && request.User.Name != String.Empty)
                personEntity["FirstName"] = request.User.Name;
            if (request.User.LastName != null && request.User.LastName != String.Empty)
                personEntity["LastName"] = request.User.LastName;
            if (request.User.MobileNumber != null && request.User.MobileNumber != String.Empty)
                personEntity["Gsm"] = request.User.MobileNumber;

            if (!string.IsNullOrEmpty(request.Nationality))
                personEntity["Nationality"] = request.Nationality;

            //BackOffice Corporate Enrollment 13.03.2008 End

            personEntity.Save();

            VPEntity userEntity = new VPEntity(VPTables.VpUser);
            userEntity["CifNo"] = request.User.CifNo;
            userEntity["HasAllAccountAccessRight"] = request.User.HasAllAccountAccessRight;
            userEntity["UserName"] = (string)request.User.UserName;
            userEntity["Otpuse3PartyTransfer"] = request.User.EnableToTransferThirdParty;

            userEntity["PersonId"] = personEntity.ToInt64("PersonId");

            if (request.User.Department != null && request.User.Department != "" &&
                request.User.Department != String.Empty)
                userEntity["Department"] = request.User.Department;


            if (request.User.CustomerType == CustomerTypeEnum.Retail)
            {
                userEntity["IsCorporate"] = false;
                userEntity["IsPasswordReset"] = false;
            }
            else
            {
                userEntity["IsCorporate"] = true;
                userEntity["IsPasswordReset"] = false;
                userEntity["PasswordResetDate"] = DateTime.Today;
            }

            if (request.User.LoginExpireDate != null && request.User.LoginExpireDate != DateTime.MinValue)
            {
                userEntity["LoginExpireDate"] = request.User.LoginExpireDate;
            }

            if (request.User.CustomerHostType.ToString() != "NotAvailable")
                userEntity["CustomerSegmentId"] = (int)request.User.CustomerHostType;
            userEntity["EnrollmentDate"] = DateTime.Now;

            //BackOffice Corporate Enrollment 13.03.2008 Start
            if (request.User.Email != null && request.User.Email != String.Empty)
            {
                userEntity["Email"] = request.User.Email;
            }
            if (request.User.CorporateId != 0)
            {
                userEntity["CorporateId"] = request.User.CorporateId;

            }
            if (request.User.EmployeeId != null && request.User.EmployeeId != String.Empty)
            {
                userEntity["EmployeeId"] = request.User.EmployeeId;
            }


            //BackOffice Corporate Enrollment 13.03.2008 End

            userEntity["OTPState"] = 0;
            userEntity["MaxPersonalTxnTransferLimit"] = -1;
            userEntity["MaxPersonalDailyTransferLimit"] = -1;
            userEntity["Max3rdPTxnTransferLimit"] = -1;
            userEntity["Max3rdPDailyTransferLimit"] = -1;
            userEntity["ActivateEmail"] = 0;

            if (!string.IsNullOrEmpty(request.IdentityNumber))
                userEntity["IdentityNumber"] = request.IdentityNumber;

            if (!string.IsNullOrEmpty(request.IdentityType))
                userEntity["IdentityType"] = request.IdentityType;

            if (request.IdentityExpiryDate != null && request.IdentityExpiryDate != DateTime.MinValue)
                userEntity["IdentityExpiryDate"] = request.IdentityExpiryDate;

            if (!string.IsNullOrEmpty(request.Memo))
                userEntity["Memo"] = request.Memo;

            if (request.IsSignatory)
            {
                if (request.SignatoryIsCorporate)
                {
                    userEntity["IsCorporate"] = true;
                    userEntity["CorporateId"] = request.SignatoryCorporateID;
                }
            }

            userEntity.Save();

            long userID = userEntity.ToInt64("UserId");

            VPEntity userRolesEntity = new VPEntity(VPTables.VpUserRoles);

            userRolesEntity["UserId"] = userID;
            userRolesEntity["RoleId"] = UserRoleConstants.LIMITED_THIRD_PARTY_PAYMENT_RETAIL;

            //BackOffice Corporate Enrollment 13.03.2008 Start
            if (request.User.IsRoleIdSpecified1)
                userRolesEntity["RoleId"] = request.User.RoleId;
            //BackOffice Corporate Enrollment 13.03.2008 End

            userRolesEntity.Save();

            //this if block means that, if the newly created user is approver and if the corporate's
            //cash management first or two step and new user's role is approver, we should automatically assig user 
            //to cash management user so that new user can approve the corporate txn.
            if (request.User.CustomerType == CustomerTypeEnum.Corporate)
            {
                try
                {
                    SignatureVerificationManager svManager = new SignatureVerificationManager();

                    if (UserRoleTypeEnum.TransactionApprover ==
                        (UserRoleTypeEnum)Enum.Parse(typeof(UserRoleTypeEnum), request.User.RoleId.ToString()))
                        svManager.CheckAndPerformCorporateCashManagementState(userID);
                }
                catch (Exception ex)
                {

                }

            }

            VPEntity userStatesEntity = new VPEntity(VPTables.VpUserStates);
            userStatesEntity["UserId"] = userID;
            userStatesEntity["UserStateTypeId"] = (int)UserStateTypeEnum.Active;
            userStatesEntity["OTPState"] = 0;
            userStatesEntity["LoginStateTypeID"] = (request.ResetPasswordOnLogin)
                                                       ? (int)LoginStateTypeEnum.WaitingNewPasswordSet
                                                       : (int)LoginStateTypeEnum.Undefined;

            //BackOffice Corporate Enrollment 14.03.2008 Start
            if (request.User.IsUserStateSpecified1)
                userStatesEntity["UserStateTypeId"] = request.User.UserStateTypeId;

            if (request.User.IsLoginStateSpecified1)
                userStatesEntity["LoginStateTypeId"] = request.User.LoginStateTypeId;
            //BackOffice Corporate Enrollment 14.03.2008 End

            userStatesEntity["ChannelId"] = ChannelStates.Internet;

            userStatesEntity.Save();

            VPEntity logonEntity = new VPEntity(VPTables.VpLogon);
            logonEntity["UserId"] = userID;
            logonEntity["RetryCount"] = 0;
            logonEntity["LastPassChangeDate"] = DateTime.Now;
            logonEntity["ChannelId"] = ChannelStates.Internet;

            logonEntity.Save();

            if (request.IsSpecifiedAccount)
            {
                if (request.AccountList != null)
                {
                    foreach (VpCheckedAccountStatus checkedAccountStatus in request.AccountList)
                    {
                        EnrollmentManager.SaveUserAccountLevelAccess(userID,
                                                                     pageController.TransactionHeader.User.UserId,
                                                                     checkedAccountStatus);
                        //EnrollmentManager.SaveUserAccountLevelAccess(CorporateUser, transactionHeader.User.UserId, checkedAccountStatus);
                    }
                }
            }
            VPEntityCollection enrollmentHistoryCollection = new VPEntityCollection(VPTables.VpEnrollmentHistory);
            IPredicateExpression cifNumberFilter = new PredicateExpression();
            cifNumberFilter.Add(
                PredicateFactory.CompareValue(new VPField(VPTables.VpEnrollmentHistory, "CustomerNumber"),
                                              ComparisonOperator.Equal, request.User.CifNo));
            enrollmentHistoryCollection.Select(cifNumberFilter);

            if (enrollmentHistoryCollection.Items.Count > 0)
            {
                int lastIndexOfTheCollection = enrollmentHistoryCollection.Items.Count - 1;
                VPEntity enrollmentHistoryEntity = enrollmentHistoryCollection[lastIndexOfTheCollection];
                enrollmentHistoryEntity["IsCompleted"] = true;
                enrollmentHistoryEntity["CreateDate"] = DateTime.Now;
                enrollmentHistoryEntity.Save();
            }
            else
            {
                VPEntity newEnrollmentHistoryEntity = new VPEntity(VPTables.VpEnrollmentHistory);
                newEnrollmentHistoryEntity["CardNumber"] = request.CardNumber;
                newEnrollmentHistoryEntity["NumberOfFaults"] = 0;
                newEnrollmentHistoryEntity["CustomerNumber"] = request.User.CifNo;
                newEnrollmentHistoryEntity["IsCompleted"] = true;
                newEnrollmentHistoryEntity["CreateDate"] = DateTime.Now;
                newEnrollmentHistoryEntity.Save();
            }

            return userID;

        }
        catch (Exception ex)
        {
            VPSystemException vpse = new VPSystemException
                {
                    InnerException = ex,
                    Origin = "AccountPOACapture.AddSignatory"
                };
            LogManager.LogException(vpse);
            throw vpse;
        }
    }

    private DataTable GetTypeData(bool withAccountHolder)
    {
        DataTable dataTable = null;
        if (pageController.GetStateValue("identityTypeDataSource") == null)
        {
            SignatureVerificationManager svManager = new SignatureVerificationManager();
            dataTable = svManager.GetTypeDomains(TypeDomainIDs.AccountSignatoryRelation);
        }
        else
        {
            dataTable = (DataTable)pageController.GetStateValue("identityTypeDataSource");
        }

        DataTable returnTable = dataTable.Copy();
        if (!withAccountHolder)
        {
            DataRow[] rows = (DataRow[])returnTable.Select(" TypeDataId = '" + TypeDataIDs.AccountSignatoryRelation_AccountHolder + "'");

            foreach (DataRow row in rows)
            {
                returnTable.Rows.Remove(row);
            }
        }
        return returnTable;
    }
    #endregion

    #region Properties

    /// <summary>
    /// Gets the account number.
    /// </summary>
    /// <value>
    /// The account number.
    /// </value>
    public string AccounNumber
    {
        get { return (pageController.GetStateValue("AccountNumber") == null) ? string.Empty : pageController.GetStateValue("AccountNumber").ToString(); }
    }

    /// <summary>
    /// Gets the account name.
    /// </summary>
    /// <value>
    /// The account name.
    /// </value>
    public string AccounName
    {
        get { return (pageController.GetStateValue("AccountTitle") == null) ? string.Empty : pageController.GetStateValue("AccountTitle").ToString().Trim(); }
    }

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    /// <value>
    /// The name of the user.
    /// </value>
    string userName
    {
        get
        {
            //return AddfullNameText.Text.Trim() + AddlastNameText.Text.Trim() + AddidentityNumberText.Text.Trim();
            return AddfullNameText.Text.Trim() + AddidentityNumberText.Text.Trim();
        }
    }

    /// <summary>
    /// Gets the front office login role id.
    /// </summary>
    /// <value>
    /// The front office login role id.
    /// </value>
    private int FrontOfficeLoginRoleId
    {
        get { return int.Parse(VpConfigurationManager.GetAppSetting("FrontOfficeLoginUserRole")); }
    }

    private bool IsCore
    {
        get
        {
            if (HttpContext.Current.Request.QueryString["CaptureOption"] != null)
                return HttpContext.Current.Request.QueryString["CaptureOption"].ToLower() == "1";

            return false;
        }
    }
    #endregion
}
