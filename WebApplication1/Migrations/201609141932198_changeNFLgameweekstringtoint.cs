namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeNFLgameweekstringtoint : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NFLGames", "Week", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NFLGames", "Week", c => c.String());
        }
    }
}
