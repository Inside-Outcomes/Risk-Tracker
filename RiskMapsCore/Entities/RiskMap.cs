using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RiskTracker.Entities {
  public class RiskMapData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string RiskIds { get; set; }
    public Guid? OwningOrganisation { get; set; }
    private IList<Risk> risks_;

    public RiskMapData() { }

    public static RiskMapData create(
        string name,
        IList<Risk> risks,
        Guid? ownerOrg) {
      string riskIds = String.Join("|", risks.Select(r => r.Id.ToString()));
      var riskMap = new RiskMapData();
      riskMap.Id = Guid.NewGuid();
      riskMap.Name = name;
      riskMap.OwningOrganisation = ownerOrg;
      riskMap.RiskIds = riskIds;
      return riskMap;
    } // create

    public void populate(IList<Risk> risks) {
      if (RiskIds.Length == 0) {
        risks_ = new List<Risk>();
        return;
      } // if ...

      IList<Guid> guids = RiskIds.Split('|').Select(g => Guid.Parse(g)).ToList();
      risks_ = risks.Where(r => guids.Contains(r.Id)).ToList();
    }

    public IList<Risk> Risks() { return risks_; }
  } // class RiskMap

  public class Risk {
    private string category;

    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    public string Score { get; set; }
    public string Theme { get; set; }
    public string Category {
      get { return category; }
      set {
        this.category = value;
        if (value.Length > 1)
          this.category = char.ToUpper(value[0]) + value.Substring(1).ToLower(); }
    }
    public string Guidance { get; set; }
    public string Grouping { get; set; }

    public string NIHCEG { get; set; }
    public string IOST { get; set; }
    public string HCP { get; set; }
    public string SJOF { get; set; }
    public string ASCOF { get; set; }
    public Guid? OwningOrganisation { get; set; }
    [Required]
    public bool Deleted { get; set; }

    public void CopyFrom(Risk other) {
      Title = other.Title;
      Score = other.Score;
      Theme = other.Theme;
      Category = other.Category;
      Guidance = other.Guidance;
      Grouping = other.Grouping;
      NIHCEG = other.NIHCEG;
      IOST = other.IOST;
      HCP = other.HCP;
      SJOF = other.SJOF;
      ASCOF = other.ASCOF;
      OwningOrganisation = other.OwningOrganisation;
    } // CopyFrom
  } // class Risk

  public class OutcomeFramework {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid? OwningOrganisation { get; set; }

    public void CopyFrom(OutcomeFramework other) {
      Title = other.Title;
      Description = other.Description;
      OwningOrganisation = other.OwningOrganisation;
    } // CopyFrom
  } // class OutcomeFramework

} // namespace ...