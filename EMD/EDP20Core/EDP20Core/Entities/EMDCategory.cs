using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// EMDCategory object that can be linked to other emd entities over the EMDCategoryEntity object
    /// </summary>
    public class EMDCategory : EMDObject<EMDCategory>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Type of the category <seealso cref="EnumCategoryType"/>
        /// </summary>
        public int CategoryType { get; set; }

        public override String Prefix { get { return "CATE"; } }

        public EMDCategory(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDCategory()
        { }
    }
}
