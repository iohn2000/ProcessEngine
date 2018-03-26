using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Defines a relation between a <seealso cref="EMDCategory"/> and another <seealso cref="EMDObject{T}"/>
    /// </summary>
    public class EMDCategoryEntity : EMDObject<EMDCategoryEntity>
    {
        /// <summary>
        /// The guid of the EMD-object linked to the category
        /// </summary>
        public string EntityGuid { get; set; }
        /// <summary>
        /// The guid of the <seealso cref="EMDCategory"/> the object is linked to
        /// </summary>
        public string CATE_Guid { get; set; }

        public override String Prefix { get { return "CAEN"; } }

        public EMDCategoryEntity(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDCategoryEntity()
        { }
    }
}
