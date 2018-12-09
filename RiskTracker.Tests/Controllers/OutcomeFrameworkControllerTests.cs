using System;
using RiskTracker.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Controllers;
using System.Web.Http;
using System.Web.Http.Results;
using System.Linq;

namespace RiskTracker.Tests.Controllers {
  [TestClass]
  public class OutcomeFrameworkControllerTests : OutcomeFrameworkBaseTests {
    private RiskMapController controller_ = new RiskMapController();

    protected override int outcomeFrameworkCount() {
      return controller_.ListOutcomeFrameworks().Count();
    } // outcomeFrameworkCount

    protected override OutcomeFramework createOutcomeFramework() {
      var result = controller_.CreateOutcomeFramework(testOutcomeFramework());
      return decodeActionResult(result);
    } // createOutcomeFramework

    protected override OutcomeFramework updateOutcomeFramework(OutcomeFramework of) {
      var result = controller_.OutcomeFramework(of.Id, of);
      return decodeActionResult(result);
    } // updateOutcomeFramework

    protected override OutcomeFramework getOutcomeFramework(Guid id) {
      var result = controller_.OutcomeFramework(id);
      return decodeActionResult(result);
    } // getOutcomeFramework

    /// ///////////////
    private OutcomeFramework decodeActionResult(IHttpActionResult actionResult) {
      var contentResult = actionResult as OkNegotiatedContentResult<OutcomeFramework>;
      return contentResult.Content;
    }
  } // OutcomeFrameworkControllerTests
} // RiskTracker.Tests.Controller
