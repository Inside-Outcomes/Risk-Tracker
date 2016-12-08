using System;
using System.Collections.Generic;
using RiskTracker.Providers;
using RiskTracker.Entities;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Data.Entity;

namespace RiskTracker {
  public static class DataConfig {
    public static void InitialData() {
      initialClientAppsList();
      setupAdmins();

      setupRiskMaps();
    } // InitialData

    private static void setupAdmins() {
      using (AuthRepository auth = new AuthRepository()) {
        //auth.AddIfNotPresentAdminUser(sample-username, password);
      } // using ...
    } // setupAdmins

    private static void initialClientAppsList() {
      using (DatabaseContext context = new DatabaseContext()) {
        foreach (ClientApp ca in BuildClientAppsList())
          context.ClientApps.AddOrUpdate(ca);
        context.SaveChanges();
      } // using ...
    } // initialClientAppsList  

    private static List<ClientApp> BuildClientAppsList() {
      List<ClientApp> clients = new List<ClientApp>();

      clients.Add(new ClientApp {
        Id = "ngAuthApp",
        Secret = Helper.GetHash("proodige"),
        Name = "AngularJS Application",
        ApplicationType = Models.ApplicationTypes.JavaScript,
        Active = true,
        RefreshTokenLifeTime = 7200,
        AllowedOrigin = "http://localhost"
      });

      clients.Add(new ClientApp {
        Id = "console",
        Secret = Helper.GetHash("fruityloops"),
        Name = "Console Application",
        ApplicationType = Models.ApplicationTypes.NativeConfidential,
        Active = true,
        RefreshTokenLifeTime = 7200,
        AllowedOrigin = "*"
      });

      return clients;
    } // BuildClientAppsList

    private static void setupRiskMaps() {
      using (DatabaseContext context = new DatabaseContext()) {
        var endOfLife = context.RiskMaps.Where(rm => rm.Name == "Dying Well").SingleOrDefault();
        if (endOfLife != null) {
          endOfLife.Name = "End of Life";
          context.SaveChanges();
        } // if ...
        
        var risks = gatherRisks(context);

        foreach (RiskMap riskMap in buildRiskMaps(risks)) {
          var existing = context.RiskMaps.Where(rm => rm.Name == riskMap.Name).SingleOrDefault();
          if (existing == null)
            context.RiskMaps.Add(riskMap);
        }
            
        updateRisksWithOutcomeFrameworks(risks);

        context.SaveChanges();
      } // using ...
    } // setupRiskMaps

    private static List<RiskMap> gatherRiskMaps(DatabaseContext context) {
      return context.RiskMaps.ToList();
    }  // gatherRiskMaps

    private static IDictionary<string, Risk> gatherRisks(DatabaseContext context) {
      var risks = new Dictionary<string, Risk>();

      // Personal circs
      risks.Add(Disclosed_domestic_violence_and_abuse, 
                new Risk {
        Id = Guid.NewGuid(),
        Title = Disclosed_domestic_violence_and_abuse,
        Score = "A",
        Theme = Personal_circumstances,
        Category = Type_domestic_abuse,
        Guidance = "Client who self-reports that they are or have been subject to domestic violence and abuse"
      });
      risks.Add(Housing_homeless,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Housing_homeless,
        Score = "A",
        Theme = Personal_circumstances,
        Category = Type_accomodation,
        Guidance = "The client has nowhere to live: someone is not homeless if they are in temporary accomodation",
        Grouping = "housing"
      });
      risks.Add(Housing_temporary_accomodation,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Housing_temporary_accomodation,
        Score = "B",
        Theme = Personal_circumstances,
        Category = Type_accomodation,
        Guidance = "The client is living in temporary accomodation, this includes hostel accomodation, sofa surfing, or short-term arrangements outside the family",
        Grouping = "housing"
      });
      risks.Add(Housing_unsuitable_housing,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Housing_unsuitable_housing,
        Score = "C",
        Theme = Personal_circumstances,
        Category = Type_accomodation,
        Guidance = "They client feels they are living in unsuitable accomodation, this might be because it is unsafe, unsanitary, overcrowded, or in disrepair",
        Grouping = "housing"
      });
      risks.Add(Financial_hardship,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Financial_hardship,
                  Score = "B",
                  Theme = Personal_circumstances,
                  Category = "Financial hardship",
                  Guidance = "Client who self-discloses that they have unmanaged debt, rent arrears, or low income"
                });
      risks.Add(Vulnerable_adult,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Vulnerable_adult,
        Score = "A",
        Theme = Personal_circumstances,
        Category = "Safeguarding",
        Guidance = "The client meets the criteria for a vulnerable adult"
      });
      risks.Add(Safeguarded_child,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Safeguarded_child,
                  Score = "A",
                  Theme = Personal_circumstances,
                  Category = "Safeguarding",
                  Guidance = "The client is at risk or classified as in need by Social Care and Health"
                });
      risks.Add(Recent_arrival_to_the_uk,
        new Risk {
          Id = Guid.NewGuid(),
          Title = Recent_arrival_to_the_uk,
          Score = "A",
          Theme = Personal_circumstances,
          Category = "",
          Guidance = "Client has moved to the UK within the last 12 months"
        });
      risks.Add(Social_isolation,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Social_isolation,
        Score = "B",
        Theme = Personal_circumstances,
        Category = "Social isolation",
        Guidance = "The client has no support from partner, family, or friend"
      });
      risks.Add(Unsupported_teen,
        new Risk {
          Id = Guid.NewGuid(),
          Title = Unsupported_teen,
          Score = "B",
          Theme = Personal_circumstances,
          Category = "family"
        });
      risks.Add(Meets_troubled_family_criteria,
        new Risk {
          Id = Guid.NewGuid(),
          Title = Meets_troubled_family_criteria,
          Score = "A",
          Theme = Personal_circumstances,
          Category = Type_family,
          Guidance = "Client who lives in a household where a Troubled Family has been identified."
        });
      risks.Add(Lack_of_affordable_childcare,
        new Risk {
          Id = Guid.NewGuid(),
          Title = Lack_of_affordable_childcare,
          Score = "C",
          Theme = Personal_circumstances,
          Category = "Financial hardship",
          Guidance = "Client for whom the cost of childcare exceeds the financial benefit of working and/or training"
        });
      risks.Add(Caring_responsibilities,
        new Risk {
          Id = Guid.NewGuid(),
          Title = Caring_responsibilities,
          Score = "C",
          Theme = Personal_circumstances,
          Category = Type_family,
          Guidance = "Client who is a carer and the cost of replacement or respite care exceeds the financial benefit of working and/or training"
        });
      risks.Add(Limited_transport_options,
        new Risk {
          Id = Guid.NewGuid(),
          Title = Limited_transport_options,
          Score = "C",
          Theme = Personal_circumstances,
          Category = "mobility",
          Guidance = "Client with limited access to affordable transport, public and/or private, such that their travel costs exceed the financial benefit of working and/or training"
        });
      risks.Add(Outdoor_spaces,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Outdoor_spaces,
        Score = "C",
        Theme = Personal_circumstances,
        Category = Type_environment,
        Guidance = "The client reports that they have not taken a visit to the natural environment for health or exercise over the previous seven days"
      });
      risks.Add(Noise, 
                new Risk {
        Id = Guid.NewGuid(),
        Title = Noise,
        Score = "C",
        Theme = Personal_circumstances,
        Category = Type_environment,
        Guidance = "The client reports that excessive noise is having an adverse effect on their health. 'Noise' includes environmental, neighbour, and neighbourhood"
      });
      risks.Add(Environment_community_safety,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Environment_community_safety,
                  Score = "C",
                  Theme = Personal_circumstances,
                  Category = Type_environment,
                  Guidance = "The client reports that they do not feel safe in their own homes or their community"
                });
      risks.Add(Young_carer,
            new Risk {
              Id = Guid.NewGuid(),
              Title = Young_carer,
              Score = "B",
              Theme = Personal_circumstances,
              Category = Type_family,
              Guidance = "Someone aged 18 or under who helps look after a relative who has a condition - such as a disability, illness, mental health condition or a drug/alcohol problem"
            });
      risks.Add(Looked_after_child_young_person,
            new Risk {
              Id = Guid.NewGuid(),
              Title = Looked_after_child_young_person,
              Score = "A",
              Theme = Personal_circumstances,
              Category = Type_family,
              Guidance = "Children who are in local authority residential care, foster care, residential school or secure unit or with parents supervised by social workers"
            });
      risks.Add(Late_diagnosis,
            new Risk {
              Id = Guid.NewGuid(),
              Title = Late_diagnosis,
              Score = "C",
              Theme = Personal_circumstances,
              Category = "health",
              Guidance = "People at the end of life who were not diagnosed in a timely way"
            });

      // behaviour 
      risks.Add(Dietary_deficiencies,
                 new Risk {
                   Id = Guid.NewGuid(),
                   Title = Dietary_deficiencies,
                   Score = "1",
                   Theme = Behaviour,
                   Category = "Nutrition",
                   Guidance = "Not meeting the recommended guidelines for nutionally balanced diet"
                 });
      risks.Add(Smoking,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Smoking,
                  Score = "2",
                  Theme = Behaviour,
                  Category = "Smoking",
                  Guidance = "The client smoked cigarettes in the last week, regardless of number per day"
                });
      risks.Add(Alcohol,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Alcohol,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Alcohol",
                  Guidance = "The client is drinking more than the recommended units of alcohol per week on average: 14 for women, 21 for men, 4 for pregnant women"
                });
      risks.Add(Substance_misuse,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Substance_misuse,
                  Score = "2",
                  Theme = Behaviour,
                  Category = "Substance misuse",
                  Guidance = "The client regularly uses intoxicants (excluding alcohol) to an extent where physical dependence or harm is a risk"
                });
      risks.Add(Difficulty_speaking_english,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Difficulty_speaking_english,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Language",
                  Guidance = "Client reports that they have difficulty speaking, reading, writing or understanding English"
                });
      risks.Add(Does_not_intend_to_breastfeed,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Does_not_intend_to_breastfeed,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Lifestyle",
                  Guidance = "Client reports that they do not intend to breastfeed"
                });
      risks.Add(Not_engaged_in_a_work_focussed_activity,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Not_engaged_in_a_work_focussed_activity,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Employability",
                  Guidance = "Client who is not currently engaged in a work focused activity",
                  Grouping = "employability"
                });
      risks.Add(Not_in_education_employment_or_training,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Not_in_education_employment_or_training,
                  Score = "2",
                  Theme = Behaviour,
                  Category = "Employability",
                  Guidance = "Client between the ages of 16 - 24 who is not in education, employment or training",
                  Grouping = "employability"
                });
      risks.Add(Low_confidence_and_self_esteem,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Low_confidence_and_self_esteem,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Employability",
                  Guidance = "Client who reports that they lack confidence to undertake training and gain employment"
                });
      risks.Add(Poor_work_ethic,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Poor_work_ethic,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Employability",
                  Guidance = "Client who has been sanctioned by the Job Centre or had disciplinary (informal or formal) issues within the last three months"
                });
      risks.Add(Low_fruit_and_veg,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Low_fruit_and_veg,
        Score = "1",
        Theme = Behaviour,
        Category = "Nutrition",
        Guidance = "The client reports that yesterday they consumed 3-4 portions of fruit or vegetables",
        Grouping = "veg"
      });
      risks.Add(Very_low_fruit_and_veg,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Very_low_fruit_and_veg,
        Score = "2",
        Theme = Behaviour,
        Category = "Nutrition",
        Guidance = "The client reports that yesterday they consumed 0-2 portions of fruit or vegetables",
        Grouping = "veg"
      });
      risks.Add(Significant_fried_and_processed_food,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Significant_fried_and_processed_food,
        Score = "1",
        Theme = Behaviour,
        Category = "Nutrition",
        Guidance = "The client reports that fried and processed food formed a significant part of their diet in the last week"
      });
      risks.Add(Excessive_sugar,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Excessive_sugar,
        Score = "1",
        Theme = Behaviour,
        Category = "Nutrition",
        Guidance = "The client reports they have consumed in excess of the recommended daily intake of sugar, on average, in the last week"
      });
      risks.Add(Iron,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Iron,
        Score = "1",
        Theme = Behaviour,
        Category = "Nutrition",
        Guidance = "It has been identified that the client has an iron deficiency"
      });
      risks.Add(Moderately_physically_active,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Moderately_physically_active,
        Score = "1",
        Theme = Behaviour,
        Category = "Exercise",
        Guidance = "The client is doing more than 30 minutes but less than 150 minutes of moderate intensity physical activity per week",
        Grouping = "exercise"
      });
      risks.Add(Physically_inactive,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Physically_inactive,
        Score = "2",
        Theme = Behaviour,
        Category = "Exercise",
        Guidance = "The client is doing less than 30 minutes of moderately intense physical activity per week in bouts of 10 minutes or more",
        Grouping = "exercise"
      });
      risks.Add(Requires_help_with_control_over_daily_life,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Requires_help_with_control_over_daily_life,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Independence",
                  Guidance = "The client reports that they have some control over their daily lives but not enough"
                });
      risks.Add(Poor_management_of_long_term_conditions,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Poor_management_of_long_term_conditions,
                  Score = "2",
                  Theme = Behaviour,
                  Category = "Independence",
                  Guidance = "The client feels unsupported or has had 2 or more unplanned admissions to hospital due to LTC"
                });
      risks.Add(Risk_of_falls,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Risk_of_falls,
                  Score = "2",
                  Theme = Behaviour,
                  Category = "Independence",
                  Guidance = "Two or more falls risk factors are present"
                });
      risks.Add(Not_had_sight_test,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Not_had_sight_test,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Independence",
                  Guidance = "Two or more falls risk factors are present"
                });
      risks.Add(Immunised_or_vaccinated,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Immunised_or_vaccinated,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Immunisations",
                  Guidance = "Children aged 5 and 13 who have received the recommended screening programmes"
                });
      risks.Add(Truancy_school_attendance,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Truancy_school_attendance,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "Truancy",
                  Guidance = "Children aged 5 -16 who have been excluded or identified as persistently truanting"
                });
      risks.Add(Excessive_screen_time,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Excessive_screen_time,
                  Score = "1",
                  Theme = Behaviour,
                  Category = "screen time",
                  Guidance = "Children aged 10 - 16 who report spending in excess of 2 hours of screen time in a day"
                });
      risks.Add(Hospital_admissions,
                      new Risk {
                        Id = Guid.NewGuid(),
                        Title = Hospital_admissions,
                        Score = "2",
                        Theme = Behaviour,
                        Category = "Hospital Admission",
                        Guidance = "Children aged 5 -16 who were admitted to hospital with unintentional or deliberate injuries"
                      });

      // status 
      risks.Add(Obese,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Obese,
                  Score = "2",
                  Theme = "Status",
                  Category = "Weight",
                  Guidance = "The client has a BMI over 30, with the exception of South Asian and Chinese clients where a BMI over 27.5 indicates obesity",
                  Grouping = "weight"
                });
      risks.Add(Underweight,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Underweight,
                  Score = "1",
                  Theme = "Status",
                  Category = "Weight",
                  Guidance = "Client has a BMI of 18 or less",
                  Grouping = "weight"
                });
      risks.Add(Low_wellbeing,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Low_wellbeing,
                  Score = "1",
                  Theme = "Status",
                  Category = "Mental health",
                  Guidance = "The client has a low sense of wellbeing, as indicated by their result on either WEMWBS or the ONS Measuring National Well-being Programme"
                });
      risks.Add(Stress_and_Anxiety,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Stress_and_Anxiety,
                  Score = "1",
                  Theme = "Status",
                  Category = "Mental health",
                  Guidance = "The client reports excessive worry about a number of different events associated with heightened tension. These have been present for at least 6 months and have significant impact on their lives"
                });
      risks.Add(Diagnosed_mental_health_condition,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Diagnosed_mental_health_condition,
                  Score = "2",
                  Theme = "Status",
                  Category = "Mental health",
                  Guidance = "Client has a diagnosed mental health condition "
                });
      risks.Add(Missed_antenatal_appointments,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Missed_antenatal_appointments,
                  Score = "1",
                  Theme = "Status",
                  Category = "Antenatal",
                  Guidance = "Client has missed two or more ante natal appointments"
                });
      risks.Add(Overweight,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Overweight,
        Score = "1",
        Theme = "Status",
        Category = "Weight",
        Guidance = "The client has a BMI of 25-30, with the exception of South Asian and Chinese clients where a BMI of 23-27.5 indicates overweight",
        Grouping = "weight"
      });
      risks.Add(Limited_it_skills,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Limited_it_skills,
            Score = "1",
            Theme = "Status",
            Category = "Employability",
            Guidance = "Client who reports that they have not independently looked for a job and submitted a job application online"
          });
      risks.Add(Recently_unemployed,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Recently_unemployed,
            Score = "1",
            Theme = "Status",
            Category = "Employment",
            Guidance = "Client has become unemployed within the previous six months and is not in receipt of Employment Support Allowance (ESA)",
            Grouping = "unemployment"
          });
      risks.Add(Long_term_unemployed,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Long_term_unemployed,
            Score = "2",
            Theme = "Status",
            Category = "Employment",
            Guidance = "Client has been unemployed for a consecutive period of six months or more and is not in receipt of Employment Support Allowance (ESA)",
            Grouping = "unemployment"
          });
      risks.Add(Recent_esa,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Recent_esa,
            Score = "1",
            Theme = "Status",
            Category = "Benefits",
            Guidance = "Client has started to receive Employment Support Allowance (ESA) within the previous six months",
            Grouping = "esa"
          });
      risks.Add(Long_term_esa,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Long_term_esa,
            Score = "2",
            Theme = "Status",
            Category = "Benefits",
            Guidance = "Client in receipt of Employment Support Allowance (ESA) for more than six months",
            Grouping = "esa"
          });
      risks.Add(No_qualifications,
          new Risk {
            Id = Guid.NewGuid(),
            Title = No_qualifications,
            Score = "3",
            Theme = "Status",
            Category = "Qualifications",
            Guidance = "Client who does not hold a Regulated Qualification that is recognised by Ofqual",
            Grouping = "qualifications"
          });
      risks.Add(Quals_entry_level,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Quals_entry_level,
            Score = "2",
            Theme = "Status",
            Category = "Qualifications",
            Guidance = "Client whose highest qualification is a Regulated Qualification at Entry Level, as recognised by Ofqual",
            Grouping = "qualifications"
          });
      risks.Add(Quals_level_one,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Quals_level_one,
            Score = "1",
            Theme = "Status",
            Category = "Qualifications",
            Guidance = "Client whose highest qualification is a Regulated Qualification at Level One, as recognised by Ofqual",
            Grouping = "qualifications"
          });
      risks.Add(Funcs_level_two,
          new Risk {
            Id = Guid.NewGuid(),
            Title = Funcs_level_two,
            Score = "1",
            Theme = "Status",
            Category = "Qualifications",
            Guidance = "Client whose highest Functional Skills Level is Two"
          });
      risks.Add(Unwanted_pregnancy,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Unwanted_pregnancy,
        Score = "1",
        Theme = "Status",
        Category = "Sexual health",
        Guidance = "The client is a woman who reports that their pregnancy is unwanted"
      });
      risks.Add(STI, 
                new Risk {
        Id = Guid.NewGuid(),
        Title = STI,
        Score = "1",
        Theme = "Status",
        Category = "Sexual health",
        Guidance = "The client is identified as having a sexually transmitted infection"
      });
      risks.Add(Pre_diabetic,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Pre_diabetic,
        Score = "2",
        Theme = "Status",
        Category = "Health check",
        Guidance = "The client has been identified as being pre-diabetic, ie they have glucose tolerance level of 7-9 after food"
      });
      risks.Add(Raised_blood_pressure,
                new Risk {
        Id = Guid.NewGuid(),
        Title = Raised_blood_pressure,
        Score = "1",
        Theme = "Status",
        Category = "Health check",
        Guidance = "The client has a blood pressure reading of between 120/80 and 140/90",
        Grouping = "bp"
      });
      risks.Add(High_blood_pressure,
                new Risk {
        Id = Guid.NewGuid(),
        Title = High_blood_pressure,
        Score = "2",
        Theme = "Status",
        Category = "Health check",
        Guidance = "The client has a blood pressure reading over 140/90",
        Grouping = "bp"
      });
      risks.Add(Frail,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Frail,
                  Score = "1",
                  Theme = "Status",
                  Category = "Independence",
                  Guidance = "The client is assessed with mild to severe frailty",
                });
      risks.Add(Excessive_weight_in_children,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Excessive_weight_in_children,
                  Score = "2",
                  Theme = "Status",
                  Category = "Weight",
                  Guidance = "Children classed as obese or overweight",
                });
      risks.Add(Low_wellbeing_children,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Low_wellbeing_children,
                  Score = "1",
                  Theme = "Status",
                  Category = "mental health",
                  Guidance = "Children aged 10 - 15 who reported low levels on 3 measures in the ONS Children and Young People Well-being tool",
                });
      risks.Add(Eating_disorder,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Eating_disorder,
                  Score = "2",
                  Theme = "Status",
                  Category = "mental health",
                  Guidance = "Children over the age of 8 who are diagnosed with anorexia, bulimia or binge eating",
                });
      risks.Add(Tooth_decay,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Tooth_decay,
                  Score = "1",
                  Theme = "Status",
                  Category = "general health",
                  Guidance = "Children aged 5 years and over who have decayed, missing or filled teeth",
                });
      risks.Add(School_readiness,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = School_readiness,
                  Score = "1",
                  Theme = "Status",
                  Category = "education",
                  Guidance = "Children at the end of Reception Year who have not reached their early learning goals",
                });
      risks.Add(First_time_entrant_justice_system,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = First_time_entrant_justice_system,
                  Score = "2",
                  Theme = "Status",
                  Category = "criminal activity",
                  Guidance = "10 - 17 year olds receiving their first reprimand, warning, youth caution or conviction",
                });
      risks.Add(Teenage_pregnancy,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Teenage_pregnancy,
                  Score = "2",
                  Theme = "Status",
                  Category = "sexual health",
                  Guidance = "Conceptions to young women aged 15 - 17",
                });
      risks.Add(Chlamydia,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Chlamydia,
                  Score = "1",
                  Theme = "Status",
                  Category = "sexual health",
                  Guidance = "Young people aged 15 - 16 who are diagnosed with chlamydia through screening",
                });
      risks.Add(Care_plan,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Care_plan,
                  Score = "2",
                  Theme = "Status",
                  Category = "independence",
                  Guidance = "Clients with a completed and up-to-date personalised care plan",
                });
      risks.Add(Dementia,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Dementia,
                  Score = "2",
                  Theme = "Status",
                  Category = "mental health",
                  Guidance = "Clients with dementia",
                });
      risks.Add(Poor_management_of_pain,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Poor_management_of_pain,
                  Score = "2",
                  Theme = "Status",
                  Category = "health",
                  Guidance = "People approaching the end of life who self declare that their pain is not being managed effectively",
                });
      risks.Add(Religious_and_spiritual_needs_not_met,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Religious_and_spiritual_needs_not_met,
                  Score = "1",
                  Theme = "Status",
                  Category = "health",
                  Guidance = "People who self-report that they have not been offered spiritual and religious support appropriate to their needs and preferences",
                });
      risks.Add(Not_treated_in_preferred_place_or_care,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Not_treated_in_preferred_place_or_care,
                  Score = "1",
                  Theme = "Status",
                  Category = "independence",
                  Guidance = "Clients who are not being treated in their preferred place of care",
                });
      risks.Add(Death_in_usual_place_of_residence,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Death_in_usual_place_of_residence,
                  Score = "1",
                  Theme = "Status",
                  Category = "independence",
                  Guidance = "People who do not die in their usual place of residence",
                });
      risks.Add(Blood_sugar_levels,
                new Risk {
                  Id = Guid.NewGuid(),
                  Title = Blood_sugar_levels,
                  Score = "2",
                  Theme = "Status",
                  Category = "health check",
                  Guidance = "The client has an above average HbA1c measurement. Above 41 mmol/mol for non-diabetics or above 48 mmol/mol for people with diabetes",
                  NIHCEG = "Statement 4"
                });

      var existingRisks = new Dictionary<string, Risk>();
      // override with existing risks
      foreach (Risk r in context.Risks)
        existingRisks.Add(r.Title, r);

      foreach (var key in risks.Keys)
        if (!existingRisks.ContainsKey(key)) {
          existingRisks.Add(key, risks[key]);
          context.Risks.Add(risks[key]);
          context.SaveChanges();
        } 

      return existingRisks;
    } // gatherRisks

    private static List<RiskMap> buildRiskMaps(IDictionary<string, Risk> risks) {
      List<RiskMap> riskMaps = new List<RiskMap>();
      riskMaps.Add(createStartingWell(risks));
      riskMaps.Add(createDevelopingWell(risks));
      riskMaps.Add(createWorkingWell(risks));
      riskMaps.Add(createAgeingWell(risks));
      riskMaps.Add(createDyingWell(risks));
      riskMaps.Add(createLivingWell(risks));
      riskMaps.Add(createDiabetes(risks));
      return riskMaps;
    } // buildRiskMaps

    private static void updateRisksWithOutcomeFrameworks(IDictionary<string, Risk> risks) {
      updateRiskWithOutcomeFrameworks(risks, Disclosed_domestic_violence_and_abuse, "PH50  CG110 1.5", "1.11", "", "KI-1", "D4");
      updateRiskWithOutcomeFrameworks(risks, Housing_homeless, "", "1.15i", "", "KI-1", "");
      updateRiskWithOutcomeFrameworks(risks, Housing_temporary_accomodation, "", "1.15i", "", "KI-1", "");
      updateRiskWithOutcomeFrameworks(risks, Housing_unsuitable_housing, "CG161 1.16", "1.17", "", "KI-1", "");
      updateRiskWithOutcomeFrameworks(risks, Financial_hardship, "", "1.1 1.17", "", "KI-4", "");
      updateRiskWithOutcomeFrameworks(risks, Vulnerable_adult, "", "", "", "", "D4");
      //Safeguarded_Child
      //Recent_Arrival_to_the_UK
      updateRiskWithOutcomeFrameworks(risks, Social_isolation, "", "1.18", "Annex H", "", "1I");
      //Unsupported_Teen
      //Meets_the_Troubled_Family_Criteria
      //Dietary_Deficiencies
      updateRiskWithOutcomeFrameworks(risks, Smoking, "CG62 1.3.10  PH14  NICE127", "2.3  2.9  2.14", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Alcohol, "CG110 1.13  CG45 1.3.9  CG100  PH24  CG127 1.4.8", "2.1  2.15", "Annex H", "KI-5", "");
      updateRiskWithOutcomeFrameworks(risks, Substance_misuse, "CH1101.2  PH4", "2.15", "Annex H", "KI-5", "");
      //Difficulty_speaking_English
      //Does_not_intend_to_breastfeed
      updateRiskWithOutcomeFrameworks(risks, Obese, "CG62 1.5.1  CG43 1.2.2.7", "2.12", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Overweight, "CG43 1.2.2.7", "2.12", "", "", "");
      //Underweight
      updateRiskWithOutcomeFrameworks(risks, Low_wellbeing, "", "2.23 - 2.25", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Stress_and_Anxiety, "CG45 1.3", "", "", "", "");
      //Diagnosed_mental_health_condition
      //Missed_antenatal_appointments
      //Lack_of_access_to_affordable_childcare
      //Caring_Responsibilities
      //Limited_transport_options
      //Not_Engaged_in_a_Work_Focused_Activity
      //Not_in_Education_Employment_or_Training
      //Low_confidence_and_self_esteem
      //Limited_IT_skills
      //Demonstrating_poor_work_ethic_in_past_three_months
      //Recently_unemployed
      //Long-term_unemployed
      //Recent_recipient_of_Employment_Support_Allowance
      //Long_term_recipient_of_Employment_Support_Allowance
      //No_qualifications
      //Highest_qualification_Entry_Level
      //Highest_qualification_Level_One
      //Highest_functional_skills_level_Two
      updateRiskWithOutcomeFrameworks(risks, Outdoor_spaces, "", "1.16", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Noise, "", "1.14", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Low_fruit_and_veg, "CG127 1.4.4  CG43 1.1.1.2", "2.11", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Very_low_fruit_and_veg, "CG127 1.4.4  CG43 1.1.1.2", "2.11", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Significant_fried_and_processed_food, "PH25", "2.8", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Excessive_sugar, "CG43 1.1.4.1", "2.11", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Iron, "PH25", "2.11", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Moderately_physically_active, "CG43 1.2.4.18", "2.13", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Physically_inactive, "PH17  CG43 1.2.4.18", "2.13", "Annex H", "", "");
      updateRiskWithOutcomeFrameworks(risks, Unwanted_pregnancy, "", "", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, STI, "", "3.02.ii", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Pre_diabetic, "PH38", "2.17", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, Raised_blood_pressure, "PH25", "2.11", "", "", "");
      updateRiskWithOutcomeFrameworks(risks, High_blood_pressure, "CG127 1.2", "", "", "", "");
      //Environment_Community_Safety
      //Requires_help_with_control_over_daily_life
      //Poor_management_of_long_term_conditions
      //Risk_of_falls
      //Not_had_a_sight_test
      //Frail
      //Young Carer
      //Looked_after_child_young_person
      //Excess_weight_in_children
      //Low_wellbeing_in_children
      //Eating_disorder
      //Tooth_decay
      //School_readiness
      //First_time_entrant_to_justice_system
      //Teenage_pregnancy
      //Chlamydia
      //Immunised_or_vaccinated
      //Truancy/school_attendance
      //Excessive_screen_time
      //Hospital_admissions
      //Care_plan
      //Late_diagnosis
      //Dementia
      //Poor_management_of_pain
      //Religious_and_Spiritual_needs_not_met
      //Not_treated_in_preferred_place_of_care
      //Death_in_usual_place_of_residence
    } // updateRisksWithOutcomeFrameworks

    private static void updateRiskWithOutcomeFrameworks(IDictionary<string, Risk> risks, 
                                                        string title, 
                                                        string NIHCEG, 
                                                        string IOST, 
                                                        string HCP,
                                                        string SJOF,
                                                        string ASCOF) {
      var risk = risks[title];
      risk.NIHCEG = NIHCEG;
      risk.IOST = IOST;
      risk.HCP = HCP;
      risk.SJOF = SJOF;
      risk.ASCOF = ASCOF;
    } // 

    private static RiskMap createLivingWell(IDictionary<string, Risk> risks) { 
      string[] livingWellRisks = { 
        Disclosed_domestic_violence_and_abuse,
        Housing_homeless,
        Housing_temporary_accomodation,
        Housing_unsuitable_housing, 
        Vulnerable_adult,
        Financial_hardship,
        Social_isolation,
        Outdoor_spaces,
        Noise,
        Low_fruit_and_veg,
        Very_low_fruit_and_veg,
        Significant_fried_and_processed_food,
        Excessive_sugar,
        Iron,
        Moderately_physically_active,
        Physically_inactive,
        Alcohol,
        Smoking,
        Substance_misuse,
        Overweight,
        Obese,
        Low_wellbeing,
        Stress_and_Anxiety,
        Unwanted_pregnancy,
        STI,
        Pre_diabetic,
        Raised_blood_pressure,
        High_blood_pressure
      };

      var riskList = new List<Risk>();
      foreach (var t in livingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("Living Well", riskList);
    } // createLivingWell

    private static RiskMap createStartingWell(IDictionary<string, Risk> risks) {
      string[] startingWellRisks = {
        Dietary_deficiencies,
        Smoking,
        Alcohol,
        Substance_misuse,
        Difficulty_speaking_english,
        Does_not_intend_to_breastfeed,
        Obese,
        Underweight,
        Low_wellbeing,
        Stress_and_Anxiety,
        Diagnosed_mental_health_condition,
        Missed_antenatal_appointments,
        Disclosed_domestic_violence_and_abuse,
        Housing_homeless,
        Housing_temporary_accomodation,
        Housing_unsuitable_housing,
        Financial_hardship,
        Vulnerable_adult,
        Safeguarded_child,
        Recent_arrival_to_the_uk,
        Social_isolation,
        Unsupported_teen,
        Meets_troubled_family_criteria,
        Teenage_pregnancy        
      };

      var riskList = new List<Risk>();
      foreach (var t in startingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("Starting Well", riskList);
    }

    private static RiskMap createDevelopingWell(IDictionary<string, Risk> risks) {
      string[] developingWellRisks = {
        Dietary_deficiencies,
        Smoking,
        Alcohol,
        Substance_misuse,
        Low_wellbeing,
        Disclosed_domestic_violence_and_abuse,
        Housing_homeless,
        Housing_temporary_accomodation,
        Housing_unsuitable_housing,
        Vulnerable_adult,
        Safeguarded_child,
        Recent_arrival_to_the_uk,
        Social_isolation,
        Unsupported_teen,
        Meets_troubled_family_criteria,
        Teenage_pregnancy,
        Significant_fried_and_processed_food,
        Physically_inactive,
        Immunised_or_vaccinated,
        Truancy_school_attendance,
        Excessive_screen_time,
        Hospital_admissions,
        Caring_responsibilities,
        Limited_transport_options,
        Outdoor_spaces,
        Noise,
        Environment_community_safety,
        Young_carer,
        Looked_after_child_young_person,
        Excessive_weight_in_children,
        Low_wellbeing_children,
        Eating_disorder,
        Tooth_decay,
        School_readiness,
        First_time_entrant_justice_system,
        Chlamydia
      };

      var riskList = new List<Risk>();
      foreach (var t in developingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("Developing Well", riskList);
    }

    private static RiskMap createWorkingWell(IDictionary<string, Risk> risks) {
      string[] workingWellRisks = {
        Alcohol,
        Substance_misuse,
        Disclosed_domestic_violence_and_abuse,
        Housing_homeless,
        Housing_temporary_accomodation,
        Housing_unsuitable_housing,
        Vulnerable_adult,
        Social_isolation,
        Meets_troubled_family_criteria,
        Caring_responsibilities,
        Limited_transport_options,
        Difficulty_speaking_english,
        Financial_hardship,
        Not_engaged_in_a_work_focussed_activity,
        Not_in_education_employment_or_training,
        Low_confidence_and_self_esteem,
        Poor_work_ethic,
        Lack_of_affordable_childcare,
        Limited_it_skills,
        Recently_unemployed,
        Long_term_unemployed,
        Recent_esa,
        Long_term_esa,
        No_qualifications,
        Quals_entry_level,
        Quals_level_one,
        Funcs_level_two
      };

      var riskList = new List<Risk>();
      foreach (var t in workingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("Working Well", riskList);
    }

    private static RiskMap createAgeingWell(IDictionary<string, Risk> risks) {
      string[] ageingWellRisks = {
        Alcohol,
        Substance_misuse,
        Disclosed_domestic_violence_and_abuse,
        Housing_homeless,
        Housing_temporary_accomodation,
        Housing_unsuitable_housing,
        Vulnerable_adult,
        Social_isolation,
        Caring_responsibilities,
        Financial_hardship,
        Limited_it_skills,
        Smoking,
        Low_wellbeing,
        Physically_inactive,
        Outdoor_spaces,
        Noise,
        Environment_community_safety,
        Obese,
        Underweight,
        Moderately_physically_active,
        Requires_help_with_control_over_daily_life,
        Poor_management_of_long_term_conditions,
        Risk_of_falls,
        Not_had_sight_test,
        Overweight,
        Raised_blood_pressure,
        High_blood_pressure,
        Frail
      };

      var riskList = new List<Risk>();
      foreach (var t in ageingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("Ageing Well", riskList);
    }

    private static RiskMap createDyingWell(IDictionary<string, Risk> risks) {
      string[] dyingWellRisks = {
        Alcohol,
        Substance_misuse,
        Disclosed_domestic_violence_and_abuse,
        Housing_homeless,
        Housing_temporary_accomodation,
        Housing_unsuitable_housing,
        Vulnerable_adult,
        Social_isolation,
        Caring_responsibilities,
        Financial_hardship,
        Limited_it_skills,
        Smoking,
        Low_wellbeing,
        Physically_inactive,
        Outdoor_spaces,
        Noise,
        Environment_community_safety,
        Obese,
        Underweight,
        Moderately_physically_active,
        Requires_help_with_control_over_daily_life,
        Poor_management_of_long_term_conditions,
        Risk_of_falls,
        Not_had_sight_test,
        Overweight,
        Raised_blood_pressure,
        High_blood_pressure,
        Frail,
        Care_plan,
        Late_diagnosis,
        Dementia,
        Poor_management_of_pain,
        Religious_and_spiritual_needs_not_met,
        Not_treated_in_preferred_place_or_care,
        Death_in_usual_place_of_residence
      };

      var riskList = new List<Risk>();
      foreach (var t in dyingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("End of Life", riskList);
    }

    private static RiskMap createDiabetes(IDictionary<string, Risk> risks) {
      string[] dyingWellRisks = {
          Disclosed_domestic_violence_and_abuse,
          Housing_homeless,
          Housing_temporary_accomodation,
          Housing_unsuitable_housing,
          Financial_hardship,
          Vulnerable_adult,
          Recent_arrival_to_the_uk,
          Social_isolation,
          Smoking,
          Alcohol,
          Substance_misuse,
          Obese,
          Overweight,
          Low_wellbeing,
          Stress_and_Anxiety,
          Outdoor_spaces,
          Low_fruit_and_veg,
          Very_low_fruit_and_veg,
          Significant_fried_and_processed_food,
          Excessive_sugar,
          Moderately_physically_active,
          Physically_inactive,
          Pre_diabetic,
          Raised_blood_pressure,
          High_blood_pressure,
          Poor_management_of_long_term_conditions,
          Care_plan,
          Blood_sugar_levels
      };

      var riskList = new List<Risk>();
      foreach (var t in dyingWellRisks)
        riskList.Add(risks[t]);

      return RiskMap.create("Diabetes", riskList);
    }

    private const string Disclosed_domestic_violence_and_abuse = "Disclosed domestic violence and abuse";
    private const string Housing_homeless = "Housing - homeless";
    private const string Housing_temporary_accomodation = "Housing - temporary accomodation";
    private const string Housing_unsuitable_housing = "Housing - unsuitable housing";
    private const string Financial_hardship = "Financial hardship";
    private const string Vulnerable_adult = "Vulnerable adult";
    private const string Safeguarded_child = "Safeguarded child";
    private const string Recent_arrival_to_the_uk = "Recent arrival to the UK";
    private const string Social_isolation = "Social isolation";
    private const string Unsupported_teen = "Unsupported teen";
    private const string Meets_troubled_family_criteria = "Meets the troubled family criteria";
    private const string Lack_of_affordable_childcare = "Lack of access to affordable childcare";
    private const string Caring_responsibilities = "Caring responsibilities - lack of access to replacement or respite care";
    private const string Limited_transport_options = "Limited transport options";
    private const string Outdoor_spaces = "Outdoor spaces";
    private const string Noise = "Noise";
    private const string Environment_community_safety = "Environment - Community safety";
    private const string Young_carer = "Young carer";
    private const string Looked_after_child_young_person = "Looked after child/young person";
    private const string Late_diagnosis = "Late diagnosis";

    private const string Dietary_deficiencies = "Dietary deficiencies";
    private const string Smoking = "Smoking";
    private const string Alcohol = "Alcohol";
    private const string Substance_misuse = "Substance misuse";
    private const string Difficulty_speaking_english = "Difficulty speaking English";
    private const string Does_not_intend_to_breastfeed = "Does not intend to breastfeed";
    private const string Not_engaged_in_a_work_focussed_activity = "Not Engaged in a Work Focussed Activity";
    private const string Not_in_education_employment_or_training = "Not in Education, Employment or Training (NEET)";
    private const string Low_confidence_and_self_esteem = "Low confidence and self esteem";
    private const string Poor_work_ethic = "Demonstrating poor work ethic in past three months";
    private const string Low_fruit_and_veg = "Low fruit and veg";
    private const string Very_low_fruit_and_veg = "Very low fruit and veg";
    private const string Significant_fried_and_processed_food = "Significant fried and processed food";
    private const string Excessive_sugar = "Excessive sugar";
    private const string Iron = "Iron";
    private const string Moderately_physically_active = "Moderately physically active";
    private const string Physically_inactive = "Physically inactive";
    private const string Requires_help_with_control_over_daily_life = "Requires help with control over daily life";
    private const string Poor_management_of_long_term_conditions = "Poor management of long term conditions";
    private const string Risk_of_falls = "Risk of falls";
    private const string Not_had_sight_test = "Not had sight test";
    private const string Immunised_or_vaccinated = "Immunised or vaccinated";
    private const string Truancy_school_attendance = "Truancy / school attendance";
    private const string Excessive_screen_time = "Excessive screen time";
    private const string Hospital_admissions = "Hospital Admissions";


    private const string Obese = "Obese";
    private const string Underweight = "Underweight";
    private const string Low_wellbeing = "Low wellbeing";
    private const string Stress_and_Anxiety = "Stress and Anxiety";
    private const string Diagnosed_mental_health_condition = "Diagnosed mental health condition";
    private const string Missed_antenatal_appointments = "Missed antenatal appointments";
    private const string Overweight = "Overweight";
    private const string Limited_it_skills = "Limited IT skills";
    private const string Recently_unemployed = "Recently Unemployed";
    private const string Long_term_unemployed = "Long-Term Unemployed";
    private const string Recent_esa = "Recent recipient of Employment Support Allowance";
    private const string Long_term_esa = "Long-term recipient of Employment Support Allowance";
    private const string No_qualifications = "No qualifications";
    private const string Quals_entry_level = "Highest qualification - Entry Level";
    private const string Quals_level_one = "Highest qualification - Level One";
    private const string Funcs_level_two = "Highest functional skills - Level Two";
    private const string Unwanted_pregnancy = "Unwanted pregnancy";
    private const string STI = "STI";
    private const string Pre_diabetic = "Pre-diabetic";
    private const string Raised_blood_pressure = "Raised blood pressure";
    private const string High_blood_pressure = "High blood pressure";
    private const string Frail = "Frail";
    private const string Excessive_weight_in_children = "Excess weight in children";
    private const string Low_wellbeing_children = "Low wellbeing (Children)";
    private const string Eating_disorder = "Eating disorder";
    private const string Tooth_decay = "Tooth decay";
    private const string School_readiness = "School readiness";
    private const string First_time_entrant_justice_system = "First time entrant to the justice system";
    private const string Teenage_pregnancy = "Teenage preganancy";
    private const string Chlamydia = "Chlamydia";
    private const string Care_plan = "Care plan";
    private const string Dementia = "Dementia";
    private const string Poor_management_of_pain = "Poor Management of Pain";
    private const string Religious_and_spiritual_needs_not_met = "Religious and spiritual needs not met";
    private const string Not_treated_in_preferred_place_or_care = "Not treated in preferred place of care";
    private const string Death_in_usual_place_of_residence = "Death in usual place of residence";
    private const string Blood_sugar_levels = "Blood Sugar Levels";

    private const string Personal_circumstances = "Personal circumstances";
    private const string Behaviour = "Behaviour";
    private const string Status = "Status";

    private const string Type_environment = "Environment";
    private const string Type_domestic_abuse = "Domestic abuse";
    private const string Type_accomodation = "Accomodation";
    private const string Type_family = "family";
  } // DataConfig
} // namespace ...