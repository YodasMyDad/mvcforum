namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sections : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Section",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 450),
                        Description = c.String(),
                        SortOrder = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        ExtendedDataString = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Category", "Section_Id", c => c.Guid());
            CreateIndex("dbo.Category", "Section_Id");
            AddForeignKey("dbo.Category", "Section_Id", "dbo.Section", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Category", "Section_Id", "dbo.Section");
            DropIndex("dbo.Category", new[] { "Section_Id" });
            DropColumn("dbo.Category", "Section_Id");
            DropTable("dbo.Section");
        }
    }
}
