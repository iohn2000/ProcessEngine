using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDPackageInstance
    {
        private EMDObjectContainer obco = null;
        private EMDObjectRelation obre = null;


        public EMDObjectContainer Package
        {
            get { return obco; }
            set { obco = value; }
        }
        public EMDObjectRelation ObjectRelation
        {
            get { return obre; }
            set { obre = value; }
        }

        public string ObjectRelationGuid { get; set; }
        public string PackageName { get; set; }
        public int PackageStatus { get; set; }
        public string PackageDescription { get; set; }

        public EMDPackageInstance()
        {

        }
        public EMDPackageInstance(EMDObjectContainer obco, EMDObjectRelation obre)
        {
            this.obco = obco;
            this.obre = obre;
        }
    }
}
