namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectAnswers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectAnswerDatas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        QuestionId = c.Guid(nullable: false),
                        Text = c.String(),
                        ClientData_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClientDatas", t => t.ClientData_Id)
                .Index(t => t.ClientData_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectAnswerDatas", "ClientData_Id", "dbo.ClientDatas");
            DropIndex("dbo.ProjectAnswerDatas", new[] { "ClientData_Id" });
            DropTable("dbo.ProjectAnswerDatas");
        }
    }
}
