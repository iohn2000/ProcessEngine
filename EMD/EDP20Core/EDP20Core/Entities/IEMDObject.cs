using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public interface IEMDObject<T>
    {
        DateTime Created { get; set; }
        string Guid { get; set; }
        DateTime? Modified { get; set; }
        DateTime ValidFrom { get; set; }
        DateTime ValidTo { get; set; }
        DateTime ActiveFrom { get; set; }
        DateTime ActiveTo { get; set; }
        string HistoryGuid { get; set; }

        string Guid_ModifiedBy { get; set; }
        string ModifyComment { get; set; }

        void Invalidate();
        void InvalidateBy(DateTime t);

        string CreateDBGuid();
        //DateTime GetMaxValidToDate();
        DateTime SetAsNew();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <param name="touchActiveTo"> if set to false do not let SetAsUpdated function change the ActiveTo date, true = default (as it was so far)</param>
        /// <returns></returns>
        DateTime SetAsUpdated(IEMDObject<T> oldVersion, bool touchActiveTo = true, bool touchActiveFrom = true);
        int FillEmptyDates(DateTime? effectDate = null);
        void SetModified();
        void SetValidityStatus();
        void Deactivate();
        void DeactivateBy(DateTime t);
        void Activate();
        void SetModifiedBy(string guid_ModifiedBy, string modifyComment);
    }
}