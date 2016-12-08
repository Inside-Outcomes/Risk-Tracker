using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class ResolutionReport {
    public static ReportData<ThemeRow> build(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      ProjectOrganisationRepository poRepo = new ProjectOrganisationRepository();
      RiskMapRepository rmRepo = new RiskMapRepository();
      ClientRepository clientRepo = new ClientRepository();

      ProjectOrganisation po = poRepo.Get(orgId);
      Project project = poRepo.FindProject(orgId, projId);
      RiskMap riskMap = rmRepo.RiskMap(project.RiskFramework);
      IList<ClientData> clients = clientRepo.ProjectClientData(projId, locId);

      IDictionary<Guid, ResolutionRow> report = new Dictionary<Guid, ResolutionRow>();
      foreach (var risk in riskMap.Risks())
        report.Add(risk.Id, new ResolutionRow(risk));

      HashSet<Guid> clientsSeen = new HashSet<Guid>();
      foreach (var client in clients) {
        if (client.RiskAssessments == null || client.RiskAssessments.Count == 0)
          continue;
        foreach (var rad in client.RiskAssessments) {
          if (Reports.outOfBounds(rad.Timestamp, startDate, endDate))
            continue;
          foreach (var riskId in rad.Risks()) {
            ResolutionRow row = report[riskId];
            row.Open(client);
          }
          foreach (var riskId in rad.ResolvedRisks()) {
            ResolutionRow row = report[riskId];
            row.Close(client);
          }

          clientsSeen.Add(client.Id);
        } // foreach RiskAssessment
      } // foreach Client

      IList<ThemeRow> themes = new List<ThemeRow>();
      foreach (var theme in riskMap.AllThemes()) {
        var catRows = buildCatRows(theme, riskMap, report);
        themes.Add(new ThemeRow(theme, catRows));
      } // foreach

      ReportData<ThemeRow> reportData =
        new ReportData<ThemeRow>(themes);
      reportData.Put("clientCount", clientsSeen.Count.ToString());
      reportData.Put("csvurl", Reports.csvUrl("resolution", orgId, projId, locId, startDate, endDate));

      return reportData;
    } // resolutionReport

    public static string buildCSV(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      ReportData<ResolutionReport.ThemeRow> reportData = build(orgId, projId, locId, startDate, endDate);

      var csv = new StringWriter();
      csv.WriteLine(reportData.Additional["clientCount"] + " clients");
      if (startDate.HasValue)
        csv.WriteLine("Start date " + startDate.Value.ToString("yyyy-MM-dd"));
      if (endDate.HasValue)
        csv.WriteLine("End date " + endDate.Value.ToString("yyyy-MM-dd"));
      csv.WriteLine("Theme, Category, Risk, Opened, Closed, NIHCE, Improving Outcomes & Supporting Transparency, Healthy Child Programme, Social Justice OF, Adult Social Care OF ");
      foreach (var theme in reportData.Data)
        foreach (var cat in theme.Categories)
          foreach (var row in cat.Rows)
            csv.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",
              theme.Title,
              cat.Title,
              row.Title,
              row.Opened,
              row.Closed,
              row.NIHCEG,
              row.IOST,
              row.HCP,
              row.SJOF,
              row.ASCOF));

      return csv.ToString();
    } // buildCSV


    private static IList<CategoryRow> buildCatRows(
        string themeName,
        RiskMap riskMap,
        IDictionary<Guid, ResolutionRow> report) {
      var cats = new List<CategoryRow>();
      foreach (var category in riskMap.AllCategories()) {
        var catRisks = riskMap.Risks().
          Where(r => r.Theme == themeName).
          Where(r => r.Category == category);

        if (catRisks.Count() == 0)
          continue;

        var risks = catRisks.
          OrderBy(r => r.Grouping).
          ThenBy(r => riskScoreSort(r)).
          Select(r => report[r.Id]).
          ToList();

        cats.Add(new CategoryRow(category, risks));
      } // foreach
      return cats;
    } // buildCatRows

    private static string riskScoreSort(Risk r) {
      // sort alpha score ascending but numeric score decending
      int s;
      if (!int.TryParse(r.Score, out s))
        return r.Score;

      return (100 - s).ToString();
    } // riskScoreSort

    public class ThemeRow {
      private string title_;
      private IList<CategoryRow> cats_;

      public ThemeRow(string title, IList<CategoryRow> cats) {
        title_ = title;
        cats_ = cats;
      } // ThemeRow

      public string Title { get { return title_; } }
      public IList<CategoryRow> Categories { get { return cats_; } }
    } // ThemeRow

    public class CategoryRow {
      private string title_;
      private IList<ResolutionRow> rows_;

      public CategoryRow(string title, IList<ResolutionRow> rows) {
        title_ = title;
        rows_ = rows;
      } // ThemeRow

      public string Title { get { return title_; } }
      public IList<ResolutionRow> Rows { get { return rows_; } }
    } // ThemeRow

    public class ResolutionRow {
      private Risk risk;
      private ISet<ClientData> opened;
      private ISet<ClientData> closed;

      public ResolutionRow(Risk r) {
        risk = r;
        opened = new HashSet<ClientData>();
        closed = new HashSet<ClientData>();
      } // ResolutionRow

      public string Title { get { return risk.Title; } }
      public int Opened { get { return opened.Count; } }
      public int Closed { get { return closed.Count; } }
      public string NIHCEG { get { return risk.NIHCEG; } }
      public string IOST { get { return risk.IOST; } }
      public string HCP { get { return risk.HCP; } }
      public string SJOF { get { return risk.SJOF; } }
      public string ASCOF { get { return risk.ASCOF; } }

      public void Open(ClientData cd) { opened.Add(cd); }
      public void Close(ClientData cd) { closed.Add(cd); }
    } // ResolutionRow
  }
}