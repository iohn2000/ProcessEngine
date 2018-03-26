using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.JobSchedulerWeb.Models
{
    public class IntervalUnitModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public IntervalUnitModel(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public IntervalUnitModel()
        {
            this.Id = String.Empty;
            this.Name = String.Empty;
        }
    }
}