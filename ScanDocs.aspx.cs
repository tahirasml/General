using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using VeriBranch.Framework.Definitions;
using VeriBranch.WebApplication.Common;
using VeriBranch.WebApplication.Constants;
using VeriBranch.WebApplication.UIProcess;

public partial class Scanner_ScanDocs : VeriBranchTransactionCommonBasePage, IScanPage
{
    protected Dictionary<string, VPDocumentType> accessKeysDic = null;

    protected override void LocalizePageContent()
    {
        base.LocalizePageContent();
        //SetHeaderLabel("Scan Documents");
        //ctlNavigationButton.BackUrl = SafePageController.GetStateValue(VpPageControllerConstants.PageController.ScanBackURL) as string;
        //ctlNavigationButton.NextUrl = SafePageController.GetStateValue(VpPageControllerConstants.PageController.ScanNextURL) as string;

        //if(!ShowChecklist)
        //    ctlNavigationButton.NextUrl = SafePageController.GetStateValue(VpPageControllerConstants.PageController.ChecklistNextURL) as string;

        //var pageHeading = SafePageController.GetStateValue(VpPageControllerConstants.PageController.TransactionPageHeading) as string;
        //if (!string.IsNullOrEmpty(pageHeading))
        //    SetHeaderLabel(pageHeading);
    }

    protected override void GetStateFromUI()
    {
        RegisterScripts();
    }

    protected override void SetUIFromState()
    {
        RegisterScripts();
    }

    protected override void DoPageAction()
    {

        SetDefaultTransactionInfo();
        LoadDocumentTypes();
        LoadDocumentPackages();
    }



    private void LoadDocumentTypes()
    {
        try
        {
            var response = ServicesHelper.DoDocumentTypeInquiry();
            if (response.IsSuccess && response.DocumentType != null && response.DocumentType.Length > 0)
            {
                SelContentType.Items.Clear();
                foreach (var documentType in response.DocumentType)
                {
                    SelContentType.Items.Add(new ListItem(documentType.Name, documentType.Code.Replace(" ", "")));
                }
            }
            else
            {
                SelContentType.Items.Add(new ListItem("Applicant’s Credit Report", "ACR"));
                SelContentType.Items.Add(new ListItem("Application Form", "APPF"));

                SelContentType.Items.Add(new ListItem("Application", "APPF"));
                SelContentType.Items.Add(new ListItem("Bill Copy", "COB"));
                SelContentType.Items.Add(new ListItem("Cheque Book Request Form", "CBRF"));
                SelContentType.Items.Add(new ListItem("Driving License", "DL"));
                SelContentType.Items.Add(new ListItem("IMP Form", "IMP"));
                SelContentType.Items.Add(new ListItem("Import Registration Certificate (IRC)", "IRC"));
                SelContentType.Items.Add(new ListItem("Iqama", "Iqama"));
                SelContentType.Items.Add(new ListItem("L/C Application", "LCA"));
                SelContentType.Items.Add(new ListItem("National Id", "NationalId"));
                SelContentType.Items.Add(new ListItem("National ID Card", "NID"));
                SelContentType.Items.Add(new ListItem("Passport", "PP"));
                SelContentType.Items.Add(new ListItem("Supplier’s Credit Report", "SCR"));
                SelContentType.Items.Add(new ListItem("Trade License", "TL"));
                SelContentType.Items.Add(new ListItem("Transaction Advice", "ADV"));
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    private void LoadDocumentPackages()
    {
        try
        {
            var documentPackages = SafePageController.GetStateValue(VpPageControllerConstants.Transaction.DocumentPackage) as List<VPDocumentPackage>;
            if (documentPackages != null && documentPackages.Count > 0)
            {
                DocumentContent.Items.Clear();
                foreach (var package in documentPackages)
                {
                    foreach (var content in package.DocumentPackageContents)
                    {
                        if (content != null && content.DocumentCategory != null && content.DocumentCategory.DocumentType != null)
                        {
                            if (content.DocumentCategory.DocumentType.Length == 0)
                                DocumentContent.Items.Add(new ListItem(content.DocumentCategory.Name, content.DocumentCategory.Code));
                            else
                            {
                                foreach (var docType in content.DocumentCategory.DocumentType)
                                {
                                    DocumentContent.Items.Add(new ListItem(docType.Name, docType.Code));
                                }

                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }

    protected void btnDone_Click(object sender, EventArgs e)
    {
        var page = Page as VeriBranchCommonBasePage;
        if (page == null)
            return;

        ctlDocumentScanning.DoSaveScanDocs();
    }

    private void RegisterScripts()
    {
        try
        {
            this.hidSuccessMessage.Value = GetLocalResource("resSuccessMessage.Text");
            this.hidCult.Value = System.Globalization.CultureInfo.CurrentCulture.ToString();
            this.hidDocNameError.Value = GetLocalResource("resDocNameEmptyError.Text");
            this.hidRelDocSelError.Value = GetLocalResource("resReloadDocError.Text");
            this.txnRefNo.Value = (string)SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber);

            string scanningProfile = Request.QueryString["scanprof"];

            var script = new StringBuilder();
            if (!string.IsNullOrEmpty(scanningProfile))
            {
                script = new StringBuilder();
                script.Append("<Script> function onLoadComplete() {");
                script.Append(";document.getElementById('ShowUI').checked = " + (scanningProfile[0].Equals('0') ? "false" : "true"));
                script.Append(";document.getElementById('ADF').checked = " + (scanningProfile[1].Equals('0') ? "false" : "true"));
                script.Append(";document.getElementById('Duplex').checked = " + (scanningProfile[2].Equals('0') ? "false" : "true"));
                script.Append(";document.getElementById('DiscardBlank').checked = " + (scanningProfile[3].Equals('0') ? "false" : "true"));

                script.Append(";document.getElementById('BW').checked = " + (scanningProfile[4].Equals('0') ? "false" : "true"));
                script.Append(";document.getElementById('Gray').checked = " + (scanningProfile[5].Equals('0') ? "false" : "true"));
                script.Append(";document.getElementById('RGB').checked = " + (scanningProfile[6].Equals('0') ? "false" : "true"));

                script.Append(";document.getElementById('Resolution').value = \"" + (scanningProfile.Substring(7, 4).TrimStart('0')) + "\";");

                script.Append(" } </Script>");
                litOnLoadCompleteJS.Text = script.ToString();
            }
            else
            {

                Uri uri = HttpContext.Current.Request.Url;
                String host = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;

                script = new StringBuilder();
                script.Append("<Script> function onLoadComplete() {");
                script.Append(";document.getElementById('ShowUI').checked = false");
                script.Append(";document.getElementById('ADF').checked = true");
                script.Append(";document.getElementById('Duplex').checked = false");
                script.Append(";document.getElementById('DiscardBlank').checked =false");

                script.Append(";document.getElementById('BW').checked = false");
                script.Append(";document.getElementById('Gray').checked =true");
                script.Append(";document.getElementById('RGB').checked = false");
                script.Append(";document.getElementById('MultiPageTIFF').checked = true");
                script.Append(";spurl = '" + host + "'");
                script.Append(";ContentType_onChange(); var previewMode = document.getElementById('PreviewMode'); if(previewMode) previewMode.selectedIndex = 0;");

                script.Append(";document.getElementById('Resolution').value = \"" + "300" + "\"; slPreviewMode();");

                script.Append(" } </Script>");
                litOnLoadCompleteJS.Text = script.ToString();

                //FillContentTypeDropDown();
                //FillDocumentNamesList();
            }

            //getReasons
            var scripts = "function getReasons() { var uploadDocumentReasons = " + ctlDocumentScanning.ClientID + "_getCaseOpenReasons();  return uploadDocumentReasons;}";
            //setDocValue
            scripts += " function setDocValue (docCode) {    document.getElementById('" + SelContentType.ClientID + "').value = docCode;    ContentType_onChange();       var btnbtnOpenScan = document.getElementById('" + btnOpenScan.ClientID + "');   if (btnbtnOpenScan)   btnbtnOpenScan.click();     };";
            //SendScannedDocumentsToCRM
            scripts += " function SendScannedDocumentsToCRM(strDocType, strDocTypeName) {  var loginToken = '" + LoginToken + "';    var transactionReferenceNumber = '" + SafePageController.GetStateValue(VpPageControllerConstants.Transaction.TransactionReferenceNumber) + "'; PageMethods.SendScannedDocumentsToCRM(transactionReferenceNumber, strDocType, strDocTypeName, loginToken, true," + ClientID + "_OnSendScannedDocumentsToCRMComplete);   }";

            //
            scripts += " function " + ClientID + "_OnSendScannedDocumentsToCRMComplete(res) {var resArray = res.split('|');var uploadResult = resArray[0]; var strDocType = resArray[1]; var strDocTypeName = resArray[2]; hideOverlay(); showWhiteBox(); document.getElementById('DWTcontainer').disabled = \"\"; if (uploadResult != 'OK') {   alert(uploadResult);    } else {   onUploadComplete(strDocType, strDocTypeName);       }   }";

            Page.ClientScript.RegisterStartupScript(this.GetType(), "Scripts", scripts, true);

            //hide maindivIE when click back or next button
            //ctlNavigationButton.BackButton.OnClientClick = "javascript: HideWhiteBox();";
            //ctlNavigationButton.NextButton.OnClientClick = "javascript: Page_ClientValidate() && HideWhiteBox();";
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }
}
