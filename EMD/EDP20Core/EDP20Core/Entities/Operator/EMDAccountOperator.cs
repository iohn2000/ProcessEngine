using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.Operator
{
    [Obsolete("Only implementation-recommendation!", true)]
    public sealed class EMDAccountOperator : BaseOperator<Account, EMDAccount>
    {
        public override string EntityPrefix { get { return "ACCO"; } }

        protected override Expression<Func<Account, bool>> IsValid { get { return (dbObj => dbObj.ValidFrom < WorkingDate && dbObj.ValidTo > WorkingDate); } }

        protected override Expression<Func<Account, bool>> IsActive { get { return (dbObj => dbObj.ActiveFrom < WorkingDate && dbObj.ActiveTo > WorkingDate); } }

        public EMDAccountOperator(string modifyComment, EMDGuid modifiedBy = null, CoreTransaction operatorTransaction = null, DateTime? workingDate = null)
            : base(modifyComment, modifiedBy, operatorTransaction, workingDate) { }

        protected override Expression<Func<Account, bool>> HasHistoryGuid(EMDGuid historyGuid)
        {
            return dbObj => dbObj.HistoryGuid == historyGuid.ToString();
        }

        protected override Expression<Func<Account, bool>> IsActiveIn(DateTime from, DateTime to)
        {
            return dbObj => dbObj.ActiveFrom < to && dbObj.ActiveTo > from;
        }

        protected override Expression<Func<Account, bool>> IsValidIn(DateTime from, DateTime to)
        {
            return dbObj => dbObj.ValidFrom < to && dbObj.ValidTo > from;
        }
    }
}
