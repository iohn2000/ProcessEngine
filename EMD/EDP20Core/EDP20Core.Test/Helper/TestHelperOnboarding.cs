using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;

namespace EDP20Core.Test.Helper
{
    class TestHelperOnboarding
    {
        public static void PrepareOnboarding(OnboardingManager onboardingManager, EMDEmployment eMDEmployment, EMDPerson eMDPerson)
        {
            onboardingManager.PrepareOnboarding(
                            requestingPersonEmplGuid: "PERS_c5ca7e0baf66405f95b5a2410ba895b7",
                            empl: eMDEmployment,
                            effectedPersonGuid: eMDPerson.Guid,
                            enteGuid: "ENTE_8fe2d7b78871446c9a6683a6d2ecc1fb",
                            locaGuid: "LOCA_474a999adf464e0083240f67b3edfe16",
                            accoGuid: "ACCO_e5ffae06c6a746dfa4ba6749b182fa2d",
                            orguGuid: "ORGU_17997e8364684679bac852bcb2f70021",
                            emtyGuid: "EMTY_82d3847b57ea4be1a0212872fcaf8ef8",
                            userdomainGuid: "USDO_fb1fe73f74c24c7393e480bca4162003",
                            digrGuid: "DIST_76dbad87706241caa8158520450516b3",
                            sponsorGuid: "PERS_c5ca7e0baf66405f95b5a2410ba895b7",
                            emailType: "intern",
                            contactList: null,
                            xmlData: null,
                            newEquipments: null,
                            leaveFrom: null,
                            leaveTo: null,
                            oldEmplChangeExit: null,
                            businessCase: EnumBusinessCase.Onboarding);
        }
    }
}
