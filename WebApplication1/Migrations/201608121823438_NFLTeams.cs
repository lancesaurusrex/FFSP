namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NFLTeams : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NFLGames",
                c => new
                    {
                        GameID = c.Int(nullable: false, identity: true),
                        DateEST = c.DateTime(nullable: false),
                        Day = c.String(),
                        Week = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        HomeTeamID = c.Int(nullable: false),
                        VisTeamID = c.Int(nullable: false),
                        HScore = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VScore = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NFLTeam_TeamID = c.Int(),
                    })
                .PrimaryKey(t => t.GameID)
                .ForeignKey("dbo.NFLTeams", t => t.NFLTeam_TeamID)
                .Index(t => t.NFLTeam_TeamID);
            
            CreateTable(
                "dbo.NFLTeams",
                c => new
                    {
                        TeamID = c.Int(nullable: false, identity: true),
                        City = c.String(),
                        Nickname = c.String(),
                    })
                .PrimaryKey(t => t.TeamID);
            
            AddColumn("dbo.NFLPlayers", "NFLTeam_TeamID", c => c.Int());
            CreateIndex("dbo.NFLPlayers", "NFLTeam_TeamID");
            AddForeignKey("dbo.NFLPlayers", "NFLTeam_TeamID", "dbo.NFLTeams", "TeamID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NFLPlayers", "NFLTeam_TeamID", "dbo.NFLTeams");
            DropForeignKey("dbo.NFLGames", "NFLTeam_TeamID", "dbo.NFLTeams");
            DropIndex("dbo.NFLGames", new[] { "NFLTeam_TeamID" });
            DropIndex("dbo.NFLPlayers", new[] { "NFLTeam_TeamID" });
            DropColumn("dbo.NFLPlayers", "NFLTeam_TeamID");
            DropTable("dbo.NFLTeams");
            DropTable("dbo.NFLGames");
        }
    }
}
