using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class OrgUnitRoleHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new OrgUnitRole().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            OrgUnitRole ouro = (OrgUnitRole)dbObject;
            EMDOrgUnitRole emdObject = new EMDOrgUnitRole(ouro.Guid, ouro.Created, ouro.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public IEMDObject<EMDOrgUnitRole> GetOrgUnitRole(string Employment_Guid, string Role_Guid, Boolean Valid = true)
        {
            List<IEMDObject<EMDOrgUnitRole>> orgUnitRoles = GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid = \"" + Employment_Guid + "\" and R_Guid=\"" + Role_Guid + "\"");

            if (orgUnitRoles.Count == 0)
                return null;
            else if (orgUnitRoles.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one Orgunit-Role found for EP_Guid {0} and R_ID {1}", Employment_Guid, Role_Guid));
            }
            else
            {
                return orgUnitRoles.First();
            }
        }

        //public List<EMDOrgUnitRole> GetOrgUnitRoleList(string Employment_Guid, string Role_Guid, Boolean Valid = true)
        //{
        //    EMD_DataBase dbContext = new EMD_DataBase();
        //    List<OrgUnitRole> dbOrgUnitRoles;
        //    if (Valid)
        //        dbOrgUnitRoles = (from item in dbContext.OrgUnitRole.Where(this.ValidClause) where item.EP_Guid == Employment_Guid && item.R_Guid == Role_Guid select item).ToList();
        //    else
        //        dbOrgUnitRoles = (from item in dbContext.OrgUnitRole select item).ToList();

        //    return ReadDBOrgUnitRoles(dbOrgUnitRoles);
        //}


        //private List<EMDOrgUnitRole> ReadDBOrgUnitRoles(List<OrgUnitRole> dbOrgUnitRoles)
        //{
        //    List<EMDOrgUnitRole> ListOrgUnitRole = new List<EMDOrgUnitRole>();
        //    foreach (OrgUnitRole dbOrgUnitRole in dbOrgUnitRoles)
        //    {
        //        EMDOrgUnitRole orgUnitRole = this.ReadDBOrgUnitRole(dbOrgUnitRole);
        //        ListOrgUnitRole.Add(orgUnitRole);
        //    }
        //    return ListOrgUnitRole;
        //}

        //private EMDOrgUnitRole ReadDBOrgUnitRole(OrgUnitRole dbOrgUnitRole)
        //{
        //    if (dbOrgUnitRole != null)
        //    {
        //        EMDOrgUnitRole orgUnitRole = new EMDOrgUnitRole(dbOrgUnitRole.Guid, dbOrgUnitRole.Created, dbOrgUnitRole.Modified);

        //        orgUnitRole.EP_Guid = dbOrgUnitRole.EP_Guid;
        //        orgUnitRole.EP_ID = dbOrgUnitRole.EP_ID;
        //        orgUnitRole.OR_ID = dbOrgUnitRole.OR_ID;
        //        orgUnitRole.O_Guid = dbOrgUnitRole.O_Guid;
        //        orgUnitRole.O_ID = dbOrgUnitRole.O_ID;
        //        orgUnitRole.R_Guid = dbOrgUnitRole.R_Guid;
        //        orgUnitRole.R_ID = dbOrgUnitRole.R_ID;
        //        orgUnitRole.ValidFrom = dbOrgUnitRole.ValidFrom;
        //        orgUnitRole.ValidTo = dbOrgUnitRole.ValidTo;
        //        orgUnitRole.SetStatus();

        //        return orgUnitRole;
        //    }
        //    else
        //        return null;

        //}


        //public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDOrgUnitRole orgUnitRole = (EMDOrgUnitRole)Object;
        //    OrgUnitRole dbObject = (from item in transaction.dbContext.OrgUnitRole where item.Guid == orgUnitRole.Guid select item).FirstOrDefault();
        //    if (dbObject != null)
        //    {
        //        MapDataToDBObject(ref dbObject, ref orgUnitRole);

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
        //    EMDOrgUnitRole orgUnitRole = (EMDOrgUnitRole)Object;
        //    OrgUnitRole dbObject = (from item in transaction.dbContext.OrgUnitRole where item.Guid == orgUnitRole.Guid select item).FirstOrDefault();
        //    if (dbObject == null)
        //    {
        //        dbObject = new OrgUnitRole();

        //        MapDataToDBObject(ref dbObject, ref orgUnitRole);

        //        //finally write to db
        //        transaction.dbContext.OrgUnitRole.Add(dbObject);
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //private void MapDataToDBObject(ref OrgUnitRole dbObject, ref EMDOrgUnitRole orgUnitRole)
        //{
        //    dbObject.Guid = orgUnitRole.Guid; 

        //    dbObject.ValidFrom = orgUnitRole.ValidFrom; 
        //    dbObject.ValidTo = orgUnitRole.ValidTo;
        //    dbObject.Modified = orgUnitRole.Modified;

        //    dbObject.Created = orgUnitRole.Created; 

        //    dbObject.EP_Guid = orgUnitRole.EP_Guid;
        //    dbObject.EP_ID = orgUnitRole.EP_ID;
        //    dbObject.OR_ID = orgUnitRole.OR_ID;
        //    dbObject.O_Guid = orgUnitRole.O_Guid;
        //    dbObject.O_ID = orgUnitRole.O_ID;
        //    dbObject.R_Guid = orgUnitRole.R_Guid;
        //    dbObject.R_ID = orgUnitRole.R_ID;
        //    dbObject.ValidFrom = orgUnitRole.ValidFrom;
        //    dbObject.ValidTo = orgUnitRole.ValidTo;
        //}        
    }
}