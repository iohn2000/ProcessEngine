using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Linq.Expressions;

namespace Kapsch.IS.EDP.Core.Entities.Operator
{
    [Obsolete("Only implementation-recommendation!", true)]
    public sealed class EMDAccountGroupOperator : BaseOperator<AccountGroup, EMDAccountGroup>
    {
        public override string EntityPrefix { get { return "ACGR"; } }

        protected override Expression<Func<AccountGroup, bool>> IsValid { get { return (dbObj => dbObj.ValidFrom < WorkingDate && dbObj.ValidTo > WorkingDate); } }

        protected override Expression<Func<AccountGroup, bool>> IsActive { get { return (dbObj => dbObj.ActiveFrom < WorkingDate && dbObj.ActiveTo > WorkingDate); } }

        public EMDAccountGroupOperator(string modifyComment, EMDGuid modifiedBy = null, CoreTransaction operatorTransaction = null, DateTime? workingDate = null)
            : base(modifyComment, modifiedBy, operatorTransaction, workingDate) { }

        protected override Expression<Func<AccountGroup, bool>> HasHistoryGuid(EMDGuid historyGuid)
        {
            return dbObj => dbObj.HistoryGuid == historyGuid.ToString();
        }

        protected override Expression<Func<AccountGroup, bool>> IsActiveIn(DateTime from, DateTime to)
        {
            return dbObj => dbObj.ActiveFrom < to && dbObj.ActiveTo > from;
        }

        protected override Expression<Func<AccountGroup, bool>> IsValidIn(DateTime from, DateTime to)
        {
            return dbObj => dbObj.ValidFrom < to && dbObj.ValidTo > from;
        }
    }
}
