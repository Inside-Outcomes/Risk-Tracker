using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RiskTracker.Entities {
  public class RiskMap {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string RiskIds { get; set; }
    private IList<Risk> risks_;

    public RiskMap() { }

    public static RiskMap create(
        string name,
        IList<Risk> risks) {
      string riskIds = String.Join("|", risks.Select(r => r.Id.ToString()));
      var riskMap = new RiskMap();
      riskMap.Id = Guid.NewGuid();
      riskMap.Name = name;
      riskMap.RiskIds = riskIds;
      return riskMap;
    } // create

    public void populate(IList<Risk> risks) {
      IList<Guid> guids = RiskIds.Split('|').Select(g => Guid.Parse(g)).ToList();
      risks_ = risks.Where(r => guids.Contains(r.Id)).ToList();
    }
    public IList<Risk> Risks() { return risks_; }

    public IList<string> AllThemes() {
      var themes = new List<string> { "personal circumstances", "behaviour", "status" };
      foreach (var risk in Risks()) {
        var index = themes.IndexOf(risk.Theme.ToLower());
        if (index == -1)
          index = themes.IndexOf(risk.Theme);

        if (index != -1)
          themes[index] = risk.Theme; // capitalise appropriate
        else
          themes.Add(risk.Theme.ToLower());     // whoa! new theme
      } // for ...
      return themes;
    } // AllThemes

    public IList<string> AllCategories() {
      var categories = new HashSet<string>(Risks().Select(r => r.Category));
      return categories.OrderBy(c => c).ToList();
    } // AllCategories
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
    } // CopyFrom
  } // class Risk
} // namespace ...