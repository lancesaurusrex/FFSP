using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WebApplication1.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebApplication1.DAL {

    public class NFLdatabase : DbContext  {
    //Data Developer Center Code First to a New Database
        public NFLdatabase() : base("NFLContext") {}

        public DbSet<NFLPlayer> NFLPlayer { get; set; }
    }

    public class NFLgame : DbContext {

        public NFLgame() : base("GameContext") {}

        //public DbSet<Drives> Drives { get; set; }
        //public DbSet<Plays> Plays { get; set; }
        //public DbSet<Players> Players { get; set; }
        //public DbSet<Start> Start { get; set; }
        //public DbSet<End> End { get; set; }
    }

    public class FF : DbContext {
        public FF() : base("FFContext") { }

        public DbSet<FFTeam> FFTeamDB { get; set; }
        public DbSet<FFGame> FFGameDB { get; set; }
        public DbSet<FFLeague> FFLeagueDB { get; set; }
        //public DbSet<FFPlayer> FFPlayerDB { get; set; }
    }
}