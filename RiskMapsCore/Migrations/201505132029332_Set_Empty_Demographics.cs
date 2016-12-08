namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity;
    using System.Linq;
    using RiskTracker.Entities;
    
    public partial class Set_Empty_Demographics : DbMigration
    {
        public override void Up()
        {
          DatabaseContext context = new DatabaseContext();
          var noDemographics = context.Clients.Where(c => c.Demographics == null);
          foreach (var client in noDemographics) {
            client.Demographics = new DemographicData();
            client.Demographics.Id = Guid.NewGuid();
            context.Entry(client).State = EntityState.Modified;
          } // foreach
      
          context.SaveChanges();
      } // Up 
        
        public override void Down()
        {
        } // Down
    } // Set_Empty_Demographics
} // namespace ...

