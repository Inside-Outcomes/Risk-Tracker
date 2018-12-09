using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RiskTracker.Entities {
  public class ReferralAgencyData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    public AddressData Address { get; set; }
    public string AssociatedRiskIds { get; set; }
    public DateTime? ReviewDate { get; set; }

    public IList<Guid> Risks() {
      return !String.IsNullOrEmpty(AssociatedRiskIds) ?
        AssociatedRiskIds.Split('|').Select(s => Guid.Parse(s)).ToList() :
        new List<Guid>();
    } // RiskMaps

    public void setRisks(IList<Guid> riskMapIds) {
      AssociatedRiskIds = String.Join("|", riskMapIds.Select(id => id.ToString()));
    } // setRiskMaps

    public void CopyFrom(ReferralAgencyData other) {
      Name = other.Name;
      Description = other.Description;
      Address.CopyFrom(other.Address);
      AssociatedRiskIds = other.AssociatedRiskIds;
      ReviewDate = other.ReviewDate;
    } // CopyFrom
  }
}
