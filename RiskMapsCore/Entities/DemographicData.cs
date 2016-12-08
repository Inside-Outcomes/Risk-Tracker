using System;
using System.ComponentModel.DataAnnotations;

namespace RiskTracker.Entities {
  public class DemographicData {
    [Key]
    public Guid Id { get; set; }
    public string EmploymentStatus { get; set; }
    public string EthnicOrigin { get; set; }
    public string Gender { get; set; }
    public DateTime? Dob { get; set; }
    public string Disability { get; set; }
    public string DisabilityType { get; set; }
    public string MaritalStatus { get; set; }
    public string HouseholdType { get; set; }
    public string HousingType { get; set; }
    public string HouseholdIncome { get; set; }

    public void CopyFrom(DemographicData other) {
      EmploymentStatus = other.EmploymentStatus;
      EthnicOrigin = other.EthnicOrigin;
      Gender = other.Gender;
      Dob = other.Dob;
      Disability = other.Disability;
      DisabilityType = other.DisabilityType;
      MaritalStatus = other.MaritalStatus;
      HouseholdType = other.HouseholdType;
      HousingType = other.HousingType;
      HouseholdIncome = other.HouseholdIncome;
    } // CopyFrom
  } // class DemographicData
} // namespace RiskTracker.Entities