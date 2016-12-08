using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Controllers;
using System.Linq;
using System.Web.Http.Results;
using RiskTracker.Models;
using System.Web.Http;
using System.Collections.Generic;

namespace RiskTracker.Tests.Controllers {
  [TestClass]
  public class OrganisationControllerTests : POBaseTests {
    private OrganisationController controller_ = new OrganisationController();

    /// /////////////////////////////////////////////
    protected override IList<ProjectOrganisation> allOrgs() {
      return controller_.GetOrganisations().ToList();
    }
    protected override ProjectOrganisation getOrg(Guid id) {
      var actionResult = controller_.GetOrganisation(id);
      return decodeActionResult<ProjectOrganisation>(actionResult);
    }
    protected override ProjectOrganisation createOrg() {
      var actionResult = controller_.CreateOrganisation(TestHelper.testOrg());
      return decodeActionResult<ProjectOrganisation>(actionResult);
    }
    protected override ProjectOrganisation updateOrg(Organisation org) {
      var actionResult = controller_.UpdateOrganisation(org.Id, org);
      return decodeActionResult<ProjectOrganisation>(actionResult);
    }

    protected override StaffMember createStaff(ProjectOrganisation org, string name) {
      var csm = TestHelper.newStaff(name);
      var actionResult = controller_.AddStaffMemberToOrganisation(org.Id, csm);
      return decodeActionResult<IList<StaffMember>>(actionResult).Where(s => s.Name == csm.Name).Single();
    }
    protected override StaffMember updateStaff(ProjectOrganisation org, StaffMember staff) {
      var actionResult = controller_.UpdateOrganisationStaffMember(org.Id, staff.Id, staff);
      return decodeActionResult<IList<StaffMember>>(actionResult).Where(s => s.Name == staff.Name).Single();
    }

    protected override Project createProject(ProjectOrganisation org, string projectName) {
      var proj = TestHelper.newProject(projectName);
      var actionResult = controller_.AddProjectToOrganisation(org.Id, proj);
      return decodeActionResult<IList<Project>>(actionResult).Where(p => p.Name == proj.Name).Single();
    }
    protected override Project updateProject(ProjectOrganisation org, Project proj) {
      var actionResult = controller_.UpdateOrganisationProject(org.Id, proj.Id, proj);
      return decodeActionResult<IList<Project>>(actionResult).Where(p => p.Name == proj.Name).Single();
    }

    protected override Location createLocation(ProjectOrganisation org, string name) {
      var loc = TestHelper.newLocation(name);
      var actionResult = controller_.AddLocationToOrganisation(org.Id, loc);
      return decodeActionResult<IList<Location>>(actionResult).Where(l => l.Name == loc.Name).Single();
    }
    protected override Location updateLocation(ProjectOrganisation org, Location loc) {
      var actionResult = controller_.UpdateOrganisationLocation(org.Id, loc.Id, loc);
      return decodeActionResult<IList<Location>>(actionResult).Where(l => l.Name == loc.Name).Single();
    }

    protected override Project addProjectCommissioner(Project proj, Organisation commissioner) {
      Assert.Inconclusive("OrganisationControllerTests.addProjectCommissioner not implemented");
      return null;
    }
    protected override IList<Project> listProjects(ProjectOrganisation org) {
      return controller_.OrganisationProjects(org.Id).ToList();
    }

    private T decodeActionResult<T>(IHttpActionResult actionResult) {
      var contentResult = actionResult as OkNegotiatedContentResult<T>;
      return contentResult.Content;
    }
  }
}
