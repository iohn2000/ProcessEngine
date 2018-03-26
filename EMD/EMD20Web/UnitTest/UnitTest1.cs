using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Kendo.Mvc;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PrintFilterOperators()
        {

            FilterOperator fo = new FilterOperator();
            System.Diagnostics.Debug.WriteLine(fo.ToString());

            FilterDescriptor fd = new FilterDescriptor("t", FilterOperator.DoesNotContain, "b");
            

        }
    }
}
