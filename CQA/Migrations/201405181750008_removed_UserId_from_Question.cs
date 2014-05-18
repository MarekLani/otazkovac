namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removed_UserId_from_Question : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Questions", "UserProfile_UserId", "dbo.UserProfile");
            DropIndex("dbo.Questions", new[] { "UserProfile_UserId" });
            DropColumn("dbo.Questions", "UserProfile_UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Questions", "UserProfile_UserId", c => c.Int());
            CreateIndex("dbo.Questions", "UserProfile_UserId");
            AddForeignKey("dbo.Questions", "UserProfile_UserId", "dbo.UserProfile", "UserId");
        }
    }
}
