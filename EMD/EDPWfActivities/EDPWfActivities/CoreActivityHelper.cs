using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity
{
    public class CoreActivityHelper
    {

        public static Tuple<string, string, string, string> GetPersonDetails(string emplGuid)
        {
            PersonHandler persH = new PersonHandler();
            string persGuid = ((EMDEmployment) new EmploymentHandler().GetObject<EMDEmployment>(emplGuid)).P_Guid;
            EMDPerson pers = (EMDPerson) persH.GetObject<EMDPerson>(persGuid);
            return new Tuple<string, string, string, string>(pers.MainMail, pers.FirstName, pers.FamilyName, pers.Guid);
        }
    }
}
