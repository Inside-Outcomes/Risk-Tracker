using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class ActivityReport {
    public static ReportData<ICollection<Referral>> build(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      ReportsRepository repo = new ReportsRepository();

      var clients = repo.findClients(projId, locId);

      var active = 0;
      var assessments = 0;
      var sessions = 0;
      var dna = 0;
      var discharged = 0;
      var referralsFrom = new GatherReferrals();
      var referralsTo = new GatherReferrals();
      foreach (var client in clients) {
        if (!Reports.activeBetweenDates(client, startDate, endDate))
          continue;

        var registeredDate = client.registeredOn();
        if (!registeredDate.HasValue || Reports.inBounds(registeredDate.Value, startDate, endDate)) {
          ++assessments;
          var referredBy = client.referredBy();
          if (referredBy != null)
            referralsFrom.Add(referredBy.Text);
        }

        ISet<DateTime> sessionDates = new HashSet<DateTime>();
        foreach (var note in client.Notes) {
          var date = note.Timestamp.Date;
          if (Reports.outOfBounds(date, startDate, endDate))
            continue;
          sessionDates.Add(date);

          if (note.Type == NoteType.Referral) 
            referralsTo.Add(Reports.referralTo(note));
        }
        foreach (var riskAssessment in client.RiskAssessments) {
          var date = riskAssessment.Timestamp.Date;
          if (Reports.outOfBounds(date, startDate, endDate))
            continue;
          sessionDates.Add(date);
        }

        sessions += sessionDates.Count;
        dna += client.Notes.
                  Where(n => n.Type == NoteType.DidNotAttend).
                  Where(n => Reports.inBounds(n.Timestamp, startDate, endDate)).
                  Count();
        if (client.Discharged == true)
          ++discharged;
        else
          ++active;
      } // for ...

      if (endDate.HasValue)
        endDate = endDate.Value.AddDays(-1);
      var rows = new List<ICollection<Referral>>();
      rows.Add(referralsFrom.Referrals);
      rows.Add(referralsTo.Referrals);
      ReportData<ICollection<Referral>> reportData = new ReportData<ICollection<Referral>>(rows);
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("location", repo.locationName(locId));
      reportData.Put("startDate", Reports.formatDate(startDate));
      reportData.Put("endDate", Reports.formatDate(endDate));
      reportData.Put("assessments", assessments.ToString());
      reportData.Put("active", active.ToString());
      reportData.Put("discharged", discharged.ToString());
      reportData.Put("sessions", sessions.ToString());
      reportData.Put("dna", dna.ToString());
      reportData.Put("csvurl", Reports.csvUrl("activityreport", orgId, projId, locId, startDate, endDate));
      return reportData;
    } // build

    public class GatherReferrals {
      private IDictionary<string, Referral> referrals_ = new Dictionary<string, Referral>();

      public void Add(string where) {
        string k = where.ToLower().Trim();
        if (!referrals_.ContainsKey(k))
          referrals_.Add(k, new Referral(where));
        else
          referrals_[k].bump();
      }

      public ICollection<Referral> Referrals { get { return referrals_.Values; } }
    }

    public class Referral {
      private string where_;
      private int count_;

      public Referral(string w) { where_ = w; count_ = 1; }
      public void bump() { ++count_; }

      public string Where { get { return where_; } }
      public int Count { get { return count_; } }
    }

    public static string buildCSV(Guid orgId, Guid projId, Guid? locId, DateTime? startDate, DateTime? endDate) {
      var reportData = build(orgId, projId, locId, startDate, endDate);
      var csv = new StringWriter();

      csv.WriteLine("Project," + reportData.Additional["project"]);
      csv.WriteLine("Location," + reportData.Additional["location"]);
      csv.WriteLine("Start Date," + reportData.Additional["startDate"]);
      csv.WriteLine("End Date," + reportData.Additional["endDate"]);
      csv.WriteLine("Assessments," + reportData.Additional["assessments"]);
      csv.WriteLine("Active clients," + reportData.Additional["active"]);
      csv.WriteLine("Clients discharged," + reportData.Additional["discharged"]);
      csv.WriteLine("Sessions," + reportData.Additional["sessions"]);
      csv.WriteLine("Did Not Attends," + reportData.Additional["dna"]);

      var rows = reportData.Data.ToList();
      var referralsFrom = rows[0];
      foreach (var from in referralsFrom)
        csv.WriteLine(from.Where + "," + from.Count);
      var referralsTo = rows[1];
      foreach (var to in referralsTo)
        csv.WriteLine(to.Where + "," + to.Count);

      return csv.ToString();
    } // buildCSV
  }
}