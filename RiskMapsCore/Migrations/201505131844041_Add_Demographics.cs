namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Demographics : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DemographicDatas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EmploymentStatus = c.String(),
                        EthnicOrigin = c.String(),
                        Gender = c.String(),
                        Disability = c.String(),
                        DisabilityType = c.String(),
                        MaritalStatus = c.String(),
                        HouseholdType = c.String(),
                        HousingType = c.String(),
                        HouseholdIncome = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ClientDatas", "Demographics_Id", c => c.Guid());
            CreateIndex("dbo.ClientDatas", "Demographics_Id");
            AddForeignKey("dbo.ClientDatas", "Demographics_Id", "dbo.DemographicDatas", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClientDatas", "Demographics_Id", "dbo.DemographicDatas");
            DropIndex("dbo.ClientDatas", new[] { "Demographics_Id" });
            DropColumn("dbo.ClientDatas", "Demographics_Id");
            DropTable("dbo.DemographicDatas");
        }
    }
}
