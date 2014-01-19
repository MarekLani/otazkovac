namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationsInSeparateTableMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        AnswerId = c.Int(nullable: false),
                        NotificationFor = c.Int(nullable: false),
                        NotificationType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Answers", t => t.AnswerId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.AnswerId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Notifications", new[] { "AnswerId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropForeignKey("dbo.Notifications", "AnswerId", "dbo.Answers");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.UserProfile");
            DropTable("dbo.Notifications");
        }
    }
}
