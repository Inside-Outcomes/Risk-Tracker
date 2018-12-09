using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class ProgressReport {
    public static ReportData<ProgressRow> build(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      var repo = new ReportsRepository();
      var riskMap = repo.projectRiskMap(orgId, projId);
      var clients = repo.findClients(projId, locId);

      int clientCount = 0;
      var progress = new List<ProgressRow>();
      progress.Add(new ProgressRow("A"));
      progress.Add(new ProgressRow("B"));
      progress.Add(new ProgressRow("C"));
      progress.Add(new ProgressRow("D"));
      var totals = new ProgressRow("Total");

      foreach (var clientData in clients) {
        if (!Reports.activeBetweenDates(clientData, startDate, endDate))
          continue;
        if (clientData.RiskAssessments == null || clientData.RiskAssessments.Count == 0)
          continue;

        var client = new Client(clientData, riskMap);
        var riskAssessment = client.CurrentRiskAssessment;
        var ta = riskAssessment.ThemeAssessments.Where(t => t.Title.ToLower().Equals("personal circumstances")).First();
        string initialScore = massageScore(ta.InitialScore);
        string highScore = massageScore(ta.HighScore);

        progress[initialScore[0] - 'A'].Add(highScore);
        totals.Add(highScore);

        ++clientCount;
      } // foreach Client

      progress.Add(totals);
      ReportData<ProgressRow> reportData = new ReportData<ProgressRow>(progress);
      reportData.Put("clientCount", clientCount.ToString());
      reportData.Put("csvurl", Reports.csvUrl("pcprogress", orgId, projId, locId, startDate, endDate));
      return reportData;
    } // buildProgressReport

    public static void buildCSV(StreamWriter csv, Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      ReportData<ProgressRow> reportData = build(orgId, projId, locId, startDate, endDate);
      
      csv.WriteLine(reportData.Additional["clientCount"] + " clients");
      if (startDate.HasValue)
        csv.WriteLine("Start date " + startDate.Value.ToString("yyyy-MM-dd"));
      if (endDate.HasValue)
        csv.WriteLine("End date " + endDate.Value.ToString("yyyy-MM-dd"));
      csv.WriteLine(",A,B,C,D,Total");

      foreach (var row in reportData.Data) {
        csv.Write(row.Score + ",");
        foreach (var p in row.Progress) {
          csv.Write(p);
          csv.Write(',');
        } // foreach
        csv.WriteLine(row.Total);
      }

      csv.Close();
    } // buildCSV

    private static string massageScore(string score) {
      var validScores = new string[] { "A", "B", "C" };
      if (validScores.Contains(score))
        return score;
      return "D";
    } // massageScore

    public class ProgressRow {
      private string initialScore_;
      private int total_;
      private int[] progress_;

      public ProgressRow(string initialScore) {
        initialScore_ = initialScore;
        progress_ = new int[] { 0, 0, 0, 0 };
        total_ = 0;
      } // ProgressRow

      public void Add(string score) {
        var index = score[0] - 'A';
        progress_[index]++;
        ++total_;
      }

      public string Score { get { return initialScore_; } }
      public int[] Progress { get { return progress_; } }
      public int Total { get { return total_; } }
    } // ProgressRow
  }
}