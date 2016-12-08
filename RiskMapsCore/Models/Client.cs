using System;
using System.Collections.Generic;
using RiskTracker.Entities;
using System.Linq;

namespace RiskTracker.Models {
  public class ClientName {
    protected ClientData client_;
    protected DateTime? lastUpdate_;

    public ClientName(ClientData client) {
      client_ = client;
      if (client.Notes != null && client.Notes.Count != 0)
        lastUpdate_ = client.Notes.
          OrderByDescending(n => n.Timestamp).
          Select(n => n.Timestamp).
          First();
    } // ClientName

    public Guid Id { get { return client_.Id; } }
    public string Name { get { return client_.Name; } }
    public string ReferenceId { get { return client_.ReferenceId; } }
    public DateTime? DateOfBirth { get { return client_.Demographics != null ? client_.Demographics.Dob : null; } }
    public Guid ProjectId { get { return client_.ProjectId; } }
    public Guid LocationId { get { return client_.LocationId; } }
    public DateTime? LastUpdate { get { return lastUpdate_; } }
    public DateTime? Discharged { get { return client_.dischargedOn(); } }
    public NoteData referredBy() { return client_.referredBy(); }
    public DateTime? registeredOn() { return client_.registeredOn();  }
  } // ClientName

  public class Client : ClientName {
    private RiskAssessment riskAssessment_;
    private List<TimelineEntry> timeLine_;
    private List<NoteData> files_;
    private String generalNote_;
    private List<QuestionAnswer> questions_;

    public Client(ClientData client, RiskMap riskMap) : this(client, riskMap, null) { }

    public Client(ClientData client, RiskMap riskMap, IList<ProjectQuestionData> projectQuestions) :
        base(client) {

      if (riskMap != null) {
        client.RiskAssessments.Sort(RiskSort);
        riskAssessment_ = new RiskAssessment(client.RiskAssessments, riskMap);
      } // if ...

      if (client_.Notes == null)
        return;

      generalNote_ = client_.Notes.
        Where(n => n.Type == NoteType.General).
        OrderByDescending(n => n.Timestamp).
        Select(n => n.Text).
        FirstOrDefault();

      timeLine_ = buildTimeLine(client.Notes, client.RiskAssessments, riskMap);
      files_ = client.Notes.
        Where(n => n.Type == NoteType.File).
        OrderByDescending(n => n.Timestamp).
        ToList();

      questions_ = new List<QuestionAnswer>();
      if (projectQuestions == null || projectQuestions.Count == 0) 
        return;

      foreach (var q in projectQuestions) {
        var answer = client.ProjectAnswers.Where(a => a.QuestionId == q.Id).SingleOrDefault();
        questions_.Add(new QuestionAnswer(q, answer));
      }
    } // Client

    public AddressData Address { get { return client_.Address;  } }
    public DemographicData Demographics { get { return client_.Demographics; } }
    public string GeneralNote { get { return generalNote_ != null ? generalNote_ : ""; } }
    public List<TimelineEntry> Timeline { get { return timeLine_; } }
    public RiskAssessment CurrentRiskAssessment { get { return riskAssessment_; } }
    public List<NoteData> Files { get { return files_;  } }
    public List<QuestionAnswer> Questions { get { return questions_; } }

    private static int RiskSort(RiskAssessmentData left, RiskAssessmentData right) {
      return right.Timestamp.CompareTo(left.Timestamp);
    } // RiskSort

    private static List<TimelineEntry> buildTimeLine(List<NoteData> notes,
                                                     List<RiskAssessmentData> riskAssessments,
                                                     RiskMap riskMap) {
                                                       Dictionary<DateTime, TimelineEntry> timeLineEntries = new Dictionary<DateTime, TimelineEntry>();

      foreach (var nd in notes.OrderByDescending(n => n.Timestamp)) {
        DateTime date = nd.Timestamp.Date;
        if (!timeLineEntries.Keys.Contains(date))
          timeLineEntries.Add(date, new TimelineEntry(date));
        TimelineEntry tle = timeLineEntries[date];

        if (nd.Type == NoteType.Narrative)
          tle.addNote(nd);
        if ((nd.Type == NoteType.Registered) ||
            (nd.Type == NoteType.Reopen) ||
            (nd.Type == NoteType.DidNotAttend) ||
            (nd.Type == NoteType.Discharged))
          tle.addNote(nd.Text);
        if (nd.Type == NoteType.File)
          tle.addNote("Uploaded " + nd.Text);
        if ((nd.Type == NoteType.Event) ||
            (nd.Type == NoteType.Referral))
          tle.addAction(nd.Text);
      } // foreach note

      foreach (var rad in riskAssessments.OrderByDescending(rad => rad.Timestamp)) {
        DateTime date = rad.Timestamp.Date;
        if (!timeLineEntries.Keys.Contains(date))
          timeLineEntries.Add(date, new TimelineEntry(date));
        TimelineEntry tle = timeLineEntries[date];

        RiskAssessment ra = new RiskAssessment(rad, riskMap);
        tle.addRiskAssessment(ra);
      } // foreach riskassessment

      List<TimelineEntry> timeLine = timeLineEntries.Values.OrderByDescending(tle => tle.Datestamp).ToList();
      if (timeLine.Count > 0) {
        TimelineEntry last = timeLine.Last();
        if (last.RiskScores == null)
          last.defaultRiskScores(riskMap);
        List<TimelineEntry.RiskScore> riskScore = last.RiskScores;
        for (int i = timeLine.Count - 1; i >= 0; --i) {
          TimelineEntry tle = timeLine[i];
          if (tle.RiskScores == null)
            tle.RiskScores = riskScore;
          else
            riskScore = tle.RiskScores;
        } // foreach
      } // if ...
      return timeLine;        
    } // 
  } // class Client

  public class QuestionAnswerUpdate {
    public Guid Id { get; set; }
    public string Answer { get; set; }
  }


  public class QuestionAnswer {
    private Guid id_;
    private string question_;
    private string answer_;
    private string[] options_;

    public QuestionAnswer(
      ProjectQuestionData pqd,
      ProjectAnswerData answer) {
        id_ = pqd.Id;
        question_ = pqd.Question;
        answer_ = answer != null ? answer.Text : "";
        options_ = pqd.Answers.Length != 0 ? pqd.Answers.Split('\n') : null;
    }

    public Guid Id { get { return id_; } }
    public string Question { get { return question_; } }
    public string Answer { get { return answer_; } }
    public string[] Options { get { return options_; } }
  }

  public class TimelineEntry {
    private DateTime date_;
    private List<TimelineNote> notes_;
    private List<TimelineNote> actions_;
    private List<RiskScore> riskScores_;
    
    public TimelineEntry(DateTime date) {
      date_ = date;
      notes_ = new List<TimelineNote>();
      actions_ = new List<TimelineNote>();
    } // TimelineEntry 

    public List<TimelineNote> Notes { get { return notes_;} }
    public List<TimelineNote> Actions { get { return actions_;} }
    public List<RiskScore> RiskScores { get { return riskScores_; } set { riskScores_ = value; } }
    public DateTime Datestamp { get { return date_; } }

    public void addNote(NoteData n) { add(notes_, n); }
    public void addNote(string n) { add(notes_, n); }
    public void addAction(string n) { add(actions_, n); }
    public void addRiskAssessment(RiskAssessment ra) {
      riskScores_ = ra.ThemeAssessments.
                       Select(ta => new RiskScore { Title = ta.Title, Score = ta.Score }).
                       ToList();
    } // addRiskAssessment
    public void defaultRiskScores(RiskMap riskMap) {
      if (riskMap == null) {
        riskScores_ = new List<RiskScore>();
        return;
      } // if ...

      riskScores_ = riskMap.AllThemes().
                       Select(t => new RiskScore { Title = t, Score = "0" }).
                       ToList();
    } // defaultRiskScores

    private void add(List<TimelineNote> nn, NoteData n) {
      nn.Add(new TimelineNote { Text = n.Text, Id = n.Id });
    } // add

    private void add(List<TimelineNote> nn, string n) {
      nn.Add(new TimelineNote { Text = n, Id = null });
    } // add


    public class RiskScore {
      public string Title { get; set; }
      public string Score { get; set; }
    } // RiskScore

    public class TimelineNote {
      public string Text { get; set; }
      public Guid? Id { get; set; }
    }
  } // TimelineEntry

  public class Note {
    private string text_;
    private DateTime ts_;

    public Note(NoteData n) {
      text_ = n.Text;
      ts_ = n.Timestamp;
    } // Note

    public string Text { get { return text_; } set { text_ = value; } }
    public DateTime Timestamp { get { return ts_; } }
  } // Note

  public class RiskAssessment {
    private static class Status {
      public const string AtRisk = "atRisk";
      public const string NotAtRisk = "notAtRisk";
      public const string ResolvedRisk = "resolvedRisk";
      public const string ManagedRisk = "managedRisk";
    } // Status

    private readonly string name_;
    private readonly IList<Guid> resolved_ = new List<Guid>();
    private readonly IList<Guid> risks_ = new List<Guid>();
    private readonly IList<Guid> managed_ = new List<Guid>();
    private readonly IList<Guid> notRisks_ = new List<Guid>();
    private readonly IList<ThemeAssessment> themeAssessments_ = new List<ThemeAssessment>();
    private readonly DateTime dateStamp_;

    public RiskAssessment(IList<RiskAssessmentData> rads, RiskMap riskMap) :
        this(rads.Count != 0 ? rads.First() : new RiskAssessmentData(), riskMap) {

      if (rads.Count != 0) {
        var allRiskAssessments = rads.Select(r => new RiskAssessment(r, riskMap)).ToList();
        setInitialScores(allRiskAssessments.Last());
        setHighScores(allRiskAssessments);
      } // if ...
    } // RiskAssessment

    public RiskAssessment(RiskAssessmentData rad, RiskMap riskMap) {
      name_ = riskMap.Name;
      dateStamp_ = rad.Timestamp.Date;

      var resolved = rad.ResolvedRisks();
      var managed = rad.ManagedRisks();
      var risks = rad.Risks();
      foreach (Guid id in riskMap.Risks().Select(r => r.Id)) {
        if (resolved.Contains(id))
          resolved_.Add(id);
        else if (managed.Contains(id))
          managed_.Add(id);
        else if (risks.Contains(id))
          risks_.Add(id);
        else
          notRisks_.Add(id);
      } // foreach ...

      var themes = riskMap.AllThemes();

      foreach (var theme in themes) {
        var categoryAssessments = buildThemeCategories(theme, riskMap);
        themeAssessments_.Add(new ThemeAssessment(theme, categoryAssessments, risks_, managed_));
      } // foreach
    } // RiskAssessment

    private void setInitialScores(RiskAssessment initial) {
      foreach (var theme in themeAssessments_) {
        var initialTheme = initial.ThemeAssessments.Where(t => t.Title == theme.Title).Single();
        theme.InitialScore = initialTheme.Score;
        theme.InitialRiskCount = initialTheme.RiskCount;
      } // foreach
    } // setInitialScores

    private void setHighScores(IList<RiskAssessment> ras) {
      foreach (var theme in themeAssessments_) {
        foreach (var ra in ras) {
          var rat = ra.ThemeAssessments.Where(t => t.Title == theme.Title).Single();
          theme.candidateHighScore(rat.Score, rat.RiskCount);
        } // foreach ...
      } // foreach ...
    } // setHighScores

    private IList<CategoryAssessment> buildThemeCategories(string themeName, RiskMap riskMap) {
      var themeCategories = new List<CategoryAssessment>();
      foreach (var category in riskMap.AllCategories()) {
        var catRisks = riskMap.Risks().
          Where(r => r.Theme == themeName).
          Where(r => r.Category == category);

        if (catRisks.Count() == 0)
          continue;

        var risks = catRisks.
          OrderBy(r => r.Grouping).
          ThenBy(r => riskScoreSort(r)).
          Select(r => new CategoryRisk(r, riskStatus(r))).
          ToList();
        themeCategories.Add(
          new CategoryAssessment(
            category,
            risks)
        );
      } // foreach
      return themeCategories;
    } // buildThemeCategories

    private static string riskScoreSort(Risk r) {
      // sort alpha score ascending but numeric score decending
      int s;
      if (!int.TryParse(r.Score, out s))
        return r.Score;

      return (100-s).ToString();
    } // riskScoreSort

    private string riskStatus(Risk risk) {
      var riskId = risk.Id;
      if (resolved_.Contains(riskId))
          return Status.ResolvedRisk;
      if (risks_.Contains(riskId))
          return Status.AtRisk;
      if (managed_.Contains(riskId))
          return Status.ManagedRisk;
      return Status.NotAtRisk;
    } // _riskStatus

    public string Name { get { return name_;  } }
    public IList<Guid> ResolvedRisk { get { return resolved_; } }
    public IList<Guid> AtRisk { get { return risks_; } }
    public IList<Guid> ManagedRisk { get { return managed_; } }
    public IList<Guid> NotAtRisk { get { return notRisks_; } }
    public IList<ThemeAssessment> ThemeAssessments { get { return themeAssessments_; } }
    public DateTime Datestamp { get { return dateStamp_; } }

    public class ThemeAssessment {
      private string title_;
      private IList<CategoryAssessment> cats_;
      private string score_;
      private string initialScore_;
      private int initialAtRiskCount_;
      private string highScore_;
      private int atRiskCount_;
      private int highRiskCount_;

      public ThemeAssessment(
          string title,
          IList<CategoryAssessment> cats,
          IList<Guid> atRisk, 
          IList<Guid> managedRisk) {
        title_ = title;
        cats_ = cats;

        var combined = new List<Guid>();
        combined.AddRange(atRisk);
        combined.AddRange(managedRisk);
        score_ = calculateScore(combined);

        atRiskCount_ = themeRisks(atRisk);
      } // ThemeAssessment

      public string Title { get { return title_; } }
      public IList<CategoryAssessment> Categories { get { return cats_; } }
      public string Score { get { return score_; } }
      public string InitialScore { get { return initialScore_; } set { initialScore_ = value; } }
      public string HighScore { get { return highScore_; } }
      public int RiskCount { get { return atRiskCount_; } }
      public int InitialRiskCount { get { return initialAtRiskCount_; } set { initialAtRiskCount_ = value; } }
      public int HighRiskCount { get { return highRiskCount_; } }

      public void candidateHighScore(string cand, int riskCount) { 
        highScore_ = findHigherScore(cand); 
        highRiskCount_ = Math.Max(atRiskCount_, riskCount);
      }

      public string findHigherScore(string cand) {
        if (highScore_ == null)
          return cand;

        var currentScore = 0;
        var candidateScore = 0;
        var currentIsInt = int.TryParse(highScore_, out currentScore);
        var candidateIsInt = int.TryParse(cand, out candidateScore);

        if (currentIsInt && candidateIsInt)
          return Math.Max(currentScore, candidateScore).ToString();
        if (!currentIsInt && !candidateIsInt)
          return String.Compare(highScore_, cand) < 0 ? highScore_ : cand;
        return !currentIsInt ? highScore_ : cand;
      } // candidateHighScore

      private int themeRisks(IList<Guid> atRisk) {
        int count = 0;
        foreach (var cat  in cats_)
          foreach (var risk in cat.Risks.Where(r => atRisk.Contains(r.Id))) 
            ++count;
        return count;
      } // themeRisks

      private string calculateScore(IList<Guid> atRisk) {
        var isLetter = isLetterCategory();
        var letter = "D";
        var count = 0;

        foreach (var cat  in cats_) 
          foreach (var risk in cat.Risks.Where(r => atRisk.Contains(r.Id))) {
            if (isLetter) {
              if (String.Compare(risk.Score, letter) < 0) 
                letter = risk.Score;
            } else { 
              int score;
              if (int.TryParse(risk.Score, out score))
                count += score;
            }
          } // foreach

        return isLetter ? letter : count.ToString();
      } // calculateScore

      private bool isLetterCategory() {
        var risk = cats_[0].Risks[0];
        int score;
        return !(int.TryParse(risk.Score, out score));
      }
    } // ThemeAssessment

    public class CategoryAssessment {
      private string title_;
      private IList<CategoryRisk> risks_;

      public CategoryAssessment(
          string title,
          IList<CategoryRisk> risks) {
        title_ = title;
        risks_ = risks;
      } // CategoryAssessment

      public string Title { get { return title_; } }
      public IList<CategoryRisk> Risks { get { return risks_; } }
    } // CategoryAssessment

    public class CategoryRisk {
      private Risk risk_;
      private string status_;

      public CategoryRisk(
          Risk risk,
          string status) { 
        risk_ = risk;
        status_ = status;
      } // CategoryRisk

      public Guid Id { get { return risk_.Id; } }
      public string Title { get { return risk_.Title; } }
      public string Score { get { return risk_.Score; } }
      public string Guidance { get { return risk_.Guidance; } }
      public string Status { get { return status_; } }
    } // CategoryRisk
  } // RiskAssessment

  ///////////////////////////////////////////////////
  public class ClientUpdate {
    private ClientData client_;
    public ClientUpdate() {
      client_ = new ClientData();
      client_.Address = new AddressData();
      client_.Demographics = new DemographicData();
    } // ClientUpdate

    public Guid Id { get { return client_.Id; } set { client_.Id = value; } }
    public string Name { get { return client_.Name; } set { client_.Name = value; } }
    public string ReferenceId { get { return client_.ReferenceId; } set { client_.ReferenceId = value; } }
    public AddressData Address { get { return client_.Address; } }
    public DemographicData Demographics { get { return client_.Demographics; } }
    public Guid ProjectId { get { return client_.ProjectId; } set { client_.ProjectId = value; } }
    public Guid LocationId { get { return client_.LocationId; } set { client_.LocationId = value; } }

    public ClientData clientData() { return client_; }
  } // ClientUpdate

  public class NewClient : ClientUpdate {
    public String Referrer { get; set; }
  }

  public class NewNote {
    public static NewNote ClientCreated() {
      return new NewNote(NoteType.Registered, "Client registered");
    } // ClientCreated
    public static NewNote Event(string description) {
      return new NewNote(NoteType.Event, description);
    } // Event
    public static NewNote Referral(string description) {
      return new NewNote(NoteType.Referral, description);
    } // Referral
    public static NewNote FileUpload(string description) {
      return new NewNote(NoteType.File, description);
    } // FileUpload
    public static NewNote Log(string description) {
      return new NewNote(NoteType.Log, description);
    } // FileUpload

    private NoteData note_;

    public NewNote()
      : this(NoteType.Narrative, "") {
    } // NewNote

    private NewNote(NoteType type, string text) {
      note_ = new NoteData() {
        Id = Guid.NewGuid(),
        Type = type,
        Text = text,
        Timestamp = DateTime.Now
      }; // note_
    } // NewNote

    public NewNote(string noteText)  : this() {
      note_.Text = noteText;
    } // NewNote

    public Guid Id { get { return note_.Id; } }
    public NoteType Type { set { note_.Type = value; } }
    public string Text { get { return note_.Text; } set { note_.Text = value; } }

    public NoteData noteData() { return note_; }
  } // NewNote

  public class UpdateRiskAssessment {
    private readonly DateTime datestamp_ = DateTime.Now.Date;
    public Guid Id { get; set; }
    public DateTime Datestamp { get { return datestamp_; } }
  }
} // namespace ...