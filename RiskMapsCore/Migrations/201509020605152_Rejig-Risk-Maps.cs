namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RejigRiskMaps : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Risks", "RiskMap_Id", "dbo.RiskMaps");
            DropIndex("dbo.Risks", new[] { "RiskMap_Id" });
            AddColumn("dbo.RiskMaps", "RiskIds", c => c.String());
            DropColumn("dbo.Risks", "RiskMap_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Risks", "RiskMap_Id", c => c.Guid());
            DropColumn("dbo.RiskMaps", "RiskIds");
            CreateIndex("dbo.Risks", "RiskMap_Id");
            AddForeignKey("dbo.Risks", "RiskMap_Id", "dbo.RiskMaps", "Id");
        }
    }
}
