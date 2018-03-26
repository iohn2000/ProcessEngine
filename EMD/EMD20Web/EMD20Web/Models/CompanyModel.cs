using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class CompanyModel
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Key { get; set; }
        public int Number { get; set; }

        public CompanyModel(string name, string shortName, string key, int number)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.Key = key;
            this.Number = number;
        }
    }
}