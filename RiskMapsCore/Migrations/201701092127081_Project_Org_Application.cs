namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Project_Org_Application : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectOrganisationDatas", "Application", c => c.String());

            Sql("UPDATE dbo.ProjectOrganisationDatas SET Application='RiskTracker'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectOrganisationDatas", "Application");
        }
    }
}
