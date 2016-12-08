namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Discharge_Flag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientDatas", "Discharged", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClientDatas", "Discharged");
        }
    }
}
