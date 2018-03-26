using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EnterpriseModelList
    {
        public List<EnterpriseModel> EnterpriseModels { get; set; }

        public EnterpriseModel ParentEnterprise { get; set; }

        public EnterpriseModel CurrentEnterprise { get; set; }

        public Int32 ParentEnterpriseLevel { get; set; }

        public Int32 CurrentEnterpriseLevel { get; set; }

        public Boolean HasParent { get; set; }

        public EnterpriseModelList()
        {
            EnterpriseModels = new List<EnterpriseModel>();
            HasParent = true;
        }
    }

    public class EnterpriseViewModel : EnterpriseModel
    {


        public EnterpriseModelList enterpriseModelList { get; set; }

        public EnterpriseViewModel()
        {
            enterpriseModelList = new EnterpriseModelList();
        }

        public static EnterpriseViewModel copyFromDBObject(Enterprise ente)
        {
            EnterpriseViewModel entemo = new EnterpriseViewModel();
            ReflectionHelper.CopyProperties(ref ente, ref entemo);
            return entemo;
        }

        public static EnterpriseViewModel copyFromObject(EMDEnterprise ente)
        {
            EnterpriseViewModel entemo = new EnterpriseViewModel();
            ReflectionHelper.CopyProperties(ref ente, ref entemo);
            return entemo;
        }
    }

    public class EnterpriseModel : BaseModel
    {
        [ScaffoldColumn(true)]
        public string Guid { get; set; }
        [Editable(false)]
        public string HistoryGuid { get; set; }

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        [Display(Name = "Parent Enterprise")]
        public string Guid_Parent { get; set; }
        [Display(Name = "Root Enterprise")]
        public string Guid_Root { get; set; }
        [Display(Name = "Enterprise ID (historic)")]
        public int E_ID { get; set; }
        [Display(Name = "Parent ID")]
        public int E_ID_Parent { get; set; }
        [Display(Name = "Root ID")]
        public int E_ID_Root { get; set; }


        [Display(Name = "Parent ID")]
        public int E_ID_new_Parent { get; set; }
        [Display(Name = "Root ID")]
        public int E_ID_new_Root { get; set; }


        [Required(), Display(Name = "Short Name")]
        public string NameShort { get; set; }
        [Required(), Display(Name = "Long Name")]
        public string NameLong { get; set; }
        public string HomeIntranet { get; set; }
        public string HomeInternet { get; set; }
        public string Synonyms { get; set; }
        [Display(Name = "Fibu Nummer")]
        public string FibuNummer { get; set; }
        [Display(Name = "Fibu Gericht")]
        public string FibuGericht { get; set; }
        public string UID1 { get; set; }
        public string UID2 { get; set; }
        public string ARA { get; set; }
        public string DVR { get; set; }
        [Required(), UIHint("String"), Range(1000, 9999), Display(Name = "Enterprise ID")]
        public Nullable<int> E_ID_new { get; set; }
        public string IntranetCOM { get; set; }
        public Nullable<int> O_ID_Dis { get; set; }
        public Nullable<int> O_ID_Prof { get; set; }

        [Display(Name = "Disciplinary Org-Unit")]
        public string O_Guid_Dis { get; set; }

        [Display(Name = "Professional Org-Unit")]
        public EMDOrgUnit OrgUnitDis { get; set; }

        public EMDEnterprise ParentEnterprise { get; set; }

        public EMDEnterprise RootEnterprise { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.EnterpriseManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.EnterpriseManager_View; } }

        public string ParentDisplayName
        {
            get
            {
                if (ParentEnterprise == null && !string.IsNullOrEmpty(Guid_Parent))
                {
                    ParentEnterprise = Manager.EnterpriseManager.Get(Guid_Parent);
                }

                if (ParentEnterprise != null)
                {
                    return ParentEnterprise.NameShort;
                }

                return string.Empty;
            }
        }

        public string RootDisplayName
        {
            get
            {
                if (RootEnterprise == null && !string.IsNullOrEmpty(Guid_Root))
                {
                    RootEnterprise = Manager.EnterpriseManager.Get(Guid_Root);
                }

                if (RootEnterprise != null)
                {
                    return RootEnterprise.NameShort;
                }

                return string.Empty;
            }
        }
        [Display(Name = "Disciplinary OrgUnit")]
        public string O_Name_Dis
        {
            get
            {
                if (OrgUnitDis == null && O_Guid_Dis != null)
                {
                    OrgUnitDis = Manager.OrgUnitManager.Get(O_Guid_Dis);
                }

                if (OrgUnitDis != null)
                {
                    return OrgUnitDis.Name;
                }
                return null;
            }
        }

        public string O_Guid_Prof { get; set; }

        public EMDOrgUnit OrgUnitProf { get; set; }
        public string O_Name_Prof
        {
            get
            {
                if (OrgUnitProf == null && O_Guid_Prof != null)
                {
                    OrgUnitProf = Manager.OrgUnitManager.Get(O_Guid_Prof);
                }

                if (OrgUnitProf != null)
                {
                    return OrgUnitProf.Name;
                }
                return null;
            }
        }

        /// <summary>
        /// Is the pictrue visible for new onboarding per default
        /// </summary>
        [Required(), Display(Name = "Picture visible per default")]
        public bool AD_Picture { get; set; }

        /// <summary>
        /// Is onboarding allowed or not
        /// </summary>
        [Required(), Display(Name = "Onboarding allowed")]
        public bool HasEmployees { get; set; }

        [Display(Name = "Distribution E-Mail")]
        public string DistributionEmailAddress { get; set; }

        [Display(Name = "Has E-Mail")]
        public bool HasDistributionEmailAddress
        {
            get
            {
                return !string.IsNullOrWhiteSpace(DistributionEmailAddress);
            }
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanView = true;    //Everyone can view enterprises
        }
        public static EnterpriseModel Initialize(Enterprise ente)
        {
            EnterpriseModel entemo = new EnterpriseModel();
            ReflectionHelper.CopyProperties(ref ente, ref entemo);

            return entemo;
        }

        public static EnterpriseModel Initialize(EMDEnterprise ente)
        {
            EnterpriseModel entemo = new EnterpriseModel();
            ReflectionHelper.CopyProperties(ref ente, ref entemo);

            return entemo;
        }
    }
}