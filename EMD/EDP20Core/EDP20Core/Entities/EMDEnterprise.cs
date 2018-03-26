using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEnterprise : EMDObject<EMDEnterprise>
    {
        public string Guid_Parent { get; set; }
        public string Guid_Root { get; set; }
        public int E_ID { get; set; }
        public int E_ID_Parent { get; set; }
        public int E_ID_Root { get; set; }
        public string USDO_Guid { get; set; }
        public string NameShort { get; set; }
        public string NameLong { get; set; }
        public string HomeIntranet { get; set; }
        public string HomeInternet { get; set; }
        public string Synonyms { get; set; }
        public string FibuNummer { get; set; }
        public string FibuGericht { get; set; }

        /// <summary>
        /// Is the pictrue visible for new onboarding per default
        /// </summary>
        public bool AD_Picture { get; set; }

        /// <summary>
        /// Is onboarding allowed or not
        /// </summary>
        public bool HasEmployees { get; set; }
        public string UID1 { get; set; }
        public string UID2 { get; set; }
        public string ARA { get; set; }
        public string DVR { get; set; }
        public Nullable<int> E_ID_new { get; set; }
        public string IntranetCOM { get; set; }
        public Nullable<int> O_ID_Dis { get; set; }
        public Nullable<int> O_ID_Prof { get; set; }
        public string O_Guid_Dis { get; set; }
        public string O_Guid_Prof { get; set; }
        //public bool AD_Picture { get; set; }
        //public bool HasEmployees { get; set; }

        public string DistributionEmailAddress { get; set; }

        public override String Prefix { get { return "ENTE"; } }
        
        public EMDEnterprise(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEnterprise()
        { }
    }
}
