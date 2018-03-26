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
    public class EnterpriseHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new Enterprise().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Enterprise ente = (Enterprise)dbObject;
            EMDEnterprise emdObject = new EMDEnterprise(ente.Guid, ente.Created, ente.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        //public override IEMDObject GetObject(string guid)
        //{
        //    EMD_DataBase dbContext = new EMD_DataBase();

        //    Enterprise dbEnterprise = (from item in dbContext.Enterprise where item.Guid == guid select item).FirstOrDefault();
        //    EMDEnterprise enterprise = this.ReadDBEnterprise(dbEnterprise);
        //    return enterprise;
        //}

        //public List<IEMDHistorizableObject> GetEnterpriseList(bool valid = true)
        //{

        //    EMD_DataBase dbContext = new EMD_DataBase();

        //    List<Enterprise> ListEnterprise = new List<Enterprise>();
        //    if (valid)
        //        ListEnterprise = (from item in dbContext.Enterprise.Where(this.ValidClause) select item).ToList();
        //    else
        //        ListEnterprise = (from item in dbContext.Enterprise select item).ToList();

        //    return ReadDBEnterprises(ListEnterprise);
        //}

        //private List<IEMDHistorizableObject> ReadDBEnterprises(List<Enterprise> dbEnterprises)
        //{
        //    List<IEMDHistorizableObject> ListObjects = new List<IEMDHistorizableObject>();
        //    foreach (Enterprise item in dbEnterprises)
        //    {
        //        EMDEnterprise enterprise = this.ReadDBEnterprise(item);
        //        ListObjects.Add(enterprise);
        //    }
        //    return ListObjects;
        //}

        //private EMDEnterprise ReadDBEnterprise(DB.Enterprise dbEnterprise)
        //{
        //    EMDEnterprise enterprise = new EMDEnterprise(dbEnterprise.Guid, dbEnterprise.Created, dbEnterprise.Modified);

        //    enterprise.AD_Picture = dbEnterprise.AD_Picture;
        //    enterprise.ARA = dbEnterprise.ARA;
        //    enterprise.DVR = dbEnterprise.DVR;
        //    enterprise.E_ID = dbEnterprise.E_ID;
        //    enterprise.E_ID_new = dbEnterprise.E_ID_new;
        //    enterprise.E_ID_Parent = dbEnterprise.E_ID_Parent;
        //    enterprise.E_ID_Root = dbEnterprise.E_ID_Root;
        //    enterprise.FibuGericht = dbEnterprise.FibuGericht;
        //    enterprise.FibuNummer = dbEnterprise.FibuNummer;
        //    enterprise.Guid_Parent = dbEnterprise.Guid_Parent;
        //    enterprise.Guid_Root = dbEnterprise.Guid_Root;
        //    enterprise.HasEmployees = dbEnterprise.HasEmployees;
        //    enterprise.HomeInternet = dbEnterprise.HomeInternet;
        //    enterprise.HomeIntranet = dbEnterprise.HomeIntranet;
        //    enterprise.IntranetCOM = dbEnterprise.IntranetCOM;
        //    enterprise.NameLong = dbEnterprise.NameLong;
        //    enterprise.NameShort = dbEnterprise.NameShort;
        //    enterprise.O_Guid_Dis = dbEnterprise.O_Guid_Dis;
        //    enterprise.O_Guid_Prof = dbEnterprise.O_Guid_Prof;
        //    enterprise.O_ID_Dis = dbEnterprise.O_ID_Dis;
        //    enterprise.O_ID_Prof = dbEnterprise.O_ID_Prof;
        //    enterprise.Synonyms = dbEnterprise.Synonyms;
        //    enterprise.UID1 = dbEnterprise.UID1;
        //    enterprise.UID2 = dbEnterprise.UID2;
        //    enterprise.ValidFrom = dbEnterprise.ValidFrom;
        //    enterprise.ValidTo = dbEnterprise.ValidTo;
        //    enterprise.SetStatus();

        //    return enterprise;
        //}

        //public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDEnterprise enterprise = (EMDEnterprise)Object;
        //    Enterprise dbObject = (from item in transaction.dbContext.Enterprise where item.Guid == enterprise.Guid select item).FirstOrDefault();
        //    if (dbObject != null)
        //    {
        //        MapDataToDBObject(ref dbObject, ref enterprise);

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
        //    EMDEnterprise enterprise = (EMDEnterprise)Object;
        //    Enterprise dbObject = (from item in transaction.dbContext.Enterprise where item.Guid == enterprise.Guid select item).FirstOrDefault();
        //    if (dbObject == null)
        //    {
        //        dbObject = new Enterprise();

        //        MapDataToDBObject(ref dbObject, ref enterprise);

        //        //finally write to db
        //        transaction.dbContext.Enterprise.Add(dbObject);
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //private void MapDataToDBObject(ref Enterprise dbObject, ref EMDEnterprise enterprise)
        //{
        //    dbObject.Guid = enterprise.Guid; 

        //    dbObject.ValidFrom = enterprise.ValidFrom; 
        //    dbObject.ValidTo = enterprise.ValidTo;
        //    dbObject.Modified = enterprise.Modified;
        //    dbObject.Created = enterprise.Created;

        //    dbObject.AD_Picture = enterprise.AD_Picture;
        //    dbObject.ARA = enterprise.ARA;
        //    dbObject.DVR = enterprise.DVR;
        //    dbObject.E_ID = enterprise.E_ID;
        //    dbObject.E_ID_new = enterprise.E_ID_new;
        //    dbObject.E_ID_Parent = enterprise.E_ID_Parent;
        //    dbObject.E_ID_Root = enterprise.E_ID_Root;
        //    dbObject.FibuGericht = enterprise.FibuGericht;
        //    dbObject.FibuNummer = enterprise.FibuNummer;
        //    dbObject.Guid_Parent = enterprise.Guid_Parent;
        //    dbObject.Guid_Root = enterprise.Guid_Root;
        //    dbObject.HasEmployees = enterprise.HasEmployees;
        //    dbObject.HomeInternet = enterprise.HomeInternet;
        //    dbObject.HomeIntranet = enterprise.HomeIntranet;
        //    dbObject.IntranetCOM = enterprise.IntranetCOM;
        //    dbObject.NameLong = enterprise.NameLong;
        //    dbObject.NameShort = enterprise.NameShort;
        //    dbObject.O_Guid_Dis = enterprise.O_Guid_Dis;
        //    dbObject.O_Guid_Prof = enterprise.O_Guid_Prof;
        //    dbObject.O_ID_Dis = enterprise.O_ID_Dis;
        //    dbObject.O_ID_Prof = enterprise.O_ID_Prof;
        //    dbObject.Synonyms = enterprise.Synonyms;
        //    dbObject.UID1 = enterprise.UID1;
        //    dbObject.UID2 = enterprise.UID2;
        //}
    }
}
