using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WebApplication1.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebApplication1.DAL {

    public class NFLdatabase : DbContext  {
        public NFLdatabase() : base("NFLContext") {}

        
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
        public FF() : base("FFContext") {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<FF, WebApplication1.Migrations.Configuration>("FFContext"));
        }

        public DbSet<FFTeam> FFTeamDB { get; set; }
        public DbSet<FFGame> FFGameDB { get; set; }
        public DbSet<FFLeague> FFLeagueDB { get; set; }
        public DbSet<TeamNFLPlayer> FFTeamNFLPlayer { get; set; }
        public DbSet<NFLPlayer> NFLPlayer { get; set; }
        public DbSet<GSettings> Settings { get; set; }
        public DbSet<StatsYearWeek> NFLPlayerStats { get; set; }
        public DbSet<NFLGame> NFLGame { get; set; }
        public DbSet<NFLTeam> NFLTeam { get; set; }
    }
}