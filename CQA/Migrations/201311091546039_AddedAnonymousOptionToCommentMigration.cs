namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAnonymousOptionToCommentMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "Anonymous", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "Anonymous");
        }
    }
}
