namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 256),
                        RealName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.UploadedImages",
                c => new
                    {
                        UploadedImageId = c.Guid(nullable: false),
                        ImageUrl = c.String(),
                        Position = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UploadedImageId)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        QuestionId = c.Int(nullable: false, identity: true),
                        QuestionText = c.String(nullable: false),
                        Hint = c.String(),
                        ImageUri = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        SetupId = c.Int(nullable: false),
                        UserProfile_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("dbo.Setups", t => t.SetupId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.UserProfile_UserId)
                .Index(t => t.SetupId)
                .Index(t => t.UserProfile_UserId);
            
            CreateTable(
                "dbo.Setups",
                c => new
                    {
                        SetupId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Active = c.Boolean(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        AnsweringProbability = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SetupId)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.Subjects",
                c => new
                    {
                        SubjectId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Shortcut = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.SubjectId);
            
            CreateTable(
                "dbo.UsersSetups",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        SetupId = c.Int(nullable: false),
                        Score = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.SetupId })
                .ForeignKey("dbo.Setups", t => t.SetupId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.SetupId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.SetupsStatistics",
                c => new
                    {
                        SetupsStatisticsId = c.Int(nullable: false, identity: true),
                        SetupId = c.Int(nullable: false),
                        AnswersCount = c.Int(nullable: false),
                        EvaluatedAnswersForUsersCount = c.Int(nullable: false),
                        FullyEvaluatedAnswersCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SetupsStatisticsId)
                .ForeignKey("dbo.Setups", t => t.SetupId, cascadeDelete: true)
                .Index(t => t.SetupId);
            
            CreateTable(
                "dbo.SetupsProbabilityChanges",
                c => new
                    {
                        SetupsProbabilityChangeId = c.Int(nullable: false, identity: true),
                        SetupId = c.Int(nullable: false),
                        Value = c.Int(nullable: false),
                        DateCreated = c.DateTime(),
                    })
                .PrimaryKey(t => t.SetupsProbabilityChangeId)
                .ForeignKey("dbo.Setups", t => t.SetupId, cascadeDelete: true)
                .Index(t => t.SetupId);
            
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        AnswerId = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        ExpertRating = c.Double(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        SeenEvaluation = c.Boolean(),
                        DateCreated = c.DateTime(),
                    })
                .PrimaryKey(t => t.AnswerId)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.UserId)
                .Index(t => t.QuestionId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Evaluations",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        AnswerId = c.Int(nullable: false),
                        Value = c.Double(nullable: false),
                        DateCreated = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.UserId, t.AnswerId })
                .ForeignKey("dbo.Answers", t => t.AnswerId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.AnswerId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UsersActions",
                c => new
                    {
                        UsersActionId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        AnswerId = c.Int(),
                        QuestionId = c.Int(),
                        Action = c.Int(nullable: false),
                        DateCreated = c.DateTime(),
                    })
                .PrimaryKey(t => t.UsersActionId)
                .ForeignKey("dbo.Answers", t => t.AnswerId)
                .ForeignKey("dbo.Questions", t => t.QuestionId)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.AnswerId)
                .Index(t => t.QuestionId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.QuestionViews",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        ViewDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.QuestionId })
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.QuestionId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        Text = c.Double(nullable: false),
                        UserId = c.Int(nullable: false),
                        AnswerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.UserProfile", t => t.UserId)
                .ForeignKey("dbo.Answers", t => t.AnswerId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.AnswerId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Comments", new[] { "AnswerId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.QuestionViews", new[] { "UserId" });
            DropIndex("dbo.QuestionViews", new[] { "QuestionId" });
            DropIndex("dbo.UsersActions", new[] { "UserId" });
            DropIndex("dbo.UsersActions", new[] { "QuestionId" });
            DropIndex("dbo.UsersActions", new[] { "AnswerId" });
            DropIndex("dbo.Evaluations", new[] { "UserId" });
            DropIndex("dbo.Evaluations", new[] { "AnswerId" });
            DropIndex("dbo.Answers", new[] { "UserId" });
            DropIndex("dbo.Answers", new[] { "QuestionId" });
            DropIndex("dbo.SetupsProbabilityChanges", new[] { "SetupId" });
            DropIndex("dbo.SetupsStatistics", new[] { "SetupId" });
            DropIndex("dbo.UsersSetups", new[] { "UserId" });
            DropIndex("dbo.UsersSetups", new[] { "SetupId" });
            DropIndex("dbo.Setups", new[] { "SubjectId" });
            DropIndex("dbo.Questions", new[] { "UserProfile_UserId" });
            DropIndex("dbo.Questions", new[] { "SetupId" });
            DropIndex("dbo.UploadedImages", new[] { "UserId" });
            DropForeignKey("dbo.Comments", "AnswerId", "dbo.Answers");
            DropForeignKey("dbo.Comments", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.QuestionViews", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.QuestionViews", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.UsersActions", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UsersActions", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.UsersActions", "AnswerId", "dbo.Answers");
            DropForeignKey("dbo.Evaluations", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Evaluations", "AnswerId", "dbo.Answers");
            DropForeignKey("dbo.Answers", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Answers", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.SetupsProbabilityChanges", "SetupId", "dbo.Setups");
            DropForeignKey("dbo.SetupsStatistics", "SetupId", "dbo.Setups");
            DropForeignKey("dbo.UsersSetups", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UsersSetups", "SetupId", "dbo.Setups");
            DropForeignKey("dbo.Setups", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.Questions", "UserProfile_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Questions", "SetupId", "dbo.Setups");
            DropForeignKey("dbo.UploadedImages", "UserId", "dbo.UserProfile");
            DropTable("dbo.Comments");
            DropTable("dbo.QuestionViews");
            DropTable("dbo.UsersActions");
            DropTable("dbo.Evaluations");
            DropTable("dbo.Answers");
            DropTable("dbo.SetupsProbabilityChanges");
            DropTable("dbo.SetupsStatistics");
            DropTable("dbo.UsersSetups");
            DropTable("dbo.Subjects");
            DropTable("dbo.Setups");
            DropTable("dbo.Questions");
            DropTable("dbo.UploadedImages");
            DropTable("dbo.UserProfile");
        }
    }
}
