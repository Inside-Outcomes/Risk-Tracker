using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RiskTracker.Entities;
using System.ComponentModel.DataAnnotations;

namespace RiskTracker.Models {
  public class Location {
    private LocationData ld_;
    private IList<Guid> projectIds_;

    public Location(LocationData ld) {
      ld_ = ld;
      projectIds_ = ld_.Projects();
    } // Location

    public Location() {
      ld_ = new LocationData();
      ld_.Address = new AddressData();
      projectIds_ = new List<Guid>();
    }

    public Guid Id { get { return ld_.Id; } set { ld_.Id = value; } }
    public string Name { get { return ld_.Name; } set { ld_.Name = value; } }
    public AddressData Address { get { return ld_.Address; } }
    public string Notes { get { return ld_.Notes; } set { ld_.Notes = value; } }
    public IList<Guid> ProjectIds { get { return projectIds_; } }

    public LocationData locationData() {
      ld_.setProjects(projectIds_);
      return ld_;
    } // locationData
  } // class Location

} // namespace RiskTracker.Models