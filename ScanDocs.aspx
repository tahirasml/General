<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Popup.master" AutoEventWireup="true" CodeFile="ScanDocs.aspx.cs" Inherits="Scanner_ScanDocs" %>
<%@ Import Namespace="VeriBranch.WebApplication.Constants" %>

<%@ Register Src="~/Controls/DocumentScanning.ascx" TagName="DocumentScanning" TagPrefix="VP" %>

<asp:Content ID="Content1" ContentPlaceHolderID="transactionContent" runat="Server">

    <script type="text/javascript" language="javascript">
        function HideWhiteBox() {
            var maindivIE = document.getElementById('maindivIE');
            if (maindivIE)
                maindivIE.style.visibility = 'hidden';
        }

        function showWhiteBox() {
            var maindivIE = document.getElementById('maindivIE');
            if (maindivIE)
                maindivIE.style.visibility = 'visible';
        }

        $(document).ready(function () {
            ContentType_onChange();
        });

    </script>

    <VP:DocumentScanning runat="server" ID="ctlDocumentScanning" Category="ScannedDocuments" />
    <br />
    <link href="styles/style.css" type="text/css" rel="stylesheet" />
    <link href="styles/toolbarStyles.css" type="text/css" rel="stylesheet" />


    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender_container" runat="server"
        BackgroundCssClass="modalBackground" CancelControlID="btnClose" Enabled="True"
        PopupControlID="container" TargetControlID="btnOpenScan" DynamicServicePath="">
    </ajaxToolkit:ModalPopupExtender>

    <VP:VBPanel runat="server" SkinID="Flip">
        <VP:VBButton ID="btnOpenScan" AccessKey="P" runat="server" CausesValidation="false" Text="Scan" SkinID="Highlight" meta:resourcekey="btnOpenScan" />
        <VP:VBButton ID="btnDone" AccessKey="D" runat="server" CausesValidation="true" Text="Done" SkinID="Highlight" OnClick="btnDone_Click" meta:resourcekey="btnDone"/>
    </VP:VBPanel>

    <VP:VBPanel id="container" CssClass="body_Broad_width" style="margin: 0 auto" runat="server">
        <div id="DWTcontainer" class="body_Broad_width">
                        
            <asp:Label runat="server" ID="ScanDocsTitle" Text="SCAN DOCUMENT" CssClass="ScanDocsTitle" ></asp:Label>
            <table cellpadding="0" cellspacing="0" border="0" width="100%" class="ScanWrapperTable">
                <tr>
                    <td width="90%">
<div id="dwtcontrolContainer">
                <div id="dwtcontrol">
                    <div class="toolbar" style="width: 99.8%">
                        <table border="0" cellspacing="0" cellpadding="0">
                            <tr>
                                <td class="trfirst">
                                    <a class="image_editor16" id="btnEditor" onclick="return btnShowImageEditor_onclick()" href="#">
                                        <asp:Localize runat="server" ID="resImageEditor" Text="Image Editor" meta:resourcekey="resImageEditor" /></a>
                                </td>
                                <td>
                                    <a class="rotate_right" id="btnRotateR" onclick="return btnRotateRight_onclick()" href="#">
                                        <asp:Localize runat="server" ID="resRotateRight" Text="Rotate Right" meta:resourcekey="resRotateRight" /></a>
                                </td>
                                <td>
                                    <a class="rotate_left" onclick="return btnRotateLeft_onclick()" href="#">
                                        <asp:Localize runat="server" ID="resRotateLeft" Text="Rotate Left" meta:resourcekey="resRotateLeft" />
                                    </a>
                                </td>
                                <td>
                                    <a class="flip" id="btnFlip" id="btnRotateL" onclick="return btnFlip_onclick()" href="#">
                                        <asp:Localize runat="server" ID="residFlip" Text="Flip" meta:resourcekey="residFlip" /></a>
                                </td>
                                <td>
                                    <a class="mirror" id="btnMirror" onclick="return btnMirror_onclick()" href="#">
                                        <asp:Localize runat="server" ID="resMirror" Text="Mirror" meta:resourcekey="resMirror" />
                                    </a>
                                </td>
                                <td>
                                    <a class="crop" id="btnCrop" onclick="btnCrop_onclick();" href="#">
                                        <asp:Localize runat="server" ID="resCrop" Text="Crop" meta:resourcekey="resCrop" /></a>
                                </td>

                                <td>
                                    <%--  <a class="resize" onclick="return btnChangeImageSize_onclick();" id="btnChangeImageSize">
                                        Resize Image</a>--%>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="ImgSizeEditor" style="visibility: hidden; text-align: left;">
                        <ul>
                            <li>
                                <label for="img_height">
                                    <b>
                                        <asp:Localize runat="server" ID="resNewHeight" Text="New Height :" meta:resourcekey="resNewHeight" /></b>
                                    <input type="text" id="img_height" style="width: 50%;" size="10" /><asp:Localize runat="server" ID="respixel" Text="pixel" meta:resourcekey="respixel" /></label></li>
                            <li>
                                <label for="img_width">
                                    <b>New Width :</b>&nbsp;
                                    <input type="text" id="img_width" style="width: 50%;" size="10" /><asp:Localize runat="server" ID="resPixel2" Text="pixel" meta:resourcekey="respixel" /></label></li>
                            <li>
                                <asp:Localize runat="server" ID="resInterpolationMethod" Text="Interpolation method:" meta:resourcekey="resInterpolationMethod" />
                                <select size="1" id="InterpolationMethod">
                                    <option value=""></option>
                                </select></li>
                            <li style="text-align: center;">
                                <input type="button" value="   OK   " id="btnChangeImageSizeOK" onclick="return btnChangeImageSizeOK_onclick();" />
                                <input type="button" value=" Cancel " id="btnCancelChange" onclick="return btnCancelChange_onclick();" /></li>
                        </ul>
                    </div>
                    <div id="maindivPlugin">
                        <div style="display: none;" id="mainControlNotInstalled">
                            <table id="maintblcontrolnotinstalled" class="divcontrol">
                                <tr>
                                    <td style="text-align: center; vertical-align: middle;">
                                        <a id="hrDWTEXE" runat="server"><strong>
                                            <asp:Localize runat="server" ID="resDownLoadPluginText" Text="Download and install the Plug-in Here" meta:resourcekey="resDownLoadPluginText" /></strong></a><br />
                                        <asp:Localize runat="server" ID="resRestartBrowserMessage" Text="After the installation, please restart your browser." meta:resourcekey="resRestartBrowserMessage" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div id="mainControlInstalled">
                        </div>
                    </div>
                    <div id="maindivIE">
                        <%--                        <object classid="clsid:5220cb21-c88d-11cf-b347-00aa00a28331" style="display: none;" height="300px">
                            <param name="LPKPath" value="/DynamicWebTwain.lpk" />
                        </object>--%>
                        <div id="maindivIEx86">
                        </div>
                        <div id="maindivIEx64">
                        </div>
                    </div>
                </div>
                <div id="extraInfo" style="font-size: 11px; color: #222222; font-family: verdana sans-serif; background-color: #f0f0f0; text-align: left; width: 580px;">
                </div>
                <div class="toolbar toolbar3" style="width:99.8%">
                    <table border="0" cellspacing="0" cellpadding="0" class="ControlToolbar">
                        <tr>
                            <td width="45">
                                <a id="btnFirstImage" onclick="return btnFirstImage_onclick()">
                                    <img src="~/Content/EN/CRM2011/Images/Scanner/resultset_first.png" width="16" height="16" />
                                </a><a id="btnPreImage" onclick="return btnPreImage_onclick()">
                                    <img src="~/Content/EN/CRM2011/Images/Scanner/resultset_previous.png" width="16" height="16" />
                                </a>
                            </td>
                            <td width="70" align="center">
                                <input type="text" size="2" id="CurrentImage" readonly="readonly" style="text-align: center; width: 20px" />
                                -
                                <input type="text" size="2" id="TotalImage" readonly="readonly" style="text-align: center; width: 20px" />
                            </td>
                            <td width="50">
                                <a id="btnNextImage" onclick="return btnNextImage_onclick()">
                                    <img src="~/Content/EN/CRM2011/Images/Scanner/resultset_next.png" width="16" height="16" /></a><a id="btnLastImage" onclick="return btnLastImage_onclick()"><img src="~/Content/EN/CRM2011/Images/Scanner/resultset_last.png" width="16" height="16" /></a>
                            </td>
                            <td>
                                <asp:Localize runat="server" ID="resPreviewMode" Text="Preview Mode" meta:resourcekey="resPreviewMode" />
                                <select size="1" id="PreviewMode" onchange="slPreviewMode();" style="width: 50px">
                                    <option value="0">1X1</option>
                                </select>
                            </td>
                        </tr>
                    </table>
                    <div class="buttonContainer">
                        <%--   <a class="lnkButton" id="btnloadimages" onclick="return LoadScannedImagesFromSP()">
                            <span class="save"></span>Reload</a> --%>
                        <a class="lnkButton" id="btnRemoveCurrentImage" onclick="return btnRemoveCurrentImage_onclick()" href="#"><span class="remove_slcted"></span>
                            <asp:Localize runat="server" ID="resRemoveSelected" Text="Remove Selected" meta:resourcekey="resRemoveSelected" /></a> <a class="lnkButton" id="btnRemoveAllImages" onclick="return btnRemoveAllImages_onclick()" href="#"><span class="remove_all"></span>
                                <asp:Localize runat="server" ID="resRemoveAll" Text="Remove All" meta:resourcekey="resRemoveAll" />
                            </a>
                    </div>
                </div>
                <div class="divinput" style="text-align: center; width: 580px; background-color: #FFFFFF; display: none">
                    <input id="btnFirstImage1" onclick="return btnFirstImage_onclick()" type="button" value=" |&lt; " />&nbsp;
                    <input id="btnPreImage1" onclick="return btnPreImage_onclick()" type="button" value=" &lt; " />&nbsp;&nbsp;
                    <input type="text" size="2" id="CurrentImage1" readonly="readonly" />/
                    <input type="text" size="2" id="TotalImage1" readonly="readonly" />&nbsp;&nbsp;
                    <input id="btnNextImage1" onclick="return btnNextImage_onclick()" type="button" value=" &gt; " />&nbsp;
                    <input id="btnLastImage1" onclick="return btnLastImage_onclick()" type="button" value=" &gt;| " />
                    <asp:Localize runat="server" ID="resPreviewMode1" Text="Preview Mode" meta:resourcekey="resPreviewMode" />
                    <select size="1" id="PreviewMode1" onchange="slPreviewMode();">
                        <option value="0">1X1</option>
                    </select><br />
                    <input id="btnRemoveCurrentImage1" onclick="return btnRemoveCurrentImage_onclick()" type="button" value="Remove Selected Images" />
                    <input id="btnRemoveAllImages1" onclick="return btnRemoveAllImages_onclick()" type="button" value="Remove All Images" /><br />
                    <div id="divMsg" style="display: none; text-align: left;">
                        Message:<br />
                        <div id="emessage" style="width: 550px; height: 80px; overflow: scroll; background-color: #ffffff; border: 1px #303030; border-style: solid; text-align: left;">
                        </div>
                    </div>
                </div>


            </div>
                    </td>
                    <td width="10%">
<div id="ScanWrapper">

                <!-- scan section -->
                <div id="divScanner" class="divinput">
                    <ul>
                        <li><%--<a href="javascript:ExpandSection('divscansec');" style="text-decoration: none;">
                            <img alt="arrow" src="~/Content/EN/CRM2011/Images/Scanner/arrow.gif" style="width: 12px; height: 12px;" id="arrowscansec" /></a>--%>
                            <b><asp:Localize runat="server" ID="resScanSectionHeading" Text="Scan" meta:resourcekey="resScanSectionHeading" /></b></li>
                    </ul>
                    <div id="divscansec">
                        <ul>
                            <li>
                                <label for="source">
                                    <asp:Localize runat="server" ID="resSelectSource" Text="Select Source" meta:resourcekey="resSelectSource" />
                                    <select size="1" id="source">
                                        <option value=""></option>
                                    </select></label></li>
                            <%--<li style="display: none;" id="pNoScanner"><a href="javascript: void(0)" class="ShowtblLoadImage"
                            style="font-size: 11px;" id="aNoScanner"><b>What if you don't have a scanner connected:</b>
                        </a></li>--%>
                            <li>
                                <label for="ShowUI">
                                    <input type="checkbox" id="ShowUI" />
                                    <asp:Localize runat="server" ID="resShowUI" Text="Show UI" meta:resourcekey="resShowUI" />&nbsp;</label>
                                <label for="ADF">
                                    <input type="checkbox" id="ADF" /><asp:Localize runat="server" ID="resADF" Text="ADF" meta:resourcekey="resADF" />&nbsp;</label>
                                <label for="Duplex">
                                    <input type="checkbox" id="Duplex" /><asp:Localize runat="server" ID="resDuplex" Text="Duplex" meta:resourcekey="resDuplex" /></label>
                                <label for="DiscardBlank">
                                    <input type="checkbox" id="DiscardBlank" /><asp:Localize runat="server" ID="resDiscardBlankImgs" Text="If Discard Blank Images" meta:resourcekey="resDiscardBlankImgs" /></label>
                            </li>
                            <li>
                                <table cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td style="width: 325px">
                                            <asp:Localize runat="server" ID="resPixelType" Text="Pixel Type:" meta:resourcekey="resPixelType" />

                                            <label for="BW">
                                                <input type="radio" id="BW" name="PixelType" /><asp:Localize runat="server" ID="resBlackandWhite" Text="B&W" meta:resourcekey="resBlackandWhite" />
                                            </label>
                                            <label for="Gray">
                                                <input type="radio" id="Gray" name="PixelType" /><asp:Localize runat="server" ID="resGray" Text="Gray" meta:resourcekey="resGray" /></label>
                                            <label for="RGB">
                                                <input type="radio" id="RGB" name="PixelType" /><asp:Localize runat="server" ID="resColor" Text="Color" meta:resourcekey="resColor" /></label>
                                        </td>
                                        <td>
                                            <a href="#" class="lnkButton" id="btnScan" onclick="btnScan_onclick();"><span class="scan"></span>
                                                <asp:Localize runat="server" ID="resBtnScan" Text="Scan" meta:resourcekey="resBtnScan" /></a>
                                        </td>

                                    </tr>
                                </table>
                            </li>
                            <li style="display: none">
                                <label for="Resolution">
                                    <asp:Localize runat="server" ID="resResolution" Text="Resolution" meta:resourcekey="resResolution" />
                                    &nbsp;<select size="1" id="Resolution"><option value=""></option>
                                    </select></label>

                            </li>

                        </ul>
                    </div>
                </div>
                
                <!-- unused -->
                <div id="tblLoadImage" style="display:none">
                    <ul>
                        <li><b>You can:</b><a href="javascript: void(0)" style="text-decoration: none; padding-left: 200px" class="ClosetblLoadImage">X</a></li>
                    </ul>
                    <div id="notformac1" style="background-color: #f0f0f0; padding: 5px;">
                        <ul>
                            <li>
                                <img alt="arrow" src="~/Content/EN/CRM2011/Images/Scanner/arrow.gif" width="9" height="12" /><b>Install a Virtual Scanner:</b></li>
                            <li style="text-align: center;"><a id="samplesource32bit" href="/DynamicWebTWAIN/twainds.win32.installer.2.1.3.msi">32-bit Sample Source</a> <a id="samplesource64bit" style="display: none;" href="/DynamicWebTWAIN/twainds.win64.installer.2.1.3.msi">64-bit Sample Source</a></li>
                        </ul>
                    </div>
                    <ul id="notformac2">
                        <li><b>Or you can:</b></li>
                    </ul>
                    <div style="background-color: #f0f0f0; padding: 5px;">
                        <ul>
                            <li>
                                <img alt="arrow" src="~/Content/EN/CRM2011/Images/Scanner/arrow.gif" width="9" height="12" /><b>Load a Sample Image:</b></li>
                            <li style="text-align: center">
                                <input id="btnLoad" class="bigbutton" type="button" style="width: 180px;" value="Load Image" onclick="return btnLoad_onclick()" /></li>
                        </ul>
                    </div>
                </div>

                <!-- unused -->
                <div id="divSave" class="divinput" style="display: none">
                    <ul>
                        <li>
                            <img alt="arrow" src="~/Content/EN/CRM2011/Images/Scanner/arrow.gif" width="9" height="12" /><b>Save Image</b></li>
                        <li style="padding-left: 15px;">
                            <label for="txt_fileNameforSave">File Name:<input type="text" size="20" id="txt_fileNameforSave" /></label></li>
                        <li style="padding-left: 12px;">
                            <label for="imgTypebmp">
                                <input type="radio" value="bmp" name="imgType_save" id="imgTypebmp" onclick="rdsave_onclick();" />BMP</label>
                            <label for="imgTypejpeg">
                                <input type="radio" value="jpg" name="imgType_save" id="imgTypejpeg" onclick="rdsave_onclick();" />JPEG</label>
                            <label for="imgTypetiff">
                                <input type="radio" value="tif" name="imgType_save" id="imgTypetiff" onclick="rdTIFFsave_onclick();" />TIFF</label>
                            <label for="imgTypepng">
                                <input type="radio" value="png" name="imgType_save" id="imgTypepng" onclick="rdsave_onclick();" />PNG</label>
                            <label for="imgTypepdf">
                                <input type="radio" value="pdf" name="imgType_save" id="imgTypepdf" onclick="rdPDFsave_onclick();" />PDF</label></li>
                        <li style="padding-left: 12px;">
                            <label for="MultiPageTIFF_save">
                                <input type="checkbox" id="MultiPageTIFF_save" />Multi-Page TIFF</label>
                            <label for="MultiPagePDF_save">
                                <input type="checkbox" id="MultiPagePDF_save" />Multi-Page PDF
                            </label>
                        </li>
                    </ul>
                    <table>
                        <tr>
                            <td style="width: 315px" />
                            <td>
                                <input id="btnSave" type="button" value="Save Image" onclick="return btnSave_onclick()" /></li>
                            </td>
                        </tr>
                    </table>
                </div>

                <!-- upload section -->
                <div id="divUpload" class="divinput">
                    <ul>
                        <li><%--<a href="javascript:ExpandSection('divsavesec');" style="text-decoration: none; vertical-align: top">
                            <img alt="arrow" src="../Content/EN/CRM2011/Images/Scanner/arrow.gif" width="12px" height="12" id="arrowsavesec" /></a>--%>
                            <b>
                                <asp:Localize runat="server" ID="resUpload" Text="Upload" meta:resourcekey="resUpload" /></b></li>
                    </ul>
                    <div id="divsavesec">
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="padding-left: 0px;">
                                    <label for="txt_fileName" style="display: none">
                                        File Name:
                                        <input type="text" size="20" id="txt_fileName" /></label>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding-left: 0px;">
                                    <!--<label for="imgTypebmp2">
                                        <input type="radio" value="bmp" name="ImageType" id="imgTypebmp2" onclick ="rd_onclick();"/>BMP</label>-->
                                    <label for="imgTypejpeg2" style="display: none">
                                        <input type="radio" value="jpg" name="ImageType" id="imgTypejpeg2" onclick="rd_onclick();" />JPEG</label>
                                    <label for="imgTypetiff2">
                                        <input type="radio" value="tif" name="ImageType" id="imgTypetiff2" onclick="rdTIFF_onclick();" /><asp:Localize runat="server" ID="resTIFF" Text="TIFF" meta:resourcekey="resTIFF" /></label>
                                    <label for="MultiPageTIFF">
                                        <input type="checkbox" id="MultiPageTIFF" /><asp:Localize runat="server" ID="resMultiPageTiff" Text="Multi-Page TIFF" meta:resourcekey="resMultiPageTiff" /></label>
                                    <label for="imgTypepng2" style="display: none">
                                        <input type="radio" value="png" name="ImageType" id="imgTypepng2" onclick="rd_onclick();" />PNG</label>
                                    <label for="imgTypepdf2" style="display: none">
                                        <input type="radio" value="pdf" name="ImageType" id="imgTypepdf2" onclick="rdPDF_onclick();" />PDF</label>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding-left: 0px; display: none">
                                    <label for="MultiPagePDF" style="display: none">
                                        <input type="checkbox" id="MultiPagePDF" />Multi-Page PDF
                                    </label>
                                </td>
                            </tr>
                            <tr>
                                <td style="height: 5px; display: none" colspan="2"></td>
                            </tr>
                            <tr>
                                <td colspan="2" style="padding-left: 0px; text-align: left">
                                    <table cellpadding="0" cellspacing="0">
                                        <tr>
                                            <td align="left" style="padding-top: 4px; padding-bottom: 4px;display:none">
                                                <asp:Localize runat="server" ID="resDocumentName" Text="Document Name" meta:resourcekey="resDocumentName" />
                                            </td>
                                            <td align="left" style="padding-top: 4px; padding-bottom: 4px;padding-left: 4px">
                                                <asp:Localize runat="server" ID="resDocumentType" Text="Document Type" meta:resourcekey="resDocumentType" />
                                            </td>
                                        </tr>
                                        <tr style="vertical-align: top;">
                                            <td style="padding-right: 2px;display:none">
                                                <input type="text" id="txtDocname" style="font-family: 'Segoe UI',Arial,Sans-Serif; font-size: 14px; height: 16px;width: 191px;" />
                                            </td>

                                            <td style="padding-left: 4px; padding-bottom:4px;">
                                                <select id="SelContentType" runat="server" onchange="ContentType_onChange();" name="SelContentType" style="width: 191px; height: 20px; font-size: 14px">
                                                </select>
                                            </td>
                                           <%-- Dummy control--%>
                                             <td style="display:none">
                                                <select id="DocumentContent" runat="server"  style="display:none">
                                                </select>
                                            </td>

                                            
                                        </tr>
                                        <tr>
                                            <td style="height: 5px"></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                        <table cellpadding="0" cellspacing="0"style="width: 400px;">
                            <tr>
                                <td style="width: 325px; height: 30px;" />
                                <td>
                                    <a href="#" class="lnkButton" id="btnUpload" onclick="return btnUpload_onclick()"><span class="save"></span>
                                        <asp:Localize runat="server" ID="resBtnSave" Text="Save" meta:resourcekey="resBtnSave" /></a>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>

                <!-- reload section -->
                <div class="divinput" style="display:none">
                    <ul>
                        <li><%-- <a href="javascript:ExpandSection('divreloadsec');" style="text-decoration: none;">
                            <img alt="arrow" src="../Content/EN/CRM2011/Images/Scanner/arrow.gif" width="12px" height="12" id="arrowreloadsec" /></a>--%>
                            <b>
                                <asp:Localize runat="server" ID="resReloadSectionHeading" Text="Reload" meta:resourcekey="resReloadSectionHeading" />

                            </b></li>
                    </ul>
                    <div id="divreloadsec">
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="text-align: left; padding-bottom: 4px">
                                    <asp:Localize runat="server" ID="resDocumentNamerel" Text="Document Name" meta:resourcekey="resDocumentName" />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: left;padding-bottom:4px;">
                                    <asp:ListBox ID="lstDocNames" runat="server" Width="393px" Height="80px" />
                                </td>
                            </tr>
                            <tr>
                                <td style="height: 30px; text-align: right; padding:5px 0 0 0;">
                                    <a href="#" class="lnkButton" id="btnReload" onclick="return LoadScannedImagesFromSP()"><span class="reload"></span>
                                        <asp:Localize runat="server" ID="resBtnReload" Text="Reload" meta:resourcekey="resBtnReload" /></a>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>

              </div>
                    </td>

                </tr>
            </table>
            
            <table cellpadding="0" cellspacing="0" style="width: 420px; padding-top: 10px; float:right; margin-right:7px;">
                <tr>
                    <td style="text-align: right">
                        <VP:VBButton ID="btnClose" Accesskey="C" runat="server" CausesValidation="false" Text="Close" meta:resourcekey="btnClose" />
                    </td>
                </tr>
            </table>            
        </div>
        <input id="hidSuccessMessage" name="hidSuccessMessage" type="hidden" runat="server" />
        <input id="hidCult" name="hidCult" type="hidden" runat="server" />
        <input id="hidDocNameError" name="hidCult" type="hidden" runat="server" />
        <input id="hidRelDocSelError" name="hidCult" type="hidden" runat="server" />
        <input id="txnRefNo" name="txnRefNo" type="hidden" runat="server" />
        <script type="text/javascript" language="javascript" src="~/Content/EN/js/online_demo_scan.js"></script>
        <asp:Literal runat="server" ID="litOnLoadCompleteJS"></asp:Literal>
    </VP:VBPanel>

</asp:Content>
