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

namespace RiskTracker.Controllers
{
    [Authorize]
    public class ClientsController : ApiController
    {
        private ClientRepository repo_ = new ClientRepository();

        public IEnumerable<ClientName> GetClients() {
          return repo_.Clients();
        }

        [Route("api/Clients/Organisation/{orgId:guid}")]
        [HttpGet]
        public IEnumerable<ClientName> GetClients(Guid orgId)
        {
          var clients = repo_.Clients(orgId);
          clients = addDischargedFilter(clients);
          return clients;
        } // GetClients

        [Route("api/Clients/Location/{locId:guid}/Project/{projId:guid}")]
        [HttpGet]
        public IEnumerable<ClientName> LocationProjectClients(Guid locId, Guid projId) {
          var clients = repo_.LocationProjectClients(locId, projId);
          clients = addDischargedFilter(clients);
          return clients;
        } // LocationProjectClients

        [Route("api/Clients/Project/{projId:guid}")]
        [HttpGet]
        public IEnumerable<ClientName> ProjectClients(Guid projId) {
          var clients = repo_.ProjectClients(projId);
          clients = addDischargedFilter(clients);
          return clients;
        } // LocationProjectClients

        private IEnumerable<ClientName> addDischargedFilter(IEnumerable<ClientName> clients) {
          if (!includeDischarged())
            clients = clients.Where(cn => !cn.Discharged.HasValue);
          return clients;
        } // addDischargedFilter

        private bool includeDischarged() {
          var requestQuery = Request.GetQueryNameValuePairs().ToDictionary((key) => key.Key, (value) => value.Value);
          var includeDischarged = requestQuery["discharged"] == "true";
          return includeDischarged;
        } // includeDischarged
        
        [Route("api/Client/{id:guid}")]
        [ResponseType(typeof(Client))]
        [HttpGet]
        public IHttpActionResult GetClient(Guid id)
        {
            Client client = repo_.Client(id);
            if (client == null)
                return NotFound();

            return Ok(client);
        } // GetClient

        [Route("api/Client/{id:guid}/Update")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult PutClient(Guid id, ClientUpdate client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != client.Id)
                return BadRequest();

            repo_.Update(client);
            repo_.AddClientNote(id, NewNote.Log("Updated details"));

            return GetClient(id);
        } // PutClient

        [Route("api/Clients")]
        [ResponseType(typeof(Client))]
        [HttpPost]
        public IHttpActionResult PostClient(NewClient newClient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Client client = repo_.AddNewClient(newClient);
            client = repo_.AddClientNote(client.Id, NewNote.ClientCreated());
            if (!String.IsNullOrWhiteSpace(newClient.Referrer))
              client = repo_.AddClientNote(client.Id, new NewNote("Referred by " + newClient.Referrer));

            return Ok(client);
        } // PostClient

        //////////////////////////////////////////////////////////////
        [Route("api/Client/{id:guid}/AddNote")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult PutAddNote(Guid id, NewNote note) {
          return addNote(id, note, NoteType.Narrative);
        } // PutAddNote

        [Route("api/Client/{id:guid}/UpdateNote/{noteId:guid}")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult PutAddNote(Guid id, Guid noteId, NewNote note) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          repo_.UpdateClientNote(id, noteId, note.Text);

          return GetClient(id);
        } // PutAddNote

        [Route("api/Client/{id:guid}/AddReferral")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult PutAddReferral(Guid id, NewNote note) {
          return addNote(id, note, NoteType.Referral);
        } // PutAddReferral

        [Route("api/Client/{id:guid}/DidNotAttend")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult PutDidNotAttend(Guid id, NewNote note) {
          return addNote(id, note, NoteType.DidNotAttend);
        } // PutDidNotAttend

        [Route("api/Client/{id:guid}/AddGeneralNote")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult PutAddGeneralNote(Guid id, NewNote note) {
          return addNote(id, note, NoteType.General);
        } // PutAddGeneralNote

        [Route("api/Client/{id:guid}/Reopen")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult ReopenClient(Guid id, NewNote reopenNote) {
          addNote(id, reopenNote, NoteType.Reopen);
          return Ok(repo_.ReopenClient(id));
        } // DischargeClient

        [Route("api/Client/{id:guid}/Discharge")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult DischargeClient(Guid id, NewNote dischargeNote) {
          addNote(id, dischargeNote, NoteType.Discharged);
          return Ok(repo_.DischargeClient(id));
        } // DischargeClient

        [Route("api/Client/{id:guid}")]
        [HttpDelete]
        public IHttpActionResult DeleteClient(Guid id) {
          repo_.DeleteClient(id);
          return Ok();
        } // PutAddNote

        private IHttpActionResult addNote(Guid id, NewNote note, NoteType type) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          note.Type = type;
          repo_.AddClientNote(id, note);

          return GetClient(id);
        } // addNotes

        //////////////////////////////////////////////////////////////
        [Route("api/Client/{id:guid}/AddRisk")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult AddAssessedRisk(Guid id, UpdateRiskAssessment risk) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          repo_.AddAssessedRisk(id, risk);
          riskNote(id, "Assessed with risk", risk);

          return GetClient(id);
        } // AddAssessedRisk

        [Route("api/Client/{id:guid}/RemoveRisk")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult RemoveAssessedRisk(Guid id, UpdateRiskAssessment risk) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          repo_.RemoveAssessedRisk(id, risk);
          riskNote(id, "Removed risk", risk);

          return GetClient(id);
        } // RemoveAssessedRisk

        [Route("api/Client/{id:guid}/ResolveRisk")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult ResolveAssessedRisk(Guid id, UpdateRiskAssessment risk) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          repo_.ResolveAssessedRisk(id, risk);
          riskNote(id, "Resolved risk", risk);

          return GetClient(id);
        } // ResolveAssessedRisk

        [Route("api/Client/{id:guid}/ReopenRisk")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult ReopenResolvedRisk(Guid id, UpdateRiskAssessment risk) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          repo_.ReopenResolvedRisk(id, risk);
          riskNote(id, "Reopened risk", risk);

          return GetClient(id);
        } // ReopenResolvedRisk

        private void riskNote(Guid id, string description, UpdateRiskAssessment risk) {
          repo_.AddClientNote(id, NewNote.Event(
              String.Format("{0} : {1}", description, repo_.RiskTitle(risk.Id))
            ));
        } // riskNote

        [Route("api/Client/{id:guid}/Questions")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult UpdateQuestions(Guid id, IList<QuestionAnswerUpdate> qa) {
          return Ok(repo_.UpdateQuestions(id, qa));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                repo_.Dispose();
            base.Dispose(disposing);
        } // Dispose
    } // class ClientsController
}