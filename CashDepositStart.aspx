<%@ Page Language="C#" MasterPageFile="~/MasterPage/VeriBranchMasterPage.master" AutoEventWireup="true" CodeFile="CashDepositStart.aspx.cs" Inherits="CashDepositStart" %>

<%@ Register Src="~/Controls/DatePickerControl.ascx" TagName="DatePickerControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/LookupControl.ascx" TagName="LookupControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/ChargesControl.ascx" TagName="ChargesControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/OffersBoxControl.ascx" TagName="OffersBoxControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/AccountLookupControl.ascx" TagName="AccountLookupControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/ChargesAndOffersTabsControl.ascx" TagName="ChargesAndOffersTabsControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/ForwardToReferCardControl.ascx" TagName="ForwardToReferCardControl" TagPrefix="VP" %>

<asp:Content ID="Content1" ContentPlaceHolderID="transactionContent" runat="Server">


    <script language="javascript" type="text/javascript">

        function validateDepositAmount(source, args) {
            args.IsValid = false;
            var totalAmount = GetTotalAmount();

            var depositAmountInput = 0;
            var ctlDepositAmount = document.getElementById('<%=ctlDepositAmount.ControlClientID%>');
            if ($.isNumeric(removeCommas(ctlDepositAmount.value)))
                depositAmountInput = parseFloat(removeCommas(ctlDepositAmount.value));
            args.IsValid = totalAmount == depositAmountInput;
        }


        function GetTotalAmount() {
            var txtDepositAmount = document.getElementById('<%=ctlCurrencyDenominations.AmountControlClientID%>');
            var txtTCRAmount = document.getElementById('<%=ctlTCRDenominations.AmountControlClientID%>');

            var totalAmount = 0;
            if ($.isNumeric(removeCommas(txtTCRAmount.value)))
                totalAmount += parseFloat(removeCommas(txtTCRAmount.value));

            if ($.isNumeric(removeCommas(txtDepositAmount.value)))
                totalAmount += parseFloat(removeCommas(txtDepositAmount.value));

            return totalAmount;
        }

        function onExRateChanged(rate) {
            try {
                var depositCurrency = document.getElementById('<%=ctlDepositAmount.CurrencyClientID%>');
                var txtDestinationCurrency = document.getElementById('<%=ctlExchangeRate.DestinationCurrencyClientID%>');
                if (depositCurrency.value != txtDestinationCurrency.value) {
                    <%=lblAmountAccountCCYValue.ClientID%>_ConvertAmount(GetTotalAmount(), rate, txtDestinationCurrency.value);
                }
            } catch (e) {

            }
        }

        function ctlCurrencyDenominations_DenominationsChanged(totalAmount) {

            var hidDepositCurrency = document.getElementById('<%=hidDepositCurrency.ClientID%>');

            totalAmount = GetTotalAmount();
            <%=ctlAmountInWords.ClientID%>_SetAmountInWords(totalAmount, hidDepositCurrency.value);
            var lblAmountAccountCCYValue = document.getElementById('<%=lblAmountAccountCCYValue.ClientID%>');

            var txtExRate = document.getElementById('<%=ctlExchangeRate.ClientID%>');
            var txtDestinationCurrency = document.getElementById('<%=ctlExchangeRate.DestinationCurrencyClientID%>');
            var exRate = 0;
            if ($.isNumeric(txtExRate.value)) {
                exRate = parseFloat(txtExRate.value);
            }


            <%=ctlTotalDepositAmountValue.ClientID%>_ConvertAmount(totalAmount, 1, hidDepositCurrency.value);
            if (typeof <%=lblAmountAccountCCYValue.ClientID%>_ConvertAmount === 'function') {
                <%=lblAmountAccountCCYValue.ClientID%>_ConvertAmount(totalAmount, exRate, txtDestinationCurrency.value);
            }
        }
    </script>


    <VP:VBCustomValidator runat="server" ID="cvValidateDepositCurrencies" OnServerValidate="cvValidateDepositCurrencies_ServerValidate"></VP:VBCustomValidator>
    <VP:VBCustomValidator runat="server" ID="cvValidateCreditOnlyAccounts" OnServerValidate="cvValidateCreditOnlyAccounts_ServerValidate" Enabled="False"></VP:VBCustomValidator>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <VP:VBPanel ID="VBPanel1" runat="server">
                <VP:StepControl ID="StepControl1" runat="server" meta:resourcekey="ctlTransactionDetailsStep" />
                <VP:VBTable ID="VBTable1" runat="server">
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAccountNumber" meta:resourcekey="lblAccountNumber" AssociatedControlID="ctlAccountNumber" IsRequired="True"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:AccountLookupControl ID="ctlAccountNumber" runat="server" PrePopulatedValueName="AccountNumber" EntityKey="AccountLookup" LinkButtonColumnName="AccountNumber" IdColumnName="AccountNumber" ValidationExpression="^\d+$" meta:resourcekey="ctlAccountNumber" TextTypeEnabled="True" OnItemSelected="ctlAccountNumber_OnItemSelected" IsRequired="True" />
                            <%--SwitchSide="True"--%>
                            <VP:VBHiddenField runat="server" ID="hidBranchCode" />
                            <VP:VBHiddenField runat="server" ID="hidQueryType" />
                            <VP:VBHiddenField runat="server" ID="hidAccountCurrency" />
                            <VP:VBHiddenField runat="server" ID="hidAccountTitle" />
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAccountName" meta:resourcekey="lblAccountName"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:VBLabel runat="server" ID="lblAccountNameValue"></VP:VBLabel>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAccountBranch" meta:resourcekey="lblAccountBranch"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:VBLabel runat="server" ID="lblAccountBranchValue"></VP:VBLabel>
                        </VP:VBValueCell>
                    </VP:VBRow>
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblDepositCurrency" meta:resourcekey="lblDepositCurrency" AssociatedControlID="ctlDepositCurrency" IsRequired="True"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:LookupControl ID="ctlDepositCurrency" runat="server" PrePopulatedValueName="AccountCurrency" EntityKey="CashDrawerAccountCurrencyLookup" LinkButtonColumnName="Name" IdColumnName="ISOCurrencyCode" ColumnsToShow="ISOCurrencyCode, Name" meta:resourcekey="ctlDepositCurrency" TextTypeEnabled="True" OnItemSelected="ctlDepositCurrency_OnItemSelected" IsRequired="True" />
                            <VP:VBHiddenField runat="server" ID="hidCashDrawerAccountNo" />
                            <VP:VBHiddenField runat="server" ID="hidDepositCurrency" />
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblDepositAmountInput" IsRequired="True" meta:resourcekey="lblDepositAmountInput" AssociatedControlID="ctlDepositAmount:txtAmount"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:AmountPicker runat="server" ID="ctlDepositAmount" Enabled="False" ShowCurrency="True" meta:resourcekey="ctlDepositAmount" RequiredFieldValidatorEnabled="True"></VP:AmountPicker>
                            <VP:VBCustomValidator runat="server" ID="cvValidateDepositAmount" ClientValidationFunction="validateDepositAmount" meta:resourcekey="cvValidateDepositAmount"></VP:VBCustomValidator>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <%--<VP:VBLabel runat="server" ID="lblExchangeRate" meta:resourcekey="lblExchangeRate" AssociatedControlID="ctlExchangeRate:txtExRate" IsRequired="True" Enabled="False"></VP:VBLabel>--%>
                            <VP:VBLabel runat="server" ID="lblExchangeRate" meta:resourcekey="lblExchangeRate" AssociatedControlID="ctlExchangeRate:txtExRate" Enabled="False"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:ExchangeRateControl runat="server" ID="ctlExchangeRate" OnClientSideValueChanged="onExRateChanged" Enabled="False" IsRequired="False" />
                        </VP:VBValueCell>
                    </VP:VBRow>
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAmountAccountCCY" meta:resourcekey="lblAmountAccountCCY"></VP:VBLabel>

                            <VP:VBLabel runat="server" ID="lblTotalDepositAmount" meta:resourcekey="lblTotalDepositAmount" style="display: none"></VP:VBLabel>
                            <VP:ConvertedAmountLabelControl runat="server" ID="ctlTotalDepositAmountValue" meta:resourcekey="ctlTotalDepositAmountValue" ValidateZeroAmount="True" ShowAmount="False"></VP:ConvertedAmountLabelControl>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:ConvertedAmountLabelControl runat="server" ID="lblAmountAccountCCYValue"></VP:ConvertedAmountLabelControl>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblDepositAmount" AssociatedControlID="ctlCurrencyDenominations:imgLookupButton" meta:resourcekey="lblDepositAmount" Enabled="False"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:CurrencyDenominationsControl ID="ctlCurrencyDenominations" runat="server" OnDenominationsChanged="ctlCurrencyDenominations_DenominationsChanged" AmountRequired="False" ValidateZeroAmount="False" meta:resourcekey="ctlCurrencyDenominations" Enabled="False" />
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblTCRDepositAmount" meta:resourcekey="lblTCRDepositAmount" Enabled="False"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:TCRDenominationsControl ID="ctlTCRDenominations" runat="server" OnDenominationsChanged="ctlCurrencyDenominations_DenominationsChanged" AmountRequired="False" meta:resourcekey="ctlTCRDenominations" Enabled="False" />
                        </VP:VBValueCell>
                    </VP:VBRow>
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAmountInWords" meta:resourcekey="lblAmountInWords"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell ColumnSpan="5">
                            <VP:AmountInWordsControl runat="server" ID="ctlAmountInWords"></VP:AmountInWordsControl>
                        </VP:VBValueCell>
                    </VP:VBRow>
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblValueDate" meta:resourcekey="lblValueDate" AssociatedControlID="ctlValueDate:txtDate" IsRequired="True"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:DatePickerControl ID="ctlValueDate" runat="server" RequiredFieldValidatorEnabled="True" ReadOnly="True"/>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblSourceOfFunds" meta:resourcekey="lblSourceOfFunds" AssociatedControlID="ddlSourceOfFunds" IsRequired="True"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell >
                            <VP:VBDropDownList ID="ddlSourceOfFunds" runat="server" />
                            <VP:VBRequiredFieldValidator runat="server" ID="rfvddlSourceOfFunds" ControlToValidate="ddlSourceOfFunds" InitialValue="" meta:resourcekey="rfvtxtSourceOfFunds">
                            </VP:VBRequiredFieldValidator>
                           <%--<VP:VBTextBox runat="server" ID="txtSourceOfFunds" SkinID="TextBox3Col" />
                            <VP:VBRequiredFieldValidator runat="server" ID="rfvtxtSourceOfFunds" ControlToValidate="txtSourceOfFunds" meta:resourcekey="rfvtxtSourceOfFunds"></VP:VBRequiredFieldValidator>
                            <VP:VBRegularExpressionValidator runat="server" ID="revtxtSourceOfFunds" meta:resourcekey="revtxtSourceOfFunds" ControlToValidate="txtSourceOfFunds" ValidationExpression="^[0-9a-zA-Z\ \.\,\u0600-\u06FF]*$"></VP:VBRegularExpressionValidator>--%>
                        </VP:VBValueCell>
                         <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblDepositPurpose" meta:resourcekey="lblDepositPurpose" IsRequired="True" AssociatedControlID="ddlDepositPurpose"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell >
                            <VP:VBDropDownList ID="ddlDepositPurpose" runat="server" />
                            <VP:VBRequiredFieldValidator runat="server" ID="rfvddlddlDepositPurpose" ControlToValidate="ddlDepositPurpose" InitialValue="" meta:resourcekey="rfvddlDepositPurpose">
                            </VP:VBRequiredFieldValidator>
                        </VP:VBValueCell>
                    </VP:VBRow>
                </VP:VBTable>
            </VP:VBPanel>
            
            <VP:ThirdPartyDetailsControl runat="server" id="ctlThirdPartyDetails" Type="Deposit"></VP:ThirdPartyDetailsControl>
           
             
            <VP:NarrationsControl runat="server" ID="ctlNarrations" ShowDescription="false" meta:resourcekey="ctlNarrations" ></VP:NarrationsControl>

            <%--            <VP:VBPanel runat="server">
                <VP:StepControl runat="server" meta:resourcekey="ctlNarrationDetailsStep" />
                <VP:VBTable runat="server">
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblNarration1" meta:resourcekey="lblNarration1" AssociatedControlID="txtNarration1"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:VBTextBox runat="server" ID="txtNarration1"></VP:VBTextBox>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblNarration2" meta:resourcekey="lblNarration2" AssociatedControlID="txtNarration2"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:VBTextBox runat="server" ID="txtNarration2"></VP:VBTextBox>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblNarration3" meta:resourcekey="lblNarration3" AssociatedControlID="txtNarration3"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:VBTextBox runat="server" ID="txtNarration3"></VP:VBTextBox>
                        </VP:VBValueCell>
                    </VP:VBRow>
                    <VP:VBRow>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblNarration4" meta:resourcekey="lblNarration4" AssociatedControlID="txtNarration4"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:VBTextBox runat="server" ID="txtNarration4"></VP:VBTextBox>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblDepositPurpose" meta:resourcekey="lblDepositPurpose" AssociatedControlID="txtDepositPurpose"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell ColumnSpan="3">
                            <VP:VBTextBox ID="txtDepositPurpose" runat="server" SkinID="TextBox3Col" />
                        </VP:VBValueCell>
                    </VP:VBRow>
                </VP:VBTable>
            </VP:VBPanel>--%>
        </ContentTemplate>
    </asp:UpdatePanel>

    <%--<VP:VBPanel runat="server">
        <VP:StepControl runat="server" meta:resourcekey="ctlFeeInformationStep" />
        <VP:ChargesControl ID="ctlCharges" runat="server" />
    </VP:VBPanel>
    <VP:VBPanel runat="server">
        <VP:OffersBoxControl ID="ctlOffers" runat="server" />
    </VP:VBPanel>--%>
    <VP:VBPanel runat="server">
        <%--<VP:ChargesAndOffersTabsControl runat="server" ID="ctlChargesAndOffersTabs" IsAmountRequired="True" IsAccountRequired="True" />--%>
        <VP:ChargesAndOffersTabsControl runat="server" ID="ctlChargesAndOffersTabs" />
    </VP:VBPanel>
    <VP:VBPanel runat="server">
        <VP:ForwardToReferCardControl ID="ctlForwardToReferCardControl" runat="server" Category="Forward To Refer Card" />
    </VP:VBPanel>



    <object id="DeviceAdapter" classid="clsid:6825BB10-988B-4B0D-B99B-F78BC99A20E9" style="display: none">
    </object>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="Server">

    <VP:NavigationButtonControl CancelButtonVisible="True" ID="btnNavigationControl" NextButtonVisible="true" NextUrl="~/RemoteOperations/Cash/CashDeposit/CashDepositConfirm.aspx" runat="server" />

</asp:Content>

