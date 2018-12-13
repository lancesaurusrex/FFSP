namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IhateMigrations : DbMigration
    {
        public override void Up()
        {
            //DropPrimaryKey("dbo.NFLGames");
            //AddColumn("dbo.NFLGames", "Id", c => c.Int(nullable: false));
            //AddPrimaryKey("dbo.NFLGames", "Id");
            //DropColumn("dbo.NFLGames", "GameID5");
            //DropColumn("dbo.NFLGames", "GameID");
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            //AddColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
            //AddColumn("dbo.NFLGames", "GameID5", c => c.Int(nullable: false));
            //DropPrimaryKey("dbo.NFLGames");
            //DropColumn("dbo.NFLGames", "Id");
            //AddPrimaryKey("dbo.NFLGames", "GameID5");
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false, identity: true));
        }
    }
}
