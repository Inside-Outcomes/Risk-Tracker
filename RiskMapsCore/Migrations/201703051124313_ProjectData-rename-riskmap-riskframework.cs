namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectDatarenameriskmapriskframework : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.ProjectDatas", "RiskMap", "RiskFramework");
        }
        
        public override void Down()
        {
          RenameColumn("dbo.ProjectDatas", "RiskFramework", "RiskMap");
        }
    }
}
