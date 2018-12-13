namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gameidbacktoDBNone : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.NFLGames");
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.NFLGames", "GameID");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.NFLGames");
            AlterColumn("dbo.NFLGames", "GameID", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.NFLGames", "GameID");
        }
    }
}
