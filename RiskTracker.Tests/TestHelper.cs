using System;
using RiskTracker.Entities;
using RiskTracker.Models;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace RiskTracker.Tests {
  class TestHelper {

    static public NewClient testClient() {
      var update = new NewClient {
        Name = "TEST__Charlie",
      };
      update.Address.PhoneNumber = "Withheld";
      update.Address.Email = "charlie@test.com";
      return update;
    } // testClient

    static public void clientCleanUp() {
      using (var db = new DatabaseContext()) {

        var toRemove = db.Clients.Where(c => c.Name.StartsWith("TEST__")).Include(c => c.Notes).Include(c => c.RiskAssessments).Include(c => c.Address);
        foreach (var c in toRemove) {
          db.Addresses.Remove(c.Address);
          c.Notes.Clear();
          c.RiskAssessments.Clear();
        } // foreach

        db.Clients.RemoveRange(toRemove);
        db.SaveChanges();
      } // using
    } // clientCleanUp

    /// ///////////////////////////
    static private int OrgCount = 0;
    static public Organisation testOrg() {
      ++OrgCount;

      var org = new Organisation();
      org.Name = String.Format("ORGTEST__Organisation__{0}", OrgCount);
      org.Address.Details = addressLine();
      org.Address.Email = "test@test.com";
      org.Contact = "Testy McTestington";
      return org;
    } // testOrg

    static public Project newProject(string name) {
      var proj =  new Project();
      proj.Name = String.Format("ORGTEST__{0}", name);
      proj.Address.Details = addressLine();
      proj.RiskFramework = "risky";
      return proj;
    } // newProject

    static public Location newLocation(string name) {
      var loc = new Location();
      loc.Name = String.Format("ORGTEST__{0}", name);
      loc.Address.Details = addressLine();
      return loc;
    } // newLocation

    static public CreateStaffMember newStaff(string name) {
      string userName = "ORGTEST__" + name.Replace(" ", "_");
      return new CreateStaffMember {
        Name = name,
        UserName = userName,
        Email = "ORGTEST__@smith.com",
        Password = "chunky",
        ConfirmPassword = "chunky"
      };
    } // newStaff

    static private int AddressCount = 0;
    static private string addressLine() {
      return String.Format("ORGTEST__{0} Test Street", ++AddressCount);
    }

    static public void orgCleanUp() {
      var db = new DatabaseContext();

      var usersToRemove = db.Users.Where(u => u.UserName.StartsWith("ORGTEST__"));
      var locationsToRemove = db.Locations.Where(l => l.Name.StartsWith("ORGTEST__"));
      var staffToRemove = db.StaffMembers.Where(s => s.UserName.StartsWith("ORGTEST__"));
      var comToRemove = db.Commissioners.Where(c => c.Organisation.Name.StartsWith("ORGTEST__"));
      var poToRemove = db.ProjectOrganisations.Where(po => po.Details.Name.StartsWith("ORGTEST__"));
      var projToRemove = db.Projects.Where(p => p.Name.StartsWith("ORGTEST__"));
      var orgToRemove = db.Organisations.Where(o => o.Name.StartsWith("ORGTEST__"));
      var addToRemove = db.Addresses.Where(a => a.Details.StartsWith("ORGTEST__"));

      foreach (var user in usersToRemove)
        db.Users.Remove(user);
      db.Locations.RemoveRange(locationsToRemove);
      db.StaffMembers.RemoveRange(staffToRemove);
      db.Commissioners.RemoveRange(comToRemove);
      db.Projects.RemoveRange(projToRemove);
      db.ProjectOrganisations.RemoveRange(poToRemove);
      db.Organisations.RemoveRange(orgToRemove);
      db.Addresses.RemoveRange(addToRemove);

      db.SaveChanges();
    } // orgClientUp
  } // TestHelper
} // namespace ...
