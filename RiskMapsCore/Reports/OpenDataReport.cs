using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RiskTracker.Reports {
  public class OpenDataReport {
    public static ReportData<Row> build(Guid orgId, Guid projId, DateTime? startDate, DateTime? endDate) {
      var repo = new ReportsRepository();
      var allRisks = new RiskMapRepository(orgId).RisksCurrentAndDeleted();

      var clients = repo.findClients(projId, null).Include(c => c.Address);
      var rows = new List<Row>();
      foreach (var client in clients) {
        if (!Reports.activeBetweenDates(client, startDate, endDate))
          continue;
        if (client.RiskAssessments == null || client.RiskAssessments.Count == 0)
          continue;

        var postCode = Reports.firstPartOfPostCode(client);
        foreach (var rad in client.RiskAssessments) {
          var row = findRow(rows, postCode);
          row.bump();
          var assessed = rad.Risks().Select(riskId => allRisks.Where(r => r.Id == riskId).Single().Title).ToList();
          row.assessed(assessed);
          var resolved = rad.ResolvedRisks().Select(riskId => allRisks.Where(r => r.Id == riskId).Single().Title).ToList();
          row.resolved(resolved);
          var managed = rad.ManagedRisks().Select(riskId => allRisks.Where(r => r.Id == riskId).Single().Title).ToList();
          row.managed(managed);
        } // foreach RiskAssessment
      } // foreach Client
  
      rows.Sort((lhs, rhs) => postCodeOrder(lhs.PostCode, rhs.PostCode));

      var reportData = new ReportData<Row>(rows);
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("startDate", Reports.formatDate(startDate));
      reportData.Put("endDate", Reports.formatDate(endDate));
      reportData.Put("csvurl", Reports.csvUrl("opendata", orgId, projId, null, startDate, endDate));
      return reportData;
    } // build

    public static void buildCSV(StreamWriter csv, Guid orgId, Guid projId, DateTime? startDate, DateTime? endDate) {
      var reportData = build(orgId, projId, startDate, endDate);

      csv.WriteLine("Project," + reportData.Additional["project"]);
      csv.WriteLine("Start Date," + reportData.Additional["startDate"]);
      csv.WriteLine("End Date," + reportData.Additional["endDate"]);

      foreach (var row in reportData.Data) {
        foreach (var risk in row.Assessed)
          csv.WriteLine(String.Format("{0},{1},assessed,{2},{3}", row.PostCode, row.ClientCount, risk.Risk, risk.Count));
        foreach (var risk in row.Resolved)
          csv.WriteLine(String.Format("{0},{1},resolved,{2},{3}", row.PostCode, row.ClientCount, risk.Risk, risk.Count));
        foreach (var risk in row.Managed)
          csv.WriteLine(String.Format("{0},{1},managed,{2},{3}", row.PostCode, row.ClientCount, risk.Risk, risk.Count));
      } // foreach

      csv.Close();
    } // buildCSV

    private static Row findRow(List<Row> rows, string postcode) {
      foreach(Row row in rows) 
        if (row.PostCode == postcode)
          return row;
      Row r = new Row(postcode);
      rows.Add(r);
      return r;
    } // findRow

    public class Row {
      private string postcode_;
      private int clients_;
      private IDictionary<string, int> assessed_;
      private IDictionary<string, int> resolved_;
      private IDictionary<string, int> managed_;

      public class RiskCount {
        private string r_;
        private int c_;
        public RiskCount(string r, int c) { r_ = r; c_ = c; }
        public string Risk { get { return r_; } }
        public int Count { get { return c_; } }
      }

      public Row(string postcode) { 
        postcode_ = postcode;
        clients_ = 0;
        assessed_ = new Dictionary<string, int>();
        resolved_ = new Dictionary<string, int>();
        managed_ = new Dictionary<string, int>();
      }

      public void bump() { ++clients_; }
      public void assessed(List<string> risks) { addAll(assessed_, risks); }
      public void resolved(List<string> risks) { addAll(resolved_, risks); }
      public void managed(List<string> risks) { addAll(managed_, risks);  }

      public int ClientCount { get { return clients_; } }
      public string PostCode { get { return postcode_; } }
      public List<RiskCount> Assessed { get { return asList(assessed_); } }
      public List<RiskCount> Resolved { get { return asList(resolved_); } }
      public List<RiskCount> Managed { get { return asList(managed_);  } }

      private List<RiskCount> asList(IDictionary<string, int> risks) {
        List<string> l = risks.Keys.ToList();
        l.Sort();

        var results = new List<RiskCount>();
        foreach (var k in l) 
          results.Add(new RiskCount(k, risks[k]));
        return results;
      } // asList

      private void addAll(IDictionary<string, int> target, List<String> source) {
        foreach (string s in source) {
          if (target.ContainsKey(s))
            target[s] = target[s] + 1;
          else
            target[s] = 1;
        }
      } // addAll
    } // Row

    private static int postCodeOrder(string lhs, string rhs) {
      if (lhs == "Not given")
        return -1;
      if (rhs == "Not given")
        return 1;
      return lhs.CompareTo(rhs);
    } // postCodeOrder
  } // class AreaReport
} 