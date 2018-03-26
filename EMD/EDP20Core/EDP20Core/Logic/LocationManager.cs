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
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class LocationManager
        : BaseManager
        , ILocationManager
    {
        #region Constructors

        public LocationManager()
            : base()
        {
        }

        public LocationManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public LocationManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public LocationManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDLocation Get(string guid)
        {
            LocationHandler handler = new LocationHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDLocation)handler.GetObject<EMDLocation>(guid);
        }

        public List<EMDLocation> GetList()
        {
            return GetList(null);
        }

        public List<EMDLocation> GetList(string whereClause)
        {
            return new LocationHandler(this.Transaction).GetObjects<EMDLocation, Location>(whereClause).Cast<EMDLocation>().ToList();
        }

        public EMDLocation Delete(string guid)
        {
            LocationHandler handler = new LocationHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDLocation emdLocation = Get(guid);
            if (emdLocation != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDLocation)handler.DeleteObject<EMDLocation>(emdLocation);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Location with guid: {0} was not found.", guid));
            }
        }
    }
}
