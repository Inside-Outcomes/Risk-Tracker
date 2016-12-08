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
using TriageTool.Models;
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

            // just hardwire in a Project
            var por = new ProjectOrganisationRepository();
            var orgs = por.AllOrganisations().Where(p => p.Name == "Triage Demo").ToList();
            var proj = por.FetchProjects(orgs[0].Id)[0];

            newClient.ProjectId = proj.Id;

            Client client = repo_.AddNewClient(newClient);
            client = repo_.AddClientNote(client.Id, NewNote.ClientCreated());

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

        [Route("api/Client/{id:guid}/ManageRisk")]
        [ResponseType(typeof(Client))]
        [HttpPut]
        public IHttpActionResult ManageRisk(Guid id, UpdateRiskAssessment risk) {
          if (!ModelState.IsValid)
            return BadRequest(ModelState);

          repo_.ManageAssessedRisk(id, risk);
          riskNote(id, "Managed risk", risk);

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

        [Route("api/Client/{id:guid}/ReferralReport")]
        [ResponseType(typeof(ReferralReport))]
        [HttpPost]
        public IHttpActionResult ReferralReport(Guid id) {
          var report = new ReferralReport(staffName(User.Identity.Name));

          var ra = repo_.Client(id).CurrentRiskAssessment;

          var who = agencies();
          foreach (var theme in ra.ThemeAssessments)
            foreach (var category in theme.Categories)
              foreach (var risk in category.Risks) {
                if (risk.Status == "notAtRisk")
                  continue;

                foreach (var agency in who)
                  if (agency.Risk.ToLower() == risk.Title.ToLower())
                    report.add(risk.Id, agency);
              }

          return Ok(report);
        }

        private String staffName(String userName) {
          var porepo = new ProjectOrganisationRepository();
          return porepo.FindStaffMember(userName).Name;
        }
        
        private List<ReferralAgency> agencies() {
          var who = new List<ReferralAgency>();
          who.Add(new ReferralAgency("housing - homeless", "SHARP", "SHARP is a charity organization working with young people who are homeless and in need of accommodation. SHARP also have an outreach service supporting people with mental health problems and an emergency hostal which caters for all age groups", "mike@sharpuk.org", "0121 544 0542", ""));
          who.Add(new ReferralAgency("Housing - Temporary Accomodation", "Sandwell Citizens Advice", "We provide free, confidential and impartial advice.  People come to us with all sorts of issues. You may have money, benefit, housing or employment problems. You may be facing a crisis, or just considering your options.", "", "03444 111 444", "https://citizensadvice.citizensadvice.org.uk/sandwellcab.htm"));
          who.Add(new ReferralAgency("Housing - Unsuitable Housing", "Sandwell Citizens Advice", "We provide free, confidential and impartial advice.  People come to us with all sorts of issues. You may have money, benefit, housing or employment problems. You may be facing a crisis, or just considering your options.", "", "03444 111 444", "https://citizensadvice.citizensadvice.org.uk/sandwellcab.htm"));
          who.Add(new ReferralAgency("Housing - Unsuitable Housing", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));

          who.Add(new ReferralAgency("Disclosed domestic violence and abuse", "Sandwell Women's Aid", "Sandwell Women’s Aid have for the last 25 years offered refuge, support and counselling to victims of domestic abuse and sexual violence. We place victims’ voics at the heart of all that we do, and our specialist team works with drive and passion to change policy, practice and people’s lives. We take a whole-person approach to interpersonal violence, and over the years the organisation has grown from a single refuge to a large accommodation provider, community support agency and training and policy advisory.", "info@sandwellwomensaid.co.uk", "0121 553 0090", "http://www.sandwellwomensaid.co.uk/"));

          who.Add(new ReferralAgency("Caring responsibilities - lack of access to replacement or respite care", "Cares Sandwell", "CARES offers a free and confidential information, advice and support service for Carers and the people they care for within Sandwell.", "cares.sandwell@btinternet.com", "0121 558 7003", "http://cares-sandwell.org.uk/"));

          who.Add(new ReferralAgency("Financial hardship", "Sandwell Citizens Advice", "We provide free, confidential and impartial advice.  People come to us with all sorts of issues. You may have money, benefit, housing or employment problems. You may be facing a crisis, or just considering your options.", "", "03444 111 444", "https://citizensadvice.citizensadvice.org.uk/sandwellcab.htm"));
          who.Add(new ReferralAgency("Financial hardship", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));

          who.Add(new ReferralAgency("Lack of Access to Affordable Childcare","Sandwell Family Information Service","Get advice and information here on finding childcare, the different types of childcare available, how to choose quality childcare, how to make sure your child is safe, how to get extra help accessing childcare and information on fiancial support and help with childcare costs.", "family_information@sandwell.gov.uk", "0121 569 4914", "http://www.sandwell.gov.uk/familyinfo"));
 
          who.Add(new ReferralAgency("Vulnerable Adult","Vulnerable People Housing Service (VPHS)","SHIP (Supported Housing to Independence Pathway) and the SRP (Single Referral Pathway). The SRP (Single Referral Pathway) is a partnership between providers of supported housing in Sandwell and Sandwell Metropolitan Borough Council. The SRP has been working for the benefit of vulnerable people in housing need since 2006.", "", "0121 569 5238", "https://openaccess.sandwellhomes.org.uk"));
          
          who.Add(new ReferralAgency("Social Isolation","Age UK Sandwell","This service is provided for older people who are isolated and vulnerable, living in any of the six towns in Sandwell.", "info@ageuksandwell.org.uk", "0121 314 4526", "http://www.ageuk.org.uk/sandwell/"));
        
          who.Add(new ReferralAgency("Alcohol","Swanswell","We’re a national alcohol and drug charity that helps people change and be happy. We believe in a society free from problem alcohol and drug use.", "admin@swanswell.org", "0121 553 1333", "https://www.swanswell.org"));

          who.Add(new ReferralAgency("Demonstrating poor work ethic in past three months", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Low confidence and self esteem", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Not in Education, Employment or Training (NEET)", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Not Engaged in a Work Focussed Activity", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          
          who.Add(new ReferralAgency("Difficulty speaking English", "Sandwell College", "There are no formal entry requirements. You will take an English assessment to find out what your current level of English is in the four skill areas: Speaking, Listening, Reading and Writing, and to identify areas that you need to develop.", "enquiries@sandwell.ac.uk", "0800 622006", "http://www.sandwell.ac.uk/courses/english-for-beginners-esol/"));

          who.Add(new ReferralAgency("Substance misuse", "IRiS Sandwell","The IRiS Partnership brings together Cranstoun, Inclusion and a range of local partners with proven expertise, creative minds and a shared desire to re-shape drug and alcohol treatment and recovery services in the local communities we serve.", "sandwellreferrals@irispartnership.org.uk", "0121 553 1333", "http://www.irispartnership.org/services/sandwell/"));

          who.Add(new ReferralAgency("Long-term recipient of Employment Support Allowance", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Recent recipient of Employment Support Allowance", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));

          who.Add(new ReferralAgency("Limited IT skills", "Future Skills Sandwell", "Future Skills Sandwell, one of the Country’s most successful providers of Work Based Learning.", "enquiries@sandwell.ac.uk", "0121 667 5080", "http://www.futureskillssandwell.com/"));

          who.Add(new ReferralAgency("Long-Term Unemployed", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Recently Unemployed", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));

          who.Add(new ReferralAgency("Highest functional skills - Level Two", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("No qualifications", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Highest qualification - Entry Level", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));
          who.Add(new ReferralAgency("Highest qualification - Level One", "Sandwell Connexions", "Support to enable young people to participate in education, employment and training", "sandwell_connexions@sandwell.gov.uk", "0121 569 2955", "http://www.connexionssandwell.co.uk/"));

          return who;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                repo_.Dispose();
            base.Dispose(disposing);
        } // Dispose
    } // class ClientsController
}