using System;
using System.Configuration;
using System.Data.Entity.Migrations;

namespace RiskTracker.Migrations {
  public class DatabaseUpdate {
    public static void Apply() {
      if (!bool.Parse(ConfigurationManager.AppSettings["MigrateDatabase"]))      
        return;

      var configuration = new Configuration();
      var migrator = new DbMigrator(configuration);
      migrator.Update();
    } // Apply
  }
}