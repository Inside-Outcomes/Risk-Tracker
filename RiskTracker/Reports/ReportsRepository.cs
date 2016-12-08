using RiskTracker.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class ReportsRepository : BaseRepository {
    private DbSet<ClientData> allClients { get { return context.Clients; } }
    private DbSet<ProjectData> projects { get { return context.Projects; } }
    private DbSet<LocationData> location { get { return context.Locations; } }

    public IQueryable<ClientData> findClients(Guid projId, Guid? locId) {
      var clients = allClients.Where(c => c.ProjectId == projId);
      if (locId.HasValue)
        clients = clients.Where(c => c.LocationId == locId.Value);

      clients = clients.Include(c => c.Notes).
                        Include(c => c.RiskAssessments);

      return clients;
    } // findClients

    public RiskMap projectRiskMap(Guid projId) {
      using (var rmRepo = new RiskMapRepository()) {
        var riskFramework = projects.Where(s => s.Id == projId).Single().RiskFramework;
        return rmRepo.RiskMap(riskFramework);
      } // using ...
    } // projectRiskMap

    public IList<ProjectQuestionData> projectQuestions(Guid projId) {
      return projects.Where(s => s.Id == projId).Include(s => s.Questions).Single().Questions;
    }

    public string projectName(Guid projId) {
      return projects.Where(s => s.Id == projId).Single().Name;
    }
    public string locationName(Guid? locId) {
      if (!locId.HasValue)
        return "All Locations";
      return location.Where(s => s.Id == locId.Value).Single().Name;
    }
  } // 
}