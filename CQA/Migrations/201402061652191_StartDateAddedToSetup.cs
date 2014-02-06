namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StartDateAddedToSetup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Setups", "StartDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Setups", "StartDate");
        }
    }
}
