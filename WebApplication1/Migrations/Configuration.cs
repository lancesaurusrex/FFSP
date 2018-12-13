namespace WebApplication1.Migrations {
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using WebApplication1.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication1.DAL.FF> {
        public Configuration() {
            AutomaticMigrationsEnabled = false;
            ContextKey = "WebApplication1.DAL.FF";
        }

        protected override void Seed(WebApplication1.DAL.FF FFContext) {
            //Add NFLTeams and NFLSchedule to DB.
            //Debug seed if necessary

            /* Running into primary key problems.  
             * update-database once with db.delete uncommented, update-database again with commented*/
            //FFContext.Database.Delete();

            if (System.Diagnostics.Debugger.IsAttached == false) {
                System.Diagnostics.Debugger.Launch();
                System.Diagnostics.Debugger.Break();  //Force Break at this point
            }
            
            FFDBInitHelper a = new FFDBInitHelper();
            /*This code is kept but does nothing other than pull stuff from text, it doesn't get saved in the db aka no call with Add or SaveChanges */
            a.NFLNicknames1();
            a.NFLTeams2();
            a.NFLAbbr3();
            var num = a.NFLGameCount();
            /*This code is kept but does nothing other than pull stuff from text, it doesn't get saved in the db aka no call with Add or SaveChanges */
            
            
            //Only need to run when db.NFLGames is empty
            //for (int i = 1978; i <= 2018; ++i) {
            //    int week = 1;
            //    int st = 1;
            //    while (week != 23) {//18 is post 21 is week before SB(empty), 22 is SB
            //        if (week == 18)
            //             st = 2;
            //        a.PullSchedulesFromNFLcom(i, week, st); //j,k
            //        week++;
                    
            //    }
            //}

            /*This method will be called after migrating to the latest version.

            You can use the DbSet<T>.AddOrUpdate() helper extension method 
            to avoid creating duplicate seed data. E.g.
            
              context.People.AddOrUpdate(
                p => p.FullName,
                new Person { FullName = "Andrew Peters" },
                new Person { FullName = "Brice Lambson" },
                new Person { FullName = "Rowan Miller" }
              );
            

          League Testing
          Due to autoinc on id, this adds a new league with a different id then */
            FFLeague ffl1 = new FFLeague();
            ffl1.FFLeagueID = 2;
            ffl1.FFLeagueName = "Lardo";

            FFContext.FFLeagueDB.Add(ffl1);
            FFContext.SaveChanges();
            //League Testing, addorupdate with same id as created
            FFContext.FFLeagueDB.AddOrUpdate(p => p.FFLeagueName, new FFLeague { FFLeagueName = "Lardo3", NumberOfTeams = 8, PlayoffWeekStart = 14 });
            FFContext.SaveChanges();
            //screwy code ahead that works
            //seeding FFteams with while loop

            FFLeague leag = FFContext.FFLeagueDB.Where(y => y.FFLeagueName == "Lardo3").FirstOrDefault();   //finds league by name

            if (leag != null) {
                int j = 0;
                while (j != 8) {
                    FFContext.FFTeamDB.AddOrUpdate( //adds or updates dbentry by teamName
                        p => p.TeamName, new FFTeam
                        {
                            TeamName = "Team" + j++,
                            FFLeagueID = leag.FFLeagueID,   //found league id
                            UserID = "737a0a07-a158-40de-b6c2-131e36e22038"
                        }
                        );
                    FFContext.SaveChanges();
                }


                //Add Teams to League List
                var TeamsInLeague = (from t in FFContext.FFTeamDB where t.FFLeagueID == leag.FFLeagueID select t).ToList();
                foreach (FFTeam t in TeamsInLeague)
                    leag.Teams.Add(t);
            }
            else
                throw new NullReferenceException("League doesnt exist on Context call LeagueName == Lardo3");


            FFContext.FFLeagueDB.AddOrUpdate(
                p => p.FFLeagueID, new FFLeague { FFLeagueID = 1 }

                );

        }
    }
}
