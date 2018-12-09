namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectDataremoveriskframework : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ProjectDatas", "RiskFramework");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProjectDatas", "RiskFramework", c => c.String(nullable: false));
        }
    }
}
