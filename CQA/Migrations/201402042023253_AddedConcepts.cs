namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedConcepts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Concepts",
                c => new
                    {
                        ConceptId = c.Int(nullable: false, identity: true),
                        SubjectId = c.Int(nullable: false),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.ConceptId)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.SubjectId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Concepts", new[] { "SubjectId" });
            DropForeignKey("dbo.Concepts", "SubjectId", "dbo.Subjects");
            DropTable("dbo.Concepts");
        }
    }
}
