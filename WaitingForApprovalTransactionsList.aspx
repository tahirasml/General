<%@ Page Language="C#" MasterPageFile="~/MasterPage/VeriBranchMasterPage.master" AutoEventWireup="true" CodeFile="WaitingForApprovalTransactionsList.aspx.cs" Inherits="LocalOperations_WaitingForApprovalTransactionsList" %>

<%@ Register Src="~/Controls/DatePickerControl.ascx" TagName="DatePickerControl" TagPrefix="VP" %>


<asp:Content ID="Content1" ContentPlaceHolderID="transactionContent" runat="Server">
    
<script language="javascript" type="text/javascript">
    function checkRadioBtn(id) {
        var gv = document.getElementById('<%=grdTransactionList.ClientID %>');

        for (var i = 1; i < gv.rows.length; i++) {
            var radioBtn = gv.rows[i].cells[0].getElementsByTagName("input");

            // Check if the id not same
            if (radioBtn[0].id != id.id) {
                radioBtn[0].checked = false;
            }
        }
    }
</script>

    <VP:VBPanel ID="VBPanel1" runat="server">
        <VP:StepControl ID="scSearchFilter" runat="server" meta:resourcekey="scSearchFilter" />
        <VP:VBTable ID="VBTable1" runat="server">
            <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblTransactionName" meta:resourcekey="lblTransactionName" AssociatedControlID="ddlTransactionName"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBDropDownList runat="server" ID="ddlTransactionName" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblFromDate" meta:resourcekey="lblFromDate" AssociatedControlID="ctlFromDate:txtDate"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:DatePickerControl ID="ctlFromDate" runat="server" ShowOnlyPreviousDates="True" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel ID="lblToDate" runat="server" meta:resourcekey="lblToDate" AssociatedControlID="ctlToDate:txtDate"/>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:DatePickerControl ID="ctlToDate" runat="server" ShowOnlyPreviousDates="True" />
                </VP:VBValueCell>
            </VP:VBRow>
            <VP:VBRow>
                <VP:VBCaptionCell>
                    <VP:VBLabel ID="lblTransactionStatus" runat="server" meta:resourcekey="lblTransactionStatus" AssociatedControlID="ddlTransactionStatus"/>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBDropDownList runat="server" ID="ddlTransactionStatus" />
                </VP:VBValueCell>
                 <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblAccountNumber" meta:resourcekey="lblAccountNumber" AssociatedControlID="txtAccountNumber"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBTextBox runat="server" ID="txtAccountNumber" />
                </VP:VBValueCell>
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblTransactionReferenceNumber" meta:resourcekey="lblTransactionReferenceNumber" AssociatedControlID="txtTransactionReferenceNumber"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBTextBox runat="server" ID="txtTransactionReferenceNumber" />
                </VP:VBValueCell>
            </VP:VBRow>
            <VP:VBRow>
              
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lblPerformerIdentity" meta:resourcekey="lblPerformerIdentity" AssociatedControlID="txtPerformerIdentity"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBTextBox runat="server" ID="txtPerformerIdentity" />
                </VP:VBValueCell>
                   <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAmountFrom" meta:resourcekey="lblAmountFrom" AssociatedControlID="ctlAmountFrom"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:AmountPicker runat="server" ID="ctlAmountFrom" ShowCurrency="False" meta:resourcekey="ctlAmountFrom" RequiredFieldValidatorEnabled="False"></VP:AmountPicker>
                        </VP:VBValueCell>
                        <VP:VBCaptionCell>
                            <VP:VBLabel runat="server" ID="lblAmountTo" meta:resourcekey="lblAmountTo" AssociatedControlID="ctlAmountTo"></VP:VBLabel>
                        </VP:VBCaptionCell>
                        <VP:VBValueCell>
                            <VP:AmountPicker runat="server" ID="ctlAmountTo" ShowCurrency="False" meta:resourcekey="ctlAmountTo" RequiredFieldValidatorEnabled="False"></VP:AmountPicker>
                        </VP:VBValueCell>
     
            </VP:VBRow>

       <VP:VBRow>

         <VP:VBCaptionCell>
             <VP:VBLabel runat="server" ID="lblApprovalDepartment" meta:resourcekey="lblApprovalDepartment" AssociatedControlID="ddlApprovalDepartment"></VP:VBLabel>
        </VP:VBCaptionCell>
         <VP:VBValueCell ID="VBValueCell2" runat="server">
              <VP:VBDropDownList runat="server" ID="ddlApprovalDepartment" />
          </VP:VBValueCell>

           <VP:VBCaptionCell>
               <VP:VBLabel runat="server" ID="lblTransactionType" meta:resourcekey="lblTransactionType" AssociatedControlID="ddlTransactionName"></VP:VBLabel>
          </VP:VBCaptionCell>
         <VP:VBValueCell ID="VBValueCell1" runat="server">

             <VP:VBCheckBox ID="cbOverDrawn" Enabled="true" Checked="true"  runat="server" />
            <VP:VBLabel runat="server" ID="lblOverDrawn" meta:resourcekey="lblOverDrawn" IsRequired="False"></VP:VBLabel>&nbsp;&nbsp;&nbsp;

             <VP:VBCheckBox ID="cbExchangeRate" Enabled="true" Checked="true"   runat="server" />
             <VP:VBLabel runat="server" ID="lblExchangeRate" meta:resourcekey="lblExchangeRate" IsRequired="False"></VP:VBLabel>&nbsp;&nbsp;&nbsp;
             
              
             <VP:VBCheckBox ID="cbValueDate" Enabled="true" Checked="true" runat="server" />
            <VP:VBLabel runat="server" ID="lblValueDate" meta:resourcekey="lblValueDate"  IsRequired="False"></VP:VBLabel>

            </VP:VBValueCell>


    
            <VP:VBCaptionCell>
             
          </VP:VBCaptionCell>
         <VP:VBValueCell ID="VBValueCell3" runat="server">
            </VP:VBValueCell>

 
            
    </VP:VBRow>


        </VP:VBTable>
    </VP:VBPanel>

    <VP:VBPanel ID="VBPanel2" runat="server" SkinID="Flip">
        <VP:VBButton runat="server" ID="btnCancelRequest" Visible="false" OnClick="btnCancelRequest_Click" meta:resourcekey="btnCancelRequest" SkinID="Highlight" CausesValidation="False" OnClientClick="showOverlay();" />
        <VP:VBButton runat="server" ID="btnInquireStatus" Visible="false" OnClick="btnInquireStatus_Click" meta:resourcekey="btnInquireStatus" SkinID="Highlight" CausesValidation="False" OnClientClick="showOverlay();" />
        <VP:VBButton runat="server" ID="btnDisplay" OnClick="btnDisplay_Click" meta:resourcekey="btnDisplay" SkinID="Highlight" CausesValidation="False" />

    </VP:VBPanel>

    
    

    <VP:VBPanel ID="VBPanel3" runat="server">

        <VP:StepControl ID="ReferCardRequestStatusStepControl" runat="server" meta:resourcekey="ReferCardRequestStatusStep" />

        <VP:VBGridView ID="grdTransactionList" runat="server" PageSize="10" AllowPaging="True" AutoGenerateColumns="False" meta:resourcekey="grdTransactionList" OnRowDataBound="grdTransactionList_RowDataBound" OnRowCommand="grdTransactionList_RowCommand">
            <EmptyDataRowStyle ForeColor="Black" />
            <Columns>

               <VP:VBTemplateField meta:resourcekey="BranchStatusBoundField">
                    <ItemTemplate>
                        <%--<VP:VBImageButton runat="server" ID="btnDetails" CommandName="details" SkinID="Details" CommandArgument='<%# Bind("TransactionReferenceNumber") %>' OnClientClick="showOverlay();return true;" />--%>
                        <VP:VBRadioButton ID="btnBranchStatus" runat="server" OnCheckedChanged="rbSelector_CheckedChanged" AutoPostBack="true" GroupName="SelectGroup" onclick="checkRadioBtn(this);" />
                    </ItemTemplate>
                    <ItemStyle Width="16px" HorizontalAlign="Center"></ItemStyle>
                </VP:VBTemplateField>

                <VP:VBTemplateField meta:resourcekey="ApprovalDepartmentBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litApprovalDepartment"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>


                <VP:VBTemplateField meta:resourcekey="BranchBoundField" Visible="False">
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
                         <VP:VBTemplateField meta:resourcekey="TransactionReferenceNumberBoundField" Visible="False">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litReferenceNumber"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>


                 <VP:VBTemplateField meta:resourcekey="AccountNumberBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litAccountNumber"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <%--<VP:VBBoundField meta:resourcekey="TransactionReferenceNumberBoundField" DataField="TransactionReferenceNumber" />--%>
                <%--<VP:VBBoundField meta:resourcekey="TransactionDateBoundField" DataField="TransactionDate" />--%>
                <VP:VBTemplateField meta:resourcekey="TransactionDateBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litTransactionDate"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <VP:VBTemplateField meta:resourcekey="ReceiveDateBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litReceiveDate"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                <VP:VBTemplateField meta:resourcekey="TransactionStatusBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litTransactionStatus"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                  <VP:VBTemplateField meta:resourcekey="ExpiryDateTimeBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litExpiryDateTime"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                 <VP:VBTemplateField meta:resourcekey="BranchTxnStatusBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litBranchTxnStatus"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                                 
                <VP:VBTemplateField meta:resourcekey="RC_ReferenceNumberBoundField" Visible="False">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litRC_ReferenceNumber"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                
                <VP:VBTemplateField meta:resourcekey="TransactionAmountBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litTransactionAmount"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>


            <VP:VBBoundField meta:resourcekey="ExchangeRateBoundField" DataField="ExchangeRate" />
                <VP:VBTemplateField meta:resourcekey="ValueDateBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litValueDate"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                


                 <VP:VBTemplateField meta:resourcekey="SpecialExchangeRateBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litSpecialExchangeRate"></VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>

                
                 <VP:VBTemplateField meta:resourcekey="SpecialValueDateBoundField">
                    <ItemTemplate>
                        <VP:VBLiteral runat="server" ID="litSpecialValueDate"> </VP:VBLiteral>
                    </ItemTemplate>
                </VP:VBTemplateField>
                
                <VP:VBBoundField meta:resourcekey="PerformerIdentityBoundField" DataField="PerformerIdentity" Visible="False" />
                <VP:VBBoundField meta:resourcekey="VerifiedByBoundField" DataField="ApproverIdentity" Visible="False" />
                
                <VP:VBBoundField meta:resourcekey="SecondaryAuthorizerBoundField" DataField="SecondaryApproverIdentity" Visible="False" />
                
                <VP:VBBoundField meta:resourcekey="CommentsBoundField" DataField="Comments" Visible="False" />
                <%--<VP:VBTemplateField meta:resourcekey="ResolveBoundField">
                    <ItemTemplate>
                        <VP:VBImageButton runat="server" ID="btnResolvePending" CommandName="inquirestatus" SkinID="Resolve" CommandArgument='<%# Bind("TransactionReferenceNumber") %>' OnClientClick="showOverlay();return true;" Visible="false" />
                    </ItemTemplate>
                    <ItemStyle Width="16px" HorizontalAlign="Center"></ItemStyle>
                </VP:VBTemplateField>--%>

                <VP:VBTemplateField meta:resourcekey="DetailsBoundField">
                    <ItemTemplate>
                        <VP:VBImageButton runat="server" ID="btnDetails" CommandName="details" SkinID="Details" CommandArgument='<%# Bind("TransactionReferenceNumber") %>' OnClientClick="showOverlay();return true;" />
                    </ItemTemplate>
                    <ItemStyle Width="16px" HorizontalAlign="Center"></ItemStyle>
                </VP:VBTemplateField>

                   <VP:VBTemplateField  Visible="False">
                    <ItemTemplate>
                        <VP:VBHiddenField runat="server" ID="hidRC_IsOverDrawnRequested" />
                        <VP:VBHiddenField runat="server" ID="hidRC_IsNewValueDateRequested" />
                        <VP:VBHiddenField runat="server" ID="hidRC_IsNewExchangeRateRequested" />
                        <VP:VBHiddenField runat="server" ID="hidRC_ApproverID" />
                        <VP:VBHiddenField runat="server" ID="hidRC_RefNumber" />
                        <VP:VBHiddenField runat="server" ID="hid_ApproverComments" />
                        <VP:VBHiddenField runat="server" ID="hid_ApprovalDepartment" />
                        
                        <VP:VBHiddenField runat="server" ID="hid_Reserved1" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved2" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved3" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved4" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved5" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved6" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved7" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved8" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved9" />
                        <VP:VBHiddenField runat="server" ID="hid_Reserved10" />

                    </ItemTemplate>
                </VP:VBTemplateField>

            </Columns>
        </VP:VBGridView>
        </VP:VBPanel>

        <VP:VBPanel ID="pnBranchStatus" runat="server">

        <VP:StepControl ID="BranchStatusStep" runat="server" meta:resourcekey="BranchStatusStep" Visible="false"/>

         <VP:VBTable ID="tblBranchStatus" runat="server" Visible="false">
            <VP:VBRow>

                  <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_TransactionName" meta:resourcekey="lbl_BS_TransactionName" AssociatedControlID="txt_BS_TransactionName"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_TransactionName" />
                </VP:VBValueCell>

              
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_ReferenceNumber" meta:resourcekey="lbl_BS_ReferenceNumber" AssociatedControlID="txt_BS_ReferenceNumber"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_ReferenceNumber"  />
                </VP:VBValueCell>

                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_TransactionDate" meta:resourcekey="lbl_BS_TransactionDate" AssociatedControlID="txt_BS_TransactionDate"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_TransactionDate" />
                </VP:VBValueCell>

            </VP:VBRow>

             
              <VP:VBRow>

                
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_TransactionAmount" meta:resourcekey="lbl_BS_TransactionAmount" AssociatedControlID="txt_BS_TransactionAmount"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_TransactionAmount"  />
                </VP:VBValueCell>
                    
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_ExchangeRate" meta:resourcekey="lbl_BS_ExchangeRate" AssociatedControlID="txt_BS_ExchangeRate"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_ExchangeRate"/>
                </VP:VBValueCell>
              
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_ValueDate" meta:resourcekey="lbl_BS_ValueDate" AssociatedControlID="txt_BS_ValueDate"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_ValueDate"  />
                </VP:VBValueCell>


            </VP:VBRow>

              <VP:VBRow>

                
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_TransactionStatus" meta:resourcekey="lbl_BS_TransactionStatus" AssociatedControlID="txt_BS_TransactionStatus"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_TransactionStatus"  />
                </VP:VBValueCell>
                    
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_HostReferenceNumber" meta:resourcekey="lbl_BS_HostReferenceNumber" AssociatedControlID="txt_BS_HostReferenceNumber"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_HostReferenceNumber"  />
                </VP:VBValueCell>
              
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_AccountNumber" meta:resourcekey="lbl_BS_AccountNumber" AssociatedControlID="txt_BS_AccountNumber"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_AccountNumber" />
                </VP:VBValueCell>


            </VP:VBRow>

              <VP:VBRow>
              
               
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_TellerID" meta:resourcekey="lbl_BS_TellerID" AssociatedControlID="txt_BS_TellerID"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_TellerID"  />
                </VP:VBValueCell>
              
                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_ApproverIdentity" meta:resourcekey="lbl_BS_ApproverIdentity" AssociatedControlID="txt_BS_TransactionStatus"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_ApproverIdentity"  />
                </VP:VBValueCell>

                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_SecondaryApproverIdentity" meta:resourcekey="lbl_BS_SecondaryApproverIdentity" AssociatedControlID="txt_BS_TellerID"></VP:VBLabel>
                </VP:VBCaptionCell>
                <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_SecondaryApproverIdentity"/>
                </VP:VBValueCell>
            </VP:VBRow>
             
         <VP:VBRow>

                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_RC_RefNumber" meta:resourcekey="lbl_BS_RC_RefNumber" AssociatedControlID="lbl_BS_RC_RefNumber"></VP:VBLabel>
               </VP:VBCaptionCell>
               <VP:VBValueCell>
                   <VP:VBLabel runat="server" ID="txt_BS_RC_RefNumber"  />
                </VP:VBValueCell>

              <VP:VBCaptionCell>
               <VP:VBLabel runat="server" ID="lbl_BS_TransactionType" meta:resourcekey="lblTransactionType" AssociatedControlID="ddlTransactionName"></VP:VBLabel>
            </VP:VBCaptionCell>

             <VP:VBValueCell ID="VBValueCell4" runat="server">

                 <VP:VBCheckBox ID="cb_BS_OverDrawn" Enabled="false" Checked="true"  ReadOnly="true" runat="server" />
                <VP:VBLabel runat="server" ID="lbl_BS_OverDrawn" meta:resourcekey="lblOverDrawn" IsRequired="False"></VP:VBLabel>&nbsp;&nbsp;&nbsp;

                 <VP:VBCheckBox ID="cb_BS_ExchangeRate" Enabled="false" Checked="false"  ReadOnly="true"  runat="server" />
                 <VP:VBLabel runat="server" ID="VBLabel2" meta:resourcekey="lblExchangeRate" IsRequired="False"></VP:VBLabel>&nbsp;&nbsp;&nbsp;
                           
                 <VP:VBCheckBox ID="cb_BS_ValueDate" Enabled="false" Checked="false"  ReadOnly="true" runat="server" />
                    <VP:VBLabel runat="server" ID="VBLabel3" meta:resourcekey="lblValueDate"  IsRequired="False"></VP:VBLabel>
            </VP:VBValueCell>

                <VP:VBCaptionCell>
                    <VP:VBLabel runat="server" ID="lbl_BS_ApprovalDepartmentUserID" meta:resourcekey="lbl_BS_ApprovalDepartmentUserID" AssociatedControlID="txt_BS_ApprovalDepartmentUserID"></VP:VBLabel>
             </VP:VBCaptionCell>
             <VP:VBValueCell>
                <VP:VBLabel runat="server" ID="txt_BS_ApprovalDepartmentUserID"  />
            </VP:VBValueCell>
          
        </VP:VBRow>
        <VP:VBRow>

            <VP:VBCaptionCell>
                <VP:VBLabel runat="server" ID="lbl_BS_CallReferenceNumber" meta:resourcekey="lbl_BS_CallReferenceNumber"></VP:VBLabel>
             </VP:VBCaptionCell>
             <VP:VBValueCell>
               <VP:VBLabel runat="server" ID="txt_BS_CallReferenceNumber" ></VP:VBLabel>
            </VP:VBValueCell>

            <VP:VBCaptionCell>
                 <VP:VBLabel runat="server" ID="lbl_BS_RejectReason" meta:resourcekey="lbl_BS_RejectReason" AssociatedControlID="txt_BS_RejectReason"></VP:VBLabel>
            </VP:VBCaptionCell>
            <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_RejectReason" />
            </VP:VBValueCell>

            <VP:VBCaptionCell>
                 <VP:VBLabel runat="server" ID="lbl_BS_Reason" meta:resourcekey="lbl_BS_Reason" AssociatedControlID="ddlTransactionName"></VP:VBLabel>
            </VP:VBCaptionCell>
            <VP:VBValueCell>
                    <VP:VBLabel runat="server" ID="txt_BS_Reason"  />
            </VP:VBValueCell>
    
         </VP:VBRow>

            <VP:VBRow>

            <VP:VBCaptionCell>
                <VP:VBLabel runat="server" ID="lbl_Branch_Remarks" meta:resourcekey="lbl_Branch_Remarks"></VP:VBLabel>
             </VP:VBCaptionCell>
             <VP:VBValueCell ColumnSpan="3">
               <VP:VBLabel runat="server" ID="txt_Branch_Remarks" ></VP:VBLabel>
            </VP:VBValueCell>

            <VP:VBCaptionCell>
            </VP:VBCaptionCell>
            <VP:VBValueCell>
            </VP:VBValueCell>
    
         </VP:VBRow>
             
        </VP:VBTable>
        </VP:VBPanel>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="Server">
    <VP:NavigationButtonControl ID="ctlNavigationButton" runat="server" CloseButtonVisible="True"  />
</asp:Content>
