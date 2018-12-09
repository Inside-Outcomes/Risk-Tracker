using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RiskTracker.Entities {
  public class OrganisationData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public AddressData Address { get; set; }
    public string Contact { get; set; }
    public string RiskMapIds { get; set; }
    public bool IsSuspended { get; set; }
 
    public IList<Guid> RiskMaps() {
      return !String.IsNullOrEmpty(RiskMapIds) ?
        RiskMapIds.Split('|').Select(s => Guid.Parse(s)).ToList() :
        new List<Guid>();
    } // RiskMaps

    public void setRiskMaps(IList<Guid> riskMapIds) {
      RiskMapIds = String.Join("|", riskMapIds.Select(id => id.ToString()));
    } // setRiskMaps
 
    public void CopyFrom(OrganisationData other) {
      Name = other.Name;
      Address.CopyFrom(other.Address);
      Contact = other.Contact;
      RiskMapIds = other.RiskMapIds;
      IsSuspended = other.IsSuspended;
    } // CopyFrom
  } // Organisation

  public class StaffMemberData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ProjectIds { get; set; }

    public void CopyFrom(StaffMemberData other) {
      Name = other.Name;
      Email = other.Email;
      PhoneNumber = other.PhoneNumber;
      ProjectIds = other.ProjectIds;
    } // CopyFrom

    public IList<Guid> Projects() {
      return !String.IsNullOrEmpty(ProjectIds) ?
        ProjectIds.Split('|').Select(s => Guid.Parse(s)).ToList() :
        new List<Guid>();
    } // Projects

    public void setProjects(IList<Guid> projectIds) {
      ProjectIds = String.Join("|", projectIds.Select(id => id.ToString()));
    } // setProjects
  } // StaffMemberData

  public class LocationData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public AddressData Address { get; set; }
    public string Notes { get; set; }
    public string ProjectIds { get; set; }

    public void CopyFrom(LocationData other) {
      Name = other.Name;
      Address.CopyFrom(other.Address);
      Notes = other.Notes;
      ProjectIds = other.ProjectIds;
    } // CopyFrom

    public IList<Guid> Projects() {
      return !String.IsNullOrEmpty(ProjectIds) ?
        ProjectIds.Split('|').Select(s => Guid.Parse(s)).ToList() :
        new List<Guid>();
    } // Projects

    public void setProjects(IList<Guid> projectIds) {
      ProjectIds = String.Join("|", projectIds.Select(id => id.ToString()));
    } // setProjects
  } // class LocationData

  public class ProjectOrganisationData {
    [Key]
    public Guid Id { get; set; }
    public OrganisationData Details { get; set; }
    public List<ProjectData> Projects { get; set; }
    public List<StaffMemberData> Staff { get; set; }
    public List<LocationData> Locations { get; set; }
    public List<ReferralAgencyData> ReferralAgencies { get; set; }

    public string Application { get; set; }
  } // ProjectOrganisation
} // RiskTracker.Entities