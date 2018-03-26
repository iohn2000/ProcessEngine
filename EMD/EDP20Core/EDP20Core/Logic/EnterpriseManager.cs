using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class EnterpriseManager
        : BaseManager
        , IEnterpriseManager
    {
        /// <summary>
        /// Take this E-Mail-Template for new Objects
        /// </summary>
        public const string TEMPLATE_DISTRIBUTION_EMAIL = "KIBSI-EDP-INFO-{0}@kapsch.net";

        public const string TEMPLATE_DISTRIBUTION_EMAIL_NOT_ASSIGNED = "KIBSI-EDP-INFO-9998@kapsch.net";

        #region Constructors

        public EnterpriseManager()
            : base()
        {
        }

        public EnterpriseManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EnterpriseManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public EnterpriseManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDEnterprise Get(string guid)
        {
            EnterpriseHandler handler = new EnterpriseHandler(this.Transaction);

            return (EMDEnterprise)handler.GetObject<EMDEnterprise>(guid);
        }

        public List<EMDEnterprise> GetList()
        {
            return GetList(null);
        }

        public List<EMDEnterprise> GetList(string whereClause)
        {
            return new EnterpriseHandler(this.Transaction).GetObjects<EMDEnterprise, Enterprise>(whereClause).Cast<EMDEnterprise>().ToList();
        }

        public List<EMDEnterprise> GetAllowedOnboardingList()
        {
            return GetList("HasEmployees = true");
        }

        public static string GenerateDistributionEmailProposal(int e_id_new)
        {
            if (e_id_new > 9999)
            {
                return TEMPLATE_DISTRIBUTION_EMAIL_NOT_ASSIGNED;
            }
            return string.Format(TEMPLATE_DISTRIBUTION_EMAIL, e_id_new);
        }

        public EMDEnterprise GetEnterpriseByOldE_ID(int e_id)
        {
            EnterpriseHandler handler = new EnterpriseHandler(this.Transaction);
            List<IEMDObject<EMDEnterprise>> enteChildList = handler.GetObjects<EMDEnterprise, Enterprise>("E_ID = " + e_id.ToString() + "");
            if (enteChildList != null && enteChildList.Count > 0)
            {
                return (EMDEnterprise)enteChildList[0];
            }
            else
                return null;
        }

        public EMDEnterprise GetEnterpriseByEIdNew(int eIdNew)
        {
            EnterpriseHandler handler = new EnterpriseHandler(this.Transaction);
            List<IEMDObject<EMDEnterprise>> enteChildList = handler.GetObjects<EMDEnterprise, Enterprise>("E_ID_new = " + eIdNew.ToString() + "");
            if (enteChildList != null && enteChildList.Count > 0)
            {
                return (EMDEnterprise)enteChildList[0];
            }
            else
                return null;
        }

        public List<EMDEnterprise> GetEnterpriseLeafes(List<int> eIdsNews)
        {
            List<EMDEnterprise> enterprises = new List<EMDEnterprise>();
            EnterpriseHandler handler = new EnterpriseHandler(this.Transaction);

            foreach (int eIdNew in eIdsNews)
            {
                EMDEnterprise foundEnterprise = GetEnterpriseByEIdNew(eIdNew);
                if (foundEnterprise != null)
                {
                    enterprises.AddRange(handler.GetAllSubEnterprisesFromParent(foundEnterprise.Guid));
                }
            }

            return enterprises;
        }

        /// <summary>
        /// Create a new Enterprise with values for Root and Parent Enterprise
        /// Uses local transaction if no Transaction is set
        /// </summary>
        /// <param name="enterprise"></param>
        public void Create(EMDEnterprise enterprise)
        {
            CoreTransaction transaction;
            bool hasLocalTranscation = false;

            if (this.Transaction == null)
            {
                transaction = new CoreTransaction();
                hasLocalTranscation = true;
            }
            else
            {
                transaction = this.Transaction;
            }

            EnterpriseHandler enterpriseHandler = new EnterpriseHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);

            try
            {
                if (hasLocalTranscation)
                {
                    transaction.Begin();
                }

                var dbCtx = transaction.dbContext;
                var parent = (from e in dbCtx.Enterprise where e.Guid == enterprise.Guid_Parent select e).SingleOrDefault();
                if (parent != null)
                {
                    enterprise.Guid_Root = parent.Guid_Root ?? parent.Guid; // nach oben suchen von newEnte.Guid_Parent aus bis nach ganz oben, diese Guid verwenden
                    enterprise.E_ID_Parent = parent.E_ID_new.Value;
                    enterprise.E_ID_Root = parent.E_ID_Root;
                }


                var orgDis = (from o in dbCtx.OrgUnit where o.Guid == enterprise.O_Guid_Dis select o.O_ID).SingleOrDefault();
                var orgProf = (from o in dbCtx.OrgUnit where o.Guid == enterprise.O_Guid_Prof select o.O_ID).SingleOrDefault();

                // old IDs
                enterprise.O_ID_Dis = orgDis;
                enterprise.O_ID_Prof = orgProf;

                enterprise = (EMDEnterprise)enterpriseHandler.CreateObject(enterprise);

                if (string.IsNullOrEmpty(enterprise.Guid_Parent))
                {
                    enterprise.Guid_Parent = enterprise.Guid;
                    enterprise.Guid_Root = enterprise.Guid;

                    enterprise.E_ID_Parent = enterprise.E_ID; // selbe id wie E_ID
                    enterprise.E_ID_Root = enterprise.E_ID;

                    enterpriseHandler.UpdateObject(enterprise, false);
                }

                if (hasLocalTranscation)
                {
                    transaction.Commit();
                }

            }
            catch (Exception ex)
            {
                if (hasLocalTranscation)
                {
                    transaction.Rollback();
                }

                string msg = "error trying to set guid_root and old IDs for new enterprise";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }
        }


        /// <summary>
        /// Updates a Enterprise with values for Root and Parent Enterprise
        /// Uses local transaction if no Transaction is set
        /// </summary>
        /// <param name="enterprise"></param>
        public void Update(EMDEnterprise enterprise)
        {
            CoreTransaction transaction;
            bool hasLocalTranscation = false;

            if (this.Transaction == null)
            {
                transaction = new CoreTransaction();
                hasLocalTranscation = true;
            }
            else
            {
                transaction = this.Transaction;
            }

            EnterpriseHandler enterpriseHandler = new EnterpriseHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);

            try
            {
                if (hasLocalTranscation)
                {
                    transaction.Begin();
                }

                var dbCtx = transaction.dbContext;
                var parent = (from e in dbCtx.Enterprise where e.Guid == enterprise.Guid_Parent select e).SingleOrDefault();
                if (parent != null)
                {
                    enterprise.Guid_Root = parent.Guid_Root ?? parent.Guid; // nach oben suchen von newEnte.Guid_Parent aus bis nach ganz oben, diese Guid verwenden
                    enterprise.E_ID_Parent = parent.E_ID_new.Value;
                    enterprise.E_ID_Root = parent.E_ID_Root;
                }


                if (string.IsNullOrEmpty(enterprise.Guid_Parent))
                {
                    enterprise.Guid_Parent = enterprise.Guid;
                    enterprise.Guid_Root = enterprise.Guid;
                }

                enterpriseHandler.UpdateObject(enterprise, false);

                if (hasLocalTranscation)
                {
                    transaction.Commit();
                }

            }
            catch (Exception ex)
            {
                if (hasLocalTranscation)
                {
                    transaction.Rollback();
                }

                string msg = "error trying to set guid_root and old IDs for new enterprise";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }
        }

        public EMDEnterprise Delete(string guid)
        {
            EnterpriseHandler handler = new EnterpriseHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDEnterprise emdEnterprise = Get(guid);
            if (emdEnterprise != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDEnterprise)handler.DeleteObject<EMDEnterprise>(emdEnterprise);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The enterprise with guid: {0} was not found.", guid));
            }
        }


    }
}
