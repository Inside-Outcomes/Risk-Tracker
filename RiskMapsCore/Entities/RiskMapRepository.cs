using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace RiskTracker.Entities {
  public class RiskMapRepository : BaseRepository {
    private static IList<RiskMap> riskMaps_;
    private static IList<Risk> risks_;

    private IList<Risk> risks() {
      if (risks_ != null)
        return risks_;

      risks_ = context.Risks.ToList();

      return risks_;
    } // risks

    public IList<RiskMap> RiskMaps() {
      if (riskMaps_ != null)
        return riskMaps_;

      riskMaps_ = context.RiskMaps.ToList();
      foreach (var rm in riskMaps_)
        rm.populate(risks());
      return riskMaps_;
    } // RiskMaps

    public RiskMap RiskMap(string name) {
      return RiskMaps().Where(rm => rm.Name == name).SingleOrDefault();
    } // RiskMap

    public RiskMap CreateRiskMap(RiskMap newRiskMap) {
      newRiskMap.Id = Guid.NewGuid();
      if (newRiskMap.RiskIds[newRiskMap.RiskIds.Length - 1] == '|')
        newRiskMap.RiskIds = newRiskMap.RiskIds.Substring(0, newRiskMap.RiskIds.Length - 1);
      context.RiskMaps.Add(newRiskMap);
      Commit();

      riskMaps_ = null;
      return RiskMap(newRiskMap.Name);
    } // CreateRiskMap

    public RiskMap UpdateRiskMap(string name, RiskMap update) {
      var r = RiskMap(name);
      if (r == null)
        return null;

      r.RiskIds = update.RiskIds;
      if (r.RiskIds[r.RiskIds.Length - 1] == '|')
        r.RiskIds = r.RiskIds.Substring(0, r.RiskIds.Length - 1);
      Commit(r);
      riskMaps_ = null;
      return RiskMap(name);
    } // UpdateRiskMap

    /// ////////////////////////////////
    public IList<Risk> Risks() {
      return risks().OrderBy(r => r.Grouping).OrderBy(r => r.Category).OrderBy(r => r.Theme).ToList();
    } // Risks

    public Risk Risk(Guid guid) {
      return Risks().Where(r => r.Id == guid).SingleOrDefault();
    } // Risk

    public Risk CreateRisk(Risk newRisk) {
      Risk r = new Risk();
      r.Id = Guid.NewGuid();
      r.CopyFrom(newRisk);

      context.Risks.Add(r);
      Commit();
      risks_ = null;
      return r;
    } // CreateRisk

    public Risk UpdateRisk(Guid guid, Risk update) {
      Risk risk = Risk(guid);
      if (risk == null)
        return null;

      risk.CopyFrom(update);
      Commit(risk);
      risks_ = null;
      return risk;
    } // UpdateRisk
  } // class RiskMapRepository
} // namespace