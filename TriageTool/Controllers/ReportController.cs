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
      [Route("api/Report/{orgId:guid}/{projId:guid}/opendata")]
      [ResponseType(typeof(ReportData<OpenDataReport.Row>))]
      [HttpGet]
      public IHttpActionResult openData(Guid orgId, Guid projId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        var report = OpenDataReport.build(orgId, projId, startDate, endDate);
        return Ok(report);
      } // openData

      [Route("api/Report/{orgId:guid}/{projId:guid}/opendatacsv")]
      [HttpGet]
      public HttpResponseMessage openDataCSV(Guid orgId, Guid projId) {
        DateTime? startDate = grabStartDate();
        DateTime? endDate = grabEndDate();

        string reportCSV = OpenDataReport.buildCSV(orgId, projId, startDate, endDate);
        return csvData(reportCSV, "areareport.csv");
      } // openDataCSV

      [Authorize]
      [Route("api/Report/{orgId:guid}/agencyreview")]
      [ResponseType(typeof(ReportData<AgencyReviewReport.Row>))]
      [HttpGet]
      public IHttpActionResult agencyReview(Guid orgId) {
        var report = AgencyReviewReport.build(orgId);
        return Ok(report);
      }
      /// ///////////////////////////
      private HttpResponseMessage csvData(string csv, string name) {
        HttpResponseMessage csvFile = new HttpResponseMessage(HttpStatusCode.OK);
        csvFile.Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv)));
        csvFile.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        csvFile.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = name };
        return csvFile;
      }

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