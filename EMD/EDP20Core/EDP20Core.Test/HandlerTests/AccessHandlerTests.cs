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
    [TestFixture, Category("HandlerTests"), Category("AccessHandlerTests")]
    public class AccessHandlerTests
    {
        [Test]
        public void GetObjectsTest()
        {
            AccessHandler acceHandler = new AccessHandler();
            List<IEMDObject<EMDAccess>> res = acceHandler.GetObjects<EMDAccess, Access>();

            Assert.That(res, Is.All.InstanceOf<EMDAccess>());
            Assert.Multiple(() =>
            {
                Assert.That(res, Is.All.Matches<EMDAccess>(acce => acce.ValidFrom < DateTime.Now));
                Assert.That(res, Is.All.Matches<EMDAccess>(acce => acce.ActiveFrom < DateTime.Now));

                Assert.That(res, Is.All.Matches<EMDAccess>(acce => acce.ValidTo > DateTime.Now));
                Assert.That(res, Is.All.Matches<EMDAccess>(acce => acce.ActiveTo > DateTime.Now));
            });
        }

        [Test]
        public void ReadWriteTest()
        {
            AccessHandler acceHandler = new AccessHandler();

            EMDAccess test = Generate.EmdAccess;

            EMDAccess written = (EMDAccess)acceHandler.CreateObject(test);

            EMDAccess read = (EMDAccess)acceHandler.GetObject<EMDAccess>(written.Guid);

            Assert.Multiple(() =>
            {
                Assert.That(read.Guid, Is.EqualTo(read.Guid));
                Assert.That(read.Form, Is.EqualTo(test.Form));

                Assert.That(read.ValidFrom, Is.LessThan(DateTime.Now));
                Assert.That(read.ActiveFrom, Is.LessThan(DateTime.Now));

                Assert.That(read.ValidTo, Is.GreaterThan(DateTime.Now));
                Assert.That(read.ActiveTo, Is.GreaterThan(DateTime.Now));
            });
        }
    }
}
