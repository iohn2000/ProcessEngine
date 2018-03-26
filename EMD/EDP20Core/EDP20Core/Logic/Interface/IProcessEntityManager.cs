using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    /// <summary>
    /// Interface for ProcessEntity managers (concrete implementation and mockups)
    /// Use the Factory-Class 'Manager.cs' for initialization
    /// </summary>
    public interface IProcessEntityManager : IBaseManager
    {
        EMDProcessEntity Get(string guid);

        List<EMDProcessEntity> GetList();

        List<EMDProcessEntity> GetList(string whereClause);

        EMDProcessEntity Create(EMDProcessEntity emdAccount);

        EMDProcessEntity Create(string woinGuid, string entityGuid, string wodeGuid, string wodeName, string emplRequestedGuid, string effectedPersGuid, DateTime targetDate);

        EMDProcessEntity Update(EMDProcessEntity emdAccount);

        EMDProcessEntity UpdateOrCreate(EMDProcessEntity pren);

        EMDProcessEntity Delete(string guid);


    }
}
