<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EditAccountPOACapture.aspx.cs"
    Inherits="EditAccountPOACapture" Theme="VeriBankTheme" %>

<%@ Register Src="~/Controls/ctlTypeData.ascx" TagName="ctlTypeData" TagPrefix="VeriBranchControls" %>
<%@ Register Src="~/Controls/ValidationSummaryControl.ascx" TagName="ValidationSummaryControl"
    TagPrefix="VeriBranchUserControls" %>
<%@ Register Src="~/Controls/ConvertDatePickerControl.ascx" TagPrefix="VeriBranchUserControls"
    TagName="DatePicker" %>
<%@ Register Src="~/Controls/PhoneNumberControl.ascx" TagName="PhoneNumberControl"
    TagPrefix="uc2" %>
<%@ Register Assembly="WebApplication.UIProcess" Namespace="WebApplication.CustomControls"
    TagPrefix="cc1" %>
<%--<asp:Content ID="AccountDetailCapture" ContentPlaceHolderID="transactionContent"
    runat="Server">--%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Edit Signatory</title>
    <base target="_self" />
    <link rel="stylesheet" type="text/css" href="../../App_Themes/VeriBankTheme/NewUi.css" />
    <link rel="stylesheet" type="text/css" href="../../Content/NewUi/fonts/fonts.css" />
    <link rel="stylesheet" href="../../Content/css/custom-theme/jquery-ui-1.9.2.custom.min.css" />
    <script language="javascript" type="text/javascript" src="../../Content/js/jquery-1.8.3.js"></script>
    <script language="javascript" type="text/javascript" src="../../Content/js/jquery-ui-1.9.2.custom.min.js"></script>
    <script language="javascript" type="text/javascript" src="../../Content/js/css_browser_selector.js"></script>
    <%--<script language="javascript" type="text/javascript" src="../../Content/js/jqueryslidemenu.js"></script>--%>
    <script language="javascript" type="text/javascript" src="../../Content/EN/js/validation.js"></script>
    <style type="text/css">
        .wrow
        {
            width: 688px;
        }
    </style>
</head>
<body onload="DisableSearchValidation(); EnableAddValidation();">
    <form id="form1" runat="server">
    <VeriBranchUserControls:ValidationSummaryControl ID="ValidationSummaryControl1" runat="server" />
    <script language="javascript" type="text/javascript">
        window.onbeforeunload = confirmExit;
        function confirmExit() {
            window.opener.postMessage(false, "*");
            return null;
        }
        function ensureSelection() {
            var grid = document.getElementById('<%= signatoryGrid.ClientID%>');
            var result = false;
            if (grid.rows.length > 0) {

                for (count = 1; count < grid.rows.length; count++) {

                    //get the reference of first column
                    var cell = grid.rows[count].cells[0];

                    //loop according to the number of childNodes in the cell
                    for (j = 0; j < cell.childNodes.length; j++) {
                        if (cell.childNodes[j].type == "checkbox") {
                            result = cell.childNodes[j].checked || result;
                        }
                    }

                }

            }
            if (!result) {
            }
            return result;
        }

        $(function () {
            $("#tabs").tabs();
            DisableAddValidation();
            EnableSearchValidation();
        });

        function EnableSearchValidation() {
            var element;

            element = document.getElementById('<%=identityNumberTextRequiredValidation.ClientID %>');
            if (element != null) ValidatorEnable(element, true);
        }

        function DisableSearchValidation() {
            var element;

            element = document.getElementById('<%=identityNumberTextRequiredValidation.ClientID %>');
            if (element != null) ValidatorEnable(element, false);
        }

        function EnableAddValidation() {
            var element;

            element = document.getElementById('<%=AddcifNoTextRequiredValidation.ClientID %>');
            if (element != null) ValidatorEnable(element, true);

            element = document.getElementById('<%=AddidentityNumberTextValidator2.ClientID %>');
            if (element != null) ValidatorEnable(element, true);

            element = document.getElementById('<%=AddfullNameTextRequriedValidator.ClientID %>');
            if (element != null) ValidatorEnable(element, true);


            element = document.getElementById('<%= AddctlIdentityType.DropDownRequiredFieldValidator.ClientID %>');
            if (element != null) ValidatorEnable(element, true);

            element = document.getElementById('<%= AddctlNationality.DropDownRequiredFieldValidator.ClientID %>');
            if (element != null) ValidatorEnable(element, true);
        }

        function DisableAddValidation() {
            var element;

            element = document.getElementById('<%= AddcifNoTextRequiredValidation.ClientID %>');
            if (element != null) ValidatorEnable(element, false);

            element = document.getElementById('<%= AddidentityNumberTextValidator2.ClientID %>');
            if (element != null) ValidatorEnable(element, false);

            element = document.getElementById('<%= AddfullNameTextRequriedValidator.ClientID %>');
            if (element != null) ValidatorEnable(element, false);



            element = document.getElementById('<%= AddctlIdentityType.DropDownRequiredFieldValidator.ClientID %>');
            if (element != null) ValidatorEnable(element, false);

            element = document.getElementById('<%= AddctlNationality.DropDownRequiredFieldValidator.ClientID %>');
            if (element != null) ValidatorEnable(element, false);
        }
    </script>
    <div id="tabs" style="margin-left: 1px; margin-right: 1px">
        <ul>
            <%--<li><a href="#Search" onclick="DisableAddValidation(); EnableSearchValidation();">Search</a></li>--%>
            <li><a href="#Add" onclick="DisableSearchValidation(); EnableAddValidation();">Edit</a></li>
        </ul>
        <div id="Search" runat="server" style="display: none">
            <table class="SecurityPageTable">
                <tr>
                    <th class="SecurityPageTableheader">
                        <asp:Label ID="subHeadingLabel" runat="server" meta:resourcekey="subHeadingLabel" />
                    </th>
                </tr>
                <tr>
                    <td>
                        <table class="wrow" cellpadding="0" cellspacing="0" border="0">
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="identityNumberLabel" runat="server" meta:resourcekey="identityNumberLabel" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="identityNumberText" runat="server" MaxLength="20" IsValidatorVisible="True"
                                        DiscardedValues="" UpperCase="False" />
                                    <asp:RequiredFieldValidator ID="identityNumberTextRequiredValidation" runat="server"
                                        ControlToValidate="identityNumberText" meta:resourcekey="identityNumberTextRequiredValidation"
                                        Display="None" />
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="identityTypeLabel" runat="server" meta:resourcekey="identityTypeLabel" />
                                </td>
                                <td class="wtdata2">
                                    <VeriBranchControls:ctlTypeData ID="ctlIdentityType" runat="server" AddAll="true">
                                    </VeriBranchControls:ctlTypeData>
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="nationalityLabel" runat="server" meta:resourcekey="nationalityLabel" />
                                </td>
                                <td class="wtdata2">
                                    <VeriBranchControls:ctlTypeData ID="ctlNationality" runat="server" AddAll="true">
                                    </VeriBranchControls:ctlTypeData>
                                </td>
                            </tr>
                        </table>
                        <br />
                        <div style="float: right;">
                            <asp:ImageButton runat="server" ID="btnSearchAccount" onmouseover="javascript:window.status='';return true;"
                                OnClick="btnSearchAccount_Click" ImageUrl="<%$ Resources:ImagesURLs, Search %>" />
                        </div>
                    </td>
                </tr>
            </table>
            <br />
            <div runat="server" id="trNewUsers" visible="false">
                <table class="SecurityPageTable">
                    <tr>
                        <th class="SecurityPageTableheader">
                            <asp:Label ID="signatoryLabel" runat="server" meta:resourcekey="signatoryLabel" />
                        </th>
                    </tr>
                    <tr>
                        <td>
                            <asp:GridView ID="signatoryGrid" runat="server" CssClass="gridbg_white_area" AutoGenerateColumns="False"
                                meta:resourcekey="signatoryGridResource1" DataKeyNames="UserID,FullName">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkSelected" runat="server" Checked="false" Enabled='<%# (DataBinder.Eval(Container.DataItem, "RelationUserId") == DBNull.Value)? true : false %>'>
                                            </asp:CheckBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="FullName" meta:resourcekey="AccountHolderNameResource1">
                                        <HeaderStyle Font-Bold="True" Font-Italic="False" HorizontalAlign="Left" CssClass="row_font" />
                                        <ItemStyle HorizontalAlign="Left" CssClass="row_font" Width="40%" />
                                        <FooterStyle HorizontalAlign="Left" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="IdentityNumber" meta:resourcekey="IdentityNumberResource1">
                                        <HeaderStyle Font-Bold="True" Font-Italic="False" HorizontalAlign="Left" CssClass="row_font" />
                                        <ItemStyle HorizontalAlign="Left" CssClass="row_font" Width="20%" />
                                        <FooterStyle HorizontalAlign="Left" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="IdentityTypeText" meta:resourcekey="IdentityType">
                                        <HeaderStyle Font-Bold="True" Font-Italic="False" HorizontalAlign="Left" CssClass="row_font" />
                                        <ItemStyle HorizontalAlign="Left" CssClass="row_font" Width="20%" />
                                        <FooterStyle HorizontalAlign="Left" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="NationalityText" meta:resourcekey="Nationality">
                                        <HeaderStyle Font-Bold="True" Font-Italic="False" HorizontalAlign="Left" CssClass="row_font" />
                                        <ItemStyle HorizontalAlign="Left" CssClass="row_font" Width="20%" />
                                        <FooterStyle HorizontalAlign="Left" />
                                    </asp:BoundField>
                                </Columns>
                            </asp:GridView>
                        </td>
                    </tr>
                </table>
                <br />
                <div style="float: right;">
                    <asp:ImageButton runat="server" ID="btnSaveAndClose" onmouseover="javascript:window.status='';return true;"
                        OnClick="btnSaveAndClose_Click" ImageUrl="<%$ Resources:ImagesURLs, btn_continue%>"
                        OnClientClick="javascript:return ensureSelection();" />
                </div>
            </div>
        </div>
        <div id="Add" runat="server">
            <asp:CustomValidator ID="cvCanConfirm" runat="server" OnServerValidate="cvCanConfirm_ServerValidate"
                meta:resourcekey="cvCanConfirm" Display="None"></asp:CustomValidator>
            <table class="SecurityPageTable" width="100%">
                <tr>
                    <th class="SecurityPageTableheader">
                        <asp:Label ID="AddsubHeadingLabel" runat="server" meta:resourcekey="AddsubHeadingLabel" />
                    </th>
                </tr>
                <tr>
                    <td>
                        <table cellpadding="1" cellspacing="0" border="0" class="wrow">
                            <tr runat="server" id="AddmessageRow" visible="false">
                                <td colspan="2" style="height: 28px; font-weight: bold">
                                    <asp:Label ID="messageLabel" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr id="Tr1" class="table-sec3" runat="server" visible="false">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddcifNoLabel" runat="server" meta:resourcekey="AddcifNoLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="AddcifNoText" runat="server" MaxLength="7" InputType="OnlyAlphaNumeric"
                                        IsValidatorVisible="True" DiscardedValues="" UpperCase="False" />
                                    <asp:RequiredFieldValidator ID="AddcifNoTextRequiredValidation" runat="server" ControlToValidate="AddcifNoText"
                                        meta:resourcekey="AddcifNoTextRequiredValidation" Display="None" />
                                </td>
                            </tr>
                            <tr runat="server" id="accountNumberRow" class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="lblAccountNumber" runat="server" meta:resourcekey="lblAccountNumber" />
                                </td>
                                <td class="wtdata2">
                                    <asp:Label ID="lblAccountNumberText" runat="server" />
                                </td>
                            </tr>
                            <tr runat="server" id="accountNameRow" class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="lblAccountName" runat="server" meta:resourcekey="accountTitleLabel" />
                                </td>
                                <td class="wtdata2">
                                    <asp:Label ID="lblAccountNameText" runat="server" />
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddlblIdentityType" runat="server" meta:resourcekey="AddlblIdentityType" />
                                </td>
                                <td class="wtdata2">
                                    <VeriBranchControls:ctlTypeData SkinID="LSizeComboControl" ID="AddctlIdentityType"
                                        runat="server" AddSelect="true" Required="true" OnCMBSelectChanged="AddctlIdentityType_CMBSelectChanged"
                                        meta:resourcekey="AddctlIdentityType"></VeriBranchControls:ctlTypeData>
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddidentityNumberLabel" runat="server" meta:resourcekey="AddidentityNumberLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="AddidentityNumberText" runat="server" InputType="Default" IsValidatorVisible="True"
                                        DiscardedValues="" MaxLength="15" />
                                    <asp:RequiredFieldValidator ID="AddidentityNumberTextValidator2" runat="server" ControlToValidate="AddidentityNumberText"
                                        meta:resourcekey="AddidentityNumberTextValidator2" Display="None" />
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddidentityExpiryLabel" runat="server" meta:resourcekey="AddidentityExpiryLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <VeriBranchUserControls:DatePicker runat="server" ID="AddidentityExpiryDatePicker"
                                        FillDate="false" AutoPostBackAllowed="true" OnTXTSelectChanged="AddidentityExpiryDatePicker_CMBSelectChanged" />
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddIdentityHijriExpiryLabel" runat="server" meta:resourcekey="AddidentityHijriExpiryLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <asp:TextBox runat="server" ID="AddIdentityHijriExpiry" MaxLength="10" OnTextChanged="AddidentityHijriExpiryDatePicker_SelectChanged"
                                        AutoPostBack="true" />
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddlblNationality" runat="server" meta:resourcekey="AddlblNationality" />
                                </td>
                                <td class="wtdata2">
                                    <VeriBranchControls:ctlTypeData ID="AddctlNationality" runat="server" AddSelect="true"
                                        Required="true" meta:resourcekey="AddctlNationality" style="width: 250px"></VeriBranchControls:ctlTypeData>
                                </td>
                            </tr>
                            <tr class="table-sec3" runat="server" visible="false" id="trDateOfBirth">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AdddateOfBirthLabel" runat="server" meta:resourcekey="AdddateOfBirthLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <VeriBranchUserControls:DatePicker runat="server" ID="AdddateOfBirthDatePicker" Type="Start" />
                                </td>
                            </tr>
                             <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="Label1" runat="server" meta:resourcekey="Relationship" />
                                </td>
                                <td class="wtdata2">
                                    <asp:DropDownList ID="ddlRelationShip" runat="server" />      
                                </td>
                            </tr>
                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddfullNameLabel" runat="server" meta:resourcekey="AddfullNameLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="AddfullNameText" runat="server" IsValidatorVisible="True" DiscardedValues=""
                                        MaxLength="200" />
                                    <asp:RequiredFieldValidator ID="AddfullNameTextRequriedValidator" runat="server"
                                        ControlToValidate="AddfullNameText" meta:resourcekey="AddfullNameTextRequriedValidator"
                                        Display="None" />
                                </td>
                            </tr>


                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="lblSignatoryStatus" runat="server" meta:resourcekey="lblSignatoryStatus" />
                                </td>
                                <td class="wtdata2">
                                    <asp:DropDownList ID="ddlSignatoryStatus" runat="server" />      
                                </td>
                            </tr>

                            <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="MemoLabel" runat="server" meta:resourcekey="AddmemoLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <asp:TextBox runat="server" ID="MemoText" TextMode="MultiLine" Height="45px" 
                                        Width="396px" MaxLength="1000000"/>
                                </td>
                            </tr>

                              <tr class="table-sec3">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="lblRemarks" runat="server" meta:resourcekey="lblRemarks" />
                                </td>
                                <td class="wtdata2">
                                        <cc1:VBTextBox ID="txtRemarks" runat="server" IsValidatorVisible="True"  MaxLength="1000"
                                        DiscardedValues=""  />

                                         <asp:RequiredFieldValidator ID="UpdateReasoRequiredValidation" runat="server"
                                        ControlToValidate="txtRemarks" meta:resourcekey="UpdateReasoRequiredValidation"
                                        Display="None" />
                                </td>
                            </tr>

                            <%-- <tr class="table-sec3">
                                    <td class="head_tbl_top_ii_2">
                                        <asp:Label ID="AddfirstNameLabel" runat="server" meta:resourcekey="AddfirstNameLabelResource1" />
                                    </td>
                                    <td class="wtdata2">
                                   
                                        <cc1:VBTextBox ID="AddfirstNameText" runat="server"  IsValidatorVisible="True" DiscardedValues="" MaxLength="200" />
                                        <asp:RequiredFieldValidator ID="AddfirstNameTextRequriedValidator" runat="server" ControlToValidate="AddfirstNameText"
                                            meta:resourcekey="AddfirstNameTextRequriedValidator" Display="None" />
                                    </td>
                                </tr>
                                <tr class="table-sec3">
                                    <td class="head_tbl_top_ii_2">
                                        <asp:Label ID="AddlastNameLabel" runat="server" meta:resourcekey="AddlastNameLabelResource1" />
                                    </td>
                                    <td class="wtdata2">
                                        <cc1:VBTextBox ID="AddlastNameText" runat="server"  IsValidatorVisible="True" DiscardedValues="" MaxLength="200" />
                                        <asp:RequiredFieldValidator ID="AddlastNameTextValidator1" runat="server" ControlToValidate="AddlastNameText" meta:resourcekey="AddlastNameTextValidator1" Display="None" />
                                    </td>
                                </tr>--%>
                            <tr class="table-sec3" runat="server" id="trUserType" visible="false">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AdduserTypeLabel" runat="server" meta:resourcekey="AdduserTypeLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <asp:DropDownList ID="AdduserTypeDDL" runat="server" AutoPostBack="true" OnSelectedIndexChanged="AdduserTypeDDL_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr class="table-sec3" runat="server" id="trCorporate" visible="false">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddcoroporateLabel" runat="server" meta:resourcekey="AddcoroporateLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <asp:DropDownList ID="AddcorporateDDL" runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr class="table-sec3" runat="server" visible="false" id="trEmail">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddemailLabel" runat="server" meta:resourcekey="AddemailLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="AddemailText" runat="server" InputType="Default" IsValidatorVisible="True"
                                        DiscardedValues="" MaxLength="100" />
                                    <asp:RegularExpressionValidator runat="server" ID="AddemailValidator" ControlToValidate="AddemailText"
                                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" meta:resourcekey="AddemailValidator">
                                    </asp:RegularExpressionValidator>
                                </td>
                            </tr>
                            <tr class="table-sec3" runat="server" visible="false" id="trMobileNumber">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AddmobileNumberLabel" runat="server" meta:resourcekey="AddmobileNumberLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <uc2:PhoneNumberControl ID="AddmobileNumberText" runat="server" PhoneNumberRequired="false"
                                        PhoneNumberRequiredErrorMessage="Please enter your mobile number" AllowPlusSign="true" />
                                </td>
                            </tr>
                            <tr class="table-sec3" runat="server" visible="false" id="trDepartment">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AdddepartmentLabel" runat="server" meta:resourcekey="AdddepartmentLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="AdddepartmentText" runat="server" InputType="Default" IsValidatorVisible="True"
                                        DiscardedValues="" MaxLength="100" />
                                </td>
                            </tr>
                            <tr class="table-sec3" runat="server" visible="false" id="trUserName">
                                <td class="head_tbl_top_ii_2">
                                    <asp:Label ID="AdduserNameLabel" runat="server" meta:resourcekey="AdduserNameLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <cc1:VBTextBox ID="AdduserNameText" runat="server" InputType="Default" IsValidatorVisible="True"
                                        DiscardedValues="" MaxLength="50" />
                                </td>
                            </tr>
                            <tr style="display: none" class="table-sec3">
                                <td class="head_tbl_top_ii_2" style="height: 28px;">
                                    <asp:Label ID="AddchannelLabel" runat="server" meta:resourcekey="AddchannelLabelResource1" />
                                </td>
                                <td class="wtdata2">
                                    <asp:DropDownList ID="AddchannelDDL" runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <%--<tr class="table-sec3">
                                    <td class="head_tbl_top_ii_2">
                                        <asp:Label ID="AddmemoLabel" runat="server" meta:resourcekey="AddmemoLabelResource1" />
                                    </td>
                                    <td class="wtdata2">
                                        <cc1:VBTextBox ID="AddmemoText" runat="server" InputType="Default" IsValidatorVisible="True"
                                            DiscardedValues="" MaxLength="255" />
                                        <asp:RequiredFieldValidator ID="AddmemoTextRequriedValidator" runat="server" ControlToValidate="AddmemoText"
                                            meta:resourcekey="AddmemoTextRequriedValidator" Display="None" />
                                    </td>
                                </tr>--%>
                        </table>
                        <br />
                        <div style="float: right;">
                            <asp:ImageButton runat="server" ID="btnAddSignatory" onmouseover="javascript:window.status='';return true;"
                                OnClick="btnAddSignatory_Click" ImageUrl="<%$ Resources:ImagesURLs, update %>" />
                        </div>
                        <div style="color: red; float: left; font-weight: bold;">
                            <asp:Label runat="server" ID="lblWarning"></asp:Label>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    </form>
</body>
</html>
