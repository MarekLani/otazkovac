namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActiveWeeksInConcepts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Concepts", "ActiveWeeks", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Concepts", "ActiveWeeks");
        }
    }
}
