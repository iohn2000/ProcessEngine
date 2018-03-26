using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class CoreTransaction
        : IDisposable
    {
        public DB.EMD_Entities dbContext = new DB.EMD_Entities();

        public System.Data.Entity.DbContextTransaction transaction;

        public event EventHandler RollBackEvent;
        
        public Action<string> DatabaseLogging
        {
            get { return this.dbContext.Database.Log; }
            set { this.dbContext.Database.Log = value; }
        }

        public void Begin()
        {
            transaction = dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
        }

        public virtual void Commit()
        {
            transaction.Commit();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }
            if(dbContext != null)
            {
                dbContext.Dispose();
                dbContext = null;
            }
        }

        public virtual void Rollback()
        {
            if (RollBackEvent != null)
            {
                RollBackEvent(this, null);
            }
            //TODO: Was passiert wenn hier eine Exception zieht?
            transaction.Rollback();
        }

        public virtual void saveChanges()
        {
            dbContext.SaveChanges();
        }

        protected void SetAutoDetectChanges(bool autoDetectChanges)
        {
            this.dbContext.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
        }

        protected bool GetAutoDetectChanges()
        {
            return this.dbContext.Configuration.AutoDetectChangesEnabled;
        }

        protected void SetValidationOnSave(bool validateOnSave)
        {
            this.dbContext.Configuration.ValidateOnSaveEnabled = validateOnSave;
        }

        protected bool GetValidationOnSave()
        {
            return this.dbContext.Configuration.ValidateOnSaveEnabled;
        }

    }
}
