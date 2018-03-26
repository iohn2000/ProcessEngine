using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using System.Xml.Linq;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.Util.ErrorHandling;
using System.Data;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Kmp")]
    public class kmpController : Controller
    {
        // GET: kmp
        public ActionResult Index()
        {
            return View();
        }

        [Route("Test")]
        [Route("Test/{userId}")]
        public ActionResult Test(string userId)
        {
            Models.Kmp.KmpTestModel kmpModel = new Models.Kmp.KmpTestModel();
            string uName = string.Empty;
            try
            {
                try
                {
                    if(!String.IsNullOrEmpty(userId))
                    {
                        uName = userId;

                        PersonHandler persHandler = new PersonHandler();
                        persHandler.transaction.dbContext.Database.Connection.Open();

                        try
                        {
                            PersonManager persManager = new PersonManager();
                            EMDPerson pers = persManager.GetPersonByUserId(uName);

                            if (pers != null)
                            {
                                EmploymentManager emplManager = new EmploymentManager();
                                EMDEmployment empl = emplManager.GetMainEploymentForPerson(pers.Guid);
                                if (empl != null)
                                {
                                    try
                                    {
                                        MonitoringResult result = Monitoring.CreateAndDeleteContactForEmployment(empl.Guid);
                                        kmpModel.Error = result.Error;
                                        EnumMonitoringStatus status = (EnumMonitoringStatus)result.Status;
                                        kmpModel.Status = status.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                                        kmpModel.Error = ex.Message + " - " + ex.StackTrace;
                                    }
                                }
                                else
                                {
                                    kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                                    kmpModel.Error = string.Format("Could not find employment for username {0} and person guid {1} - {2}", uName, pers.Guid);
                                }
                            }
                            else
                            {
                                kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                                kmpModel.Error = string.Format("Could not find person for username {0}", uName);
                            }
                        }
                        catch (BaseException bex)
                        {
                            throw bex;
                        }
                        catch (Exception ex)
                        {
                            kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                            kmpModel.Error = string.Format("Error getting person for username {0}", uName);
                        }
                    }
                    else
                    {
                        kmpModel.Status = kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                        kmpModel.Error = "No username specified. Try: www.example.com/kmp/test/{userID}";
                    }
                }
                catch (Exception ex)
                {
                    kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                    kmpModel.Error = string.Format("Could not get username - {0} - {1}", ex.Message, ex.StackTrace);
                    throw ex;
                }
            }
            catch (TimeoutException tex)
            {
                kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                kmpModel.Error = string.Format("TimeoutException - {0} - {1}", tex.Message, tex.StackTrace);
            }
            catch (BaseException bex)
            {
                kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                kmpModel.Error = string.Format("General error occured:  {0} - {1}", bex.Message, bex.StackTrace);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(kmpModel.Status))
                {
                    kmpModel.Status = EnumMonitoringStatus.CRITICAL.ToString();
                }
                if (string.IsNullOrWhiteSpace(kmpModel.Error))
                {
                    kmpModel.Error = string.Format("General error occured: {0} - {1}", ex.Message, ex.StackTrace);
                }
            }
            return View("Test","_EmptyLayout",kmpModel);
        }
    }
}