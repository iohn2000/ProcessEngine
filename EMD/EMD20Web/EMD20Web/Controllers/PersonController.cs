using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.Strings;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.ReflectionHelper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using Kapsch.IS.EDP.Core.Framework;
using System.Web;
using Kapsch.IS.EMD.EMD20Web.Core;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Person")]
    public class PersonController : BaseController
    {
        private const string MANAGEROUTE = "Person";

        // GET: Person
        [Route]
        [Route("Index")]
        [HandleError()]
        public ActionResult Index()
        {
            PersonModel model = new PersonModel();
            model.InitializeBaseSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            //  throw new NotImplementedException("Simulating an exception on a general level");

            PopulateGenders();
            return View("Index", model);
        }

        [Route("Read")]
        [HandleError()]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult myresult;
            try
            {
                PersonModel dummySecurityModel = PersonModel.Initialize(new EMDPerson());
                dummySecurityModel.InitializeBaseSecurity(SecurityUser.NewSecurityUser(this.UserName));
                //throw new Exception("Simulating an exception for reading persons as could be a Database-Connection Problem");

                PersonManager persManager = new PersonManager();
                PersonHandler handler = new PersonHandler();
                List<EMDPerson> persons = null;



                persons = handler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList();


                List<PersonModel> personModels = new List<PersonModel>();

                persons.ForEach(item =>
                {
                    PersonModel model = PersonModel.Initialize((EMDPerson)item); ;
                    model.CanManage = dummySecurityModel.CanManage;
                    model.CanView = dummySecurityModel.CanView;
                    model.CanOnboard = dummySecurityModel.CanOnboard;
                    model.IsCreatedByMe = item.Guid_ModifiedBy == this.PersonGuid;
                    personModels.Add(model);
                });

                myresult = personModels.ToDataSourceResult(request, ModelState);

                //using (var dbContext = new EMD_Entities())
                //{
                //    IQueryable<Person> persons = dbContext.Person;

                //    List<Person> personsFiltered = (from p in persons where p.ValidFrom < DateTime.Now && p.ValidTo > DateTime.Now && p.ActiveTo > DateTime.Now select p).ToList();
                //    List<PersonModel> personModels = new List<PersonModel>();
                //    // Advice: we do convert the database object into a model by reflection. if we need some fields
                //    // handled special this should be done in he Object-Specific copyFromDBObject-Method 

                //    personsFiltered.ForEach(item =>
                //    {
                //        PersonModel model = PersonModel.Initialize(item); ;
                //        model.CanManage = dummySecurityModel.CanManage;
                //        model.CanView = dummySecurityModel.CanView;
                //        personModels.Add(model);
                //    });


                //    //myresult = personsFiltered.ToDataSourceResult(request, ModelState, person =>
                //    //     PersonModel.Initialize(person));

                //    myresult = personModels.ToDataSourceResult(request, ModelState);
                //}
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading persons";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [Route("Create")]
        [Route("Create/{isPartialView}")]
        [HttpGet]
        public ActionResult Create(bool isPartialView = false)
        {
            PersonModel persModel = new PersonModel();
            persModel.InitializeBaseSecurity(SecurityUser.NewSecurityUser(this.UserName));

            if (!persModel.CanManage)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("Create", persModel);
            }
            else
            {
                return View("Create", persModel);
            }
        }

        [Route("DoCreate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoCreate([DataSourceRequest] DataSourceRequest request, PersonModel person)
        {
            Exception handledException = null;
            person.InitializeBaseSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (person != null && ModelState.IsValid)
            {
                try
                {
                    if (!person.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    PersonHandler ph = new PersonHandler(this.PersonGuid);
                    PersonManager pm = new PersonManager(this.PersonGuid);
                    EMDPerson newEMDPers = new EMDPerson();
                    //newEMDPers.C128_DegreePrefix = person.C128_DegreePrefix.Trim();
                    //newEMDPers.C128_DegreeSuffix = person.C128_DegreeSuffix.Trim();
                    newEMDPers.C128_FamilyName = person.C128_FamilyName;
                    newEMDPers.C128_FirstName = person.C128_FirstName;
                    newEMDPers.DegreePrefix = person.DegreePrefix;
                    newEMDPers.DegreeSuffix = person.DegreeSuffix;
                    newEMDPers.Display_FamilyName = person.Display_FamilyName;
                    newEMDPers.Display_FirstName = person.Display_FirstName;
                    newEMDPers.FamilyName = person.FamilyName.Trim();
                    newEMDPers.FirstName = person.FirstName.Trim();
                    newEMDPers.InsBY = ViewData["HeaderUsername"].ToString();
                    //newEMDPers.P_ID = //TODO GetMax P_ID
                    newEMDPers.Sex = person.Sex;
                    pm.Create(newEMDPers);

                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Enterprise could not be created!" + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", person, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Person has been created!" });
            }
        }

        [Route("Update/{pers_guid}")]
        [Route("Update/{pers_guid}/{isPartialView}")]
        [HttpGet]
        public ActionResult Update(string pers_guid, bool isPartialView = false)
        {
            PersonModel persModel = new PersonModel();
            try
            {
                using (var dbContext = new EMD_Entities())
                {
                    Person person = (from per in dbContext.Person where per.Guid == pers_guid select per).FirstOrDefault();
                    if (person == null)
                        throw new Exception("Could not find Person " + pers_guid);

                    persModel = PersonModel.Initialize(person);
                    ObjectFlagManager ofm = new ObjectFlagManager();
                    persModel.IsVisibleInPhonebook = ofm.IsPersonVisibleInPhonebook(pers_guid);
                    persModel.IsPictureVisible = ofm.IsPictureVisible(pers_guid);
                    persModel.IsPictureVisibleInAD = ofm.IsPictureVisibleAD(pers_guid);

                    PersonManager persManager = new PersonManager();

                    List<EMDPerson> myPersons = persManager.GetCreatedBy(this.PersonGuid);

                    bool isCreatedByMe = myPersons.Exists(a => a.Guid == pers_guid);

                    SecurityUser securityUser = SecurityUser.NewSecurityUser(this.UserName);
                    if (!isCreatedByMe)
                    {
                        persModel.InitializeSecurity(securityUser);
                    }
                    else
                    {
                        persModel.InitializeSecurity(securityUser);
                        persModel.CanManage = true;
                        persModel.CanSave = true;
                        persModel.CanOnboard = true;
                        persModel.CanManagePictureVisiblePhonebook = true;
                        persModel.CanManagePictureVisibleAD = true;
                        persModel.CanManagePersonVisible = true;
                    }

                    SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                    if (!persModel.CanManage)
                    {
                        return GetNoPermissionView(isPartialView);
                    }

                    if (!secUser.IsAllowedPerson(pers_guid, SecurityPermission.PersonManagement_View_Manage) && !persModel.IsItSelf)
                    {
                        return GetNoPermissionView(isPartialView);
                    }



                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error", "Could not get Persondata: " + ex.Message.ToString());
            }

            if (isPartialView)
            {
                return PartialView("Update", persModel);
            }
            else
            {
                return View("Update", persModel);
            }
        }

        //[Route("DoUpdate/{pers_guid}")]
        [Route("DoUpdate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoUpdate([DataSourceRequest] DataSourceRequest request, PersonModel person)
        {
            Exception handledException = null;
            string errmsg = String.Empty;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (person != null && ModelState.IsValid)
            {
                try
                {
                    PersonHandler ph = new PersonHandler(this.PersonGuid);
                    EMDPerson emdPerson = new EMDPerson();
                    ReflectionHelper.CopyProperties<PersonModel, EMDPerson>(ref person, ref emdPerson);

                    person.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!person.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                    if (!secUser.IsAllowedPerson(person.Guid, SecurityPermission.PersonManagement_View_Manage) && !person.IsItSelf)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    if (!person.IsPictureVisible && person.IsPictureVisibleInAD)
                    {
                        ModelState.AddModelError("error", "It is not allowed to show the picture in the AD and hide it in the phonebook!");
                    }
                    else
                    {
                        if (person.CanManagePersonMainData || person.CanManageGender)
                        {
                            EMDPerson _emdPerson = (EMDPerson)ph.UpdateObject<EMDPerson>(emdPerson);
                        }

                        ObjectFlagManager ofm = new ObjectFlagManager(this.PersonGuid);
                        if (person.CanManagePersonVisible)
                        {
                            ofm.UpdateIsPersonVisibleInPhonebook(emdPerson.Guid, person.IsVisibleInPhonebook);
                        }


                        if (person.CanManagePictureVisiblePhonebook)
                            ofm.UpdateIsPictureVisible(emdPerson.Guid, person.IsPictureVisible);

                        if (person.CanManagePictureVisibleAD || (!person.IsPictureVisibleInAD && person.CanRemovePictureVisibleAD))
                        {
                            ofm.UpdateIsPictureVisibleAD(emdPerson.Guid, person.IsPictureVisibleInAD);
                        }
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Person: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Update", person, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Person has been updated!", personModel = person });
            }


        }


        [Route("readGenderListForSelect")]
        public ActionResult readGenderListForSelect()
        {

            List<GenderModel> listGenders = PersonModel.GetGenderList();
            return Json(listGenders, JsonRequestBehavior.AllowGet);
        }

        public void PopulateGenders()
        {
            List<GenderModel> listGenders = PersonModel.GetGenderList();
            ViewData["GenderNames"] = listGenders;
            ViewData["GenderNamesDefault"] = listGenders.FirstOrDefault();
        }

        [HttpGet]
        [Route("ReadAllPersonsForSelect")]
        public JsonResult ReadAllPersonsForSelect([DataSourceRequest]DataSourceRequest request)
        {
            List<Person> persons = new TaskItemManager().GetPersonsWithAssignedTasks();


            List<EMDPerson> emdPersons = new List<EMDPerson>();

            foreach (Person person in persons)
            {
                EMDPerson emdPerson = new EMDPerson();
                Person refPerson = person;
                ReflectionHelper.CopyProperties(ref refPerson, ref emdPerson);
                emdPersons.Add(emdPerson);
            }

            //   List < IEMDObject < EMDPerson >> persons = persHandler.GetObjects<EMDPerson, Person>("");
            List<TextValueModel> personModels = new List<TextValueModel>();
            foreach (EMDPerson pers in emdPersons)
            {
                if (pers.UserID != null)
                {
                    string userID = pers.UserID.Trim().ToLower();

                    TextValueModel persModel = new TextValueModel();
                    persModel.Text = pers.Display_FamilyName + " " + pers.Display_FirstName + " (" + pers.UserID + ")";
                    persModel.Value = pers.Guid;
                    personModels.Add(persModel);

                }
            }

            personModels = (from item in personModels orderby item.Text select item).ToList();

            return Json(personModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("CheckReducedName/{Name}")]
        public String CheckReducedName([DataSourceRequest]DataSourceRequest request, string name)
        {
            string nameReduced = name;
            nameReduced = TextAndNamingHelper.ReplaceGermanUmlaut(nameReduced);
            nameReduced = TextAndNamingHelper.RemoveSpecialCharacters(nameReduced);

            return nameReduced;
            //return Json(personModels, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("DeletePerson")]
        public ActionResult DeletePerson(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.PersonManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                PersonManager manager = new PersonManager(this.PersonGuid);
                EMDPerson person = manager.Delete(guid);

                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success });
        }

        [HttpPost]
        [Route("ReduceString")]
        public ActionResult ReduceString(string toReduce, string fieldName)
        {
            bool success = true;
            string reduced = StringUtils.TranslateGermanMutationCharacters(toReduce);
            reduced = StringUtils.TranslateSpecialCharacters(reduced);

            return Json(new { success = success, reduced = reduced, fieldName = fieldName });
        }

        [HttpPost]
        [Route("UploadPersonImage")]
        public ActionResult UploadPersonImage(IEnumerable<HttpPostedFileBase> imageFiles)
        {
            try
            {
                // The Name of the Upload component is "files"
                if (imageFiles != null)
                {
                    //For DEV-Environments
                    string directoryPath = String.Empty;

                    if (ConfigurationManager.AppSettings["EMD20Web.FolderPathPersonImageUpload"] == null || ConfigurationManager.AppSettings["EMD20Web.FolderPathPersonImageUpload"] == String.Empty)
                    {
                        string relative_path = "/Controller";
                        string absolute_path = Server.MapPath(relative_path);

                        DirectoryInfo directory = new DirectoryInfo(absolute_path);
                        DirectoryInfo root = directory.Parent.Parent;
                        directoryPath = root.FullName + @"\TempFileUploads\KAGMA";
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);
                    }
                    else
                    {
                        //For UAT & LIVE Deployments etc.
                        directoryPath = ConfigurationManager.AppSettings["EMD20Web.FolderPathPersonImageUpload"].ToString();
                    }

                    foreach (var file in imageFiles)
                    {
                        // Some browsers send file names with full path.
                        // We are only interested in the file name.
                        var fileName = Path.GetFileName(file.FileName);
                        //var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                        var physicalPath = Path.Combine(directoryPath, fileName);

                        // The files are not actually saved in this demo
                        file.SaveAs(physicalPath);
                    }
                }
                return Content("");

            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message.ToString());
                //return Content("Failed");
            }

            // Return an empty string to signify success

        }
    }
}
