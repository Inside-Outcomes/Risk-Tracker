namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ManagedRisks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RiskAssessmentDatas", "ManagedRiskIds", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RiskAssessmentDatas", "ManagedRiskIds");
        }
    }
}
