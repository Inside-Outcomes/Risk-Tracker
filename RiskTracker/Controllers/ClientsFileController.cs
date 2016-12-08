using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace RiskTracker.Controllers
{
    [Authorize]
    public class ClientsFileController : ApiController
    {
        private ClientRepository repo_ = new ClientRepository();

        private IHttpActionResult GetClient(Guid id) {
          Client client = repo_.Client(id);
          if (client == null)
            return NotFound();

          return Ok(client);
        } // GetClient

        [Route("api/Client/{id:guid}/FileUpload")]
        [ResponseType(typeof(Client))]
        [HttpPost]
        public async Task<IHttpActionResult> FileUpload(Guid id) {
          if (!Request.Content.IsMimeMultipartContent())
            return BadRequest();

          var provider = GetMultipartProvider();
          var result = await Request.Content.ReadAsMultipartAsync(provider);
          var fileData = result.FileData.First();
          var fileName = fileData.Headers.ContentDisposition.FileName;
          fileName = fileName.Replace("\"", "").Replace("'", "");

          var note = NewNote.FileUpload(fileName);
          await saveUpload(id, note.Id, fileData);
          repo_.AddClientNote(id, note);

          return GetClient(id);
        } // FileUpload

        [Route("api/Client/{id:guid}/file/{fileId:guid}")]
        [ResponseType(typeof(Client))]
        [HttpDelete]
        public async Task<IHttpActionResult> FileDelete(Guid id, Guid fileId) {
          var uploadDir = await openFileShare();
          var shareDir = uploadDir.GetDirectoryReference(id.ToString());
          var shareFile = shareDir.GetFileReference(fileId.ToString());
          await shareFile.DeleteIfExistsAsync();
          
          NoteData note;
          using (var clientRepo = new ClientRepository()) {
            note = clientRepo.GetClientNote(id, fileId);
          } // using 

          using (var notesRepo = new NoteRepository()) {
            notesRepo.RemoveNote(note);
          } // using

          return GetClient(id);
        } // FileDelete

        [Route("api/Client/{id:guid}/file/{fileId:guid}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> FileDownload(Guid id, Guid fileId) {
          var note = repo_.GetClientNote(id, fileId);
          var originalFileName = note.Text;

          var uploadDir = await openFileShare();
          var shareDir = uploadDir.GetDirectoryReference(id.ToString());
          var shareFile = shareDir.GetFileReference(fileId.ToString());
          
          var localFilename = DownloadFilename(fileId);
          await shareFile.DownloadToFileAsync(localFilename, FileMode.Create);

          HttpResponseMessage userFile = new HttpResponseMessage(HttpStatusCode.OK);
          userFile.Content = new StreamContent(new FileStream(localFilename, FileMode.Open));
          userFile.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MIMEAssistant.GetMIMEType(originalFileName));
          userFile.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = originalFileName };
          return userFile;
        } // FileDownload

        private MultipartFormDataStreamProvider GetMultipartProvider() {
          var uploadFolder = "~/App_Data/Tmp/FileUploads";
          var root = HttpContext.Current.Server.MapPath(uploadFolder);
          Directory.CreateDirectory(root);
          return new MultipartFormDataStreamProvider(root);
        } // GetMultipartProvider

        private string DownloadFilename(Guid noteId) {
          var download = "~/App_Data/Tmp/FileDownloads";
          var root = HttpContext.Current.Server.MapPath(download);
          Directory.CreateDirectory(root);
          return String.Format("{0}/{1}", root, noteId);
        } // DownloadDirs

        private async Task saveUpload(Guid clientId, Guid noteId, MultipartFileData fileData) {
          var uploadDir = await openFileShare();

          var shareDir = uploadDir.GetDirectoryReference(clientId.ToString());
          await shareDir.CreateIfNotExistsAsync();

          var shareFile = shareDir.GetFileReference(noteId.ToString());
          await shareFile.UploadFromFileAsync(fileData.LocalFileName, FileMode.Open);
        } // saveUpload

        private async Task<CloudFileDirectory> openFileShare() {
          var fileStorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
          var fileStorageAccount = CloudStorageAccount.Parse(fileStorageConnectionString);
          var fileClient = fileStorageAccount.CreateCloudFileClient();
          var shareName = ConfigurationManager.AppSettings["FileShare"];
          var share = fileClient.GetShareReference(shareName);

          var rootDir = share.GetRootDirectoryReference();
          var uploadDir = rootDir.GetDirectoryReference("Uploads");
          await uploadDir.CreateIfNotExistsAsync();
          return uploadDir;
        } // openFileShare

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                repo_.Dispose();
            base.Dispose(disposing);
        } // Dispose
    } // class ClientsController
}