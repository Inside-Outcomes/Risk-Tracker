namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRisksToReferralAgency : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReferralAgencyDatas", "AssociatedRiskIds", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReferralAgencyDatas", "AssociatedRiskIds");
        }
    }
}
