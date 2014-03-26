namespace CQA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ErrorsModelAddedMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Errors",
                c => new
                    {
                        ErrorId = c.Int(nullable: false, identity: true),
                        Action = c.Int(nullable: false),
                        Data = c.String(),
                        ExceptionMessage = c.String(),
                        DateCreated = c.DateTime(),
                    })
                .PrimaryKey(t => t.ErrorId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Errors");
        }
    }
}
