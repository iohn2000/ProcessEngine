using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EmploymentAccountHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new EmploymentAccount().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            EmploymentAccount emac = (EmploymentAccount)dbObject;
            EMDEmploymentAccount emdObject = new EMDEmploymentAccount(emac.Guid, emac.Created, emac.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        //public List<IEMDObject> GetObjectsForEmployment(String EP_Guid, Boolean Valid = true)
        //{
        //    DB.EMD_DataBase dbContext = new DB.EMD_DataBase();

        //    List<DB.EmploymentAccount> dbEmploymentAccount; 
        //    if (Valid)
        //        dbEmploymentAccount  = (from item in dbContext.EmploymentAccount.Where(this.ValidClause) where item.EP_Guid == EP_Guid select item).ToList();
        //    else
        //        dbEmploymentAccount = (from item in dbContext.EmploymentAccount where item.EP_Guid == EP_Guid select item).ToList();

        //    return ReadDBEmploymentAccounts(dbEmploymentAccount);
        //}

        public IEMDObject<EMDEmploymentAccount> GetMainEmploymentAccount(String EP_Guid)
        {
            List<IEMDObject<EMDEmploymentAccount>> employmentAccounts = GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + EP_Guid + "\"");

            if (employmentAccounts.Count == 0)
                return null;
            else if (employmentAccounts.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one Main-Account found for Employment EP_Guid: {0}",EP_Guid));
            }
            else
            {
                return employmentAccounts.First();
            }
        }


        //private List<IEMDObject> ReadDBEmploymentAccounts(List<EmploymentAccount> dbEmploymentAccounts)
        //{
        //    List<IEMDObject> ListObjects = new List<IEMDObject>();
        //    foreach (DB.EmploymentAccount item in dbEmploymentAccounts)
        //    {
        //        EMDEmploymentAccount employmentaccount = this.ReadDBEmploymentAccount(item);
        //        ListObjects.Add(employmentaccount);
        //    }
        //    return ListObjects;
        //}



        //private EMDEmploymentAccount ReadDBEmploymentAccount(EmploymentAccount dbEmploymentAccount)
        //{
        //    EMDEmploymentAccount employmentaccount = new EMDEmploymentAccount(dbEmploymentAccount.Guid, dbEmploymentAccount.Created, dbEmploymentAccount.Modified);

        //    employmentaccount.AC_Guid = dbEmploymentAccount.AC_Guid;
        //    employmentaccount.AC_ID = dbEmploymentAccount.AC_ID;
        //    employmentaccount.EPA_ID = dbEmploymentAccount.EPA_ID;
        //    employmentaccount.EP_Guid = dbEmploymentAccount.EP_Guid;
        //    employmentaccount.EP_ID = dbEmploymentAccount.EP_ID;
        //    employmentaccount.Guid = dbEmploymentAccount.Guid;
        //    employmentaccount.Main = dbEmploymentAccount.Main;
        //    employmentaccount.Percent = dbEmploymentAccount.Percent;
        //    employmentaccount.ValidFrom = dbEmploymentAccount.ValidFrom;
        //    employmentaccount.ValidTo = dbEmploymentAccount.ValidTo;
        //    employmentaccount.SetStatus();

        //    return employmentaccount;
        //}



        //public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDEmploymentAccount employmentAccount = (EMDEmploymentAccount)Object;
        //    EmploymentAccount dbObject = (from item in transaction.dbContext.EmploymentAccount where item.Guid == employmentAccount.Guid select item).FirstOrDefault();
        //    if (dbObject != null)
        //    {
        //        MapDataToDBObject(ref dbObject, ref employmentAccount);

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
        //    EMDEmploymentAccount employmentAccount = (EMDEmploymentAccount)Object;
        //    EmploymentAccount dbObject = (from item in transaction.dbContext.EmploymentAccount where item.Guid == employmentAccount.Guid select item).FirstOrDefault();
        //    if (dbObject == null)
        //    {
        //        dbObject = new EmploymentAccount();

        //        MapDataToDBObject(ref dbObject, ref employmentAccount);

        //        //finally write to db
        //        transaction.dbContext.EmploymentAccount.Add(dbObject);
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //private void MapDataToDBObject(ref EmploymentAccount dbObject, ref EMDEmploymentAccount employmentAccount)
        //{
        //    dbObject.Guid = employmentAccount.Guid; 

        //    dbObject.ValidFrom = employmentAccount.ValidFrom; 
        //        dbObject.ValidTo = employmentAccount.ValidTo;
        //        dbObject.Modified = employmentAccount.Modified;
        //    dbObject.Created = employmentAccount.Created;

        //        dbObject.AC_Guid = employmentAccount.AC_Guid;
        //        dbObject.AC_ID = employmentAccount.AC_ID;
        //        dbObject.Created = employmentAccount.Created;
        //        dbObject.EPA_ID = employmentAccount.EPA_ID;
        //        dbObject.EP_Guid = employmentAccount.EP_Guid;
        //        dbObject.EP_ID = employmentAccount.EP_ID;
        //        dbObject.Main = employmentAccount.Main;
        //        dbObject.Percent = employmentAccount.Percent;
        //}                
    }
    }
