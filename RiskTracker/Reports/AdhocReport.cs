using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class AdhocReport {

    public static ReportData<Row> build(Guid orgId, Guid projId, Guid? locId, string field, DateTime? startDate, DateTime? endDate) {
      var repo = new ReportsRepository();
      var riskMap = repo.projectRiskMap(projId);
      var projectQs = repo.projectQuestions(projId);

      var classifier = howToClassify(projectQs, field);
      var rows = new List<Row>();
      var clientCount = 0;

      const int pageSize = 100;
      int page = 0;
      while (true) {
        var clients = repo.findClients(projId, locId).
                          OrderBy(c => c.Id).
                          Skip(page*pageSize).
                          Take(pageSize).
                          Include(c => c.Demographics).
                          Include(c => c.Address).
                          Include(c => c.ProjectAnswers).
                          ToList();

        if (clients.Count == 0)
          break;

        clients = clients.Where(c => Reports.activeBetweenDates(c, startDate, endDate)).ToList();

        foreach (var clientData in clients) {
          var client = new Client(clientData, riskMap, projectQs);

          ++clientCount;
          var classifications = classifier.classify(client, clientData.Notes);
          foreach (var classification in classifications) {
            var row = rows.SingleOrDefault(r => r.Label.ToLower() == classification.ToLower());
            if (row == null) {
              row = new Row(classification);
              rows.Add(row);
              rows.Sort((l, r) => l.Label.CompareTo(r.Label));
            } // if ...

            row.bump();
            foreach (var theme in client.CurrentRiskAssessment.ThemeAssessments) {
              var high = theme.HighScore;
              var current = theme.Score;

              bool improvedScore = high != null && !high.Equals(current);
              bool reducedRiskCount = theme.RiskCount < theme.HighRiskCount;

              row.bump(theme.Title, improvedScore, reducedRiskCount);
            } // foreach
          } // foreach
        } // foreach 
        ++page;
      } // page

      ReportData<Row> reportData = new ReportData<Row>(rows);
      reportData.Put("field", classifier.Label);
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("location", repo.locationName(locId));
      reportData.Put("clientCount", clientCount.ToString());
      reportData.Put("csvurl", Reports.csvUrl("adhoc", orgId, projId, locId, startDate, endDate) + "&field=" + field);
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

    public delegate string classifier(Client c, List<NoteData> notes);
    public delegate List<string> multiclassifier(Client c, List<NoteData> notes);

    public class Classifier {
      private string field_;
      private string label_;
      private classifier classifier_;
      private multiclassifier multiclassifier_;

      public Classifier(string field, 
                        string label,
                        classifier c) {
        field_ = field;
        label_ = label;
        classifier_ = c;
      } // Classifier
      public Classifier(string field,
                        string label,
                        multiclassifier c) {
        field_ = field;
        label_ = label;
        multiclassifier_ = c;
      } // Classifier

      public string Field { get { return field_; } }
      public string Label { get { return label_; } }

      public List<string> classify(Client c, List<NoteData> notes) {
        if (multiclassifier_ != null)
          return multiclassifier_(c, notes);
        List<string> l = new List<string>();
        l.Add(classifier_(c, notes));
        return l;
      } // classify
    }
    private static Classifier make(string f, string l, classifier c) { return new Classifier(f, l, c); }
    private static Classifier make(string f, string l, multiclassifier c) { return new Classifier(f, l, c); }

    private static Classifier[] allClassifiers = new Classifier[] {
      make("demographics.employmentstatus", "Employment Status", (c, n) => demographicData(c.Demographics.EmploymentStatus)),
      make("demographics.ethnicorigin", "Ethnic Origin", (c, n) => demographicData(c.Demographics.EthnicOrigin)),
      make("demographics.gender", "Gender", (c, n) => demographicData(c.Demographics.Gender)),
      make("demographics.Disability", "Disability", (c, n) => demographicData(c.Demographics.Disability)),
      make("demographics.DisabilityType", "Disability Type", (c, n) => demographicData(c.Demographics.DisabilityType)),
      make("demographics.MaritalStatus", "Marital  Status", (c, n) => demographicData(c.Demographics.MaritalStatus)),
      make("demographics.HouseholdType", "Household Type", (c, n) => demographicData(c.Demographics.HouseholdType)),
      make("demographics.HousingType", "Housing Type", (c, n) => demographicData(c.Demographics.HousingType)),
      make("demographics.HouseholdIncome", "Household Income", (c, n) => demographicData(c.Demographics.HouseholdIncome)),
      make("demographics.AgeBracket", "Age", (c, n) => c.Demographics.Dob != null ? ageBracket(c.Demographics.Dob.Value) : "Not stated"),
      make("referral.to", "Referral To", (c, n) => referralTo(n)),
      make("referral.from", "Referral From", (c, n) => referralFrom(c)),
      make("postcode", "Post Code", (c, n) => postCode(c)),
      make("deprivation", "In area of deprivation", (c, n) => deprivation(c))
    };

    private static string demographicData(string d) {
      if (d == null || d.Trim().Length == 0)
        return "Not stated";
      return d.Trim();
    } // demographicData

    private static string ageBracket(DateTime dob) {
      DateTime now = DateTime.Now;
      int age = now.Year - dob.Year;
      if (now.Month < dob.Month || (now.Month == dob.Month && now.Day < dob.Day)) --age;

      if (age < 16)
        return "Under 16";
      if (age <= 18)
        return "16 - 18";
      if (age <= 24)
        return "19 - 24";
      if (age <= 35)
        return "25 - 35";
      if (age <= 45)
        return "36 - 45";
      if (age <= 55)
        return "46 - 55";
      if (age <= 65)
        return "56 - 65";
      return "65+";
    } // ageBracket

    private static List<string> referralTo(List<NoteData> notes) {
      return notes.Where(n => n.Type == NoteType.Referral).Select(n => Reports.referralTo(n)).ToList();
    } 
    private static List<string> referralFrom(Client c) {
      var r = new List<string>();
      var referral = c.referredBy();
      if (referral != null)
        r.Add(referral.Text);
      return r;
    }

    private static string postCode(Client c) {
      return Reports.firstPartOfPostCode(c);
    }

    private static string deprivation(Client c) {
      return c.Address.IsInDeprivedArea ? "Yes" : "No";
    }
    private static Classifier howToClassify(IList<ProjectQuestionData> projectQs, string field) {
      return Classifiers(projectQs).Where(c => c.Field == field).First();
    } // howToClassify

    public static List<Classifier> Classifiers(Guid orgId, Guid projId) {
      var repo = new ReportsRepository();
      var projectQs = repo.projectQuestions(projId);

      return Classifiers(projectQs);
    } // Classifiers

    private static List<Classifier> Classifiers(IList<ProjectQuestionData> projectQs) {
      List<Classifier> classifiers = new List<Classifier>();
      classifiers.AddRange(allClassifiers.ToList());

      foreach (var q in projectQs) {
        var classifier = make("question." + q.Id.ToString(),
                              q.Question,
                              (c, n) => projectQuestionClassifier(c, q.Id));
        classifiers.Add(classifier);
      } // foreach

      return classifiers;
    } // Classifiers

    private static string projectQuestionClassifier(Client c, Guid questionId) {
      if (c.Questions == null)
        return "Not specified";

      var qa = c.Questions.Where(a => a.Id == questionId).SingleOrDefault();
      if (qa == null)
        return "Not specified";

      var answer = qa.Answer;
      if (answer == null || answer == "")
        return "Not specified";

      return answer;
    } // projectQuestionClassifier

    public static string buildCSV(Guid orgId, Guid projId, Guid? locId, string field, DateTime? startDate, DateTime? endDate) {
      ReportData<Row> reportData = build(orgId, projId, locId, field, startDate, endDate);
      var csv = new StringWriter();
      csv.WriteLine("Project," + reportData.Additional["project"]);
      csv.WriteLine("Location," + reportData.Additional["location"]);
      csv.WriteLine("Clients," + reportData.Additional["clientCount"] + " clients");

      var firstRow = reportData.Data.First();
      csv.Write(",");
      foreach(var theme in firstRow.Themes) {
         csv.Write(",");
         csv.Write(theme.Label);
         csv.Write(",");
      } // foreach
      csv.WriteLine();      

      csv.Write(reportData.Additional["field"]);
      csv.Write(",Total number of clients");
      if (reportData.Data.Count > 0) { 
        foreach(var theme in reportData.Data.First().Themes) {
          csv.Write(",Number of clients where score has improved,Percentage of clients where score has improved");
          if (theme.Reduced != null)
            csv.Write(",Number of clients with reduced number of issues");
          csv.WriteLine();
        }
      }

      foreach(var row in reportData.Data) {
        csv.Write(row.Label);
        csv.Write(",");
        csv.Write(row.ClientCount);
        foreach(var theme in row.Themes) {
          csv.Write(",");
          csv.Write(theme.Score);
          csv.Write(",");
          csv.Write(theme.Percentage);
          csv.Write("%");
        }
        csv.WriteLine();
      }
      return csv.ToString();
    } // buildCSV

    public class Row {
      private string label_;
      private int count_;
      private List<ThemeScores> cats_ = new List<ThemeScores>();

      public Row(string label) { label_ = label; }
      public void bump() { ++count_; }
      public void bump(string themeName, bool improved, bool reducedRisk) {
        var cat = cats_.SingleOrDefault(c => c.Label == themeName);
        if (cat == null) {
          cat = new ThemeScores(themeName);
          cats_.Add(cat);
          cats_.Sort((l, r) => l.Label.CompareTo(r.Label));
        } // if ...
        cat.bump(improved, reducedRisk);
      } // bump

      public string Label { get { return label_; } }
      public int ClientCount { get { return count_; } }
      public List<ThemeScores> Themes { get { return cats_; } }
    } // class Row

    public class ThemeScores {
      private string label_;
      private int count_ = 0;
      private int improved_ = 0;
      private int reduced_ = 0;

      public ThemeScores(string label) { label_ = label; }
      public void bump(bool improved, bool reduced) {
        ++count_;
        if (improved)
          ++improved_;
        if (reduced)
          ++reduced_;
      } // bump
      public string Label { get { return label_; } }
      public int Score { get { return improved_; } }
      public int Percentage { get { return (int)(improved_ * 100.0 / count_); } }
      public int? Reduced { get {
        if ("personal circumstances".Equals(label_.ToLower()))
          return reduced_;
        return null;
      } }
    } // CategoryScores
  } // AdhocReport
} // ...