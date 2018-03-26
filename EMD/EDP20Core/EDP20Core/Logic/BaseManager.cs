using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic.Interface;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class BaseManager
        : IBaseManager
    {
        #region Properties

        /// <summary>
        /// GUID of the person ("PERS") which uses the manager.
        /// </summary>
        public string Guid_ModifiedBy { get; set; }

        /// <summary>
        /// Modification comment for all changes made with this manager.
        /// </summary>
        public string ModifyComment { get; set; }

        /// <summary>
        /// Transaction to use for database-access.
        /// </summary>
        public CoreTransaction Transaction { get; set; }

        /// <summary>
        /// Does just the checks and does not start the workflows, and also does not write to EDP-database
        /// Only implemented for DoOffboarding
        /// </summary>
        public bool ReadOnlyMode { get; set; } = false;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates new instance of this class.
        /// </summary>
        public BaseManager()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Creates new instance of this class with the given external transaction.
        /// </summary>
        /// <param name="transaction"><see cref="Transaction"/></param>
        public BaseManager(CoreTransaction transaction)
            : this(transaction, null, null)
        {
        }

        /// <summary>
        /// Creates new instance of this class.
        /// </summary>
        /// <param name="guid_ModifiedBy"><see cref="Guid_ModifiedBy"/></param>
        /// <param name="modifyComment"><see cref="ModifyComment"/></param>
        public BaseManager(string guid_ModifiedBy, string modifyComment = null)
            : this(null, guid_ModifiedBy, modifyComment)
        {
        }

        /// <summary>
        /// Creates new instance of this class with the given external transaction.
        /// </summary>
        /// <param name="transaction"><see cref="Transaction"/></param>
        /// <param name="guid_ModifiedBy"><see cref="Guid_ModifiedBy"/></param>
        /// <param name="modifyComment"><see cref="ModifyComment"/></param>
        public BaseManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
        {
            this.Transaction = transaction;
            this.Guid_ModifiedBy = guid_ModifiedBy;
            this.ModifyComment = modifyComment;
        }

        #endregion Constructors
    }
}
