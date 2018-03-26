using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Filter
{
    public class FilterCriteria
    {
        public string Company { get; set; }
        public string Location { get; set; }
        public string CostCenter { get; set; }
        public string EmploymentType { get; set; }
        public List<string> UserTypeIds { get; set; }


        public FilterCriteria()
        {
            this.Company = "";
            this.Location = "";
            this.CostCenter = "";
            this.EmploymentType = "";
            this.UserTypeIds = new List<string>();
        }

        public FilterCriteria(string company, string location, string costCenter, string employmentType)
        {
            this.Company = company;
            this.Location = location;
            this.CostCenter = costCenter;
            this.EmploymentType = employmentType;
            this.UserTypeIds = new List<string>();
        }

        public FilterCriteria(string company, string location, string costCenter, string employmentType, List<string> userTypeGuids)
        {
            this.Company = company;
            this.Location = location;
            this.CostCenter = costCenter;
            this.EmploymentType = employmentType;
            this.UserTypeIds = userTypeGuids;
        }

        public override string ToString()
        {
            return string.Format("Company:'{0}' Location:'{1}' CostCenter:'{2}' EmploymentType:'{3}' UserTypeGuid:'{4}'",
                this.Company, this.Location, this.CostCenter, this.EmploymentType, string.Join(",", this.UserTypeIds));
        }
    }
}
