namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
       
    public partial class Extend_Notes_With_Type : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NoteDatas", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NoteDatas", "Type");
        }
    }
}
