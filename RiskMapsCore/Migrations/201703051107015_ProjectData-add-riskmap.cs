namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectDataaddriskmap : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectDatas", "RiskMap", c => c.Guid(nullable: false));

            Sql("UPDATE dbo.ProjectDatas SET dbo.ProjectDatas.RiskMap = dbo.RiskMaps.Id FROM dbo.ProjectDatas INNER JOIN dbo.RiskMaps ON dbo.ProjectDatas.RiskFramework = dbo.RiskMaps.Name");
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectDatas", "RiskMap");
        }
    }
}
