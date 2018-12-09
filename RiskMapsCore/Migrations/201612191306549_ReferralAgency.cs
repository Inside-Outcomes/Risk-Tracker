namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReferralAgency : DbMigration
    {
        public override void Up()
        {
            AddPrimaryKey("dbo.AddressDatas", "Id");
            AddPrimaryKey("dbo.ProjectOrganisationDatas", "Id");

            CreateTable(
                "dbo.ReferralAgencyDatas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        Address_Id = c.Guid(),
                        ProjectOrganisationData_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AddressDatas", t => t.Address_Id)
                .ForeignKey("dbo.ProjectOrganisationDatas", t => t.ProjectOrganisationData_Id)
                .Index(t => t.Address_Id)
                .Index(t => t.ProjectOrganisationData_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReferralAgencyDatas", "ProjectOrganisationData_Id", "dbo.ProjectOrganisationDatas");
            DropForeignKey("dbo.ReferralAgencyDatas", "Address_Id", "dbo.AddressDatas");
            DropIndex("dbo.ReferralAgencyDatas", new[] { "ProjectOrganisationData_Id" });
            DropIndex("dbo.ReferralAgencyDatas", new[] { "Address_Id" });
            DropTable("dbo.ReferralAgencyDatas");
            DropPrimaryKey("dbo.ProjectOrganisationDatas");
            DropPrimaryKey("dbo.AddressDatas");
        }
    }
}
