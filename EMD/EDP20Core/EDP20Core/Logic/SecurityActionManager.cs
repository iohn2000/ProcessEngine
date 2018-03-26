using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.Logging;
using System.Reflection;
using Kapsch.IS.Util.ErrorHandling;
using System.Data.Entity;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using System.Collections;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class SecurityActionManager
        : BaseManager
        , ISecurityActionManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public SecurityActionManager()
            : base()
        {
        }

        public SecurityActionManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public SecurityActionManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public SecurityActionManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDSecurityAction Get(string guid)
        {
            SecurityActionHandler securityActionHandler = new SecurityActionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDSecurityAction)securityActionHandler.GetObject<EMDSecurityAction>(guid);
        }

        public List<EMDSecurityAction> GetList()
        {
            SecurityActionHandler securityActionHandler = new SecurityActionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            List<IEMDObject<EMDSecurityAction>> list = securityActionHandler.GetObjects<EMDSecurityAction, SecurityAction>();

            List<EMDSecurityAction> emdSecurityActions = new List<EMDSecurityAction>();

            foreach (var item in list)
            {
                emdSecurityActions.Add((EMDSecurityAction)item);
            }

            return emdSecurityActions;
        }

        public EMDSecurityAction Create(EMDSecurityAction emdSecurityAction)
        {
            SecurityActionHandler handler = new SecurityActionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return (EMDSecurityAction)handler.CreateObject(emdSecurityAction);
        }

        public EMDSecurityAction Update(EMDSecurityAction emdSecurityAction)
        {
            SecurityActionHandler handler = new SecurityActionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return (EMDSecurityAction)handler.UpdateObject(emdSecurityAction);
        }

        public EMDSecurityAction Delete(string guid)
        {
            SecurityActionHandler handler = new SecurityActionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDSecurityAction securityAction = Get(guid);
            if (securityAction != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDSecurityAction)handler.DeleteObject<EMDSecurityAction>(securityAction);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The SecurityAction with guid: {0} was not found.", guid));
            }
        }

        public List<EMDSecurityAction> GetSecurityActions(string orgu_guid)
        {
            List<EMDSecurityAction> emdSecurityActions = new List<EMDSecurityAction>();
            SecurityActionHandler handler = new SecurityActionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            List<IEMDObject<EMDSecurityAction>> securityActions = (List<IEMDObject<EMDSecurityAction>>)handler.GetObjects<EMDSecurityAction, DB.SecurityAction>("ORGU_Guid='" + orgu_guid + "'");

            foreach (var item in securityActions)
            {
                emdSecurityActions.Add((EMDSecurityAction)item);
            }

            return emdSecurityActions;
        }
    }
}
