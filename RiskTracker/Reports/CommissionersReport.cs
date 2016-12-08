using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class CommissionersReport {
    public static ReportData<Row> build(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      var repo = new ReportsRepository();
      var riskMap = repo.projectRiskMap(projId);

      var rows = new List<Row>();
      var clients = repo.findClients(projId, locId);

      var clientsDischarged = 0;
      var clientsActive = 0;
      var clientsAssessed = 0;
      foreach (var clientData in clients) {
        if (!Reports.activeBetweenDates(clientData, startDate, endDate))
          continue;

        if (clientData.Discharged == true)
          ++clientsDischarged;
        else
          ++clientsActive;

        if (Reports.inBounds(clientData.registeredOn().Value, startDate, endDate)) 
          ++clientsAssessed;

        var client = new Client(clientData, riskMap);
        foreach(var theme in client.CurrentRiskAssessment.ThemeAssessments) {
          var row = rows.SingleOrDefault(r => r.Theme == theme.Title);
          if (row == null) {
            row = new Row(theme.Title);
            rows.Add(row);
            rows.Sort((l, r) => l.Theme.CompareTo(r.Theme));
          } // if ...

          if (client.Discharged != null)
            row.discharged(theme);
          else
            row.current(theme);
        } // foreach
      } // foreach

      if (endDate.HasValue)
        endDate = endDate.Value.AddDays(-1);
      var reportData = new ReportData<Row>(rows);
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("location", repo.locationName(locId));
      reportData.Put("startDate", Reports.formatDate(startDate));
      reportData.Put("endDate", Reports.formatDate(endDate));
      reportData.Put("clientsActive", clientsActive.ToString());
      reportData.Put("clientsDischarged", clientsDischarged.ToString());
      reportData.Put("clientsAssessed", clientsAssessed.ToString());
      reportData.Put("csvurl", Reports.csvUrl("commissioners", orgId, projId, locId, startDate, endDate));
      return reportData;
    } // build

    private static bool inBounds(Client client, DateTime? startDate, DateTime? endDate) {
      foreach (var timeline in client.Timeline) {
        var d = timeline.Datestamp;
        if (Reports.inBounds(d, startDate, endDate))
          return true;
      } // foreach
      return false;
    } // inBounds

    public static string buildCSV(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      ReportData<Row> reportData = build(orgId, projId, locId, startDate, endDate);
      var csv = new StringWriter();
      csv.WriteLine("Project," + reportData.Additional["project"]);
      csv.WriteLine("Location," + reportData.Additional["location"]);
      csv.WriteLine("Active clients," + reportData.Additional["clientsActive"]);
      csv.WriteLine("Assessed clients," + reportData.Additional["clientsAssessed"]);
      csv.WriteLine("Discharged clients," + reportData.Additional["clientsDischarged"]);

      csv.WriteLine(",Assessment, Current, Discharge");
      foreach(var r in reportData.Data) 
        csv.WriteLine(String.Format("Average {0} score,{1},{2},{3}", r.Theme, r.AssessmentScore, r.CurrentScore, r.DischargeScore));
      csv.WriteLine(",Assessment, Current, Discharge");
      foreach(var r in reportData.Data) 
        csv.WriteLine(String.Format("Average number of {0} issues,{1},{2},{3}", r.Theme, r.AssessmentIssues, r.CurrentIssues, r.DischargeIssues));

      foreach(var r in reportData.Data) {
        csv.WriteLine(String.Format("Clients with improved {0} score, {1}%", r.Theme, r.Percentage));
        if (r.Reduced != null)
          csv.WriteLine(String.Format("Clients with reduced number of {0} issues, {1}%", r.Theme, r.Reduced));
      }

      return csv.ToString();
    } // buildCSV

    public class Row {
      private static string[] LetterScores = new string[] { "D", "C", "B", "A"};
      private string theme_;
      private int count_;
      private int improved_;
      private int reduced_;

      private int currentCount_;
      private int dischargeCount_;
      private int assessmentsScores_;
      private int currentScores_;
      private int dischargeScores_;
      private int assessmentIssues_;
      private int currentIssues_;
      private int dischargeIssues_;

      private bool letterScore_;

      public Row(string label) { theme_ = label; }

      public void current(RiskAssessment.ThemeAssessment theme) {
        ++currentCount_;
        currentIssues_ += theme.RiskCount;
        currentScores_ += score(theme.Score);

        handle(theme);
      } // current

      public void discharged(RiskAssessment.ThemeAssessment theme) {
        ++dischargeCount_;
        dischargeIssues_ += theme.RiskCount;
        dischargeScores_ += score(theme.Score);

        handle(theme);
      } // discharge

      private void handle(RiskAssessment.ThemeAssessment theme) {
        ++count_;
        assessmentIssues_ += theme.InitialRiskCount;
        assessmentsScores_ += score(theme.InitialScore);

        bool improvedScore = theme.HighScore != null && !theme.HighScore.Equals(theme.Score);
        bool reducedRiskCount = theme.RiskCount < theme.HighRiskCount;
        if (improvedScore)
          ++improved_;
        if (reducedRiskCount)
          ++reduced_;
      }

      private int score(string s) {
        if (s == null)
          return 0;
        for (int i = 0; i < LetterScores.Length; ++i) {
          if (LetterScores[i] == s) {
            letterScore_ = (i > 0) || letterScore_;
            return i;
          }
        }

        return Int32.Parse(s);
      } // score

      public string Theme { get { return theme_; } }
      
      public string AssessmentScore { get { return avgScore(assessmentsScores_, count_); } }
      public string CurrentScore { get { return avgScore(currentScores_, currentCount_); } }
      public string DischargeScore { get { return avgScore(dischargeScores_, dischargeCount_); } }

      private string avgScore(int runningScores, int count) {
        if (count == 0)
          return null;
        float avg = (float)runningScores/count;
        int iavg = Convert.ToInt16(avg);
        if (!letterScore_)
          return iavg.ToString();
        return LetterScores[iavg];
      }

      public string AssessmentIssues { get { return avgIssues(assessmentIssues_, count_); } }
      public string CurrentIssues { get { return avgIssues(currentIssues_, currentCount_); } }
      public string DischargeIssues { get { return avgIssues(dischargeIssues_, dischargeCount_); } }

      private string avgIssues(int issueCount, int count) {
        if (count == 0)
          return null;

        float avg = (float)issueCount / count;
        int iavg = Convert.ToInt16(avg);
        return iavg.ToString();
      } // avgIssues

      public int Percentage { get { 
        if (count_ != 0)
          return (int)(improved_ * 100.0 / count_); 
        return 0;
      } }
      public int? Reduced { get {
        if ("personal circumstances".Equals(theme_.ToLower()))
          return reduced_;
        return null;
      } }
    } // class Row

  } // CommissionersReport
} // RiskTracker.Reports