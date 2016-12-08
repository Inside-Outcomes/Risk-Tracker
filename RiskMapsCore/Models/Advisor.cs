using System;
using System.Collections.Generic;
using RiskTracker.Entities;
using System.Linq;

namespace RiskTracker.Models {
  public class Staff {
    private StaffMemberData smd_;
    private Organisation org_;

    public Staff(StaffMemberData smd,
                 ProjectOrganisationData pod) {
      smd_ = smd;
      org_ = new Organisation(pod.Details);
    } // Advisor

    public string Name { get { return smd_.Name; } }
    public string Email { get { return smd_.Email; } }
    public string PhoneNumber { get { return smd_.PhoneNumber; } }
    public Organisation Organisation { get { return org_; } }
    public List<Project> Projects { get; protected set; }
    public List<Location> Locations { get; protected set; }
  } // class Staff
  
  public class Advisor : Staff {
    public Advisor(StaffMemberData smd,
                   ProjectOrganisationData pod) : 
        base(smd, pod) {
      Projects = pod.Projects.Where(p => smd.Projects().Contains(p.Id)).Select(p => new Project(p)).OrderBy(p => p.Name).ToList();
      Locations = pod.Locations.Where(l => smd.Projects().Intersect(l.Projects()).Count() != 0).Select(l => new Location(l)).OrderBy(l => l.Name).ToList();
    } // Advisor
  } // class Advisor

  public class Coordinator : Advisor {
    public Coordinator(StaffMemberData smd,
                       ProjectOrganisationData pod) :
        base(smd, pod) {
    } // Coordinator
  } // class Coordinator

  public class Supervisor : Staff {
    public Supervisor(StaffMemberData smd,
                   ProjectOrganisationData pod) : 
        base(smd, pod) {
      Projects = pod.Projects.Select(p => new Project(p)).OrderBy(p => p.Name).ToList();
      Locations = pod.Locations.Select(l => new Location(l)).OrderBy(l => l.Name).ToList();
    } // Supervisor
  } // class Supervisor

  public class Manager : Supervisor {
    public Manager(StaffMemberData smd,
                   ProjectOrganisationData pod) :
        base(smd, pod) {
    } // Manager
  } // class Manager
} // namespace RiskTracker.Models