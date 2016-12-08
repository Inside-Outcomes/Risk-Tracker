using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class ActionRequired {
    public static ReportData<Row> build(Guid orgId, Guid projId, Guid? locationId, DateTime cutoff) {
      ReportsRepository repo = new ReportsRepository();
      ClientRepository clientRepo = new ClientRepository();

      List<Row> rows = new List<Row>();
      var clients = repo.findClients(projId, locationId).Where(c => c.Discharged != true);

      foreach(var client in clients) {
        DateTime? lastNote = null;
        DateTime? lastAssessment = null;

        if (client.Notes != null && client.Notes.Count > 0)
          lastNote = client.Notes.
                            OrderByDescending(n => n.Timestamp).
                            Select(n => n.Timestamp).
                            First();
        if (client.RiskAssessments != null && client.RiskAssessments.Count > 0)
          lastAssessment = client.RiskAssessments.
                            OrderByDescending(n => n.Timestamp).
                            Select(n => n.Timestamp).
                            First();

        DateTime? lastUpdate = mostRecent(lastNote, lastAssessment);

        if (!lastUpdate.HasValue) {
          rows.Add(new Row(client.Id, client.Name, client.ReferenceId, null));
          continue;
        } 

        var lastUpdateDate = lastUpdate.Value.Date;
        if (lastUpdateDate.CompareTo(cutoff) < 0)
          rows.Add(new Row(client.Id, client.Name, client.ReferenceId, lastUpdate.Value));
      }

      rows.Sort(
        delegate(Row lhs, Row rhs) {
          if (!lhs.LastActivity.HasValue)
            return -1;
          if (!rhs.LastActivity.HasValue)
            return 1;
          return lhs.LastActivity.Value.CompareTo(rhs.LastActivity.Value);
        }
      );

      ReportData<Row> reportData = new ReportData<Row>(rows);
      reportData.Put("clientCount", rows.Count.ToString());
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("location", repo.locationName(locationId));
      reportData.Put("cutoff", cutoff.ToLongDateString());
      reportData.Put("csvurl", Reports.csvUrl("actionrequired", orgId, projId, locationId, cutoff, null));
 
      return reportData;
    } // build

    private static DateTime? mostRecent(DateTime? a, DateTime? b) {
      if (!a.HasValue)
        return b;
      if (!b.HasValue)
        return a;

      if (a.Value.CompareTo(b) > 0)
        return a;
      return b;
    } // mostRecent

    public static string buildCSV(Guid orgId, Guid projId, Guid? locationId, DateTime cutoff) {
      ReportData<Row> reportData = build(orgId, projId, locationId, cutoff);

      var csv = new StringWriter();
      csv.WriteLine(reportData.Additional["clientCount"] + " clients");
      csv.WriteLine("Name, Ref. Id, Last Activity");

      foreach (var row in reportData.Data) {
        csv.Write(row.Name);
        csv.Write(",");
        csv.Write(row.ReferenceId);
        csv.Write(",");
        csv.WriteLine(row.LastActivity);
      }

      return csv.ToString();
    }

      
    public class Row {
      private Guid id_;
      private string name_;
      private string refId_;
      private DateTime? lastActivity_;

      public Row(Guid id, string name, string refId, DateTime? lastActivity) {
        id_ = id;
        name_ = name;
        refId_ = refId;
        lastActivity_ = lastActivity;
      } // Row

      public Guid Id { get { return id_; } }
      public string Name { get { return name_; } }
      public string ReferenceId { get { return refId_ != null ? refId_ : ""; } }
      public DateTime? LastActivity { get { return lastActivity_; } }
    }
  }
}