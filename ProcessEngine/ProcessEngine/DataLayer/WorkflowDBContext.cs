namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System.Data.Entity;

    public partial class WorkflowDBContext : DbContext
    {
        public WorkflowDBContext()
            : base("name=WorkflowDBConnStr")
        {
        }

        public virtual DbSet<ActivityDefinition> ActivityDefinitions { get; set; }
        public virtual DbSet<ActivityResultMessage> ActivityResultMessages { get; set; }
        public virtual DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
        public virtual DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public virtual DbSet<EngineAlert> EngineAlerts { get; set; }
        public virtual DbSet<AsyncWaitItem> AsyncWaitItems { get; set; }
        public virtual DbSet<TaskItem> TaskItems { get; set; }
        public virtual DbSet<TaskItemLink> TaskItemLinks { get; set; }
        public virtual DbSet<WorkflowXmlDataRepository> WorkflowXmlDataRepositorys { get; set; }
        public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public virtual DbSet<DocumentTemplateType> DocumentTemplateTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        private void FixEfProviderServicesProblem()
        {
            // The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            // for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            // Make sure the provider assembly is available to the running application. 
            // See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
