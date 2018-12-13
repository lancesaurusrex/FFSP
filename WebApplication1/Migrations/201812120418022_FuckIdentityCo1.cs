namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FuckIdentityCo1 : DbMigration
    {
        public override void Up()
        {
            //DropPrimaryKey("dbo.NFLGames");
            //AddColumn("dbo.NFLGames", "GameID5", c => c.Int(nullable: false));
            //AddPrimaryKey("dbo.NFLGames", "Id");
        }
        
        public override void Down()
        {
            //DropPrimaryKey("dbo.NFLGames");
            //DropColumn("dbo.NFLGames", "GameID5");
            //AddPrimaryKey("dbo.NFLGames", "Id");
        }
    }
}
