using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities.Operator.Enum;
using Kapsch.IS.EDP.Core.Utils;

namespace Kapsch.IS.EDP.Core.Entities.Operator.Interface
{
    /// <summary>
    /// Defines methods to read objects of the specific type from the database
    /// </summary>
    /// <typeparam name="EMDType">Type of the specific EMD-implementation.</typeparam>
    /// <typeparam name="EFType">Type of the database-object in the entity-framework.</typeparam>
    public interface IReadOperator<EFType, EMDType> where EMDType : IEMDObject<EMDType>, new()
    {
        /// <summary>
        /// Reads the currently valid data-record for the object with the given <paramref name="guid"/>.
        /// </summary>
        /// <param name="guid">Guid of the object to get.</param>
        /// <returns>Returns the object from the database or <see langword="null"/> if no such object was found.</returns>
        /// <seealso cref="EMDGuid"/>
        EMDType Get(EMDGuid guid);

        /// <summary>
        /// Gets a list of data-records for the given object-guid according to <paramref name="searchType"/>.
        /// Each data-record is represented as an own <see cref="EMDObject{T}"/> where only one can be valid.
        /// </summary>
        /// <param name="guid">GUID of the object to get the history. Must be a valid guid of Regex([A-Z]{4}_[0-9a-f]{32}).</param>
        /// <param name="searchType">Defines what objects are added to the history.</param>
        /// <returns>List of objects representing the data-records in the history of an object according to <paramref name="searchType"/> or an empty list if no such objects were found.</returns>
        /// <seealso cref="EMDGuid"/>
        List<EMDType> GetHistory(EMDGuid guid, HistorySearchTypeEnum searchType);

        /// <summary>
        /// Gets all objects of the specific type where <paramref name="filter"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="filter">Filter applied to the query. Corresponds to a where-clause.</param>
        /// <returns>List of objects that are valid, active and where <paramref name="filter"/> is <see langword="true"/> or an empty list if no such objects were found.</returns>
        /// <overloads>Methods to query for multiple objects of the specific type.</overloads>
        List<EMDType> GetMultiple(Expression<Func<EFType, bool>> filter);

        /// <summary>
        /// Gets all objects of the specific type where <paramref name="filter"/> is <see langword="true"/> and that are active in the given timespan (<paramref name="from"/>-<paramref name="to"/>).
        /// </summary>
        /// <param name="from">Begin of the timespan.</param>
        /// <param name="to">End of the timespan.</param>
        /// <param name="filter">Filter applied to the query. Corresponds to a where-clause.</param>
        /// <returns>List of objects that are valid, active in the given timespan and where <paramref name="filter"/> is <see langword="true"/> or an empty list if no such objects were found.</returns>
        List<EMDType> GetMultiple(DateTime from, DateTime to, Expression<Func<EFType, bool>> filter);
    }
}
