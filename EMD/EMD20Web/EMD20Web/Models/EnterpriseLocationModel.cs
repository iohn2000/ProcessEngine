using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EnterpriseLocationModel : BaseModel
    {
        public EnterpriseLocationModel()
        {
            EnterpriseNameEnhanced = string.Empty;
            LocationNameEnhanced = string.Empty;
        }

        public string Guid { get; set; }

        public string HistoryGuid { get; set; }

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        [Display(Name = "Enterprise"), Required()]
        public string E_Guid { get; set; }
        [Display(Name = "Location"), Required()]
        public string L_Guid { get; set; }
        public Nullable<int> L_ID { get; set; }

        public Nullable<int> E_ID { get; set; }
        [Display(Name = "Distribution Group int")]
        public string DistList_int { get; set; }
        [Display(Name = "Distribution Group ext")]
        public string DistList_ext { get; set; }

        public byte Status { get; set; }

        [Display(Name = "Status")]
        public string StatusDisplayName
        {
            get
            {
                string statusName = "Not defined";
                //StatusItem item = new EnterpriseLocationProcessStatus().GetProcessStatusItem((int)this.Status);

                switch ((int)Status)
                {
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ERROR:
                        statusName = "Error";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_NOTSET:
                        statusName = "Not set";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ACTIVE:
                        statusName = "Active";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_REMOVED:
                        statusName = "Removed";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ORDERED:
                        statusName = "Ordered";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_QUEUED:
                        statusName = "Queued";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_INPROGRESS:
                        statusName = "In Progress";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_TIMEOUT:
                        statusName = "Timeout";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_DECLINED:
                        statusName = "Declined";
                        break;
                    default:
                        statusName = new EnterpriseLocationProcessStatus().GetProcessStatusItem((int)this.Status).StatusLong;
                        break;
                }

                return statusName;
            }
        }

        public override String CanManagePermissionString { get { return SecurityPermission.EnterpriseLocationManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.EnterpriseLocationManager_View; } }

        [Display(Name = "Enterprise")]
        public string EnterpriseNameEnhanced { get; internal set; }
        public string EnterpriseName { get; internal set; }
        public string EnterpriseNumber { get; internal set; }

        [Display(Name = "Location")]
        public string LocationNameEnhanced { get; internal set; }

        public string LocationName { get; internal set; }
        public string LocationObjectNumber { get; internal set; }
        public string EnteGuid { get; internal set; }
        public string LocaGuid { get; internal set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanView = true; //Everyone can see enterpriseLocations
        }

        public static EnterpriseLocationModel Initialize(EnterpriseLocation enlo)
        {
            EnterpriseLocationModel enloMod = new EnterpriseLocationModel();
            ReflectionHelper.CopyProperties(ref enlo, ref enloMod);
            return enloMod;
        }

        public static EnterpriseLocationModel Initialize(EMDEnterpriseLocation enlo)
        {
            EnterpriseLocationModel enloMod = new EnterpriseLocationModel();
            ReflectionHelper.CopyProperties(ref enlo, ref enloMod);
            return enloMod;
        }
    }
}