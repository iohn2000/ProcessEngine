using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class GenderModel
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public GenderModel(string name, string key)
        {
            this.Name = name;
            this.Key = key;
        }
    }
}