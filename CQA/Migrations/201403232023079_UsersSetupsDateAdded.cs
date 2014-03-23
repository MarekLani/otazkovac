namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UsersSetupsDateAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UsersSetups", "DateCreated", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UsersSetups", "DateCreated");
        }
    }
}
