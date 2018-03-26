using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEmploymentPackage : EMDContentTemplate
    {

        public EMDEmploymentPackage(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        {
            Prefix = "EMPA";
            Key = this.GetType().ToString();
        }

        public EMDEmploymentPackage()
        {
            Prefix = "EMPA";
            Key = this.GetType().ToString();
        }

    }
}