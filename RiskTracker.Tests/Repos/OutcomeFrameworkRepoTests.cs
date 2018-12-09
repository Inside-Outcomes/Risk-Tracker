using System;
using RiskTracker.Models;
using RiskTracker.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiskTracker.Tests.Repos {
  [TestClass]
  public class OutcomeFrameworkRepoTests : OutcomeFrameworkBaseTests {
    private RiskMapRepository repo_ = new RiskMapRepository(null);

    protected override int outcomeFrameworkCount() {
      using (var db = new DatabaseContext()) {
        return db.OutcomeFrameworks.Count();
      } // using
    } // outcomeFrameworkCount

    protected override OutcomeFramework createOutcomeFramework() {
      return repo_.CreateOutcomeFramework(testOutcomeFramework());
    } // createOutcomeFramework

    protected override OutcomeFramework updateOutcomeFramework(OutcomeFramework of) {
      return repo_.UpdateOutcomeFramework(of.Id, of);
    } // updateOutcomeFramework

    protected override OutcomeFramework getOutcomeFramework(Guid id) {
      return repo_.OutcomeFramework(id);
    } // getOutcomeFramework
  } // OutcomeFrameworkTests
} 
