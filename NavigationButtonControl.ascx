<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NavigationButtonControl.ascx.cs" Inherits="NavigationButtonControl" EnableViewState="true" %>

<script language="javascript" type="text/javascript" src="~/Content/js/Controls/NavigationButtonControl.js"></script>
<%@ Register Src="~/Controls/WarningDisplayControl.ascx" TagName="WarningDisplayControl" TagPrefix="VP" %>


<script language="javascript" type="text/javascript">

    function openSVS(svsUrl) {
        if (svsUrl != '') {
            window.open(svsUrl, "Signature", "width=1024, height=800,top=0,left=0, scrollbars=1");

        }
        return false;
    }
    
    function openSVS(svsUrl) {
        if (svsUrl != '') {
            window.open(svsUrl, "Signature", "width=1024, height=800,top=0,left=0, scrollbars=1");
            
        }
        return false;
    }

    function initiateReceiptPrint(scan) {

        var hidScanOption = document.getElementById('<%=hidScanOption.ClientID%>');

        if (hidScanOption != null) {
            hidScanOption.value = scan ? "1" : "0";
            __doPostBack('<%=hidScanOption.ClientID %>', '');
        }
    }
    
    function clickNextButton() {
        var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
        if (btnNextNavigation != null) {
            btnNextNavigation.click();
        }
    }

    function gatherReasons() {

        var reasons2Array = getReasons();

        var reasons1 = document.getElementById('<%=hidCaseOpenReasons.ClientID%>').value;
        var reasons1Array = reasons1.split('|');

        reasons1Array.clean('');
        reasons2Array.clean('');

        var totalReasons = reasons1Array.concat(reasons2Array).unique();
        var willOpenCase = totalReasons.length > 0;
        if (willOpenCase) {

            AddReasonsToTable(totalReasons);
            var lblMessage = document.getElementById('<%=ctlCaseOpenMessage.MessageLabelClientID%>');
            if (lblMessage != null)
                lblMessage.innerHTML = document.getElementById('<%=hidCaseOpenMsgHeading.ClientID%>').value;

            $find("<%=mpeExecute.ClientID%>").show();
        }
        else {
            document.getElementById('<%=btnNextNavigation.ClientID%>').click();
        }
    }

    function AddReasonsToTable(arrTotalReasons) {
        var table = document.getElementById('<%=grdReasons.ClientID%>');

        removeRows(table);

        for (var index = 1; index <= arrTotalReasons.length; index++) {
            var row = table.insertRow(index);
            row.className = "GridViewRowStyle";
            var cell0 = row.insertCell(0);
            var cell1 = row.insertCell(1);
            cell0.innerHTML = index;
            cell1.style.textAlign = "left";
            var element1 = document.createElement("span");
            element1.innerHTML = arrTotalReasons[index - 1];
            cell1.appendChild(element1);
        }

    }

    function onConfirmExecute() {
        $find("<%=mpeExecute.ClientID%>").hide();

    }

    function removeRows(table) {
        if (table != null & table.rows != null) {
            var rowCount = table.rows.length;
            for (var i = 1; i < rowCount; i++) {
                table.deleteRow(i);
                rowCount--;
                i--;
            }
        }
    }

    function <%=btnPopupScan.ClientID%>_showScan(loginKey) {
        showOverlay();
        var ifr = document.getElementById('<%=ifrScan.ClientID%>');
        var pnlScan = document.getElementById('<%=pnlScan.ClientID%>');
        
        if (ifr != null) {
            ifr.onload = function () { hideOverlay(); };
            ifr.src = getAppName() + "Scanner/ScanDocs.aspx?AppNo=<%=AppNo%>&CIF=<%=CifNo%>&LoginTokenKey=" + loginKey;
            ifr.style.display = "";
            $(pnlScan)[0].className = "modalPopup scanVisible";
        } else {
            hideOverlay();
        }
    }

    function <%=btnPopupChecklist.ClientID%>_showChecklist(loginKey) {
        showOverlay();
        var ifr = document.getElementById('<%=irfChecklist.ClientID%>');
        var pnlChecklist = document.getElementById('<%=pnlChecklist.ClientID%>');
        if (ifr != null) {
            ifr.onload = function () { hideOverlay(); };
            ifr.src = getAppName() + "Checklist/Checklist.aspx?LoginTokenKey=" + loginKey;
            ifr.style.display = "";
            $(pnlChecklist)[0].className = "modalPopup checklistVisible";
        } else {
            hideOverlay();
        }
    }

    function <%=btnPopupDocuments.ClientID%>_showDocuments(loginKey) {
        showOverlay();
        var ifr = document.getElementById('<%=ifrDocuments.ClientID%>');
        var pnlDocuments = document.getElementById('<%=pnlDocuments.ClientID%>');

        if (ifr != null) {
            ifr.onload = function () { hideOverlay(); };
            ifr.src = getAppName() + "Scanner/ViewDocuments.aspx?AppNo=<%=AppNo%>&CIF=<%=CifNo%>&LoginTokenKey=" + loginKey;
            ifr.style.display = "";
            $(pnlDocuments)[0].className = "modalPopup scanVisible";
        } else {
            hideOverlay();
        }
    }

    function hideScan() {
        var ifr = document.getElementById('<%=ifrScan.ClientID%>');
        var pnlScan = document.getElementById('<%=pnlScan.ClientID%>');
        if (ifr != null) {
            ifr.src = "";
            ifr.style.display = "none";
            $(pnlScan)[0].className = "modalPopup scan";
        }
        var mpeScan = document.getElementById('<%=mpeScan.ClientID%>_backgroundElement');
        if (mpeScan != null) {
            mpeScan.style.display = "none";
        }
    }

    function hideCheckList() {
        var ifr = document.getElementById('<%=irfChecklist.ClientID%>');
        var pnlChecklist = document.getElementById('<%=pnlChecklist.ClientID%>');
        if (ifr != null) {
            ifr.src = "";
            ifr.style.display = "none";
            $(pnlChecklist)[0].className = "modalPopup checklist";
        }
    }
    
    function closeDocuments() {
        var ifr = document.getElementById('<%=ifrDocuments.ClientID%>');
        var pnlDocuments = document.getElementById('<%=pnlDocuments.ClientID%>');
        if (ifr != null) {
            ifr.src = "";
            ifr.style.display = "none";
            $(pnlDocuments)[0].className = "modalPopup documents";
        }
        var mpeDocuments = document.getElementById('<%=mpeDocuments.ClientID%>_backgroundElement');
        if (mpeDocuments != null) {
            mpeDocuments.style.display = "none";
        }
    }

    function closeScan() {
        hideScan();
        var btnCloseScan = document.getElementById('<%=btnCloseScan.ClientID%>');
        //if (btnCloseScan)
        //    btnCloseScan.click();
    }

    function closeCheckList() {
        hideCheckList();
        var btncloseCheckList = document.getElementById('<%=btnCloseCheckList.ClientID%>');
        if (btncloseCheckList)
            btncloseCheckList.click();
    }

    function askForRepost() {

        alertModal(getErrorTitleText(), '<%=RepostTransactionMessage%>', '<%=NoButtonText%>', 0, [{ text: '<%=YesButtonText%>', click: function () { $(this).dialog('close'); retryBtnClicked(); } }]);
        var btnRetry = document.getElementById('<%=btnRetry.ClientID%>');
        if (btnRetry) {
            btnRetry.style.display = "";
        }


        return false;

    }
    
    function hideRetryButton() {

        var btnRetry = document.getElementById('<%=btnRetry.ClientID%>');
        if (btnRetry) {
            btnRetry.style.display = "none";
        }

        return false;

    }

    function showRejectApprovalRequestButton() {
        var btnRejectApprovalRequest = document.getElementById('<%=btnRejectApprovalRequest.ClientID%>');
        if (btnRejectApprovalRequest) {
            $(btnRejectApprovalRequest).css('display', 'inline-block');
        }

        return false;

    }

    function showComplianceButton() {

        var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
        var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
        var btnReferCardCall = document.getElementById('<%=btnReferCardCall.ClientID%>');
        var btnComplianceCall = document.getElementById('<%=btnComplianceCall.ClientID%>');
        
        if (btnSendAskCall) {
            $(btnSendAskCall).css('display', 'none');
        }
        if (btnNextNavigation) {
            $(btnNextNavigation).css('display', 'none');
        }
        if (btnReferCardCall) {
            $(btnReferCardCall).css('display', 'none');
        }
        if (btnComplianceCall) {
            $(btnComplianceCall).css('display', 'inline-block');
        }


            return false;

    }

    function showOnlyCloseButton() {

        var btnComplianceCall = document.getElementById('<%=btnComplianceCall.ClientID%>');
        var btnClose = document.getElementById('<%=btnClose.ClientID%>');
        var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
        var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
        var btnReferCardCall = document.getElementById('<%=btnReferCardCall.ClientID%>');
        var btnCancelNavigation = document.getElementById('<%=btnCancelNavigation.ClientID%>');
        var btnCallCenter = document.getElementById('<%=btnCallCenter.ClientID%>');

        if (btnComplianceCall) {
            $(btnComplianceCall).css('display', 'none');
        }

        if (btnSendAskCall) {
            $(btnSendAskCall).css('display', 'none');
        }
        if (btnNextNavigation) {
            $(btnNextNavigation).css('display', 'none');
        }
        if (btnCancelNavigation) {
            $(btnCancelNavigation).css('display', 'none');
        }
        if (btnReferCardCall) {
            $(btnReferCardCall).css('display', 'none');
        }
        if (btnCallCenter) {
            $(btnCallCenter).css('display', 'none');
        }
        if (btnClose) {
            $(btnClose).css('display', 'inline-block');
            }


    }


    function retryBtnClicked() {
        showOverlay();
        var btnRetryDummy = document.getElementById('<%=btnRetryDummy.ClientID%>');
        if (btnRetryDummy) {
            btnRetryDummy.click();
        }
    }

    function ClickButton(buttonClientID) {
        var btn = document.getElementById(buttonClientID);
        if (btn) {
            btn.click();
        }
    }

    function sendAskCall() {
        var isCallServerEnabled = '<%=IsAskEnabled %>';
        if (typeof callServer === 'function' && isCallServerEnabled == 'True') {
            callServer('SendAskCall');
        }
    }

    function onAskComplete() {
        if (typeof onAskCompleteCustom == 'function') {
            onAskCompleteCustom();
        } else {

            var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
            var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');

            if (btnSendAskCall) {
                $(btnSendAskCall).css('display', 'none');
            }
            if (btnNextNavigation) {
                $(btnNextNavigation).css('display', 'inline-block');
            }
        }
    }

    function onRejectApprovalRequestCallComplete() {

        var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
            var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
            var btnReferCardCall = document.getElementById('<%=btnReferCardCall.ClientID%>');
            var btnCancelNavigation = document.getElementById('<%=btnCancelNavigation.ClientID%>');
            var btnClose = document.getElementById('<%=btnClose.ClientID%>');


            if (btnSendAskCall) {
                $(btnSendAskCall).css('display', 'none');
            }
            if (btnNextNavigation) {
                $(btnNextNavigation).css('display', 'none');
            }
            if (btnReferCardCall) {
                $(btnReferCardCall).css('display', 'none');
            }
            if (btnCancelNavigation) {
                $(btnCancelNavigation).css('display', 'none');
            }
            if (btnClose) {
                $(btnClose).css('display', 'inline-block');

            }


    }

    function onReferCardCallComplete() {
        
            var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
            var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
            var btnReferCardCall = document.getElementById('<%=btnReferCardCall.ClientID%>');
            var btnCancelNavigation = document.getElementById('<%=btnCancelNavigation.ClientID%>');
            var btnClose = document.getElementById('<%=btnClose.ClientID%>');
                            

            if (btnSendAskCall) {
                $(btnSendAskCall).css('display', 'none');
            }
            if (btnNextNavigation) {
                $(btnNextNavigation).css('display', 'none');
            }
            if (btnReferCardCall) {
                $(btnReferCardCall).css('display', 'none');
            }
            if (btnCancelNavigation) {
                $(btnCancelNavigation).css('display', 'none');
            }
            if (btnClose) {
                $(btnClose).css('display', 'inline-block');
                                                
            }
        
        
    }

    function onReferCardCallFailed() {

        var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
         var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
         var btnReferCardCall = document.getElementById('<%=btnReferCardCall.ClientID%>');

         if (btnSendAskCall) {
             $(btnSendAskCall).css('display', 'none');
         }
         if (btnNextNavigation) {
             $(btnNextNavigation).css('display', 'none');
         }
         if (btnReferCardCall) {
             $(btnReferCardCall).css('display', 'inline-block');
         }

     }


    function onAskTextBoxChanged(obj) {
        if (!obj)
            return;

     
        var readonly = obj.getAttribute('readonly');
        if (readonly != null && readonly != '')
            return;

        onAskFieldChanged();
    }

    function onAskFieldChanged() {
        if (typeof onAskFieldChangedCustom == 'function') {
            onAskFieldChangedCustom();
        } else {

            var btnReferCardCall = document.getElementById('<%=btnReferCardCall.ClientID%>');

            ////if refer card is enabled then return.
            //if (btnReferCardCall) {
            //    //var btnReferCardCall_display = btnReferCardCall.offsetParent;
            //    if (btnReferCardCall.offsetParent !== null) {
            //        return;
            //    }
            //}
            
            var btnSendAskCall = document.getElementById('<%=btnSendAskCall.ClientID%>');
            var btnNextNavigation = document.getElementById('<%=btnNextNavigation.ClientID%>');
            var btnComplianceCall = document.getElementById('<%=btnComplianceCall.ClientID%>');
            var btnCallCenter = document.getElementById('<%=btnCallCenter.ClientID%>');
            
            if (btnSendAskCall) {
                $(btnSendAskCall).css('display', 'inline-block');
            }
            if (btnNextNavigation) {
                $(btnNextNavigation).css('display', 'none');
            }
            if (btnComplianceCall) {
                $(btnComplianceCall).css('display', 'none');
            }
            if (btnReferCardCall) {
                $(btnReferCardCall).css('display', 'none');
            }
            if (btnCallCenter) {
                $(btnCallCenter).css('display', 'none');
            }
         }

        }

           
    
</script>

<VP:VBHiddenField runat="server" ID="hidScanOption" OnValueChanged="hidScanOption_ValueChanged"/>

<ajaxToolkit:ModalPopupExtender ID="mpeScan" runat="server"
    BackgroundCssClass="modalBackground" Enabled="True" CancelControlID="btnCloseScan"
    PopupControlID="pnlScan" TargetControlID="btnPopupScan" DynamicServicePath="">
</ajaxToolkit:ModalPopupExtender>

<ajaxToolkit:ModalPopupExtender ID="mpeChecklist" runat="server"
    BackgroundCssClass="modalBackground" Enabled="True" CancelControlID="btnCloseCheckList"
    PopupControlID="pnlChecklist" TargetControlID="btnPopupChecklist" DynamicServicePath="">
</ajaxToolkit:ModalPopupExtender>

<asp:Panel ID="pnlScan" runat="server" CssClass="modalPopup scan" Visible="False">
    <iframe style="display: none;" frameborder="0" id="ifrScan" runat="server" class="ScanPopup" scrolling="auto" />
    <VP:VBButton UseSubmitBehavior="False" runat="server" ID="btnCloseScan" CausesValidation="False" Text="Close" Style="display: none" />
</asp:Panel>

<asp:Panel ID="pnlChecklist" runat="server" CssClass="modalPopup checklist" Visible="False">
    <iframe style="display: none;" frameborder="0" id="irfChecklist" runat="server" class="ChecklistPopup" />
    <VP:VBButton UseSubmitBehavior="False" runat="server" ID="btnCloseCheckList" CausesValidation="False" Text="Close" Style="display: none" />
</asp:Panel>

<ajaxToolkit:ModalPopupExtender ID="mpeDocuments" runat="server"
    BackgroundCssClass="modalBackground" Enabled="True" CancelControlID="btnCloseDocuments"
    PopupControlID="pnlDocuments" TargetControlID="btnPopupDocuments" DynamicServicePath="">
</ajaxToolkit:ModalPopupExtender>


<asp:Panel ID="pnlDocuments" runat="server" CssClass="modalPopup documents" Visible="False">
    <iframe style="display: none;" frameborder="0" id="ifrDocuments" runat="server" class="ScanPopup" scrolling="auto" />
    <VP:VBButton UseSubmitBehavior="False" runat="server" ID="btnCloseDocuments" CausesValidation="False" Text="Close" Style="display: none" />
</asp:Panel>

<VP:VBButton UseSubmitBehavior="False" ID="btnDefaultDummy" runat="server" Style="display: none" OnClientClick="return false;" />
<VP:VBPanel runat="server" ID="pnlNavigation" SkinID="Navigation">
    <table cellpadding="1" cellspacing="0" border="0" runat="server" class="tblNavigation" id="tblNavigation">
        <tr>
            <td class="tblNavigationBack">
                <VP:VBButton UseSubmitBehavior="False" ID="btnBackNavigation" runat="server" OnClientClick="DisableValidators();" CausesValidation="False" meta:resourcekey="navigationButtonControllerBackButton" OnClick="btnBackNavigation_Click" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnModifyNavigation" runat="server" CausesValidation="False" meta:resourcekey="navigationButtonControllerModifyButton" OnClick="btnModifyNavigation_Click" onmouseover="javascript:window.status='';return true;" Visible="false" />
            </td>
            <td style="vertical-align: bottom;" class="tblNavigationAll">
                
                <VP:VBButton UseSubmitBehavior="False" ID="btnSignature" runat="server" CausesValidation="False" meta:resourcekey="btnSignature" onmouseover="javascript:window.status='';return true;" Visible="False"/>
                <VP:VBButton UseSubmitBehavior="False" ID="btnReserved1" runat="server" CausesValidation="False" OnClick="btnReserved1_Click" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnAuthorize" runat="server" CausesValidation="False" OnClick="btnAuthorize_Click" meta:resourcekey="btnAuthorize" onmouseover="javascript:window.status='';return true;" Visible="False"/>
                <VP:VBButton UseSubmitBehavior="False" ID="btnReject" runat="server" CausesValidation="False" OnClick="btnReject_Click" meta:resourcekey="btnReject" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnResubmit" runat="server" CausesValidation="False" OnClick="btnResubmit_Click" meta:resourcekey="btnResubmit" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnAcceptRejection" runat="server" CausesValidation="False" OnClick="btnAcceptRejection_Click" meta:resourcekey="btnAcceptRejection" onmouseover="javascript:window.status='';return true;" Visible="False" />
                
                <VP:VBButton UseSubmitBehavior="False" ID="btnRejectApprovalRequest" runat="server" CausesValidation="False" OnClick="RejectApprovalRequestButton_Click" meta:resourcekey="btnRejectApprovalRequest" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="Highlight" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnCancelNavigation" runat="server" CausesValidation="False" OnClick="btnCancelNavigation_Click" meta:resourcekey="cancelButton" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnPopupScan" CausesValidation="False" runat="server" meta:resourcekey="Scan" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnPopupDocuments" CausesValidation="False" runat="server" meta:resourcekey="btnPopupDocuments" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnPopupChecklist" CausesValidation="False" runat="server" meta:resourcekey="Checklist" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnPrintCheque" runat="server" CausesValidation="false" OnClick="btnPrintCheque_Click" Visible="false" meta:resourcekey="btnPrintCheque" onmouseover="javascript:window.status='';return true;" SkinID="Highlight" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnPrintNavigation" runat="server" CausesValidation="false" OnClick="btnPrintNavigation_Click" Visible="false" meta:resourcekey="PrintAck" onmouseover="javascript:window.status='';return true;" SkinID="Highlight" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnReciptNavigation" runat="server" CausesValidation="false" OnClick="btnReciptNavigation_Click" Visible="false" meta:resourcekey="reciptButton" onmouseover="javascript:window.status='';return true;" SkinID="Highlight" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnScanNavigation" runat="server" Text="Scan Advice" Visible="false" meta:resourcekey="Scan" onmouseover="javascript:window.status='';return true;" SkinID="Highlight" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnNextNavigation" runat="server" OnClick="NextButton_Click" meta:resourcekey="navigationButtonControllerNextButton" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="Highlight" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnDummyExecute" runat="server" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="Highlight" Text="Execute" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnClose" runat="server" CausesValidation="False" meta:resourcekey="closeButton" onmouseover="javascript:window.status='';return true;" Visible="False" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnRetry" runat="server" Style="display: none" CausesValidation="False" onmouseover="javascript:window.status='';return true;" SkinID="Highlight" meta:resourcekey="btnRetry" Text="Retry" OnClientClick="return askForRepost();" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnRetryDummy" runat="server" Style="display: none" onmouseover="javascript:window.status='';return true;" Text="RetryDummy" OnClick="RetryButton_Click" />
                <%--<VP:VBButton UseSubmitBehavior="False" ID="btnSendAskCall" runat="server" meta:resourcekey="btnSendAskCall" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="ASK" OnClientClick="javascript:showOverlay(); sendAskCall(); return false;" />--%>
                <VP:VBButton UseSubmitBehavior="False" ID="btnSendAskCall" runat="server" OnClick="AskButton_Click" meta:resourcekey="btnSendAskCall" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="ASK" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnReferCardCall" runat="server" OnClick="ReferCardButton_Click" meta:resourcekey="btnReferCardCall" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="ASK" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnComplianceCall" runat="server" OnClick="ComplianceButton_Click" meta:resourcekey="btnComplianceCall" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="ASK" />
                <VP:VBButton UseSubmitBehavior="False" ID="btnCallCenter" runat="server" OnClick="CallCenterButton_Click" meta:resourcekey="btnCallCenter" onmouseover="javascript:window.status='';return true;" Visible="False" SkinID="ASK" />
                
                
            </td>
        </tr>
    </table>
</VP:VBPanel>


<%--<ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender_container" runat="server"
    BackgroundCssClass="modalBackground" CancelControlID="btnDone" Enabled="True"
    PopupControlID="container" TargetControlID="VBButton1" DynamicServicePath="">
</ajaxToolkit:ModalPopupExtender>


        <VP:VBButton UseSubmitBehavior="False" ID="VBButton1" runat="server" CausesValidation="false" Text="Done" />
<VP:VBPanel ID="container" CssClass="body_Broad_width" Style="margin: 0 auto" runat="server">

    <VP:VBPanel ID="VBPanel1" runat="server" SkinID="Flip">
        <VP:VBButton UseSubmitBehavior="False" ID="btnDone" runat="server" CausesValidation="false" Text="Done" />
    </VP:VBPanel>

</VP:VBPanel>--%>


<input type="button" id="btnDummyPopupOpener" runat="server" style="display: none" />
<ajaxToolkit:ModalPopupExtender ID="mpeExecute" runat="server"
    BackgroundCssClass="modalBackground" CancelControlID="btnCancelExecute" Enabled="True"
    PopupControlID="pnlConfirmExecution" TargetControlID="btnDummyPopupOpener" DynamicServicePath="">
    <Animations>
    </Animations>
</ajaxToolkit:ModalPopupExtender>


<VP:VBHiddenField runat="server" ID="hidCaseOpenReasons" />
<VP:VBHiddenField runat="server" ID="hidCaseOpenMsgHeading" />
<asp:Panel ID="pnlConfirmExecution" runat="server" CssClass="modalPopup CaseOpen" meta:resourcekey="pnlConfirmExecution" Visible="True" Style="display: none">

    <div runat="server" id="Div1" class="AlertModalHeader">
        Warning
    </div>
    <div style="padding-left: 5px; padding-top: 4px;">
        <VP:WarningDisplayControl runat="server" ID="ctlCaseOpenMessage"></VP:WarningDisplayControl>
    </div>
    <br />
    <br />
    <VP:VBGridView runat="server" AutoGenerateColumns="False" ID="grdReasons">
        <Columns>
            <VP:VBBoundField HeaderText="#">
                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                <HeaderStyle CssClass="GridViewHeaderCenter"></HeaderStyle>

            </VP:VBBoundField>

            <VP:VBBoundField HeaderText="Reason" DataField="Reason">
                <ItemStyle HorizontalAlign="Left"></ItemStyle>
            </VP:VBBoundField>
        </Columns>
    </VP:VBGridView>

    <VP:VBPanel runat="server" SkinID="FlipCaseOpenButtons">
        <VP:VBButton UseSubmitBehavior="False" ID="btnCancelExecute" runat="server" meta:resourcekey="btnCancelExecute" Text="Cancel" CausesValidation="False" />
        <VP:VBButton UseSubmitBehavior="False" ID="btnConfirmNextNavigation" runat="server" OnClick="NextButton_Click" OnClientClick="onConfirmExecute();" meta:resourcekey="btnConfirmNextNavigation" SkinID="Highlight" />
    </VP:VBPanel>
</asp:Panel>
