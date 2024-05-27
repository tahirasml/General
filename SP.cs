using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using Hyland.Unity;
using System.Configuration;

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
            #region Get The Configuration List

            ListItemCollection PeriodListColl = SharePointHelper.GetPeriodList(sp);

            log.Info("Connect To Retention Period  " + " At =  " + DateTime.Now.ToString());
            Console.WriteLine("Connect To Retention Period  " + " At =  " + DateTime.Now.ToString());

            #endregion

            // connect to OnBase
            InitateOnBaseApplication();

            foreach (ListItem configListItems in PeriodListColl)
            {
                string RowLimit = null;

                try
                {
                    #region Fill the Configuration Parameter

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

                            listOfFolders = SharePointHelper.GetFolders(sp, RowLimit, Period, ArchiveStartDate, list);

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
                            int archiveAction = 1; // default 1 -> Archive

                            #region check if the type is Folder

                            if (rootItem.FileSystemObjectType == FileSystemObjectType.Folder)
                            {
                                totalProcessedFolders += 1;
                                Folder folder = sp.GetFolderItems(rootItem["FileRef"].ToString());
                                bool ErrorOnArchiveItem = false;

                                #region Get All Subfolders up to 4 levels

                                List<ListItem> allFolders = new List<ListItem>();
                                SharePointHelper.GetAllFoldersAndSubfolders(sp, list, allFolders, rootItem["FileRef"].ToString());

                                foreach (ListItem subFolderItem in allFolders)
                                {
                                    // Process each subfolder
                                    Folder subFolder = sp.GetFolderItems(subFolderItem["FileRef"].ToString());
                                    ProcessFolder(sp, app, subFolder, configListItems, documentType, list, ref libCount, ref totalProcessedFolders, ref totalSuccessFiles, ref totalFailedFiles, ref totalSuccessFolders, ref totalFailedFolders, ref totalFoldersNoNeedToArchive, ref TotalNumberofEmptyFolders);
                                }

                                #endregion

                                // Process the root folder itself
                                ProcessFolder(sp, app, folder, configListItems, documentType, list, ref libCount, ref totalProcessedFolders, ref totalSuccessFiles, ref totalFailedFiles, ref totalSuccessFolders, ref totalFailedFolders, ref totalFoldersNoNeedToArchive, ref TotalNumberofEmptyFolders);
                            }

                            #endregion
                        }

                        #endregion
                    }

                    DateTime End = DateTime.Now;
                    log.Info("Finished! " + DateTime.Now.ToString());
                    Console.WriteLine("Finished! " + DateTime.Now.ToString());

                    log.Info("Total no of document archived:" + totalSuccessFiles);
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

        private static void ProcessFolder(SPClient sp, Hyland.Unity.Application app, Folder folder, ListItem configListItems, Hyland.Unity.DocumentType documentType, List list, ref int libCount, ref int totalProcessedFolders, ref int totalSuccessFiles, ref int totalFailedFiles, ref int totalSuccessFolders, ref int totalFailedFolders, ref int totalFoldersNoNeedToArchive, ref int TotalNumberofEmptyFolders)
        {
            bool ErrorOnArchiveItem = false;
            int archiveAction = 1; // default 1 -> Archive

            #region Check if the folder contains documents

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
                        // archiveAction = _CheckArchiveAction.CheckArchiveAction(spName, appNoValue, SQLConnString);
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
                    log.Info("This error occurred while fetching folders from SharePoint list name is  : " + list.Title);
                    ErrorFunction(ex, false);
                    return;
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
                        return;
                    }

                    #endregion

                    #region IF Action is 1

                    if (archiveAction == 1)
                    {
                        DocumentHelper dochelper = new DocumentHelper();
                        long docHandle = 0;

                        #region folder files loop

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
                                        string fileFormat = nametokens[nametokens.Length - 1];  // make sure the extension is the last token from split

                                        if (nametokens.Length == 1) // in case no extension 
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

                                    #region Replace the original file

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
                                            log.Info("Created Document Was Deleted from OnBase and SharePoint");
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
                                // ErrorOnArchiveItem = true;
                            }

                            #endregion
                        }

                        #endregion

                        #region if all files archived update status in CRM

                        if (ErrorOnArchiveItem == false && folder.ItemCount > 0)
                        {
                            try
                            {
                                bool updateInCRM = Convert.ToBoolean(configListItems["UpdateStausInCRM"].ToString());

                                if (updateInCRM == true
                                    && !string.IsNullOrEmpty(configListItems["LibSchemaName"].ToString())
                                    && !string.IsNullOrEmpty(folder.Name))
                                {
                                    var libschemname = configListItems["LibSchemaName"].ToString();
                                    var applicationNo = GetApplicationNum(folder.Name, list.Title);
                                    bool sStaus = client.UpdateApplicationArchivalStatus(libschemname, "vrp_applicationno", applicationNo, "1");

                                    log.Info("Call service SISL  = " + list.Title + " vrp_applicationno " + applicationNo + " 1 " + " and return value is " + sStaus);
                                    Console.WriteLine("Call service SISL  = " + list.Title + " vrp_applicationno " + applicationNo + " 1 " + " and return value is " + sStaus);

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
                    return;
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
}
