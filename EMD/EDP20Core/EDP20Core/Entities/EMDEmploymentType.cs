using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEmploymentType : EMDObject<EMDEmploymentType>
    {
        public int ET_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public Nullable<int> CheckIntervalInDays { get; set; }

        public Nullable<int> ReminderIntervalInDays { get; set; }
        public int ETC_ID { get; set; }
        public string EMailSign { get; set; }

        public string NameShort { get; set; }

        public override String Prefix { get { return "EMTY"; } }


        /// <summary>
        /// Returns true if the Employment with a configured EmploymentType requires a SponsorGuid
        /// </summary>
        public bool MustHaveSponsor
        {
            get
            {
                return CheckIntervalInDays.HasValue && CheckIntervalInDays > 0;
            }
        }

        public bool DoEntityCheck
        {
            get
            {
                return CheckIntervalInDays.HasValue && CheckIntervalInDays.Value > 0;
            }
        }

        public EMDEmploymentType(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEmploymentType()
        { }
    }
}
