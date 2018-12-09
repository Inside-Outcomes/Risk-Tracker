using RiskTracker.Entities;
using RiskTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RiskTracker.Reports {
  public class Reports {
    public static string ageBracket(DateTime? dobMaybe) {
      if (dobMaybe == null)
        return "Not stated";

      DateTime now = DateTime.Now;
      DateTime dob = dobMaybe.Value;
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

    public static bool activeBetweenDates(ClientData cd, 
                                          DateTime? startDate,
                                          DateTime? endDate) {
      DateTime? registeredOn = cd.registeredOn();
      DateTime? dischargedOn = cd.dischargedOn();

      if ((startDate != null) && (dischargedOn != null))
        if (dischargedOn.Value.CompareTo(startDate) < 0)
          return false;   // discharged before start

      if ((endDate != null) && (registeredOn != null))
        if (registeredOn.Value.CompareTo(endDate) > 0)
          return false;   // registered after end

      return true;
    } // activeBetweenDates

    public static bool outOfBounds(DateTime dt,
                                   DateTime? startDate,
                                   DateTime? endDate) {
      if (startDate != null)
        if (dt.CompareTo(startDate) < 0)
          return true;
      if (endDate != null)
        if (dt.CompareTo(endDate) > 0)
          return true;
      return false;
    } // outOfBounds

    public static bool inBounds(DateTime dt,
                                DateTime? startDate,
                                DateTime? endDate) {
      return !outOfBounds(dt, startDate, endDate);
    } // outOfBounds

    public static string formatDate(DateTime? date) {
      return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : "";
    }

    public static string csvUrl(string action, Guid orgId, Guid projId, Guid? locationId, DateTime? startDate, DateTime? endDate) {
      string urlTemplate = locationId.HasValue ? "api/Report/{1}/{2}/{3}/{0}csv?startDate={4}&endDate={5}" : "api/Report/{1}/{2}/{0}csv?startDate={4}&endDate={5}";
      return string.Format(urlTemplate, action, orgId, projId, locationId, formatDate(startDate), formatDate(endDate));
    }

    public static string firstPartOfPostCode(ClientData client) {
      return firstPartOfPostCode(client.Address);
    } // firstPartOfPostCode
    public static string firstPartOfPostCode(Client client) {
      return firstPartOfPostCode(client.Address);
    } // firstPartOfPostCode

    public static string firstPartOfPostCode(AddressData address) {
      var postCode = address.PostCode;
      if (postCode == null)
        return "Not given";
      postCode = postCode.Replace(" ", string.Empty);

      var regex = new Regex(@"^([A-Z]{1,2}[1-9][0-9]?)([0-9])([A-Z]{2})?$");
      var match = regex.Match(postCode);
      if (!match.Success) 
        return "Not given";
      var r = match.Groups[1].Value + " " + match.Groups[2].Value;
      return r;
    } // firstPartOfPostCode

    public static string referralTo(NoteData note) {
      var newline = note.Text.IndexOf('\n');
      var referral = newline != -1 ? note.Text.Substring(0, newline) : note.Text;
      return referral;
    } // referralTo
  }
}