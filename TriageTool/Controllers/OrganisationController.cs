using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Models;
using RiskTracker.Entities;
using System.Linq;

namespace RiskTracker.Controllers {
  [Authorize]
  public class OrganisationController : ApiController {
    private ProjectOrganisationRepository repo_ = new ProjectOrganisationRepository();

    [Route("api/Userpwd")]
    [HttpPut]
    public IHttpActionResult UpdateUserPassword(PasswordUpdate pwd) {
      var userName = User.Identity.Name;

      repo_.UpdatePassword(userName, pwd.Password);

      return Ok();
    } // UserDetails

    [Route("api/User")]
    [ResponseType(typeof(Staff))]
    [HttpGet]
    public IHttpActionResult UserDetails() {
      var userName = User.Identity.Name;

      var user = repo_.FindStaffMember(userName);

      return Ok(user);
    } // UserDetails

    [Route("api/User")]
    [ResponseType(typeof(Staff))]
    [HttpPut]
    public IHttpActionResult UpdateUserDetails(StaffMemberData update) {
      var userName = User.Identity.Name;

      var user = repo_.UpdateStaffMember(userName, update);
      
      return Ok(user);
    } // UpdateUserDetails

    [Route("api/Advisor")]
    [ResponseType(typeof(Advisor))]
    [HttpGet]
    public IHttpActionResult AdvisorDetails() {
      var userName = User.Identity.Name;

      var advisor = repo_.FindAdvisor(userName);

      return Ok(advisor);
    } // AdvisorDetails

    [Route("api/Coordinator")]
    [ResponseType(typeof(Coordinator))]
    [HttpGet]
    public IHttpActionResult CoordinatorDetails() {
      var userName = User.Identity.Name;

      var coordinator = repo_.FindCoordinator(userName);

      return Ok(coordinator);
    } // CoordinatorDetails

    [Route("api/Supervisor")]
    [ResponseType(typeof(Supervisor))]
    [HttpGet]
    public IHttpActionResult SupervisorDetails() {
      var userName = User.Identity.Name;

      var supervisor = repo_.FindSupervisor(userName);

      return Ok(supervisor);
    } // SupervisorDetails

    [Route("api/Manager")]
    [ResponseType(typeof(Manager))]
    [HttpGet]
    public IHttpActionResult ManagerDetails() {
      var userName = User.Identity.Name;

      var manager = repo_.FindManager(userName);

      return Ok(manager);
    } // ManagerDetails
    
    [Route("api/Organisations")]
    [HttpGet]
    public IEnumerable<ProjectOrganisation> GetOrganisations() {
      return repo_.AllOrganisations();
    } // GetOrganisations

    [Route("api/Organisation/{id:guid}")]
    [ResponseType(typeof(ProjectOrganisation))]
    [HttpGet]
    public IHttpActionResult GetOrganisation(Guid id) {
      ProjectOrganisation po = repo_.Get(id);

      if (po == null)
        return NotFound();

      return Ok(po);
    } // GetOrganisation

    [Route("api/Organisations")]
    [ResponseType(typeof(ProjectOrganisation))]
    [HttpPost]
    public IHttpActionResult CreateOrganisation(Organisation orgDetails) {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var newPo = repo_.Create(orgDetails);

      return Ok(newPo);
    } // CreateOrganisation

    [Route("api/Organisation/{id:guid}/Update")]
    [ResponseType(typeof(ProjectOrganisation))]
    [HttpPut]
    public IHttpActionResult UpdateOrganisation(Guid id, Organisation org) {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (id != org.Id)
        return BadRequest("Supplied Id and Organisation do not match.");

      var updatedPo = repo_.Update(org);

      return Ok(updatedPo);
    } // UpdateOrganisation

    [Route("api/Organisation/{id:guid}/Suspend")]
    [ResponseType(typeof(ProjectOrganisation))]
    [HttpPut]
    public IHttpActionResult SuspendOrganisation(Guid id) {
      var po = repo_.Get(id);
      po.IsSuspended = true;
      var updatedPo = repo_.Update(po);

      return Ok(updatedPo);
    } // SuspendOrganisation

    [Route("api/Organisation/{id:guid}/Activate")]
    [ResponseType(typeof(ProjectOrganisation))]
    [HttpPut]
    public IHttpActionResult ActivateOrganisation(Guid id) {
      var po = repo_.Get(id);
      po.IsSuspended = false;
      var updatedPo = repo_.Update(po);

      return Ok(updatedPo);
    } // ActivateOrganisation

    [Route("api/Organisation/{id:guid}/CanDelete")]
    [ResponseType(typeof(Boolean))]
    [HttpGet]
    public IHttpActionResult CanDeleteOrganisation(Guid id) {
      return Ok(repo_.CanDeleteOrganisation(id));
    } // CanDeleteOrganisation

    [Route("api/Organisation/{id:guid}")]
    [ResponseType(typeof(Boolean))]
    [HttpDelete]
    public IHttpActionResult DeleteOrganisation(Guid id) {
      return Ok(repo_.DeleteOrganisation(id));
    } // DeleteOrganisation

    /////////////////////////////////////////////////////
    [Route("api/Organisation/{id:guid}/Projects")]
    [HttpGet]
    public IEnumerable<Project> OrganisationProjects(Guid id) {
      return repo_.FetchProjects(id);
    } // OrganisationProjects

    [Route("api/Organisation/{id:guid}/AddProject")]
    [ResponseType(typeof(IList<Project>))]
    [HttpPost]
    public IHttpActionResult AddProjectToOrganisation(Guid id, Project newProject) {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var projects = repo_.AddProject(id, newProject);

      return Ok(projects);
    } // AddProjectToOrganisation

    [Route("api/Organisation/{id:guid}/Project/{projId:guid}/CanDelete")]
    [ResponseType(typeof(Boolean))]
    [HttpGet]
    public IHttpActionResult CanDeleteProject(Guid id, Guid projId) {
      var canDelete = repo_.CanDeleteProject(id, projId);
      return Ok(canDelete);
    } // CanDeleteProject

    [Route("api/Organisation/{id:guid}/Project/{projId:guid}")]
    [ResponseType(typeof(IList<Project>))]
    [HttpDelete]
    public IHttpActionResult DeleteProject(Guid id, Guid projId) {
      var projects = repo_.DeleteProject(id, projId);
      return Ok(projects);
    } // DeleteProject

    [Route("api/Organisation/{id:guid}/Project/{projId:guid}/Update")]
    [ResponseType(typeof(IList<Project>))]
    [HttpPut]
    public IHttpActionResult UpdateOrganisationProject(Guid id, Guid projId, Project proj) {
      if (!ModelState.IsValid)
        return badModelState();

      if (projId != proj.Id)
        return BadRequest("Bad Project Id");

      try {
        var allProjects = repo_.UpdateProject(id, proj);

        return Ok(allProjects);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // UpdateOrganisationProject

    /////////////////////////////////////////////////////
    [Route("api/Organisation/{id:guid}/ReferralAgencies")]
    [HttpGet]
    public IEnumerable<ReferralAgency> OrganisationReferralAgencies(Guid id) {
      return repo_.FetchReferralAgencies(id);
    } // OrganisationReferralAgencies

    [Route("api/Organisation/{id:guid}/AddReferralAgency")]
    [ResponseType(typeof(IList<ReferralAgency>))]
    [HttpPost]
    public IHttpActionResult AddReferralAgencyToOrganisation(Guid id, ReferralAgency agency) {
      if (!ModelState.IsValid)
        return badModelState();

      try {
        var allAgencies = repo_.AddReferralAgency(id, agency);

        return Ok(allAgencies);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // AddReferralAgencyToOrganisation

    [Route("api/Organisation/{id:guid}/ReferralAgency/{refId:guid}/Update")]
    [ResponseType(typeof(IList<Project>))]
    [HttpPut]
    public IHttpActionResult UpdateReferralAgency(Guid id, Guid refId, ReferralAgency ra) {
      if (!ModelState.IsValid)
        return badModelState();

      if (refId != ra.Id)
        return BadRequest("Bad Referral Agency Id");

      try {
        var allAgencies = repo_.UpdateReferralAgency(id, ra);

        return Ok(allAgencies);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // UpdateReferralAgency

    [Route("api/Organisation/{id:guid}/ReferralAgency/{refId:guid}")]
    [ResponseType(typeof(IList<Location>))]
    [HttpDelete]
    public IHttpActionResult DeleteReferralAgency(Guid id, Guid refId) {
      var allAgencies = repo_.DeleteReferralAgency(id, refId);
      return Ok(allAgencies);
    } // DeleteReferralAgency

    /////////////////////////////////////////////////////
    [Route("api/Organisation/{id:guid}/StaffMembers/{projId:guid}")]
    [HttpGet]
    public IEnumerable<StaffMember> OrganisationStaff(Guid id, Guid projId) {
      return repo_.FetchStaffMembers(id).Where(sm => sm.ProjectIds.Contains(projId));
    } // OrganisationStaff

    [Route("api/Organisation/{id:guid}/StaffMembers")]
    [HttpGet]
    public IEnumerable<StaffMember> OrganisationStaff(Guid id) {
      return repo_.FetchStaffMembers(id);
    } // OrganisationStaff

    [Route("api/Organisation/{id:guid}/AddStaffMember")]
    [ResponseType(typeof(IList<StaffMember>))]
    [HttpPost]
    public IHttpActionResult AddStaffMemberToOrganisation(Guid id, CreateStaffMember csm) {
      return AddStaffMemberToOrganisation(id, null, csm);
    } // AddStaffMemberToOrganisation

    [Route("api/Organisation/{id:guid}/AddStaffMember/{projId:guid}")]
    [ResponseType(typeof(IList<StaffMember>))]
    [HttpPost]
    public IHttpActionResult AddStaffMemberToOrganisation(Guid id, Guid? projId, CreateStaffMember csm) {
      if (!ModelState.IsValid)
        return badModelState();

      try {
        var staff = repo_.AddStaffMember(id, csm);
        if (projId.HasValue)
          staff = staff.Where(sm => sm.ProjectIds.Contains(projId.Value)).ToList();

        return Ok(staff);
      } catch (Exception e) {
        return BadRequest(e.Message);
      } 
    } // AddStaffMemberToOrganisation

    [Route("api/Organisation/{id:guid}/StaffMember/{staffId:guid}/Update")]
    [ResponseType(typeof(IList<StaffMember>))]
    [HttpPut]
    public IHttpActionResult UpdateOrganisationStaffMember(Guid id, Guid staffId, StaffMember staff) {
      return UpdateOrganisationStaffMember(id, null, staffId, staff);
    } // UpdateOrganisationStaffMember
      
    [Route("api/Organisation/{id:guid}/StaffMember/{projId:guid}/{staffId:guid}/Update")]
    [ResponseType(typeof(IList<StaffMember>))]
    [HttpPut]
    public IHttpActionResult UpdateOrganisationStaffMember(Guid id, Guid? projId, Guid staffId, StaffMember staffMember) {
      if (!ModelState.IsValid)
        return badModelState();

      if (staffId != staffMember.Id)
        return BadRequest("Bad Staff Member Id");

      try {
        var staff = repo_.UpdateStaffMember(id, staffMember);
        if (projId.HasValue)
          staff = staff.Where(sm => sm.ProjectIds.Contains(projId.Value)).ToList();

        return Ok(staff);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // UpdateOrganisationStaffMember

    [Route("api/Organisation/{id:guid}/StaffMember/{staffId:guid}/Password")]
    [HttpPut]
    public IHttpActionResult UpdateOrganisationStaffMember(Guid id, Guid staffId, PasswordUpdate update) {
      if (!ModelState.IsValid)
        return badModelState();

      if (staffId != update.Id)
        return BadRequest("Bad Staff Member Id");

      try {
        repo_.UpdatePassword(update.LoginId, update.Password);

        return Ok();
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // UpdateOrganisationStaffMember

    [Route("api/Organisation/{id:guid}/StaffMember/{staffId:guid}")]
    [HttpDelete]
    public IHttpActionResult DeleteOrganisationStaffMember(Guid id, Guid staffId) {
      return DeleteOrganisationStaffMember(id, null, staffId);
    } // DeleteOrganisationStaffMember
    
    [Route("api/Organisation/{id:guid}/StaffMember/{projId:guid}/{staffId:guid}")]
    [HttpDelete]
    public IHttpActionResult DeleteOrganisationStaffMember(Guid id, Guid? projId, Guid staffId) {
      try {
        var staff = repo_.DeleteStaffMember(id, staffId);
        if (projId.HasValue)
          staff = staff.Where(sm => sm.ProjectIds.Contains(projId.Value)).ToList();

        return Ok(staff);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // DeleteOrganisationStaffMember

    /////////////////////////////////////////////////////
    [Route("api/Organisation/{id:guid}/Locations")]
    [HttpGet]
    public IEnumerable<Location> OrganisationLocations(Guid id) {
      return repo_.FetchLocations(id);
    } // OrganisationLocations

    [Route("api/Organisation/{id:guid}/AddLocation")]
    [ResponseType(typeof(IList<Location>))]
    [HttpPost]
    public IHttpActionResult AddLocationToOrganisation(Guid id, Location loc) {
      if (!ModelState.IsValid)
        return badModelState();

      try {
        var allLocs = repo_.AddLocation(id, loc);

        return Ok(allLocs);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // AddLocationToOrganisation

    [Route("api/Organisation/{id:guid}/AddLocation/{projId:guid}")]
    [ResponseType(typeof(IList<Location>))]
    [HttpPost]
    public IHttpActionResult AddLocationToOrganisation(Guid id, Guid projId, Location loc) {
      if (!ModelState.IsValid)
        return badModelState();

      try {
        loc.ProjectIds.Add(projId);
        var projLocs = repo_.AddLocation(id, loc).Where(l => l.ProjectIds.Contains(projId));

        return Ok(projLocs);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // AddLocationToOrganisation


    [Route("api/Organisation/{id:guid}/Location/{locId:guid}/Update")]
    [ResponseType(typeof(IList<Location>))]
    [HttpPut]
    public IHttpActionResult UpdateOrganisationLocation(Guid id, Guid locId, Location loc) {
      if (!ModelState.IsValid)
        return badModelState();

      if (locId != loc.Id)
        return BadRequest("Bad Location Id");

      try {
        var allLocs = repo_.UpdateLocation(id, loc);

        return Ok(allLocs);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // UpdateOrganisationLocation

    [Route("api/Organisation/{id:guid}/Location/{projId:guid}/{locId:guid}/Update")]
    [ResponseType(typeof(IList<Location>))]
    [HttpPut]
    public IHttpActionResult UpdateOrganisationLocation(Guid id, Guid projId, Guid locId, Location loc) {
      if (!ModelState.IsValid)
        return badModelState();

      if (locId != loc.Id)
        return BadRequest("Bad Location Id");

      try {
        var projLocs = repo_.UpdateLocation(id, loc).Where(l => l.ProjectIds.Contains(projId));

        return Ok(projLocs);
      } catch (Exception e) {
        return BadRequest(e.Message);
      }
    } // UpdateOrganisationLocation

    [Route("api/Organisation/{id:guid}/Location/{locationId:guid}/CanDelete")]
    [ResponseType(typeof(Boolean))]
    [HttpGet]
    public IHttpActionResult CanDeleteLocation(Guid id, Guid locationId) {
      var canDelete = repo_.CanDeleteLocation(id, locationId);
      return Ok(canDelete);
    } // CanDeleteProject

    [Route("api/Organisation/{id:guid}/Location/{locationId:guid}")]
    [ResponseType(typeof(IList<Location>))]
    [HttpDelete]
    public IHttpActionResult DeleteLocation(Guid id, Guid locationId) {
      var locations = repo_.DeleteLocation(id, locationId);
      return Ok(locations);
    } // DeleteLocation

    [Route("api/Organisation/{id:guid}/Location/{projId:guid}/{locationId:guid}")]
    [ResponseType(typeof(IList<Location>))]
    [HttpDelete]
    public IHttpActionResult DeleteLocation(Guid id, Guid projId, Guid locationId) {
      var locations = repo_.DeleteLocation(id, locationId).Where(l => l.ProjectIds.Contains(projId));
      return Ok(locations);
    } // DeleteLocation

    /////////////////////////////////////////////////////
    [Route("api/Organisation/{id:guid}/RiskMaps")]
    [HttpGet]
    public IEnumerable<RiskMap> OrganisationRiskMaps(Guid id) {
      var rmRepo = new RiskMapRepository(id);
      return rmRepo.RiskMaps();
    } // OrganisationRiskMaps

    [Route("api/Organisation/{id:guid}/Risks")]
    [HttpGet]
    public IEnumerable<Risk> OrganisationRisks(Guid id) {
      var riskMaps = OrganisationRiskMaps(id);
      var risks = new List<Risk>();
      foreach (var rm in riskMaps)
        risks.AddRange(rm.Risks());

      return risks.OrderBy(r => r.Id).Distinct()
           .OrderBy(r => r.Grouping).OrderBy(r => r.Category).OrderBy(r => r.Theme).ToList();
    } // OrganisationRisks

    /////////////////////////////////////////////////////
    private IHttpActionResult badModelState() {
      var errorList = ModelState.Values.
                          SelectMany(v => v.Errors).
                          Select(e => e.ErrorMessage).
                          ToList();
      var message = String.Join("\n", errorList);
      return BadRequest(message);
    } // badModelState
  } // class OrganisationController
} // namespace ...