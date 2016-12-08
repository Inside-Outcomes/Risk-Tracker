namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Dob : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DemographicDatas", "Dob", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DemographicDatas", "Dob");
        }
    }
}
