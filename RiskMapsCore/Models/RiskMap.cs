using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskTracker.Entities;

namespace RiskTracker.Models {
  public class RiskMap {
    private RiskMapData riskMap_;

    public RiskMap(RiskMapData riskMap) {
      riskMap_ = riskMap;
    } // RiskMap

    public Guid Id { get { return riskMap_.Id; } }
    public string Name { get { return riskMap_.Name; } }
    public IList<Risk> Risks { get { return riskMap_.Risks(); } }
    public Guid? OwningOrganisation { get { return riskMap_.OwningOrganisation; } }

    public IList<string> AllThemes() {
      var builtInThemes = new List<string> { "personal circumstances", "behaviour", "status" };
      var bit = new List<string>();
      foreach (var risk in Risks) {
        var index = builtInThemes.IndexOf(risk.Theme.ToLower());
        if (index == -1)
          continue;
        if (bit.IndexOf(risk.Theme.ToLower()) == -1 && bit.IndexOf(risk.Theme) == -1)
          bit.Add(risk.Theme.ToLower());
      }
      var themes = new List<string>();
      foreach (var t in builtInThemes) {
        var index = bit.IndexOf(t);
        if (index == -1)
          continue;
        themes.Add(t);
      } 

      foreach (var risk in Risks) {
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
      var categories = new HashSet<string>(Risks.Select(r => r.Category));
      return categories.OrderBy(c => c).ToList();
    } // AllCategories
  } // RiskMap

  public class RiskMapUpdate {
    public string Name { get; set; }
    public IList<RiskId> Risks { get; set; }

    public string RiskIds {
      get {
        return String.Join("|", Risks.Select(r => r.Id));
      }
    } // RiskIds
  } // class RiskMapUpdate

  public class RiskId { 
    public string Id { get; set; }
  }

/*  class Risk {
    RiskData risk_;

    Risk(RiskData risk) {
      risk_ = risk;
    } // Risk

    public Guid Id { get { return risk_.Id; } }
    public string Title { get { return risk_.Title; } }
    public string Score { get { return risk_.Score; } }
    public string Theme { get { return risk_.Category; } }
    public string Guidance { get { return risk_.Guidance; } }
    public string Grouping { get { return risk_.Grouping; } }

    public Guid? OwningOrganisation { get { return risk_.OwningOrganisation; } }
  } // class Risk
*/
}

