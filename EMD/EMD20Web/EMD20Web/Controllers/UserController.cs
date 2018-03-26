using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("User")]
    public class UserController : BaseController
    {
        [HttpGet]
        [Route("Edit")]
        [Route("Add")]
        [Route("Add/{guidEmployment}/{isPartialView}")]
        [Route("Edit/{guidUser}/{isPartialView}")]
        [Route("Edit/{guidUser}")]
        public ActionResult Edit(string guidUser, string guidEmployment, bool isPartialView = false, bool isEditable = true)
        {
            UserModel mappedModel = new UserModel(true);

            ViewBag.Titel = "Edit Workflow";
            try
            {
                if (!string.IsNullOrEmpty(guidUser))
                {
                    mappedModel.EMPL_Guid = guidEmployment;
                    ViewBag.Titel = "Edit Workflow";

                    EMDUser user = Manager.UserManager.Get(guidUser);
                    mappedModel = UserModel.Map(user, true);
                }
                else
                {
                    mappedModel.EMPL_Guid = guidEmployment;
                    ViewBag.Titel = "Add new User";
                }

            }
            catch (Exception e)
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Info("UserController.Edit(string guidUser, string guidEmployment, bool isPartialView=false, bool isEditable=true) => TODO: write HelperMethod for generalizing this kind of handling", e);


            }
            if (mappedModel != null)
            {
                mappedModel.isEditable = isEditable;
            }

            if (isPartialView)
            {
                return PartialView("Edit", mappedModel);
            }
            else
            {
                return View("Edit", mappedModel);
            }
        }



        [HttpGet]
        [Route("View/{guidUser}/{isPartialView}")]
        public ActionResult View(string guidUser, string guidEmployment, bool isPartialView = false)
        {
            UserModel mappedModel = new UserModel();
            mappedModel.EMPL_Guid = guidEmployment;


            ViewBag.Titel = "Edit Workflow";
            try
            {
                EMDUser user = Manager.UserManager.Get(guidUser);
                mappedModel = UserModel.Map(user);
            }
            catch (Exception e)
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Info("UserController.View(string guidUser, string guidEmployment, bool isPartialView=false) => TODO: write HelperMethod for generalizing this kind of handling", e);
            }



            if (isPartialView)
            {
                return PartialView("View", mappedModel);
            }
            else
            {
                return View("View", mappedModel);
            }
        }


        [ValidateInput(false)]
        [HttpPost]
        [Route("Edit")]
        public ActionResult Edit(UserModel userModel)
        {
            Exception handledException = null;
            UserModel mappedModel = new UserModel();

            ViewBag.Title = "Edit User";
            string message = string.Empty;
            string newUserName = string.Empty;

            if (userModel != null && ModelState.IsValid)
            {
                CoreTransaction transaction = new CoreTransaction();

                try
                {
                    IPersonManager personManager = Manager.PersonManager;
                    personManager.Transaction = transaction;
                    personManager.Guid_ModifiedBy = this.PersonGuid;

                    IUserManager userManager = Manager.UserManager;
                    userManager.Transaction = transaction;
                    userManager.Guid_ModifiedBy = this.PersonGuid;

                    transaction.Begin();

                    // Update user
                    if (!string.IsNullOrEmpty(userModel.Guid))
                    {
                        EMDPerson person = personManager.GetPersonByEmployment(userModel.EMPL_Guid);

                        EMDUser emdUser = Manager.UserManager.Get(userModel.Guid);
                        string userNameOld = person.UserID;

                        emdUser = UserModel.Update(emdUser, userModel);
                        ViewBag.Title = "Edit User";
                        userManager.Update(emdUser);

                        if (person != null && person.USER_GUID != userModel.Guid && userModel.IsMainUser)
                        {
                            new UserManager(transaction, this.PersonGuid).SetPersonMainUser(userModel.Guid, person.Guid);

                            // if the username has changed >> change also the picturename
                            if (userNameOld != userModel.Username)
                            {
                                string personPortraitFullPathOld = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"].ToString() + userNameOld + ".jpg";
                                string personPortraitFullPathNew = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"].ToString() + userModel.Username + ".jpg";

                                if (System.IO.File.Exists(personPortraitFullPathOld))
                                {
                                    System.IO.File.Move(personPortraitFullPathOld, personPortraitFullPathNew);
                                }
                            }


                            message = "The User has been updated and is set as main-user!";
                        }
                        else
                        {
                            message = "The User has been updated!";
                        }
                    }
                    else
                    {
                        EMDUser emdUser = UserModel.Map(userModel);
                        ViewBag.Title = "Add new User";

                        userManager.Create(emdUser);
                        message = "The User has been created!";
                    }

                    transaction.Commit();
                }
                catch (UserException ex)
                {
                    transaction.Rollback();
                    handledException = ex;
                    switch (ex.UserExceptionType)
                    {
                        case EnumUserExceptionType.General:
                            message = string.Format("A general error occured for the user {0}", userModel.Username);
                            break;
                        case EnumUserExceptionType.UserExists:
                            message = string.Format("A user with the name {0} already exists!", userModel.Username);
                            break;
                        case EnumUserExceptionType.UserNotFound:
                            message = string.Format("A user with the name {0} couldn't be updated because the user was not found!", userModel.Username);
                            break;
                        default:
                            break;
                    }

                    ModelState.AddModelError("error", message);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", mappedModel, handledException, "The User couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = message, idUpdateGrid = string.Format("UserModel{0}", userModel.EMPL_Guid), newUserName = newUserName });
            }
        }


        [Route("GetUserStatusModelList")]
        public ActionResult GetUserStatusModelList()
        {
            return Json(UserStatusModel.GetUserStatusModelList(), JsonRequestBehavior.AllowGet);
        }

        [Route("GetUserTypeModelList")]
        public ActionResult GetUserTypeModelList(bool hideDeleted)
        {
            return Json(UserTypeModel.GetUserTypeModelList(hideDeleted), JsonRequestBehavior.AllowGet);
        }

        [Route("ReadUserDomainListForSelect")]
        public ActionResult ReadUserDomainListForSelect()
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                UserDomainHandler handler = new UserDomainHandler();
                List<IEMDObject<EMDUserDomain>> emdEntities = handler.GetObjects<EMDUserDomain, UserDomain>(null);


                emdEntities.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDUserDomain)entity).Name, entity.Guid));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading userDomains";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Route("DeleteUser")]
        public ActionResult DeleteUser(string guid)
        {
            IUserManager userManager = Manager.UserManager;
            userManager.Guid_ModifiedBy = this.PersonGuid;

            EMDUser emdUser = userManager.Get(guid);

            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                userManager.Delete(guid);
                success = true;
            }
            catch (UserException ex)
            {
                errorModel = new ErrorModel(ex);

                switch (ex.UserExceptionType)
                {
                    case EnumUserExceptionType.General:
                        errorModel.ErrorMessage = string.Format("A general error occured for the user {0}", emdUser.Username);
                        break;

                    case EnumUserExceptionType.UserNotFound:
                        errorModel.ErrorMessage = string.Format("A user with the name {0} couldn't be deleted because the user was not found!", emdUser.Username);
                        break;
                    default:
                        break;
                }


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
        [Route("LogoutImpersonatedUser")]
        public ActionResult LogoutImpersonatedUser()
        {
            ResetImpersonation();

            return Json(new { success = true });
        }

        [Route("GetUserNameForEmployment")]
        [HandleError()]
        public ActionResult GetUserNameForEmployment(string emplGuid)
        {
            Exception handledException = null;
            bool success = false;
            ErrorModel errorModel = null;
            string userName = string.Empty;
            try
            {
                PersonManager pm = new PersonManager(this.PersonGuid);
                EmploymentManager employmentManager = new EmploymentManager(this.PersonGuid);
                EMDEmployment employment = employmentManager.GetEmployment(emplGuid);
                EMDEmploymentType employmentType = (EMDEmploymentType)new EmploymentTypeHandler().GetObject<EMDEmploymentType>(employment.ET_Guid);
                EMDPerson person = pm.GetPersonByEmployment(emplGuid);

                UserManager userManager = new UserManager(this.PersonGuid);
                userName = userManager.GetNewMainUserName(person.C128_FamilyName, person.C128_FirstName, employmentType);
                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                handledException = ex;
                logger.Error(string.Format("GetUserNameForEmployment failed for Employment: {0}", emplGuid), ex);
            }


            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success, userName = userName });
        }

        [HttpPost]
        [Route("DeleteCache")]
        public ActionResult DeleteCache()
        {
            DeleteWebCache();
            new UserHandler().ClearCache();

            return Json(new { success = true });
        }
    }
}