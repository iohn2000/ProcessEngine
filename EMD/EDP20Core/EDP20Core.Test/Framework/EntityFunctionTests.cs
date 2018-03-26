using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;

namespace EDP20Core.Test.Framework
{
    [TestFixture, Category("EntityQueryTests")]
    public class EntityFunctionTests
    {

        [Test]
        public void CallEntityFunctionByException()
        {
            string queryString = @"""DemoFunction(parametercontent)@@EMPL_00000000000000000000000000000000""";

            EntityQuery eq = new EntityQuery();
            Type propType;
            var pid1 = eq.QueryMixedString(queryString, out propType);

            Assert.That(pid1, Is.EqualTo(@"""worked on parametercontent"""));

            //string queryString1 = @"""DemoFunction()@@EMPL_00000000000000000000000000000000""";

        }

        [Test]
        public void GetUserTypeName_Test()
        {
            string queryString = @"""GetUserTypeName(20)@@USER_00000000000000000000000000000000""";

            EntityQuery eq = new EntityQuery();
            Type propType;
            var pid1 = eq.QueryMixedString(queryString, out propType);

            Assert.That(pid1, Is.EqualTo(@"""ADUserFullAccount"""));
        }

        [Test]
        public void GetAccountGuidForEmployment_Test()
        {
            string testempl = Generate.GetTestEmployment().Guid;
            string queryString = @"""GetAccountGuidForEmployment(" + testempl + ")@@" + testempl + "\"";

            EntityQuery eq = new EntityQuery();
            Type propType;
            var pid1 = eq.QueryMixedString(queryString, out propType);

            Assert.That(pid1, Is.EqualTo(@"""ACCO_8e1a3caa0c114557ba506c93aefb119e"""));
        }


        [TestCase("Username@@USER_GUID@@P_Guid@@{0}")]
        public void EntityQueryTests(string query)
        {
            List<String> empls = Generate.GetEmploymentsToTest();
            foreach (String emplGuid in empls)
            {
                query = String.Format(query, emplGuid);
                EntityQuery eq = new EntityQuery();
                Type propType;
                String pid1 = (String)eq.QueryMixedString(query, out propType);
                TestContext.Out.WriteLine("Queryresult: "+pid1+" for query: "+query);
                Assert.That(!String.IsNullOrEmpty(pid1));
            }

        }
       
    }


}
