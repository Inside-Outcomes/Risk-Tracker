using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using RiskTracker.Models;

namespace RiskTracker.Entities {
  public class RiskMapRepository : BaseRepository {
    private static List<RiskMap> riskMaps_;
    private static List<Risk> currentRisks_;

    private Guid? orgId_;

    public RiskMapRepository(Guid? orgId) {
      orgId_ = orgId;
    } // RiskMapRepository

    private List<Risk> risks(bool incDeleted) {
      if (incDeleted)
        return context.Risks.ToList();

      if (currentRisks_ != null)
        return currentRisks_;

      currentRisks_ = context.Risks.Where(risk => risk.Deleted == false).ToList();

      return currentRisks_;
    } // risks

    public IList<RiskMap> RiskMaps() {
      if (riskMaps_ == null) {
        var rmData = context.RiskMaps.ToList();
        foreach (var rm in rmData)
          rm.populate(orgrisks(false));

        riskMaps_ = rmData.Select(m => new RiskMap(m)).ToList();
      } // if not loaded ...
        
        
      if (!orgId_.HasValue)
        return riskMaps_.Where(rm => !rm.OwningOrganisation.HasValue).ToList();

      var po = new ProjectOrganisationRepository().Get(orgId_.Value);
      var maps = riskMaps_.Where(rm => po.RiskMaps.Contains(rm.Id) || rm.OwningOrganisation == orgId_).ToList();
      return maps;
    } // RiskMaps

    public RiskMap RiskMap(Guid id) {
      return RiskMaps().Where(rm => rm.Id == id).SingleOrDefault();
    } // RiskMap

    public RiskMapData riskMapData(Guid id) {
      return context.RiskMaps.Where(rm => rm.Id == id).SingleOrDefault();
    } // riskMapData

    public RiskMap CreateRiskMap(RiskMapUpdate riskMap) {
      var newRiskMap = new RiskMapData();
      newRiskMap.Id = Guid.NewGuid();
      newRiskMap.Name = riskMap.Name;
      newRiskMap.OwningOrganisation = orgId_;
      newRiskMap.RiskIds = riskMap.RiskIds;

      context.RiskMaps.Add(newRiskMap);
      Commit();

      riskMaps_ = null;
      return RiskMap(newRiskMap.Id);
    } // CreateRiskMap

    public RiskMap UpdateRiskMap(Guid id, RiskMapUpdate update) {
      var r = riskMapData(id);
      if (r == null)
        return null;

      r.Name = update.Name;
      r.RiskIds = update.RiskIds;
      Commit(r);
      riskMaps_ = null;
      return RiskMap(id);
    } // UpdateRiskMap

    /// ////////////////////////////////
    private IList<Risk> orgrisks(bool incDeleted) {
      var allRisks = risks(incDeleted);

      var currentRisks = (!orgId_.HasValue) ?
        allRisks.Where(risk => !risk.OwningOrganisation.HasValue) :
        allRisks.Where(risk => (!risk.OwningOrganisation.HasValue) || (risk.OwningOrganisation == orgId_)).ToList();

      return currentRisks.OrderBy(r => r.Grouping).OrderBy(r => r.Category).OrderBy(r => r.Theme).Distinct().ToList();
    } // orgrisks
    
    public IList<Risk> Risks() {
      return orgrisks(false);
    } // Risks

    public IList<Risk> RisksCurrentAndDeleted() {
      return orgrisks(true);
    } // RisksCurrentAndDeleted

    public Risk Risk(Guid guid) {
      return Risks().Where(r => r.Id == guid).SingleOrDefault();
    } // Risk

    public Risk CreateRisk(Risk newRisk) {
      Risk r = new Risk();
      r.Id = Guid.NewGuid();
      r.Deleted = false;
      r.CopyFrom(newRisk);
      r.OwningOrganisation = orgId_;

      context.Risks.Add(r);
      Commit();
      currentRisks_ = null;
      return r;
    } // CreateRisk

    public Risk UpdateRisk(Guid guid, Risk update) {
      Risk risk = Risk(guid);
      if (risk == null)
        return null;

      risk.CopyFrom(update);
      Commit(risk);
      currentRisks_ = null;
      return risk;
    } // UpdateRisk

    public Risk DeleteRisk(Guid guid) {
      Risk risk = Risk(guid);
      if (risk == null)
        return null;

      risk.Deleted = true;
      Commit(risk);
      currentRisks_ = null;
      return risk;
    } // DeleteRisk

    /////////////////////////////////////////////////
    public IList<OutcomeFramework> OutcomeFrameworks() {
      return context.OutcomeFrameworks.
        Where(of => (of.OwningOrganisation == null ||
                     of.OwningOrganisation == orgId_)).
        ToList();
    } // OutcomeFrameworks

    public OutcomeFramework OutcomeFramework(Guid guid) {
      return OutcomeFrameworks().
        Where(of => of.Id == guid).
        SingleOrDefault();
    } // OutcomeFramework 
    
    public OutcomeFramework CreateOutcomeFramework(OutcomeFramework newOF) {
      OutcomeFramework of = new OutcomeFramework();
      of.Id = Guid.NewGuid();
      of.CopyFrom(newOF);

      context.OutcomeFrameworks.Add(of);
      Commit();

      return of;
    } // CreateOutcomeFramework

    public OutcomeFramework UpdateOutcomeFramework(Guid guid, OutcomeFramework update) {
      OutcomeFramework of = OutcomeFramework(guid);
      if (of == null)
        return null;

      of.CopyFrom(update);
      Commit(of);

      return of;
    } // UpdateOutcomeFramework
  } // class RiskMapRepository
} // namespace