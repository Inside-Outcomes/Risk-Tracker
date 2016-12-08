using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RiskTracker.Entities {
  public class ClientData {
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string ReferenceId { get; set; }
    public AddressData Address { get; set; }
    public DemographicData Demographics { get; set; }
    public Guid ProjectId { get; set; }
    public Guid LocationId { get; set; }
    public List<NoteData> Notes { get; set; }
    public List<RiskAssessmentData> RiskAssessments { get; set; }
    public List<ProjectAnswerData> ProjectAnswers { get; set; }
    public bool? Discharged { get; set; }
    public DateTime? registeredOn() {
      var registrationNote = Notes.Where(n => n.Type == NoteType.Registered).SingleOrDefault();
      if (registrationNote == null)
        return null;
      return registrationNote.Timestamp;
    }
    public DateTime? dischargedOn() {
      if (Discharged != true)
        return null;
      return Notes.
          Where(n => n.Type == NoteType.Discharged).
          OrderByDescending(n => n.Timestamp).
          Select(n => n.Timestamp).
          First();
    }
    public NoteData referredBy() {
      return Notes.Where(n => n.Type == NoteType.Narrative).Where(n => n.Text.StartsWith("Referred by")).SingleOrDefault();
    } // referredBy

    public void CopyFrom(ClientData other) {
      if (other.Name != null)
        Name = other.Name;
      if (other.ReferenceId != null)
        ReferenceId = other.ReferenceId;
      if (other.Address.Id != Guid.Empty)
        Address.CopyFrom(other.Address);
      if (other.Demographics.Id != Guid.Empty)
        Demographics.CopyFrom(other.Demographics);
      if (other.ProjectId != Guid.Empty)
        ProjectId = other.ProjectId;
      if (other.LocationId != Guid.Empty)
        LocationId = other.LocationId;
    } // CopyFrom
  } //Client

  public enum NoteType {
    Registered,
    General,
    Event,
    Narrative,
    DidNotAttend,
    Referral,
    Discharged,
    File,
    Log,
    Reopen
  }

  public class NoteData {
    [Key]
    public Guid Id { get; set; }
    public NoteType Type { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
  } // NoteData

  public class ProjectAnswerData {
    [Key]
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Text { get; set; }
  }

  public class RiskAssessmentData {
    public RiskAssessmentData() {
      RiskIds = "";
      Timestamp = DateTime.Now;
    } // RiskAssessmentData
    public RiskAssessmentData(Guid id) : this() {
      Id = id;
    } // RiskAssessmentData

    [Key]
    public Guid Id { get; set; }
    public string RiskIds { get; set; }
    public string ManagedRiskIds { get; set; }
    public string ResolvedRiskIds { get; set; }
    public DateTime Timestamp { get; set; }

    public IList<Guid> Risks() { return riskGuids(); }
    public IList<Guid> ManagedRisks() { return managedRiskGuids();  }
    public IList<Guid> ResolvedRisks() { return resolvedRiskGuids(); }

    public void AddRisk(Guid riskId) {
      if (resolvedRiskGuids().Contains(riskId))
        throw new Exception("This risk is already resolved");
      if (managedRiskGuids().Contains(riskId))
        throw new Exception("The client is already assigned");

      var riskIds = riskGuids();
      if (riskIds.Contains(riskId))
        return;

      riskIds.Add(riskId);
      setRisks(riskIds);
    } // AddRisk
    public void ManageRisk(Guid riskId) {
      if (resolvedRiskGuids().Contains(riskId))
        throw new Exception("This risk is already resolved");
      if (riskGuids().Contains(riskId))
        throw new Exception("The client is already assigned");

      var managedIds = managedRiskGuids();
      if (managedIds.Contains(riskId))
        return;

      managedIds.Add(riskId);
      setManagedRisks(managedIds);
    } // ManageRisk
    public void RemoveRisk(Guid riskId) {
      var riskIds = riskGuids();
      riskIds.Remove(riskId);
      setRisks(riskIds);

      riskIds = managedRiskGuids();
      riskIds.Remove(riskId);
      setManagedRisks(riskIds);

      var resolvedIds = resolvedRiskGuids();
      resolvedIds.Remove(riskId);
      setResolvedRisks(resolvedIds);
    } // RemoveRisk
    public void ResolveRisk(Guid riskId) {
      var resolvedIds = resolvedRiskGuids();
      if (resolvedIds.Contains(riskId))
        return;
      if (!riskGuids().Contains(riskId) && !managedRiskGuids().Contains(riskId))
        throw new Exception("The client has not been assessed for this risk");

      resolvedIds.Add(riskId);
      RemoveRisk(riskId);
      setResolvedRisks(resolvedIds);
    } // ResolveRisk
    public void ReopenRisk(Guid riskId) {
      var resolvedIds = resolvedRiskGuids();
      if (!resolvedIds.Contains(riskId))
        throw new Exception("The client has not resolved this risk");

      resolvedIds.Remove(riskId);
      setResolvedRisks(resolvedIds);
      AddRisk(riskId);
    } // ReopenRisk

    private void setRisks(IEnumerable<Guid> ids) {
      RiskIds = stringizeGuids(ids);
    } // setRisks
    private void setManagedRisks(IEnumerable<Guid> ids) {
      ManagedRiskIds = stringizeGuids(ids);
    }
    private void setResolvedRisks(IEnumerable<Guid> ids) {
      ResolvedRiskIds = stringizeGuids(ids);
    } // setResolvedRisks
    private string stringizeGuids(IEnumerable<Guid> ids) {
      return String.Join("|", ids.Select(ri => ri.ToString()));
    } // stringizeGuids

    private List<Guid> riskGuids() { return guidList(RiskIds); }
    private List<Guid> managedRiskGuids() { return guidList(ManagedRiskIds);  }
    private List<Guid> resolvedRiskGuids() { return guidList(ResolvedRiskIds); }
    private List<Guid> guidList(string risks) {
      if (String.IsNullOrEmpty(risks))
        return new List<Guid>();
      return risks.Split('|').Select(g => Guid.Parse(g)).ToList();
    } // guidList
  } // RiskAssessment
}