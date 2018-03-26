using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Logic;

namespace Kapsch.IS.EDP.Core.Entities.Enhanced
{
    public class EMDPersonEmployment
    {
        public EMDEmployment Empl { get; set; }
        public EMDPerson Pers { get; set; }
        //public int EP_ID { get; set; }
        //public int P_ID { get; set; }
        //public string Ente_Guid { get; set; }
        //public string Pers_Guid { get; set; }
        //public string Empl_Guid { get; set; }
        //public string FamilyName { get; set; }
        //public string FirstName { get; set; }
        //public string Display_FamilyName { get; set; }
        //public string Display_FirstName { get; set; }
        //public string DegreePrefix { get; set; }
        //public string DegreeSuffix { get; set; }
        //public string UserID { get; set; }
        //public string MainMail { get; set; }
        //public string EnterpriseName { get; set; }
        public string FullDisplayNameWithUserIdAndPersNr { get; set; }



        public EMDPersonEmployment()
        {

        }

        public EMDPersonEmployment(EMDPerson pers, EMDEmployment empl)
        {
            this.Empl = empl;
            this.Pers = pers;
            //FullDisplayNameWithUserIdAndPersNr = pers.Display_FamilyName + " " + pers.Display_FirstName;
            this.FullDisplayNameWithUserIdAndPersNr = PersonManager.getFullDisplayNameWithUserIdAndPersNr(pers, empl);

        }
    }
}

