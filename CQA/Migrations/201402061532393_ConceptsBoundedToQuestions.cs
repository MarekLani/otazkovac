namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConceptsBoundedToQuestions : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Concepts", "SubjectId", "dbo.Subjects");
            DropIndex("dbo.Concepts", new[] { "SubjectId" });
            CreateTable(
                "dbo.QuestionConcepts",
                c => new
                    {
                        ConceptId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ConceptId, t.QuestionId })
                .ForeignKey("dbo.Concepts", t => t.ConceptId, cascadeDelete: true)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.ConceptId)
                .Index(t => t.QuestionId);
            
            AddColumn("dbo.Concepts", "Subject_SubjectId", c => c.Int());
            AddForeignKey("dbo.Concepts", "Subject_SubjectId", "dbo.Subjects", "SubjectId");
            CreateIndex("dbo.Concepts", "Subject_SubjectId");
            DropColumn("dbo.Concepts", "SubjectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Concepts", "SubjectId", c => c.Int(nullable: false));
            DropIndex("dbo.QuestionConcepts", new[] { "QuestionId" });
            DropIndex("dbo.QuestionConcepts", new[] { "ConceptId" });
            DropIndex("dbo.Concepts", new[] { "Subject_SubjectId" });
            DropForeignKey("dbo.QuestionConcepts", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.QuestionConcepts", "ConceptId", "dbo.Concepts");
            DropForeignKey("dbo.Concepts", "Subject_SubjectId", "dbo.Subjects");
            DropColumn("dbo.Concepts", "Subject_SubjectId");
            DropTable("dbo.QuestionConcepts");
            CreateIndex("dbo.Concepts", "SubjectId");
            AddForeignKey("dbo.Concepts", "SubjectId", "dbo.Subjects", "SubjectId", cascadeDelete: true);
        }
    }
}
