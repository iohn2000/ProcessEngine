using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class CountryManager
        : BaseManager
        , ICountryManagercs
    {
        #region Constructors

        public CountryManager()
            : base()
        {
        }

        public CountryManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public CountryManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public CountryManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDCountry Get(string guid)
        {
            CountryHandler handler = new CountryHandler(this.Transaction);

            return (EMDCountry)handler.GetObject<EMDCountry>(guid);
        }

        public EMDCountry Delete(string guid)
        {
            CountryHandler handler = new CountryHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDCountry emdCountry = Get(guid);
            if (emdCountry != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDCountry)handler.DeleteObject<EMDCountry>(emdCountry);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Country with guid: {0} was not found.", guid));
            }
        }
    }
}
