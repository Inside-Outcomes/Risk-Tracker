﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Linq;

namespace RiskTracker.Tests {
  public abstract class ClientBaseTests {

    [TestCleanup]
    public void CleanUp() {
      TestHelper.clientCleanUp();
      TestHelper.orgCleanUp();
    } // CleanUp

    [TestMethod]
    public void create_new_client() {
      var initialCount = clientCount();

      var client = createClient();

      Assert.AreNotEqual(Guid.Empty, client.Id);
      Assert.AreEqual(initialCount + 1, clientCount());
    } // create_new_client

    [TestMethod]
    public void update_client() {
      Guid clientId = createClient().Id;

      var update = new ClientUpdate() {
        Id = clientId,
        Name = "TEST__Fred"
      };
      updateClient(update);

      var client = getClient(clientId);
      Assert.AreEqual("TEST__Fred", client.Name);
    } // FindAndUpdate

    [TestMethod]
    public void set_risk_map_and_add_risk() {
      var clientId = createClient().Id;
      var client = getClient(clientId);
      Assert.IsNull(client.CurrentRiskAssessment);
      
      var proj = project();
      var risks = riskMap(proj.RiskFramework);

      var update = new ClientUpdate() {
        Id = clientId,
        ProjectId = proj.Id
      };
      updateClient(update);

      client = getClient(clientId);
      Assert.IsNotNull(client.CurrentRiskAssessment);

      client = addAssessedRisk(client, risks.Risks().First());
      Assert.AreEqual(DateTime.Now.Date, client.CurrentRiskAssessment.Datestamp);

      var riskCount = risks.Risks().Count;
      Assert.AreEqual(1, client.CurrentRiskAssessment.AtRisk.Count);
      Assert.AreEqual(riskCount - 1, client.CurrentRiskAssessment.NotAtRisk.Count);
    } 

    [TestMethod]
    public void remove_risk() {       
      var clientAndRisks = createClientWithRiskMap();

      var client = clientAndRisks.Item1;
      var risks = clientAndRisks.Item2;

      addAssessedRisk(client, risks.Risks().First());
      client = removeAssessedRisk(client, risks.Risks().First());

      var riskCount = risks.Risks().Count;
      Assert.AreEqual(0, client.CurrentRiskAssessment.AtRisk.Count);
      Assert.AreEqual(0, client.CurrentRiskAssessment.ResolvedRisk.Count);
      Assert.AreEqual(riskCount, client.CurrentRiskAssessment.NotAtRisk.Count);
    }

    [TestMethod]
    public void adding_risk_in_a_group_unsets_others_in_group() {
      var clientAndRisks = createClientWithRiskMap();

      var client = clientAndRisks.Item1;
      var risks = clientAndRisks.Item2;

      var groupName = risks.Risks().Where(r => !String.IsNullOrEmpty(r.Grouping)).Select(r => r.Grouping).First();
      var groupRisks = risks.Risks().Where(r => r.Grouping == groupName).ToList();

      client = addAssessedRisk(client, groupRisks.First());

      var riskCount = risks.Risks().Count;
      Assert.AreEqual(1, client.CurrentRiskAssessment.AtRisk.Count);
      Assert.AreEqual(groupRisks.First().Id, client.CurrentRiskAssessment.AtRisk[0]);
      Assert.AreEqual(riskCount-1, client.CurrentRiskAssessment.NotAtRisk.Count);

      // setting a risk should unset risks in the same group
      client = addAssessedRisk(client, groupRisks.Last());
      Assert.AreEqual(1, client.CurrentRiskAssessment.AtRisk.Count);
      Assert.AreEqual(0, client.CurrentRiskAssessment.ResolvedRisk.Count);
      Assert.AreEqual(groupRisks.Last().Id, client.CurrentRiskAssessment.AtRisk[0]);
      Assert.AreEqual(riskCount - 1, client.CurrentRiskAssessment.NotAtRisk.Count);
    }

    [TestMethod]
    public void resolve_risk() {
      var clientAndRisks = createClientWithRiskMap();

      var client = clientAndRisks.Item1;
      var risks = clientAndRisks.Item2;

      addAssessedRisk(client, risks.Risks().First());
      client = resolveAssessedRisk(client, risks.Risks().First());

      var riskCount = risks.Risks().Count;
      Assert.AreEqual(0, client.CurrentRiskAssessment.AtRisk.Count);
      Assert.AreEqual(1, client.CurrentRiskAssessment.ResolvedRisk.Count);
      Assert.AreEqual(riskCount-1, client.CurrentRiskAssessment.NotAtRisk.Count);
    }

    [TestMethod]
    public void reopen_resolved_risk() {
      var clientAndRisks = createClientWithRiskMap();

      var client = clientAndRisks.Item1;
      var risks = clientAndRisks.Item2;

      addAssessedRisk(client, risks.Risks().First());
      resolveAssessedRisk(client, risks.Risks().First());
      client = reopenResolvedRisk(client, risks.Risks().First());

      var riskCount = risks.Risks().Count;
      Assert.AreEqual(1, client.CurrentRiskAssessment.AtRisk.Count);
      Assert.AreEqual(0, client.CurrentRiskAssessment.ResolvedRisk.Count);
      Assert.AreEqual(riskCount - 1, client.CurrentRiskAssessment.NotAtRisk.Count);
    }

    private Tuple<Client, RiskMap> createClientWithRiskMap() {
      var clientId = createClient().Id;
      var proj = project();
      var risks = riskMap(proj.RiskFramework);

      var update = new ClientUpdate() {
        Id = clientId,
        ProjectId = proj.Id
      };
      updateClient(update);

      return Tuple.Create(getClient(clientId), risks);
    } // createClientWithRiskMap

    /////////////////////////////
    protected abstract int clientCount();
    protected abstract Client createClient();
    protected abstract Client getClient(Guid id);
    protected abstract Client updateClient(ClientUpdate client);

    protected abstract Client addClientNote(Client client, string note);

    protected Project project() {
      using (var repo = new ProjectOrganisationRepository()) {
        var org = repo.Create(TestHelper.testOrg());
        var proj = TestHelper.newProject("CHUNKS");
        proj.RiskFramework = riskMapName();
        return repo.AddProject(org.Id, proj).Where(s => s.Name == proj.Name).Single();
      }
    } // project
    protected string riskMapName() {
      using (var riskMapRepo = new RiskMapRepository())
        return riskMapRepo.RiskMaps().First().Name;
    }
    protected RiskMap riskMap(string name) {
      using (var riskMapRepo = new RiskMapRepository())
        return riskMapRepo.RiskMap(name);
    } // RiskMap
    protected abstract Client addAssessedRisk(Client client, Risk risk);
    protected abstract Client removeAssessedRisk(Client client, Risk risk);
    protected abstract Client resolveAssessedRisk(Client client, Risk risk);
    protected abstract Client reopenResolvedRisk(Client client, Risk risk);
  } // class ClientBaseTests
} // namespace ...
