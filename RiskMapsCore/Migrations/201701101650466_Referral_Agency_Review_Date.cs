namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Referral_Agency_Review_Date : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReferralAgencyDatas", "ReviewDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReferralAgencyDatas", "ReviewDate");
        }
    }
}
