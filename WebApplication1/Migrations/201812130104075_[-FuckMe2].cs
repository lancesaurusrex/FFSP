namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FuckMe2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.NFLGames");
            AddColumn("dbo.NFLGames", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.NFLGames", "Id");
            DropColumn("dbo.NFLGames", "GameID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.NFLGames");
            DropColumn("dbo.NFLGames", "Id");
            AddPrimaryKey("dbo.NFLGames", "GameID");
        }
    }
}
