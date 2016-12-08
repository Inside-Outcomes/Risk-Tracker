namespace RiskTracker.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class addsuspendedflagtoorgs : DbMigration
  {
    public override void Up()
    {
      AddColumn("dbo.OrganisationDatas", "IsSuspended", c => c.Boolean(nullable: false, defaultValue: false));
    }

    public override void Down()
    {
      DropColumn("dbo.OrganisationDatas", "IsSuspended");
    }
  }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
