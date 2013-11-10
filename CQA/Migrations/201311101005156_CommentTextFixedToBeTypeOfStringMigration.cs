namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommentTextFixedToBeTypeOfStringMigration : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Comments", "Text", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Comments", "Text", c => c.Double(nullable: false));
        }
    }
}
