namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Make_Dob_Nullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DemographicDatas", "Dob", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DemographicDatas", "Dob", c => c.DateTime());
        }
    }
}
