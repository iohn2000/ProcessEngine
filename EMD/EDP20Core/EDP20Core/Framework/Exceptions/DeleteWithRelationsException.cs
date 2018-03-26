using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class DeleteWithRelationsException : BaseException
    {
        public EMDGuid ObjGuid { get; }
        public IReadOnlyDictionary<string, int> RelatedEntities { get; }

        public DeleteWithRelationsException(EMDGuid guid, IReadOnlyDictionary<string, int> related) : base(ErrorCodeHandler.E_DB_GENERAL_ERROR)
        {
            ObjGuid = guid;
            RelatedEntities = related;
        }

        public DeleteWithRelationsException(EMDGuid guid, IReadOnlyDictionary<string, int> related, string msg) : base(ErrorCodeHandler.E_DB_GENERAL_ERROR, msg)
        {
            ObjGuid = guid;
            RelatedEntities = related;
        }
    }
}
