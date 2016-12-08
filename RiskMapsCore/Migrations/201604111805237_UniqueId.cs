namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientDatas", "ReferenceId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClientDatas", "ReferenceId");
        }
    }
}
