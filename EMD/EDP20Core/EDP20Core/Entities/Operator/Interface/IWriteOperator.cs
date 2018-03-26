using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.Operator.Interface
{
    /// <summary>
    /// Defines methods to write and update objects of the specific type from the database
    /// </summary>
    /// <typeparam name="EMDType">Type of the specific EMD-implementation.</typeparam>
    /// <typeparam name="EFType">Type of the database-object in the entity-framework.</typeparam>
    public interface IWriteOperator<EFType, EMDType> where EMDType : IEMDObject<EMDType>, new()
    {
        /// <summary>
        /// GUID of the Person which uses this operator to modify the database.
        /// </summary>
        EMDGuid GUID_ModifiedBy { get; }

        /// <summary>
        /// Comment to describe the modifications made.
        /// </summary>
        string ModificationComment { get; }

        /// <summary>
        /// Writes the given object to the database. Automatically sets validity and activity (if not already set).
        /// </summary>
        /// <param name="toWrite">The object to write. Mustn't be <see langword="null"/>.</param>
        /// <param name="generateGuid">If <see langword="true"/> generates a new GUID and overrides the one from the object <paramref name="toWrite"/> is checked and used. Default is <see langword="true"/>.</param>
        /// <returns>The object with all changes made during the write-process (validity, activity and GUID)</returns>
        EMDType Write(EMDType toWrite, bool generateGuid = true);

        /// <summary>
        /// Updates the object. Automatically sets validity. Only allows active objects to be updated. The active-values are allowed to change during the update. 
        /// </summary>
        /// <param name="toUpdate">Object to update. Has to exist in the database.</param>
        /// <returns>Returns the updated object.</returns>
        EMDType Update(EMDType toUpdate);

        /// <summary>
        /// Deletes the given object on the given <paramref name="dueDate"/> or on <see cref="DateTime.Now"/> if <paramref name="dueDate"/> is <see langword="null"/>. 
        /// The object has to be active and mustn't have dependencies in the database. If the method should ignore violations of this requirements the parameter <paramref name="force"/> can be set to <see langword="true"/>.
        /// </summary>
        /// <param name="toDelete">Object to delete. Has to exist in the database.</param>
        /// <param name="dueDate">DateTime of the deletion or <see langword="null"/> if the object should be deleted immediately. Default is <see langword="null"/>.</param>
        /// <param name="force">If <see langword="true"/> the object is deleted even tho there are dependencies in the database. Default is <see langword="false"/>.</param>
        /// <returns>Returns the deleted object.</returns>
        EMDType Delete(EMDType toDelete, DateTime? dueDate = null, bool force = false);
    }
}
