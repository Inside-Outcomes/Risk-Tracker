using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Entities;
using RiskTracker.Models;

namespace RiskTracker.Controllers {
  [Authorize]
  public class RiskMapController : ApiController {
    private RiskMapRepository repo_ = new RiskMapRepository();

    [Route("api/RiskMaps")]
    [ResponseType(typeof(RiskMap))]
    [HttpPost]
    public IHttpActionResult RiskMap(RiskMap newRiskMap) {
      RiskMap riskMap = repo_.CreateRiskMap(newRiskMap);
      if (riskMap == null)
        return NotFound();

      return Ok(riskMap);
    } // RiskMap

    [Route("api/RiskMaps")]
    [HttpGet]
    public IEnumerable<RiskMap> ListRiskMaps() {
      return repo_.RiskMaps();
    } // ListRiskMaps

    [Route("api/RiskMap/{name}")]
    [ResponseType(typeof(RiskMap))]
    [HttpGet]
    public IHttpActionResult RiskMap(string name) {
      RiskMap riskMap = repo_.RiskMap(name);
      if (riskMap == null)
        return NotFound();

      return Ok(riskMap);
    } // RiskMap

    [Route("api/RiskMap/{name}")]
    [ResponseType(typeof(RiskMap))]
    [HttpPut]
    public IHttpActionResult RiskMap(string name, RiskMap update) {
      RiskMap riskMap = repo_.UpdateRiskMap(name, update);
      if (riskMap == null)
        return NotFound();

      return Ok(riskMap);
    } // RiskMap

    /// //////////////////////////
    /// //////////////////////////
    [Route("api/Risks")]
    [HttpGet]
    public IEnumerable<Risk> ListRisks() {
      return repo_.Risks();
    } // ListRisks

    [Route("api/Risks")]
    [ResponseType(typeof(Risk))]
    [HttpPost]
    public IHttpActionResult CreateRisk(Risk update) {
      Risk risk = repo_.CreateRisk(update);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    } // CreateRisk

    [Route("api/Risk/{id:guid}")]
    [ResponseType(typeof(Risk))]
    [HttpGet]
    public IHttpActionResult Risk(Guid id) {
      Risk risk = repo_.Risk(id);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    } // Risk

    [Route("api/Risk/{id:guid}")]
    [ResponseType(typeof(Risk))]
    [HttpPut]
    public IHttpActionResult Risk(Guid id, Risk update) {
      Risk risk = repo_.UpdateRisk(id, update);
      if (risk == null)
        return NotFound();

      return Ok(risk);
    } // Risk

  } // RiskMapController
} // namespace ...