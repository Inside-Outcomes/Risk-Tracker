using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Entities;
using RiskTracker.Models;

namespace RiskTracker.Controllers {
  [Authorize]
  public class RiskMapController : ApiController {
    [Route("api/RiskMaps")]
    [ResponseType(typeof(RiskMap))]
    [HttpPost]
    public IHttpActionResult RiskMap(RiskMapUpdate newRiskMap) {
      RiskMap riskMap = repo().CreateRiskMap(newRiskMap);
      if (riskMap == null)
        return NotFound();

      return Ok(riskMap);
    } // RiskMap

    [Route("api/RiskMaps")]
    [HttpGet]
    public IEnumerable<RiskMap> ListRiskMaps() {
      return repo().RiskMaps();
    } // ListRiskMaps

    [Route("api/RiskMap/{id}")]
    [ResponseType(typeof(RiskMap))]
    [HttpGet]
    public IHttpActionResult RiskMap(Guid id) {
      RiskMap riskMap = repo().RiskMap(id);
      if (riskMap == null)
        return NotFound();

      return Ok(riskMap);
    } // RiskMap

    [Route("api/RiskMap/{id}")]
    [ResponseType(typeof(RiskMap))]
    [HttpPut]
    public IHttpActionResult RiskMap(Guid id, RiskMapUpdate update) {
      RiskMap riskMap = repo().UpdateRiskMap(id, update);
      if (riskMap == null)
        return NotFound();

      return Ok(riskMap);
    } // RiskMap

    /// //////////////////////////
    /// //////////////////////////
    [Route("api/Risks")]
    [HttpGet]
    public IEnumerable<Risk> ListRisks() {
      return repo().Risks();
    } // ListRisks

    [Route("api/Risks")]
    [ResponseType(typeof(Risk))]
    [HttpPost]
    public IHttpActionResult CreateRisk(Risk update) {
      Risk risk = repo().CreateRisk(update);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    } // CreateRisk

    [Route("api/Risk/{id:guid}")]
    [ResponseType(typeof(Risk))]
    [HttpGet]
    public IHttpActionResult Risk(Guid id) {
      Risk risk = repo().Risk(id);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    } // Risk

    [Route("api/Risk/{id:guid}")]
    [ResponseType(typeof(Risk))]
    [HttpPut]
    public IHttpActionResult Risk(Guid id, Risk update) {
      Risk risk = repo().UpdateRisk(id, update);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    } // Risk

    [Route("api/Risk/{id:guid}")]
    [ResponseType(typeof(Risk))]
    [HttpDelete]
    public IHttpActionResult DeleteRisk(Guid id) {
      Risk risk = repo().DeleteRisk(id);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    }

    /// //////////////////////////
    /// //////////////////////////
    [Route("api/OutcomeFrameworks")]
    [HttpGet]
    public IEnumerable<OutcomeFramework> ListOutcomeFrameworks() {
      return repo().OutcomeFrameworks();
    } // ListOutcomeFrameworks

    [Route("api/OutcomeFramework/{id:guid}")]
    [ResponseType(typeof(OutcomeFramework))]
    [HttpGet]
    public IHttpActionResult OutcomeFramework(Guid id) {
      OutcomeFramework of = repo().OutcomeFramework(id);
      if (of == null)
        return NotFound();

      return Ok(of);
    } // OutcomeFramework

    [Route("api/OutcomeFrameworks")]
    [ResponseType(typeof(OutcomeFramework))]
    [HttpPost]
    public IHttpActionResult CreateOutcomeFramework(OutcomeFramework update) {
      OutcomeFramework of = repo().CreateOutcomeFramework(update);
      if (of == null)
        return NotFound();

      return Ok(of);
    } // CreateOutcomeFramework

    [Route("api/OutcomeFramework/{id:guid}")]
    [ResponseType(typeof(OutcomeFramework))]
    [HttpPut]
    public IHttpActionResult OutcomeFramework(Guid id, OutcomeFramework update) {
      OutcomeFramework of = repo().UpdateOutcomeFramework(id, update);
      if (of == null)
        return NotFound();

      return Ok(of);
    } // OutcomeFramework

    /// //////////////////////////
    /// //////////////////////////
    private RiskMapRepository repo() {
      return new RiskMapRepository(organisationId());
    } // repo()

    private Guid? organisationId() {
      var poRepo = new ProjectOrganisationRepository();
      var projectOrg = poRepo.GetByStaffMember(User.Identity.Name);

      if (projectOrg != null)
        return projectOrg.Id;
      return null; // Admin 
    } // organisationId
  } // RiskMapController
} // namespace ...