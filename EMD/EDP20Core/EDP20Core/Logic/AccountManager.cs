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
using Kapsch.IS.EDP.Core.Utils;
using System.Data.SqlClient;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class AccountManager
        : BaseManager
        , IAccountManager
    {
        public bool DeliverInActive { get; set; }

        #region Constructor

        public AccountManager()
            : base()
        {
        }

        public AccountManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public AccountManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public AccountManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructor

        public EMDAccount Create(EMDAccount emdAccount)
        {
            AccountHandler handler = new AccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            emdAccount.AC_ID = GetNextFreeAC_ID();
            return (EMDAccount)handler.CreateObject(emdAccount);
        }

        public EMDAccount Delete(string guid)
        {
            AccountHandler handler = new AccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            AccountManager accountManager = new AccountManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDAccount emdRole = Get(guid);
            if (emdRole != null)
            {
                accountManager.CleanupEmploymentAccountRelations(guid);

                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDAccount)handler.DeleteObject<EMDAccount>(emdRole);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Account with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// Checks if a AccountId for an specific enterprise is available
        /// </summary>
        /// <param name="accoGuid"></param>
        /// <param name="costCenterId"></param>
        /// <param name="enteGuid"></param>
        /// <returns></returns>
        public bool IsAccountIdAvailable(string accoGuid, string costCenterId, string enteGuid)
        {
            bool isAvailable = true;

            AccountHandler handler = new AccountHandler(this.Transaction);

            List<EMDAccount> roles = handler.GetObjects<EMDAccount, Account>(string.Format("KstID = \"{0}\" AND E_Guid = \"{1}\"", costCenterId, enteGuid)).Cast<EMDAccount>().OrderBy(a => a.AC_ID).ToList();

            if (!string.IsNullOrEmpty(accoGuid))
            {
                roles = roles.Where(a => a.Guid != accoGuid).ToList();
            }


            if (roles != null && roles.Count > 0)
            {
                isAvailable = false;
            }

            return isAvailable;
        }

        public EMDAccount Get(string guid)
        {
            AccountHandler handler = new AccountHandler(this.Transaction);

            return (EMDAccount)handler.GetObject<EMDAccount>(guid);
        }

        public List<EMDAccount> GetAccounts()
        {
            List<EMDAccount> emdAccounts = new List<EMDAccount>();
            AccountHandler handler = new AccountHandler(this.Transaction);


            List<IEMDObject<EMDAccount>> userDomains = (List<IEMDObject<EMDAccount>>)handler.GetObjects<EMDAccount, DB.Account>("1=1");

            foreach (var item in userDomains)
            {
                emdAccounts.Add((EMDAccount)item);
            }

            return emdAccounts;
        }

        public string GetCostCenterResponsibleName(string acco_guid)
        {
            EMDPerson emdPerson = null;

            AccountHandler accountHandler = new AccountHandler(this.Transaction);

            EMDEmployment emdEmployment = accountHandler.GetResponsible(acco_guid);
            if (emdEmployment != null)
            {
                PersonHandler personController = new PersonHandler(this.Transaction);
                emdPerson = (EMDPerson)personController.GetObject<EMDPerson>(emdEmployment.P_Guid);

                if (emdPerson != null)
                    return string.Format("{0} {1}", emdPerson.Display_FirstName, emdPerson.Display_FamilyName);
                else
                    return string.Empty;
            }
            else
            {
                return string.Empty;
            }

        }

        public void Update(EMDAccount emdAccount)
        {
            AccountHandler handler = new AccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            handler.UpdateDBObject(emdAccount);
        }

        public string GetAccountGuidForEmployment_DIRECT(SqlConnection sqlConnection, string empl_guid)
        {
            EMDAccount emdAccount = null;

            string getAccountHandlerQuery = string.Format("select top 1 AC_Guid from EmploymentAccount where EP_Guid = '{0}' and guid=historyguid", empl_guid);
            SqlCommand sqlCommand = new SqlCommand(getAccountHandlerQuery, sqlConnection);
            var reader = sqlCommand.ExecuteReader();

            string AC_Guid = null;
            if (reader.Read())
            {
                AC_Guid = reader.GetString(0);
               reader.Close();

            }
            return AC_Guid;
        }

        public EMDAccount GetAccountForEmployment(string empl_guid)
        {
            EMDAccount emdAccount = null;
            //ObjectFlagManager ofm = new ObjectFlagManager(this.Transaction);
            
            EmploymentAccountHandler empAch = new EmploymentAccountHandler();
            List<EMDEmploymentAccount> empAcc = empAch.GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + empl_guid + "\"").Cast<EMDEmploymentAccount>().ToList();

            if (empAcc.Count > 0)
            {
                // there should be only one, but ignore that case
                AccountHandler ach = new AccountHandler(this.Transaction);
                emdAccount = (EMDAccount)ach.GetObject<EMDAccount>(empAcc[0].AC_Guid);
            }

            return emdAccount;
        }

        public List<EMDAccountEnhanced> GetAccountsEnhancedList(string whereClause, string guidAccount = null)
        {
            List<EMDAccountEnhanced> emdAccountEnhancedList = new List<EMDAccountEnhanced>();

            DataSet dataSet = new DataSet();

            // Fill Accounts
            AccountHandler accountHandler = new AccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            accountHandler.DeliverInActive = this.DeliverInActive;

            List<EMDAccount> listAccounts = null;

            StringBuilder filter = new StringBuilder("1 = 1");
            if (!string.IsNullOrEmpty(guidAccount))
            {
                //filter.Append(string.Format(" && Guid = \"{0}\"", guidAccount));
                EMDAccount accountFound = (EMDAccount)accountHandler.GetObject<EMDAccount>(guidAccount);
                if (accountFound != null)
                {
                    listAccounts = new List<EMDAccount>();
                    listAccounts.Add(accountFound);
                }
            }
            else
            {
                filter.Append(" && " + whereClause);
                listAccounts = accountHandler.GetObjects<EMDAccount, Account>(filter.ToString(), null).Cast<EMDAccount>().ToList();
            }

            DataTable tableAccounts = OrgUnitRoleManager.ToDataTable(listAccounts, "EMDAccount");
            dataSet.Tables.Add(tableAccounts);

            // Fill Employments
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDEmployment>> iListEmployments = (List<IEMDObject<EMDEmployment>>)employmentHandler.GetObjects<EMDEmployment, Employment>();
            List<EMDEmployment> listEmployments = iListEmployments.Cast<EMDEmployment>().ToList();

            DataTable tableEmployments = OrgUnitRoleManager.ToDataTable(listEmployments, "EMDEmployment");
            dataSet.Tables.Add(tableEmployments);
            // add relation
            dataSet.Relations.Add(
                "RelationEP_Guid",
                dataSet.Tables["EMDAccount"].Columns["Responsible"],
                dataSet.Tables["EMDEmployment"].Columns["Guid"], false
            );

            // Fill Persons
            PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDPerson>> iListPersons = (List<IEMDObject<EMDPerson>>)personHandler.GetObjects<EMDPerson, Person>();
            List<EMDPerson> listPersons = iListPersons.Cast<EMDPerson>().ToList();

            DataTable tablePersons = OrgUnitRoleManager.ToDataTable(listPersons, "EMDPerson");
            dataSet.Tables.Add(tablePersons);
            // add relation
            dataSet.Relations.Add(
                "RelationP_Guid",
                dataSet.Tables["EMDEmployment"].Columns["P_Guid"],
                dataSet.Tables["EMDPerson"].Columns["Guid"], false
            );


            foreach (DataRow parentRow in dataSet.Tables["EMDAccount"].Rows)
            {
                EMDAccountEnhanced accountEnhanced = new EMDAccountEnhanced();
                accountEnhanced.Guid = parentRow["Guid"].ToString();
                accountEnhanced.ActiveFrom = Convert.ToDateTime(parentRow["ActiveFrom"]);
                accountEnhanced.ActiveTo = Convert.ToDateTime(parentRow["ActiveTo"]);
                accountEnhanced.ValidFrom = Convert.ToDateTime(parentRow["ValidFrom"]);
                accountEnhanced.ValidTo = Convert.ToDateTime(parentRow["ValidTo"]);
                accountEnhanced.E_Guid = parentRow["E_Guid"].ToString();
                accountEnhanced.KstID = parentRow["KstID"].ToString();
                accountEnhanced.Name = parentRow["Name"].ToString();
                accountEnhanced.Responsible = parentRow["Responsible"].ToString();
                accountEnhanced.ResponsibleEmplGuid = parentRow["Responsible"].ToString();

                DataRow[] employmentRows = parentRow.GetChildRows("RelationEP_Guid");
                if (employmentRows.Length > 0)
                {
                    DataRow[] personRows = employmentRows[0].GetChildRows("RelationP_Guid");

                    if (personRows.Length > 0)
                    {
                        DataRow personRow = personRows[0];
                        string firstName = personRow["FirstName"] as string;
                        string familyName = personRow["FamilyName"] as string;
                        string userId = personRow["UserID"] as string;

                        accountEnhanced.ResponsibleName = familyName + " " + firstName;
                        if (!String.IsNullOrWhiteSpace(userId))
                        {
                            accountEnhanced.ResponsibleName += " (" + userId.ToUpper() + ")";
                        }

                        accountEnhanced.ResponsiblePersGuid = personRow["Guid"].ToString();

                        emdAccountEnhancedList.Add(accountEnhanced);
                    }

                }
            }

            return emdAccountEnhancedList;
        }

        /// <summary>
        /// check wether given employment is member of given account
        /// </summary>
        /// <param name="effectedEmplGuid"></param>
        /// <param name="acco_Guid"></param>
        /// <returns></returns>
        public bool EmploymentIsInCostCenter(string effectedEmplGuid, string acco_Guid)
        {
            bool result = false;

            string actualAccoGuid = this.GetAccountForEmployment(effectedEmplGuid).Guid;

            if (acco_Guid == actualAccoGuid)
            {
                result = true;
            }
            return result;

        }

        public int GetNextFreeAC_ID()
        {
            EMD_Entities dbcontext = new EMD_Entities();
            IQueryable<int> query = (from item in dbcontext.Account orderby item.AC_ID descending select item.AC_ID);
            //Some performance improvement
            IQueryable<int> newQuery = query.Take(1);

            List<int> result = newQuery.ToList();
            //return the + 1 added new Int.
            return result.Single() + 1;
        }

        /// <summary>
        /// Deletes account-employment Relations from database to allow the deletion of an account
        /// This is a workaround for data-cleanup, because the processengine has no end-state to cleanup the relations after a suceeded run
        /// </summary>
        /// <remarks>if the account is null, all relations will be deleted. Long running process.</remarks>
        /// <param name="accoGuid">cleans relations for a specific orgunit</param>
        public void CleanupEmploymentAccountRelations(string accoGuid = null)
        {


            if (!string.IsNullOrEmpty(accoGuid) && new EMDGuid(accoGuid).Prefix != new EMDAccount().Prefix)
            {
                throw new GuidCastException((int)EnumErrorCode.E_EDP_ENTITY, accoGuid);
            }

            if (this.Transaction == null)
            {
                this.Transaction = new CoreTransaction();
            }

            EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(this.Transaction, Guid_ModifiedBy, ModifyComment);

            List<EmploymentAccount> employmentAccounts = null;

            if (!string.IsNullOrEmpty(accoGuid))
            {
                employmentAccounts = this.Transaction.dbContext.EmploymentAccount.SqlQuery(string.Format(@"select ea.* 
                                        from EmploymentAccount as ea join Employment as empl on empl.Guid = ea.EP_Guid 
                                        where ea.Guid = ea.HistoryGuid and ea.ActiveTo >  GETDATE() and ea.ValidTo > GETDATE() and 
                                        empl.Guid = empl.HistoryGuid and
                                        ea.AC_Guid = '{0}' and
                                        empl.Status = 70", accoGuid), new object[] { }).ToList();
            }
            else
            {
                employmentAccounts = this.Transaction.dbContext.EmploymentAccount.SqlQuery(@"select ea.* 
                                        from EmploymentAccount as ea join Employment as empl on empl.Guid = ea.EP_Guid 
                                        where ea.Guid = ea.HistoryGuid and ea.ActiveTo >  GETDATE() and ea.ValidTo > GETDATE() and 
                                        empl.Guid = empl.HistoryGuid and
                                        empl.Status = 70", new object[] { }).ToList();
            }

            int counter = 0;
            foreach (EmploymentAccount employmentAccount in employmentAccounts)
            {
                counter++;
                EMDEmploymentAccount emplAccount = (EMDEmploymentAccount)employmentAccountHandler.GetObject<EMDEmploymentAccount>(employmentAccount.Guid);
                employmentAccountHandler.DeleteObject<EMDEmploymentAccount>(emplAccount);
            }
        }
    }
}
