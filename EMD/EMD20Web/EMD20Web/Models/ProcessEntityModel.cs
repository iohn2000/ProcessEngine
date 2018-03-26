using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.DB;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Logic;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ProcessEntityModel : BaseModel
    {
        public string Guid { get; set; }
        public string EntityGuid { get; set; }
        public string WFI_ID { get; set; }
        public string WFD_ID { get; set; }
        public string WFD_Name { get; set; }

        /// <summary>
        /// The Action of the workflowprocess
        /// </summary>
        public string WorkflowAction { get; set; }

        [Display(Name = "Result Messages")]
        public string WFResultMessages { get; set; }

        [Display(Name = "Result Messages")]
        public string WFResultMessagesShort
        {
            get
            {
                if (string.IsNullOrEmpty(WFResultMessages))
                {
                    return string.Empty;
                }
                int length = 50;
                if (WFResultMessages.Length < length)
                {
                    length = WFResultMessages.Length;
                }
                return string.Format("{0} ...", WFResultMessages.Substring(0, length));
            }
        }

        [Display(Name = "Requestor")]
        public string RequestorEmplFullName { get; set; }

        [Display(Name = "Requestor GUID")]
        public string RequestorEmplGuid { get; set; }
        [Display(Name = "Effected GUID")]
        public string EffectedPersGuid { get; set; }

        [Display(Name = "Requesting Date")]
        public System.DateTime RequestingDate { get; set; }

        [Display(Name = "WF Start Date")]
        public System.DateTime WFStartTime { get; set; }
        [Display(Name = "Target Date")]
        public System.DateTime WFTargetDate { get; set; }

        public System.DateTime Modified { get; set; }
        public string Status { get; internal set; }

        public bool IsPartialView { get; set; }

        /// <summary>
        /// Has Info is true if any processentities were found
        /// </summary>
        public bool HasInfo
        {
            get
            {
                return !string.IsNullOrWhiteSpace(WFI_ID);
            }
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static ProcessEntityModel Map(EMDProcessEntity processEntity)
        {
            ProcessEntityModel model = new ProcessEntityModel();

            ReflectionHelper.CopyProperties<EMDProcessEntity, ProcessEntityModel>(ref processEntity, ref model);

            PersonManager personManager = new PersonManager();
            EMDPerson eMDPerson = personManager.GetPersonByEmployment(processEntity.RequestorEmplGuid);
            model.RequestorEmplFullName = PersonManager.GetFullDisplayName(eMDPerson);
            model.RequestingDate = processEntity.Created;

            return model;
        }
    }

}