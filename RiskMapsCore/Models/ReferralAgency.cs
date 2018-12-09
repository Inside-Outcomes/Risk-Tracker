using System;
using System.Collections.Generic;
using System.Linq;
using RiskTracker.Entities;

namespace RiskTracker.Models {
  public class ReferralAgency {
    private ReferralAgencyData agency_;
    private IList<Guid> riskIds_;

    public ReferralAgency() {
      agency_ = new ReferralAgencyData();
      agency_.Address = new AddressData();
      riskIds_ = new List<Guid>();
    } // ReferralAgency

    public ReferralAgency(ReferralAgencyData rad) {
      agency_ = rad;
      riskIds_ = rad.Risks();
    } // ReferralAgency

    public Guid Id { get { return agency_.Id; } set { agency_.Id = value; } }
    public string Name { get { return agency_.Name; } set { agency_.Name = value; } }
    public string Description { get { return agency_.Description; } set { agency_.Description = value; } }
    public AddressData Address { get { return agency_.Address; } }
    public IList<Guid> AssociatedRiskIds { get { return riskIds_; } }
    public DateTime? ReviewDate { get { return agency_.ReviewDate; } set { agency_.ReviewDate = value; } }

    public ReferralAgencyData referralAgencyData() {
      agency_.setRisks(riskIds_);
      return agency_; 
    }
  } // class ReferralAgency
} // namespace ...
