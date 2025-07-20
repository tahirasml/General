<%@ Page Language="C#" MasterPageFile="~/MasterPage/VeriBranchMasterPage.master" AutoEventWireup="true" CodeFile="CVUTransactionDetails.aspx.cs" Inherits="CVUTransactionDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="transactionContent" runat="Server">

   

    <VP:VBPanel runat="server">
        <VP:StepControl ID="scTransactionDetails" runat="server" meta:resourcekey="scTransactionDetails" />
        <VP:VBTable runat="server">
            <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblReferenceNo" meta:resourcekey="lblReferenceNo"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblReferenceNoValue"></VP:VBLabel>
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblTransactionDate" meta:resourcekey="lblTransactionDate"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblTransactionDateValue"></VP:VBLabel>
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblTransactionType" meta:resourcekey="lblTransactionType"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblTransactionTypeValue"></VP:VBLabel>
                </VP:VBValueCell>
            </VP:VBRow>
            <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblChannel" meta:resourcekey="lblChannel"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblChannelValue"></VP:VBLabel>
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblCVUStatus" meta:resourcekey="lblCVUStatus"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblCVUStatusValue"></VP:VBLabel>
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblHostReferenceNumber" meta:resourcekey="lblHostReferenceNumber"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblHostReferenceNumberValue"></VP:VBLabel>
                </VP:VBValueCell>
            </VP:VBRow>
              <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblOverrideReason" meta:resourcekey="lblOverrideReason" SkinID="Warning"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell ColumnSpan="5">
                    <VP:VBLabel runat="server" ID="lblOverrideReasonValue" SkinID="Warning"></VP:VBLabel>
                </VP:VBValueCell>
         
            </VP:VBRow>

        </VP:VBTable>
    </VP:VBPanel>

    <iframe runat="server" id="ifrTransactionDetails" style="overflow: hidden; height: 100%; width: 100%" height="100%" width="100%" frameborder="0"></iframe>

    <script language="javascript" type="text/javascript">
        function iframeOnLoad(iframe) {
            if (iframe == null) {
                iframe = document.getElementById('<%=ifrTransactionDetails.ClientID%>');
            }
            iframe.style.height = (iframe.contentWindow.document.body.offsetHeight + 5) + 'px';

            hideOverlay();
        }

        //$(document).ready(function () { showOverlay(); });
    </script>

    <VP:VBPanel ID="VBPanel2" runat="server" >
        <VP:StepControl ID="StepControl2" runat="server" meta:resourcekey="scCVUApproval" />
        
     <VP:VBTable ID="VBTable1" runat="server">

            <VP:VBRow>
                 <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblCustomerName" meta:resourcekey="lblCustomerName"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblCustomerNameValue"></VP:VBLabel>
                 </VP:VBValueCell>

                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblCustomerSegment" meta:resourcekey="lblCustomerSegment"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblCustomerSegmentValue"></VP:VBLabel>
                 </VP:VBValueCell>

                 <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblCustomerRank" meta:resourcekey="lblCustomerRank"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblCustomerRankValue"></VP:VBLabel>
                 </VP:VBValueCell>
                
                
            </VP:VBRow>
     </VP:VBTable>

    </VP:VBPanel>

    <VP:VBPanel ID="pnlComments" runat="server" >
               
         <VP:VBTable ID="VBTable2" runat="server">
            <VP:VBRow>

                <VP:VBCaptionCell>
                    <VP:VBLabel ID="lblErrorType" runat="server" meta:resourcekey="lblErrorType" AssociatedControlID="ddlErrorType"  />
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBDropDownList runat="server" ID="ddlErrorType" Enabled="False" />
                </VP:VBValueCell>

                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblRiskStatus" meta:resourcekey="lblRiskStatus" AssociatedControlID="ddlRiskStatusValue"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="lblRiskStatusValue" Visible="False"/>
                    <VP:VBDropDownList runat="server" ID="ddlRiskStatusValue" Visible="False"/>
                    <VP:VBRequiredFieldValidator runat="server" ID="rfvddlRiskStatusValue" meta:resourcekey="rfvddlRiskStatusValue" ControlToValidate="ddlRiskStatusValue" Enabled="False" ValidationGroup="CVU"></VP:VBRequiredFieldValidator>
                </VP:VBValueCell>
                
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblComments" meta:resourcekey="lblComments"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBTextBox runat="server" ID="txtComments"></VP:VBTextBox>
                    <VP:VBRequiredFieldValidator runat="server" ID="rfvtxtComments" meta:resourcekey="rfvtxtComments" ControlToValidate="txtComments" Enabled="False" ValidationGroup="CVU"></VP:VBRequiredFieldValidator>
                </VP:VBValueCell>
      
            </VP:VBRow>

                 <VP:VBRow>

                <VP:VBCaptionCell>
                    <VP:VBLabel ID="lblOriginalDocReceived" runat="server" meta:resourcekey="lblOriginalDocReceived" AssociatedControlID="cbOriginalDocReceived"  />
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBCheckBox runat="server" ID="cbOriginalDocReceived" Enabled="False"/>
                </VP:VBValueCell>

                <VP:VBCaptionCell>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                </VP:VBValueCell>

                <VP:VBCaptionCell>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                </VP:VBValueCell>


                </VP:VBRow>



        </VP:VBTable>
    </VP:VBPanel>



<VP:ValidationSummaryControl runat="server" ID="ctlValidationSummaryCVU" ValidationGroup="CVU"></VP:ValidationSummaryControl>

        <VP:VBGridView ID="grdCVUAuditLog" runat="server" AutoGenerateColumns="False" meta:resourcekey="grdCVUAuditLog" OnRowDataBound="grdCVUAuditLog_RowDataBound">
            <EmptyDataRowStyle ForeColor="Black" />
            <Columns>
                <VP:VBTemplateField meta:resourcekey="CVUStatusBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litCVUStatus"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <VP:VBTemplateField meta:resourcekey="CVUErrorTypeBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litCVUErrorType"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <VP:VBBoundField meta:resourcekey="CommentsBoundField" DataField="Comments" />
                <VP:VBTemplateField meta:resourcekey="RiskStatusBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litRiskStatus"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                
                 <VP:VBTemplateField meta:resourcekey="IsOriginalDocumentReceived">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litIsOriginalDocumentReceived"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <VP:VBBoundField meta:resourcekey="PerformerIdentityBoundField" DataField="PerformerIdentity" />
                <VP:VBTemplateField meta:resourcekey="UpdatedOnBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litUpdatedOn"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
            </Columns>
        </VP:VBGridView>


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="Server">
    <VP:NavigationButtonControl ID="ctlNavigationButton" meta:resourcekey="ctlNavigationButton" runat="server" DocumentsButtonVisible="True" CloseButtonVisible="True" SignatureButtonVisible="True" BackButtonVisible="True" BackUrl="TransactionsList.aspx?m=1" OnReceiptButtonClicked="ctlNavigationButton_ReceiptButtonClicked" OnAuthorizeButtonClicked="ctlNavigationButton_AuthorizeButtonClicked" OnRejectButtonClicked="ctlNavigationButton_RejectButtonClicked" OnResubmitButtonClicked="ctlNavigationButton_ResubmitButtonClicked" OnAcceptRejectionButtonClicked="ctlNavigationButton_AcceptRejectionButtonClicked" OnSignatureButtonClicked ="ctlNavigationButton_SignatureButtonClicked" OnReserved1ButtonClicked = "ctlNavigationButton_ReservedButtonClicked" />
    <%--ModifyButtonVisible="True" OnModifyButtonClicked="ModifyButtonClicked"--%>
</asp:Content>
