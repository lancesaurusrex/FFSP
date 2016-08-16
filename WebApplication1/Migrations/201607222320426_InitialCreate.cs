namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FFGames",
                c => new
                    {
                        FFGameID = c.Int(nullable: false, identity: true),
                        Week = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        HScore = c.Decimal(precision: 18, scale: 2),
                        VScore = c.Decimal(precision: 18, scale: 2),
                        HomeTeamID = c.Int(),
                        VisTeamID = c.Int(),
                        FFLeagueID = c.Int(nullable: false),
                        FFTeam_FFTeamID = c.Int(),
                    })
                .PrimaryKey(t => t.FFGameID)
                .ForeignKey("dbo.FFLeagues", t => t.FFLeagueID, cascadeDelete: true)
                .ForeignKey("dbo.FFTeams", t => t.FFTeam_FFTeamID)
                .ForeignKey("dbo.FFTeams", t => t.HomeTeamID)
                .ForeignKey("dbo.FFTeams", t => t.VisTeamID)
                .Index(t => t.HomeTeamID)
                .Index(t => t.VisTeamID)
                .Index(t => t.FFLeagueID)
                .Index(t => t.FFTeam_FFTeamID);
            
            CreateTable(
                "dbo.FFTeams",
                c => new
                    {
                        FFTeamID = c.Int(nullable: false, identity: true),
                        TeamName = c.String(),
                        Win = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Lose = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Tie = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FPTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DivisionID = c.Int(),
                        UserID = c.String(),
                        FFLeagueID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FFTeamID)
                .ForeignKey("dbo.FFLeagues", t => t.FFLeagueID, cascadeDelete: true)
                .Index(t => t.FFLeagueID);
            
            CreateTable(
                "dbo.FFLeagues",
                c => new
                    {
                        FFLeagueID = c.Int(nullable: false, identity: true),
                        FFLeagueName = c.String(),
                        NumberOfTeams = c.Int(nullable: false),
                        NumberOfDivision = c.Int(nullable: false),
                        PlayoffWeekStart = c.Int(nullable: false),
                        QBStart = c.Int(nullable: false),
                        RBStart = c.Int(nullable: false),
                        WRTESame = c.Boolean(nullable: false),
                        WRStart = c.Int(nullable: false),
                        TEStart = c.Int(nullable: false),
                        DEFStart = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FFLeagueID);
            
            CreateTable(
                "dbo.NFLPlayers",
                c => new
                    {
                        id = c.Int(nullable: false),
                        id_nflformat = c.String(),
                        name = c.String(),
                        team = c.String(),
                        pos = c.String(),
                        currentPts = c.Decimal(nullable: false, precision: 18, scale: 2),
                        week = c.Int(),
                        year = c.Int(),
                        isAvailable = c.Boolean(nullable: false),
                        isChecked = c.Boolean(nullable: false),
                        PassingStats_PassAtt = c.Int(),
                        PassingStats_PassCmp = c.Int(),
                        PassingStats_PassYds = c.Int(),
                        PassingStats_PassTds = c.Int(),
                        PassingStats_PassInts = c.Int(),
                        PassingStats_PassTwopta = c.Int(),
                        PassingStats_PassTwoptm = c.Int(),
                        RushingStats_RushAtt = c.Int(),
                        RushingStats_RushYds = c.Int(),
                        RushingStats_RushTds = c.Int(),
                        RushingStats_RushLng = c.Int(),
                        RushingStats_RushLngtd = c.Int(),
                        RushingStats_RushTwopta = c.Int(),
                        RushingStats_RushTwoptm = c.Int(),
                        ReceivingStats_Rec = c.Int(),
                        ReceivingStats_RecYds = c.Int(),
                        ReceivingStats_RecTds = c.Int(),
                        ReceivingStats_RecLng = c.Int(),
                        ReceivingStats_RecLngtd = c.Int(),
                        ReceivingStats_RecTwopta = c.Int(),
                        ReceivingStats_RecTwoptm = c.Int(),
                        ReceivingStats_RecTrg = c.Int(),
                        FumbleStats_Tot = c.Int(),
                        FumbleStats_Rcv = c.Int(),
                        FumbleStats_Trcv = c.Int(),
                        FumbleStats_Yds = c.Int(),
                        FumbleStats_Lost = c.Int(),
                        KickingStats_Fgm = c.Int(),
                        KickingStats_Fga = c.Int(),
                        KickingStats_Fgyds = c.Int(),
                        KickingStats_Totpfg = c.Int(),
                        KickingStats_Xpmade = c.Int(),
                        KickingStats_Xpmissed = c.Int(),
                        KickingStats_Xpa = c.Int(),
                        KickingStats_Xpb = c.Int(),
                        KickingStats_Xptot = c.Int(),
                        FFLeague_FFLeagueID = c.Int(),
                        FFTeam_FFTeamID = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.FFLeagues", t => t.FFLeague_FFLeagueID)
                .ForeignKey("dbo.FFTeams", t => t.FFTeam_FFTeamID)
                .Index(t => t.FFLeague_FFLeagueID)
                .Index(t => t.FFTeam_FFTeamID);
            
            CreateTable(
                "dbo.TeamNFLPlayers",
                c => new
                    {
                        TNPID = c.Int(nullable: false),
                        TeamID = c.Int(nullable: false),
                        PlayerID = c.Int(nullable: false),
                        position = c.Int(nullable: false),
                        isActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TNPID);
            
            CreateTable(
                "dbo.StatsYearWeeks",
                c => new
                    {
                        id = c.Int(nullable: false),
                        PlayerID = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        Week = c.Int(nullable: false),
                        currentPts = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PassingStats_PassAtt = c.Int(),
                        PassingStats_PassCmp = c.Int(),
                        PassingStats_PassYds = c.Int(),
                        PassingStats_PassTds = c.Int(),
                        PassingStats_PassInts = c.Int(),
                        PassingStats_PassTwopta = c.Int(),
                        PassingStats_PassTwoptm = c.Int(),
                        RushingStats_RushAtt = c.Int(),
                        RushingStats_RushYds = c.Int(),
                        RushingStats_RushTds = c.Int(),
                        RushingStats_RushLng = c.Int(),
                        RushingStats_RushLngtd = c.Int(),
                        RushingStats_RushTwopta = c.Int(),
                        RushingStats_RushTwoptm = c.Int(),
                        ReceivingStats_Rec = c.Int(),
                        ReceivingStats_RecYds = c.Int(),
                        ReceivingStats_RecTds = c.Int(),
                        ReceivingStats_RecLng = c.Int(),
                        ReceivingStats_RecLngtd = c.Int(),
                        ReceivingStats_RecTwopta = c.Int(),
                        ReceivingStats_RecTwoptm = c.Int(),
                        ReceivingStats_RecTrg = c.Int(),
                        FumbleStats_Tot = c.Int(),
                        FumbleStats_Rcv = c.Int(),
                        FumbleStats_Trcv = c.Int(),
                        FumbleStats_Yds = c.Int(),
                        FumbleStats_Lost = c.Int(),
                        KickingStats_Fgm = c.Int(),
                        KickingStats_Fga = c.Int(),
                        KickingStats_Fgyds = c.Int(),
                        KickingStats_Totpfg = c.Int(),
                        KickingStats_Xpmade = c.Int(),
                        KickingStats_Xpmissed = c.Int(),
                        KickingStats_Xpa = c.Int(),
                        KickingStats_Xpb = c.Int(),
                        KickingStats_Xptot = c.Int(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.GSettings",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        CurrentWeek = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FFGames", "VisTeamID", "dbo.FFTeams");
            DropForeignKey("dbo.FFGames", "HomeTeamID", "dbo.FFTeams");
            DropForeignKey("dbo.NFLPlayers", "FFTeam_FFTeamID", "dbo.FFTeams");
            DropForeignKey("dbo.FFGames", "FFTeam_FFTeamID", "dbo.FFTeams");
            DropForeignKey("dbo.FFTeams", "FFLeagueID", "dbo.FFLeagues");
            DropForeignKey("dbo.FFGames", "FFLeagueID", "dbo.FFLeagues");
            DropForeignKey("dbo.NFLPlayers", "FFLeague_FFLeagueID", "dbo.FFLeagues");
            DropIndex("dbo.NFLPlayers", new[] { "FFTeam_FFTeamID" });
            DropIndex("dbo.NFLPlayers", new[] { "FFLeague_FFLeagueID" });
            DropIndex("dbo.FFTeams", new[] { "FFLeagueID" });
            DropIndex("dbo.FFGames", new[] { "FFTeam_FFTeamID" });
            DropIndex("dbo.FFGames", new[] { "FFLeagueID" });
            DropIndex("dbo.FFGames", new[] { "VisTeamID" });
            DropIndex("dbo.FFGames", new[] { "HomeTeamID" });
            DropTable("dbo.GSettings");
            DropTable("dbo.StatsYearWeeks");
            DropTable("dbo.TeamNFLPlayers");
            DropTable("dbo.NFLPlayers");
            DropTable("dbo.FFLeagues");
            DropTable("dbo.FFTeams");
            DropTable("dbo.FFGames");
        }
    }
}
