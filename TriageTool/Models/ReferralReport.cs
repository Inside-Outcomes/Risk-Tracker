using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RiskTracker.Models;

namespace TriageTool.Models {
  public class ReferralReport {
    public ReferralReport(String user) { user_ = user; }

    public String By { get { return user_; } }
    public String Date { get { return date_; } }
    public IDictionary<Guid, IList<ReferralAgency>> Referrals { get { return referrals_; } }
    public IList<ReferralAgency> ReferralsFor(Guid riskId) {
      if (referrals_.ContainsKey(riskId))
        return referrals_[riskId];
      return new List<ReferralAgency>();
    }

    public void add(Guid riskId, ReferralAgency referral) {
      if (referrals_.ContainsKey(riskId))
        referrals_[riskId].Add(referral);
      else {
        var l = new List<ReferralAgency>();
        l.Add(referral);
        referrals_[riskId] = l;
      }
    }
    private IDictionary<Guid, IList<ReferralAgency>> referrals_ = new Dictionary<Guid, IList<ReferralAgency>>();
    private String user_;
    private String date_ = DateTime.Now.ToLongDateString();
  }
}