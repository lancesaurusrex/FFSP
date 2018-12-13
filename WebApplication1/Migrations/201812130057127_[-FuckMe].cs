namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FuckMe : DbMigration
    {
        public override void Up()
        {
            //DropPrimaryKey("dbo.NFLGames");
            //AddColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
            //AddPrimaryKey("dbo.NFLGames", "GameID");
            //DropColumn("dbo.NFLGames", "Id");
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            //AddColumn("dbo.NFLGames", "Id", c => c.Int(nullable: false));
            //DropPrimaryKey("dbo.NFLGames");
            //DropColumn("dbo.NFLGames", "GameID");
            //AddPrimaryKey("dbo.NFLGames", "Id");
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false, identity: true));
        }
    }
}
