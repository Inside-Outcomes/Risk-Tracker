using System;
using RiskTracker.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiskTracker.Tests {
  [TestClass]
  public abstract class OutcomeFrameworkBaseTests {
    [ClassInitialize]
    public static void DataSetup(TestContext tc) {
      RiskTracker.DataConfig.InitialData();
    } // DataSetup

    [TestCleanup]
    public void CleanUp() {
      using (var db = new DatabaseContext()) {

        var toRemove = db.OutcomeFrameworks.Where(c => c.Title.StartsWith("TEST__"));
        db.OutcomeFrameworks.RemoveRange(toRemove);
        db.SaveChanges();
      } // using
    } // CleanUp

    [TestMethod]
    public void create_new_outcome_framework() {
      var initialCount = outcomeFrameworkCount();

      var outcomeFramework = createOutcomeFramework();

      Assert.AreNotEqual(outcomeFramework.Id, Guid.Empty);
      Assert.IsNotNull(outcomeFramework.Title);
      Assert.IsNotNull(outcomeFramework.Description);

      Assert.AreEqual(initialCount + 1, outcomeFrameworkCount());
    } // create_new_outcome_framework

    [TestMethod]
    public void update_outcome_framework() {
      var outcomeFramework = createOutcomeFramework();
      var count = outcomeFrameworkCount();
      
      outcomeFramework.Description = "Massive Trousers";
      var updated = updateOutcomeFramework(outcomeFramework);

      updated = getOutcomeFramework(outcomeFramework.Id);
      Assert.AreEqual(updated.Title, outcomeFramework.Title);
      Assert.AreEqual(updated.Description, "Massive Trousers");

      updated = getOutcomeFramework(outcomeFramework.Id);
      Assert.AreEqual(updated.Title, outcomeFramework.Title);
      Assert.AreEqual(updated.Description, "Massive Trousers");

      Assert.AreEqual(count, outcomeFrameworkCount());
    } // update_outcome_framework

    /////////////////////////////////////
    protected OutcomeFramework testOutcomeFramework() {
      var of = new OutcomeFramework();
      of.Title = "TEST__OUTCOME__FRAMEWORK";
      of.Description = "Test Description";
      return of;
    } // testOutcomeFramework

    /////////////////////////////////////
    protected abstract int outcomeFrameworkCount();
    protected abstract OutcomeFramework createOutcomeFramework();
    protected abstract OutcomeFramework updateOutcomeFramework(OutcomeFramework of);
    protected abstract OutcomeFramework getOutcomeFramework(Guid id);

    /////////////////////////////////////
    static public void clientCleanUp() {
      using (var db = new DatabaseContext()) {

        var toRemove = db.OutcomeFrameworks.
          Where(of => of.Title.StartsWith("TEST__"));

        db.OutcomeFrameworks.
          RemoveRange(toRemove);
        db.SaveChanges();
      } // using
    } // clientCleanUp

  }
}
