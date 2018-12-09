using RiskTracker.Entities;
using RiskTracker.Models;
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

    private static Dictionary<Guid, string> projectNames_ = new Dictionary<Guid, string>();
    private static Dictionary<Guid, string> locationNames_ = new Dictionary<Guid, string>();

    public IQueryable<ClientData> findClients(Guid? projId, Guid? locId) {
      var clients = projId.HasValue ? allClients.Where(c => c.ProjectId == projId) : allClients;
      if (locId.HasValue)
        clients = clients.Where(c => c.LocationId == locId.Value);

      clients = clients.Include(c => c.Notes).
                        Include(c => c.RiskAssessments);

      return clients;
    } // findClients

    public RiskMap projectRiskMap(Guid? orgId, Guid projId) {
      using (var rmRepo = new RiskMapRepository(orgId)) {
        var riskMap = projects.Where(s => s.Id == projId).Single().RiskFramework;
        return rmRepo.RiskMap(riskMap);
      } // using ...
    } // projectRiskMap

    public IList<ProjectQuestionData> projectQuestions(Guid projId) {
      return projects.Where(s => s.Id == projId).Include(s => s.Questions).Single().Questions;
    }

    public string projectName(Guid projId) {
      if (projectNames_.ContainsKey(projId))
        return projectNames_[projId];
      var name = projects.Where(s => s.Id == projId).Single().Name;
      projectNames_[projId] = name;
      return name;
    } // projectName
    public string locationName(Guid? locId) {
      if (!locId.HasValue)
        return "All Locations";
      if (locationNames_.ContainsKey(locId.Value))
        return locationNames_[locId.Value];
      var name = location.Where(s => s.Id == locId.Value).Single().Name;
      locationNames_[locId.Value] = name;
      return name;
    } // locationName
  } // 
}