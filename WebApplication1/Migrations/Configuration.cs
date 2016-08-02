namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
using WebApplication1.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication1.DAL.FF>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "WebApplication1.DAL.FF";
        }

        protected override void Seed(WebApplication1.DAL.FF FFContext)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //League Testing
            //Due to autoinc on id, this adds a new league with a different id then assigned
            FFLeague ffl1 = new FFLeague();
            ffl1.FFLeagueID = 2;
            ffl1.FFLeagueName = "Lardo";
            FFContext.FFLeagueDB.Add(ffl1);
            FFContext.SaveChanges();
            //League Testing, addorupdate with same id as created
            FFContext.FFLeagueDB.AddOrUpdate(p => p.FFLeagueName, new FFLeague{FFLeagueName = "Lardo3"});
            FFContext.SaveChanges();
            //screwy code ahead that works
            //seeding FFteams with while loop
            //TURNED OFF AUTO PRIMKEY IN FantasyFootball
            FFLeague leag = FFContext.FFLeagueDB.Where(a => a.FFLeagueName == "Lardo3").FirstOrDefault();
            int j = 0;
            while (j != 8) {
                FFContext.FFTeamDB.AddOrUpdate(
                    p => p.TeamName, new FFTeam
                    {
                        TeamName = "Team" + j++,
                        FFLeagueID = leag.FFLeagueID,
                        UserID = "737a0a07-a158-40de-b6c2-131e36e22038"
                    }

                    );
                FFContext.SaveChanges();
            }

            

            FFContext.FFLeagueDB.AddOrUpdate(
                p => p.FFLeagueID, new FFLeague { FFLeagueID = 1 }

                );
        }
    }
}
