using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   
using System.Data.SqlClient;
using System.IO;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint;
using System.Configuration;
using System.Threading;
using System.Net;
using log4net;
using SAIB.OnBase.CRM.Archival.OnBaseHelper;

namespace SAIB.OnBase.CRM.Archival.Core
{
    public class CRM : BaseClass
    {
       static Hyland.Unity.Application app = null;
        int totalProcessedFolders = 0, totalFailedFolders = 0, totalSuccessFolders = 0, totalSuccessFiles = 0, totalFailedFiles = 0, totalFoldersNoNeedToArchive = 0, TotalNumberofEmptyFolders = 0;
        private static readonly ILog log = LogManager.GetLogger(typeof(CRM));
        DateTime Start;
        public void Execute()
        {   

              // connect to sharepoint 
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            using (SPClient sp = new SPClient(SharePointSite, SharePointUserName, SharePointPassword, SharePointDomain))
            {

                log.Info("Connected To SharePoint = " + SharePointSite + " At " + DateTime.Now.ToString());
                Console.WriteLine("Connected To SharePoint = " + SharePointSite + " At " + DateTime.Now.ToString());
                SISL.SISLClient client = new SISL.SISLClient();
                Connection conn = new Connection();
                Start = DateTime.Now;
                try
                {    

                    // get meta data about the tool and folders from retention period in sharepoint 
                    #region Get The Configration List
                    ListItemCollection PeriodListColl = GetPeriodList(sp);
                    log.Info("Connect To Retention Period  " + " At =  " + DateTime.Now.ToString());
                    Console.WriteLine("Connect To Retention Period  " + " At =  " + DateTime.Now.ToString());
                    #endregion
                     // connect to onbase
                    InitateOnBaseApplication();
                    foreach (ListItem configListItems in PeriodListColl)
                    {
                        
                        string RowLimit = null;

                        try
                        {
                            #region Fill the Configration Parameter
                            string listName = configListItems.FieldValues["Title"].ToString();
                            List list = sp.GetListByTitle(listName);
                            if (configListItems.FieldValues["RowLimit"].ToString() == "0")
                                RowLimit = "100";
                            else
                                RowLimit = configListItems.FieldValues["RowLimit"].ToString();
                            #endregion
                            // Connect To OnBase
                            // InitateOnBaseApplication();
                            Hyland.Unity.DocumentType documentType = app.Core.DocumentTypes.Find(Convert.ToInt64(configListItems.FieldValues["OBDocTypeId"].ToString()));

                            if (list.BaseType.ToString() == "DocumentLibrary" && documentType != null) // check if the list exist in OnBase
                            {
                                #region Get List of folders from SP
                                ListItemCollection listOfFolders = null;
                                try
                                {
                                    string Period = configListItems.FieldValues["Period"].ToString();
                                    string ArchiveStartDate = Convert.ToDateTime(configListItems.FieldValues["ArchiveStartDate"].ToString()).ToString("yyyy-MM-ddTHH:mm:ssZ");
                                
                                    listOfFolders = GetFolders(sp, RowLimit, Period, ArchiveStartDate, list);

                                    #region Logs
                                    log.Info("Working on archiving List " + configListItems.FieldValues["Title"].ToString() + " At =  " + DateTime.Now.ToString());
                                    Console.WriteLine("Working on archiving List " + configListItems.FieldValues["Title"].ToString() + " At =  " + DateTime.Now.ToString());
                                    log.Info("RowLimit = " + RowLimit + " and the actual number of data retrieved from " + configListItems.FieldValues["Title"].ToString() + " is " + listOfFolders.Count);
                                    Console.WriteLine("RowLimit = " + RowLimit + " and the actual number of data retrieved from " + configListItems.FieldValues["Title"].ToString() + " is " + listOfFolders.Count);
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    ErrorFunction(ex, false);

                                    continue;
                                }
                                #endregion
                                int libCount = -1;

                                #region loop in the list of folders
                                foreach (ListItem rootItem in listOfFolders)
                                {
                                    libCount = libCount + 1;
                                    int archiveAction = 1; //defualt 1 -> Archive
                                    #region check if the type is Folder
                                    if (rootItem.FileSystemObjectType == FileSystemObjectType.Folder)
                                    {
                                        totalProcessedFolders += 1;
                                        Folder folder = null;
                                        folder = sp.GetFolderItems(rootItem["FileRef"].ToString());
                                        bool ErrorOnArchiveItem = false;
                                        #region Check if the folder contains doceument
                                        if (folder.ItemCount > 0)
                                        {
                                            #region Get the archiveAction from DB
                                            try
                                            {
                                                
                                                if (!string.IsNullOrEmpty(Convert.ToString(configListItems.FieldValues["StoredProcedureName"].ToString())))
                                                {
                                                    var spName = configListItems["StoredProcedureName"].ToString();
                                                    var appNoValue = GetApplicationNum(folder.Name, list.Title);
                                                    DBIntegration.DBHelper _CheckArchiveAction = new DBIntegration.DBHelper();
                                                    archiveAction = _CheckArchiveAction.CheckArchiveAction(spName, appNoValue, SQLConnString);

                                                }
                                            }
                                            catch (Exception)
                                            {

                                            }
                                            #endregion
                                            
                                            FileCollection FolderFilelist;
                                            #region Get Folder File List
                                            try
                                            {
                                                FolderFilelist = sp.getFolderFiles(folder);

                                            }
                                            catch (Exception ex)
                                            {
                                                log.Info("This error occurrd while fetching folders from SharePoint list name is  : " + list.Title);
                                                ErrorFunction(ex, false);
                                                continue;
                                            }
                                            int folderFilesCount = FolderFilelist.Count;
                                            #endregion 
                                            try
                                            {
                                                #region Fill OnBase KeyWords
                                                Dictionary<long, object> frmData = FillParameter(folder.Name, list.Title);
                                                if (frmData == null)
                                                {
                                                    log.Info("Folder Name: " + folder.Name + " has Error in Name");
                                                    Console.WriteLine("Folder Name: " + folder.Name + " has Error in Name");
                                                    continue;
                                                }
                                                #endregion
                                                #region IF Action is 1
                                                if (archiveAction == 1)
                                                {
                                                    DocumentHelper dochelper = new DocumentHelper();

                                                    long docHandle = 0;
                                                    #region folder files loob 
                                                    for (int fileCount = 0; fileCount < folderFilesCount; fileCount++)
                                                    {
                                                        bool ArchivedItemInOnBase = false;
                                                        #region check if the file not archived
                                                        if (!FolderFilelist[fileCount].Name.Contains(".aspx"))
                                                        {
                                                            string[] fileurltokens = FolderFilelist[fileCount].ServerRelativeUrl.Split('.');
                                                            fileurltokens[fileurltokens.Length - 1] = "aspx";
                                                            #region check if it's already archived but the original file not deleted
                                                            if (sp.GetFileByUrl(string.Join(".", fileurltokens)) == null)
                                                            {
                                                                if (FolderFilelist[fileCount].CheckOutType != CheckOutType.None)
                                                                {
                                                                    FolderFilelist[fileCount].CheckIn("Checked in By CRM Archiving Tool", CheckinType.OverwriteCheckIn);
                                                                }
                                                                FileInformation fin = sp.DownloadFileInformation(FolderFilelist[fileCount]);
                                                                #region Archive to OnBase
                                                                try
                                                                {
                                                                   // InitateOnBaseApplication();

                                                                    string[] nametokens = FolderFilelist[fileCount].Name.Split('.');

                                                                    string fileFormat = nametokens[nametokens.Length - 1];  //make sure the extension is the last token from split

                                                                    if (nametokens.Length==1) // in case no extension 
                                                                    {
                                                                        fileFormat = "pdf";
                                                                    }

                                                                    Hyland.Unity.PageData pageData = app.Core.Storage.CreatePageData(fin.Stream, fileFormat);

                                                                    object entry;
                                                                    string SPfileName = FolderFilelist[fileCount].Name;
                                                                    if (frmData.TryGetValue(Convert.ToInt64(ConfigurationManager.AppSettings["SPFileName"]), out entry))
                                                                    {
                                                                        if (entry != null)
                                                                            frmData.Remove(Convert.ToInt64(ConfigurationManager.AppSettings["SPFileName"]));
                                                                    }

                                                                    if (SPfileName.Length > 250)
                                                                        SPfileName = FolderFilelist[fileCount].Name.Substring(0, 249);
                                                                    frmData.Add(Convert.ToInt64(ConfigurationManager.AppSettings["SPFileName"]), SPfileName);
                                                    
                                                                    docHandle = dochelper.CreateNewDocment(app, documentType.Name, fileFormat, frmData, pageData);
                                                                    #region Logs
                                                                    log.Info("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " OnBase->CreateNewDocment=" + docHandle);
                                                                    Console.WriteLine("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " OnBase->CreateNewDocment = " + docHandle);
                                                                    #endregion
                                                                    ArchivedItemInOnBase = true;
                                                                    totalSuccessFiles += 1;
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    ErrorOnArchiveItem = true;
                                                                    totalFailedFiles += 1;
                                                                    log.Error("Error Exception Message" + ex.Message);
                                                                    if (ex.StackTrace != null)
                                                                        log.Error("Error Exception.StackTrace  = " + ex.StackTrace);

                                                                    continue;
                                                                }
                                                                #endregion
                                                                #region Replace the orgignal file
                                                                if (docHandle != 0 && ArchivedItemInOnBase == true)
                                                                {
                                                                    #region Create new ASPX file
                                                                    string Url = FolderFilelist[fileCount].Name.Split('.')[0] + ".aspx";
                                                                    string Description = FolderFilelist[fileCount].Name;
                                                                    Microsoft.SharePoint.Client.File file = SPArchive(sp, docHandle.ToString(), Url, folder, Description);

                                                                    log.Info("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " onBase link added");
                                                                    #endregion
                                                                    #region Delete Original File
                                                                    try
                                                                    {
                                                                        
                                                                        if (FolderFilelist[fileCount].CheckOutType == CheckOutType.None)
                                                                        {
                                                                            FolderFilelist[fileCount].CheckOut();
                                                                        }
                                                                        SPClient.DeleteFile(sp, FolderFilelist[fileCount]);
                                                                        
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        ErrorFunction(ex, false);
                                                                        app.Core.Storage.PurgeDocument(app.Core.GetDocumentByID(docHandle));
                                                                        if (file.CheckOutType == CheckOutType.None)
                                                                        {
                                                                            file.CheckOut();
                                                                        }
                                                                        SPClient.DeleteFile(sp, file);
                                                                        log.Info("Created Document Was Deleted from OnBase and Shared Point");
                                                                    }
                                                                    #endregion
                                                                    string fileUrl = FolderFilelist[fileCount].ServerRelativeUrl.Split('.')[0];

                                                                    SPClient.UpdateItemArchived(sp, folder, fileUrl, list.Title);

                                                                    log.Info("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " is Archived");
                                                                    Console.WriteLine("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " is Archived");


                                                                    ArchivedItemInOnBase = false;
                                                                }
                                                                else
                                                                {

                                                                    log.Info("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " for number [" + fileCount + "] is FAILED");
                                                                    Console.WriteLine("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " for number [" + fileCount + "] is FAILED");

                                                                    ErrorOnArchiveItem = true;

                                                                }
                                                                #endregion
                                                            }
                                                            else
                                                            {
                                                                try
                                                                {
                                                                    if (FolderFilelist[fileCount].CheckOutType == CheckOutType.None)
                                                                    {
                                                                        FolderFilelist[fileCount].CheckOut();
                                                                    }
                                                                    SPClient.DeleteFile(sp, FolderFilelist[fileCount]);

                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    log.Error("Error on Delete file Exception Message  = " + ex.Message);
                                                                }
                                                            }
                                                            #endregion

                                                        }
                                                        else
                                                        {
                                                            log.Info("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " for number [" + fileCount + "] is already archived");
                                                            Console.WriteLine("App seq #" + libCount + ", " + folder.Name + ",File Name = " + FolderFilelist[fileCount].Name + " for number [" + fileCount + "] is already archived");

                                                            //ErrorOnArchiveItem = true;

                                                        }
                                                        #endregion
                                                    }
                                                    #endregion
                                                    #region if all files archvied update status in CRM
                                                    if (ErrorOnArchiveItem == false && folder.ItemCount > 0)
                                                    {

                                                        try
                                                        {
                                                            bool updateInCRM = Convert.ToBoolean(configListItems["UpdateStausInCRM"].ToString());

                                                            if ((updateInCRM == true
                                                                && !string.IsNullOrEmpty(configListItems["LibSchemaName"].ToString())
                                                                && !string.IsNullOrEmpty(folder.Name)))
                                                            {
                                                                var libschemname = configListItems["LibSchemaName"].ToString();
                                                                var applicationNo = GetApplicationNum(folder.Name, list.Title);

                                                                bool sStaus = client.UpdateApplicationArchivalStatus(libschemname, "vrp_applicationno", applicationNo, "1");

                                                                log.Info("Call servise SISL  = " + list.Title + " vrp_applicationno " + applicationNo + " 1 " + " and return value is " + sStaus);
                                                                Console.WriteLine("Call servise SISL  = " + list.Title + " vrp_applicationno " + applicationNo + " 1 " + " and return value is " + sStaus);
                                                                log.Info("update status in CRM done");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            ErrorFunction(ex, true);

                                                        }
                                                        
                                                        rootItem["Archived"] = Convert.ToDateTime(DateTime.Now);
                                                        rootItem.Update();
                                                        sp.ExecuteQuery();
                                                        totalSuccessFolders += 1;
                                                        log.Info("update status in SP done");

                                                        log.Info("**************");
                                                        Console.WriteLine("**************");
                                                    }
                                                    else
                                                    {
                                                        totalFailedFolders += 1;
                                                    }
                                                    #endregion
                                                }
                                                else if (archiveAction == 2)
                                                {
                                                    totalFoldersNoNeedToArchive += 1;
                                                    SPClient.MarkItemArchived(sp, rootItem, 2);
                                                    log.Info("App seq #" + libCount + ", " + folder.Name + "ArcihveAction: " + archiveAction + " ARCHIVED as 1/1/2000");

                                                }
                                                else
                                                {
                                                    log.Info("App seq #" + libCount + ", " + folder.Name + "ArcihveAction: " + archiveAction);
                                                }
                                                #endregion
                                            }
                                            catch (Exception ex)
                                            {

                                                ErrorFunction(ex, false);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            TotalNumberofEmptyFolders += 1;
                                            SPClient.MarkItemArchived(sp, rootItem, 2);
                                            log.Info("App seq #" + libCount + ", " + folder.Name + "ArcihveAction:" + archiveAction + " ARCHIVED as 1/1/2000");
                                            log.Info("App seq #" + libCount + ": No Files in this folder...");
                                        }
                                        #endregion
                                    }
                                    #endregion

                                }
                                #endregion
                            }
                            DateTime End = DateTime.Now;
                            log.Info("Finished! " + DateTime.Now.ToString());
                            Console.WriteLine("Finished! " + DateTime.Now.ToString());

                            log.Info("Total no of doucment archived:" + totalSuccessFiles);
                            log.Info("Total no of applications archived:" + totalSuccessFolders);

                            log.Info("---------------------------------------------");
                            Console.WriteLine("---------------------------------------------");
                        }
                        catch (Exception ex)
                        {
                            ErrorFunction(ex, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorFunction(ex, false);
                  //  if (app != null) 
                       // conn.Disconnect(app);
                }
                finally
                {
                    EmailSender emlSender = new EmailSender();
                    emlSender.SendEmail(Start, DateTime.Now, "OnBase CRM Tool Status", totalProcessedFolders , totalFailedFolders , totalSuccessFolders , totalSuccessFiles , totalFailedFiles , totalFoldersNoNeedToArchive , TotalNumberofEmptyFolders);
                   // if (app != null) 
                       // conn.Disconnect(app);
                }
            }
        }
        public static  void  InitateOnBaseApplication()
        {

            try
            {

                if (app == null)
                {
                    Connection conn = new Connection();
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, x509Certificate, chain, sslPolicyErrors) => true;

                    app = conn.Connect(OnBaseUserName, OnBasePassword, OnBaseODBC, OnBaseService, Hyland.Unity.LicenseType.QueryMetering);
                    
                    log.Info("Connected To BaseSite = " + OnBaseService + " At =  " + DateTime.Now.ToString());
                    Console.WriteLine("Connected To BaseSite = " + OnBaseService + " At =  " + DateTime.Now.ToString());
                }


            }

            catch (Exception ex)
            {
                ErrorFunction(ex, false);
            }
        }
        private Dictionary<long, object> FillParameter(string folderName, string documentType)
        {
            try
            {
                Dictionary<long, object> frmData = new Dictionary<long, object>() { };

                string CIF = null;
                string Subject = null;

                switch (documentType)
                {
                    case "CreditCard Application":
                        {
                            string[] metaData = folderName.Split('-');


                            int NumIndex = metaData.Count();

                            if (NumIndex == 4)
                            {
                                string CustomerName = metaData[0] + " " + metaData[1];
                                if (CustomerName.Length >= 250)
                                {
                                    frmData.Add(Convert.ToInt64(AppCustomerName), CustomerName.Substring(0, 249)); // Customer Name 117
                                }
                                frmData.Add(Convert.ToInt64(AppCustomerName), CustomerName); // CIF 257
                                frmData.Add(Convert.ToInt64(AppCIF), metaData[2]); // Applicaton No 369
                                frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[3]); // Customer Name 117
                            }
                            else
                            {
                                if (metaData[0].Length >= 250)
                                {
                                    frmData.Add(Convert.ToInt64(AppCustomerName), metaData[0].Substring(0, 249)); // Customer Name 117
                                }
                                frmData.Add(Convert.ToInt64(AppCustomerName), metaData[0]); // CIF 257
                                frmData.Add(Convert.ToInt64(AppCIF), metaData[1]); // Applicaton No 369
                                frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[2]); // Customer Name 117
                            }


                        }
                        break;
                    case "Case":
                        {
                            CIF = folderName.Split(' ')[0].ToString();
                            Subject = folderName.Substring(8).Split('[')[0];

                            string trackingNo = "";
                            if (folderName.Substring(8).Split('[').Length > 1)
                            {
                                trackingNo = folderName.Substring(8).Split('[')[1];
                                trackingNo = trackingNo.Split(']')[0].Trim();
                            }

                            frmData.Add(Convert.ToInt64(AppCIF), CIF); // CIF 257
                            if (Subject.Length >= 250)
                            {
                                frmData.Add(Convert.ToInt64(AppSubject), Subject.Substring(0, 249)); // Customer Name 117
                            }
                            frmData.Add(Convert.ToInt64(AppSubject), Subject); // Customer Name 117
                            frmData.Add(Convert.ToInt64(AppTrackingNumber), trackingNo); // Customer Name 117

                        }
                        break;
                    case "Account":
                        {
                            string[] metaData = folderName.Split('-');
                            frmData.Add(Convert.ToInt64(AppCustomerName), metaData[1]); // Customer Name 117
                        }
                        break;
                    case "Background Check":
                    case "Document Retrieval":
                    case "Eligibility Check":
                        {
                            string[] metaData = folderName.Split('-');
                            frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[0]); // Applicaton No 369
                        }
                        break;
                    case "CD Application":
                        {
                            string[] metaData = folderName.Split('-');
                            frmData.Add(Convert.ToInt64(AppCIF), metaData[0]); // CIF 257
                            frmData.Add(Convert.ToInt64(AppCustomerName), metaData[1]); // Customer Name 117
                            frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[0]); // Applicaton No 369
                        }
                        break;
                    case "Dishonored Check Application":
                    case "Retail Account Opening":
                    case "Corporate Customer Enrollment":
                        {
                            ////ADUserInfo.ADUserName = !string.IsNullOrEmpty(DEUser.Properties[ADUserName].Value as string) ? DEUser.Properties[ADUserName].Value as string : string.Empty;
                            string[] metaData = folderName.Split('-');
                            if (metaData.Length == 2)
                            {
                                if (metaData[1].Length >= 250)
                                {
                                    frmData.Add(Convert.ToInt64(AppCustomerName), metaData[1].Substring(0, 249)); // Customer Name 117
                                }
                                frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[0]); // Applicaton No
                                frmData.Add(Convert.ToInt64(AppCustomerName), metaData[1]); // Customer Name
                            }
                            else
                            {
                                frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[0]);
                                //frmData.Add(Convert.ToInt64(ConfigurationManager.AppSettings["ApplicatonNo"]), "Null");
                            }

                        }
                        break;
                    case "systemuser":
                        {
                            //frmData.Add(369, metaData[0]); // Branch
                            //frmData.Add(257, metaData[1]); // UserID
                            //frmData.Add(117, metaData[2]); // YYYYMM
                        }
                        break;
                    default:
                        {
                            string[] metaData = folderName.Split('-');
                            if (metaData.Length == 3)
                            {
                                if (metaData[2].Length >= 250)
                                {
                                    frmData.Add(Convert.ToInt64(AppCustomerName), metaData[2].Substring(0, 249)); // Customer Name 117
                                }
                                frmData.Add(Convert.ToInt64(AppApplicatonNo), metaData[0]); // Applicaton No
                                frmData.Add(Convert.ToInt64(AppCIF), metaData[1]); // CIF                          
                                frmData.Add(Convert.ToInt64(AppCustomerName), metaData[2]); // Customer Name
                            }
                        }
                        break;

                }
                return frmData;
            }
            catch (Exception ex)
            {
                ErrorFunction(ex, false);

                return null;
            }
        }
        private string GetApplicationNum(string folderName, string documentType)
        {
            try
            {
                switch (documentType)
                {
                    case "CreditCard Application":
                        {
                            
                            string[] metaData = folderName.Split('-');
                            return metaData[metaData.Length - 1].ToString().Trim(); ;
                        }
                    case "Case":
                        {
                            string trackingNo = "";
                            if (folderName.Substring(8).Split('[').Length > 1)
                            {
                                trackingNo = folderName.Substring(8).Split('[')[1];
                                trackingNo = trackingNo.Split(']')[0].Trim();
                            }
                            else
                            {
                                trackingNo = folderName;
                            }


                            return trackingNo;
                        }
    
                    case "Account":
                        {
                            string[] metaData = folderName.Split('-');
                            return metaData[0].ToString().Trim(); 
                        }
         
                    case "Background Check":
                    case "Document Retrieval":
                    case "Eligibility Check":
                        {
                            string[] metaData = folderName.Split('-');
                            return metaData[0].ToString().Trim(); 
                        }
            
                    case "CD Application":
                        {
                            string[] metaData = folderName.Split('-');
                            return metaData[0].ToString().Trim(); 
                        }
         
                    case "Dishonored Check Application":
                    case "Retail Account Opening":
                    case "Corporate Customer Enrollment":
                        {
                            ////ADUserInfo.ADUserName = !string.IsNullOrEmpty(DEUser.Properties[ADUserName].Value as string) ? DEUser.Properties[ADUserName].Value as string : string.Empty;
                            string[] metaData = folderName.Split('-');
                            return metaData[0].ToString().Trim();

                        }
              
                    case "systemuser":
                        {
                            //frmData.Add(369, metaData[0]); // Branch
                            //frmData.Add(257, metaData[1]); // UserID
                            //frmData.Add(117, metaData[2]); // YYYYMM
                        }
                        break;
                    default:
                        {
                            string[] metaData = folderName.Split('-');
                            return metaData[0].ToString().Trim();
                        }
               

                }
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Error Exception Message  =  " + ex.Message);

                if (ex.InnerException != null)
                    log.Error("Error Exception  Message InnerException  = " + ex.InnerException);

                if (ex.StackTrace != null)
                    log.Error("Error Exception  Message StackTrace  = " + ex.StackTrace);
                Console.WriteLine("Error Exception Message : " + "  " + DateTime.Now.ToString());
                Console.WriteLine("Error Exception.InnerException : " + "  " + DateTime.Now.ToString());
                return null;
            }
        }
        public static ListItemCollection GetPeriodList(SPClient sp)
        {
            ListItemCollection _ListItemCollection = null;

            try
            {
                List configList = sp.GetListByTitle(PeriodListName);
                CamlQuery configCamlQuery = new CamlQuery();
                configCamlQuery.ViewXml =
                                    @"<View>
                                             <Query>
                                                 <Where>
                                              <Eq>
                                                 <FieldRef Name='Active' />
                                                 <Value Type='Integer'>1</Value>
                                              </Eq>
                                            </Where>
                                                 </Query>
                                              </View>";
                 _ListItemCollection = configList.GetItems(configCamlQuery);
                 sp.Load(_ListItemCollection);
                sp.ExecuteQuery();

            }
            catch (Exception ex)
            {
                ErrorFunction(ex, false);
            }
            return _ListItemCollection;
        }
        public static ListItemCollection GetFolders(SPClient sp, string RowLimit, string Period, string ArchiveStartDate, List list)
        {
            ListItemCollection listCollection = null;

            try
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                                "<View>" +
                                    @"<Query>
                                            <Where>
                                                <And>
                                                    <And>
                                                        <IsNull>
                                                               <FieldRef Name='" + "Archived" + @"'/>
                                                        </IsNull>
                                                        <Leq>
                                                            <FieldRef Name='Created'/>
                                                            <Value Type='DateTime' IncludeTimeValue='FALSE'><Today OffsetDays='-" + Period + @"'/></Value>
                                                        </Leq>
                                                    </And>
                                                    <Geq>
                                                        <FieldRef Name='Created'/>
                                                        <Value Type='DateTime' IncludeTimeValue='FALSE'>" + ArchiveStartDate + @"</Value>
                                                    </Geq>
                                                </And>
                                            </Where>
                                        </Query><RowLimit>" + RowLimit + "</RowLimit></View>";

                listCollection = list.GetItems(camlQuery);
                sp.Load(listCollection);
                sp.ExecuteQuery();

            }
            catch (Exception ex)
            {
                ErrorFunction(ex, false);
            }
            return listCollection;

        }
        public Microsoft.SharePoint.Client.File SPArchive(SPClient sp, string docHandle, string Url, Folder folder, string Description)
        {
            string docLinkTemplate = null;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(DocLinkTemplate + "\\SharePointDocLinkTemplate.txt"))
            {
                docLinkTemplate = sr.ReadToEnd();
            }
            string ServerUrl = OnBaseSite.ToString();
            string docLink = ServerUrl + "/AppNet/docpop/docpop.aspx?clienttype=html&docid=" + docHandle.ToString();

            docLinkTemplate = docLinkTemplate.Replace("{0}", docLink);

            FileCreationInformation fileCreateInfo = new FileCreationInformation();
            fileCreateInfo.Content = Encoding.UTF8.GetBytes(docLinkTemplate);
            fileCreateInfo.Url = Url;
            Microsoft.SharePoint.Client.File file = folder.Files.Add(fileCreateInfo);
            ListItem fileListItem = file.ListItemAllFields;
            fileListItem["ContentType"] = "Link to a Document";
            FieldUrlValue urlValue = new FieldUrlValue();
            urlValue.Description = Description;
            urlValue.Url = docLink;

            fileListItem["URL"] = urlValue;
            sp.Load(file);
            sp.ExecuteQuery();
            return file;
        }
        public static void ErrorFunction(Exception ex, bool SISL)
        {
            if (SISL == true)
            {
                log.Info("Error Exception Message  = " + ex.Message);

                if (ex.InnerException != null)
                    log.Error("Call servise SISL.InnerException = " + ex.InnerException);

                if (ex.StackTrace != null)
                    log.Error("Call servise SISL.StackTrace  = " + ex.StackTrace);
                Console.WriteLine("Error Exception Message : " + ex.Message + "  " + DateTime.Now.ToString());
                Console.WriteLine("Call servise SISL.InnerException : " + ex.InnerException + "  " + DateTime.Now.ToString());
            }
            else
            {
                log.Error("Error Exception  Message  = " + ex.Message);

                if (ex.InnerException != null)
                    log.Error("Error Exception Message InnerException  = " + ex.InnerException);

                if (ex.StackTrace != null)
                    log.Error("Error Exception Message StackTrace  = " + ex.StackTrace);
                Console.WriteLine("Error Exception Message : " + ex.Message + "  " + DateTime.Now.ToString());
                Console.WriteLine("Error Exception.InnerException : " + ex.InnerException + "  " + DateTime.Now.ToString());
            }
        }
        public void Console_CancelKeyPress(object sender, EventArgs e)
        {
            if (app != null)
                app.Disconnect();
        }
    }

}
