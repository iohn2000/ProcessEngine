using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.ProcessEngine.Shared.Enums;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedProcessEntityResultModel : BaseModel
    {
        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public string Guid { get; set; }
        public string EntityGuid { get; set; }

        public string EntityGuidPrefix
        {
            get
            {
                string returnValue = string.Empty;

                if (!string.IsNullOrWhiteSpace(EntityGuid) && EntityGuid.Length > 4)
                {
                    returnValue = EntityGuid.Substring(0, 4);
                }

                return returnValue;
            }
        }

        public string WFI_ID { get; set; }
        public string WFD_ID { get; set; }
        public string WFD_Name { get; set; }
        public string WFResultMessages { get; set; }
        public string RequestorEmplGuid { get; set; }
        public string EffectedPersGuid { get; set; }
        public System.DateTime WFStartTime { get; set; }
        public System.DateTime WFTargetDate { get; set; }

        public int Status { get; set; }

        public string StatusString
        {
            get
            {
                return ((EnumWorkflowInstanceStatus)Status).ToString();
            }
        }

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        public string ActionLinkView { get; set; }

        public string ActionLinkViewPopupTitle { get; set; }
        public string ActionLinkManage { get; set; }

        public string ActionLinkManagePopupTitle { get; set; }
        public bool ShowActionLinkView
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ActionLinkView);
            }

        }
        public bool ShowActionLinkManage
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ActionLinkManage);
            }
        }


        public string EntityName
        {
            get
            {
                string entityName = "not found";

                switch (EntityGuid.ToLower().Substring(0, 4))
                {
                    case "empl":
                        entityName = "Employment";
                        break;
                    case "enlo":
                        entityName = "Enterprise-Location";
                        break;
                    case "obre":
                        entityName = "Equipment";
                        break;
                    default:
                        break;
                }

                return entityName;
            }
        }

        public string RequestorName { get; set; }

        public string EffectedPersonName { get; set; }

    }
}