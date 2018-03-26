namespace Kapsch.IS.EDP.Core.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Kapsch.IS.EDP.Core.DB.EMD_Entities>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Kapsch.IS.EDP.Core.DB.EMD_Entities context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new EMDPerson { FullName = "Andrew Peters" },
            //      new EMDPerson { FullName = "Brice Lambson" },
            //      new EMDPerson { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
