using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.ReflectionHelper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{

    [RoutePrefix("ContactData")]
    public class ContactDataController : BaseController
    {


        [HttpGet]
        [Route("EditCallNumbers/{empl_guid}")]
        [Route("EditCallNumbers/{empl_guid}/{isPartialView}")]
        public ActionResult EditCallNumbers(string empl_guid, bool isPartialView = false)
        {
            ContactDataModel contactDataModel = new ContactDataModel();
            contactDataModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!contactDataModel.CanManage)
                return GetNoPermissionView(isPartialView);

            contactDataModel.GuidEmployment = empl_guid;

            if (isPartialView)
            {
                return PartialView("EditCallNumbers", contactDataModel);
            }
            else
            {
                return View("EditCallNumbers", contactDataModel);
            }
        }

        [HttpGet]
        [Route("EditNumber")]
        [Route("AddNumber/{empl_guid}/{ct_id}/{isFuture}")]
        [Route("AddNumber/{empl_guid}/{ct_id}/{isFuture}/{isPartialView}")]
        [Route("EditNumber/{empl_guid}/{cont_guid}/{ct_id}/{isFuture}")]
        [Route("EditNumber/{empl_guid}/{cont_guid}/{ct_id}/{isFuture}/{isPartialView}")]
        public ActionResult EditNumber([DataSourceRequest]DataSourceRequest request, string empl_guid, string cont_guid, int ct_id, bool isFuture, bool isPartialView = false)
        {
            ContactHandler locaHandler = new ContactHandler();
            ContactModel model = null;

            if (EMDContact.IsEMDGuid(cont_guid))
            {
                EMDContact contact = (EMDContact)locaHandler.GetObject<EMDContact>(cont_guid);
                model = ContactModel.Map(contact);
            }
            else
            {
                model = ContactModel.New(empl_guid, isFuture);

                EMDContactType emdContactType = new ContactTypeHandler().GetObjects<EMDContactType, ContactType>("CT_ID == " + ct_id + "").Cast<EMDContactType>().FirstOrDefault();

                model.CT_Guid = emdContactType.Guid;
                model.C_CT_ID = emdContactType.CT_ID;
            }

            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage)
                return GetNoPermissionView(isPartialView);


            if (isPartialView)
            {
                return PartialView("EditNumber", model);
            }
            else
            {
                return View("EditNumber", model);
            }
        }

        [HttpGet]
        [Route("EditContactData")]
        [Route("EditContactData/{empl_guid}/{ct_id}")]
        [Route("EditContactData/{empl_guid}/{ct_id}/{isPartialView}")]
        public ActionResult EditContactData([DataSourceRequest]DataSourceRequest request, string empl_guid, int ct_id, bool isPartialView = false)
        {
            ContactHandler locaHandler = new ContactHandler();
            ContactDataFutureModel model = new ContactDataFutureModel();

            EMDContactType emdContactType = new ContactTypeHandler().GetObjects<EMDContactType, ContactType>("CT_ID == " + ct_id + "").Cast<EMDContactType>().FirstOrDefault();

            List<EMDContact> contacts = locaHandler.GetActiveObjectsInInterval<EMDContact, Contact>(DateTime.Now, EMDContact.INFINITY, string.Format("EP_Guid == \"{0}\" && CT_Guid == \"{1}\"", empl_guid, emdContactType.Guid)).ToList();




            // create empty models for init
            model.CurrentContact = ContactModel.New(empl_guid, false);
            model.CurrentContact.CT_Guid = emdContactType.Guid;
            model.CurrentContact.C_CT_ID = emdContactType.CT_ID;

            model.FutureContact = ContactModel.New(empl_guid, true);
            model.FutureContact.CT_Guid = emdContactType.Guid;
            model.FutureContact.C_CT_ID = emdContactType.CT_ID;

            foreach (EMDContact emdContact in contacts)
            {
                ContactModel contactModel = ContactModel.Map(emdContact);

                if (contactModel.IsFuture)
                {
                    model.FutureContact = contactModel;
                }
                else
                {
                    model.CurrentContact = contactModel;
                }
            }

            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage)
                return GetNoPermissionView(isPartialView);


            if (isPartialView)
            {
                return PartialView("EditContactData", model);
            }
            else
            {
                return View("EditContactData", model);
            }
        }


        [HttpPost, ValidateInput(false)]
        [Route("EditContactData")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult EditContactData(ContactDataFutureModel contactDataFutureModel)
        {
            contactDataFutureModel.CurrentContact.Number = contactDataFutureModel.CurrentContact.NumberAsText;
            contactDataFutureModel.FutureContact.Number = contactDataFutureModel.FutureContact.NumberAsText;

            Exception handledException = null;
            contactDataFutureModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            ContactManager contactManager = new ContactManager();

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            if (!contactDataFutureModel.IsFutureChecked)
            {
                ModelState.Remove("FutureContact.Number");
            }


            if (contactDataFutureModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!contactDataFutureModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    bool hasChanges = true;
                    if (!string.IsNullOrEmpty(contactDataFutureModel.CurrentContact.Guid))
                    {
                        EMDContact existing = contactManager.Get(contactDataFutureModel.CurrentContact.Guid);
                        if (existing.Text.Trim() == contactDataFutureModel.CurrentContact.Number?.Trim())
                        {
                            hasChanges = false;
                        }

                    }
                    if (hasChanges)
                    {
                        SaveContact(contactDataFutureModel.CurrentContact);
                    }


                    hasChanges = true;
                    if (!string.IsNullOrEmpty(contactDataFutureModel.FutureContact.Guid))
                    {
                        if (!contactDataFutureModel.IsFutureChecked)
                        {
                            hasChanges = false;
                            contactManager.Delete(contactDataFutureModel.FutureContact.Guid);
                        }
                        else
                        {
                            EMDContact existing = contactManager.Get(contactDataFutureModel.FutureContact.Guid);
                            if (existing.Text.Trim() == contactDataFutureModel.FutureContact.Number.Trim() && existing.ActiveFrom == contactDataFutureModel.FutureContact.ActiveFrom)
                            {
                                hasChanges = false;
                            }
                        }

                    }

                    if (hasChanges)
                    {
                        SaveContact(contactDataFutureModel.FutureContact);
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit contact number: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", contactDataFutureModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                List<ContactModel> models = GetAllContacts(contactDataFutureModel.CurrentContact.EP_Guid, true);
                return Json(new { Url = "EditNumber", isMain = new EmploymentManager().IsMainEmployment(contactDataFutureModel.CurrentContact.EP_Guid), tabName = GetCompanyShortName(contactDataFutureModel.CurrentContact.EP_Guid), contactDataViewModel = models, message = "The contact number has been updated!" });
            }
        }

        private string GetCompanyShortName(List<ContactModel> models)
        {
            string name = string.Empty;
            if (models != null && models.Count > 0)
            {
                name = GetCompanyShortName(models[0].EP_Guid);
            }

            return name;
        }

        private string GetCompanyShortName(string empl_guid)
        {
            EMDEmployment emdEmployment = (EMDEmployment)new EmploymentHandler().GetObject<EMDEmployment>(empl_guid);
            EMDEnterpriseLocation enteLocation = (EMDEnterpriseLocation)new EnterpriseLocationHandler().GetObject<EMDEnterpriseLocation>(emdEmployment.ENLO_Guid);
            EMDEnterprise emdEnterprise = (EMDEnterprise)new EnterpriseHandler().GetObject<EMDEnterprise>(enteLocation.E_Guid);
            return emdEnterprise.NameShort;
        }

        [HttpPost, ValidateInput(false)]
        [Route("EditNumber")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult EditNumber(ContactModel contactModel)
        {
            Exception handledException = null;
            contactModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            if (string.IsNullOrEmpty(contactModel.Number))
            {
                if (!string.IsNullOrEmpty(contactModel.NationalCode))
                {
                    ModelState.AddModelError("NationalCode", "If a Nationalcode is defined, the number must be set.");
                }

                if (!string.IsNullOrEmpty(contactModel.Prefix))
                {
                    ModelState.AddModelError("Prefix", "If a Prefix is defined, the number must be set.");
                }
            }
            else
            {
                if (contactModel.C_CT_ID == ContactTypeHandler.MOBILE || contactModel.C_CT_ID == ContactTypeHandler.PHONE)
                {
                    if (string.IsNullOrEmpty(contactModel.NationalCode) || string.IsNullOrEmpty(contactModel.Prefix))
                    {
                        ModelState.AddModelError("Number", "You have to set the international code and the prefix");
                    }
                }
            }

            if (contactModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!contactModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    SaveContact(contactModel);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit contact number: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", contactModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                List<ContactModel> models = GetAllContacts(contactModel.EP_Guid, true);
                return Json(new { Url = "EditNumber", isMain = new EmploymentManager().IsMainEmployment(contactModel.EP_Guid), tabName = GetCompanyShortName(contactModel.EP_Guid), contactDataViewModel = models, message = "The contact number has been updated!" });
            }
        }


        private List<ContactModel> GetAllContacts(string empl_guid, bool includeNonNumbers = false)
        {
            ContactHandler contactHandler = new ContactHandler();
            List<ContactModel> models = new List<ContactModel>();


            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.DIRECTDIAL));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.DIRECTDIAL, true));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.DIRECTDIAL2));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.DIRECTDIAL2, true));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.MOBILE));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.MOBILE, true));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.PHONE));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.PHONE, true));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.DIRECTEFAX));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.DIRECTEFAX, true));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.EFAX));
            models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.EFAX, true));

            if (includeNonNumbers)
            {
                models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.JOBTITLE));
                models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.JOBTITLE, true));
                models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.ROOM));
                models.Add(ContactModel.GetContactModel(contactHandler, empl_guid, ContactTypeHandler.ROOM, true));
            }

            ContactModel.UpdateModels(ref models);

            return models;
        }

        [Route("ReadContacts")]
        public ActionResult ReadContacts([DataSourceRequest]DataSourceRequest request, string empl_guid)
        {
            ContactHandler contactHandler = new ContactHandler();
            List<ContactModel> models = GetAllContacts(empl_guid);


            DataSourceResult myresult;
            myresult = models.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }




        private string SaveContact(ContactModel contactModel)
        {
            ContactManager contactManager = new ContactManager(this.PersonGuid, MODIFY_COMMENT);
            EMDContact emdContact = new EMDContact();
            ReflectionHelper.CopyProperties<ContactModel, EMDContact>(ref contactModel, ref emdContact);

            string textValue = string.Empty;

            switch (contactModel.C_CT_ID)
            {
                case ContactTypeHandler.JOBTITLE:
                    emdContact.Text = contactModel.Number?.Trim();

                    break;
                case ContactTypeHandler.ROOM:
                    emdContact.Text = contactModel.Number?.Trim();
                    // overwrite model, because for room the flags must be always set to true
                    contactModel.IsVisibleInPhoneBook = true;
                    contactModel.IsVisibleInPhoneBook = true;
                    break;
                case ContactTypeHandler.DIRECTDIAL:
                case ContactTypeHandler.DIRECTDIAL2:
                case ContactTypeHandler.DIRECTEFAX:
                    emdContact.Details = contactModel.Prefix?.Trim();
                    emdContact.Text = contactModel.Number?.Trim();

                    break;
                case ContactTypeHandler.MOBILE:
                case ContactTypeHandler.PHONE:
                case ContactTypeHandler.EFAX:
                    emdContact.Text = string.Format("{0} {1} {2}", contactModel.NationalCode?.Trim(), contactModel.Prefix?.Trim(), contactModel.Number?.Trim());
                    break;
                default:
                    break;
            }

            emdContact.VisibleKatce = contactModel.IsVisibleInCallCenter;
            emdContact.VisiblePhone = contactModel.IsVisibleInPhoneBook;


            if (!EMDContact.IsEMDGuid(emdContact.Guid))
            {
                if (!string.IsNullOrEmpty(emdContact.Text))
                {
                    contactManager.WriteOrModifyContact(emdContact);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(emdContact.Text))
                {
                    emdContact.Text = string.Empty;
                }

                if (EMDContact.IsEMDGuid(emdContact.Guid))
                {
                    contactManager.Update(emdContact);
                }
                else
                {
                    contactManager.WriteOrModifyContact(emdContact);
                }
            }


            textValue = string.IsNullOrEmpty(emdContact.Text) ? "&nbsp;" : emdContact.Text;

            return textValue;
        }

        [HttpPost]
        [Route("Delete")]
        public ActionResult Delete(string guid)
        {
            
            if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.ContactManager_View_Manage) )
                return GetNoPermissionView(false);

            ErrorModel errorModel = null;
            bool success = false;
            EMDContact contact = null;

            try
            {
                //if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.ContactManager_View_Manage))
                //    throw new Exception(SecurityHelper.NoPermissionText);

                ContactManager manager = new ContactManager(this.PersonGuid, MODIFY_COMMENT);
                EMDContact emdContact = manager.Get(guid);

                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);

                if (!secUser.IsAllowedEmployment(secUser.UserId, emdContact.EP_Guid))
                {
                    return GetNoPermissionView(false);
                }
                    
                contact = manager.Delete(guid);

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

            List<ContactModel> models = GetAllContacts(contact.EP_Guid, true);
            return Json(new { success = success, Url = "EditNumber", isMain = new EmploymentManager().IsMainEmployment(contact.EP_Guid), tabName = GetCompanyShortName(contact.EP_Guid), contactDataViewModel = models, message = "The contact number has been deleted!" });
        }
    }
}