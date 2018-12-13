namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClassTeamGameChanges : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.NFLGames");
            AddColumn("dbo.NFLGames", "GSIS", c => c.Int(nullable: false));
            AddColumn("dbo.NFLGames", "StartTime", c => c.String());
            AddColumn("dbo.NFLTeams", "IsDefunct", c => c.Boolean(nullable: false));
            AddColumn("dbo.NFLTeams", "currentLocation", c => c.String());
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
            AlterColumn("dbo.NFLGames", "DateEST", c => c.DateTime());
            AddPrimaryKey("dbo.NFLGames", "GameID");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.NFLGames");
            AlterColumn("dbo.NFLGames", "DateEST", c => c.DateTime(nullable: false));
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.NFLTeams", "currentLocation");
            DropColumn("dbo.NFLTeams", "IsDefunct");
            DropColumn("dbo.NFLGames", "StartTime");
            DropColumn("dbo.NFLGames", "GSIS");
            AddPrimaryKey("dbo.NFLGames", "GameID");
        }
    }
}
