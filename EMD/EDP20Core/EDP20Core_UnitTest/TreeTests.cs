using System;
using System.Collections.Generic;

using System.Linq;
using System.Data.SqlClient;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;


//using Kendo.Mvc;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Reflection;
using Kapsch.IS.Util.ErrorHandling;
using System.Diagnostics;
using System.Collections;
using Kapsch.IS.EDP.Core.WF.Message;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class TreeTests
    {
        [TestMethod]
        [TestCategory("Trees")]
        public void TestEntityTree()
        {

            String key = "ENTE_";
            EMDEnterprise e1 = new EMDEnterprise(key + "e1", DateTime.Now, DateTime.Now);
            EMDEnterprise e2 = new EMDEnterprise(key + "e2", DateTime.Now, DateTime.Now);
            EMDEnterprise e3 = new EMDEnterprise(key + "e3", DateTime.Now, DateTime.Now);
            EMDEnterprise e4 = new EMDEnterprise(key + "e4", DateTime.Now, DateTime.Now);
            EMDEnterprise e5 = new EMDEnterprise(key + "e5", DateTime.Now, DateTime.Now);
            EMDEnterprise e6 = new EMDEnterprise(key + "e6", DateTime.Now, DateTime.Now);
            EMDEnterprise e7 = new EMDEnterprise(key + "e7", DateTime.Now, DateTime.Now);
            EMDEnterprise e8 = new EMDEnterprise(key + "e8", DateTime.Now, DateTime.Now);
            EMDEnterprise e9 = new EMDEnterprise(key + "e9", DateTime.Now, DateTime.Now);
            EMDEnterprise e10 = new EMDEnterprise(key + "e10", DateTime.Now, DateTime.Now);

            //now create the tree
            // e1
            // |
            // e2------e3-e4
            // |       |  |
            // e5      e6 e9
            // |          |
            // e7-e8      e10

            EntityTree<EMDEnterprise> tree = new EntityTree<EMDEnterprise>();

            tree.Root = new EntityTreeNode<EMDEnterprise>(e1);

            EntityTreeNode<EMDEnterprise> ee10 = new EntityTreeNode<EMDEnterprise>(e10, null, null);
            EntityTreeNode<EMDEnterprise> ee9 = new EntityTreeNode<EMDEnterprise>(e9, ee10, null);
            EntityTreeNode<EMDEnterprise> ee4 = new EntityTreeNode<EMDEnterprise>(e4, ee9, null);

            EntityTreeNode<EMDEnterprise> ee6 = new EntityTreeNode<EMDEnterprise>(e6, null, null);
            EntityTreeNode<EMDEnterprise> ee3 = new EntityTreeNode<EMDEnterprise>(e3, ee6, ee4);

            EntityTreeNode<EMDEnterprise> ee8 = new EntityTreeNode<EMDEnterprise>(e8, null, null);
            EntityTreeNode<EMDEnterprise> ee7 = new EntityTreeNode<EMDEnterprise>(e7, null, ee8);
            EntityTreeNode<EMDEnterprise> ee5 = new EntityTreeNode<EMDEnterprise>(e5, ee7, null);
            EntityTreeNode<EMDEnterprise> ee2 = new EntityTreeNode<EMDEnterprise>(e2, ee5, ee3);

            tree.Root.FirstChild = ee2;

            //done

            //Test GetAllChildren
            List<EMDEnterprise> el = tree.GetAllChildrenOf(e2.Guid);
            Assert.AreEqual(3, el.Count);

            el = tree.GetAllChildrenOf(e10.Guid);
            Assert.AreEqual(0, el.Count);

            el = tree.GetAllChildrenOf(e7.Guid);
            Assert.AreEqual(0, el.Count);

            el = tree.GetAllChildrenOf(e3.Guid);
            Assert.AreEqual(1, el.Count);

            el = tree.GetAllChildrenOf(e4.Guid);
            Assert.AreEqual(2, el.Count);

            //Test GetLastSibling
            EntityTreeNode<EMDEnterprise> r = ee10.GetLastSibling();
            Assert.AreEqual(ee10.Entity.Guid, r.Entity.Guid);

            r = ee2.GetLastSibling();
            Assert.AreEqual(ee4.Entity.Guid, r.Entity.Guid);

            r = tree.Root.GetLastSibling();
            Assert.AreEqual(tree.Root.Entity.Guid, r.Entity.Guid);

            //Test isParentOf
            Assert.IsTrue(tree.Root.IsParentOf(ee3.Entity.Guid));

            Assert.IsFalse(ee10.IsParentOf(ee3.Entity.Guid));

            Assert.IsFalse(ee10.IsParentOf(null));
        }

        [TestMethod]
        [TestCategory("Trees")]
        public void TestEnterpriseTree()
        {
            EnterpriseHandler eh = new EnterpriseHandler();
            //TODO KGB-AT should be found via Name not via Guid....
            EMDEnterprise kgb = (EMDEnterprise)eh.GetObject<EMDEnterprise>("ENTE_1c0b8655bf834740b65a4931885bec2a");

            EnterpriseTree et = new EnterpriseTree();
            et.Fill(kgb);
            Assert.AreEqual(et.EnterpriseList.Count, et.GetAllChildrenOf(kgb.Guid).Count+1);

            Trace.WriteLine("Found " + et.GetAllChildrenOf(kgb.Guid).Count + " Elements");

            String output = et.Print();
            System.Diagnostics.Trace.Write(output);

            List<EMDEnterprise> el = et.GetAllChildrenOf("ENTE_1c0b8655bf834740b65a4931885bec2a");
            Assert.AreNotEqual(0, el.Count);
        }

        [TestMethod]
        [TestCategory("Trees")]
        public void TestOrgUnitTree()
        {
            OrgUnitHandler oh = new OrgUnitHandler();
            //TODO KGB-AT should be found via Name not via Guid....
            EMDOrgUnit oben = (EMDOrgUnit)oh.GetObject<EMDOrgUnit>("ORGU_561a500d61d24e3593bfecf2bec8efbc");

            OrgUnitTree ot = new OrgUnitTree();
            ot.Fill(oben);
            Assert.AreEqual(ot.OrgUnitList.Count, ot.GetAllChildrenOf(oben.Guid).Count + 1);

            Trace.WriteLine("Found " + ot.GetAllChildrenOf(oben.Guid).Count + " Elements");

            String output = ot.Print();
            System.Diagnostics.Trace.Write(output);

            List<EMDOrgUnit> ol = ot.GetAllChildrenOf("ORGU_561a500d61d24e3593bfecf2bec8efbc");
            Assert.AreNotEqual(0, ol.Count);

        }
    }
}
