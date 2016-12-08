using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RiskTracker.Entities;
using System.ComponentModel.DataAnnotations;

namespace RiskTracker.Models {
  public class StaffMember {
    protected readonly StaffMemberData sd_;
    private IList<Guid> projectIds_;
    private IList<string> roles_;

    public StaffMember(StaffMemberData sd, IList<string> roles) {
      sd_ = sd;
      projectIds_ = sd.Projects();
      roles_ = roles;
    } // StaffMember

    public StaffMember() {
      sd_ = new StaffMemberData();
      projectIds_ = new List<Guid>();
      roles_ = new List<string>();
    } // StaffMemberData

    public Guid Id { get { return sd_.Id; } set { sd_.Id = value; } }
    [Required]
    public string Name { get { return sd_.Name; } set { sd_.Name = value; } }
    public string LoginId { get { return sd_.UserName; } }
    public string Email { get { return sd_.Email; } set { sd_.Email = value; } }
    public string PhoneNumber { get { return sd_.PhoneNumber; } set { sd_.PhoneNumber = value; } }
    public IList<Guid> ProjectIds { get { return projectIds_; } }
    public IList<string> Roles { get { return roles_; } }

    public StaffMemberData staffData() {
      sd_.setProjects(projectIds_);
      return sd_;
    }
  } // class StaffMember

  public class CreateStaffMember : StaffMember {
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [Display(Name = "user name")]
    public string UserName { get { return sd_.UserName; } set { sd_.UserName = value; } }
  } // class CreateStaffMember

  public class PasswordUpdate {
    public Guid Id { get; set; }
    public string LoginId { get; set; }
    public string Password { get; set; }
  }
} // namespace RiskTracker.Models