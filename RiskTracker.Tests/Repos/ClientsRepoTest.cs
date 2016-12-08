using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Models;
using RiskTracker.Entities;
using System.Linq;

namespace RiskTracker.Tests.Controllers {
  [TestClass]
  public class ClientsRepoTest : ClientBaseTests {
    private ClientRepository repo_ = new ClientRepository();

    [ClassInitialize]
    public static void DataSetup(TestContext tc) {
      RiskTracker.DataConfig.InitialData();
    } // DataSetup

    /////////////////////////////
    protected override int clientCount() {
      return repo_.ClientCount; 
    } // clientCount

    protected override Client createClient() {
      return repo_.AddNewClient(TestHelper.testClient());
    } // createClient
    protected override Client getClient(Guid id) {
      return repo_.Client(id);
    } // getClient
    protected override Client updateClient(ClientUpdate client) {
      return repo_.Update(client);
    } // updateClient


    protected override Client addClientNote(Client client, string note) {
      return repo_.AddClientNote(client.Id, new NewNote() { Text = note });
    } // addClientNote

    protected override Client addAssessedRisk(Client client, Risk risk) {
      return repo_.AddAssessedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
    } // addAssessedRisk
    protected override Client removeAssessedRisk(Client client, Risk risk) {
      return repo_.RemoveAssessedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
    } // addAssessedRisk
    protected override Client resolveAssessedRisk(Client client, Risk risk) {
      return repo_.ResolveAssessedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
    }
    protected override Client reopenResolvedRisk(Client client, Risk risk) {
      return repo_.ReopenResolvedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
    }

  } // class ClientsRepoTest
} // namespace ...
