namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionFileIdAddedToQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "QuestionFileId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "QuestionFileId");
        }
    }
}
