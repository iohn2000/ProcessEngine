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
    public class EmploymentHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new Employment().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Employment empl = (Employment)dbObject;
            EMDEmployment emdObject = new EMDEmployment(empl.Guid, empl.Created, empl.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        //    public override IEMDObject GetObject(String guid)
        //    {
        //        EMD_DataBase dbContext = new EMD_DataBase();

        //        Employment dbEmployment = (from item in dbContext.Employment where item.Guid == guid select item).FirstOrDefault();
        //        EMDEmployment employment = this.ReadDBEmployment(dbEmployment);
        //        return employment;
        //    }

        //public EMDEmployment GetMainEmploymentForPerson(string PERS_Guid)
        //{

        //    //List<IEMDObject<EMDEmployment>> personlist = (List<IEMDObject<EMDEmployment>>)GetObjects<EMDEmployment, Employment>("EP_Guid = " + PERS_Guid + " and ", null).ToList();

        //    DB.EMD_DataBase dbContext = new DB.EMD_DataBase();

        //    Employment dbEmployment = (from mainemp in dbContext.MainEmployment.Where(this.ValidClause) join emp in dbContext.Employment.Where(this.ValidClause) on mainemp.EP_Guid equals emp.Guid where mainemp.PERS_Guid == PERS_Guid select emp).FirstOrDefault();
        //    EMDEmployment employment = this.ReadDBEmployment(dbEmployment);
        //    return employment;
        //}

        public List<IEMDObject<EMDEmployment>> GetEmploymentsForPerson(string pers_guid)
        {
            List<IEMDObject<EMDEmployment>> employments = GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers_guid + "\"");
            return employments;
        }

        //    private List<EMDEmployment> ReadDBEmployments(List<Employment> dbEmployments)
        //    {
        //        List<EMDEmployment> ListEmployments = new List<EMDEmployment>();
        //        foreach (Employment item in dbEmployments)
        //        {
        //            ListEmployments.Add(this.ReadDBEmployment(item));
        //        }
        //        return ListEmployments;
        //    }

        //    private EMDEmployment ReadDBEmployment(Employment dbEmployment)
        //    {
        //        EMDEmployment employment = new EMDEmployment(dbEmployment.Guid, dbEmployment.Created, dbEmployment.Modified);

        //        employment.AD_Update = dbEmployment.AD_Update;
        //        employment.DGT_Guid = dbEmployment.DGT_Guid;
        //        employment.DGT_ID = dbEmployment.DGT_ID;
        //        employment.DNA_Update = dbEmployment.DNA_Update;
        //        employment.dpwKey = dbEmployment.dpwKey;
        //        employment.Entry = dbEmployment.Entry;
        //        employment.EP_ID = dbEmployment.EP_ID;
        //        employment.ET_Guid = dbEmployment.ET_Guid;
        //        employment.ET_ID = dbEmployment.ET_ID;
        //        employment.Exit = dbEmployment.Exit;
        //        employment.Exit_Report = dbEmployment.Exit_Report;
        //        employment.E_Guid = dbEmployment.E_Guid;
        //        employment.E_ID = dbEmployment.E_ID;
        //        employment.FirstWorkDay = dbEmployment.FirstWorkDay;
        //        employment.Guid = dbEmployment.Guid;
        //        employment.LastDay = dbEmployment.LastDay;
        //        employment.L_Guid = dbEmployment.L_Guid;
        //        employment.L_ID = dbEmployment.L_ID;
        //        employment.PersNr = dbEmployment.PersNr;
        //        employment.P_Guid = dbEmployment.P_Guid;
        //        employment.P_ID = dbEmployment.P_ID;
        //        employment.Sponsor_Guid = dbEmployment.Sponsor_Guid;
        //        employment.Sponsor = dbEmployment.Sponsor;
        //        employment.Visible = dbEmployment.Visible;
        //        employment.ValidFrom = dbEmployment.ValidFrom;
        //        employment.ValidTo = dbEmployment.ValidTo;
        //        employment.SetStatus();
        //        return employment;
        //    }

        //    public override void InsertDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //    {
        //        EMDEmployment employment = (EMDEmployment)Object;
        //        string eguid = employment.Guid;
        //        //TODO wenn historizable dann seetze Datensatz mit person.guid auf ValidTo = Now 
        //        // - erzeuge neues Datenobjekt mit neuer Guid (inl validF und ValidTo) 
        //        // - befülle Datensatz neu

        //        Employment dbObject = (from item in transaction.dbContext.Employment where item.Guid == eguid select item).FirstOrDefault();
        //        if (dbObject == null)
        //        {
        //            dbObject = new Employment();

        //            MapDataToDBObject(ref dbObject, ref employment);

        //            //finally write to db
        //            transaction.dbContext.Employment.Add(dbObject);
        //            transaction.saveChanges();
        //        }
        //        else
        //        {
        //            //throw new CoreException createAlreadyExistingObject
        //        }
        //    }

        //    public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //    {
        //        EMDEmployment employment = (EMDEmployment)Object;
        //        string eguid = employment.Guid;
        //        //TODO wenn historizable dann seetze Datensatz mit person.guid auf ValidTo = Now 
        //        // - erzeuge neues Datenobjekt mit neuer Guid (inl validF und ValidTo) 
        //        // - befülle Datensatz neu

        //        Employment dbObject = (from item in transaction.dbContext.Employment where item.Guid == eguid select item).FirstOrDefault();
        //        if (dbObject != null)
        //        {
        //            MapDataToDBObject(ref dbObject, ref employment);

        //            //finally write to db
        //            transaction.saveChanges();
        //        }
        //        else
        //        {
        //            //throw new CoreException createAlreadyExistingObject
        //        }

        //    }

        //    private void MapDataToDBObject(ref Employment dbObject, ref EMDEmployment employment)
        //    {
        //        dbObject.Guid = employment.Guid; 
        //        dbObject.ValidFrom = employment.ValidFrom; 
        //        dbObject.ValidTo = employment.ValidTo;
        //        dbObject.Modified = employment.Modified;
        //        dbObject.Created = employment.Created;

        //        dbObject.AD_Update = employment.AD_Update;
        //        dbObject.DGT_Guid = employment.DGT_Guid;
        //        dbObject.DGT_ID = employment.DGT_ID;
        //        dbObject.DNA_Update = employment.DNA_Update;
        //        dbObject.dpwKey = employment.dpwKey;
        //        dbObject.Entry = employment.Entry;
        //        dbObject.EP_ID = employment.EP_ID;
        //        dbObject.ET_Guid = employment.ET_Guid;
        //        dbObject.ET_ID = employment.ET_ID;
        //        dbObject.Exit = employment.Exit;
        //        dbObject.Exit_Report = employment.Exit_Report;
        //        dbObject.E_Guid = employment.E_Guid;
        //        dbObject.E_ID = employment.E_ID;
        //        dbObject.FirstWorkDay = employment.FirstWorkDay;
        //        dbObject.Guid = employment.Guid;
        //        dbObject.LastDay = employment.LastDay;
        //        dbObject.L_Guid = employment.L_Guid;
        //        dbObject.L_ID = employment.L_ID;
        //        dbObject.PersNr = employment.PersNr;
        //        dbObject.P_Guid = employment.P_Guid;
        //        dbObject.P_ID = employment.P_ID;
        //        dbObject.Sponsor_Guid = employment.Sponsor_Guid;
        //        dbObject.Sponsor = employment.Sponsor;
        //        dbObject.Visible = employment.Visible;
        //    }

        //    public EMDEmployment CreateDefault(string P_Guid, string E_Guid, string L_Guid, string ET_Guid, string DGT_Guid, string PersNr)
        //    {
        //        EMDEmployment emp = new EMDEmployment();

        //        emp.AD_Update = false;
        //        emp.DGT_Guid = DGT_Guid;
        //        emp.DGT_ID = 1;
        //        emp.DNA_Update = false;
        //        emp.dpwKey = String.Empty;
        //        emp.EP_ID = 132456; //TODO implement ID Generator
        //        emp.ET_Guid = ET_Guid;
        //        emp.ET_ID = 1;
        //        emp.E_Guid = E_Guid;
        //        emp.E_ID = 1;
        //        emp.L_Guid = L_Guid;
        //        //emp.Main = true;
        //        emp.PersNr = PersNr;
        //        emp.P_Guid = P_Guid;
        //        emp.P_ID = 1;
        //        emp.Visible = true;

        //        return emp;

        //    }

        //    public string getSponsorName(String guid)
        //    {
        //        String sponsorName = String.Empty;
        //        EMD_DataBase dbContext = new EMD_DataBase();

        //        Employment dbEmployment = (from item in dbContext.Employment where item.Guid == guid select item).FirstOrDefault();
        //        if (dbEmployment != null)
        //        {
        //            EMDEmployment employment = this.ReadDBEmployment(dbEmployment);
        //            PersonHandler ph = new PersonHandler();
        //            EMDPerson person = (EMDPerson)ph.GetObject(employment.P_Guid);
        //            if (person != null)
        //            {
        //                sponsorName = person.Display_FirstName + " " + person.Display_FamilyName;
        //            }
        //        }
        //        return sponsorName;
        //    }
    }
}
