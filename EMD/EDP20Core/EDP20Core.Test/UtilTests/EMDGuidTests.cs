using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDP20Core.Test.UtilTests
{
    [TestFixture, Category("UtilTests"), Category("EMDGuidTests")]
    public class EMDGuidTests
    {
        [TestCase("CONT_00040584d88f4c32af664c176d957440", "CONT", "00040584d88f4c32af664c176d957440")]
        [TestCase("EMPL_0057618823f34d6b91e238bb5831d407", "EMPL", "0057618823f34d6b91e238bb5831d407")]
        [TestCase("LOCA_0844ba3d71e740919ff374a7f9900334", "LOCA", "0844ba3d71e740919ff374a7f9900334")]
        public void ParseValidGuidsTest(string validGuid, string expectedPrefix, string expectedGuid)
        {
            EMDGuid toTest = new EMDGuid(validGuid);

            Assert.Multiple(() =>
            {
                Assert.That(toTest.Prefix, Is.EqualTo(expectedPrefix));
                Assert.That(toTest.Guid, Is.EqualTo(expectedGuid));
            });
        }

        [TestCase("CONT00040584d88f4c32af664c176d957440")]       //no separator
        [TestCase("CONT.00040584d88f4c32af664c176d957440")]      //false separator
        [TestCase("00040584d88f4c32af664c176d957440")]           //no prefix
        [TestCase("CONT_040584d88f4c32af664c176d957440")]        //short guid
        [TestCase("CONT_00040584d88f4c32af664c176d95744x")]      //non-hexadecimal digits
        [TestCase("CONT_00040584d88f4c32af664c176d95744440")]    //to long
        public void ParseInvalidGuidsTest(string invalidGuid)
        {
            Assert.That(() => new EMDGuid(invalidGuid), Throws.InstanceOf(typeof(GuidCastException)));
        }

        [TestCase("CONT_00040584d88f4c32af664c176d957440", "CONT", "00040584d88f4c32af664c176d957440")]
        [TestCase("EMPL_0057618823f34d6b91e238bb5831d407", "EMPL", "0057618823f34d6b91e238bb5831d407")]
        [TestCase("LOCA_0844ba3d71e740919ff374a7f9900334", "LOCA", "0844ba3d71e740919ff374a7f9900334")]
        public void ImplicitCastValidGuidsTest(string validGuid, string expectedPrefix, string expectedGuid)
        {
            EMDGuid toTest = validGuid;

            Assert.Multiple(() =>
            {
                Assert.That(toTest.Prefix, Is.EqualTo(expectedPrefix));
                Assert.That(toTest.Guid, Is.EqualTo(expectedGuid));
            });
        }

        [TestCase("CONT00040584d88f4c32af664c176d957440")]       //no separator
        [TestCase("CONT.00040584d88f4c32af664c176d957440")]      //false separator
        [TestCase("00040584d88f4c32af664c176d957440")]           //no prefix
        [TestCase("CONT_040584d88f4c32af664c176d957440")]        //to short
        [TestCase("CONT_00040584d88f4c32af664c176d95744x")]      //non-hexadecimal digits
        [TestCase("CONT_00040584d88f4c32af664c176d95744440")]    //to long
        public void ImplicitCastInvalidGuidsTest(string invalidGuid)
        {
            Assert.That(() => { EMDGuid guid = invalidGuid; }, Throws.InstanceOf(typeof(GuidCastException)));
        }

        [TestCase("CONT_00040584d88f4c32af664c176d957440")]
        [TestCase("EMPL_0057618823f34d6b91e238bb5831d407")]
        [TestCase("LOCA_0844ba3d71e740919ff374a7f9900334")]
        public void ToStringTest(string validGuid)
        {
            EMDGuid toTest = new EMDGuid(validGuid);
            Assert.That(toTest.ToString(), Is.EqualTo(validGuid)); //String format mustn't be altered by the class
        }

        [TestCase("CONT_00040584d88f4c32af664c176d957440")]
        [TestCase("EMPL_0057618823f34d6b91e238bb5831d407")]
        [TestCase("LOCA_0844ba3d71e740919ff374a7f9900334")]
        public void ImplicitToStringTest(string validGuid)
        {
            EMDGuid toTest = new EMDGuid(validGuid);
            string guid = toTest;
            Assert.That(guid, Is.EqualTo(validGuid)); //String format mustn't be altered by the class
        }
    }
}
