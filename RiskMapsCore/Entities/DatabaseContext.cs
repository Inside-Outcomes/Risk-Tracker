using System;
using Microsoft.AspNet.Identity.EntityFramework;
using RiskTracker.Entities;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Text;
using System.Data.Entity.Migrations;

namespace RiskTracker.Entities {
  public class DatabaseContext : IdentityDbContext<IdentityUser> {
    public DatabaseContext() :
        base("AuthContext") {
          Database.SetInitializer<DatabaseContext>(new CreateDatabaseIfNotExists<DatabaseContext>());
    } // DatabaseContext

    public DbSet<ClientApp> ClientApps { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<ClientData> Clients { get; set; }
    public DbSet<NoteData> Notes { get; set; }

    public DbSet<ProjectOrganisationData> ProjectOrganisations { get; set; }
    public DbSet<ProjectData> Projects { get; set; }
    public DbSet<CommissioningOrganisation> Commissioners { get; set; }
    public DbSet<OrganisationData> Organisations { get; set; }
    public DbSet<AddressData> Addresses { get; set; }
    public DbSet<StaffMemberData> StaffMembers { get; set; }
    public DbSet<LocationData> Locations { get; set; }

    public DbSet<RiskMapData> RiskMaps { get; set; }
    public DbSet<Risk> Risks { get; set; }
    public DbSet<OutcomeFramework> OutcomeFrameworks { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
    } // OnModelCreating

    public override int SaveChanges() {
      try {
        return base.SaveChanges();
      } catch (DbEntityValidationException e) {
        var sb = new StringBuilder();

        foreach (var failure in e.EntityValidationErrors) {
          sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
          foreach (var error in failure.ValidationErrors)
            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage)
              .AppendLine();
        }

        throw new DbEntityValidationException(
            "Entity Validation Failed \n" + sb.ToString(),
            e);
      } // catch
    } // SaveChanges
  } // DatabaseContext
} // namespace ...