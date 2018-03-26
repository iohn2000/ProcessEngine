using Kapsch.IS.EDP.Core.Framework;
using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Interface extending the <see cref="IEMDObjectHandler"/> with methods to get, modify and create database-objects.
    /// </summary>
    public interface IEMDBaseObjectHandler
        : IEMDObjectHandler
    {
        /// <summary>
        /// Get the database object with the given GUID.
        /// </summary>
        /// <typeparam name="T">Concrete Type of the database-object.</typeparam>
        /// <param name="guid">GUID of the object to get.</param>
        /// <param name="dbObjectType">Type of the database-object in the Entity-Framework (<see cref="Kapsch.IS.EDP.Core.DB"/>).</param>
        /// <returns>Returns the database-object with the given GUID or <see langword="null"/> if and only if no such object exists.</returns>
        IEMDObject<T> GetObject<T>(String guid, Type dbObjectType);

        /// <summary>
        /// Get the database-object that matches the given WHERE-clause.
        /// </summary>
        /// <typeparam name="T">Concrete Type of the database-object.</typeparam>
        /// <param name="clause">SQL-WHERE-clause to filter for a specific object. Throws an exception if it contains database-columns that are not in the table.</param>
        /// <param name="dbObjectType">Type of the database-object in the Entity-Framework (<see cref="Kapsch.IS.EDP.Core.DB"/>).</param>
        /// <returns>Returns the database-object with the given GUID, <see langword="null"/> if and only if no such object exists or throws an exception if more than one object matches the WHERE-clause.</returns>
        IEMDObject<T> GetObjectByClause<T>(String clause, Type dbObjectType);

        /// <summary>
        /// Writes updated of the given database-object to the database with correct historization.
        /// </summary>
        /// <typeparam name="T">Concrete Type of the database-object.</typeparam>
        /// <param name="emdObject">Updated database-object.</param>
        /// <returns>The updated-object because some fields are changed during the process.</returns>
        IEMDObject<T> UpdateObject<T>(IEMDObject<T> emdObject, bool trackModifier = true);

        /// <summary>
        /// Write a new database-object to the database.
        /// </summary>
        /// <typeparam name="T">Concrete Type of the database-object.</typeparam>
        /// <param name="emdObject">Object to write</param>
        /// <returns>The object because some fields are changed during the process.</returns>
        IEMDObject<T> CreateObject<T>(IEMDObject<T> emdObject, bool trackModifier = true);

    }
}