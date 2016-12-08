using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using RiskTracker.Models;

namespace RiskTracker.Entities {
  public class NoteRepository : BaseRepository {
    private DbSet<NoteData> notes { get { return context.Notes; } }

    public void RemoveNote(NoteData note) {
      Delete(note);
    } // RemoveNote
  }
}