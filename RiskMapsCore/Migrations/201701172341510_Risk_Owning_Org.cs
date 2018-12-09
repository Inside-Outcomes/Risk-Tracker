namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Risk_Owning_Org : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Risks", "OwningOrganisation", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Risks", "OwningOrganisation");
        }
    }
}
