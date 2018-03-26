using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.EDPExports.Entities
{
    public class EDPDataForIT
    {
        public string Status { get; set; }
        public string UserID { get; set; }
        public string UserType { get; set; }
        public string UserStatus { get; set; }
        public string FirstName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public int ObjID { get; set; }
        public string CompanyShortName { get; set; }
        public int EmploymentTypeID { get; set; }
        public string Direct { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string EFax { get; set; }
        public string Room { get; set; }
        public string PersonalNr { get; set; }
        public int PersonID { get; set; }
        public int EmploymentID { get; set; }
        public string Gender { get; set; }
        public DateTime created { get; set; }

        public EDPDataForIT()
        {

        }
    }
}
