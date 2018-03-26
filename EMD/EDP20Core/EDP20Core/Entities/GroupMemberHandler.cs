using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class GroupMemberHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new GroupMember().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            GroupMember grme = (GroupMember)dbObject;
            EMDGroupMember emdObject = new EMDGroupMember(grme.Guid, grme.Created, grme.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        //public override IEMDObject GetObject(String guid)
        //{
        //    EMD_DataBase dbContext = new EMD_DataBase();

        //    GroupMember dbGroupMember = (from item in dbContext.GroupMember where item.Guid == guid select item).FirstOrDefault();
        //    EMDGroupMember groupMember = this.ReadDBGroupMember(dbGroupMember);
        //    return groupMember;
        //}

        //private EMDGroupMember ReadDBGroupMember(GroupMember dbGroup)
        //{
        //    EMDGroupMember groupMember = new EMDGroupMember(dbGroup.Guid, dbGroup.Created, dbGroup.Modified);

        //    groupMember.EP_Guid = dbGroup.EP_Guid;
        //    groupMember.EP_ID = dbGroup.EP_ID;
        //    groupMember.GM_ID = dbGroup.GM_ID;
        //    groupMember.G_Guid = dbGroup.G_Guid;
        //    groupMember.G_ID = dbGroup.G_ID;
        //    groupMember.ValidFrom = dbGroup.ValidFrom;
        //    groupMember.ValidTo = dbGroup.ValidTo;
        //    groupMember.SetStatus();

        //    return groupMember;
        //}

        //public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDGroupMember groupMember = (EMDGroupMember)Object;
        //    //TODO wenn historizable dann seetze Datensatz mit person.guid auf ValidTo = Now 
        //    // - erzeuge neues Datenobjekt mit neuer Guid (inl validF und ValidTo) 
        //    // - befülle Datensatz neu

        //    GroupMember dbObject = (from item in transaction.dbContext.GroupMember where item.Guid == groupMember.Guid select item).FirstOrDefault();
        //    if (dbObject != null)
        //    {
        //        MapDataToDBObject(ref dbObject, ref groupMember);

        //        //finally write to db
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //public override void InsertDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDGroupMember groupMember = (EMDGroupMember)Object;
        //    GroupMember dbObject = (from item in transaction.dbContext.GroupMember where item.Guid == groupMember.Guid select item).FirstOrDefault();
        //    if (dbObject == null)
        //    {
        //        dbObject = new GroupMember();

        //        MapDataToDBObject(ref dbObject, ref groupMember);

        //        //finally write to db
        //        transaction.dbContext.GroupMember.Add(dbObject);
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //private void MapDataToDBObject(ref GroupMember dbObject, ref EMDGroupMember groupMember)
        //{
        //    dbObject.Guid = groupMember.Guid;
        //    dbObject.ValidFrom = groupMember.ValidFrom;
        //    dbObject.ValidTo = groupMember.ValidTo;
        //    dbObject.Modified = groupMember.Modified;
        //    dbObject.Created = groupMember.Created;

        //    dbObject.EP_Guid = groupMember.EP_Guid;
        //    dbObject.EP_ID = groupMember.EP_ID;
        //    dbObject.GM_ID = groupMember.GM_ID;
        //    dbObject.G_Guid = groupMember.G_Guid;
        //    dbObject.G_ID = groupMember.G_ID;
        //}
    }
}
