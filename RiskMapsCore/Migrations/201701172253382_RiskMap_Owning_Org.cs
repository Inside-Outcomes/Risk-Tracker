namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RiskMap_Owning_Org : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RiskMaps", "OwningOrganisation", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RiskMaps", "OwningOrganisation");
        }
    }
}
