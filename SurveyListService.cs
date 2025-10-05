using System;
using System.Collections.Generic;
using System.Linq;
using Veripark.Xrm.Survey.BusinessLogic.Contracts;
using Veripark.Xrm.Survey.Domain.Contracts;
using Vrp.Crm.Tracing;
using Microsoft.Xrm.Sdk;
using System.Data;
using Veripark.Xrm.Survey.Domain;
using Veripark.Xrm.Survey.SMSHelperLibrary.Helper;
using Vrp.Crm.Utility.ConfigurationManager;
using System.Data.SqlClient;
using Veripark.Xrm.Survey.SMSHelperLibrary;
namespace Veripark.Xrm.Survey.BusinessLogic
{


    public class SurveyListService : ISurveyListService
    {
        ISurveyRepository surveyRepository;
        IMarketListRepository marketListRepository;
        IMarketListMemberRepository marketListMemberRepository;
        ISurveyFilterRepository surveyFilterRepository;
        ICommonRepository commonRepository;
        ISurveyFilterService surveyFilterService;
        List<Guid> ExistingListIDs;
        List<Guid> AllRecords;
        List<Guid> ExistingParticpants;
        List<Guid> StaticListIDConvertedFromDynamicList;


        public SurveyListService(ISurveyRepository surveyRepository, IMarketListRepository marketListRepository, IMarketListMemberRepository marketListMemberRepository, ISurveyFilterRepository surveyFilterRepository, ICommonRepository commonRepository)
        {
            this.surveyRepository = surveyRepository;
            this.marketListRepository = marketListRepository;
            this.marketListMemberRepository = marketListMemberRepository;
            this.surveyFilterRepository = surveyFilterRepository;
            this.commonRepository = commonRepository;
        }






        public bool CreateSurveySubList(Guid surveyId)
        {
            try
            {
                Domain.vrp_survey survey = surveyRepository.GetSurveyById(surveyId);



                AllRecords = new List<Guid>();
                ExistingParticpants = new List<Guid>();
                // VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList Validation..", "Start");

                if (!survey.vrp_IsChildSurvey.Value)
                {
                    // VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList survey.vrp_IsChildSurvey.Value..", survey.vrp_IsChildSurvey.Value.ToString());


                    if (survey.vrp_ApplyFilterClick == "Submit" ||
                        survey.vrp_ApplyFilterClick == "Approve" ||
                        survey.vrp_ApplyFilterClick == "SendBack" ||
                        survey.vrp_ApplyFilterClick == "Distribute")
                    {
                        //     VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList survey.vrp_ApplyFilterClick..", survey.vrp_ApplyFilterClick);
                        return true;
                    }


                    if (survey.vrp_SurveyStatus.Value != 1 && survey.vrp_SurveyStatus.Value != 9) // On Draft Or Apply Filter Click 
                        return true;
                }
                //   VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList Validation..", "Ends");



                if (survey.vrp_SurveyStatus.Value == 5) // Survey Expiry 
                    return true;

                if (survey.vrp_SurveyStatus.Value == 1) // Survey Draft 
                    return true;

                int TempRecordCounts = 0;


                //cleare existing filters
                surveyRepository.LoadSurveyFilters(survey);
                List<vrp_surveyfilter> PreviousSurveyFilters = new List<vrp_surveyfilter>();

                if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
                {
                    foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey)
                    {
                        commonRepository.DeleteEntityRecord(Filter.LogicalName, Filter.Id);
                        PreviousSurveyFilters.Add(Filter);
                    }


                }

                // Autigenerate Filter Conditions
                if (survey.vrp_SurveyMultiFilter != null)
                {
                    if (survey.vrp_SurveyMultiFilter.Id != null)
                    {
                        surveyFilterService = new SurveyFilterService(surveyFilterRepository, surveyRepository);
                        surveyFilterService.AutoGenerateFilters(surveyId);
                    }
                }




                surveyRepository.LoadSurveyMarketLists(survey);
                if (survey.vrp_vrp_survey_M_list == null) //If marketing list is not loaded.
                {
                    survey.vrp_SurveyStatus.Value = 1;
                    surveyRepository.UpdateSurvey(survey);

                    return true;

                }

                surveyRepository.LoadSurveyQuestions(survey);
                if (survey.vrp_survey_surveyquestion == null) //If questions is not list is not loaded.
                {
                    survey.vrp_SurveyStatus.Value = 1;
                    surveyRepository.UpdateSurvey(survey);

                    return true;

                }



                // Auto Generate Marketing List for Survey For Types

                //if (survey.vrp_SurveyForType != null)
                //{
                //    if (survey.vrp_SurveyForType.Value != 168260000)// not general survey
                //    {
                //        //  create marketing list for if type =complain/murahaba/accountopening
                //        CreateMLSpecial(survey);
                //    }

                //}




                surveyRepository.LoadFilteredParticipantsList(survey);
                // if exists , delete all
                ExistingListIDs = new List<Guid>();
                if (survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey != null)
                {
                    foreach (var FilteredParticipantsList in survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey)
                    {
                        ExistingListIDs.Add(FilteredParticipantsList.vrp_ParticipantList.Id);
                        commonRepository.DeleteEntityRecord(FilteredParticipantsList.vrp_ParticipantList.LogicalName, FilteredParticipantsList.vrp_ParticipantList.Id);
                        commonRepository.DeleteEntityRecord(FilteredParticipantsList.LogicalName, FilteredParticipantsList.Id);
                    }
                }



                StaticListIDConvertedFromDynamicList = new List<Guid>();
                foreach (var list in survey.vrp_vrp_survey_M_list)
                {
                    if (list.Type.Value)//Dynamic
                    {
                        StaticListIDConvertedFromDynamicList.Add(marketListRepository.CopyDynamicListToStatics(list.Id));
                    }
                }
                //Guid MergedListID = MergeMarketingLists(survey.vrp_vrp_survey_M_list, survey);


                // Sample Size calculation
                int SampleSize = 0;
                if (survey.vrp_EnableSurveySampling.Value)
                {
                    if (survey.vrp_SampleSizeofSurvey != null)
                    {
                        SampleSize = survey.vrp_SampleSizeofSurvey.Value;
                    }
                    if (survey.vrp_IsOverrideSampleSize.Value == true)
                        SampleSize = survey.vrp_OverrideSampleSize.Value;
                }
                else
                {

                    if (StaticListIDConvertedFromDynamicList.Count > 0)
                    {
                        foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                        {
                            SampleSize = SampleSize + marketListRepository.GetMemeberCountOfStaticList(ListID);
                        }
                    }


                    foreach (var list in survey.vrp_vrp_survey_M_list)
                    {
                        if (!list.Type.Value)//Dynamic
                            SampleSize = SampleSize + list.MemberCount.Value;
                    }
                }





                Guid TargetListID = new Guid();
                switch (survey.vrp_SurveyFor.Value)
                {

                    case 168260000:
                        TargetListID = marketListRepository.CreateMarketingList("SGL_C_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 2, false, "");
                        break;
                    case 168260001:
                        TargetListID = marketListRepository.CreateMarketingList("SGL_A_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 1, false, "");
                        break;
                    case 168260002:
                        TargetListID = marketListRepository.CreateMarketingList("SGL_L_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 4, false, "");
                        break;
                }
                marketListRepository.CreateFilteredSurveyList(TargetListID.ToString(), TargetListID, survey.Id);



                // Apply survey Filter
                surveyRepository.LoadSurveyFilters(survey);
                if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
                {
                    if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters) != null)
                        foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters))
                        {
                            decimal Percentage = Filter.vrp_Percentage.Value;
                            Int32 NumberOfRecords = Convert.ToInt32(SampleSize * Percentage / 100);
                            TempRecordCounts = NumberOfRecords; ;
                            Filter.vrp_RequiredSamples = NumberOfRecords.ToString();

                            if (NumberOfRecords > 0)
                            {
                                // string createdfromcode = "";
                                switch (survey.vrp_SurveyFor.Value)
                                {

                                    case 168260000: // Contacts//       createdfromcode = "2";                                          


                                        ApplyFilterForContacts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                                        break;
                                    case 168260001:
                                        //     createdfromcode = "1";  


                                        ApplyFilterForAccounts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                                        break;
                                    case 168260002:
                                        //   createdfromcode = "4";

                                        ApplyFilterForLeads(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                                        break;
                                    default:
                                        // createdfromcode = "99";
                                        break;
                                }


                            }
                        }
                }
                // Total Filtered Percentage
                // Load remaining memebers
                decimal totalFilteredPercentage = 0;
                decimal RequiredNumberOfRecord = 0;
                surveyRepository.LoadSurveyFilters(survey);
                if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
                    if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters) != null)
                        foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters))
                        {
                            totalFilteredPercentage = totalFilteredPercentage + Filter.vrp_Percentage.Value;

                        }
                if (totalFilteredPercentage < 100)
                {
                    if (totalFilteredPercentage == 0)
                    {
                        RequiredNumberOfRecord = SampleSize;
                    }
                    else
                    {

                        RequiredNumberOfRecord = SampleSize * Convert.ToInt32(100 - totalFilteredPercentage) / 100;
                    }
                }
                TempRecordCounts = (int)RequiredNumberOfRecord;
                if (TempRecordCounts > 0)
                {

                    switch (survey.vrp_SurveyFor.Value)
                    {

                        case 168260000://createdfromcode = "2"; //Contacts

                            ApplyFilterForContacts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);
 
                            break;
                        case 168260001: //createdfromcode = "1"; // Accounts

                            ApplyFilterForAccounts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                            break;
                        case 168260002: //createdfromcode = "4"; // Leads

                            ApplyFilterForLeads(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                            break;
                        default:
                            //createdfromcode = "99";
                            break;
                    }

                }
                if (StaticListIDConvertedFromDynamicList.Count > 0)
                {
                    foreach (Guid ListId in StaticListIDConvertedFromDynamicList)
                    {
                        commonRepository.DeleteEntityRecord("list", ListId);
                    }
                }


                survey.vrp_SurveyStatus.Value = 1;
                surveyRepository.UpdateSurvey(survey);

                return true;

            }
            catch (Exception ex)
            {
                string exceptionMessage = Vrp.Crm.ExceptionLibrary.ExceptionMethods.ProcessException(ex);
                VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList failed..", exceptionMessage + "--" + ex.ToString());
                throw new Exception(exceptionMessage, ex);
            }
        }
        public void CreateSurveySubList(Guid surveyId, List<DataRow> contactListWithTransactionDetail, Logging log, SqlConnection externalDBConnection)
        {
            try
            {
                if (CreateSurveySubList(surveyId))
                {
                    log.MessageTraceLog("Setting outbound Transaction table with Survey Id - Start");
                    //After inserting customers to TML, update the Outbound Transactions
                    //with the “SurveyId”, “ProcessedDateTime” and update flag “IsAssignedToSurvey” to True.
                    DataTable dt = DatabaseHelper.createDataTable<DailyTransactions>();
                    List<DataRow> subContactListWithTransaction = new List<DataRow>();
                    List<Error> errorMessages = new List<Error>();

                    var transactionId = string.Empty;
                    var transactionDate = string.Empty;

                    int NumberOfObjectsPerPage = SurveySMSLogic.getNumberOfObjectsPerPage();
                    int TotalRecordCount = contactListWithTransactionDetail.Count;
                    int TotalPages = Convert.ToInt32(Math.Ceiling((double)TotalRecordCount / (double)NumberOfObjectsPerPage));
                    int i = 0;
                    log.MessageTraceLog("Total members asscoiated with memberlist - " + TotalRecordCount.ToString());
                    log.MessageTraceLog("Total members asscoiated with filtered memberlist - " + ExistingParticpants.Count.ToString());
                    i = 0;
                    while (i < TotalPages || TotalPages == 0)
                    {
                        subContactListWithTransaction = contactListWithTransactionDetail.Skip(i * NumberOfObjectsPerPage).Take(NumberOfObjectsPerPage).ToList();

                        foreach (var contact in subContactListWithTransaction)
                        {
                            DataRow dr = dt.NewRow();
                            //Checking whether the contactList member is presented in the SurveySublist or Filtered Out                           
                            var transactionDetail = ExistingParticpants.Where(e => e.ToString().ToLower() == contact.ItemArray[0].ToString().ToLower()).FirstOrDefault();
                            if (transactionDetail != null && transactionDetail != Guid.Empty)
                            {
                                transactionId = contact.ItemArray[1].ToString();
                                dr["SurveyId"] = surveyId.ToString();
                                dr["TransactionId"] = transactionId;
                                dr["IsAssignedToSurvey"] = "true";
                                dr["Status"] = "SurveyAssigned";
                                dr["ErrorCode"] = "";
                                dr["ErrorMessage"] = "";
                                dt.Rows.Add(dr);
                            }
                            else
                            {
                                transactionId = contact.ItemArray[1].ToString();
                                dr["SurveyId"] = surveyId.ToString();
                                dr["TransactionId"] = transactionId;
                                //contact is added in marketing list but not added in "Filtered Survey Participant List"
                                dr["IsAssignedToSurvey"] = "true";
                                dr["Status"] = "FilteredOut";
                                dr["ErrorCode"] = "";
                                dr["ErrorMessage"] = "";
                                dt.Rows.Add(dr);
                            }

                        }
                        i = i + 1;
                        if (DatabaseHelper.setTransactionStatusWithSurveyIdInBulk(externalDBConnection, dt, errorMessages))
                            log.MessageTraceLog("Outbound Transaction table updated with status & Survey Id. Page - " + i.ToString() + "/" + TotalPages.ToString());
                        else
                            log.MessageTraceLog("Outbound Transaction table is not updated with status & Survey Id. Page - " + i.ToString() + "/" + TotalPages.ToString());
                    }
                }
                else
                {
                    throw new Exception("Survey Sublist not created.");
                }
            }
            catch (Exception ex)
            {
                string exceptionMessage = Vrp.Crm.ExceptionLibrary.ExceptionMethods.ProcessException(ex);
                VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList failed..", exceptionMessage + "--" + ex.ToString());
                throw new Exception(exceptionMessage, ex);
            }
        }

        // To Create Marketing List for special type of cases
        private void CreateMLSpecial(vrp_survey survey)
        {
            string SurveyTypeFor = "";
            string FetchXML = "";
            switch (survey.vrp_SurveyForType.Value)
            {
                case 168260001:
                    SurveyTypeFor = "RMA";//Recent Murahab Account
                    switch (survey.vrp_SurveyFor.Value)
                    {
                        case 168260000:
                            // Fetch XML Recent Retail Loan;
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_C_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 2, true, FetchXML);
                            break;
                        case 168260001:
                            // Fetch XML Recent Corporate Loan
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_A_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 1, true, FetchXML);
                            break;
                            //case 168260002:
                            //    marketListRepository.CreateMarketingList("SGL_L_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 4, true, FetchXML);
                            //    break;
                    }
                    break;
                case 168260002:
                    SurveyTypeFor = "RAO";//Recent Account Opened  
                    switch (survey.vrp_SurveyFor.Value)
                    {
                        case 168260000:
                            //Fetch XML for Recent Retail Account Opened
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_C_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 2, true, FetchXML);
                            break;
                        case 168260001:
                            //Fetch XML for Recent Corporate Account Opened
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_A_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 1, true, FetchXML);
                            break;
                            //case 168260002:
                            //    marketListRepository.CreateMarketingList("SGL_L_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 4, true, FetchXML);
                            //    break;
                    }
                    break;
                case 168260003:
                    SurveyTypeFor = "RCC"; // Recent Complaints Closed
                    switch (survey.vrp_SurveyFor.Value)
                    {
                        case 168260000:
                            //Fetch XML for Recent Complaint closed for Retail
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_C_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 2, true, FetchXML);
                            break;
                        case 168260001:
                            //Fetch XML for Recent Complaint closed for Corporate
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_A_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 1, true, FetchXML);
                            break;
                        case 168260002:
                            //Fetch XML for Recent Complaint closed for Leads
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_L_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 4, true, FetchXML);
                            break;
                    }
                    break;

            }
        }

        private void ApplyFilterForContacts(Domain.vrp_survey Survey, Domain.vrp_surveyfilter Filter, Int32 NumberOfRecords, DateTime ExcludeDate, Guid TargetListID)
        {

            string GenderFilter = "";
            string RegionFilter = "";
            string BranchFilter = "";
            string SegmentFilter = "";
            string AgeFilter = "";
            string ExcludeDaysFilter = "";
            string MarketingListEntityLink = "";
            string FilterValues = "";
            string RecordCountSnippet = "";
            string FetchXML = "";
            string ExceptLists = "";

            // Setup Filter
            if (Filter != null)
            {
                if (Filter.vrp_Gender != null)
                {
                    if (Filter.vrp_Gender.Value != null)
                    {
                        string vrp_gendertype = "";

                        string gendercode = "";


                        if (Filter.vrp_Gender.Value.ToString() == "1")
                        {
                            vrp_gendertype = "1";

                            gendercode = "1";

                        }
                        else
                        {
                            vrp_gendertype = "2";

                            gendercode = "2";

                        }

                        GenderFilter = @"<filter type='or'>
                                            <condition attribute='vrp_gendertype' operator='eq' value='" + vrp_gendertype + @"' />                                       
                                            <condition attribute='gendercode' operator='eq' value='" + gendercode + @"' />
                                            
                                          </filter>";
                    }

                }
                if (Filter.vrp_Branch != null)
                    if (Filter.vrp_Branch.Id != null)
                    {
                        BranchFilter = @"<filter type='or'>
                                                <condition attribute='vrp_branch' operator='eq'  uitype='vrp_branch' value='{" + Filter.vrp_Branch.Id.ToString() + @"}' />
                                                <condition attribute='vrp_branchid' operator='eq'  uitype='vrp_branch' value='{" + Filter.vrp_Branch.Id.ToString() + @"}' />
                                            </filter>";
                    }


                if (Filter.vrp_RetailSegment != null)
                    if (Filter.vrp_RetailSegment.Id != null)
                        SegmentFilter = @"<condition attribute='vrp_customersegment' operator='eq'   uitype='vrp_customersegment' value='{" + Filter.vrp_RetailSegment.Id.ToString() + "}' />";

                if (Filter.vrp_RetailRegion != null)
                    if (Filter.vrp_RetailRegion.Id != null)
                        RegionFilter = @"<condition attribute='vrp_region' operator='eq'   uitype='territory' value='{" + Filter.vrp_RetailRegion.Id.ToString() + "}' />";


                if (Filter.vrp_AgeFrom != null && Filter.vrp_AgeTo != null)
                {
                    if (Filter.vrp_AgeFrom.HasValue && Filter.vrp_AgeTo.HasValue)
                    {
                        AgeFilter = @" <filter type='and'>
                            <condition attribute='birthdate' operator='on-or-before' value='" + DateTime.Now.AddYears(-1 * Filter.vrp_AgeFrom.Value) + @"' />
                            <condition attribute='birthdate' operator='on-or-after' value='" + DateTime.Now.AddYears(-1 * Filter.vrp_AgeTo.Value) + @"' />
                          </filter>";
                    }
                }

            }

            var SurveyTransactionType = Survey.vrp_transactiontype != null && !string.IsNullOrEmpty(Survey.vrp_transactiontype.Name) ? Survey.vrp_transactiontype.Name : "";
            var SurveyExcludeDaysFilter = "";
            bool isTimeExemptedTransactionTypes = false;
            var fetchTransactionTypesFromLocalization = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                              <entity name='vrp_veritouch_localization'>
                                                                <attribute name='vrp_veritouch_localizationid' />
                                                                <attribute name='vrp_key' />
                                                                <attribute name='vrp_value' />
                                                                <order attribute='vrp_key' descending='false' />
                                                                <filter type='and'>
                                                                  <condition attribute='vrp_key' operator='eq' value='SurveyTimeExemptedTransactionTypes' />
                                                                </filter>
                                                              </entity>
                                                            </fetch>";

            EntityCollection FilteredTransactionTypesFromLocalizationRecords = commonRepository.RetriveByFetchXML(fetchTransactionTypesFromLocalization);
            if (FilteredTransactionTypesFromLocalizationRecords.Entities != null || FilteredTransactionTypesFromLocalizationRecords.Entities.Count > 0)
            {
                if(FilteredTransactionTypesFromLocalizationRecords.Entities[0].Contains("vrp_value") && !string.IsNullOrEmpty(SurveyTransactionType))
                {
                    string[] SurveyTimeExemptedTransactionTypes = FilteredTransactionTypesFromLocalizationRecords.Entities[0].Attributes["vrp_value"].ToString().Split(',');
                    if(SurveyTimeExemptedTransactionTypes.Contains(SurveyTransactionType))
                    {
                        isTimeExemptedTransactionTypes = true;
                    }
                }
            }
                if (ExcludeDate != null)
            {
                //ExcludeDaysFilter = @"<filter type='or'>
                //                              <condition attribute='vrp_dateoflastsurvey' operator='on-or-before' value='" + ExcludeDate + @"' />
                //                              <condition attribute='vrp_dateoflastsurvey' operator='null'/>
                //                            </filter>";
                if (!isTimeExemptedTransactionTypes)
                {
                    SurveyExcludeDaysFilter = @"<condition attribute='createdon' operator='on-or-after' value='" + ExcludeDate + @"' />";
                }
            }

            if (StaticListIDConvertedFromDynamicList.Count > 0)
            {
                foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + ListID.ToString() + "}</value>";
                }
            }

            foreach (var list in Survey.vrp_vrp_survey_M_list)
            {
                if (!list.Type.Value)//not dynamic 
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + list.Id.ToString() + "}</value>";
                }
            }


            MarketingListEntityLink = @"  <link-entity name='listmember' from='entityid' to='contactid' visible='false' intersect='true'>
                                                  <link-entity name='list' from='listid' to='listid' alias='aw'>
                                                    <filter type='and'>
                                                      <condition attribute='listid' operator='in'>
                                                        " + MarketingListEntityLink + @"
                                                      </condition>                                             
                                                    </filter>
                                                  </link-entity>
                                                </link-entity>";


            FilterValues = GenderFilter + RegionFilter + BranchFilter + SegmentFilter + AgeFilter + ExcludeDaysFilter;

            if (FilterValues.Trim() != "")
            {
                FilterValues = "<filter type='and'>" + FilterValues + "</filter>";
            }
            if (NumberOfRecords > 0)
            {
                RecordCountSnippet = "count='" + NumberOfRecords.ToString() + "'";
            }


            FetchXML = @"<fetch   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='contact'> 
                                    <attribute name='contactid' /> 
                                    <attribute name='vrp_dateoflastsurvey' /> 
                                  " + MarketingListEntityLink + FilterValues + " </entity> </fetch>";




            int ReuiredRecord = NumberOfRecords;
            bool bFinished = false;
            int i = 1;
            while (bFinished == false)
            {
                EntityCollection FilteredRecords = commonRepository.RetriveByFetchXML(FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'"));
                // Create Marketlist List with processed memebers              

                if (FilteredRecords.MoreRecords)
                {
                    bFinished = false;
                    i++;

                }
                else
                {
                    bFinished = true;
                    i++;
                }


                List<Guid> Results = new List<Guid>();
                foreach (Entity contact in FilteredRecords.Entities)
                {
                    if (!contact.Contains("vrp_dateoflastsurvey"))
                    {
                        if (!ExistingParticpants.Contains((Guid)contact.Attributes["contactid"]))
                        {
                            Results.Add((Guid)contact.Attributes["contactid"]);
                            ExistingParticpants.Add((Guid)contact.Attributes["contactid"]);
                            ReuiredRecord--;
                            if (ReuiredRecord == 0)
                            {
                                bFinished = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var fetchSurveySessions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='vrp_surveysession'>
                                                <attribute name='vrp_surveysessionid' />
                                                <attribute name='vrp_subject' />
                                                <attribute name='createdon' />
                                                <order attribute='vrp_subject' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='vrp_contact' operator='eq' uitype='contact' value='" + contact.Id + @"' />
                                                  " + SurveyExcludeDaysFilter + @"
                                                </filter>
                                                <link-entity name='vrp_survey' from='vrp_surveyid' to='vrp_survey' link-type='inner' alias='ai'>
                                                  <link-entity name='vrp_transactiontype' from='vrp_transactiontypeid' to='vrp_transactiontype' link-type='inner' alias='aj'>
                                                    <filter type='and'>
                                                      <condition attribute='vrp_name' operator='eq' value='" + SurveyTransactionType + @"' />
                                                    </filter>
                                                  </link-entity>
                                                </link-entity>
                                              </entity>
                                            </fetch>";

                        EntityCollection FilteredSurveySessionRecords = commonRepository.RetriveByFetchXML(fetchSurveySessions);
                        if (FilteredSurveySessionRecords.Entities == null || FilteredSurveySessionRecords.Entities.Count == 0)
                        {
                            if (!ExistingParticpants.Contains((Guid)contact.Attributes["contactid"]))
                            {
                                Results.Add((Guid)contact.Attributes["contactid"]);
                                ExistingParticpants.Add((Guid)contact.Attributes["contactid"]);
                                ReuiredRecord--;
                                if (ReuiredRecord == 0)
                                {
                                    bFinished = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (isTimeExemptedTransactionTypes)
                            {
                                if (!ExistingParticpants.Contains((Guid)contact.Attributes["contactid"]))
                                {
                                    Results.Add((Guid)contact.Attributes["contactid"]);
                                    ExistingParticpants.Add((Guid)contact.Attributes["contactid"]);
                                    ReuiredRecord--;
                                    if (ReuiredRecord == 0)
                                    {
                                        bFinished = true;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    //if (!ExistingParticpants.Contains((Guid)contact.Attributes["contactid"]))
                    //{
                    //    Results.Add((Guid)contact.Attributes["contactid"]);
                    //    ExistingParticpants.Add((Guid)contact.Attributes["contactid"]);
                    //    ReuiredRecord--;
                    //    if (ReuiredRecord == 0)
                    //    {
                    //        bFinished = true;
                    //        break;
                    //    }
                    //}
                }

                if (marketListMemberRepository.AddMembersToTheList(TargetListID, Results) == false)
                {
                    foreach (Guid contactid in Results)
                    {
                        marketListMemberRepository.AddMemberToTheList(contactid, TargetListID);
                    }
                }
            }



            if (NumberOfRecords > 0)
            {
                if (Filter != null)
                {
                    Filter.vrp_AvailableSamples = (NumberOfRecords - ReuiredRecord).ToString();
                    surveyFilterRepository.UpdateSurveyFilter(Filter);
                }
            }

        }

        private void ApplyFilterForAccounts(Domain.vrp_survey Survey, Domain.vrp_surveyfilter Filter, Int32 NumberOfRecords, DateTime ExcludeDate, Guid TargetListID)
        {

            string CorporateSegmentFilter = "";
            string BranchFilter = "";
            string ExcludeDaysFilter = "";
            string MarketingListEntityLink = "";
            string FilterValues = "";
            string RecordCountSnippet = "";
            string FetchXML = "";
            string ExceptLists = "";


            if (Filter != null)
            {
                if (Filter.vrp_CorporateSegment != null)
                    if (Filter.vrp_CorporateSegment.Id != null)
                        FilterValues = FilterValues + @"<condition attribute='vrp_corporatesegment' operator='eq'   uitype='vrp_corporatesegment' value='{" + Filter.vrp_CorporateSegment.Id.ToString() + "}' />";
                if (Filter.vrp_Branch != null)
                    if (Filter.vrp_BranchForCorporate.Id != null)
                        FilterValues = FilterValues + @"<condition attribute='vrp_branch' operator='eq'   uitype='vrp_branch' value='{" + Filter.vrp_BranchForCorporate.Id.ToString() + "}' />";
            }
            if (ExcludeDate != null)
            {
                ExcludeDaysFilter = @"<filter type='or'>
		                                    <condition attribute='vrp_dateoflastsurvey' operator='on-or-before' value='" + ExcludeDate + @"' />
		                                    <condition attribute='vrp_dateoflastsurvey' operator='null'/>
	                                   </filter>";
            }


            if (StaticListIDConvertedFromDynamicList.Count > 0)
            {
                foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + ListID.ToString() + "}</value>";
                }
            }

            foreach (var list in Survey.vrp_vrp_survey_M_list)
            {
                if (!list.Type.Value)//not dynamic 
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + list.Id.ToString() + "}</value>";
                }
            }




            MarketingListEntityLink = @"  <link-entity name='listmember' from='entityid' to='accountid' visible='false' intersect='true'>
                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
                                            <filter type='and'> 
                                              <condition attribute='listid' operator='in'>
                                                " + MarketingListEntityLink + @"
                                              </condition>
                                            </filter>
                                          </link-entity>
                                        </link-entity>";


            FilterValues = BranchFilter + CorporateSegmentFilter + ExcludeDaysFilter;

            if (FilterValues.Trim() != "")
            {
                FilterValues = "<filter type='and'>" + FilterValues + "</filter>";
            }


            if (NumberOfRecords > 0)
            {
                RecordCountSnippet = "count='" + NumberOfRecords.ToString() + "'";
            }



            FetchXML = @"<fetch   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                          <entity name='account'> 
                            <attribute name='accountid' /> 
                          " + FilterValues + MarketingListEntityLink + " </entity></fetch>";





            int ReuiredRecord = NumberOfRecords;
            bool bFinished = false;
            int i = 1;
            while (bFinished == false)
            {
                EntityCollection FilteredRecords = commonRepository.RetriveByFetchXML(FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'"));
                // Create Marketlist List with processed memebers              

                if (FilteredRecords.MoreRecords)
                {
                    bFinished = false;
                    i++;

                }
                else
                {
                    bFinished = true;
                    i++;
                }


                List<Guid> Results = new List<Guid>();
                foreach (Entity account in FilteredRecords.Entities)
                {
                    if (!ExistingParticpants.Contains((Guid)account.Attributes["accountid"]))
                    {
                        Results.Add((Guid)account.Attributes["accountid"]);
                        ExistingParticpants.Add((Guid)account.Attributes["accountid"]);
                        ReuiredRecord--;
                        if (ReuiredRecord == 0)
                        {
                            bFinished = true;
                            break;
                        }
                    }
                }

                if (marketListMemberRepository.AddMembersToTheList(TargetListID, Results) == false)
                {
                    foreach (Guid accountid in Results)
                    {
                        marketListMemberRepository.AddMemberToTheList(accountid, TargetListID);
                    }
                }
            }



            if (NumberOfRecords > 0)
            {
                if (Filter != null)
                {
                    Filter.vrp_AvailableSamples = (NumberOfRecords - ReuiredRecord).ToString();
                    surveyFilterRepository.UpdateSurveyFilter(Filter);
                }
            }

        }

        private void ApplyFilterForLeads(Domain.vrp_survey Survey, Domain.vrp_surveyfilter Filter, Int32 NumberOfRecords, DateTime ExcludeDate, Guid TargetListID)
        {

            string RegionFilter = "";
            string ExcludeDaysFilter = "";
            string MarketingListEntityLink = "";
            string FilterValues = "";
            string RecordCountSnippet = "";
            string FetchXML = "";
            string ExceptLists = "";



            if (Filter != null)
            {
                if (Filter.vrp_Region != null)
                    if (Filter.vrp_Region.Id != null)
                        RegionFilter = @"<condition attribute='vrp_region' operator='eq'   uitype='vrp_region' value='{" + Filter.vrp_Region.Id.ToString() + "}' />";
            }
            if (ExcludeDate != null)
            {
                ExcludeDaysFilter = @"<filter type='or'>
		                                    <condition attribute='vrp_dateoflastsurvey' operator='on-or-before' value='" + ExcludeDate + @"' />
		                                    <condition attribute='vrp_dateoflastsurvey' operator='null'/>
	                                   </filter>";
            }


            if (StaticListIDConvertedFromDynamicList.Count > 0)
            {
                foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + ListID.ToString() + "}</value>";
                }
            }

            foreach (var list in Survey.vrp_vrp_survey_M_list)
            {
                if (!list.Type.Value)//not dynamic 
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + list.Id.ToString() + "}</value>";
                }
            }



            MarketingListEntityLink = @"  <link-entity name='listmember' from='entityid' to='leadid' visible='false' intersect='true'>
                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
                                            <filter type='and'> 
                                              <condition attribute='listid' operator='in'>
                                                " + MarketingListEntityLink + @"
                                              </condition>
                                            </filter>
                                          </link-entity>
                                        </link-entity>";


            FilterValues = RegionFilter + ExcludeDaysFilter;

            if (FilterValues.Trim() != "")
            {
                FilterValues = "<filter type='and'>" + FilterValues + "</filter>";
            }


            if (NumberOfRecords > 0)
            {
                RecordCountSnippet = "count='" + NumberOfRecords.ToString() + "'";
            }



            FetchXML = @"<fetch  version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                          <entity name='lead'> 
                            <attribute name='leadid' /> 
                          " + FilterValues + MarketingListEntityLink + " </entity></fetch>";




            int ReuiredRecord = NumberOfRecords;
            bool bFinished = false;
            int i = 1;
            while (bFinished == false)
            {
                EntityCollection FilteredRecords = commonRepository.RetriveByFetchXML(FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'"));
                // Create Marketlist List with processed memebers              

                if (FilteredRecords.MoreRecords)
                {
                    bFinished = false;
                    i++;

                }
                else
                {
                    bFinished = true;
                    i++;
                }


                List<Guid> Results = new List<Guid>();
                foreach (Entity lead in FilteredRecords.Entities)
                {
                    if (!ExistingParticpants.Contains((Guid)lead.Attributes["leadid"]))
                    {
                        Results.Add((Guid)lead.Attributes["leadid"]);
                        ExistingParticpants.Add((Guid)lead.Attributes["leadid"]);
                        ReuiredRecord--;
                        if (ReuiredRecord == 0)
                        {
                            bFinished = true;
                            break;
                        }
                    }
                }

                if (marketListMemberRepository.AddMembersToTheList(TargetListID, Results) == false)
                {
                    foreach (Guid lead in Results)
                    {
                        marketListMemberRepository.AddMemberToTheList(lead, TargetListID);
                    }
                }
            }



            if (NumberOfRecords > 0)
            {
                if (Filter != null)
                {
                    Filter.vrp_AvailableSamples = (NumberOfRecords - ReuiredRecord).ToString();
                    surveyFilterRepository.UpdateSurveyFilter(Filter);
                }
            }
        }


        //      private Guid MergeMarketingLists(IEnumerable<Domain.List> ParticipantList, vrp_survey survey)
        //        {
        //            string FetchXML = "";
        //            Guid MergedListID = new Guid();
        //            VeriTouchTraceHandler.AddTrace("Load Started", DateTime.Now.ToString());

        //            switch (survey.vrp_SurveyFor.Value)
        //            {

        //                case 168260000:
        //                    MergedListID = marketListRepository.CreateMarketingList("SGL_DoNotUse_C" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 2, false, "");
        //                    break;
        //                case 168260001:
        //                    MergedListID = marketListRepository.CreateMarketingList("SGL_DoNotUse_A" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 1, false, "");
        //                    break;
        //                case 168260002:
        //                    MergedListID = marketListRepository.CreateMarketingList("SGL_DoNotUse_L" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 4, false, "");
        //                    break;
        //            }



        //            foreach (var list in ParticipantList)
        //            {
        //                bool bFinished = false;
        //                int i = 1;
        //                while (bFinished == false)
        //                {
        //                    VeriTouchTraceHandler.AddTrace("Load 5000 Loop", DateTime.Now.ToString());
        //                    if (list.Type.Value)
        //                    {
        //                        FetchXML = list.Query;
        //                        FetchXML = FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'");
        //                    }
        //                    else
        //                    {
        //                        if (list.CreatedFromCode == 2)
        //                            FetchXML = @"<fetch page='" + i.ToString() + @"'   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
        //                          <entity name='contact'> 
        //                            <attribute name='contactid' /> 
        //                          <link-entity name='listmember' from='entityid' to='contactid' visible='false' intersect='true'>
        //                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
        //                                            <filter type='and'>
        //                                              <condition attribute='listid' operator='in'>
        //                                               <value uiname='" + list.ListName + "' uitype='list'>{" + list.Id.ToString() + @"}</value>
        //                                              </condition>                                             
        //                                            </filter>
        //                                          </link-entity>
        //                                        </link-entity>
        //                            </entity> </fetch>";
        //                        else if (list.CreatedFromCode == 1)
        //                            FetchXML = @"<fetch page='" + i.ToString() + @"'   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
        //                          <entity name='account'> 
        //                            <attribute name='accountid' /> 
        //                          <link-entity name='listmember' from='entityid' to='accountid' visible='false' intersect='true'>
        //                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
        //                                            <filter type='and'>
        //                                              <condition attribute='listid' operator='in'>
        //                                               <value uiname='" + list.ListName + "' uitype='list'>{" + list.Id.ToString() + @"}</value>
        //                                              </condition>                                             
        //                                            </filter>
        //                                          </link-entity>
        //                                        </link-entity>
        //                            </entity> </fetch>";
        //                        else if (list.CreatedFromCode == 4)
        //                            FetchXML = @"<fetch page='" + i.ToString() + @"'   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
        //                          <entity name='lead'> 
        //                            <attribute name='leadid' /> 
        //                          <link-entity name='listmember' from='entityid' to='leadid' visible='false' intersect='true'>
        //                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
        //                                            <filter type='and'>
        //                                              <condition attribute='listid' operator='in'>
        //                                               <value uiname='" + list.ListName + "' uitype='list'>{" + list.Id.ToString() + @"}</value>
        //                                              </condition>                                             
        //                                            </filter>
        //                                          </link-entity>
        //                                        </link-entity>
        //                            </entity> </fetch>";
        //                    }


        //                    EntityCollection returnCollection = commonRepository.RetriveByFetchXML(FetchXML);
        //                    if (list.CreatedFromCode == 2)
        //                        foreach (Entity contact in returnCollection.Entities)
        //                        {
        //                            AllRecords.Add((Guid)contact.Attributes["contactid"]);
        //                        }
        //                    else if (list.CreatedFromCode == 1)
        //                        foreach (Entity contact in returnCollection.Entities)
        //                        {
        //                            AllRecords.Add((Guid)contact.Attributes["accountid"]);
        //                        }
        //                    else if (list.CreatedFromCode == 4)
        //                        foreach (Entity contact in returnCollection.Entities)
        //                        {
        //                            AllRecords.Add((Guid)contact.Attributes["leadid"]);
        //                        }


        //                    if (returnCollection.MoreRecords)
        //                    {
        //                        bFinished = false;
        //                        i++;

        //                    }
        //                    else
        //                    {
        //                        bFinished = true;
        //                        i++;
        //                    }
        //                }
        //            }




        //            marketListMemberRepository.AddMembersToTheList(MergedListID, AllRecords);
        //            VeriTouchTraceHandler.AddTrace("Load Ends", DateTime.Now.ToString());
        //            return MergedListID;
        //        }

        //public bool CreateSurveySubListForRecurrenceSurvey(Guid surveyId)
        //{
        //    try
        //    {
        //        Domain.vrp_survey survey = surveyRepository.GetSurveyById(surveyId);
        //        VeriTouchTraceHandler.AddTrace("RST.CreateSurveySubList.SurveyById", surveyId.ToString());
        //        VeriTouchTraceHandler.AddTrace("RST.CreateSurveySubList.SurveyStatus", survey.vrp_SurveyStatus.ToString());
        //        VeriTouchTraceHandler.AddTrace("RST.CreateSurveySubList.ApplyFilterClick", survey.vrp_ApplyFilterClick.ToString());



        //        AllRecords = new List<Guid>();
        //        ExistingParticpants = new List<Guid>();
        //        int TempRecordCounts = 0;





        //        surveyRepository.LoadSurveyMarketLists(survey);
        //        if (survey.vrp_vrp_survey_M_list == null) //If marketing list is not loaded.
        //            return true;

        //        surveyRepository.LoadSurveyQuestions(survey);
        //        if (survey.vrp_survey_surveyquestion == null) //If questions is not list is not loaded.
        //            return true;

        //        surveyRepository.LoadFilteredParticipantsList(survey);
        //        // if exists , delete all
        //        ExistingListIDs = new List<Guid>();
        //        if (survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey != null)
        //        {
        //            foreach (var FilteredParticipantsList in survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey)
        //            {
        //                ExistingListIDs.Add(FilteredParticipantsList.vrp_ParticipantList.Id);
        //                commonRepository.DeleteEntityRecord(FilteredParticipantsList.vrp_ParticipantList.LogicalName, FilteredParticipantsList.vrp_ParticipantList.Id);
        //                commonRepository.DeleteEntityRecord(FilteredParticipantsList.LogicalName, FilteredParticipantsList.Id);
        //            }

        //            StaticListIDConvertedFromDynamicList = new List<Guid>();
        //            foreach (var list in survey.vrp_vrp_survey_M_list)
        //            {
        //                if (list.Type.Value)//Dynamic
        //                {
        //                    StaticListIDConvertedFromDynamicList.Add(marketListRepository.CopyDynamicListToStatics(list.Id));
        //                }
        //            }
        //            //Guid MergedListID = MergeMarketingLists(survey.vrp_vrp_survey_M_list, survey);


        //            // Sample Size calculation
        //            int SampleSize = 0;
        //            if (survey.vrp_EnableSurveySampling.Value)
        //            {
        //                if (survey.vrp_SampleSizeofSurvey != null)
        //                {
        //                    SampleSize = survey.vrp_SampleSizeofSurvey.Value;
        //                }
        //                if (survey.vrp_IsOverrideSampleSize.Value == true)
        //                    SampleSize = survey.vrp_OverrideSampleSize.Value;
        //            }
        //            else
        //            {
        //                foreach (var list in survey.vrp_vrp_survey_M_list)
        //                {
        //                    if (StaticListIDConvertedFromDynamicList.Count > 0)
        //                    {
        //                        foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
        //                        {
        //                            SampleSize = SampleSize + marketListRepository.GetMemeberCountOfStaticList(ListID);
        //                        }
        //                    }


        //                    //if (list.Type.Value)//Dynamic
        //                    //{
        //                    //    SampleSize = SampleSize + getDynamicListMemeberCount(list);
        //                    //}
        //                    //else //static
        //                    //{
        //                    SampleSize = SampleSize + list.MemberCount.Value;
        //                    //}
        //                }
        //            }





        //            Guid TargetListID = new Guid();
        //            switch (survey.vrp_SurveyFor.Value)
        //            {

        //                case 168260000:
        //                    TargetListID = marketListRepository.CreateMarketingList("SGL_C_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 2, false, "");
        //                    break;
        //                case 168260001:
        //                    TargetListID = marketListRepository.CreateMarketingList("SGL_A_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 1, false, "");
        //                    break;
        //                case 168260002:
        //                    TargetListID = marketListRepository.CreateMarketingList("SGL_L_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 4, false, "");
        //                    break;
        //            }
        //            marketListRepository.CreateFilteredSurveyList(TargetListID.ToString(), TargetListID, survey.Id);



        //            // Apply survey Filter
        //            surveyRepository.LoadSurveyFilters(survey);
        //            if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
        //                foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey)
        //                {
        //                    decimal Percentage = Filter.vrp_Percentage.Value;
        //                    Int32 NumberOfRecords = Convert.ToInt32(SampleSize * Percentage / 100);
        //                    TempRecordCounts = NumberOfRecords; ;
        //                    Filter.vrp_RequiredSamples = NumberOfRecords.ToString();

        //                    if (NumberOfRecords > 0)
        //                    {
        //                        // string createdfromcode = "";
        //                        switch (survey.vrp_SurveyFor.Value)
        //                        {

        //                            case 168260000: // Contacts//       createdfromcode = "2";                                          


        //                                ApplyFilterForContacts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                                break;
        //                            case 168260001:
        //                                //     createdfromcode = "1";  


        //                                ApplyFilterForAccounts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                                break;
        //                            case 168260002:
        //                                //   createdfromcode = "4";

        //                                ApplyFilterForLeads(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                                break;
        //                            default:
        //                                // createdfromcode = "99";
        //                                break;
        //                        }


        //                    }
        //                }

        //            // Total Filtered Percentage
        //            // Load remaining memebers
        //            decimal totalFilteredPercentage = 0;
        //            decimal RequiredNumberOfRecord = 0;
        //            surveyRepository.LoadSurveyFilters(survey);
        //            if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
        //                foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey)
        //                {
        //                    totalFilteredPercentage = totalFilteredPercentage + Filter.vrp_Percentage.Value;

        //                }
        //            if (totalFilteredPercentage < 100)
        //            {
        //                if (totalFilteredPercentage == 0)
        //                {
        //                    RequiredNumberOfRecord = SampleSize;
        //                }
        //                else
        //                {

        //                    RequiredNumberOfRecord = SampleSize * Convert.ToInt32(100 - totalFilteredPercentage) / 100;
        //                }
        //            }
        //            TempRecordCounts = (int)RequiredNumberOfRecord;
        //            if (TempRecordCounts > 0)
        //            {

        //                switch (survey.vrp_SurveyFor.Value)
        //                {

        //                    case 168260000://createdfromcode = "2"; //Contacts

        //                        ApplyFilterForContacts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                        break;
        //                    case 168260001: //createdfromcode = "1"; // Accounts

        //                        ApplyFilterForAccounts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                        break;
        //                    case 168260002: //createdfromcode = "4"; // Leads

        //                        ApplyFilterForLeads(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                        break;
        //                    default:
        //                        //createdfromcode = "99";
        //                        break;
        //                }

        //            }
        //            if (StaticListIDConvertedFromDynamicList.Count > 0)
        //            {
        //                foreach (Guid ListId in StaticListIDConvertedFromDynamicList)
        //                {
        //                    commonRepository.DeleteEntityRecord("list", ListId);
        //                }
        //            }
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        string exceptionMessage = Vrp.Crm.ExceptionLibrary.ExceptionMethods.ProcessException(ex);
        //        VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList failed..", exceptionMessage + "--" + ex.ToString());
        //        throw new Exception(exceptionMessage, ex);
        //    }
        //}



        //private int getDynamicListMemeberCount(Domain.List list)
        //{
        //    string aggrigatefield = "";
        //    int totalMembers = 0;
        //    switch (list.CreatedFromCode.Value)
        //    {
        //        case 1: //Account
        //            aggrigatefield = "accountid";
        //            break;
        //        case 2: //Contact
        //            aggrigatefield = "contactid";
        //            break;
        //        case 4: //lead
        //            aggrigatefield = "leadid";
        //            break;
        //    }

        //    var countQuery = ModifyFetchXML(list.Query, aggrigatefield);
        //    var memberCountResult = commonRepository.RetriveByFetchXML(countQuery);
        //    DataCollection<Entity> dataCollection = memberCountResult.Entities;

        //    if (dataCollection != null && dataCollection.Count > 0)
        //    {
        //        foreach (Entity entityVal in dataCollection)
        //        {
        //            AliasedValue value = (entityVal.Attributes["member_count"] as AliasedValue);
        //            totalMembers = (int)value.Value;
        //        }
        //    }

        //    return totalMembers;
        //}

        //private string ModifyFetchXML(string dynamicQuery, string entityidname)
        //{
        //    var doc = new XmlDocument();
        //    doc.LoadXml(dynamicQuery);

        //    var entitynode = doc.GetElementsByTagName("entity")[0];

        //    int childCount = entitynode.ChildNodes.Count;

        //    // Remove all the attribute and order tag
        //    for (int i = 0; i <= childCount; i++)
        //    {
        //        var attributenode = entitynode.SelectSingleNode("//attribute");
        //        if (attributenode != null) entitynode.RemoveChild(attributenode);
        //        else
        //        {
        //            var ordernode = entitynode.SelectSingleNode("//order");
        //            if (ordernode != null) entitynode.RemoveChild(ordernode);
        //        }
        //    }

        //    // add a new attribute tag
        //    // <attribute name="fullname" alias="member_count" aggregate="count" />
        //    XmlElement xmlNodeCustomSettings = doc.CreateElement("attribute");

        //    xmlNodeCustomSettings.SetAttribute("name", entityidname);
        //    xmlNodeCustomSettings.SetAttribute("alias", "member_count");
        //    xmlNodeCustomSettings.SetAttribute("aggregate", "count");

        //    entitynode.AppendChild(xmlNodeCustomSettings);

        //    // Add aggregate= true attribute
        //    // <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false" aggregate="true">
        //    var root = doc.DocumentElement;
        //    if (root != null) root.SetAttribute("aggregate", "true");

        //    return doc.InnerXml;
        //}


    }

}



Before

using System;
using System.Collections.Generic;
using System.Linq;
using Veripark.Xrm.Survey.BusinessLogic.Contracts;
using Veripark.Xrm.Survey.Domain.Contracts;
using Vrp.Crm.Tracing;
using Microsoft.Xrm.Sdk;
using System.Data;
using Veripark.Xrm.Survey.Domain;
using Veripark.Xrm.Survey.SMSHelperLibrary.Helper;
using Vrp.Crm.Utility.ConfigurationManager;
using System.Data.SqlClient;
using Veripark.Xrm.Survey.SMSHelperLibrary;
namespace Veripark.Xrm.Survey.BusinessLogic
{


    public class SurveyListService : ISurveyListService
    {
        ISurveyRepository surveyRepository;
        IMarketListRepository marketListRepository;
        IMarketListMemberRepository marketListMemberRepository;
        ISurveyFilterRepository surveyFilterRepository;
        ICommonRepository commonRepository;
        ISurveyFilterService surveyFilterService;
        List<Guid> ExistingListIDs;
        List<Guid> AllRecords;
        List<Guid> ExistingParticpants;
        List<Guid> StaticListIDConvertedFromDynamicList;


        public SurveyListService(ISurveyRepository surveyRepository, IMarketListRepository marketListRepository, IMarketListMemberRepository marketListMemberRepository, ISurveyFilterRepository surveyFilterRepository, ICommonRepository commonRepository)
        {
            this.surveyRepository = surveyRepository;
            this.marketListRepository = marketListRepository;
            this.marketListMemberRepository = marketListMemberRepository;
            this.surveyFilterRepository = surveyFilterRepository;
            this.commonRepository = commonRepository;
        }






        public bool CreateSurveySubList(Guid surveyId)
        {
            try
            {
                Domain.vrp_survey survey = surveyRepository.GetSurveyById(surveyId);



                AllRecords = new List<Guid>();
                ExistingParticpants = new List<Guid>();
                // VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList Validation..", "Start");

                if (!survey.vrp_IsChildSurvey.Value)
                {
                    // VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList survey.vrp_IsChildSurvey.Value..", survey.vrp_IsChildSurvey.Value.ToString());


                    if (survey.vrp_ApplyFilterClick == "Submit" ||
                        survey.vrp_ApplyFilterClick == "Approve" ||
                        survey.vrp_ApplyFilterClick == "SendBack" ||
                        survey.vrp_ApplyFilterClick == "Distribute")
                    {
                        //     VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList survey.vrp_ApplyFilterClick..", survey.vrp_ApplyFilterClick);
                        return true;
                    }


                    if (survey.vrp_SurveyStatus.Value != 1 && survey.vrp_SurveyStatus.Value != 9) // On Draft Or Apply Filter Click 
                        return true;
                }
                //   VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList Validation..", "Ends");



                if (survey.vrp_SurveyStatus.Value == 5) // Survey Expiry 
                    return true;

                if (survey.vrp_SurveyStatus.Value == 1) // Survey Draft 
                    return true;

                int TempRecordCounts = 0;


                //cleare existing filters
                surveyRepository.LoadSurveyFilters(survey);
                List<vrp_surveyfilter> PreviousSurveyFilters = new List<vrp_surveyfilter>();

                if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
                {
                    foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey)
                    {
                        commonRepository.DeleteEntityRecord(Filter.LogicalName, Filter.Id);
                        PreviousSurveyFilters.Add(Filter);
                    }


                }

                // Autigenerate Filter Conditions
                if (survey.vrp_SurveyMultiFilter != null)
                {
                    if (survey.vrp_SurveyMultiFilter.Id != null)
                    {
                        surveyFilterService = new SurveyFilterService(surveyFilterRepository, surveyRepository);
                        surveyFilterService.AutoGenerateFilters(surveyId);
                    }
                }




                surveyRepository.LoadSurveyMarketLists(survey);
                if (survey.vrp_vrp_survey_M_list == null) //If marketing list is not loaded.
                {
                    survey.vrp_SurveyStatus.Value = 1;
                    surveyRepository.UpdateSurvey(survey);

                    return true;

                }

                surveyRepository.LoadSurveyQuestions(survey);
                if (survey.vrp_survey_surveyquestion == null) //If questions is not list is not loaded.
                {
                    survey.vrp_SurveyStatus.Value = 1;
                    surveyRepository.UpdateSurvey(survey);

                    return true;

                }



                // Auto Generate Marketing List for Survey For Types

                //if (survey.vrp_SurveyForType != null)
                //{
                //    if (survey.vrp_SurveyForType.Value != 168260000)// not general survey
                //    {
                //        //  create marketing list for if type =complain/murahaba/accountopening
                //        CreateMLSpecial(survey);
                //    }

                //}




                surveyRepository.LoadFilteredParticipantsList(survey);
                // if exists , delete all
                ExistingListIDs = new List<Guid>();
                if (survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey != null)
                {
                    foreach (var FilteredParticipantsList in survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey)
                    {
                        ExistingListIDs.Add(FilteredParticipantsList.vrp_ParticipantList.Id);
                        commonRepository.DeleteEntityRecord(FilteredParticipantsList.vrp_ParticipantList.LogicalName, FilteredParticipantsList.vrp_ParticipantList.Id);
                        commonRepository.DeleteEntityRecord(FilteredParticipantsList.LogicalName, FilteredParticipantsList.Id);
                    }
                }



                StaticListIDConvertedFromDynamicList = new List<Guid>();
                foreach (var list in survey.vrp_vrp_survey_M_list)
                {
                    if (list.Type.Value)//Dynamic
                    {
                        StaticListIDConvertedFromDynamicList.Add(marketListRepository.CopyDynamicListToStatics(list.Id));
                    }
                }
                //Guid MergedListID = MergeMarketingLists(survey.vrp_vrp_survey_M_list, survey);


                // Sample Size calculation
                int SampleSize = 0;
                if (survey.vrp_EnableSurveySampling.Value)
                {
                    if (survey.vrp_SampleSizeofSurvey != null)
                    {
                        SampleSize = survey.vrp_SampleSizeofSurvey.Value;
                    }
                    if (survey.vrp_IsOverrideSampleSize.Value == true)
                        SampleSize = survey.vrp_OverrideSampleSize.Value;
                }
                else
                {

                    if (StaticListIDConvertedFromDynamicList.Count > 0)
                    {
                        foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                        {
                            SampleSize = SampleSize + marketListRepository.GetMemeberCountOfStaticList(ListID);
                        }
                    }


                    foreach (var list in survey.vrp_vrp_survey_M_list)
                    {
                        if (!list.Type.Value)//Dynamic
                            SampleSize = SampleSize + list.MemberCount.Value;
                    }
                }





                Guid TargetListID = new Guid();
                switch (survey.vrp_SurveyFor.Value)
                {

                    case 168260000:
                        TargetListID = marketListRepository.CreateMarketingList("SGL_C_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 2, false, "");
                        break;
                    case 168260001:
                        TargetListID = marketListRepository.CreateMarketingList("SGL_A_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 1, false, "");
                        break;
                    case 168260002:
                        TargetListID = marketListRepository.CreateMarketingList("SGL_L_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 4, false, "");
                        break;
                }
                marketListRepository.CreateFilteredSurveyList(TargetListID.ToString(), TargetListID, survey.Id);



                // Apply survey Filter
                surveyRepository.LoadSurveyFilters(survey);
                if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
                {
                    if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters) != null)
                        foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters))
                        {
                            decimal Percentage = Filter.vrp_Percentage.Value;
                            Int32 NumberOfRecords = Convert.ToInt32(SampleSize * Percentage / 100);
                            TempRecordCounts = NumberOfRecords; ;
                            Filter.vrp_RequiredSamples = NumberOfRecords.ToString();

                            if (NumberOfRecords > 0)
                            {
                                // string createdfromcode = "";
                                switch (survey.vrp_SurveyFor.Value)
                                {

                                    case 168260000: // Contacts//       createdfromcode = "2";                                          


                                        ApplyFilterForContacts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                                        break;
                                    case 168260001:
                                        //     createdfromcode = "1";  


                                        ApplyFilterForAccounts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                                        break;
                                    case 168260002:
                                        //   createdfromcode = "4";

                                        ApplyFilterForLeads(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                                        break;
                                    default:
                                        // createdfromcode = "99";
                                        break;
                                }


                            }
                        }
                }
                // Total Filtered Percentage
                // Load remaining memebers
                decimal totalFilteredPercentage = 0;
                decimal RequiredNumberOfRecord = 0;
                surveyRepository.LoadSurveyFilters(survey);
                if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
                    if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters) != null)
                        foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey.Except(PreviousSurveyFilters))
                        {
                            totalFilteredPercentage = totalFilteredPercentage + Filter.vrp_Percentage.Value;

                        }
                if (totalFilteredPercentage < 100)
                {
                    if (totalFilteredPercentage == 0)
                    {
                        RequiredNumberOfRecord = SampleSize;
                    }
                    else
                    {

                        RequiredNumberOfRecord = SampleSize * Convert.ToInt32(100 - totalFilteredPercentage) / 100;
                    }
                }
                TempRecordCounts = (int)RequiredNumberOfRecord;
                if (TempRecordCounts > 0)
                {

                    switch (survey.vrp_SurveyFor.Value)
                    {

                        case 168260000://createdfromcode = "2"; //Contacts

                            ApplyFilterForContacts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                            break;
                        case 168260001: //createdfromcode = "1"; // Accounts

                            ApplyFilterForAccounts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                            break;
                        case 168260002: //createdfromcode = "4"; // Leads

                            ApplyFilterForLeads(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

                            break;
                        default:
                            //createdfromcode = "99";
                            break;
                    }

                }
                if (StaticListIDConvertedFromDynamicList.Count > 0)
                {
                    foreach (Guid ListId in StaticListIDConvertedFromDynamicList)
                    {
                        commonRepository.DeleteEntityRecord("list", ListId);
                    }
                }


                survey.vrp_SurveyStatus.Value = 1;
                surveyRepository.UpdateSurvey(survey);

                return true;

            }
            catch (Exception ex)
            {
                string exceptionMessage = Vrp.Crm.ExceptionLibrary.ExceptionMethods.ProcessException(ex);
                VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList failed..", exceptionMessage + "--" + ex.ToString());
                throw new Exception(exceptionMessage, ex);
            }
        }
        public void CreateSurveySubList(Guid surveyId, List<DataRow> contactListWithTransactionDetail, Logging log, SqlConnection externalDBConnection)
        {
            try
            {
                if (CreateSurveySubList(surveyId))
                {                   
                    log.MessageTraceLog("Setting outbound Transaction table with Survey Id - Start");
                    //After inserting customers to TML, update the Outbound Transactions
                    //with the “SurveyId”, “ProcessedDateTime” and update flag “IsAssignedToSurvey” to True.
                    DataTable dt = DatabaseHelper.createDataTable<DailyTransactions>();
                    List<DataRow> subContactListWithTransaction = new List<DataRow>();
                    List<Error> errorMessages = new List<Error>();

                    var transactionId = string.Empty;
                    var transactionDate = string.Empty;
                   
                    int NumberOfObjectsPerPage = SurveySMSLogic.getNumberOfObjectsPerPage(); 
                    int TotalRecordCount = contactListWithTransactionDetail.Count;
                    int TotalPages = Convert.ToInt32(Math.Ceiling((double)TotalRecordCount / (double)NumberOfObjectsPerPage));
                    int i = 0;
                    log.MessageTraceLog("Total members asscoiated with memberlist - " + TotalRecordCount.ToString());
                    log.MessageTraceLog("Total members asscoiated with filtered memberlist - " + ExistingParticpants.Count.ToString());
                    i = 0;
                    while (i < TotalPages || TotalPages == 0)
                    {                        
                        subContactListWithTransaction = contactListWithTransactionDetail.Skip(i * NumberOfObjectsPerPage).Take(NumberOfObjectsPerPage).ToList();

                        foreach (var contact in subContactListWithTransaction)
                        {
                            DataRow dr = dt.NewRow();
                            //Checking whether the contactList member is presented in the SurveySublist or Filtered Out                           
                            var transactionDetail = ExistingParticpants.Where(e => e.ToString().ToLower() == contact.ItemArray[0].ToString().ToLower()).FirstOrDefault();                      
                            if (transactionDetail != null && transactionDetail != Guid.Empty)
                            {
                                transactionId = contact.ItemArray[1].ToString();
                                dr["SurveyId"] = surveyId.ToString();
                                dr["TransactionId"] = transactionId;
                                dr["IsAssignedToSurvey"] = "true";
                                dr["Status"] = "SurveyAssigned";
                                dr["ErrorCode"] = "";
                                dr["ErrorMessage"] = "";
                                dt.Rows.Add(dr);
                            }
                            else
                            {
                                transactionId = contact.ItemArray[1].ToString();
                                dr["SurveyId"] = surveyId.ToString();
                                dr["TransactionId"] = transactionId;
                                //contact is added in marketing list but not added in "Filtered Survey Participant List"
                                dr["IsAssignedToSurvey"] = "true";
                                dr["Status"] = "FilteredOut";
                                dr["ErrorCode"] = "";
                                dr["ErrorMessage"] = "";
                                dt.Rows.Add(dr);
                            }

                        }
                        i = i + 1;
                        if (DatabaseHelper.setTransactionStatusWithSurveyIdInBulk(externalDBConnection, dt, errorMessages))
                            log.MessageTraceLog("Outbound Transaction table updated with status & Survey Id. Page - " + i.ToString() + "/" + TotalPages.ToString());
                        else
                            log.MessageTraceLog("Outbound Transaction table is not updated with status & Survey Id. Page - " + i.ToString() + "/" + TotalPages.ToString());
                    }
                }
                else
                {
                    throw new Exception("Survey Sublist not created.");
                }
            }
            catch (Exception ex)
            {
                string exceptionMessage = Vrp.Crm.ExceptionLibrary.ExceptionMethods.ProcessException(ex);
                VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList failed..", exceptionMessage + "--" + ex.ToString());
                throw new Exception(exceptionMessage, ex);
            }
        }

        // To Create Marketing List for special type of cases
        private void CreateMLSpecial(vrp_survey survey)
        {
            string SurveyTypeFor = "";
            string FetchXML = "";
            switch (survey.vrp_SurveyForType.Value)
            {
                case 168260001:
                    SurveyTypeFor = "RMA";//Recent Murahab Account
                    switch (survey.vrp_SurveyFor.Value)
                    {
                        case 168260000:
                            // Fetch XML Recent Retail Loan;
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_C_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 2, true, FetchXML);
                            break;
                        case 168260001:
                            // Fetch XML Recent Corporate Loan
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_A_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 1, true, FetchXML);
                            break;
                        //case 168260002:
                        //    marketListRepository.CreateMarketingList("SGL_L_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 4, true, FetchXML);
                        //    break;
                    }
                    break;
                case 168260002:
                    SurveyTypeFor = "RAO";//Recent Account Opened  
                    switch (survey.vrp_SurveyFor.Value)
                    {
                        case 168260000:
                            //Fetch XML for Recent Retail Account Opened
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_C_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 2, true, FetchXML);
                            break;
                        case 168260001:
                            //Fetch XML for Recent Corporate Account Opened
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_A_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 1, true, FetchXML);
                            break;
                        //case 168260002:
                        //    marketListRepository.CreateMarketingList("SGL_L_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 4, true, FetchXML);
                        //    break;
                    }
                    break;
                case 168260003:
                    SurveyTypeFor = "RCC"; // Recent Complaints Closed
                    switch (survey.vrp_SurveyFor.Value)
                    {
                        case 168260000:
                            //Fetch XML for Recent Complaint closed for Retail
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_C_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 2, true, FetchXML);
                            break;
                        case 168260001:
                            //Fetch XML for Recent Complaint closed for Corporate
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_A_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 1, true, FetchXML);
                            break;
                        case 168260002:
                            //Fetch XML for Recent Complaint closed for Leads
                            FetchXML = "";
                            marketListRepository.CreateMarketingList("SGL_L_" + SurveyTypeFor + "_" + (new Random()).Next().ToString(), 4, true, FetchXML);
                            break;
                    }
                    break;

            }
        }
        
        private void ApplyFilterForContacts(Domain.vrp_survey Survey, Domain.vrp_surveyfilter Filter, Int32 NumberOfRecords, DateTime ExcludeDate, Guid TargetListID)
        {

            string GenderFilter = "";
            string RegionFilter = "";
            string BranchFilter = "";
            string SegmentFilter = "";
            string AgeFilter = "";
            string ExcludeDaysFilter = "";
            string MarketingListEntityLink = "";
            string FilterValues = "";
            string RecordCountSnippet = "";
            string FetchXML = "";
            string ExceptLists = "";

            // Setup Filter
            if (Filter != null)
            {
                if (Filter.vrp_Gender != null)
                {
                    if (Filter.vrp_Gender.Value!=null)
                    {
                        string vrp_gendertype = "";

                        string gendercode = "";


                        if (Filter.vrp_Gender.Value.ToString() == "1")
                        {
                            vrp_gendertype = "1";

                            gendercode = "1";

                        }
                        else
                        {
                            vrp_gendertype = "2";

                            gendercode = "2";

                        }

                        GenderFilter = @"<filter type='or'>
                                            <condition attribute='vrp_gendertype' operator='eq' value='" + vrp_gendertype + @"' />                                       
                                            <condition attribute='gendercode' operator='eq' value='" + gendercode + @"' />
                                            
                                          </filter>";
                    }

                }
                if (Filter.vrp_Branch != null)
                    if (Filter.vrp_Branch.Id != null)
                    {
                        BranchFilter = @"<filter type='or'>
                                                <condition attribute='vrp_branch' operator='eq'  uitype='vrp_branch' value='{" + Filter.vrp_Branch.Id.ToString() + @"}' />
                                                <condition attribute='vrp_branchid' operator='eq'  uitype='vrp_branch' value='{" + Filter.vrp_Branch.Id.ToString() + @"}' />
                                            </filter>";
                    }


                if (Filter.vrp_RetailSegment != null)
                    if (Filter.vrp_RetailSegment.Id != null)
                        SegmentFilter = @"<condition attribute='vrp_customersegment' operator='eq'   uitype='vrp_customersegment' value='{" + Filter.vrp_RetailSegment.Id.ToString() + "}' />";

                if (Filter.vrp_RetailRegion != null)
                    if (Filter.vrp_RetailRegion.Id != null)
                        RegionFilter = @"<condition attribute='vrp_region' operator='eq'   uitype='territory' value='{" + Filter.vrp_RetailRegion.Id.ToString() + "}' />";


                if (Filter.vrp_AgeFrom != null && Filter.vrp_AgeTo != null)
                {
                    if (Filter.vrp_AgeFrom.HasValue && Filter.vrp_AgeTo.HasValue)
                    {
                        AgeFilter = @" <filter type='and'>
                            <condition attribute='birthdate' operator='on-or-before' value='" + DateTime.Now.AddYears(-1 * Filter.vrp_AgeFrom.Value) + @"' />
                            <condition attribute='birthdate' operator='on-or-after' value='" + DateTime.Now.AddYears(-1 * Filter.vrp_AgeTo.Value) + @"' />
                          </filter>";
                    }
                }

            }


            if (ExcludeDate != null)
            {
                ExcludeDaysFilter = @"<filter type='or'>
        		                                    <condition attribute='vrp_dateoflastsurvey' operator='on-or-before' value='" + ExcludeDate + @"' />
        		                                    <condition attribute='vrp_dateoflastsurvey' operator='null'/>
        	                                   </filter>";
            }

            if (StaticListIDConvertedFromDynamicList.Count > 0)
            {
                foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + ListID.ToString() + "}</value>";
                }
            }

            foreach (var list in Survey.vrp_vrp_survey_M_list)
            {
                if (!list.Type.Value)//not dynamic 
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + list.Id.ToString() + "}</value>";
                }
            }


            MarketingListEntityLink = @"  <link-entity name='listmember' from='entityid' to='contactid' visible='false' intersect='true'>
                                                  <link-entity name='list' from='listid' to='listid' alias='aw'>
                                                    <filter type='and'>
                                                      <condition attribute='listid' operator='in'>
                                                        " + MarketingListEntityLink + @"
                                                      </condition>                                             
                                                    </filter>
                                                  </link-entity>
                                                </link-entity>";


            FilterValues = GenderFilter + RegionFilter + BranchFilter + SegmentFilter + AgeFilter + ExcludeDaysFilter;

            if (FilterValues.Trim() != "")
            {
                FilterValues = "<filter type='and'>" + FilterValues + "</filter>";
            }
            if (NumberOfRecords > 0)
            {
                RecordCountSnippet = "count='" + NumberOfRecords.ToString() + "'";
            }


            FetchXML = @"<fetch   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='contact'> 
                                    <attribute name='contactid' /> 
                                  " + MarketingListEntityLink + FilterValues + " </entity> </fetch>";




            int ReuiredRecord = NumberOfRecords;
            bool bFinished = false;
            int i = 1;
            while (bFinished == false)
            {
                EntityCollection FilteredRecords = commonRepository.RetriveByFetchXML(FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'"));
                // Create Marketlist List with processed memebers              

                if (FilteredRecords.MoreRecords)
                {
                    bFinished = false;
                    i++;

                }
                else
                {
                    bFinished = true;
                    i++;
                }


                List<Guid> Results = new List<Guid>();
                foreach (Entity contact in FilteredRecords.Entities)
                {
                    if (!ExistingParticpants.Contains((Guid)contact.Attributes["contactid"]))
                    {
                        Results.Add((Guid)contact.Attributes["contactid"]);
                        ExistingParticpants.Add((Guid)contact.Attributes["contactid"]);
                        ReuiredRecord--;
                        if (ReuiredRecord == 0)
                        {
                            bFinished = true;
                            break;
                        }
                    }
                }

                if (marketListMemberRepository.AddMembersToTheList(TargetListID, Results) == false)
                {
                    foreach (Guid contactid in Results)
                    {
                        marketListMemberRepository.AddMemberToTheList(contactid, TargetListID);
                    }
                }
            }



            if (NumberOfRecords > 0)
            {
                if (Filter != null)
                {
                    Filter.vrp_AvailableSamples = (NumberOfRecords - ReuiredRecord).ToString();
                    surveyFilterRepository.UpdateSurveyFilter(Filter);
                }
            }

        }

        private void ApplyFilterForAccounts(Domain.vrp_survey Survey, Domain.vrp_surveyfilter Filter, Int32 NumberOfRecords, DateTime ExcludeDate, Guid TargetListID)
        {

            string CorporateSegmentFilter = "";
            string BranchFilter = "";
            string ExcludeDaysFilter = "";
            string MarketingListEntityLink = "";
            string FilterValues = "";
            string RecordCountSnippet = "";
            string FetchXML = "";
            string ExceptLists = "";


            if (Filter != null)
            {
                if (Filter.vrp_CorporateSegment != null)
                    if (Filter.vrp_CorporateSegment.Id != null)
                        FilterValues = FilterValues + @"<condition attribute='vrp_corporatesegment' operator='eq'   uitype='vrp_corporatesegment' value='{" + Filter.vrp_CorporateSegment.Id.ToString() + "}' />";
                if (Filter.vrp_Branch != null)
                    if (Filter.vrp_BranchForCorporate.Id != null)
                        FilterValues = FilterValues + @"<condition attribute='vrp_branch' operator='eq'   uitype='vrp_branch' value='{" + Filter.vrp_BranchForCorporate.Id.ToString() + "}' />";
            }
            if (ExcludeDate != null)
            {
                ExcludeDaysFilter = @"<filter type='or'>
		                                    <condition attribute='vrp_dateoflastsurvey' operator='on-or-before' value='" + ExcludeDate + @"' />
		                                    <condition attribute='vrp_dateoflastsurvey' operator='null'/>
	                                   </filter>";
            }


            if (StaticListIDConvertedFromDynamicList.Count > 0)
            {
                foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + ListID.ToString() + "}</value>";
                }
            }

            foreach (var list in Survey.vrp_vrp_survey_M_list)
            {
                if (!list.Type.Value)//not dynamic 
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + list.Id.ToString() + "}</value>";
                }
            }




            MarketingListEntityLink = @"  <link-entity name='listmember' from='entityid' to='accountid' visible='false' intersect='true'>
                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
                                            <filter type='and'> 
                                              <condition attribute='listid' operator='in'>
                                                " + MarketingListEntityLink + @"
                                              </condition>
                                            </filter>
                                          </link-entity>
                                        </link-entity>";


            FilterValues = BranchFilter + CorporateSegmentFilter + ExcludeDaysFilter;

            if (FilterValues.Trim() != "")
            {
                FilterValues = "<filter type='and'>" + FilterValues + "</filter>";
            }


            if (NumberOfRecords > 0)
            {
                RecordCountSnippet = "count='" + NumberOfRecords.ToString() + "'";
            }



            FetchXML = @"<fetch   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                          <entity name='account'> 
                            <attribute name='accountid' /> 
                          " + FilterValues + MarketingListEntityLink + " </entity></fetch>";





            int ReuiredRecord = NumberOfRecords;
            bool bFinished = false;
            int i = 1;
            while (bFinished == false)
            {
                EntityCollection FilteredRecords = commonRepository.RetriveByFetchXML(FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'"));
                // Create Marketlist List with processed memebers              

                if (FilteredRecords.MoreRecords)
                {
                    bFinished = false;
                    i++;

                }
                else
                {
                    bFinished = true;
                    i++;
                }


                List<Guid> Results = new List<Guid>();
                foreach (Entity account in FilteredRecords.Entities)
                {
                    if (!ExistingParticpants.Contains((Guid)account.Attributes["accountid"]))
                    {
                        Results.Add((Guid)account.Attributes["accountid"]);
                        ExistingParticpants.Add((Guid)account.Attributes["accountid"]);
                        ReuiredRecord--;
                        if (ReuiredRecord == 0)
                        {
                            bFinished = true;
                            break;
                        }
                    }
                }

                if (marketListMemberRepository.AddMembersToTheList(TargetListID, Results) == false)
                {
                    foreach (Guid accountid in Results)
                    {
                        marketListMemberRepository.AddMemberToTheList(accountid, TargetListID);
                    }
                }
            }



            if (NumberOfRecords > 0)
            {
                if (Filter != null)
                {
                    Filter.vrp_AvailableSamples = (NumberOfRecords - ReuiredRecord).ToString();
                    surveyFilterRepository.UpdateSurveyFilter(Filter);
                }
            }

        }

        private void ApplyFilterForLeads(Domain.vrp_survey Survey, Domain.vrp_surveyfilter Filter, Int32 NumberOfRecords, DateTime ExcludeDate, Guid TargetListID)
        {

            string RegionFilter = "";
            string ExcludeDaysFilter = "";
            string MarketingListEntityLink = "";
            string FilterValues = "";
            string RecordCountSnippet = "";
            string FetchXML = "";
            string ExceptLists = "";



            if (Filter != null)
            {
                if (Filter.vrp_Region != null)
                    if (Filter.vrp_Region.Id != null)
                        RegionFilter = @"<condition attribute='vrp_region' operator='eq'   uitype='vrp_region' value='{" + Filter.vrp_Region.Id.ToString() + "}' />";
            }
            if (ExcludeDate != null)
            {
                ExcludeDaysFilter = @"<filter type='or'>
		                                    <condition attribute='vrp_dateoflastsurvey' operator='on-or-before' value='" + ExcludeDate + @"' />
		                                    <condition attribute='vrp_dateoflastsurvey' operator='null'/>
	                                   </filter>";
            }


            if (StaticListIDConvertedFromDynamicList.Count > 0)
            {
                foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + ListID.ToString() + "}</value>";
                }
            }

            foreach (var list in Survey.vrp_vrp_survey_M_list)
            {
                if (!list.Type.Value)//not dynamic 
                {
                    MarketingListEntityLink = MarketingListEntityLink + "     <value   uitype='list'>{" + list.Id.ToString() + "}</value>";
                }
            }



            MarketingListEntityLink = @"  <link-entity name='listmember' from='entityid' to='leadid' visible='false' intersect='true'>
                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
                                            <filter type='and'> 
                                              <condition attribute='listid' operator='in'>
                                                " + MarketingListEntityLink + @"
                                              </condition>
                                            </filter>
                                          </link-entity>
                                        </link-entity>";


            FilterValues = RegionFilter + ExcludeDaysFilter;

            if (FilterValues.Trim() != "")
            {
                FilterValues = "<filter type='and'>" + FilterValues + "</filter>";
            }


            if (NumberOfRecords > 0)
            {
                RecordCountSnippet = "count='" + NumberOfRecords.ToString() + "'";
            }



            FetchXML = @"<fetch  version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                          <entity name='lead'> 
                            <attribute name='leadid' /> 
                          " + FilterValues + MarketingListEntityLink + " </entity></fetch>";




            int ReuiredRecord = NumberOfRecords;
            bool bFinished = false;
            int i = 1;
            while (bFinished == false)
            {
                EntityCollection FilteredRecords = commonRepository.RetriveByFetchXML(FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'"));
                // Create Marketlist List with processed memebers              

                if (FilteredRecords.MoreRecords)
                {
                    bFinished = false;
                    i++;

                }
                else
                {
                    bFinished = true;
                    i++;
                }


                List<Guid> Results = new List<Guid>();
                foreach (Entity lead in FilteredRecords.Entities)
                {
                    if (!ExistingParticpants.Contains((Guid)lead.Attributes["leadid"]))
                    {
                        Results.Add((Guid)lead.Attributes["leadid"]);
                        ExistingParticpants.Add((Guid)lead.Attributes["leadid"]);
                        ReuiredRecord--;
                        if (ReuiredRecord == 0)
                        {
                            bFinished = true;
                            break;
                        }
                    }
                }

                if (marketListMemberRepository.AddMembersToTheList(TargetListID, Results) == false)
                {
                    foreach (Guid lead in Results)
                    {
                        marketListMemberRepository.AddMemberToTheList(lead, TargetListID);
                    }
                }
            }



            if (NumberOfRecords > 0)
            {
                if (Filter != null)
                {
                    Filter.vrp_AvailableSamples = (NumberOfRecords - ReuiredRecord).ToString();
                    surveyFilterRepository.UpdateSurveyFilter(Filter);
                }
            }
        }


         //      private Guid MergeMarketingLists(IEnumerable<Domain.List> ParticipantList, vrp_survey survey)
        //        {
        //            string FetchXML = "";
        //            Guid MergedListID = new Guid();
        //            VeriTouchTraceHandler.AddTrace("Load Started", DateTime.Now.ToString());

        //            switch (survey.vrp_SurveyFor.Value)
        //            {

        //                case 168260000:
        //                    MergedListID = marketListRepository.CreateMarketingList("SGL_DoNotUse_C" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 2, false, "");
        //                    break;
        //                case 168260001:
        //                    MergedListID = marketListRepository.CreateMarketingList("SGL_DoNotUse_A" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 1, false, "");
        //                    break;
        //                case 168260002:
        //                    MergedListID = marketListRepository.CreateMarketingList("SGL_DoNotUse_L" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 4, false, "");
        //                    break;
        //            }



        //            foreach (var list in ParticipantList)
        //            {
        //                bool bFinished = false;
        //                int i = 1;
        //                while (bFinished == false)
        //                {
        //                    VeriTouchTraceHandler.AddTrace("Load 5000 Loop", DateTime.Now.ToString());
        //                    if (list.Type.Value)
        //                    {
        //                        FetchXML = list.Query;
        //                        FetchXML = FetchXML.Replace("<fetch", "<fetch page='" + i.ToString() + "'");
        //                    }
        //                    else
        //                    {
        //                        if (list.CreatedFromCode == 2)
        //                            FetchXML = @"<fetch page='" + i.ToString() + @"'   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
        //                          <entity name='contact'> 
        //                            <attribute name='contactid' /> 
        //                          <link-entity name='listmember' from='entityid' to='contactid' visible='false' intersect='true'>
        //                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
        //                                            <filter type='and'>
        //                                              <condition attribute='listid' operator='in'>
        //                                               <value uiname='" + list.ListName + "' uitype='list'>{" + list.Id.ToString() + @"}</value>
        //                                              </condition>                                             
        //                                            </filter>
        //                                          </link-entity>
        //                                        </link-entity>
        //                            </entity> </fetch>";
        //                        else if (list.CreatedFromCode == 1)
        //                            FetchXML = @"<fetch page='" + i.ToString() + @"'   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
        //                          <entity name='account'> 
        //                            <attribute name='accountid' /> 
        //                          <link-entity name='listmember' from='entityid' to='accountid' visible='false' intersect='true'>
        //                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
        //                                            <filter type='and'>
        //                                              <condition attribute='listid' operator='in'>
        //                                               <value uiname='" + list.ListName + "' uitype='list'>{" + list.Id.ToString() + @"}</value>
        //                                              </condition>                                             
        //                                            </filter>
        //                                          </link-entity>
        //                                        </link-entity>
        //                            </entity> </fetch>";
        //                        else if (list.CreatedFromCode == 4)
        //                            FetchXML = @"<fetch page='" + i.ToString() + @"'   version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
        //                          <entity name='lead'> 
        //                            <attribute name='leadid' /> 
        //                          <link-entity name='listmember' from='entityid' to='leadid' visible='false' intersect='true'>
        //                                          <link-entity name='list' from='listid' to='listid' alias='aw'>
        //                                            <filter type='and'>
        //                                              <condition attribute='listid' operator='in'>
        //                                               <value uiname='" + list.ListName + "' uitype='list'>{" + list.Id.ToString() + @"}</value>
        //                                              </condition>                                             
        //                                            </filter>
        //                                          </link-entity>
        //                                        </link-entity>
        //                            </entity> </fetch>";
        //                    }


        //                    EntityCollection returnCollection = commonRepository.RetriveByFetchXML(FetchXML);
        //                    if (list.CreatedFromCode == 2)
        //                        foreach (Entity contact in returnCollection.Entities)
        //                        {
        //                            AllRecords.Add((Guid)contact.Attributes["contactid"]);
        //                        }
        //                    else if (list.CreatedFromCode == 1)
        //                        foreach (Entity contact in returnCollection.Entities)
        //                        {
        //                            AllRecords.Add((Guid)contact.Attributes["accountid"]);
        //                        }
        //                    else if (list.CreatedFromCode == 4)
        //                        foreach (Entity contact in returnCollection.Entities)
        //                        {
        //                            AllRecords.Add((Guid)contact.Attributes["leadid"]);
        //                        }


        //                    if (returnCollection.MoreRecords)
        //                    {
        //                        bFinished = false;
        //                        i++;

        //                    }
        //                    else
        //                    {
        //                        bFinished = true;
        //                        i++;
        //                    }
        //                }
        //            }




        //            marketListMemberRepository.AddMembersToTheList(MergedListID, AllRecords);
        //            VeriTouchTraceHandler.AddTrace("Load Ends", DateTime.Now.ToString());
        //            return MergedListID;
        //        }

        //public bool CreateSurveySubListForRecurrenceSurvey(Guid surveyId)
        //{
        //    try
        //    {
        //        Domain.vrp_survey survey = surveyRepository.GetSurveyById(surveyId);
        //        VeriTouchTraceHandler.AddTrace("RST.CreateSurveySubList.SurveyById", surveyId.ToString());
        //        VeriTouchTraceHandler.AddTrace("RST.CreateSurveySubList.SurveyStatus", survey.vrp_SurveyStatus.ToString());
        //        VeriTouchTraceHandler.AddTrace("RST.CreateSurveySubList.ApplyFilterClick", survey.vrp_ApplyFilterClick.ToString());



        //        AllRecords = new List<Guid>();
        //        ExistingParticpants = new List<Guid>();
        //        int TempRecordCounts = 0;





        //        surveyRepository.LoadSurveyMarketLists(survey);
        //        if (survey.vrp_vrp_survey_M_list == null) //If marketing list is not loaded.
        //            return true;

        //        surveyRepository.LoadSurveyQuestions(survey);
        //        if (survey.vrp_survey_surveyquestion == null) //If questions is not list is not loaded.
        //            return true;

        //        surveyRepository.LoadFilteredParticipantsList(survey);
        //        // if exists , delete all
        //        ExistingListIDs = new List<Guid>();
        //        if (survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey != null)
        //        {
        //            foreach (var FilteredParticipantsList in survey.vrp_vrp_survey_vrp_surveylistfiltered_Survey)
        //            {
        //                ExistingListIDs.Add(FilteredParticipantsList.vrp_ParticipantList.Id);
        //                commonRepository.DeleteEntityRecord(FilteredParticipantsList.vrp_ParticipantList.LogicalName, FilteredParticipantsList.vrp_ParticipantList.Id);
        //                commonRepository.DeleteEntityRecord(FilteredParticipantsList.LogicalName, FilteredParticipantsList.Id);
        //            }

        //            StaticListIDConvertedFromDynamicList = new List<Guid>();
        //            foreach (var list in survey.vrp_vrp_survey_M_list)
        //            {
        //                if (list.Type.Value)//Dynamic
        //                {
        //                    StaticListIDConvertedFromDynamicList.Add(marketListRepository.CopyDynamicListToStatics(list.Id));
        //                }
        //            }
        //            //Guid MergedListID = MergeMarketingLists(survey.vrp_vrp_survey_M_list, survey);


        //            // Sample Size calculation
        //            int SampleSize = 0;
        //            if (survey.vrp_EnableSurveySampling.Value)
        //            {
        //                if (survey.vrp_SampleSizeofSurvey != null)
        //                {
        //                    SampleSize = survey.vrp_SampleSizeofSurvey.Value;
        //                }
        //                if (survey.vrp_IsOverrideSampleSize.Value == true)
        //                    SampleSize = survey.vrp_OverrideSampleSize.Value;
        //            }
        //            else
        //            {
        //                foreach (var list in survey.vrp_vrp_survey_M_list)
        //                {
        //                    if (StaticListIDConvertedFromDynamicList.Count > 0)
        //                    {
        //                        foreach (Guid ListID in StaticListIDConvertedFromDynamicList)
        //                        {
        //                            SampleSize = SampleSize + marketListRepository.GetMemeberCountOfStaticList(ListID);
        //                        }
        //                    }


        //                    //if (list.Type.Value)//Dynamic
        //                    //{
        //                    //    SampleSize = SampleSize + getDynamicListMemeberCount(list);
        //                    //}
        //                    //else //static
        //                    //{
        //                    SampleSize = SampleSize + list.MemberCount.Value;
        //                    //}
        //                }
        //            }





        //            Guid TargetListID = new Guid();
        //            switch (survey.vrp_SurveyFor.Value)
        //            {

        //                case 168260000:
        //                    TargetListID = marketListRepository.CreateMarketingList("SGL_C_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 2, false, "");
        //                    break;
        //                case 168260001:
        //                    TargetListID = marketListRepository.CreateMarketingList("SGL_A_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 1, false, "");
        //                    break;
        //                case 168260002:
        //                    TargetListID = marketListRepository.CreateMarketingList("SGL_L_" + survey.Id.ToString() + "_" + (new Random()).Next().ToString(), 4, false, "");
        //                    break;
        //            }
        //            marketListRepository.CreateFilteredSurveyList(TargetListID.ToString(), TargetListID, survey.Id);



        //            // Apply survey Filter
        //            surveyRepository.LoadSurveyFilters(survey);
        //            if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
        //                foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey)
        //                {
        //                    decimal Percentage = Filter.vrp_Percentage.Value;
        //                    Int32 NumberOfRecords = Convert.ToInt32(SampleSize * Percentage / 100);
        //                    TempRecordCounts = NumberOfRecords; ;
        //                    Filter.vrp_RequiredSamples = NumberOfRecords.ToString();

        //                    if (NumberOfRecords > 0)
        //                    {
        //                        // string createdfromcode = "";
        //                        switch (survey.vrp_SurveyFor.Value)
        //                        {

        //                            case 168260000: // Contacts//       createdfromcode = "2";                                          


        //                                ApplyFilterForContacts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                                break;
        //                            case 168260001:
        //                                //     createdfromcode = "1";  


        //                                ApplyFilterForAccounts(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                                break;
        //                            case 168260002:
        //                                //   createdfromcode = "4";

        //                                ApplyFilterForLeads(survey, Filter, NumberOfRecords, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                                break;
        //                            default:
        //                                // createdfromcode = "99";
        //                                break;
        //                        }


        //                    }
        //                }

        //            // Total Filtered Percentage
        //            // Load remaining memebers
        //            decimal totalFilteredPercentage = 0;
        //            decimal RequiredNumberOfRecord = 0;
        //            surveyRepository.LoadSurveyFilters(survey);
        //            if (survey.vrp_vrp_survey_vrp_surveyfilter_Survey != null)
        //                foreach (var Filter in survey.vrp_vrp_survey_vrp_surveyfilter_Survey)
        //                {
        //                    totalFilteredPercentage = totalFilteredPercentage + Filter.vrp_Percentage.Value;

        //                }
        //            if (totalFilteredPercentage < 100)
        //            {
        //                if (totalFilteredPercentage == 0)
        //                {
        //                    RequiredNumberOfRecord = SampleSize;
        //                }
        //                else
        //                {

        //                    RequiredNumberOfRecord = SampleSize * Convert.ToInt32(100 - totalFilteredPercentage) / 100;
        //                }
        //            }
        //            TempRecordCounts = (int)RequiredNumberOfRecord;
        //            if (TempRecordCounts > 0)
        //            {

        //                switch (survey.vrp_SurveyFor.Value)
        //                {

        //                    case 168260000://createdfromcode = "2"; //Contacts

        //                        ApplyFilterForContacts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                        break;
        //                    case 168260001: //createdfromcode = "1"; // Accounts

        //                        ApplyFilterForAccounts(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                        break;
        //                    case 168260002: //createdfromcode = "4"; // Leads

        //                        ApplyFilterForLeads(survey, null, TempRecordCounts, DateTime.Now.AddDays(-1 * survey.vrp_ExclusionPeriod.Value), TargetListID);

        //                        break;
        //                    default:
        //                        //createdfromcode = "99";
        //                        break;
        //                }

        //            }
        //            if (StaticListIDConvertedFromDynamicList.Count > 0)
        //            {
        //                foreach (Guid ListId in StaticListIDConvertedFromDynamicList)
        //                {
        //                    commonRepository.DeleteEntityRecord("list", ListId);
        //                }
        //            }
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        string exceptionMessage = Vrp.Crm.ExceptionLibrary.ExceptionMethods.ProcessException(ex);
        //        VeriTouchTraceHandler.AddTrace("BusinessLogic.SurveyListService.CreateSurveySubList failed..", exceptionMessage + "--" + ex.ToString());
        //        throw new Exception(exceptionMessage, ex);
        //    }
        //}



        //private int getDynamicListMemeberCount(Domain.List list)
        //{
        //    string aggrigatefield = "";
        //    int totalMembers = 0;
        //    switch (list.CreatedFromCode.Value)
        //    {
        //        case 1: //Account
        //            aggrigatefield = "accountid";
        //            break;
        //        case 2: //Contact
        //            aggrigatefield = "contactid";
        //            break;
        //        case 4: //lead
        //            aggrigatefield = "leadid";
        //            break;
        //    }

        //    var countQuery = ModifyFetchXML(list.Query, aggrigatefield);
        //    var memberCountResult = commonRepository.RetriveByFetchXML(countQuery);
        //    DataCollection<Entity> dataCollection = memberCountResult.Entities;

        //    if (dataCollection != null && dataCollection.Count > 0)
        //    {
        //        foreach (Entity entityVal in dataCollection)
        //        {
        //            AliasedValue value = (entityVal.Attributes["member_count"] as AliasedValue);
        //            totalMembers = (int)value.Value;
        //        }
        //    }

        //    return totalMembers;
        //}

        //private string ModifyFetchXML(string dynamicQuery, string entityidname)
        //{
        //    var doc = new XmlDocument();
        //    doc.LoadXml(dynamicQuery);

        //    var entitynode = doc.GetElementsByTagName("entity")[0];

        //    int childCount = entitynode.ChildNodes.Count;

        //    // Remove all the attribute and order tag
        //    for (int i = 0; i <= childCount; i++)
        //    {
        //        var attributenode = entitynode.SelectSingleNode("//attribute");
        //        if (attributenode != null) entitynode.RemoveChild(attributenode);
        //        else
        //        {
        //            var ordernode = entitynode.SelectSingleNode("//order");
        //            if (ordernode != null) entitynode.RemoveChild(ordernode);
        //        }
        //    }

        //    // add a new attribute tag
        //    // <attribute name="fullname" alias="member_count" aggregate="count" />
        //    XmlElement xmlNodeCustomSettings = doc.CreateElement("attribute");

        //    xmlNodeCustomSettings.SetAttribute("name", entityidname);
        //    xmlNodeCustomSettings.SetAttribute("alias", "member_count");
        //    xmlNodeCustomSettings.SetAttribute("aggregate", "count");

        //    entitynode.AppendChild(xmlNodeCustomSettings);

        //    // Add aggregate= true attribute
        //    // <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false" aggregate="true">
        //    var root = doc.DocumentElement;
        //    if (root != null) root.SetAttribute("aggregate", "true");

        //    return doc.InnerXml;
        //}


    }

}



