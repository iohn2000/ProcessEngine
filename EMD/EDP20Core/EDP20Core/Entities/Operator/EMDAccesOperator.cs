using Kapsch.IS.EDP.Core.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Utils;
using System.Linq.Expressions;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities.Operator
{
    [Obsolete("Only implementation-recommendation!", true)]
    public sealed class EMDAccessOperator : BaseOperator<Access, EMDAccess>
    {
        public override string EntityPrefix { get { return "ACCE"; } }

        protected override Expression<Func<Access, bool>> IsValid { get { return (dbObj => dbObj.ValidFrom < WorkingDate && dbObj.ValidTo > WorkingDate); } }

        protected override Expression<Func<Access, bool>> IsActive { get { return (dbObj => dbObj.ActiveFrom < WorkingDate && dbObj.ActiveTo > WorkingDate); } }

        public EMDAccessOperator(string modifyComment, EMDGuid modifiedBy = null, CoreTransaction operatorTransaction = null, DateTime? workingDate = null)
            : base(modifyComment, modifiedBy, operatorTransaction, workingDate) { }

        protected override Expression<Func<Access, bool>> HasHistoryGuid(EMDGuid historyGuid)
        {
            return dbObj => dbObj.HistoryGuid == historyGuid.ToString();
        }

        protected override Expression<Func<Access, bool>> IsActiveIn(DateTime from, DateTime to)
        {
            return dbObj => dbObj.ActiveFrom < to && dbObj.ActiveTo > from;
        }

        protected override Expression<Func<Access, bool>> IsValidIn(DateTime from, DateTime to)
        {
            return dbObj => dbObj.ValidFrom < to && dbObj.ValidTo > from;
        }
    }
}
