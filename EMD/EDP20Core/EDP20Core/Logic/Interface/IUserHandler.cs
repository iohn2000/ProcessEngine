using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IUserHandler
        : IEMDObjectHandler
    {
        List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName, string prefix = "");
    }
}
