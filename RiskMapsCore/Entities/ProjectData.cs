using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace RiskTracker.Entities {
  public class ProjectData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public AddressData Address { get; set; }
    [Required]
    public Guid RiskFramework { get; set; }
    public IList<CommissioningOrganisation> CommissioningOrganisations { get; set; }
    public IList<ProjectQuestionData> Questions { get; set; }

    public void CopyFrom(ProjectData other) {
      Name = other.Name;
      Address.CopyFrom(other.Address);
      RiskFramework = other.RiskFramework;
    } // CopyFrom

  } // class Project

  public class CommissioningOrganisation {
    [Key]
    public Guid Id { get; set; }
    public OrganisationData Organisation { get; set; }
  } // class CommissioningOrganisation

  public class ProjectQuestionData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Question { get; set; }
    public string Answers { get; set; }
  } // ProjectQuestions
} // namespace ...