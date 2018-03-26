using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;

namespace EDP20Core.Test.HandlerTests
{
    [TestFixture, Category("HandlerTests"), Category("UserHandlerTests")]
    public class UserHandlerTests
    {
        [Test]
        public void GetUserTypeTest()
        {
            UserHandler userHandler = new UserHandler();
            string usertypename = userHandler.GetUserTypeName(string.Empty,Convert.ToInt32(EnumUserType.ADUserFullAccount).ToString());
            Assert.That(usertypename, Is.EqualTo("ADUserFullAccount"));

            usertypename = userHandler.GetUserTypeName(string.Empty, "999");
            Assert.That(usertypename, Is.EqualTo("UT_999"));

            usertypename = userHandler.GetUserTypeName(null, null);
            Assert.That(usertypename, Is.EqualTo(string.Empty));

        }
    }
}
