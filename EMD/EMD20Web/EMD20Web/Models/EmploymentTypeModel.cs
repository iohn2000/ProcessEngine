using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EmploymentTypeModel
    {
        public string Guid { get; set; }
        public int ET_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public Nullable<int> CheckIntervalInDays { get; set; }
        public int ETC_ID { get; set; }
        public string EMailSign { get; set; }

        public static EmploymentTypeModel copyFromDBObject(EmploymentType emptype)
        {
            EmploymentTypeModel emptypemod = new EmploymentTypeModel();
            ReflectionHelper.CopyProperties(ref emptype, ref emptypemod);
            return emptypemod;
        }

        public static EmploymentTypeModel copyFromObject(EMDEmploymentType emptype)
        {
            EmploymentTypeModel emptypemo = new EmploymentTypeModel();
            ReflectionHelper.CopyProperties(ref emptype, ref emptypemo);
            return emptypemo;
        }
    }
}