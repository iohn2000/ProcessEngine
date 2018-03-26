using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDFilterRule : EMDObject<EMDFilterRule>
    {
        public string Obj_Guid { get; set; }
        public Nullable<int> FilterOrder { get; set; }
        public string FilterAction { get; set; }
        public string E_Guid { get; set; }
        public string L_Guid { get; set; }
        public string ET_Guid { get; set; }
        public string ACC_Guid { get; set; }
        public string USTY_Enum { get; set; }
        public override String Prefix { get { return "FIRU"; } }
        /// <summary>
        /// Switches inheritance for an enterprise, if 0 there is no inheritance
        /// </summary>
        public bool EnteIsInherited { get; set; }
        /// <summary>
        /// holds the list of filterable properties
        /// </summary>
        private static List<string> filterableProperties;

        /// <summary>
        /// Returns a list of all possible 
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFilterableProperties()
        {
            if (filterableProperties == null)
            {
                filterableProperties = new List<string>();
                EMDFilterRule obj = new EMDFilterRule();

                filterableProperties.Add(nameof(obj.E_Guid));
                filterableProperties.Add(nameof(obj.ET_Guid));
                filterableProperties.Add(nameof(obj.ACC_Guid));
                filterableProperties.Add(nameof(obj.L_Guid));
                filterableProperties.Add(nameof(obj.USTY_Enum));
            }
            return filterableProperties;
        }


        public EMDFilterRule(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDFilterRule()
        { }
    }
}
