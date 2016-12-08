namespace RiskTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity;
    using RiskTracker.Entities;
    
    public partial class Correct_Notes_Data : DbMigration
    {
        public override void Up()
        {
            DatabaseContext context = new DatabaseContext();
            foreach (var note in context.Notes) {
              note.Type = NoteType.Narrative;
              context.Entry(note).State = EntityState.Modified;
            } // foreach
            context.SaveChanges();
        }
        
        public override void Down()
        {
        }
    }
}
