using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace RiskTracker.Tests.Repos {
  [TestClass]
  public class ProjectOrganisationTests : POBaseTests {
    private ProjectOrganisationRepository repo_ = new ProjectOrganisationRepository();

    protected override IList<ProjectOrganisation> allOrgs() {
      return repo_.AllOrganisations();
    }
    protected override ProjectOrganisation getOrg(Guid id) {
      return repo_.Get(id);
    }
    protected override ProjectOrganisation createOrg() {
      return repo_.Create(TestHelper.testOrg());
    }
    protected override ProjectOrganisation updateOrg(Organisation org) {
      return repo_.Update(org);
    }

    protected override StaffMember createStaff(ProjectOrganisation org, string name) {
      var csm = TestHelper.newStaff(name);
      return repo_.AddStaffMember(org.Id, csm).Where(s => s.Name == csm.Name).Single();
    }
    protected override StaffMember updateStaff(ProjectOrganisation org, StaffMember staff) {
      return repo_.UpdateStaffMember(org.Id, staff).Where(s => s.Name == staff.Name).Single();
    }

    protected override Project createProject(ProjectOrganisation org, string name) {
      var proj = TestHelper.newProject(name);
      return repo_.AddProject(org.Id, proj).Where(s => s.Name == proj.Name).Single();
    }
    protected override Project updateProject(ProjectOrganisation org, Project proj) {
      return repo_.UpdateProject(org.Id, proj).Where(p => p.Name == proj.Name).Single();
    }

    protected override Location createLocation(ProjectOrganisation org, string name) {
      var loc = TestHelper.newLocation(name);
      return repo_.AddLocation(org.Id, loc).Where(l => l.Name == loc.Name).Single();
    }
    protected override Location updateLocation(ProjectOrganisation org, Location loc) {
      return repo_.UpdateLocation(org.Id, loc).Where(l => l.Name == loc.Name).Single();
    }

    protected override Project addProjectCommissioner(Project proj, Organisation commissioner) {
      return repo_.AddProjectCommissioner(proj.Id, commissioner);
    }
    protected override IList<Project> listProjects(ProjectOrganisation org) {
      return repo_.FetchProjects(org.Id);
    }
  } // ProjectOrganisationTests
}
