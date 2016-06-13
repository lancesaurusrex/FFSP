using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WebApplication1.Models;

//getting started with ef using mvc tutorial on asp.net/mvc
//to FULLY delete database go to package console and type sqllocaldb.exe X MSSQLLocalDB, X=stop,delete,start in that order one at a time

namespace WebApplication1.DAL {
    public class NFLdbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<NFLdatabase> {
        protected override void Seed(NFLdatabase NFLcontext) {

            //String FileName = "2015101200_gtd.json";

            //ReadJSONDatafromNFL j = new ReadJSONDatafromNFL();
           // j.DeserializeData(FileName);    
            //will just need players for this project
            //NFLcontext.SaveChanges();
        }
    }

    public class GamedbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<NFLgame> {
        protected override void Seed(NFLgame Gamecontext) {
            //Deal with later
        }
    }

    public class FFdbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<FF> {
        protected override void Seed(FF FFcontext) {

            String FileName = "2015101200_gtd.json";
            string gameID = "2015101200";
            string fileName2= "2015101108_gtd.json";
            string gameID2 = "2015101108";
            ReadJSONDatafromNFL j = new ReadJSONDatafromNFL();
            j.DeserializeData(FileName, gameID);
            j.DeserializeData(fileName2, gameID2);
            //will just need players for this project

            //temp proj
            GSettings g = new GSettings();
            
            g.CurrentWeek = 1;
            FFcontext.Settings.Add(g);

            FFcontext.SaveChanges();
        }
    }
}