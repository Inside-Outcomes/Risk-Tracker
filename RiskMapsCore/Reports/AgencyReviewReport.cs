using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace RiskTracker.Reports {
  public class AgencyReviewReport {
    public static ReportData<Row> build(Guid orgId) {
      var repo = new ProjectOrganisationRepository();
      var agencies = repo.FetchReferralAgencies(orgId);

      var rows = new List<Row>();
      foreach (var agency in agencies) {
        rows.Add(new Row(agency.Id, agency.Name, agency.ReviewDate));
      }

      rows.Sort(
        delegate(Row lhs, Row rhs) {
          if (!lhs.ReviewDate.HasValue)
            return 1;
          if (!rhs.ReviewDate.HasValue)
            return -1;
          return lhs.ReviewDate.Value.CompareTo(rhs.ReviewDate.Value);
        }
      );

      ReportData<Row> reportData = new ReportData<Row>(rows);
      reportData.Put("agencyCount", rows.Count.ToString());

      return reportData;
    } // build

    public class Row {
      private Guid id_;
      private string name_;
      private DateTime? reviewDate_;
      private bool overdue_ = false;

      public Row(Guid id, string name, DateTime? reviewDate) {
        id_ = id;
        name_ = name;
        reviewDate_ = reviewDate;

        if (reviewDate_.HasValue)
          overdue_ = (reviewDate_.Value.CompareTo(DateTime.Now.Date) <= 0);
      } // Row

      public Guid Id { get { return id_; } }
      public string Name { get { return name_; } }
      public DateTime? ReviewDate { get { return reviewDate_; } }
      public bool Overdue { get { return overdue_; } }
    }
  }
}
