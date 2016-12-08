using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using RiskTracker.Models;

namespace RiskTracker.Entities {
  public class ClientRepository : BaseRepository {
    private DbSet<ClientData> clients { get { return context.Clients; } }

    public IEnumerable<ClientName> Clients() {
      return listClients(clients.Include(c => c.Notes));
    }

    public IEnumerable<ClientName> Clients(Guid orgId) {
      ProjectOrganisationData pod = ProjectOrganisationRepository.findOrg(orgId);
      List<Guid> locations = pod.Locations.Select(loc => loc.Id).ToList();
      return listClients(
        clients.Where(c => locations.Contains(c.LocationId)).
                Include(c => c.Notes).
                Include(c => c.Demographics));
    } // Clients

    public IEnumerable<ClientName> SearchByRefId(String refId) {
      return listClients(
        clients.Where(c => c.ReferenceId == refId).
                Include(c => c.Notes));
    }

    public IEnumerable<ClientName> UndischargedProjectClients(Guid projectId) {
      return listClients(
        clients.Where(c => c.Discharged != true).
                Where(c => c.ProjectId == projectId).
                Include(c => c.Demographics).
                Include(c => c.Notes));
    } // UndischargedProjectClients
    
    public IEnumerable<ClientName> ProjectClients(Guid projectId) {
      return listClients(
        clients.Where(c => c.ProjectId == projectId).
                Include(c => c.Demographics).
                Include(c => c.Notes));
    } // ProjectClients

    public IEnumerable<ClientName> LocationProjectClients(Guid locationId, Guid projectId) {
      return listClients(
        clients.Where(c => c.LocationId == locationId).
                Where(c => c.ProjectId == projectId).
                Include(c => c.Demographics).
                Include(c => c.Notes));
    } // Clients

    public IEnumerable<ClientName> UndischargedLocationClients(Guid locationId) {
      return listClients(
        clients.Where(c => c.Discharged != true).
                Where(c => c.LocationId == locationId).
                Include(c => c.Demographics).
                Include(c => c.Notes));
    } // Clients

    public IList<ClientData> ProjectClientData(Guid projectId, Guid? locationId) {
      var cl = clients.Where(c => c.ProjectId == projectId);

      if (locationId != null)
        cl = cl.Where(c => c.LocationId == locationId);

      return cl.Include(c => c.RiskAssessments).ToList();
    } // ProjectClients

    private IEnumerable<ClientName> listClients(IEnumerable<ClientData> clientData) {
      return clientData.Select(cd => new ClientName(cd));
    } // listClients

    private ClientData clientData(Guid clientId) {
      var clientData = clients.
        Where(c => c.Id == clientId).
        Include(c => c.Address).
        Include(c => c.Demographics).
        Include(c => c.Notes).
        Include(c => c.RiskAssessments).
        Include(c => c.ProjectAnswers).
        Single();
      return clientData;
    } // clientData

    public Client Client(Guid clientId) {
      var cd = clientData(clientId);
      return new Client(cd, riskMap(cd), projectQuestions(cd));
    } // Client

    private RiskMap riskMap(ClientData clientData) {
      var riskFramework = context.Projects.Where(p => p.Id == clientData.ProjectId).Select(p => p.RiskFramework).SingleOrDefault();
      var riskMapRepo = new RiskMapRepository();
      return riskMapRepo.RiskMap(riskFramework);
    } // riskMap

    private IList<ProjectQuestionData> projectQuestions(ClientData clientData) {
      var project = context.Projects.Where(p => p.Id == clientData.ProjectId).Include(p => p.Questions).Single();
      return project.Questions;
    } // projectQuestions

    public Client AddNewClient(ClientUpdate newClient) {
      if (!Guid.Empty.Equals(newClient.Id))
        throw new Exception("Client has an id - does it already exist?");

      newClient.Id = Guid.NewGuid();
      newClient.Address.Id = Guid.NewGuid();
      newClient.Demographics.Id = Guid.NewGuid();
      clients.Add(newClient.clientData());
      Commit();

      return Client(newClient.Id);      
    } // AddNewClient

    public Client Update(ClientUpdate clientUpdate) {
      var existingClient = clients.
        Where(c => c.Id == clientUpdate.Id).
        Include(c => c.Address).
        Include(c => c.Demographics).
        Single();

      if (existingClient == null)
        throw new Exception("Client not found - can't update");

      existingClient.CopyFrom(clientUpdate.clientData());

      Commit(existingClient);

      return Client(clientUpdate.Id);
    } // Update

    public Client ReopenClient(Guid id) {
      return dischargeState(id, false);
    } // dischargeState

    public Client DischargeClient(Guid id) {
      return dischargeState(id, true);
    } // DischargeClient

    private Client dischargeState(Guid id, bool discharged) {
      var client = clients.Where(c => c.Id == id).Single();

      if (client == null)
        throw new Exception("Client not found - can't update");

      client.Discharged = discharged;

      Commit(client);

      return Client(id);
    } // DischargeClient

    public Client UpdateQuestions(Guid id, IList<QuestionAnswerUpdate> qaus) {
      var client = clients.Where(c => c.Id == id).Include(c => c.ProjectAnswers).Single();

      if (client == null)
        throw new Exception("Client not found - can't update");

      if (client.ProjectAnswers == null)
        client.ProjectAnswers = new List<ProjectAnswerData>();

      foreach (var qu in qaus) {
        var answer = client.ProjectAnswers.Where(pa => pa.QuestionId == qu.Id).SingleOrDefault();
        if (answer == null) {
          answer = new ProjectAnswerData();
          answer.Id = Guid.NewGuid();
          answer.QuestionId = qu.Id;
          client.ProjectAnswers.Add(answer);
        }
        answer.Text = qu.Answer;
      } // foreach

      Commit(client);

      return Client(id);
    } // UpdateQuestions

    //////////////////////////////////////////////////////////////////
    public void DeleteClient(Guid clientId) {
      var client = clientData(clientId);

      if (client.Address != null)
        Delete(client.Address);
      if (client.Demographics != null)
        Delete(client.Demographics);
      if (client.Notes != null)
        foreach (var note in client.Notes.ToList())
          Delete(note);
      if (client.ProjectAnswers != null)
        foreach (var pa in client.ProjectAnswers.ToList())
          Delete(pa);
      if (client.RiskAssessments != null)
        foreach (var ra in client.RiskAssessments.ToList())
          Delete(ra);

      Delete(client);
    } // DeleteClient

    public Client AddClientNote(Guid clientid, NewNote note) {
      var client = clients.Find(clientid);

      if (client.Notes == null)
        client.Notes = new List<NoteData>();
      client.Notes.Add(note.noteData());
      Commit(client);

      return Client(client.Id);
    } // AddClientNote

    public Client UpdateClientNote(Guid clientId, Guid noteId, string noteText) {
      var noteData = GetClientNote(clientId, noteId);
      noteData.Text = noteText;
      Commit(noteData);

      return Client(clientId);
    } // UpdateClientNote

    public NoteData GetClientNote(Guid clientId, Guid noteId) {
      var clientData = clients.
        Where(c => c.Id == clientId).
        Include(c => c.Notes).
        Single();
      return clientData.Notes.Where(note => note.Id == noteId).Single();
    } // GetClientNote

    //////////////////////////////////////////////////////////////////
    private ClientData clientWithRiskAssessment(Guid clientId) {
      return clients.
        Where(c => c.Id == clientId).
        Include(c => c.RiskAssessments).
        Single();
    } // clientWithRiskAssessment

    public string RiskTitle(Guid riskId) {
      return context.Risks.Find(riskId).Title;
    } // RiskTitle

    public Client AddAssessedRisk(Guid clientId, UpdateRiskAssessment ura) {
      var client = clientWithRiskAssessment(clientId);

      var ra = riskAssessment(client, ura.Datestamp);

      var risksInGroup = groupRisks(client, ura.Id);
      foreach (var id in risksInGroup)
        ra.RemoveRisk(id);
      ra.AddRisk(ura.Id);
      
      Commit(client);

      return Client(client.Id);
    } // AddAssessedRisk

    private static readonly IList<Guid> NoGuids = new List<Guid>();
    private IEnumerable<Guid> groupRisks(ClientData client, Guid riskId) {
      var risk = context.Risks.Find(riskId);
      if (String.IsNullOrEmpty(risk.Grouping))
        return NoGuids;
      return context.Risks.Where(r => r.Grouping == risk.Grouping).Select(r => r.Id).ToList();
    } // groupRisks

    public Client RemoveAssessedRisk(Guid clientId, UpdateRiskAssessment ura) {
      var client = clientWithRiskAssessment(clientId);

      var ra = riskAssessment(client, ura.Datestamp);
      ra.RemoveRisk(ura.Id);

      Commit(client);

      return Client(client.Id);
    } // RemoveAssessedRisk

    public Client ResolveAssessedRisk(Guid clientId, UpdateRiskAssessment ura) {
      var client = clientWithRiskAssessment(clientId);

      var ra = riskAssessment(client, ura.Datestamp);
      ra.ResolveRisk(ura.Id);

      Commit(client);

      return Client(client.Id);
    } // ResolveAssessedRisk

    public Client ManageAssessedRisk(Guid clientId, UpdateRiskAssessment ura) {
      var client = clientWithRiskAssessment(clientId);

      var ra = riskAssessment(client, ura.Datestamp);
      ra.ManageRisk(ura.Id);

      Commit(client);

      return Client(client.Id);
    } // ManageAssessedRisk

    public Client ReopenResolvedRisk(Guid clientId, UpdateRiskAssessment ura) {
      var client = clientWithRiskAssessment(clientId);

      var ra = riskAssessment(client, ura.Datestamp);
      ra.ReopenRisk(ura.Id);

      Commit(client);

      return Client(client.Id); 
    } // ReopenAssessedRisk


    private RiskAssessmentData riskAssessment(ClientData client, DateTime date) {
      if (client.RiskAssessments == null)
        client.RiskAssessments = new List<RiskAssessmentData>();
      var ra = client.RiskAssessments.Where(r => r.Timestamp.Date == date.Date).SingleOrDefault();
      if (ra != null)
        return ra;

      var previousRa = client.RiskAssessments.OrderByDescending(r => r.Timestamp).FirstOrDefault();
      var todaysRa = new RiskAssessmentData(Guid.NewGuid());
      todaysRa.RiskIds = previousRa != null ? previousRa.RiskIds : "";
      todaysRa.ResolvedRiskIds = previousRa != null ? previousRa.ResolvedRiskIds : "";

      client.RiskAssessments.Add(todaysRa);
      return todaysRa;
    } // riskAssessment



    public int ClientCount {
      get { return clients.Count(); }
    } // ClientCount
  } // ClientRepository
} // namespace ...