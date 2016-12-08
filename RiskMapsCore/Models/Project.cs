using System;
using System.Collections.Generic;
using System.Linq;
using RiskTracker.Entities;

namespace RiskTracker.Models {
  public class Project {
    private ProjectData proj_;
    private IList<Organisation> commissioners_;

    public Project() {
      proj_ = new ProjectData();
      proj_.Address = new AddressData();
      commissioners_ = new List<Organisation>();
    } // Project

    public Project(ProjectData proj) {
      proj_ = proj;
      commissioners_ = new List<Organisation>();
      if (proj_.CommissioningOrganisations != null)
        foreach (OrganisationData od in proj_.CommissioningOrganisations.Select(o => o.Organisation))
          commissioners_.Add(new Organisation(od));
    } // Project

    public Guid Id { get { return proj_.Id; } set { proj_.Id = value; } }
    public string Name { get { return proj_.Name; } set { proj_.Name = value; } }
    public AddressData Address { get { return proj_.Address; } }
    public string RiskFramework { get { return proj_.RiskFramework; } set { proj_.RiskFramework = value; } }
    public IEnumerable<Organisation> Commissioners { get { return commissioners_; } }
    public IEnumerable<ProjectQuestionData> Questions { get { return proj_.Questions; } }

    public ProjectData project() { return proj_; }
  } // class Project
} // namespace ...