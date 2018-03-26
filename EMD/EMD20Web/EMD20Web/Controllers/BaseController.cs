using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [EMD20Web.Filters.Logging]
    [EDPHandleErrorAttribute]
    public class BaseController : Controller
    {
        internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string SESSION_MAIN_EMPLOYMENT = "SESSION_MAIN_EMPLOYMENT";
        public const string IMPERSONATION_SESSION_USER = "IMPERSONATION_SESSION_USER";
        public const string SESSION_EMDUSER = "SESSION_EMDUSER";
        /// <summary>
        /// Use this comment for all Database changes.
        /// This information is important to know, from which system the Data was changed. (could also be a workflow process or a job)
        /// </summary>
        public const string MODIFY_COMMENT = "Written from Webuserinterface";

        private string actionName;
        private string controllerName;
        public string UserName;
        public string UserFullName { get; private set; }

        public string PersonGuid;

        private string userMainEmplyoment;

        public string UserMainEmplGuid
        {
            get
            {
                if (System.Web.HttpContext.Current.Session[SESSION_MAIN_EMPLOYMENT] != null)
                {
                    this.userMainEmplyoment = System.Web.HttpContext.Current.Session[SESSION_MAIN_EMPLOYMENT].ToString();
                }


                if (this.userMainEmplyoment == null)
                {
                    EMDEmployment empl = new EmploymentHandler().GetMainEmploymentForPerson(this.PersonGuid);
                    if (empl != null)
                    {
                        this.userMainEmplyoment = empl.Guid;
                        System.Web.HttpContext.Current.Session[SESSION_MAIN_EMPLOYMENT] = this.userMainEmplyoment;
                    }
                    else
                    {
                        //TODO Redirect to special page
                        logger.Error("cannot find main employment for person : " + this.PersonGuid);
                    }
                }

                return userMainEmplyoment;
            }
        }

        public string ActionName { get { return this.actionName; } set { this.actionName = value; } }
        public string ControllerName { get { return this.controllerName; } set { this.controllerName = value; } }

        public bool IsAjaxRequest { get; set; }


        #region Caching


        public SecurityUser SecurityUser
        {
            get
            {
                return GetSecurityUserFromCache();
            }
        }

        internal static ObjectCache webCache = MemoryCache.Default;

        //Create a custom Timeout of 10 seconds
        private static CacheItemPolicy policy = new CacheItemPolicy();


        private string GetCacheKey()
        {
            if (System.Web.HttpContext.Current.Request.UrlReferrer == null)
            {
                return null;
            }

            return string.Format("{0}-{1}", this.UserName, System.Web.HttpContext.Current.Request.UrlReferrer.OriginalString);
        }

        internal void AddSecurityUserToCache(SecurityUser securityUser)
        {
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
            webCache.Add(GetCacheKey(), securityUser, policy);
        }

        internal SecurityUser GetSecurityUserFromCache()
        {

            SecurityUser securityUser = null;
            string cacheKey = GetCacheKey();
            if (cacheKey != null)
            {
                securityUser = webCache.Get(cacheKey) as SecurityUser;
            }

            if (securityUser == null && !string.IsNullOrEmpty(this.UserName))
            {
                securityUser = SecurityUser.NewSecurityUser(this.UserName);
                AddSecurityUserToCache(securityUser);
            }

            return securityUser;
        }

        internal List<string> GetCacheKeys()
        {

            List<string> cacheKeys = new List<string>();

            IEnumerable<KeyValuePair<string, object>> en = webCache.AsEnumerable();

            cacheKeys = (from x in en select x.Key).ToList();

            return cacheKeys;
        }

        internal void DeleteWebCache()
        {
            List<string> cacheKeys = GetCacheKeys();
            if (cacheKeys != null)
            {
                foreach (string cacheKey in cacheKeys)
                {
                    webCache.Remove(cacheKey);
                }
            }
        }

        #endregion

        //public string UserName { get { return this.userName; } }

        //public List<string> ListControllerDefinedPermission = new List<string>();

        private ProcessServiceClient service;
        public ProcessServiceClient Service
        {
            get
            {
                if (this.service == null)
                {
                    this.service = new ProcessServiceClient();
                }

                return this.service;
            }
        }



        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (ControllerContext != null)
            {
                if (ControllerContext.RouteData.Values["action"] != null)
                    //this.actionName = ControllerContext.RouteData.Values["action"].ToString();
                    this.actionName = requestContext.RouteData.Values["action"].ToString();
                else
                    this.actionName = String.Empty;

                if (ControllerContext.RouteData.Values["controller"] != null)
                    //this.controllerName = ControllerContext.RouteData.Values["controller"].ToString();
                    this.controllerName = requestContext.RouteData.Values["controller"].ToString();
                else
                    this.controllerName = String.Empty;
            }
        }

        private bool ImpersonateUser(HttpContext requestContext, string currentUserName)
        {
            string impersonationUser = null;

            try
            {
                if (System.Web.HttpContext.Current.Session[IMPERSONATION_SESSION_USER] != null)
                {
                    impersonationUser = System.Web.HttpContext.Current.Session[IMPERSONATION_SESSION_USER].ToString();
                }
                else
                {
                    if (requestContext.Request.HttpMethod.ToLower() != "post")
                    {
                        impersonationUser = requestContext.Request.Params["user"];
                        if (!string.IsNullOrEmpty(impersonationUser))
                        {
                            // TODO reset username
                            System.Web.HttpContext.Current.Session[SESSION_EMDUSER] = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            // it's only allowed to impersonate a user who is no admin
            //if (!string.IsNullOrEmpty(impersonationUser) && EDPAuthorizeAttribute.IsAdminConfig(currentUserName) && !EDPAuthorizeAttribute.IsAdminConfig(impersonationUser))
            SecurityUser secUser = SecurityUser.NewSecurityUser(currentUserName);
            if (!string.IsNullOrEmpty(impersonationUser) && secUser.IsAdmin && !secUser.IsAdminUser(impersonationUser))
            {
                EMDPerson person = System.Web.HttpContext.Current.Session[SESSION_EMDUSER] as EMDPerson;

                if (person == null)
                {
                    PersonManager persManager = new PersonManager();
                    person = (EMDPerson)persManager.GetPersonByUserId(impersonationUser);


                }

                if (person != null)
                {
                    System.Web.HttpContext.Current.Session[IMPERSONATION_SESSION_USER] = impersonationUser;
                    System.Web.HttpContext.Current.Session[SESSION_EMDUSER] = person;
                    InitializeCurrentPerson(person, secUser.IsAdmin);

                    logger.Info(string.Format("IMPERSONATION - Adminuser: {0} impersonates person {1}", System.Web.HttpContext.Current.User.Identity.Name.Trim(), person.UserID));

                    return true;
                }


            }

            return false;
        }

        internal void ResetImpersonation()
        {
            if (System.Web.HttpContext.Current.Session[IMPERSONATION_SESSION_USER] != null)
            {
                logger.Info(string.Format("IMPERSONATION STOP - Adminuser: {0} stops impersonating person {1}", System.Web.HttpContext.Current.User.Identity.Name.Trim(), System.Web.HttpContext.Current.Session[IMPERSONATION_SESSION_USER]));
            }

            System.Web.HttpContext.Current.Session[IMPERSONATION_SESSION_USER] = null;
            System.Web.HttpContext.Current.Session[SESSION_EMDUSER] = null;


        }


        private void InitializeCurrentPerson(EMDPerson person, bool isAdmin)
        {
            ViewData["HeaderSurname"] = person.Display_FamilyName;
            ViewData["HeaderFirstname"] = person.Display_FirstName;

            this.UserFullName = string.Format("{0} {1}", person.Display_FamilyName, person.Display_FirstName);

            //ToDo Get Role
            if (isAdmin)
            {
                ViewData["HeaderEDPRole"] = "Administrator";
            }
            else
            {
                ViewData["HeaderEDPRole"] = "User";
            }

            this.UserName = person.UserID;
            this.PersonGuid = person.Guid;
            ViewData["EMD_pers_guid"] = person.Guid;
            ViewData["HeaderUsername"] = person.UserID;

            //EMDEmployment empl = new EmploymentHandler().GetMainEmploymentForPerson(this.UserGuid);
            //if (empl != null)
            //    this.UserMainEmplGuid = empl.Guid;
            //else
            //{
            //    //TODO Redirect to special page
            //    logger.Error("cannot find main employment for person : " + this.UserGuid);
            //}
        }


        public BaseController()
        {
            InitializeCurrentUserProperties();
        }

        public void InitializeCurrentUserProperties()
        {
            // get saved EmdPerson to avoid calling database

            string uName = System.Web.HttpContext.Current.User.Identity.Name.Trim();
            if (uName.IndexOf("\\") > -1)
            {
                uName = uName.Substring(uName.IndexOf("\\") + 1);
            }
            uName = uName.Trim();

            if (!ImpersonateUser(System.Web.HttpContext.Current, uName))
            {
                this.UserName = uName;
            }

            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            if (this.UserName != String.Empty)
            {
                EMDPerson sessionEmdPerson = System.Web.HttpContext.Current.Session[SESSION_EMDUSER] as EMDPerson;

                if (sessionEmdPerson == null)
                {

                    PersonManager persManager = new PersonManager();
                    EMDPerson person = (EMDPerson)persManager.GetPersonByUserId(this.UserName);

                    if (person != null)
                    {
                        System.Web.HttpContext.Current.Session[SESSION_EMDUSER] = person;
                        InitializeCurrentPerson(person, secUser.IsAdmin);
                    }
                }
                else
                {
                    InitializeCurrentPerson(sessionEmdPerson, secUser.IsAdmin);
                }
            }

            //Not working as expected
            if (ControllerContext != null)
            {
                if (ControllerContext.RouteData.Values["action"] != null)
                    this.actionName = ControllerContext.RouteData.Values["action"].ToString();
                else
                    this.actionName = String.Empty;

                if (ControllerContext.RouteData.Values["controller"] != null)
                    this.controllerName = ControllerContext.RouteData.Values["controller"].ToString();
                else
                    this.controllerName = String.Empty;
            }
        }

        [Obsolete("Use Route with isPartialView - see workflowcontroller", false)]
        public string getLayout()
        {
            string pageLayout = String.Empty;


            if (Request.QueryString["layout"] != null && Request.QueryString["layout"].ToString().Trim() != String.Empty)
            {
                pageLayout = Request.QueryString["layout"].ToString().Trim();
            }



            return pageLayout;
        }

        internal void AddResponseCloseWindow()
        {
            if (HttpContext.Response.Headers["closeWindow"] == null)
            {
                HttpContext.Response.Headers.Add("closeWindow", "true");
            }
        }

        /// <summary>
        /// Returns a partial View with the complete HTML to render on client side
        /// </summary>
        /// <param name="partialViewName"></param>
        /// <param name="model"></param>
        /// <param name="optionalErrorMessage"></param>
        /// <returns></returns>
        public PartialViewResult GetPartialFormWithErrors(string partialViewName, object model, Exception exception, string optionalErrorMessage = null)
        {
            if (!ModelState.IsValid)
            {
                HttpContext.Response.StatusCode = 500;

                if (ModelState.Keys.Contains("error"))
                {
                    HttpContext.Response.StatusDescription = ModelState["error"].Errors.First().ErrorMessage.ToString();
                    if (exception != null)
                    {
                        HttpContext.Response.AddHeader("innerException", exception.InnerException == null ? string.Empty : exception.InnerException.ToString());
                        HttpContext.Response.AddHeader("stackTrace", exception.StackTrace == null ? string.Empty : exception.StackTrace);
                    }
                }
                else
                {
                    try
                    {
                        StringBuilder errorStringBuilder = new StringBuilder();


                        if (!string.IsNullOrEmpty(optionalErrorMessage))
                        {
                            errorStringBuilder.Append(optionalErrorMessage);
                        }
                        else
                        {
                            errorStringBuilder.Append("<div style='font-weight:bold'>The item couldn't be saved.</div><br>Following errors occured:<br>");
                        }

                        List<ModelError> modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();

                        errorStringBuilder.Append("<ul>");
                        foreach (ModelError modelError in modelErrors)
                        {
                            errorStringBuilder.Append(string.Format("<li>{0}</li>", modelError.ErrorMessage));
                        }
                        errorStringBuilder.Append("</ul>");

                        if (errorStringBuilder.Length > 512)    //HttpContext.Response.StatusDescription can only be 512 characters
                            HttpContext.Response.StatusDescription = errorStringBuilder.ToString().Substring(0, 512);
                        else
                            HttpContext.Response.StatusDescription = errorStringBuilder.ToString();
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message.ToString();
                        logger.Error(ex, ControllerContext?.HttpContext);
                    }
                }
                return PartialView(partialViewName, model);
            }
            return null;
        }

        public PartialViewResult GetNoDataFoundView(Exception ex)
        {
            return PartialView("~/Views/Shared/ErrorHandling/_ViewError.cshtml", ex);
        }

        public ActionResult GetNoPermissionView(bool isPartialView = false)
        {
            if (isPartialView)
                return PartialView("~/Views/Shared/ErrorHandling/_NoPermission.cshtml");
            else
                return View("~/Views/Shared/ErrorHandling/_NoPermission.cshtml", "_Layout");
        }

        public void PopulateEnterprises()
        {
            //Kapsch.IS.EDP.Core.EMD.DB.EMDDataContext EMD_Context = Kapsch.IS.EDP.Core.EMD.DB.EMDDB_Helper.GetEMDDBContext();
            //List<Kapsch.IS.EDP.Core.EMD.DB.Enterprise> listEnt = (from ent in EMD_Context.Enterprises where ent.E_ValidFrom < DateTime.Now && ent.E_ValidTo > DateTime.Now select ent).ToList();

            //EnterpriseHandler eh = new EnterpriseHandler();
            EnterpriseController ec = new EnterpriseController();
            List<EnterpriseModel> listEnterpriseModels = ec.getList(false);

            //List<EDP.Core.Entities.IEMDHistorizableObject> listEnterprise = eh.GetEnterpriseList(true);
            List<TextValueModel> listTextValueItems = new List<TextValueModel>();

            foreach (EnterpriseModel item in listEnterpriseModels)
            {
                listTextValueItems.Add(new TextValueModel(item.NameShort, item.Guid));
            }
            ViewData["EnterpriseNames"] = listTextValueItems;
            ViewData["EnterpriseNamesDefault"] = listTextValueItems.FirstOrDefault();
        }

        public void PopulateLocations()
        {
            LocationController lc = new LocationController();
            List<LocationModel> listLocationModels = lc.getList();

            //LocationHandler lh = new LocationHandler();
            //List<IEMDHistorizableObject> listLoc = lh.GetLocations();

            List<TextValueModel> listTextValueItems = new List<TextValueModel>();

            foreach (LocationModel item in listLocationModels)
            {
                listTextValueItems.Add(new TextValueModel(item.Name, item.Guid));
            }

            ViewData["LocationNames"] = listTextValueItems;
            ViewData["LocationNamesDefault"] = listTextValueItems.FirstOrDefault();
        }

        public void PopulateOrgUnits()
        {
            OrgUnitController ouc = new OrgUnitController();
            List<OrgUnitModel> listOrgUnitModels = ouc.GetList();

            List<TextValueModel> listTextValueItems = new List<TextValueModel>();

            foreach (OrgUnitModel item in listOrgUnitModels)
            {
                listTextValueItems.Add(new TextValueModel(item.Name, item.Guid));
            }

            ViewData["OrgUnitNames"] = listTextValueItems;
            ViewData["OrgUnitNamesDefault"] = listTextValueItems.FirstOrDefault();
        }



        public List<TextValueModel> getPersonEmploymentList()
        {

            List<TextValueModel> listTextValueItems = new List<TextValueModel>();
            using (var dbContext = new EMD_Entities())
            {
                // performance enhancement         
                var items = (from pers in dbContext.Person
                             join empl in dbContext.Employment on pers.Guid equals empl.P_Guid
                             where pers.ActiveTo > DateTime.Now && pers.ValidTo > DateTime.Now && pers.ValidFrom < DateTime.Now && pers.ActiveFrom < DateTime.Now
                        && empl.ActiveTo > DateTime.Now && empl.ValidTo > DateTime.Now && empl.ValidFrom < DateTime.Now && empl.ActiveFrom < DateTime.Now

                             select new { name = pers.FamilyName + " " + pers.FirstName + " (" + pers.UserID + ")", value = empl.Guid }).OrderBy(item => item.name).ToList();


                foreach (var item in items)
                {
                    listTextValueItems.Add(new TextValueModel(item.name, item.value));
                }


                //IQueryable<Person> persons = dbContext.Person.Where(pers => pers.ActiveFrom < DateTime.Now && pers.ActiveTo > DateTime.Now);
                //IQueryable<Employment> employments = dbContext.Employment.Where(empl => empl.ActiveFrom < DateTime.Now && empl.ActiveTo > DateTime.Now);

                //foreach (Employment empl in employments)
                //{
                //    Person pers = persons.Where(p => p.Guid == empl.P_Guid).FirstOrDefault();
                //    if (pers != null)
                //    {
                //        listTextValueItems.Add(new TextValueModel(pers.FamilyName + " " + pers.FirstName + " (" + pers.UserID + ")", empl.Guid));
                //    }
                //}
            }
            //  listTextValueItems = listTextValueItems.OrderBy(item => item.Text).ToList();
            return listTextValueItems;
        }





    }
}