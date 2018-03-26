using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Xml.Linq;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("AdvancedSearch")]
    public class AdvancedSearchController : BaseController
    {
        [Route, Route("Index")]
        public ActionResult Index()
        {
            AdvancedSearchModel model = new AdvancedSearchModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            if (!model.CanView)
                return GetNoPermissionView(false);

            return View(model);
        }

        [Route("Index2")]
        public ActionResult Index2()
        {
            AdvancedSearchModel model = new AdvancedSearchModel();
            return View(model);
        }

        [Route("DoSearch")]
        [HttpPost]
        public ActionResult DoSearch([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (pModel != null && ModelState.IsValid)
            {

            }
            DataSourceResult myresult = null;
            //myresult = countryList.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///                          StartDate              EndDate (der Suche)
        ///Entry/Firstday                                                                     Exit/LastDay
        ///                              |                     |
        ///o-----------------------------|---------------------|-----------------------------------o A
        ///                              |                     |
        ///  o--------------------------------o B
        ///                              |                     |
        ///                                           o---------------------------o C
        ///                              |                     |
        ///                                 o-------------o D
        ///   
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        [Route("SearchEmployment")]
        //[HttpPost]
        public ActionResult SearchEmployment([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!pModel.CanViewAdvancedSearchEmployment)
                return GetNoPermissionView(false);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string mySearchString = pModel.SearchString;
            DataSourceResult myresult = null;
            List<AdvancedSearchEmploymentResultModel> searchresults = new List<AdvancedSearchEmploymentResultModel>();
            if (pModel != null && ModelState.IsValid)
            {
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);

                EMD_Entities dbContext = new EMD_Entities();
                TimeSpan endOfDayTime = new TimeSpan(23, 59, 59);
                DateTime startDate = DateTimeHelper.Iso8601ToDateTime(pModel.StartDate);
                DateTime endDate = (DateTimeHelper.Iso8601ToDateTime(pModel.EndDate)).Add(endOfDayTime);
                //endDate = endDate.Add(endOfDayTime);

                //                           StartDate              EndDate (der Suche)
                //Entry/Firstday                                                                     Exit/LastDay
                //                              |                     |
                //o-----------------------------|---------------------|-----------------------------------o A
                //                              |                     |
                //  o--------------------------------o B
                //                              |                     |
                //                                           o---------------------------o C
                //                              |                     |
                //                                 o-------------o D

                //var items = (from emp in dbContext.Employment
                //             join per in dbContext.Person on emp.P_Guid equals per.Guid
                //             join enlo in dbContext.EnterpriseLocation on emp.ENLO_Guid equals enlo.Guid
                //             where
                //            //enlo.E_Guid.Contains(allowedEnterprises.gui) &&
                //            //allowedEnterprises.Contains(enlo.E_Guid) &&
                //            //(((emp.Entry <= startDate && emp.Exit >= endDate) ||                                               //A siehe Zeitfenster oben (1x für Exit Date, 1x für LastDay)
                //            //(emp.Entry <= startDate && emp.Exit >= startDate && emp.Exit <= endDate) ||                       //B
                //            //(emp.Entry >= startDate && emp.Entry <= endDate && emp.Exit >= endDate) ||                         //C
                //            //(emp.Entry >= startDate && emp.Entry <= endDate && emp.Exit >= startDate && emp.Exit <= endDate)   //D
                //            //) ||
                //            //((emp.Entry <= startDate && emp.LastDay >= endDate) ||
                //            //(emp.Entry <= startDate && emp.LastDay >= startDate && emp.LastDay <= endDate) ||
                //            //(emp.Entry >= startDate && emp.Entry <= endDate && emp.LastDay >= endDate) ||
                //            //(emp.Entry >= startDate && emp.Entry <= endDate && emp.LastDay >= startDate && emp.LastDay <= endDate)
                //            //)
                //            //)
                //            ((emp.ActiveFrom <= startDate && emp.ActiveTo >= endDate) ||                                                           //A siehe Zeitfenster
                //            (emp.ActiveFrom <= startDate && emp.ActiveTo >= startDate && emp.ActiveTo <= endDate) ||                               //B
                //            (emp.ActiveFrom >= startDate && emp.ActiveFrom <= endDate && emp.ActiveTo >= endDate) ||                               //C
                //            (emp.ActiveFrom >= startDate && emp.ActiveFrom <= endDate && emp.ActiveTo >= startDate && emp.ActiveTo <= endDate))    //D
                //            &&
                //            (per.FamilyName.Contains(mySearchString) || per.FirstName.Contains(mySearchString) || per.UserID.Contains(mySearchString) || emp.PersNr.Contains(mySearchString)) &&
                //            (emp.ValidFrom <= DateTime.Now && emp.ValidTo >= DateTime.Now)
                //            select new { per, emp });

                var items = (from emp in dbContext.Employment
                             join per in dbContext.Person on emp.P_Guid equals per.Guid
                             join enlo in dbContext.EnterpriseLocation on emp.ENLO_Guid equals enlo.Guid
                             where
                                pModel.SearchStringIsGuid == false ?
                                (per.FamilyName.Contains(mySearchString) || per.FirstName.Contains(mySearchString) || per.UserID.Contains(mySearchString) || emp.PersNr.Contains(mySearchString))
                                && (emp.ValidFrom <= DateTime.Now && emp.ValidTo >= DateTime.Now)
                                : (emp.ValidFrom <= DateTime.Now && emp.ValidTo >= DateTime.Now) && emp.Guid == pModel.SearchString
                             select new { per, emp, enlo });

                if (pModel.CanViewAdvancedSearchEmploymentHistorical)
                {
                    List<string> allowedEnterpriseGuids = (from ente in secUser.AllowedEnterprises(SecurityPermission.AdvancedSearch_View_Employment_ViewDetail_Historical) select ente.Guid).ToList();

                    //items = items.Where(item => (((item.emp.ActiveFrom <= startDate && item.emp.ActiveTo >= endDate) || item.emp.ActiveFrom <= startDate && item.emp.ActiveTo >= startDate && item.emp.ActiveTo <= endDate) || (item.emp.ActiveFrom >= startDate && item.emp.ActiveFrom <= endDate && item.emp.ActiveTo >= endDate) || (item.emp.ActiveFrom >= startDate && item.emp.ActiveFrom <= endDate && item.emp.ActiveTo >= startDate && item.emp.ActiveTo <= endDate)) && allowedEnterpriseGuids.Contains(item.enlo.E_Guid));
                    //items = items.Where(item => ((
                    //                            ((item.emp.ActiveFrom <= startDate && item.emp.ActiveTo >= endDate) //A
                    //                            || item.emp.ActiveFrom <= startDate && item.emp.ActiveTo >= startDate && item.emp.ActiveTo <= endDate)  //B
                    //                            || (item.emp.ActiveFrom >= startDate && item.emp.ActiveFrom <= endDate && item.emp.ActiveTo >= endDate) //D
                    //                            || (item.emp.ActiveFrom >= startDate && item.emp.ActiveFrom <= endDate && item.emp.ActiveTo >= startDate && item.emp.ActiveTo <= endDate) //C
                    //                            )
                    //                            && allowedEnterpriseGuids.Contains(item.enlo.E_Guid))
                    //                            );

                    //|| item.emp.ActiveFrom >= startDate && item.emp.ActiveTo >= DateTime.Now);
                    //|| item.emp.ActiveFrom <= DateTime.Now && item.emp.ActiveTo >= DateTime.Now);

                    items = items.Where(item => ((
                                                (((item.emp.Entry <= startDate || item.emp.FirstWorkDay <= startDate) && (item.emp.Exit >= endDate || item.emp.LastDay >= endDate))
                                                || (item.emp.Entry <= startDate || item.emp.FirstWorkDay <= startDate) && (item.emp.Exit >= startDate || item.emp.LastDay >= startDate) && (item.emp.Exit <= endDate || item.emp.LastDay <= endDate))
                                                || ((item.emp.Entry >= startDate || item.emp.FirstWorkDay >= startDate) && (item.emp.Entry <= endDate || item.emp.FirstWorkDay <= endDate) && (item.emp.Exit >= endDate || item.emp.LastDay >= endDate))
                                                || ((item.emp.Entry >= startDate || item.emp.FirstWorkDay >= startDate) && (item.emp.Entry <= endDate || item.emp.FirstWorkDay <= endDate) && (item.emp.Exit >= startDate || item.emp.LastDay >= startDate) && (item.emp.Exit <= endDate || item.emp.LastDay <= endDate))
                                                )
                                                //&& allowedEnterpriseGuids.Contains(item.enlo.E_Guid)));
                                                ));
                    //|| (item.emp.Entry <= DateTime.Now || item.emp.FirstWorkDay <= DateTime.Now) && (item.emp.Exit >= DateTime.Now || item.emp.LastDay >= DateTime.Now));
                }
                else
                {
                    //items = items.Where(item => item.emp.ActiveFrom <= DateTime.Now && item.emp.ActiveTo >= DateTime.Now);
                    items = items.Where(item => (item.emp.Entry <= DateTime.Now || item.emp.FirstWorkDay <= DateTime.Now) && (item.emp.Exit >= DateTime.Now || item.emp.LastDay >= DateTime.Now));
                }

                foreach (var item in items)
                {
                    AdvancedSearchEmploymentResultModel mappedModel = new AdvancedSearchEmploymentResultModel();
                    mappedModel.EMPL_Guid = item.emp.Guid;
                    mappedModel.PERS_Guid = item.per.Guid;
                    mappedModel.FamilyName = item.per.FamilyName;
                    mappedModel.FirstName = item.per.FirstName;
                    mappedModel.PersNr = item.emp.PersNr;
                    mappedModel.UserID = item.per.UserID;
                    mappedModel.EP_ID = item.emp.EP_ID;
                    mappedModel.P_ID = item.per.P_ID;
                    mappedModel.Entry = item.emp.Entry;
                    mappedModel.FirstWorkDay = item.emp.FirstWorkDay;
                    mappedModel.LastDay = item.emp.LastDay;
                    mappedModel.Exit = item.emp.Exit;
                    searchresults.Add(mappedModel);
                }
            }

            myresult = searchresults.ToDataSourceResult(request, ModelState);
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [Route("SearchEnterprise")]
        public ActionResult SearchEnterprise([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string mySearchString = pModel.SearchString;
            List<AdvancedSearchEnterpriseResultModel> searchresults = new List<AdvancedSearchEnterpriseResultModel>();

            if (!pModel.CanViewAdvancedSearchEnterprise)
                return Json(searchresults, JsonRequestBehavior.AllowGet);

            if (pModel != null && ModelState.IsValid)
            {
                TimeSpan endOfDayTime = new TimeSpan(23, 59, 59);
                DateTime startDate = DateTimeHelper.Iso8601ToDateTime(pModel.StartDate);
                DateTime endDate = DateTimeHelper.Iso8601ToDateTime(pModel.EndDate).Add(endOfDayTime);

                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                List<string> allowedEnterpriseGuids = (from item in secUser.AllowedEnterprises(SecurityPermission.AdvancedSearch_View_Enterprise_ViewDetail_Historical) select item.Guid).ToList();
                EnterpriseHandler eh = new EnterpriseHandler();

                List<IEMDObject<EMDEnterprise>> enterpriseItems = null;

                if (pModel.CanViewAdvancedSearchEnterpriseHistorical)
                {
                    eh.DeliverInActive = true;
                    if (pModel.SearchStringIsGuid)
                    {
                        enterpriseItems = new List<IEMDObject<EMDEnterprise>>();
                        EMDEnterprise FoundEnterprise = (EMDEnterprise)eh.GetObject<EMDEnterprise>(mySearchString);
                        if (
                            (FoundEnterprise.ActiveFrom <= startDate && FoundEnterprise.ActiveTo >= endDate)
                            || (FoundEnterprise.ActiveFrom <= startDate && FoundEnterprise.ActiveTo >= startDate && FoundEnterprise.ActiveTo <= endDate)
                            || (FoundEnterprise.ActiveFrom >= startDate && FoundEnterprise.ActiveFrom <= endDate && FoundEnterprise.ActiveTo >= endDate)
                            || (FoundEnterprise.ActiveFrom >= startDate && FoundEnterprise.ActiveFrom <= endDate && FoundEnterprise.ActiveTo >= startDate && FoundEnterprise.ActiveTo <= endDate)
                            )
                        {
                            enterpriseItems.Add(FoundEnterprise);
                        }
                    }
                    else
                    {
                        endDate = endDate.AddDays(1);
                        string searchQueryAdd = null;
                        if (mySearchString.All(char.IsDigit))
                        {
                            searchQueryAdd = "&& (NameShort.Contains(\"" + mySearchString + "\") || NameLong.Contains(\"" + mySearchString + "\") || E_ID_new == " + mySearchString + ")";
                        }
                        else
                        {
                            searchQueryAdd = "&& (NameShort.Contains(\"" + mySearchString + "\") || NameLong.Contains(\"" + mySearchString + "\"))";
                        }



                        enterpriseItems = eh.GetObjects<EMDEnterprise, Enterprise>(
                          "((ActiveFrom <=  " + eh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo >=  " + eh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") || "
                        + "(ActiveFrom <=  " + eh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo >=  " + eh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo <= " + eh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") ||"
                        + "(ActiveFrom >=  " + eh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveFrom <=  " + eh.GenerateDateForWhereClauseInDynamicLinq(endDate) + " && ActiveTo >= " + eh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") || "
                        + "(ActiveFrom >=  " + eh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveFrom <=  " + eh.GenerateDateForWhereClauseInDynamicLinq(endDate) + " && ActiveTo >= " + eh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo <= " + eh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ")) "
                        + searchQueryAdd
                        );
                    }

                }
                else
                {
                    eh.DeliverInActive = false;
                    if (pModel.SearchStringIsGuid)
                    {
                        enterpriseItems = new List<IEMDObject<EMDEnterprise>>();
                        EMDEnterprise FoundEnterprise = (EMDEnterprise)eh.GetObject<EMDEnterprise>(mySearchString);
                        enterpriseItems.Add(FoundEnterprise);
                    }
                    else
                    {
                        enterpriseItems = eh.GetObjects<EMDEnterprise, Enterprise>(
                        "(NameShort.Contains(\"" + mySearchString + "\") || NameLong.Contains(\"" + mySearchString + "\"))"
                        );
                    }

                }

                foreach (EMDEnterprise item in enterpriseItems)
                {
                    AdvancedSearchEnterpriseResultModel mappedModel = new AdvancedSearchEnterpriseResultModel();
                    mappedModel.Guid = item.Guid;
                    mappedModel.NameShort = item.NameShort;
                    mappedModel.NameLong = item.NameLong;
                    mappedModel.E_ID = item.E_ID;
                    mappedModel.E_ID_new = item.E_ID_new.HasValue ? item.E_ID_new.Value : 0;
                    mappedModel.O_ID_Dis = item.O_ID_Dis;
                    mappedModel.E_ID_Parent = item.E_ID_Parent;
                    mappedModel.E_ID_Root = item.E_ID_Root;
                    mappedModel.ActiveFrom = item.ActiveFrom;
                    mappedModel.ActiveTo = item.ActiveTo;

                    if (allowedEnterpriseGuids.Where(ent => ent == item.Guid).Count() == 0)
                    {
                        mappedModel.CanManage = false;
                    }
                    else
                    {
                        if (pModel.CanManageEnterprise && item.IsActive)
                            mappedModel.CanManage = pModel.CanManageEnterprise;
                        else
                            mappedModel.CanManage = false;
                    }



                    mappedModel.CanView = pModel.CanViewEnterprise;

                    searchresults.Add(mappedModel);
                }
            }

            return Json(searchresults, JsonRequestBehavior.AllowGet);
        }

        [Route("SearchLocation")]
        public ActionResult SearchLocation([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));


            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string mySearchString = pModel.SearchString;
            List<AdvancedSearchLocationResultModel> searchresults = new List<AdvancedSearchLocationResultModel>();
            if (!pModel.CanViewAdvancedSearchLocation)
                return Json(searchresults, JsonRequestBehavior.AllowGet);

            if (pModel != null && ModelState.IsValid)
            {
                EMD_Entities dbContext = new EMD_Entities();
                TimeSpan endOfDayTime = new TimeSpan(23, 59, 59);
                DateTime startDate = DateTimeHelper.Iso8601ToDateTime(pModel.StartDate);
                DateTime endDate = DateTimeHelper.Iso8601ToDateTime(pModel.EndDate).Add(endOfDayTime);

                LocationHandler lh = new LocationHandler();
                lh.DeliverInActive = true;

                List<IEMDObject<EMDLocation>> locationItems = null;

                if (pModel.CanViewAdvancedSearchLocationHistorical)
                {
                    lh.DeliverInActive = true;
                    if (pModel.SearchStringIsGuid)
                    {
                        locationItems = new List<IEMDObject<EMDLocation>>();
                        EMDLocation FoundLocation = (EMDLocation)lh.GetObject<EMDLocation>(mySearchString);
                        if (
                            (FoundLocation.ActiveFrom <= startDate && FoundLocation.ActiveTo >= endDate)
                            || (FoundLocation.ActiveFrom <= startDate && FoundLocation.ActiveTo >= startDate && FoundLocation.ActiveTo <= endDate)
                            || (FoundLocation.ActiveFrom >= startDate && FoundLocation.ActiveFrom <= endDate && FoundLocation.ActiveTo >= endDate)
                            || (FoundLocation.ActiveFrom >= startDate && FoundLocation.ActiveFrom <= endDate && FoundLocation.ActiveTo >= startDate && FoundLocation.ActiveTo <= endDate)
                            )
                        {
                            locationItems.Add(FoundLocation);
                        }
                    }
                    else
                    {
                        endDate = endDate.AddDays(1);
                        locationItems = lh.GetObjects<EMDLocation, Location>(
                          "((ActiveFrom <=  " + lh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo >=  " + lh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") || "
                        + "(ActiveFrom <=  " + lh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo >=  " + lh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo <= " + lh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") ||"
                        + "(ActiveFrom >=  " + lh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveFrom <=  " + lh.GenerateDateForWhereClauseInDynamicLinq(endDate) + " && ActiveTo >= " + lh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") || "
                        + "(ActiveFrom >=  " + lh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveFrom <=  " + lh.GenerateDateForWhereClauseInDynamicLinq(endDate) + " && ActiveTo >= " + lh.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo <= " + lh.GenerateDateForWhereClauseInDynamicLinq(endDate) + ")) "
                        + "&& (Name.Contains(\"" + mySearchString + "\") || City.Contains(\"" + mySearchString + "\") || Region.Contains(\"" + mySearchString + "\") || Street.Contains(\"" + mySearchString + "\") || ZipCode.Contains(\"" + mySearchString + "\") || EL_ID = " + mySearchString + ")"
                        );
                    }

                }
                else
                {
                    lh.DeliverInActive = false;
                    if (pModel.SearchStringIsGuid)
                    {
                        locationItems = new List<IEMDObject<EMDLocation>>();
                        EMDLocation FoundAccount = (EMDLocation)lh.GetObject<EMDLocation>(mySearchString);
                        locationItems.Add(FoundAccount);
                    }
                    else
                    {
                        locationItems = lh.GetObjects<EMDLocation, Location>(
                        "(Name.Contains(\"" + mySearchString + "\") || City.Contains(\"" + mySearchString + "\") || Region.Contains(\"" + mySearchString + "\") || Street.Contains(\"" + mySearchString + "\") || ZipCode.Contains(\"" + mySearchString + "\") )");
                    }

                }

                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                List<string> allowedLocationGuids = (from item in secUser.AllowedLocations(SecurityPermission.AdvancedSearch_View_Location_ViewDetail_Historical) select item.Guid).ToList();

                foreach (EMDLocation item in locationItems)
                {
                    AdvancedSearchLocationResultModel mappedModel = new AdvancedSearchLocationResultModel();
                    mappedModel.Guid = item.Guid;
                    mappedModel.Name = item.Name;
                    mappedModel.EL_ID = item.EL_ID;
                    mappedModel.Region = item.Region;
                    mappedModel.Street = item.Street;
                    mappedModel.ZipCode = item.ZipCode;
                    mappedModel.City = item.City;
                    mappedModel.ActiveFrom = item.ActiveFrom;
                    mappedModel.ActiveTo = item.ActiveTo;
                    if (allowedLocationGuids.Where(loc => loc == item.Guid).Count() == 0)
                    {
                        mappedModel.CanManage = false;
                    }
                    else
                    {
                        if (pModel.CanManageLocation && item.IsActive)
                            mappedModel.CanManage = pModel.CanManageLocation;
                        else
                            mappedModel.CanManage = false;
                    }

                    mappedModel.CanView = pModel.CanViewLocation;
                    searchresults.Add(mappedModel);
                }
            }

            return Json(searchresults, JsonRequestBehavior.AllowGet);
        }

        [Route("SearchAccount")]
        public ActionResult SearchAccount([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string mySearchString = pModel.SearchString;
            List<AdvancedSearchAccountResultModel> searchresults = new List<AdvancedSearchAccountResultModel>();
            if (!pModel.CanViewAdvancedSearchCostcenter)
                return Json(searchresults, JsonRequestBehavior.AllowGet);

            if (pModel != null && ModelState.IsValid)
            {
                EMD_Entities dbContext = new EMD_Entities();
                TimeSpan endOfDayTime = new TimeSpan(23, 59, 59);
                DateTime startDate = DateTimeHelper.Iso8601ToDateTime(pModel.StartDate);
                DateTime endDate = DateTimeHelper.Iso8601ToDateTime(pModel.EndDate).Add(endOfDayTime);

                AccountHandler ah = new AccountHandler();
                AccountManager am = new AccountManager();

                List<EMDAccountEnhanced> accountItemsEnhanced = null;
                string whereClause = string.Empty;
                if (pModel.CanViewAdvancedSearchCostcenterHistorical)
                {
                    am.DeliverInActive = true;
                    if (pModel.SearchStringIsGuid)
                    {
                        accountItemsEnhanced = new List<EMDAccountEnhanced>();
                        List<EMDAccountEnhanced> accountItemsEnhancedList = am.GetAccountsEnhancedList(String.Empty, mySearchString);
                        if (accountItemsEnhancedList.Count > 0)
                        {
                            EMDAccountEnhanced FoundAccount = accountItemsEnhancedList.FirstOrDefault();
                            if (FoundAccount != null)
                            {
                                if (
                                    (FoundAccount.ActiveFrom <= startDate && FoundAccount.ActiveTo >= endDate)
                                    || (FoundAccount.ActiveFrom <= startDate && FoundAccount.ActiveTo >= startDate && FoundAccount.ActiveTo <= endDate)
                                    || (FoundAccount.ActiveFrom >= startDate && FoundAccount.ActiveFrom <= endDate && FoundAccount.ActiveTo >= endDate)
                                    || (FoundAccount.ActiveFrom >= startDate && FoundAccount.ActiveFrom <= endDate && FoundAccount.ActiveTo >= startDate && FoundAccount.ActiveTo <= endDate)
                                    )
                                {
                                    accountItemsEnhanced.Add(FoundAccount);
                                }
                            }
                        }
                    }
                    else
                    {
                        endDate = endDate.AddDays(1);

                        whereClause = "((ActiveFrom <=  " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo >=  " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") || "
                        + "(ActiveFrom <=  " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo >=  " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo <= " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") ||"
                        + "(ActiveFrom >=  " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveFrom <=  " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + " && ActiveTo >= " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + ") || "
                        + "(ActiveFrom >=  " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveFrom <=  " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + " && ActiveTo >= " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && ActiveTo <= " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + ")) "
                        + "&& (Name.Contains(\"" + mySearchString + "\") || KstID.Contains(\"" + mySearchString + "\"))";

                        accountItemsEnhanced = am.GetAccountsEnhancedList(whereClause);
                    }
                }
                else
                {
                    am.DeliverInActive = false;
                    if (pModel.SearchStringIsGuid)
                    {
                        accountItemsEnhanced = am.GetAccountsEnhancedList(String.Empty, mySearchString);
                    }
                    else
                    {
                        whereClause = "(Name.Contains(\"" + mySearchString + "\") || KstID.Contains(\"" + mySearchString + "\"))";
                        accountItemsEnhanced = am.GetAccountsEnhancedList(whereClause);
                    }
                }

                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                List<string> allowedCostcenterGuids = (from item in secUser.AllowedCostCenters(SecurityPermission.AdvancedSearch_View_CostCenter_ViewDetail_Historical) select item.Guid).ToList();

                foreach (EMDAccountEnhanced item in accountItemsEnhanced)
                {
                    AdvancedSearchAccountResultModel mappedModel = new AdvancedSearchAccountResultModel();
                    mappedModel.Guid = item.Guid;
                    mappedModel.Name = item.Name;
                    mappedModel.E_Guid = item.E_Guid;
                    mappedModel.KstID = item.KstID;
                    mappedModel.MainOrgUnit = item.MainOrgUnit;
                    mappedModel.Responsible = item.Responsible;
                    mappedModel.Responsible_EP_ID = item.Responsible_EP_ID;
                    mappedModel.ActiveFrom = item.ActiveFrom;
                    mappedModel.ActiveTo = item.ActiveTo;
                    mappedModel.ResponsibleName = item.ResponsibleName;
                    if (allowedCostcenterGuids.Where(acc => acc == item.Guid).Count() == 0)
                    {
                        mappedModel.CanManage = false;
                    }
                    else
                    {
                        if (pModel.CanManageCostcenter && item.IsActive)
                        {
                            mappedModel.CanManage = pModel.CanManageCostcenter;
                        }
                        else
                        {
                            mappedModel.CanManage = false;
                        }
                    }

                    mappedModel.CanView = pModel.CanViewCostcenter;
                    searchresults.Add(mappedModel);
                }
            }

            return Json(searchresults, JsonRequestBehavior.AllowGet);
        }

        [Route("SearchUser")]
        public ActionResult SearchUser([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string mySearchString = pModel.SearchString;
            string mySearchString2 = pModel.SearchString;

            string[] splitSearch = mySearchString.Split(' ');
            if (splitSearch.Length > 1)
            {
                mySearchString = splitSearch[0];
                mySearchString2 = splitSearch[1];
            }

            List<AdvancedSearchUserResultModel> searchresults = null;

            if (!pModel.CanViewAdvancedSearchUser)
                return Json(searchresults, JsonRequestBehavior.AllowGet);

            if (pModel != null && ModelState.IsValid)
            {
                EMD_Entities dbContext = new EMD_Entities();
                TimeSpan endOfDayTime = new TimeSpan(23, 59, 59);
                DateTime startDate = DateTimeHelper.Iso8601ToDateTime(pModel.StartDate);
                DateTime endDate = DateTimeHelper.Iso8601ToDateTime(pModel.EndDate).Add(endOfDayTime);

                // Search also the name in Person Table 
                List<User> items;

                if (splitSearch.Length > 1)
                {

                    //items = (from us in dbContext.User join emp in dbContext.Employment on us.EMPL_Guid equals emp.Guid join per in dbContext.Person on emp.P_Guid equals per.Guid join enlo in dbContext.EnterpriseLocation on emp.ENLO_Guid equals enlo.Guid
                    //         where
                    //            pModel.SearchStringIsGuid == false ?                                
                    //            ((us.ActiveFrom <= startDate && us.ActiveTo >= endDate) ||                                                                //A siehe Zeitfenster oben (1x für Exit Date, 1x für LastDay)
                    //            (us.ActiveFrom <= startDate && us.ActiveTo >= startDate && us.ActiveTo <= endDate) ||                               //B
                    //            (us.ActiveFrom >= startDate && us.ActiveFrom <= endDate && us.ActiveTo >= endDate) ||                               //C
                    //            (us.ActiveFrom >= startDate && us.ActiveFrom <= endDate && us.ActiveTo >= startDate && us.ActiveTo <= endDate))    //D
                    //            && (((per.FirstName == mySearchString) && (per.FamilyName == mySearchString2)) || ((per.FirstName == mySearchString2) && (per.FamilyName == mySearchString)))
                    //            :
                    //            us.Guid == mySearchString
                    //            select us).Distinct().ToList();

                    items = (from us in dbContext.User
                             join emp in dbContext.Employment on us.EMPL_Guid equals emp.Guid
                             join per in dbContext.Person on emp.P_Guid equals per.Guid
                             join enlo in dbContext.EnterpriseLocation on emp.ENLO_Guid equals enlo.Guid
                             where
                                ((us.ActiveFrom <= startDate && us.ActiveTo >= endDate) ||                                                                //A siehe Zeitfenster oben (1x für Exit Date, 1x für LastDay)
                                (us.ActiveFrom <= startDate && us.ActiveTo >= startDate && us.ActiveTo <= endDate) ||                               //B
                                (us.ActiveFrom >= startDate && us.ActiveFrom <= endDate && us.ActiveTo >= endDate) ||                               //C
                                (us.ActiveFrom >= startDate && us.ActiveFrom <= endDate && us.ActiveTo >= startDate && us.ActiveTo <= endDate))     //D
                                &&
                                pModel.SearchStringIsGuid == false ?
                                (((per.FirstName == mySearchString) && (per.FamilyName == mySearchString2)) || ((per.FirstName == mySearchString2) && (per.FamilyName == mySearchString)))
                                : us.Guid == mySearchString
                             select us).Distinct().ToList();

                    //items = (from acc in dbContext.User
                    //         join pe in dbContext.Person on acc.Guid equals pe.USER_GUID into tmpPersons
                    //         from pe in tmpPersons.DefaultIfEmpty()
                    //         where
                    //          ((acc.ActiveFrom <= startDate && acc.ActiveTo >= endDate) ||                                                                //A siehe Zeitfenster oben (1x für Exit Date, 1x für LastDay)
                    //          (acc.ActiveFrom <= startDate && acc.ActiveTo >= startDate && acc.ActiveTo <= endDate) ||                               //B
                    //          (acc.ActiveFrom >= startDate && acc.ActiveFrom <= endDate && acc.ActiveTo >= endDate) ||                               //C
                    //          (acc.ActiveFrom >= startDate && acc.ActiveFrom <= endDate && acc.ActiveTo >= startDate && acc.ActiveTo <= endDate))    //D
                    //          && (((pe.FirstName == mySearchString) && (pe.FamilyName == mySearchString2)) || ((pe.FirstName == mySearchString2) && (pe.FamilyName == mySearchString)))
                    //         select acc).Distinct().ToList();
                }
                else
                {
                    items = (from us in dbContext.User
                             join emp in dbContext.Employment on us.EMPL_Guid equals emp.Guid
                             join per in dbContext.Person on emp.P_Guid equals per.Guid
                             join enlo in dbContext.EnterpriseLocation on emp.ENLO_Guid equals enlo.Guid
                             where
                             ((us.ActiveFrom <= startDate && us.ActiveTo >= endDate) ||                                                                //A siehe Zeitfenster oben (1x für Exit Date, 1x für LastDay)
                             (us.ActiveFrom <= startDate && us.ActiveTo >= startDate && us.ActiveTo <= endDate) ||                               //B
                             (us.ActiveFrom >= startDate && us.ActiveFrom <= endDate && us.ActiveTo >= endDate) ||                               //C
                             (us.ActiveFrom >= startDate && us.ActiveFrom <= endDate && us.ActiveTo >= startDate && us.ActiveTo <= endDate))    //D
                             &&
                             pModel.SearchStringIsGuid == false ?
                             us.Username.Contains(mySearchString)
                             : us.Guid == mySearchString
                             select us).Distinct().ToList();

                    //items = (from acc in dbContext.User
                    //         join pe in dbContext.Person on acc.Guid equals pe.USER_GUID into tmpPersons
                    //         from pe in tmpPersons.DefaultIfEmpty()
                    //         where
                    //          ((acc.ActiveFrom <= startDate && acc.ActiveTo >= endDate) ||                                                                //A siehe Zeitfenster oben (1x für Exit Date, 1x für LastDay)
                    //          (acc.ActiveFrom <= startDate && acc.ActiveTo >= startDate && acc.ActiveTo <= endDate) ||                               //B
                    //          (acc.ActiveFrom >= startDate && acc.ActiveFrom <= endDate && acc.ActiveTo >= endDate) ||                               //C
                    //          (acc.ActiveFrom >= startDate && acc.ActiveFrom <= endDate && acc.ActiveTo >= startDate && acc.ActiveTo <= endDate))    //D
                    //          && acc.Username.Contains(mySearchString)
                    //         select acc).Distinct().ToList();
                }

                searchresults = AdvancedSearchUserResultModel.Map(items);
            }

            return Json(searchresults, JsonRequestBehavior.AllowGet);
        }

        [Route("SearchProcessEntity")]
        public ActionResult SearchProcessEntity([DataSourceRequest] DataSourceRequest request, AdvancedSearchModel pModel)
        {
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string mySearchString = pModel.SearchString;
            List<AdvancedProcessEntityResultModel> searchresults = new List<AdvancedProcessEntityResultModel>();
            if (!pModel.CanViewAdvancedSearchCostcenter)
                return Json(searchresults, JsonRequestBehavior.AllowGet);

            if (pModel != null && ModelState.IsValid)
            {
                EMD_Entities dbContext = new EMD_Entities();
                TimeSpan endOfDayTime = new TimeSpan(23, 59, 59);
                DateTime startDate = DateTimeHelper.Iso8601ToDateTime(pModel.StartDate);
                DateTime endDate = DateTimeHelper.Iso8601ToDateTime(pModel.EndDate).Add(endOfDayTime);

                AccountHandler ah = new AccountHandler();
                ProcessEntityManager peManager = new ProcessEntityManager();
                EmploymentManager emplManager = new EmploymentManager();

                List<EMDProcessEntity> processEntityItems = null;
                string whereClause = string.Empty;

                peManager.DeliverInActive = true;

                endDate = endDate.AddDays(1);

                whereClause = "((Created >=  " + ah.GenerateDateForWhereClauseInDynamicLinq(startDate) + " && Created <=  " + ah.GenerateDateForWhereClauseInDynamicLinq(endDate) + ")) "
                               + "&& (WFD_Name.Contains(\"" + mySearchString + "\") || WFResultMessages.Contains(\"" + mySearchString + "\") || EntityGuid.Contains(\"" + mySearchString + "\") || WFI_ID.Contains(\"" + mySearchString + "\"))";

                //    whereClause = "WFD_Name.Contains(\"" + mySearchString + "\") || WFResultMessages.Contains(\"" + mySearchString + "\") || EntityGuid.Contains(\"" + mySearchString + "\") || WFI_ID.Contains(\"" + mySearchString + "\")";

                processEntityItems = peManager.GetList(whereClause);



                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                List<string> allowedCostcenterGuids = (from item in secUser.AllowedCostCenters(SecurityPermission.AdvancedSearch_View_CostCenter_ViewDetail_Historical) select item.Guid).ToList();


                List<EMDPersonEmployment> persEmpls = emplManager.GetAllPersonEmployments();
                List<EMDPerson> persons = new PersonHandler().GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList();

                foreach (EMDProcessEntity item in processEntityItems)
                {
                    AdvancedProcessEntityResultModel mappedModel = new AdvancedProcessEntityResultModel()
                    {
                        Guid = item.Guid,
                        EntityGuid = item.EntityGuid,
                        WFI_ID = item.WFI_ID,
                        WFD_ID = item.WFD_ID,
                        WFD_Name = item.WFD_Name,
                        WFResultMessages = item.WFResultMessages,
                        RequestorEmplGuid = item.RequestorEmplGuid,
                        EffectedPersGuid = item.EffectedPersGuid,
                        WFStartTime = item.WFStartTime,
                        WFTargetDate = item.WFTargetDate,
                        ActiveFrom = item.ActiveFrom,
                        ActiveTo = item.ActiveTo,
                        Created = item.Created,
                        Modified = item.Modified
                    };

                    EMDPersonEmployment persEmplRequestor = persEmpls.Where(itemd => itemd.Empl.Guid == item.RequestorEmplGuid).FirstOrDefault();
                    if (persEmplRequestor != null)
                    {
                        mappedModel.RequestorName = EMDPerson.GetDisplayFullName(persEmplRequestor.Pers);
                    }

                    EMDPerson effectedPerson = persons.Where(itemd => itemd.Guid == item.EffectedPersGuid).FirstOrDefault();
                    if (effectedPerson != null)
                    {
                        mappedModel.EffectedPersonName = EMDPerson.GetDisplayFullName(effectedPerson);
                    }


                    switch (mappedModel.EntityGuidPrefix.ToLower())
                    {
                        case "empl":
                            EMDPersonEmployment entityPerson = persEmpls.Where(itemd => itemd.Empl.Guid == item.EntityGuid).FirstOrDefault();
                            if (entityPerson != null)
                            {
                                mappedModel.ActionLinkViewPopupTitle = "Person Profile";
                                mappedModel.ActionLinkView = string.Format("/PersonProfile/Profile/{0}/true?empl_guid={1}", entityPerson.Pers.Guid, entityPerson.Empl.Guid);
                            }
                            break;
                        case "enlo":
                            mappedModel.ActionLinkViewPopupTitle = "Enterprise-Location";
                            mappedModel.ActionLinkView = string.Format("/EnterpriseLocation/View/{0}/true", item.EntityGuid);

                            mappedModel.ActionLinkManagePopupTitle = "Enterprise Locations";
                            mappedModel.ActionLinkManage = string.Format("/EnterpriseLocation/Manage/true?Guid={0}", item.EntityGuid);
                            break;
                        case "obre":
                            EMDPerson person = new PersonManager().Get(item.EffectedPersGuid);
                            if (person != null)
                            {
                                EMDObjectRelation objectRelation = new ObjectRelationManager().Get(item.EntityGuid);
                                if (objectRelation != null)
                                {
                                    mappedModel.ActionLinkViewPopupTitle = "Person Profile";
                                    mappedModel.ActionLinkView = string.Format("/PersonProfile/Profile/{0}/true?empl_guid={1}&obre_guid={2}", person.Guid, objectRelation.Object1, item.EntityGuid);
                                }
                            }
                            break;
                        default:
                            break;
                    }


                    if (allowedCostcenterGuids.Where(acc => acc == item.Guid).Count() == 0)
                    {
                        mappedModel.CanManage = false;
                    }
                    else
                    {
                        if (pModel.CanManageCostcenter && item.IsActive)
                        {
                            mappedModel.CanManage = pModel.CanManageCostcenter;
                        }
                        else
                        {
                            mappedModel.CanManage = false;
                        }
                    }

                    mappedModel.CanView = pModel.CanViewCostcenter;
                    searchresults.Add(mappedModel);
                }
            }

            string[] requestWoinArray = searchresults.Select(o => o.WFI_ID).ToArray();
            WorkflowInstanceStatusItem[] workflowInstanceStatusItemArray = Service.GetStatusList(requestWoinArray);

            foreach (AdvancedProcessEntityResultModel currentItem in searchresults)
            {
                WorkflowInstanceStatusItem wfiStatusItem = workflowInstanceStatusItemArray.FirstOrDefault(a => a.InstanceID == currentItem.WFI_ID);
                if (wfiStatusItem != null)
                {
                    currentItem.Status = (int)wfiStatusItem.Status;
                }
            }

            return Json(searchresults, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("GetEntityPrefixFromGuid")]
        public ActionResult GetEntityPrefixFromGuid(string EntityGuid)
        {
            bool success = true;
            EntityPrefix entityPrefix = EntityPrefix.Instance;
            string prefix = entityPrefix.GetPrefixFromGuid(EntityGuid);

            return Json(new { success = success, entityprefix = prefix });
        }
    }
}