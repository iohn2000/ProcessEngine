using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDContactType : EMDObject<EMDContactType>
    {
        const string SERVERKATCE_GUID = "COTY_08657cb3116b48899bf266153217e1d3";
        const string ISMDKATCE_GUID = "COTY_0d86b09274a9444795441a066bb90082";
        const string LOCATIONPHONE_GUID = "COTY_186805950507484c9bc9033887bea8ce";
        const string ROOM_GUID = "COTY_26a1bebc72004b82a87338a67e2e45f7";
        const string DIRECTDIAL2_GUID = "COTY_3c6b264d16ad446ba759cbca65e2c337";
        const string JOBTITLE_GUID = "COTY_517adae62b7e48e5a4c2ac368fff66cc";
        const string AGENDKATCE_GUID = "COTY_55d0a3c7bc064f90a49e7923b4bb51f6";
        const string DIRECTDIALKATCE_GUID = "COTY_5b61d44560304b1fb904789ee2938c99";
        const string MOBILE_GUID = "COTY_5f99a40a0d7247c5a2681777ea57bdce";
        const string PHONE_GUID = "COTY_82afdaa606e142088f915b1d66d38428";
        const string LOCATIONEMAIL_GUID = "COTY_9a3f7fcc46ff4134b0d9dbf6cba2c9bb";
        const string LOCATIONFAX_GUID = "COTY_9c4dd7a9a92b450c8a4d15f5250a0e5f";
        const string DIRECTDIAL_GUID = "COTY_9e73f884acb8493191927cd196fba1b3";
        const string EFAX_GUID = "COTY_a91f854b3a984080a5a790109d578958";
        const string FAX_GUID = "COTY_b834cd346afd4f8082e080bee00fc039";
        const string MONITORKATCE_GUID = "COTY_c52a2796a35740e09eaf28077254f529";
        const string DIRECTEFAX_GUID = "COTY_d1b546c07e3443b9b1729bb20a9f835d";
        const string EMAIL_GUID = "COTY_f5972ccf8458490da6ed388bba2aa8a2";
        const string HIERARCHY_GUID = "COTY_f7d3e7bcc7f549fd91b00528f9103a7f";


        public int CT_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Edit { get; set; }

        public override String Prefix { get { return "COTY"; } }
        
        public EMDContactType(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDContactType()
        { }
    }
}
