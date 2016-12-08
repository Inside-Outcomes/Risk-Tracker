using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace RiskTracker.Tests {
  public abstract class POBaseTests {
    [TestCleanup]
    public void cleanUp() {
      TestHelper.orgCleanUp();
    }

    [TestMethod]
    public void create_a_new_organisation() {
      int initialCount = allOrgs().Count();

      var org = createOrg();

      Assert.AreEqual(initialCount + 1, allOrgs().Count());
      Assert.AreNotEqual(Guid.Empty, org.Id);

      Assert.AreEqual(initialCount + 1, allOrgs().Count());
    } // createProjectOrganisation

    [TestMethod]
    public void create_new_projects_for_an_organisation() {
      var org = createOrg();

      Assert.AreEqual(0, listProjects(org).Count());

      var proj = createProject(org, "First Project");

      Assert.AreEqual("ORGTEST__First Project", proj.Name);

      createProject(org, "Second Project");
      createProject(org, "A Project");

      var projects = listProjects(org);

      Assert.AreEqual(3, projects.Count());
      // alphabetic order please
      Assert.AreEqual("ORGTEST__A Project", projects[0].Name);
      Assert.AreEqual("ORGTEST__First Project", projects[1].Name);
      Assert.AreEqual("ORGTEST__Second Project", projects[2].Name);

      org = getOrg(org.Id);
      Assert.AreEqual(3, org.ProjectCount);
      Assert.AreEqual("ORGTEST__A Project", org.ProjectNames[0]);
      Assert.AreEqual("ORGTEST__First Project", org.ProjectNames[1]);
      Assert.AreEqual("ORGTEST__Second Project", org.ProjectNames[2]);
    } // ...

    [TestMethod]
    public void update_a_project() {
      var org = createOrg();
      var project = createProject(org, "My Project");
      Assert.AreEqual("ORGTEST__My Project", project.Name);

      project.Name = project.Name.Replace("My", "Your");
      updateProject(org, project);

      var projects = listProjects(org);

      Assert.AreEqual(1, projects.Count());
      Assert.AreEqual("ORGTEST__Your Project", projects[0].Name);
    } // ...

    [TestMethod]
    public void add_a_commissioning_organisation_to_a_project() {
      var org = createOrg();

      var project = createProject(org, "Trousers");

      Assert.AreEqual(0, project.Commissioners.Count());

      var commissioningOrg = TestHelper.testOrg();
      project = addProjectCommissioner(project, commissioningOrg);

      Assert.AreEqual(1, project.Commissioners.Count());
      Assert.AreEqual(commissioningOrg.Name, project.Commissioners.Single().Name);
    } // add_a_commissioning_org...

    [TestMethod]
    public void update_an_organisation() {
      Organisation org = createOrg();

      org.Name += " UPDATED";
      org.Address.Email = "Sock puppet";

      updateOrg(org);

      var updatedOrg = getOrg(org.Id);
      Assert.AreEqual(org.Id, updatedOrg.Id);
      Assert.AreEqual(org.Name, updatedOrg.Name);
      Assert.AreEqual(org.Address.Email, updatedOrg.Address.Email);
    }

    [TestMethod]
    public async Task add_a_staff_member() {
      ProjectOrganisation org = createOrg();

      var staff = createStaff(org, "Charlie Smith");

      var updatedOrg = getOrg(org.Id);
      Assert.AreEqual(1, updatedOrg.StaffCount);

      using (var authRepo = new AuthRepository()) {
        IdentityUser user = await authRepo.FindUser(staff.LoginId, "chunky");
        Assert.IsNotNull(user);
      }
    } // add_a_staff_member

    [TestMethod]
    public void add_project_to_staff_member() {
      var org = createOrg();

      var staff = createStaff(org, "Chuckie T");

      Assert.AreEqual(0, staff.ProjectIds.Count());

      var proj1 = createProject(org, "Project 1");
      var proj2 = createProject(org, "Project 2");

      staff.ProjectIds.Add(proj1.Id);
      staff.ProjectIds.Add(proj2.Id);

      staff = updateStaff(org, staff);
      Assert.AreEqual(2, staff.ProjectIds.Count());
      Assert.IsTrue(staff.ProjectIds.Contains(proj1.Id));
      Assert.IsTrue(staff.ProjectIds.Contains(proj2.Id));
    } // add_project_to_staff_member

    [TestMethod]
    public void add_a_location() {
      ProjectOrganisation org = createOrg();

      var staff = createLocation(org, "Kitzinger Baby Shack");

      var updatedOrg = getOrg(org.Id);
      Assert.AreEqual(1, updatedOrg.LocationCount);
    } // add_a_location

    [TestMethod]
    public void add_project_to_location() {
      var org = createOrg();

      var location = createLocation(org, "Chuckie T");

      Assert.AreEqual(0, location.ProjectIds.Count());

      var proj1 = createProject(org, "Project 1");
      var proj2 = createProject(org, "Project 2");

      location.ProjectIds.Add(proj1.Id);
      location.ProjectIds.Add(proj2.Id);

      location = updateLocation(org, location);
      Assert.AreEqual(2, location.ProjectIds.Count());
      Assert.IsTrue(location.ProjectIds.Contains(proj1.Id));
      Assert.IsTrue(location.ProjectIds.Contains(proj2.Id));
    } // add_project_to_location


    protected abstract IList<ProjectOrganisation> allOrgs();
    protected abstract ProjectOrganisation getOrg(Guid id);
    protected abstract ProjectOrganisation createOrg();
    protected abstract ProjectOrganisation updateOrg(Organisation org);

    protected abstract StaffMember createStaff(ProjectOrganisation org, string name);
    protected abstract StaffMember updateStaff(ProjectOrganisation org, StaffMember staff);

    protected abstract Location createLocation(ProjectOrganisation org, string name);
    protected abstract Location updateLocation(ProjectOrganisation org, Location staff);

    protected abstract Project createProject(ProjectOrganisation org, string name);
    protected abstract Project updateProject(ProjectOrganisation org, Project proj);
    protected abstract Project addProjectCommissioner(Project proj, Organisation commissioner);
    protected abstract IList<Project> listProjects(ProjectOrganisation org);
  } // ProjectOrganisationTests
}
