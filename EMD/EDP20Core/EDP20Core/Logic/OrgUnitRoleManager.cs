using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using System.Collections;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using System.Data;
using Kapsch.IS.EDP.Core.DB;
using System.Reflection;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class OrgUnitRoleManager
        : BaseManager
        , IOrgUnitRoleManager
    {
        #region Constructors

        public OrgUnitRoleManager()
            : base()
        {
        }

        public OrgUnitRoleManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public OrgUnitRoleManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public OrgUnitRoleManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDOrgUnitRole Delete(string guid)
        {
            OrgUnitRoleHandler handler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDOrgUnitRole emdItem = Get(guid);
            if (emdItem != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDOrgUnitRole)handler.DeleteObject<EMDOrgUnitRole>(emdItem);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Orgunit-Role with guid: {0} was not found.", guid));
            }
        }

        public EMDOrgUnitRole Update(string guid_user, bool isAdmin, EMDOrgUnitRole emdOrgunitRole, CoreTransaction transaction = null)
        {
            if (this.EqualOrgUnitRoleExists(emdOrgunitRole))
                throw new EntityNotAllowedException("OrgUnitRole", EnumEntityNotAllowedError.EntityAllowedOnlyOnceForSelectedParameters,
                        ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The requested employment is already contained in the OrgUnit with the selected Role!");

            OrgUnitRoleHandler orgunitRoleHandler = new OrgUnitRoleHandler(transaction, guid_user);
            IsNewOrgunitRoleAllowed(emdOrgunitRole.O_Guid, emdOrgunitRole.EP_Guid, emdOrgunitRole.R_Guid, isAdmin);
            return (EMDOrgUnitRole)orgunitRoleHandler.UpdateObject<EMDOrgUnitRole>(emdOrgunitRole);
        }

        private bool EqualOrgUnitRoleExists(EMDOrgUnitRole emdOrgunitRole)
        {
            OrgUnitRoleHandler orgunitRoleHandler = new OrgUnitRoleHandler();
            //Check if there is already an orgunitrole with the same employment, orgunit and role
            int equalOrgUnitRoles = orgunitRoleHandler.GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid = \"" + emdOrgunitRole.EP_Guid + "\" && O_Guid=\"" + emdOrgunitRole.O_Guid + "\" && R_Guid=\"" + emdOrgunitRole.R_Guid + "\"").Count();

            if (equalOrgUnitRoles > 0)
                return true;
            else
                return false;
        }

        public EMDOrgUnitRole Create(string guid_user, bool isAdmin, EMDOrgUnitRole emdOrgunitRole, CoreTransaction transaction = null)
        {
            if (this.EqualOrgUnitRoleExists(emdOrgunitRole))
                throw new EntityNotAllowedException("OrgUnitRole", EnumEntityNotAllowedError.EntityAllowedOnlyOnceForSelectedParameters,
                        ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The requested employment is already contained in the OrgUnit with the selected Role!");

            OrgUnitRoleHandler orgunitRoleHandler = new OrgUnitRoleHandler(transaction, guid_user);
            IsNewOrgunitRoleAllowed(emdOrgunitRole.O_Guid, emdOrgunitRole.EP_Guid, emdOrgunitRole.R_Guid, isAdmin);
            return (EMDOrgUnitRole)orgunitRoleHandler.CreateObject<EMDOrgUnitRole>(emdOrgunitRole);
        }



        /// <summary>
        /// throws new Exception
        /// </summary>
        /// <param name="guid_orgu"></param>
        /// <param name="guid_empl"></param>
        /// <param name="guid_role"></param>
        public void IsNewOrgunitRoleAllowed(string guid_orgu, string guid_empl, string guid_role, bool isAdmin)
        {
            EMDRole personRole = null;
            bool isPersonRoleRequested = false;
            try
            {
                personRole = (EMDRole)new RoleHandler().GetRoleById(RoleHandler.PERSON);
                if (guid_role == personRole.Guid)
                {
                    isPersonRoleRequested = true;

                    if (!isAdmin)
                    {
                        throw new EdpSecurityException("OrgUnitRole", EdpEnumSecurityError.OnlyAdmin,
                            ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The requested employment is already contained in the OrgUnit with the Role PERSON!");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("PersonRole not found");
            }


            List<EMDOrgUnitRole> configuredOrgunitRoles = new OrgUnitRoleHandler().GetObjects<EMDOrgUnitRole, OrgUnitRole>(string.Format("EP_Guid = \"{0}\"", guid_empl)).Cast<EMDOrgUnitRole>().ToList();

            foreach (EMDOrgUnitRole currentOrgunitRole in configuredOrgunitRoles)
            {
                if (currentOrgunitRole.R_Guid == personRole.Guid && isPersonRoleRequested)
                {
                    throw new EntityNotAllowedException("OrgUnitRole", EnumEntityNotAllowedError.EntityAllowedOnlyOnce,
                        ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The requested employment is already contained in the OrgUnit with the Role PERSON!");
                }


            }
        }

        public List<EMDOrgUnitRoleEnhanced> GetOrgUnitRoleEnhancedList(string guidOrgunit = null)
        {
            List<EMDOrgUnitRoleEnhanced> emdOrgUnitRoleEnhancedList = new List<EMDOrgUnitRoleEnhanced>();

            DataSet dataSet = new DataSet();

            // Fill OrgunitRoles
            OrgUnitRoleHandler orgUnitRoleHandler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            StringBuilder filter = new StringBuilder("1 = 1");
            if (!string.IsNullOrEmpty(guidOrgunit))
            {
                filter.Append(string.Format(" && O_Guid = \"{0}\"", guidOrgunit));
            }

            List<EMDOrgUnitRole> listOrgUnitRoles = orgUnitRoleHandler.GetObjects<EMDOrgUnitRole, OrgUnitRole>(filter.ToString(), null).Cast<EMDOrgUnitRole>().ToList();

            DataTable tableOrgUnitRoles = ToDataTable(listOrgUnitRoles, "EMDOrgUnitRole");
            dataSet.Tables.Add(tableOrgUnitRoles);

            // Fill Orgunits
            OrgUnitHandler orgUnitHandler = new OrgUnitHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDOrgUnit>> iListOrgUnits = (List<IEMDObject<EMDOrgUnit>>)orgUnitHandler.GetObjects<EMDOrgUnit, OrgUnit>("1 = 1", null);
            //   List<EMDOrgUnit> listOrgUnits = new List<EMDOrgUnit>().ConvertAll(x => (EMDOrgUnit)x);
            List<EMDOrgUnit> listOrgUnits = iListOrgUnits.Cast<EMDOrgUnit>().ToList();

            DataTable tableOrgUnits = ToDataTable(listOrgUnits, "EMDOrgUnit");
            dataSet.Tables.Add(tableOrgUnits);
            // add relation
            dataSet.Relations.Add(
                "RelationO_Guid",
                dataSet.Tables["EMDOrgUnitRole"].Columns["O_Guid"],
                dataSet.Tables["EMDOrgUnit"].Columns["Guid"], false
            );

            // Fill Roles
            RoleHandler roleHandler = new RoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDRole>> iListRoles = (List<IEMDObject<EMDRole>>)roleHandler.GetObjects<EMDRole, Role>("1 = 1", null);
            List<EMDRole> listRoles = iListRoles.Cast<EMDRole>().ToList();

            DataTable tableRoles = ToDataTable(listRoles, "EMDRole");
            dataSet.Tables.Add(tableRoles);
            // add relation
            dataSet.Relations.Add(
                "RelationR_Guid",
                dataSet.Tables["EMDOrgUnitRole"].Columns["R_Guid"],
                dataSet.Tables["EMDRole"].Columns["Guid"], false
            );

            // Fill Employments
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDEmployment>> iListEmployments = (List<IEMDObject<EMDEmployment>>)employmentHandler.GetObjects<EMDEmployment, Employment>();
            List<EMDEmployment> listEmployments = iListEmployments.Cast<EMDEmployment>().ToList();

            DataTable tableEmployments = ToDataTable(listEmployments, "EMDEmployment");
            dataSet.Tables.Add(tableEmployments);
            // add relation
            dataSet.Relations.Add(
                "RelationEP_Guid",
                dataSet.Tables["EMDOrgUnitRole"].Columns["EP_Guid"],
                dataSet.Tables["EMDEmployment"].Columns["Guid"], false
            );

            // Fill Persons
            PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDPerson>> iListPersons = (List<IEMDObject<EMDPerson>>)personHandler.GetObjects<EMDPerson, Person>();
            List<EMDPerson> listPersons = iListPersons.Cast<EMDPerson>().ToList();

            DataTable tablePersons = ToDataTable(listPersons, "EMDPerson");
            dataSet.Tables.Add(tablePersons);
            // add relation
            dataSet.Relations.Add(
                "RelationP_Guid",
                dataSet.Tables["EMDEmployment"].Columns["P_Guid"],
                dataSet.Tables["EMDPerson"].Columns["Guid"], false
            );

            // Fill EmploymentTypes
            EmploymentTypeHandler employmentTypeHandler = new EmploymentTypeHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDEmploymentType>> iListEmploymentTypes = (List<IEMDObject<EMDEmploymentType>>)employmentTypeHandler.GetObjects<EMDEmploymentType, EmploymentType>();
            List<EMDEmploymentType> ListEmploymentTypes = iListEmploymentTypes.Cast<EMDEmploymentType>().ToList();

            DataTable tableEmploymentTypes = ToDataTable(ListEmploymentTypes, "EMDEmploymentType");
            dataSet.Tables.Add(tableEmploymentTypes);
            // add relation
            dataSet.Relations.Add(
                "RelationET_Guid",
                dataSet.Tables["EMDEmployment"].Columns["ET_Guid"],
                dataSet.Tables["EMDEmploymentType"].Columns["Guid"], false
            );

            DateTime lastDay;
            DateTime exitDate;

            foreach (DataRow parentRow in dataSet.Tables["EMDOrgUnitRole"].Rows)
            {
                string x = parentRow["Guid"].ToString();
                string y = x;

                EMDOrgUnitRoleEnhanced orgunitRole = new EMDOrgUnitRoleEnhanced()
                {
                    Guid = parentRow["Guid"] as string,
                    O_Guid = parentRow["O_Guid"] as string,
                    ActiveFrom = Convert.ToDateTime(parentRow["ActiveFrom"]),
                    ActiveTo = Convert.ToDateTime(parentRow["ActiveTo"]),
                    ValidFrom = Convert.ToDateTime(parentRow["ValidFrom"]),
                    ValidTo = Convert.ToDateTime(parentRow["ValidTo"]),
                    R_ID = Convert.ToInt32(parentRow["R_ID"]),
                    //OrgUnitName = parentRow.GetChildRows("RelationO_Guid").Length > 0 ? parentRow.GetChildRows("RelationO_Guid")[0]["Name"] as string : string.Empty,
                    //RoleName = parentRow.GetChildRows("RelationR_Guid").Length > 0 ? parentRow.GetChildRows("RelationR_Guid")[0]["Name"] as string : string.Empty,
                    EmploymentPersonalId = parentRow.GetChildRows("RelationEP_Guid").Length > 0 ? parentRow.GetChildRows("RelationEP_Guid")[0]["PersNr"] as string : string.Empty,
                };

                DataRow[] employmentRows = parentRow.GetChildRows("RelationEP_Guid");

                if (employmentRows.Length > 0)
                {
                    DataRow employmentRow = employmentRows[0];
                    orgunitRole.EmploymentPersonalId = employmentRow["PersNr"] as string;

                    string employmentId = employmentRow["EP_ID"] as string;
                    int test = 0;
                    if (int.TryParse(employmentId, out test))
                    {
                        orgunitRole.EP_ID = test;
                    }

                    object lastObject = employmentRow["LastDay"];
                    if (lastObject == DBNull.Value)
                    {
                        lastDay = EMDEmployment.INFINITY;
                    }
                    else
                    {
                        lastDay = DateTime.Parse(lastObject as string);
                    }


                    object exitObject = employmentRow["Exit"];
                    if (exitObject == DBNull.Value)
                    {
                        exitDate = EMDEmployment.INFINITY;
                    }
                    else
                    {
                        exitDate = DateTime.Parse(exitObject as string);
                    }
                    if (lastDay > DateTime.Now && exitDate > DateTime.Now)
                    {

                        DataRow[] personRows = employmentRow.GetChildRows("RelationP_Guid");


                        if (personRows.Length > 0)
                        {
                            DataRow personRow = personRows[0];

                            string firstName = personRow["FirstName"] as string;
                            string familyName = personRow["FamilyName"] as string;
                            string userId = personRow["UserID"] as string;


                            orgunitRole.PersonName = employmentRow["PersNr"] as string;

                            orgunitRole.PersonName = familyName + " " + firstName;
                            if (!String.IsNullOrWhiteSpace(userId))
                            {
                                orgunitRole.PersonName += "(" + userId.ToUpper() + ")";
                            }

                            DataRow[] employmentTypeRows = employmentRow.GetChildRows("RelationET_Guid");
                            if (employmentTypeRows.Length > 0)
                            {
                                orgunitRole.EmploymentTypeName = employmentTypeRows[0]["Name"].ToString();
                            }

                            DataRow[] orgUnitRows = parentRow.GetChildRows("RelationO_Guid");
                            if (orgUnitRows.Length > 0)
                            {
                                orgunitRole.OrgUnitName = orgUnitRows[0]["Name"].ToString();
                                //orgunitRole.Guid = orgUnitRows[0]["Guid"].ToString();

                                DataRow[] roleRows = parentRow.GetChildRows("RelationR_Guid");
                                if (roleRows.Length > 0)
                                {
                                    orgunitRole.RoleName = roleRows[0]["Name"].ToString();

                                    emdOrgUnitRoleEnhancedList.Add(orgunitRole);
                                }
                            }
                        }
                    }
                    //Nicht hier hinzufügen da kein aktives Employment oder keine aktive person/rolle/Orgunit  gefunden wurde => OrgUnit anscheinend nicht deaktiviert wurde
                }

            }
            return emdOrgUnitRoleEnhancedList;
        }


        public static DataTable ToDataTable<T>(List<T> items, string tableName)
        {
            DataTable dataTable = new DataTable(tableName);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);

                if (prop.Name.Equals("Guid"))
                {
                    dataTable.Columns["Guid"].Unique = true;
                }
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public EMDOrgUnitRole Get(string guid)
        {
            OrgUnitRoleHandler handler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDOrgUnitRole)handler.GetObject<EMDOrgUnitRole>(guid);
        }

        /// <summary>
        /// retrieve all Roles for a given employment in the given orgunit
        /// </summary>
        /// <param name="guid_orgu"></param>
        /// <param name="guid_empl"></param>
        /// <returns></returns>
        public List<EMDOrgUnitRole> GetOrgUnitRolesForEmploymentinOrgUnit(string guid_orgu, string guid_empl)
        {
            List<EMDOrgUnitRole> resultList = new List<EMDOrgUnitRole>();
            OrgUnitRoleHandler orgUnitRoleHandler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            resultList = orgUnitRoleHandler.GetObjects<EMDOrgUnitRole, OrgUnitRole>
                (string.Format("EP_Guid = \"{0}\" && O_Guid = \"{1}\"", guid_empl, guid_orgu))
                .Cast<EMDOrgUnitRole>().ToList();
            return resultList;
        }

        /// <summary>
        /// Move a given OrgUnitRole to an other orgunit
        /// </summary>
        /// <param name="ouro"></param>
        /// <param name="guid_new_orgu"></param>
        public void MoveRoleToOrgUnit(EMDOrgUnitRole ouro, string guid_new_orgu)
        {
            OrgUnitRoleHandler orgUnitRoleHandler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            ouro.O_Guid = guid_new_orgu;
            orgUnitRoleHandler.UpdateObject(ouro);
        }

        /// <summary>
        /// Check whether this Employment has its personRole in a given OrgUnit
        /// </summary>
        /// <param name="guid_orgu"></param>
        /// <returns></returns>
        public bool IsEmploymentInOrgUnitWithPersonRole(string effectedEmplGuid, string guid_orgu)
        {
            bool result = false;

            OrgUnitRoleHandler orgUnitRoleHandler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            string actualOrguGuid = ((EMDOrgUnitRole)orgUnitRoleHandler.GetOrgUnitRole(effectedEmplGuid, RoleHandler.PERSON)).O_Guid;

            if (guid_orgu == actualOrguGuid)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Deletes orgunit-employment Relations from database to allow the deletion of an orgunit
        /// This is a workaround for data-cleanup, because the processengine has no end-state to cleanup the relations after a suceeded run
        /// </summary>
        /// <remarks>if the orgunit is null, all relations will be deleted. Long running process.</remarks>
        /// <param name="orguGuid">cleans relations for a specific orgunit</param>
        public void CleanupOrgunitRoleRelations(string orguGuid = null)
        {
            if (this.Transaction == null)
            {
                this.Transaction = new CoreTransaction();
            }

            List<OrgUnitRole> orgunitRoles = null;

            if (!string.IsNullOrEmpty(orguGuid))
            {
                orgunitRoles = this.Transaction.dbContext.OrgUnitRole.SqlQuery(string.Format(@"select orgu.* 
                                from OrgUnitRole as orgu join Employment as empl on empl.Guid = orgu.EP_Guid 
                                where orgu.Guid = orgu.HistoryGuid and orgu.ActiveTo >  GETDATE() and orgu.ValidTo > GETDATE() and 
                                empl.Guid = empl.HistoryGuid and
                                orgu.O_Guid = '{0}' and
                                empl.Status = 70", orguGuid), new object[] { }).ToList();
            }
            else
            {
                orgunitRoles = this.Transaction.dbContext.OrgUnitRole.SqlQuery(@"select orgu.* 
                                from OrgUnitRole as orgu join Employment as empl on empl.Guid = orgu.EP_Guid 
                                where orgu.Guid = orgu.HistoryGuid and orgu.ActiveTo >  GETDATE() and orgu.ValidTo > GETDATE() and 
                                empl.Guid = empl.HistoryGuid and 
                                empl.Status = 70", new object[] { }).ToList();
            }

            int counter = 0;
            foreach (OrgUnitRole orgunitRole in orgunitRoles)
            {
                counter++;
                Delete(orgunitRole.Guid);
            }
        }
    }
}
