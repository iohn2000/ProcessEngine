using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Logic;

namespace EDP20Core.Test
{
    public static class Generate
    {
        public static Randomizer rand = new Randomizer();

        public static string testuser = "wie";

        private static readonly string[] EmdAccessForms = { "ONBOARDING", "EMD104", "CHANGE", "ADVANCEDSEARCH", "OFFBOARDING", "PICTUREMANAGER" };
        public static readonly Faker<EMDAccess> EmdAccess = new Faker<EMDAccess>()
            .StrictMode(false)
            .Rules((faker, acce) =>
            {
                acce.UserId = faker.Person.UserName;
                acce.E_Guid = null;
                acce.Form = faker.PickRandom(EmdAccessForms);
                acce.Note = faker.Lorem.Sentence();
            });

        public static EMDEmployment GetTestEmployment()
        {
            PersonManager pmgr = new PersonManager();
            EMDPerson pers = pmgr.GetPersonByUserId(testuser);

            EmploymentManager emgr = new EmploymentManager();
                
            EMDEmployment empl = emgr.GetMainEploymentForPerson(personGuid: pers.Guid);

            return empl;
        }

        internal static List<string> GetEmploymentsToTest()
        {
            List<string> employmentGuids = new List<string>();
            employmentGuids.Add(GetEmploymentWithNumberInUsername());
            employmentGuids.Add(GetTestEmployment().Guid);
            return employmentGuids;
        }
        private static string GetEmploymentWithNumberInUsername()
        {
            UserHandler uh = new UserHandler();
            EMD_Entities emdEntities = uh.transaction.dbContext;

            User user = (from u in emdEntities.User
                                 where
                                 u.Username.EndsWith("2")
                                 select u)
                       .FirstOrDefault();

            return user.EMPL_Guid;
        }
        
    }

}
