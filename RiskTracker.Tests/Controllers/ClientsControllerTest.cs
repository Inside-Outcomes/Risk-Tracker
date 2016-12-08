using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Controllers;
using System.Linq;
using System.Web.Http.Results;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Web.Http;
using System.Collections.Generic;
using System.Threading;

namespace RiskTracker.Tests.Controllers {
  [TestClass]
  public class ClientsControllerTest : ClientBaseTests {

    private ClientsController controller_ = new ClientsController();

    [ClassInitialize]
    public static void DataSetup(TestContext tc) {
      RiskTracker.DataConfig.InitialData();
    } // DataSetup

    [TestMethod]
    public void add_note_to_client() {
      var client = createClient();

      Assert.AreEqual(1, client.Timeline.Count);

      client = addClientNote(client, "Here's a note!");

      Assert.AreEqual(1, client.Timeline.Count);
      Assert.AreEqual("Here's a note!\nClient registered", client.Timeline[0].Notes);

      Thread.Sleep(100);

      client = addClientNote(client, "Second note");
      Assert.AreEqual(1, client.Timeline.Count);
      Assert.AreEqual("Second note\nHere's a note!\nClient registered", client.Timeline[0].Notes);
    } // AddNote

    /////////////////////////////
    protected override int clientCount() {
      return controller_.GetClients().Count();
    } // clientCount

    protected override Client createClient() {
      var actionResult = controller_.PostClient(TestHelper.testClient());
      return decodeActionResult<Client>(actionResult);
    } // createClient

    protected override Client getClient(Guid id) {
      var actionResult = controller_.GetClient(id);
      return decodeActionResult<Client>(actionResult);
    } // getClient

    protected override Client updateClient(ClientUpdate client) {
      var actionResult = controller_.PutClient(client.Id, client);
      return decodeActionResult<Client>(actionResult);
    } // updateClient

    
    protected override Client addClientNote(Client client, string note) {
      var actionResult = controller_.PutAddNote(client.Id, new NewNote() { Text = note });
      return decodeActionResult<Client>(actionResult);
    } // addClientNote

    protected override Client addAssessedRisk(Client client, Risk risk) {
      var actionResult = controller_.AddAssessedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
      return decodeActionResult<Client>(actionResult);
    } // addAssessedRisk
    protected override Client removeAssessedRisk(Client client, Risk risk) {
      var actionResult = controller_.RemoveAssessedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
      return decodeActionResult<Client>(actionResult);
    } // removeAssessedRisk
    protected override Client resolveAssessedRisk(Client client, Risk risk) {
      var actionResult = controller_.ResolveAssessedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
      return decodeActionResult<Client>(actionResult);
    } // resolveAssessedRisk
    protected override Client reopenResolvedRisk(Client client, Risk risk) {
      var actionResult = controller_.ReopenResolvedRisk(client.Id, new UpdateRiskAssessment() { Id = risk.Id });
      return decodeActionResult<Client>(actionResult);
    } // reopenResolvedRisk

    private T decodeActionResult<T>(IHttpActionResult actionResult) {
      var contentResult = actionResult as OkNegotiatedContentResult<T>;
      return contentResult.Content;
    }
  }
}
