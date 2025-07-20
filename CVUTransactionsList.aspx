<%@ Page Language="C#" MasterPageFile="~/MasterPage/VeriBranchMasterPage.master" AutoEventWireup="true" CodeFile="CVUTransactionsList.aspx.cs" Inherits="CVUTransactionsList" %>

<%@ Register Src="~/Controls/DatePickerControl.ascx" TagName="DatePickerControl" TagPrefix="VP" %>
<%@ Register Src="~/Controls/LookupControl.ascx" TagName="LookupControl" TagPrefix="VP" %>
<asp:Content ID="Content1" ContentPlaceHolderID="transactionContent" runat="Server">
    <VP:VBPanel ID="VBPanel1" runat="server">
        <VP:StepControl ID="scSearchFilter" runat="server" meta:resourcekey="scSearchFilter" />
        <VP:VBTable ID="VBTable1" runat="server">
            <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblBranch" meta:resourcekey="lblBranch" AssociatedControlID="ctlBranchLookup"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:LookupControl ID="ctlBranchLookup" runat="server" EntityKey="BranchLookup" LinkButtonColumnName="BranchName" IdColumnName="BranchCode" ColumnsToShow="BranchCode,BranchName" meta:resourcekey="ctlBranchLookup" TextTypeEnabled="True" IsRequired="True" OnItemSelected="ctlBranchLookup_ItemSelected" OnSelectionCleared="ctlBranchLookup_OnSelectionCleared" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblPerformerIdentity" meta:resourcekey="lblPerformerIdentity" AssociatedControlID="ctlBranchUsers"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:LookupControl ID="ctlBranchUsers" runat="server" FrameWidth="500px" EntityKey="BranchUsersLookup" LinkButtonColumnName="UserName" IdColumnName="UserName" ColumnsToShow="UserName" meta:resourcekey="ctlBranchUsers" TextTypeEnabled="True" IsRequired="True" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblTransactionName" meta:resourcekey="lblTransactionName" AssociatedControlID="ddlTransactionName"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBDropDownList runat="server" ID="ddlTransactionName" />
                </VP:VBValueCell>
            </VP:VBRow>
            <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblFromDate" meta:resourcekey="lblFromDate" AssociatedControlID="ctlFromDate:txtDate"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:DatePickerControl ID="ctlFromDate" runat="server" ShowOnlyPreviousDates="True" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel ID="lblToDate" runat="server" meta:resourcekey="lblToDate" AssociatedControlID="ctlToDate:txtDate" />
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:DatePickerControl ID="ctlToDate" runat="server" ShowOnlyPreviousDates="True" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel ID="lblCVUStatus" runat="server" meta:resourcekey="lblCVUStatus" AssociatedControlID="ddlCVUStatus" />
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBDropDownList runat="server" ID="ddlCVUStatus" />
                </VP:VBValueCell>
            </VP:VBRow>
        </VP:VBTable>
    </VP:VBPanel>
    <VP:VBPanel ID="VBPanel2" runat="server" SkinID="Flip">
        <VP:VBButton runat="server" ID="btnDisplay" OnClick="btnDisplay_Click" meta:resourcekey="btnDisplay" SkinID="Highlight" CausesValidation="False" />
    </VP:VBPanel>

    <VP:VBPanel ID="pnlUserSummary" runat="server" Visible="False">
        <VP:StepControl ID="scSummary" runat="server" meta:resourcekey="scSummary" />
        <VP:VBGridView ID="grdUsers" runat="server" AutoGenerateColumns="False" meta:resourcekey="grdTransactionList">
            <emptydatarowstyle forecolor="Black" />
            <columns>
                <VP:VBBoundField meta:resourcekey="PerformerBranchBoundField" DataField="PerformerBranchCode" HtmlEncode="False" />
                <VP:VBBoundField meta:resourcekey="BranchTotalBoundField" DataField="BranchTotal" HtmlEncode="False" />
                <VP:VBBoundField meta:resourcekey="ApprovedByCheckerBoundField" DataField="ApprovedByChecker" HtmlEncode="False" />
                <VP:VBBoundField meta:resourcekey="SubmittedBoundField" DataField="Submitted" HtmlEncode="False" />
                <VP:VBBoundField meta:resourcekey="RevertedToBranchBoundField" DataField="RevertedToBranch" HtmlEncode="False" />
                <VP:VBBoundField meta:resourcekey="ResubmittedBoundField" DataField="Resubmitted" HtmlEncode="False" />
                <VP:VBBoundField meta:resourcekey="CompletedBoundField" DataField="Completed" HtmlEncode="False" />
                                
            </columns>
            </VP:VBGridView>
    </VP:VBPanel>
    


    <VP:VBPanel ID="VBPanel3" runat="server">
        <VP:StepControl ID="scTransactions" runat="server" meta:resourcekey="scTransactions" Visible="false" />
        <VP:VBGridView ID="grdTransactionList" runat="server" PageSize="10" AllowPaging="True" AutoGenerateColumns="False" meta:resourcekey="grdTransactionList" OnRowDataBound="grdTransactionList_RowDataBound" OnRowCommand="grdTransactionList_RowCommand">
            <emptydatarowstyle forecolor="Black" />
            <columns>
                <VP:VBTemplateField meta:resourcekey="BranchBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litBranch"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                <VP:VBTemplateField meta:resourcekey="ChannelBoundField" Visible="False">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litChannel"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                <VP:VBTemplateField meta:resourcekey="TxnNameBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litTransactionDescription"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                <VP:VBBoundField meta:resourcekey="TransactionReferenceNumberBoundField" DataField="TransactionReferenceNumber" />
                <%--<VP:VBBoundField meta:resourcekey="TransactionDateBoundField" DataField="TransactionDate" />--%>
                <VP:VBTemplateField meta:resourcekey="TransactionDateBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litTransactionDate"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                <VP:VBTemplateField meta:resourcekey="CVUStatusBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litCVUStatus"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                <VP:VBBoundField meta:resourcekey="HostReferenceNumberBoundField" DataField="HostReferenceNumber" />
                <VP:VBBoundField meta:resourcekey="PerformerIdentityBoundField" DataField="PerformerIdentity" />
                
                <VP:VBTemplateField meta:resourcekey="CustomerHasOralAgreementField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litCustomerHasOralAgreement"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                  <VP:VBTemplateField meta:resourcekey="OriginalDocReceivedField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litOriginalDocReceived"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <VP:VBBoundField meta:resourcekey="VerifiedByBoundField" DataField="ApproverIdentity" Visible="False" />
                <VP:VBTemplateField meta:resourcekey="RiskStatusBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litRiskStatus"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                <VP:VBBoundField meta:resourcekey="CVUCommentsBoundField" DataField="CVUComments" />
                <VP:VBTemplateField meta:resourcekey="DetailsBoundField">
                    <ItemTemplate>
                        <VP:VBImageButton runat="server" ID="btnDetails" CommandName="Details" SkinID="Details" CommandArgument='<%# Bind("TransactionReferenceNumber") %>' OnClientClick="showOverlay();return true;" />
                    </ItemTemplate>
                    <ItemStyle Width="16px" HorizontalAlign="Center"></ItemStyle>
                </VP:VBTemplateField>
            </columns>
        </VP:VBGridView>
    </VP:VBPanel>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="Server">
    <VP:NavigationButtonControl ID="ctlNavigationButton" meta:resourcekey="ctlNavigationButton" runat="server" CloseButtonVisible="True" />
</asp:Content>
