using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using RiskTracker.Models;

namespace RiskTracker.Entities {
  public class ProjectOrganisationRepository : BaseRepository {
    public int Count {
      get { return context.ProjectOrganisations.Count(); }
    } // Count

    public static ProjectOrganisationData findOrg(Guid orgId) {
      ProjectOrganisationRepository repo = new ProjectOrganisationRepository();
      return repo.POD(orgId);
    } // findOrg

    private ProjectOrganisationData POD(Guid orgId) {
      return POD(p => p.Details.Id == orgId);
    } // POD

    private ProjectOrganisationData POD(Expression<Func<ProjectOrganisationData, bool>> where) {
      ProjectOrganisationData pod = 
        context.ProjectOrganisations.
                Where(where).
                Include(p => p.Details).
                Include(p => p.Details.Address).
                Include(p => p.Projects).
                Include(p => p.Projects.Select(pj => pj.Address)).
                Include(p => p.Projects.Select(pj => pj.Questions)).
                Include(p => p.Locations).
                Include(p => p.Locations.Select(loc => loc.Address)).
                Include(p => p.Staff).
                Single();
      if (pod.Projects != null) 
        pod.Projects.Sort(ProjectCompareByName);
      return pod;
    } // POD
    private static int ProjectCompareByName(ProjectData left, ProjectData right) {
      return left.Name.CompareTo(right.Name);
    } // ProjectCompareByName

    private ProjectData projectData(Guid projId) {
      ProjectData pad = context.Projects.Find(projId);
      return pad;
    } // projectData

    public ProjectOrganisation Get(Guid id) {
      var pod = POD(id);
      return pod != null ? new ProjectOrganisation(pod) : null;
    } // Get

    public IList<ProjectOrganisation> AllOrganisations() {
      var all = new List<ProjectOrganisation>();
      var pOrgs = context.ProjectOrganisations.Include(p => p.Details).
                Include(p => p.Details.Address).
                Include(p => p.Projects).
                Include(p => p.Projects.Select(pj => pj.Address)).
                Include(p => p.Locations).
                Include(p => p.Locations.Select(loc => loc.Address)).
                Include(p => p.Staff).ToList();
      foreach (var po in pOrgs) {
        if (po.Projects != null)
          po.Projects.Sort(ProjectCompareByName);
        all.Add(new ProjectOrganisation(po));
      } // foreach
      return all;
    } // AllOrganisations

    public ProjectOrganisation Create(Organisation org) {
      ProjectOrganisationData pod = new ProjectOrganisationData();
      pod.Details = org.organisation();
      pod.Id = Guid.NewGuid();
      pod.Details.Id = Guid.NewGuid();
      pod.Details.Address.Id = Guid.NewGuid();

      context.ProjectOrganisations.Add(pod);
      Commit();

      return Get(pod.Details.Id);
    } // Create

    public ProjectOrganisation Update(Organisation org) {
      ProjectOrganisationData pod = POD(org.Id);
      pod.Details.CopyFrom(org.organisation());
      Commit(pod);

      return Get(pod.Details.Id);
    } // Update

    public bool CanDeleteOrganisation(Guid orgId) {
      var pod = POD(orgId);

      return (pod.Projects.Count == 0) && (pod.Locations.Count == 0) && (pod.Staff.Count == 0);
    } // CanDeleteOrganisation

    public bool DeleteOrganisation(Guid orgId) {
      if (!CanDeleteOrganisation(orgId))
        return false;

      var pod = POD(orgId);
      Delete(pod);

      return true;
    }

    /// ////////////////////
    public IList<Project> AddProject(Guid orgId, Project project) {
      var projData = project.project();
      projData.Id = Guid.NewGuid();
      projData.Address.Id = Guid.NewGuid();

      var pod = POD(orgId);
      if (pod.Projects == null)
        pod.Projects = new List<ProjectData>();
      pod.Projects.Add(projData);

      Commit(pod);

      return FetchProjects(orgId);
    } // AddProject

    public IList<Project> UpdateProject(Guid orgId, Project project) {
      var pod = POD(orgId);
      if (pod.Projects == null)
        throw new Exception("Organisation has no projects");
      var currentProject = pod.Projects.Find(p => p.Id == project.Id);
      if (currentProject == null)
        throw new Exception("Project does not belong to the organisation");

      currentProject.CopyFrom(project.project());

      Commit(currentProject);

      return FetchProjects(orgId);
    } // UpdateProject

    public bool CanDeleteProject(Guid orgId, Guid projId) {
      var pod = POD(orgId);
      if (pod.Projects == null)
        throw new Exception("Organisation has no projects");

      var hasAttachedLocations = pod.Locations.Count(s => s.Projects().Contains(projId)) != 0;
      if (hasAttachedLocations)
        return false;

      using (var clientRepo = new ClientRepository()) {
        var sampleClient = clientRepo.UndischargedProjectClients(projId).FirstOrDefault();
        var hasClients = sampleClient != null;

        if (hasClients)
          return false;
      } // using ...

      return true;
    } // CanDeleteProject

    public IList<Project> DeleteProject(Guid orgId, Guid projId) {
      if (CanDeleteProject(orgId, projId)) {
        var pod = POD(orgId);
        var proj = pod.Projects.Find(p => p.Id == projId);
        pod.Projects.Remove(proj);
        Commit(pod);
      }
      return FetchProjects(orgId);
    } // DeleteProjects


    public IList<Project> AddProjectQuestion(Guid orgId, Guid projId, ProjectQuestionData pqd) {
      var pod = POD(orgId);
      if (pod.Projects == null)
        throw new Exception("Organisation has no projects");
      var currentProject = pod.Projects.Find(p => p.Id == projId);
      if (currentProject == null)
        throw new Exception("Project does not belong to the organisation");

      if (currentProject.Questions == null)
        currentProject.Questions = new List<ProjectQuestionData>();

      pqd.Id = Guid.NewGuid();
      currentProject.Questions.Add(pqd);

      Commit(currentProject);

      return FetchProjects(orgId);
    } // AddProjectQuestion

    public IList<Project> UpdateProjectQuestion(Guid orgId, Guid projId, ProjectQuestionData pqd) {
      var pod = POD(orgId);
      if (pod.Projects == null)
        throw new Exception("Organisation has no projects");
      var currentProject = pod.Projects.Find(p => p.Id == projId);
      if (currentProject == null)
        throw new Exception("Project does not belong to the organisation");

      var question = currentProject.Questions.Where(q => q.Id == pqd.Id).Single();
      question.Question = pqd.Question;
      question.Answers = pqd.Answers;

      Commit(currentProject);

      return FetchProjects(orgId);
    } // AddProjectQuestion

    public IList<Project> FetchProjects(Guid orgId) {
      ProjectOrganisationData pod = POD(orgId);

      var projects = new List<Project>();
      if (pod.Projects != null)
        foreach (var proj in pod.Projects)
          projects.Add(new Project(proj));

      return projects.OrderBy(p => p.Name).ToList();
    } // FetchProjects

    public Project FindProject(Guid orgId, Guid projId) {
      return FetchProjects(orgId).Where(p => p.Id == projId).First();
    } // FindProject

    public Project AddProjectCommissioner(Guid projId, Organisation commissioner) {
      ProjectData pad = projectData(projId);

      CommissioningOrganisation co = new CommissioningOrganisation();
      co.Organisation = commissioner.organisation();
      if (pad.CommissioningOrganisations == null)
        pad.CommissioningOrganisations = new List<CommissioningOrganisation>();

      co.Id = Guid.NewGuid();
      co.Organisation.Id = Guid.NewGuid();
      co.Organisation.Address.Id = Guid.NewGuid();
      pad.CommissioningOrganisations.Add(co);

      Commit(pad);

      return new Project(projectData(projId));
    } // AddProjectCommissioner

    //////////////////////
    public IList<StaffMember> AddStaffMember(Guid orgId, CreateStaffMember newStaff) {
      using (var authRepo = new AuthRepository()) {
        var result = authRepo.RegisterUser(newStaff.UserName, newStaff.Password, newStaff.Roles);
        if (!result.Succeeded) 
          throw new Exception(String.Join("\n", result.Errors));
      } // using 

      newStaff.Id = Guid.NewGuid();

      var pod = POD(orgId);
      if (pod.Staff == null)
        pod.Staff = new List<StaffMemberData>();
      pod.Staff.Add(newStaff.staffData());

      Commit(pod);

      return FetchStaffMembers(orgId);
    } // AddStaffMember

    public IList<StaffMember> UpdateStaffMember(Guid orgId, StaffMember staff) {
      var pod = POD(orgId);
      if (pod.Staff == null)
        throw new Exception("Organisation has no staff");
      var currentStaff = pod.Staff.Find(s => s.Id == staff.Id);
      if (currentStaff == null)
        throw new Exception("Staff member does not belong to the organisation");

      currentStaff.CopyFrom(staff.staffData());

      Commit(currentStaff);
      using (var authRepo = new AuthRepository()) {
        authRepo.SetUserRoles(currentStaff.UserName, staff.Roles);
      } // using ...

      return FetchStaffMembers(orgId);
    } // UpdateStaffMember

    public IList<StaffMember> DeleteStaffMember(Guid orgId, Guid staffId) {
      var pod = POD(orgId);
      if (pod.Staff == null)
        throw new Exception("Organisation has no staff");
      var currentStaff = pod.Staff.Find(s => s.Id == staffId);
      if (currentStaff == null)
        throw new Exception("Staff member does not belong to the organisation");

      pod.Staff.Remove(currentStaff);
      Commit(pod);

      using (var authRepo = new AuthRepository()) {
        authRepo.DeleteUser(currentStaff.UserName);
      }

      Delete(currentStaff);

      return FetchStaffMembers(orgId);
    } // DeleteStaffMember

    public IList<StaffMember> FetchStaffMembers(Guid orgId) {
      ProjectOrganisationData pod = POD(orgId);

      if (pod.Staff != null) {
        using (var authRepo = new AuthRepository()) {
          return pod.Staff.Select(sm => new StaffMember(sm, authRepo.UserRoles(sm.UserName))).OrderBy(sm => sm.Name).ToList();
        }
      } // if ...
      return new List<StaffMember>();
    } // FetchStaffMembers

    //////////////////////
    public IList<Location> AddLocation(Guid orgId, Location loc) {
      loc.Id = Guid.NewGuid();
      loc.Address.Id = Guid.NewGuid();

      var pod = POD(orgId);
      if (pod.Locations == null)
        pod.Locations = new List<LocationData>();
      pod.Locations.Add(loc.locationData());

      Commit(pod);

      return FetchLocations(orgId);
    } // AddLocation

    public IList<Location> UpdateLocation(Guid orgId, Location location) {
      var pod = POD(orgId);
      if (pod.Locations == null)
        throw new Exception("Organisation has no locations");
      var currentLocation = pod.Locations.Find(s => s.Id == location.Id);
      if (currentLocation == null)
        throw new Exception("Staff member does not belong to the organisation");

      currentLocation.CopyFrom(location.locationData());

      Commit(currentLocation);

      return FetchLocations(orgId);
    } // UpdateLocation

    public bool CanDeleteLocation(Guid orgId, Guid locId) {
      var pod = POD(orgId);
      if (pod.Locations == null)
        throw new Exception("Organisation has no locations");

      using (var clientRepo = new ClientRepository()) {
        var hasClients = clientRepo.UndischargedLocationClients(locId).FirstOrDefault() != null;

        return !hasClients;
      } // using ...
    } // CanDeleteProject

    public IList<Location> DeleteLocation(Guid orgId, Guid locId) {
      if (CanDeleteLocation(orgId, locId)) {
        var pod = POD(orgId);
        var loc = pod.Locations.Find(l => l.Id == locId);
        pod.Locations.Remove(loc);
        Commit(pod);
      }
      return FetchLocations(orgId);
    } // DeleteProjects

    public IList<Location> FetchLocations(Guid orgId) {
      ProjectOrganisationData pod = POD(orgId);

      if (pod.Locations != null)
        return pod.Locations.Select(l => new Location(l)).OrderBy(loc => loc.Name).ToList();

      return new List<Location>();
    } // FetchLocations

    //////////////////////
    public Staff UpdateStaffMember(string userName, StaffMemberData update) {
      var staffMember = context.StaffMembers.Where(sm => sm.UserName == userName).Single();
      staffMember.Name = update.Name;
      staffMember.PhoneNumber = update.PhoneNumber;
      staffMember.Email = update.Email;
      Commit(staffMember);

      return FindStaffMember(userName);
    } // UpdateStaffMember

    public void UpdatePassword(string userName, string newpwd) {
      using (var authRepo = new AuthRepository()) {
        authRepo.UpdateUserPassword(userName, newpwd);
      } // using 
    } // UpdateStaffMember

    //////////////////////
    public Staff FindStaffMember(string userName) {
      return findStaff(userName, (smd, pod) => { return new Staff(smd, pod); });
    } // FindStaffMember
    public Advisor FindAdvisor(string userName) {
      return findStaff(userName, (smd, pod) => { return new Advisor(smd, pod); });
    } // FindAdvisor
    public Coordinator FindCoordinator(string userName) {
      return findStaff(userName, (smd, pod) => { return new Coordinator(smd, pod); });
    } // FindCoordinatori
    public Supervisor FindSupervisor(string userName) {
      return findStaff(userName, (smd, pod) => { return new Supervisor(smd, pod); });
    } // FindSupervisor
    public Manager FindManager(string userName) {
      return findStaff(userName, (smd, pod) => { return new Manager(smd, pod); });
    } // FindManager

    private T findStaff<T>(string userName, Func<StaffMemberData, ProjectOrganisationData, T> builder) {
      var staffMemberData = context.StaffMembers.Where(sm => sm.UserName == userName).Single();
      var pod = POD(pm => pm.Staff.Any(s => s.Id == staffMemberData.Id));

      return builder(staffMemberData, pod);
    } // findStaff
  } // ProjectOrganisationRepository
} // namespace ...