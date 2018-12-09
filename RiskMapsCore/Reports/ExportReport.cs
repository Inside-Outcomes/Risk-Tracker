using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class ExportReport {
    public static ReportData<Row> build(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      var repo = new ReportsRepository();

      var reportData = new ReportData<Row>(new List<Row>());
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("location", repo.locationName(locId));
      reportData.Put("startDate", Reports.formatDate(startDate));
      reportData.Put("endDate", Reports.formatDate(endDate));
      reportData.Put("csvurl", Reports.csvUrl("export", orgId, projId, locId, startDate, endDate));
      return reportData;
    }

    public static void buildCSV(StreamWriter csv, Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      var repo = new ReportsRepository();
      var riskMap = repo.projectRiskMap(orgId, projId);
      var projectQs = repo.projectQuestions(projId);

      csv.WriteLine("Project," + repo.projectName(projId));
      csv.WriteLine("Location," + repo.locationName(locId));
      csv.WriteLine("Start Date," + Reports.formatDate(startDate));
      csv.WriteLine("End Date," + Reports.formatDate(endDate));


      csv.Write("Name,");
      csv.Write("Ref Id,");
      csv.Write("Post Code,");
      csv.Write("In area of deprivation,");
      csv.Write("Referred by,");
      csv.Write("Project,");
      csv.Write("Location,");
      csv.Write("Date of birth,");
      csv.Write("Disability,");
      csv.Write("Disability Type,");
      csv.Write("Employment Status,");
      csv.Write("Ethnic Origin,");
      csv.Write("Gender,");
      csv.Write("Household Income,");
      csv.Write("Household Type,");
      csv.Write("Housing Type,");
      csv.Write("Marital Status,");
      csv.Write("Registration Date,");
      foreach (var q in projectQs)
        csv.Write(q.Question + ",");
      foreach (var t in riskMap.AllThemes())
        csv.Write("Initial " + t + " score,");
      foreach (var t in riskMap.AllThemes())
        csv.Write("Highest " + t + " score,");
      foreach (var t in riskMap.AllThemes())
        csv.Write("Current " + t + " score,");
      csv.Write("Discharge Date");
      csv.WriteLine();

      const int pageSize = 200;
      int page = 0;
      while (true) {
        var clientDatas = repo.findClients(projId, locId).
                                OrderBy(c => c.Id).
                                Skip(page * pageSize).
                                Take(pageSize).
                                Include(c => c.Address).
                                Include(c => c.Demographics).
                                Include(c => c.ProjectAnswers).ToList();
        if (clientDatas.Count == 0)
          break;

        var clients = new List<Client>();
        foreach (var cd in clientDatas) {
          if (!Reports.activeBetweenDates(cd, startDate, endDate))
            continue;
          clients.Add(new Client(cd, riskMap, projectQs, null));
        } // foreach

        foreach (var client in clients) {
          try {
            var riskAssessment = client.CurrentRiskAssessment;

            csv.Write(client.Name + ",");
            csv.Write(client.ReferenceId + ",");
            csv.Write(client.Address.PostCode + ",");
            csv.Write(client.Address.IsInDeprivedArea + ",");
            csv.Write(referredBy(client.referredBy()) + ",");
            csv.Write(repo.projectName(client.ProjectId) + ",");
            csv.Write(repo.locationName(client.LocationId) + ",");

            var demographics = client.Demographics;
            if (demographics == null)
              demographics = new DemographicData();

            csv.Write(Reports.formatDate(demographics.Dob) + ",");
            csv.Write(demographics.Disability + ",");
            csv.Write(demographics.DisabilityType + ",");
            csv.Write(demographics.EmploymentStatus + ",");
            csv.Write(demographics.EthnicOrigin + ",");
            csv.Write(demographics.Gender + ",");
            csv.Write(demographics.HouseholdIncome + ",");
            csv.Write(demographics.HouseholdType + ",");
            csv.Write(demographics.HousingType + ",");
            csv.Write(demographics.MaritalStatus + ",");
            csv.Write(Reports.formatDate(client.registeredOn()) + ",");
            foreach (var pq in client.Questions)
              csv.Write(pq.Answer + ",");

            var ra = client.CurrentRiskAssessment;
            // Initial scores
            foreach (var t in ra.ThemeAssessments)
              csv.Write(t.InitialScore + ",");
            // High scores
            foreach (var t in ra.ThemeAssessments)
              csv.Write(t.HighScore + ",");
            // Current scores
            foreach (var t in ra.ThemeAssessments)
              csv.Write(t.Score + ",");

            // Discharge Date
            csv.Write(Reports.formatDate(client.Discharged));
          } catch (Exception e) {
            csv.Write("Error exporting - " + e.Message);
          }
          csv.WriteLine();
          csv.Flush();
        } // foreach Client 
        ++page;
      } // while(true)

      csv.Close();
    }

    public class Row {  }

    private static string referredBy(NoteData noteData) {
      if (noteData == null)
        return "";
      return noteData.Text.Replace("Referred by ", "");
    }
  }
}