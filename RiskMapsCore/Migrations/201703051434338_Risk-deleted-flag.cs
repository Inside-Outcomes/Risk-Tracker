namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Riskdeletedflag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Risks", "Deleted", c => c.Boolean(nullable: false));
            Sql("UPDATE dbo.Risks SET Deleted = 'False'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Risks", "Deleted");
        }
    }
}
