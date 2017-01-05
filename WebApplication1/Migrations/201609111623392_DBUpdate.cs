namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBUpdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NFLGames", "NFLTeam_TeamID", "dbo.NFLTeams");
            DropForeignKey("dbo.NFLPlayers", "NFLTeam_TeamID", "dbo.NFLTeams");
            DropIndex("dbo.NFLPlayers", new[] { "NFLTeam_TeamID" });
            DropIndex("dbo.NFLGames", new[] { "NFLTeam_TeamID" });
            RenameColumn(table: "dbo.NFLGames", name: "NFLTeam_TeamID", newName: "NFLTeam_Abbr");
            RenameColumn(table: "dbo.NFLPlayers", name: "NFLTeam_TeamID", newName: "NFLTeam_Abbr");
            DropPrimaryKey("dbo.NFLTeams");
            AddColumn("dbo.NFLGames", "NOTRegular", c => c.Boolean(nullable: false));
            AddColumn("dbo.NFLTeams", "Abbr", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.NFLPlayers", "NFLTeam_Abbr", c => c.String(maxLength: 128));
            AlterColumn("dbo.NFLGames", "Week", c => c.Int());
            AlterColumn("dbo.NFLGames", "HomeTeamID", c => c.String());
            AlterColumn("dbo.NFLGames", "VisTeamID", c => c.String());
            AlterColumn("dbo.NFLGames", "HScore", c => c.Int());
            AlterColumn("dbo.NFLGames", "VScore", c => c.Int());
            AlterColumn("dbo.NFLGames", "NFLTeam_Abbr", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.NFLTeams", "Abbr");
            CreateIndex("dbo.NFLPlayers", "NFLTeam_Abbr");
            CreateIndex("dbo.NFLGames", "NFLTeam_Abbr");
            CreateIndex("dbo.StatsYearWeeks", "PlayerID");
            AddForeignKey("dbo.StatsYearWeeks", "PlayerID", "dbo.NFLPlayers", "id", cascadeDelete: true);
            AddForeignKey("dbo.NFLGames", "NFLTeam_Abbr", "dbo.NFLTeams", "Abbr");
            AddForeignKey("dbo.NFLPlayers", "NFLTeam_Abbr", "dbo.NFLTeams", "Abbr");
            DropColumn("dbo.NFLPlayers", "week");
            DropColumn("dbo.NFLPlayers", "year");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassAtt");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassCmp");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassYds");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassTds");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassInts");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassTwopta");
            DropColumn("dbo.NFLPlayers", "PassingStats_PassTwoptm");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushAtt");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushYds");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushTds");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushLng");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushLngtd");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushTwopta");
            DropColumn("dbo.NFLPlayers", "RushingStats_RushTwoptm");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_Rec");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecYds");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecTds");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecLng");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecLngtd");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecTwopta");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecTwoptm");
            DropColumn("dbo.NFLPlayers", "ReceivingStats_RecTrg");
            DropColumn("dbo.NFLPlayers", "FumbleStats_Tot");
            DropColumn("dbo.NFLPlayers", "FumbleStats_Rcv");
            DropColumn("dbo.NFLPlayers", "FumbleStats_Trcv");
            DropColumn("dbo.NFLPlayers", "FumbleStats_Yds");
            DropColumn("dbo.NFLPlayers", "FumbleStats_Lost");
            DropColumn("dbo.NFLPlayers", "KickingStats_Fgm");
            DropColumn("dbo.NFLPlayers", "KickingStats_Fga");
            DropColumn("dbo.NFLPlayers", "KickingStats_Fgyds");
            DropColumn("dbo.NFLPlayers", "KickingStats_Totpfg");
            DropColumn("dbo.NFLPlayers", "KickingStats_Xpmade");
            DropColumn("dbo.NFLPlayers", "KickingStats_Xpmissed");
            DropColumn("dbo.NFLPlayers", "KickingStats_Xpa");
            DropColumn("dbo.NFLPlayers", "KickingStats_Xpb");
            DropColumn("dbo.NFLPlayers", "KickingStats_Xptot");
            DropColumn("dbo.NFLTeams", "TeamID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NFLTeams", "TeamID", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.NFLPlayers", "KickingStats_Xptot", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Xpb", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Xpa", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Xpmissed", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Xpmade", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Totpfg", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Fgyds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Fga", c => c.Int());
            AddColumn("dbo.NFLPlayers", "KickingStats_Fgm", c => c.Int());
            AddColumn("dbo.NFLPlayers", "FumbleStats_Lost", c => c.Int());
            AddColumn("dbo.NFLPlayers", "FumbleStats_Yds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "FumbleStats_Trcv", c => c.Int());
            AddColumn("dbo.NFLPlayers", "FumbleStats_Rcv", c => c.Int());
            AddColumn("dbo.NFLPlayers", "FumbleStats_Tot", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecTrg", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecTwoptm", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecTwopta", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecLngtd", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecLng", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecTds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_RecYds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "ReceivingStats_Rec", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushTwoptm", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushTwopta", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushLngtd", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushLng", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushTds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushYds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "RushingStats_RushAtt", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassTwoptm", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassTwopta", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassInts", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassTds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassYds", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassCmp", c => c.Int());
            AddColumn("dbo.NFLPlayers", "PassingStats_PassAtt", c => c.Int());
            AddColumn("dbo.NFLPlayers", "year", c => c.Int());
            AddColumn("dbo.NFLPlayers", "week", c => c.Int());
            DropForeignKey("dbo.NFLPlayers", "NFLTeam_Abbr", "dbo.NFLTeams");
            DropForeignKey("dbo.NFLGames", "NFLTeam_Abbr", "dbo.NFLTeams");
            DropForeignKey("dbo.StatsYearWeeks", "PlayerID", "dbo.NFLPlayers");
            DropIndex("dbo.StatsYearWeeks", new[] { "PlayerID" });
            DropIndex("dbo.NFLGames", new[] { "NFLTeam_Abbr" });
            DropIndex("dbo.NFLPlayers", new[] { "NFLTeam_Abbr" });
            DropPrimaryKey("dbo.NFLTeams");
            AlterColumn("dbo.NFLGames", "NFLTeam_Abbr", c => c.Int());
            AlterColumn("dbo.NFLGames", "VScore", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.NFLGames", "HScore", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.NFLGames", "VisTeamID", c => c.Int(nullable: false));
            AlterColumn("dbo.NFLGames", "HomeTeamID", c => c.Int(nullable: false));
            AlterColumn("dbo.NFLGames", "Week", c => c.String());
            AlterColumn("dbo.NFLPlayers", "NFLTeam_Abbr", c => c.Int());
            DropColumn("dbo.NFLTeams", "Abbr");
            DropColumn("dbo.NFLGames", "NOTRegular");
            AddPrimaryKey("dbo.NFLTeams", "TeamID");
            RenameColumn(table: "dbo.NFLPlayers", name: "NFLTeam_Abbr", newName: "NFLTeam_TeamID");
            RenameColumn(table: "dbo.NFLGames", name: "NFLTeam_Abbr", newName: "NFLTeam_TeamID");
            CreateIndex("dbo.NFLGames", "NFLTeam_TeamID");
            CreateIndex("dbo.NFLPlayers", "NFLTeam_TeamID");
            AddForeignKey("dbo.NFLPlayers", "NFLTeam_TeamID", "dbo.NFLTeams", "TeamID");
            AddForeignKey("dbo.NFLGames", "NFLTeam_TeamID", "dbo.NFLTeams", "TeamID");
        }
    }
}
