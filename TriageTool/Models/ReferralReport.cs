using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TriageTool.Models {
  public class ReferralReport {
    public ReferralReport(String user) { user_ = user; }

    public String By { get { return user_; } }
    public String Date { get { return date_; } }
    public IDictionary<Guid, IList<ReferralAgency>> Referrals { get { return referrals_; } }

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

  public class ReferralAgency {
    public ReferralAgency(String risk, String name, String description, String email, String phone, String url) {
      risk_ = risk;
      name_ = name;
      desc_ = description;
      email_ = email;
      url_ = url;
      phone_ = phone;
    }

    public String Risk { get { return risk_; } }
    public String Name { get { return name_; } }
    public String Description { get { return desc_; } }
    public String Email { get { return email_; } }
    public String Website { get { return url_; } }
    public String PhoneNumber { get { return phone_; } }

    private String risk_;
    private String name_;
    private String desc_;
    private String email_;
    private String url_;
    private String phone_;
  }
}