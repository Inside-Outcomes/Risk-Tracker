using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Models;
using RiskTracker.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.IO.Compression;
using RiskTracker.Reports;
using System.Net.Http;
using System.Net;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace RiskTracker.Controllers {
  public class Progress {
    public Progress(string m, string u, bool c) {
      msg = m;
      url = u;
      complete = c;
    }
    public string msg;
    public string url;
    public bool complete;
  }

  [Authorize]
  public class ExportEverythingController : ApiController {
    static int count = 3;
    static int total = 0;
    static int done = 0;
    static Task<String> job = null;

    [Route("api/Exportolump/{id:guid}")]
    [ResponseType(typeof(Progress))]
    [HttpGet]
    public IHttpActionResult Exportolump(Guid id) {
      var url = "";
      var msg = message();
      var complete = false;

      if (job == null) {
        msg = "Starting ";

        string folder = orgFolder(id);
        job = Task.Run(() => exportOrganisation(id, folder));
      } else if (job.IsCompleted) {
        if (job.IsFaulted) {
          msg = "Failed: " + job.Exception.Message;
        } else {
          msg = "Completed!";
          url = job.Result;
        }
        done = 0;
        complete = true;
        job = null;
      }

      var p = new Progress(msg, url, complete);
      return Ok(p);
    }

    [Route("api/exportofile/{fileId:guid}")]
    [HttpGet]
    [AllowAnonymous]
    public HttpResponseMessage FileDownload(Guid fileId) {
      var fileName = HttpContext.Current.Server.MapPath(String.Format("~/App_Data/Uploads/{0}.zip", fileId));

      HttpResponseMessage userFile = new HttpResponseMessage(HttpStatusCode.OK);
      userFile.Content = new StreamContent(new FileStream(fileName, FileMode.Open));
      userFile.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MIMEAssistant.GetMIMEType("full-export.zip"));
      userFile.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "full-export.zip" };
      return userFile;
    } // FileDownload


    private string message() {
      var msg = "In Progress ";

      if (total != 0)
        msg += done + " of " + total;

      for (var i = 0; i != count; ++i)
        msg += ".";
      ++count;
      if (count == 4)
        count = 1;
      return msg;      
    } // message

    private string exportOrganisation(Guid orgId, String folder) {
      var clientListFileName = filePath(folder, "clients.csv");

      var clientListFile = new StreamWriter(clientListFileName);
      clientListFile.Write("id,name,referenceId,dateOfBirth,registeredOn,lastUpdate,dischargedDate,");
      clientListFile.Write("address,postcode,phone,email,inAreaOfDeprivation,");
      clientListFile.Write("employmentStatus,ethnicOrigin,gender,disability,disabilityType,maritalStatus,householdType,housingType,householdIncome,");

      var clientIds = new ClientRepository().Clients(orgId).Select(c => c.Id).ToArray();
      total = clientIds.Count();
      done = 0;

      foreach (var id in clientIds) {
        var client = new ClientRepository().Client(id);
        writeClientInfo(client, clientListFile);

        var clientFolder = String.Format("{0}\\{1}-{2}", folder, client.Name.Replace(" ", "_"), client.Id);
        Directory.CreateDirectory(clientFolder);

        writeInfo(client, clientFolder);
        writeTimeLine(client, clientFolder);
        writeQuestions(client, clientFolder);
        writeDemographics(client, clientFolder);
        grabFiles(client, clientFolder);
        ++done;
      } // foreach

      clientListFile.Close();

      string zipFileName = String.Format("{0}.zip", folder);
      File.Delete(zipFileName);
      ZipFile.CreateFromDirectory(folder, zipFileName);
      Directory.Delete(folder, true);
      string path = String.Format("api/exportofile/{0}", orgId);
      return path;
    } // exportOrganisation

    private static void writeClientInfo(Client client, StreamWriter f) {
      write(f, client.Id.ToString());
      write(f, client.Name);
      write(f, client.ReferenceId);
      write(f, client.DateOfBirth); 
      write(f, client.registeredOn());
      write(f, client.LastUpdate); 
      write(f, client.Discharged); 

      var address = client.Address;
      write(f, address.Details);
      write(f, address.PostCode);
      write(f, address.PhoneNumber);
      write(f, address.Email);
      write(f, address.IsInDeprivedArea);

      var demos = client.Demographics;
      write(f, demos.EmploymentStatus);
      write(f, demos.EthnicOrigin);
      write(f, demos.Gender);
      write(f, demos.Disability);
      write(f, demos.DisabilityType);
      write(f, demos.MaritalStatus);
      write(f, demos.HouseholdType);
      write(f, demos.HousingType);
      write(f, demos.HouseholdIncome);

      f.WriteLine();
    } // writeClientInfo

    private static void writeTimeLine(Client client, String folder) {
      if (client.Timeline.Count == 0)
        return;

      var tlFile = new StreamWriter(String.Format("{0}\\timeline.txt", folder));

      foreach (var tle in client.Timeline) {
        if (tle.Notes.Count == 0 && tle.Actions.Count == 0)
          continue;

        var d = String.Format("{0:dd-MM-yyyy}", tle.Datestamp);

        foreach (var note in tle.Notes) 
          tlFile.WriteLine(String.Format("{0},{1}", d, note.Text.Replace("\n","\\n")));
        foreach (var note in tle.Actions)
          tlFile.WriteLine(String.Format("{0},{1}", d, note.Text.Replace("\n", "\\n")));

        bool doScores = false;
        foreach (var riskScore in tle.RiskScores)
          if (riskScore.Score != "0")
            doScores = true;

        if (doScores) {
          foreach (var riskScore in tle.RiskScores)
            tlFile.WriteLine(String.Format("{0},{1},Score {2}", d, riskScore.Title, riskScore.Score));
        }
      }
      tlFile.Close();
    } // writeTimeLine

    private static void writeQuestions(Client client, String folder) {
      if (client.Questions.Count == 0)
        return;

      var qFile = new StreamWriter(String.Format("{0}\\questions.txt", folder));

      foreach (var qa in client.Questions) {
        var formattedAnswer = qa.Answer != null ? qa.Answer.Replace("\n", "\\n") : "";
        qFile.WriteLine("{0}: {1}", qa.Question, formattedAnswer);
      }

      qFile.Close();
    }

    private static void writeDemographics(Client client, String folder) {
      var dFile = new StreamWriter(String.Format("{0}\\demographics.txt", folder));

      var d = client.Demographics;
      dFile.WriteLine(String.Format("Employment Status: {0}", d.EmploymentStatus));
      dFile.WriteLine(String.Format("Ethnic Origin: {0}", d.EthnicOrigin));
      dFile.WriteLine(String.Format("Gender: {0}", d.Gender));
      dFile.WriteLine(String.Format("Disability: {0}", d.Disability));
      dFile.WriteLine(String.Format("Disability Type: {0}", d.DisabilityType));
      dFile.WriteLine(String.Format("Marital Status: {0}", d.MaritalStatus));
      dFile.WriteLine(String.Format("Household Type: {0}", d.HouseholdType));
      dFile.WriteLine(String.Format("Housing Type: {0}", d.HousingType));
      dFile.WriteLine(String.Format("Household Income: {0}", d.HouseholdIncome));

      dFile.Close();
    }

    private static void grabFiles(Client client, String folder) {
      if (client.Files.Count == 0)
        return;

      var filesDir = String.Format("{0}\\files", folder);
      Directory.CreateDirectory(filesDir);

      foreach (var file in client.Files) {
        try {
          var uploadDir = openFileShare();
          var shareDir = uploadDir.GetDirectoryReference(client.Id.ToString());
          var shareFile = shareDir.GetFileReference(file.Id.ToString());

          var localFilename = String.Format("{0}\\{1:yyyy-MM-dd}-{2}", filesDir, file.Timestamp, file.Text);
          shareFile.DownloadToFile(localFilename, FileMode.Create);
        } catch (Exception e) {

        }
      }
    }

    private static CloudFileDirectory openFileShare() {
      var fileStorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
      var fileStorageAccount = CloudStorageAccount.Parse(fileStorageConnectionString);
      var fileClient = fileStorageAccount.CreateCloudFileClient();
      var shareName = ConfigurationManager.AppSettings["FileShare"];
      var share = fileClient.GetShareReference(shareName);

      var rootDir = share.GetRootDirectoryReference();
      var uploadDir = rootDir.GetDirectoryReference("Uploads");
      uploadDir.CreateIfNotExists();
      return uploadDir;
    }

    private static void writeInfo(Client client, String folder) {
      var pFile = new StreamWriter(String.Format("{0}\\personal-information.txt", folder));

      pFile.WriteLine(client.Name);
      pFile.WriteLine();

      if (client.ReferenceId != null && client.ReferenceId.Length != 0) 
        pFile.WriteLine(String.Format("Reference id {0}", client.ReferenceId));
      if (client.DateOfBirth.HasValue)
        pFile.WriteLine(String.Format("Date of birth {0:dd-MM-yyyy}", client.DateOfBirth.Value));
      if (client.registeredOn().HasValue)
        pFile.WriteLine(String.Format("Registered on {0:dd-MM-yyyy}", client.registeredOn().Value));
      if (client.Discharged.HasValue)
        pFile.WriteLine(String.Format("Discharged on {0:dd-MM-yyyy}", client.Discharged.Value));
      if (client.LastUpdate.HasValue)
        pFile.WriteLine(String.Format("Last update on {0:dd-MM-yyyy}", client.LastUpdate.Value));
      pFile.WriteLine();

      var a = client.Address;
      if (a.Details != null && a.Details.Length != 0) 
        pFile.WriteLine(a.Details);
      if (a.PostCode != null && a.PostCode.Length != 0) 
        pFile.WriteLine(a.PostCode);
      if (a.PhoneNumber != null && a.PhoneNumber.Length != 0) 
        pFile.WriteLine(String.Format("Phone {0}", a.PhoneNumber));
      if (a.Email != null && a.Email.Length != 0) 
        pFile.WriteLine(String.Format("Email {0}", a.Email));

      pFile.Close();
    }

    private static void write(StreamWriter f, DateTime? d) {
      if (d == null) {
        f.Write(",");
        return;
      }
      f.Write("{0:dd-MM-yyyy},", d.Value);
    }
    private static void write(StreamWriter f, string s) {
      if (s == null || s.Length == 0) {
        f.Write(",");
        return;
      }
      f.Write("\"{0}\",", s.Replace("\n","\\n"));
    }
    private static void write(StreamWriter f, bool b) {
      f.Write("{0},", b);
    }

    private string orgFolder(Guid orgId) {
      var orgFolder = String.Format("~/App_Data/Uploads/{0}", orgId);
      var root = HttpContext.Current.Server.MapPath(orgFolder);
      Directory.CreateDirectory(root);
      return root;
    } // orgFolder

    private static string filePath(String folder, string name) {
      return String.Format("{0}\\{1}", folder, name);
    }
  }
}