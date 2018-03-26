using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class RelatedEntitiesException : BaseException
    {
        public Hashtable RelatedEntities { get; set; }

        public RelatedEntitiesException(int errorCode, Hashtable relatedEntities) : base(errorCode)
        {
            this.RelatedEntities = relatedEntities;
        }

        public RelatedEntitiesException(int errorCode, string message, Hashtable relatedEntities) : base(errorCode, message)
        {
            this.RelatedEntities = relatedEntities;
        }
    }

}
