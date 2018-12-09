using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RiskTracker.Reports {
  public class AuditReport {
    public static ReportData<Row> build(Guid orgId, Guid projId, Guid? locId, DateTime refDate) {
      var repo = new ReportsRepository();
      var riskMap = repo.projectRiskMap(orgId, projId);
      var clients = repo.findClients(projId, locId);

      var startDate = refDate.AddDays(-30);

      var newClients = new List<ClientData>();
      var currentClients = new List<ClientData>();
      var dischargedClients = new List<ClientData>();

      foreach(var client in clients) {
        if (isNew(client, startDate, refDate))
          newClients.Add(client);
        else if (isDischarged(client, startDate, refDate))
          dischargedClients.Add(client);
        else if (isCurrent(client, startDate, refDate))
          currentClients.Add(client);
      } // foreach

      reduceTo10(newClients);
      reduceTo10(currentClients);
      reduceTo10(dischargedClients);

      var rows = new List<Row>();
      rows.Add(new Row(newClients));
      rows.Add(new Row(currentClients));
      rows.Add(new Row(dischargedClients));

      var reportData = new ReportData<Row>(rows);
      reportData.Put("project", repo.projectName(projId));
      reportData.Put("location", repo.locationName(locId));
      reportData.Put("startDate", Reports.formatDate(startDate));
      reportData.Put("refDate", Reports.formatDate(refDate));
      reportData.Put("csvurl", Reports.csvUrl("audit", orgId, projId, locId, refDate, null));
      return reportData;
    } // build

    public static void buildCSV(StreamWriter csv, Guid orgId, Guid projId, Guid? locId, DateTime startDate) {
      ReportData<Row> reportData = build(orgId, projId, locId, startDate);

      csv.WriteLine("Project," + reportData.Additional["project"]);
      csv.WriteLine("Location," + reportData.Additional["location"]);
      csv.WriteLine("Start date," + reportData.Additional["startDate"]);
      csv.WriteLine("Reference date," + reportData.Additional["refDate"]);

      List<Row> rows = new List<Row>();
      rows.AddRange(reportData.Data);

      foreach (var c in rows[0].Clients)
        csv.WriteLine("New," + c.Name);
      foreach (var c in rows[1].Clients)
        csv.WriteLine("Current," + c.Name);
      foreach (var c in rows[2].Clients)
        csv.WriteLine("Discharged," + c.Name);

      csv.Close();
    } // buildCSV

    private static bool isNew(ClientData client, DateTime startDate, DateTime refDate) {
      var registered = client.registeredOn();
      if (registered == null)
        return false;
      return (startDate <= registered && refDate >= registered);
    } // isNew
    private static bool isDischarged(ClientData client, DateTime startDate, DateTime refDate) {
      var discharged = client.dischargedOn();
      if (discharged == null)
        return false;
      return (startDate <= discharged && refDate >= discharged);
    } // isDischarged
    private static bool isCurrent(ClientData client, DateTime startDate, DateTime refDate) {
      return Reports.activeBetweenDates(client, startDate, refDate);
    } // isCurrent

    private static void reduceTo10(List<ClientData> clients) {
      var random = new Random();
      while (clients.Count > 10) {
        int kill = random.Next(clients.Count);
        clients.RemoveAt(kill);
      } // while
    } // reduceTo10

    public class Row {
      private List<RowData> clients_;

      public Row(List<ClientData> clients) {
        clients_ = clients.Select(c => new RowData(c.Id, c.Name)).ToList();
      }

      public List<RowData> Clients { get { return clients_; } }
    } // Row

    public class RowData {
      Guid id_;
      string name_;

      public RowData(Guid id, string name) {
        id_ = id;
        name_ = name;
      } // RowData

      public Guid Id { get { return id_; } }
      public string Name { get { return name_; } }
    }
  } // AuditReport
}