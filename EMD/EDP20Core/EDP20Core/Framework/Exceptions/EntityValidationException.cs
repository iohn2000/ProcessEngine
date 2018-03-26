using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class EntityValidationException : BaseException
    {
        public string EntityType { get; }

        private Dictionary<string, string> violations;
        public IReadOnlyDictionary<string, string> ViolatedProperties
        {
            get
            {
                return violations as IReadOnlyDictionary<string, string>;
            }
        }

        public EntityValidationException(string entityType, string msg, DbEntityValidationException ex) : base(ErrorCodeHandler.E_DB_GENERAL_ERROR, msg, ex)
        {
            EntityType = entityType;
            violations = new Dictionary<string, string>();
            foreach (var entityViolation in ex.EntityValidationErrors)
            {
                foreach (var violation in entityViolation.ValidationErrors)
                {
                    violations.Add(violation.PropertyName, violation.ErrorMessage);
                }
            }
        }
    }
}
