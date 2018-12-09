using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Linq;
using System.IO;
using RiskTracker.Reports;

namespace RiskTracker.Controllers {
    public class ReportController : ApiController {
      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/resolution")]
      [ResponseType(typeof(ReportData<RiskTracker.Reports.ResolutionReport.ResolutionRow>))]
      [HttpGet]
      public IHttpActionResult resolutionReport(Guid orgId, Guid projId) {
        return resolutionReport(orgId, projId, null);
      } // resolutionReport

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/resolution")]
      [ResponseType(typeof(ReportData<RiskTracker.Reports.ResolutionReport.ResolutionRow>))]
      [HttpGet]
      public IHttpActionResult resolutionReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        ReportData<ResolutionReport.ThemeRow> reportData = ResolutionReport.build(orgId, projId, locId, startDate, endDate);
        return Ok(reportData);
      } // resolutionReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/resolutioncsv")]
      [HttpGet]
      public HttpResponseMessage resolutionReportCSV(Guid orgId, Guid projId) {
        return resolutionReportCSV(orgId, projId, null);
      } // resolutionReportCSV

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/resolutioncsv")]
      [HttpGet]
      public HttpResponseMessage resolutionReportCSV(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var response = csvStream("resolution.csv", new PushStreamContent((stream, content, context) => {
          var writer = new StreamWriter(stream);
          ResolutionReport.buildCSV(writer, orgId, projId, locId, startDate, endDate);
        }));
        return response;
      } // resolutionReport

      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/pcprogress")]
      [ResponseType(typeof(ReportData<ResolutionReport.ResolutionRow>))]
      [HttpGet]
      public IHttpActionResult progressReport(Guid orgId, Guid projId) {
        return progressReport(orgId, projId, null);
      } // progressReport

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/pcprogress")]
      [ResponseType(typeof(ReportData<ResolutionReport.ResolutionRow>))]
      [HttpGet]
      public IHttpActionResult progressReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        ReportData<ProgressReport.ProgressRow> reportData = ProgressReport.build(orgId, projId, locId, startDate, endDate);
        return Ok(reportData);
      } // progressReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/pcprogresscsv")]
      [HttpGet]
      public HttpResponseMessage progressReportCSV(Guid orgId, Guid projId) {
        return progressReportCSV(orgId, projId, null);
      } // progressReportCSV

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/pcprogresscsv")]
      [HttpGet]
      public HttpResponseMessage progressReportCSV(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var response = csvStream("progress.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          ProgressReport.buildCSV(writer, orgId, projId, locId, startDate, endDate);
        }));

        return response;
      } // progressReportCSV

      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/actionrequired")]
      [ResponseType(typeof(ReportData<ActionRequired.Row>))]
      [HttpGet]
      public IHttpActionResult actionRequired(Guid orgId, Guid projId) {
        return actionRequired(orgId, projId, null);
      } // actionRequired

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/actionrequired")]
      [ResponseType(typeof(ReportData<ActionRequired.Row>))]
      [HttpGet]
      public IHttpActionResult actionRequired(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate(DateTime.Now.AddDays(-14));
        ReportData<ActionRequired.Row> reportData = ActionRequired.build(orgId, projId, locId, startDate.Value);
        return Ok(reportData);
      } // actionRequired

      [Route("api/Report/{orgId:guid}/{projId:guid}/actionrequiredcsv")]
      [HttpGet]
      public HttpResponseMessage actionRequiredCSV(Guid orgId, Guid projId) {
        return actionRequiredCSV(orgId, projId, null);
      } // actionRequiredcsv

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/actionrequiredcsv")]
      [HttpGet]
      public HttpResponseMessage actionRequiredCSV(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate(DateTime.Now.AddDays(-14));

        var response = csvStream("actionrequired.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          ActionRequired.buildCSV(writer, orgId, projId, locId, startDate.Value);
        }));

        return response;
      } // actionRequiredcsv

      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/activityreport")]
      [ResponseType(typeof(ReportData<ActionRequired.Row>))]
      [HttpGet]
      public IHttpActionResult activityReport(Guid orgId, Guid projId) {
        return activityReport(orgId, projId, null);
      } // actionRequired

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/activityreport")]
      [ResponseType(typeof(ReportData<Dictionary<string, int>>))]
      [HttpGet]
      public IHttpActionResult activityReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        ReportData<ICollection<ActivityReport.Referral>> reportData = ActivityReport.build(orgId, projId, locId, startDate, endDate);
        return Ok(reportData);
      } // actionRequired

      [Route("api/Report/{orgId:guid}/{projId:guid}/activityreportcsv")]
      [HttpGet]
      public HttpResponseMessage activityReportCSV(Guid orgId, Guid projId) {
        return activityReportCSV(orgId, projId, null);
      } // actionRequiredcsv

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/activityreportcsv")]
      [HttpGet]
      public HttpResponseMessage activityReportCSV(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var response = csvStream("activity.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          ActivityReport.buildCSV(writer, orgId, projId, locId, startDate, endDate);
        }));

        return response;
      } // actionRequiredcsv

      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/opendata")]
      [ResponseType(typeof(ReportData<OpenDataReport.Row>))]
      [HttpGet]
      public IHttpActionResult openData(Guid orgId, Guid projId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var report = OpenDataReport.build(orgId, projId, startDate, endDate);
        return Ok(report);
      } // areaReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/opendatacsv")]
      [HttpGet]
      public HttpResponseMessage openDataCSV(Guid orgId, Guid projId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var response = csvStream("opendata.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          OpenDataReport.buildCSV(writer, orgId, projId, startDate, endDate);
        }));

        return response;
      } // areaReportCSV

      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/fields")]
      [ResponseType(typeof(List<AdhocReport.Classifier>))]
      [HttpGet]
      public IHttpActionResult adhocReportField(Guid orgId, Guid projId) {
        var fields = AdhocReport.Classifiers(orgId, projId);

        return Ok(fields);
      }

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/adhoc")]
      [ResponseType(typeof(ReportData<AdhocReport.Row>))]
      [HttpGet]
      public IHttpActionResult adhocReport(Guid orgId, Guid projId) {
        return adhocReport(orgId, projId, null);
      } // adhocReport

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/adhoc")]
      [ResponseType(typeof(ReportData<AdhocReport.Row>))]
      [HttpGet]
      public IHttpActionResult adhocReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var requestQuery = Request.GetQueryNameValuePairs().ToDictionary((key) => key.Key, (value) => value.Value);
        var field = requestQuery["field"];

        var report = AdhocReport.build(orgId, projId, locId, field, startDate, endDate);
        return Ok(report);
      } // adhocReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/adhoccsv")]
      [HttpGet]
      public HttpResponseMessage adhocReportCSV(Guid orgId, Guid projId) {
        return adhocReportCSV(orgId, projId, null);
      } // adhocReportCSV

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/adhoccsv")]
      [HttpGet]
      public HttpResponseMessage adhocReportCSV(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();
        var requestQuery = Request.GetQueryNameValuePairs().ToDictionary((key) => key.Key, (value) => value.Value);
        var field = requestQuery["field"];

        var response = csvStream("adhocReport.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          AdhocReport.buildCSV(writer, orgId, projId, locId, field, startDate, endDate);
        }));

        return response;
      } // adhocReportCSV

      /// ///////////////////////////
      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/commissioners")]
      [ResponseType(typeof(ReportData<CommissionersReport.Row>))]
      [HttpGet]
      public IHttpActionResult commissionersReport(Guid orgId, Guid projId) {
        return commissionersReport(orgId, projId, null);
      } // commissionersReport

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/commissioners")]
      [ResponseType(typeof(ReportData<CommissionersReport.Row>))]
      [HttpGet]
      public IHttpActionResult commissionersReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var report = CommissionersReport.build(orgId, projId, locId, startDate, endDate);
        return Ok(report);
      } // commissionersReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/commissionerscsv")]
      [HttpGet]
      public HttpResponseMessage commissionersReportCsv(Guid orgId, Guid projId) {
        return commissionersReportCsv(orgId, projId, null);
      } // commissionersReportCsv

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/commissionerscsv")]
      [HttpGet]
      public HttpResponseMessage commissionersReportCsv(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var response = csvStream("commionersReport.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          CommissionersReport.buildCSV(writer, orgId, projId, locId, startDate, endDate);
        }));

        return response;
      } // commissionersReportCsv

      /// ///////////////////////////
      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/audit")]
      [ResponseType(typeof(ReportData<AuditReport.Row>))]
      [HttpGet]
      public IHttpActionResult auditReport(Guid orgId, Guid projId) {
        return auditReport(orgId, projId, null);
      } // auditReport

      [Authorize]
      [Route("avpi/Report/{orgId:guid}/{projId:guid}/{locId:guid}/audit")]
      [ResponseType(typeof(ReportData<AuditReport.Row>))]
      [HttpGet]
      public IHttpActionResult auditReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        if (!startDate.HasValue)
          return Ok();

        return Ok(AuditReport.build(orgId, projId, locId, startDate.Value));
      } // auditReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/auditcsv")]
      [HttpGet]
      public HttpResponseMessage auditReportCsv(Guid orgId, Guid projId) {
        return auditReportCsv(orgId, projId, null);
      } // auditReportCsv

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/auditcsv")]
      [HttpGet]
      public HttpResponseMessage auditReportCsv(Guid orgId, Guid projId, Guid? locId) {
        DateTime startDate = grabStartDate().Value;

        var response = csvStream("auditReport.csv", new PushStreamContent((stream, Content, context) => {
          var writer = new StreamWriter(stream);
          AuditReport.buildCSV(writer, orgId, projId, locId, startDate);
        }));

        return response;
      } // auditReportCsv

      /// ///////////////////////////
      /// ///////////////////////////
      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/export")]
      [ResponseType(typeof(ReportData<ExportReport.Row>))]
      [HttpGet]
      public IHttpActionResult exportReport(Guid orgId, Guid projId) {
        return exportReport(orgId, projId, null);
      } // exportReport

      [Authorize]
      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/export")]
      [ResponseType(typeof(ReportData<ExportReport.Row>))]
      [HttpGet]
      public IHttpActionResult exportReport(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        return Ok(ExportReport.build(orgId, projId, locId, startDate, endDate));
      } // exportReport

      [Route("api/Report/{orgId:guid}/{projId:guid}/exportcsv")]
      [HttpGet]
      public HttpResponseMessage exportReportCsv(Guid orgId, Guid projId) {
        return exportReportCsv(orgId, projId, null);
      } // exportReportCsv

      [Route("api/Report/{orgId:guid}/{projId:guid}/{locId:guid}/exportcsv")]
      [HttpGet]
      public HttpResponseMessage exportReportCsv(Guid orgId, Guid projId, Guid? locId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var response = csvStream("export.csv", new PushStreamContent((stream, content, context) => {
          var writer = new StreamWriter(stream);
          ExportReport.buildCSV(writer, orgId, projId, locId, startDate, endDate);
        }));
        return response; // csvData(ExportReport.buildCSV(orgId, projId, locId, startDate, endDate), "export.csv");
      } // exportReportCsv
      /// ///////////////////////////
      /// ///////////////////////////
      private HttpResponseMessage csvData(string csv, string name) {
        HttpResponseMessage csvFile = new HttpResponseMessage(HttpStatusCode.OK);
        csvFile.Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv)));
        csvFile.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        csvFile.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = name };
        return csvFile;
      }

      private HttpResponseMessage csvStream(string name, HttpContent content) {
        HttpResponseMessage csvFile = new HttpResponseMessage(HttpStatusCode.OK);
        csvFile.Content = content;
        csvFile.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        csvFile.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = name };
        return csvFile;
      } // csvStream

      private DateTime? grabStartDate() { return grabStartDate(null); }
      private DateTime? grabStartDate(DateTime? defaultDate) {
        var requestQuery = Request.GetQueryNameValuePairs().ToDictionary((key) => key.Key, (value) => value.Value);
        return requestQuery.ContainsKey("startDate") ? parseDate(requestQuery["startDate"]) : defaultDate;
      } // startDate
      private DateTime? grabEndDate() { return grabEndDate(null); }
      private DateTime? grabEndDate(DateTime? defaultDate) { 
        var requestQuery = Request.GetQueryNameValuePairs().ToDictionary((key) => key.Key, (value) => value.Value);
        return requestQuery.ContainsKey("endDate") ? parseEndDate(requestQuery["endDate"]) : defaultDate;
      } // endDate

      private DateTime? parseDate(string param) {
        if (String.IsNullOrWhiteSpace(param))
          return null;

        char[] delim = { '-' };
        string[] parts = param.Split(delim);
        return new DateTime(Int32.Parse(parts[0]), Int32.Parse(parts[1]), Int32.Parse(parts[2]));
      } // parseDate

      private DateTime? parseEndDate(string param) {
        DateTime? date = parseDate(param);
        if (date == null)
          return null;

        return date.Value.AddDays(1);
      } // parseEndDate

    } // class ReportController
} // RiskTracker.Controllers