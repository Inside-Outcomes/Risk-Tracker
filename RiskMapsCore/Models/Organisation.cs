using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RiskTracker.Entities;
using System.ComponentModel.DataAnnotations;

namespace RiskTracker.Models {
  public class Organisation {
    protected readonly OrganisationData org_;
    private IList<Guid> riskMaps_;

    public Organisation() {
      org_ = new OrganisationData();
      org_.Address = new AddressData();
      riskMaps_ = new List<Guid>();
    } // Organisation

    public Organisation(OrganisationData org) {
      org_ = org;
      riskMaps_ = org.RiskMaps();
    } // Organisation

    public Guid Id { get { return org_.Id; } set { org_.Id = value; } }
    public string Name { get { return org_.Name; } set { org_.Name = value; } }
    public AddressData Address { get { return org_.Address; } }
    public string Contact { get { return org_.Contact; } set { org_.Contact = value; } }
    public IList<Guid> RiskMaps { get { return riskMaps_; } }
    public bool IsSuspended { get { return org_.IsSuspended; } set { org_.IsSuspended = value; } }

    public OrganisationData organisation() {
      org_.setRiskMaps(riskMaps_);
      return org_; 
    }
  } // class Organisation

  public class ProjectOrganisation : Organisation {
    protected readonly IList<ProjectData> projects_;
    protected readonly int staffCount_;
    protected readonly int locationCount_;
    protected readonly int referralAgencyCount_;

    public ProjectOrganisation(ProjectOrganisationData projOrg) : 
        base(projOrg.Details) {
      projects_ = projOrg.Projects != null ? projOrg.Projects : new List<ProjectData>();
      staffCount_ = projOrg.Staff != null ? projOrg.Staff.Count() : 0;
      locationCount_ = projOrg.Locations != null ? projOrg.Locations.Count() : 0;
      referralAgencyCount_ = projOrg.ReferralAgencies != null ? projOrg.ReferralAgencies.Count() : 0;
    } // ProjectOrganisation

    public int ProjectCount { get { return projects_.Count(); } }
    public IList<string> ProjectNames { 
      get { return projects_.Select(p => p.Name).ToList(); }
    } // ProjectNames

    public int StaffCount { get { return staffCount_; } }
    public int LocationCount { get { return locationCount_; } }
    public int ReferralAgencyCount { get { return referralAgencyCount_;  } }
  } // ProjectOrganisation
} // namespace ...