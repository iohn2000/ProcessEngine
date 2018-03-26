using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EMD.EMD20Web.EMD.Data
{
    public class EmploymentPerson
    {
        public EMDPerson person { get; set; }

        public EMDEmployment employment { get; set; }

        public EmploymentPerson(string empl_guid)
        {
            EmploymentHandler emplHandler = new EmploymentHandler();
            employment = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(empl_guid);
            if (employment != null)
            {
                PersonHandler persHandler = new PersonHandler();
                person = (EMDPerson)persHandler.GetObject<EMDPerson>(employment.P_Guid);
            }
            

        }
    }
}