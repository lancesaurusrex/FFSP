namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class New : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NFLGames", "IsExhibition", c => c.Boolean(nullable: false));
            DropColumn("dbo.NFLGames", "NOTRegular");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NFLGames", "NOTRegular", c => c.Boolean(nullable: false));
            DropColumn("dbo.NFLGames", "IsExhibition");
        }
    }
}
