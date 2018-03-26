using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// @@ func to get name of usertype
    /// param name="userType"
    /// param name="userGuid"
    /// </summary>
    /// <returns></returns>
    public class GetUserTypeName : EntityFunction<GetUserTypeName>
    {
        /// <summary>
        /// Return Name of usertype for givem ID
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override GetUserTypeName Worker()
        {

            UserHandler uh = new UserHandler();
            base.Result = uh.GetUserTypeName(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
