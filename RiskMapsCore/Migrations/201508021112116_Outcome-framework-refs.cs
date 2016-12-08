namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Outcomeframeworkrefs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Risks", "NIHCEG", c => c.String());
            AddColumn("dbo.Risks", "IOST", c => c.String());
            AddColumn("dbo.Risks", "HCP", c => c.String());
            AddColumn("dbo.Risks", "SJOF", c => c.String());
            AddColumn("dbo.Risks", "ASCOF", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Risks", "ASCOF");
            DropColumn("dbo.Risks", "SJOF");
            DropColumn("dbo.Risks", "HCP");
            DropColumn("dbo.Risks", "IOST");
            DropColumn("dbo.Risks", "NIHCEG");
        }
    }
}
