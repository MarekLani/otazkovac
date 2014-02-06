namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConceptBindingToSubjectRemoved : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Concepts", "Subject_SubjectId", "dbo.Subjects");
            DropIndex("dbo.Concepts", new[] { "Subject_SubjectId" });
            DropColumn("dbo.Concepts", "Subject_SubjectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Concepts", "Subject_SubjectId", c => c.Int());
            CreateIndex("dbo.Concepts", "Subject_SubjectId");
            AddForeignKey("dbo.Concepts", "Subject_SubjectId", "dbo.Subjects", "SubjectId");
        }
    }
}
