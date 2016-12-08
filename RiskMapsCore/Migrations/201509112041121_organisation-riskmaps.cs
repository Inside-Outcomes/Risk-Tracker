namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class organisationriskmaps : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrganisationDatas", "RiskMapIds", c => c.String());
            Sql("UPDATE dbo.OrganisationDatas SET RiskMapIds = (SELECT id FROM dbo.RiskMaps WHERE name='Living Well')");
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrganisationDatas", "RiskMapIds");
        }
    }
}
