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
    public class RoleHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new Role().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Role role = (Role)dbObject;
            EMDRole emdObject = new EMDRole(role.Guid, role.Created, role.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public const int DISCIPLINALROLES = 10000;
        public const int PERSON = 10100;
        public const int TEAMLEADER = 10400;
        public const int LINEMANAGER = 10500;


        public IEMDObject<EMDRole> GetRoleById(int Role_Id)
        {
            List<IEMDObject<EMDRole>> roles = GetObjects<EMDRole, Role>("R_ID = " + Role_Id);

            if (roles.Count == 0)
                return null;
            else if (roles.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one role found for R_ID {0}", Role_Id));
            }
            else
            {
                return roles.First();
            }
        }



        //public List<EMDRole> GetRoleList(Boolean Valid = true)
        //{
        //    EMD_DataBase dbContext = new EMD_DataBase();
        //    List<Role> dbRoles;
        //    if (Valid)
        //        dbRoles = (from item in dbContext.Role.Where(this.ValidClause) select item).ToList();
        //    else
        //        dbRoles = (from item in dbContext.Role select item).ToList();

        //    return ReadDBRoles(dbRoles);
        //}


        //private List<EMDRole> ReadDBRoles(List<Role> dbRoles)
        //{
        //    List<EMDRole> ListRole = new List<EMDRole>();
        //    foreach (Role dbRole in dbRoles)
        //    {
        //        EMDRole role = this.ReadDBRole(dbRole);
        //        ListRole.Add(role);
        //    }
        //    return ListRole;
        //}

        //private EMDRole ReadDBRole(Role dbRole)
        //{
        //    EMDRole role = new EMDRole(dbRole.Guid, dbRole.Created, dbRole.Modified);

        //    role.DescriptionID = dbRole.DescriptionID;
        //    role.GroupNr = dbRole.GroupNr;
        //    role.Guid_Parent = dbRole.Guid_Parent;
        //    role.Guid_Root = dbRole.Guid_Root;
        //    role.ID_Parent = dbRole.ID_Parent;
        //    role.ID_Root = dbRole.ID_Root;
        //    role.Key1 = dbRole.Key1;
        //    role.Key2 = dbRole.Key2;
        //    role.Key3 = dbRole.Key3;
        //    role.Name = dbRole.Name;
        //    role.Priority = dbRole.Priority;
        //    role.R_ID = dbRole.R_ID;
        //    role.URL_Icon = dbRole.URL_Icon;
        //    role.ValidFrom = dbRole.ValidFrom;
        //    role.ValidTo = dbRole.ValidTo;
        //    role.SetStatus();

        //    return role;
        //}

        //public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDRole role = (EMDRole)Object;
        //    Role dbObject = (from item in transaction.dbContext.Role where item.Guid == role.Guid select item).FirstOrDefault();
        //    if (dbObject != null)
        //    {
        //        MapDataToDBObject(ref dbObject, ref role);

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
        //    EMDRole role = (EMDRole)Object;
        //    Role dbObject = (from item in transaction.dbContext.Role where item.Guid == role.Guid select item).FirstOrDefault();
        //    if (dbObject == null)
        //    {
        //        dbObject = new Role();

        //        MapDataToDBObject(ref dbObject, ref role);

        //        //finally write to db
        //        transaction.dbContext.Role.Add(dbObject);
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //private void MapDataToDBObject(ref Role dbObject, ref EMDRole role)
        //{
        //    dbObject.Guid = role.Guid; 
        //    dbObject.ValidFrom = role.ValidFrom; 
        //    dbObject.ValidTo = role.ValidTo;
        //    dbObject.Modified = role.Modified;
        //    dbObject.Created = role.Created; 

        //    dbObject.DescriptionID = role.DescriptionID;
        //    dbObject.GroupNr = role.GroupNr;
        //    dbObject.Guid_Parent = role.Guid_Parent;
        //    dbObject.Guid_Root = role.Guid_Root;
        //    dbObject.ID_Parent = role.ID_Parent;
        //    dbObject.ID_Root = role.ID_Root;
        //    dbObject.Key1 = role.Key1;
        //    dbObject.Key2 = role.Key2;
        //    dbObject.Key3 = role.Key3;
        //    dbObject.Name = role.Name;
        //    dbObject.Priority = role.Priority;
        //    dbObject.R_ID = role.R_ID;
        //    dbObject.URL_Icon = role.URL_Icon;
        //}        
    }
}
