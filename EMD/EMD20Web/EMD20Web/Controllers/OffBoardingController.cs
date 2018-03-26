using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.WF;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("OffBoarding")]
    //[Filters.AccessPermissions(AccessPermission="ONBOARDING,  EMDADMIN  ,  PRIME")]
    public class OffBoardingController : BaseController
    {
        [Route()]
        public ActionResult Index()
        {
            return View();
        }

        [Route("DoOffboarding")]
        [Route("DoOffboarding/{employmentGuid}/{isPartialView}")]
        public ActionResult DoOffboarding(string employmentGuid, bool isPartialView = false)
        {
            OffboardingModel model = new OffboardingModel();

            model.EmploymentGuid = employmentGuid;
            model.EquipmentInstanceModels = EquipmentInstanceModel.GetEquipmentInstanceModels(employmentGuid, this.UserName).ToList();

            // in case of offboarding set do keep to false, to avoid errors in remove process
            foreach (EquipmentInstanceModel equipmentInstanceModel in model.EquipmentInstanceModels)
            {
                equipmentInstanceModel.DoKeep = false;
            }


            if (isPartialView)
            {
                return PartialView("DoOffboarding", model);
            }
            else
            {
                return View("DoOffboarding", model);
            }

            //string layout = getLayout();
            //if (layout != String.Empty)
            //{
            //    return View("Create", layout);
            //}
            //else
            //{
            //    return View();
            //}
        }

        [Route("DoOffboarding")]
        [HttpPost]
        public ActionResult DoOffboarding(OffboardingModel offBoardingModel)
        {
            Exception handledException = null;
            bool dateError = false;
            string enterpriseName = null;

            if (offBoardingModel.EquipmentInstanceModels != null)
            {
                for (int i = 0; i < offBoardingModel.EquipmentInstanceModels.Count; i++)
                {
                    if (offBoardingModel.EquipmentInstanceModels[i].TargetDate == null)
                    {
                        ModelState.AddModelError(string.Format("EquipmentInstanceModels[{0}].DoRemove", i), string.Format("You must define a date for {0}", offBoardingModel.EquipmentInstanceModels[i].EquipmentName));
                        dateError = true;
                    }
                }
            }

            if (dateError)
            {
                ModelState.AddModelError("error", "All date fields must have a value!");
            }


            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (offBoardingModel != null && ModelState.IsValid)
            {
                try
                {
                    List<RemoveEquipmentInfo> removeEquipmentInfos = new List<RemoveEquipmentInfo>();

                    ObjectRelationHandler handler = new ObjectRelationHandler(this.PersonGuid);

                    if (offBoardingModel.EquipmentInstanceModels != null)
                    {
                        foreach (var equipmentInstanceModel in offBoardingModel.EquipmentInstanceModels)
                        {
                            if (equipmentInstanceModel.ObjectRelationGuid != null)
                            {
                                removeEquipmentInfos.Add(new RemoveEquipmentInfo()
                                {
                                    TargetDate = equipmentInstanceModel.TargetDate,
                                    ObreGuid = equipmentInstanceModel.ObjectRelationGuid,
                                    DoKeep = equipmentInstanceModel.DoKeep,
                                    EquipmentDefinitionGuid = ((EMDObjectRelation)handler.GetObject<EMDObjectRelation>(equipmentInstanceModel.ObjectRelationGuid)).Object2
                                });
                            }
                        }
                    }

                    bool offBoardingSuccess = false;
                    EmploymentManager employmentManager = new EmploymentManager(this.PersonGuid, MODIFY_COMMENT);
                    OffboardingManager offboardingManager = new OffboardingManager(this.PersonGuid, MODIFY_COMMENT);
                    offBoardingSuccess = offboardingManager.StartOffBoardingWorkflow(offBoardingModel.EmploymentGuid , offBoardingModel.ExitDate.Value, offBoardingModel.LastDay.Value, removeEquipmentInfos, this.UserMainEmplGuid, offBoardingModel.ResourceNumber);
                    if (offBoardingSuccess == false)
                    {
                        Exception ex = new Exception(string.Format("Could not start Offboarding for Employment {0}", offBoardingModel.EmploymentGuid));
                        handledException = ex;
                        ModelState.AddModelError("error", string.Format("Could not start Offboarding for Employment {0}", offBoardingModel.EmploymentGuid));
                    }

                    try
                    {
                        EMDEmployment employment = (EMDEmployment)new EmploymentHandler().GetObject<EMDEmployment>(offBoardingModel.EmploymentGuid);
                        EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)new EnterpriseLocationHandler().GetObject<EMDEnterpriseLocation>(employment.ENLO_Guid);
                        EMDEnterprise enterprise = (EMDEnterprise)new EnterpriseHandler().GetObject<EMDEnterprise>(enlo.E_Guid);
                        // set the enterprise name to get the right tab for disable change buttons
                        enterpriseName = enterprise.NameShort;
                    }
                    catch (Exception ex)
                    {
                        handledException = ex;
                        logger.Error("Couldn't get the enterprisename of updating UI", ex);
                    }

                }
                catch (Exception ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not edit Workflow: {0}", ex.Message));
                }
            }


            PartialViewResult result = GetPartialFormWithErrors("Edit", offBoardingModel, handledException, "The Offboarding couldn't be started. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = "The Offboarding workflow has been started!", enterpriseName = enterpriseName });
            }


        }
    }
}