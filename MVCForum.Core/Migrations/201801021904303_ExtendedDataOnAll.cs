namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendedDataOnAll : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Badge", "ExtendedDataString", c => c.String());
            AddColumn("dbo.Category", "ExtendedDataString", c => c.String());
            AddColumn("dbo.Settings", "ExtendedDataString", c => c.String());
            AddColumn("dbo.Topic", "ExtendedDataString", c => c.String());
            AddColumn("dbo.Post", "ExtendedDataString", c => c.String());
            AddColumn("dbo.Poll", "ExtendedDataString", c => c.String());
            AddColumn("dbo.PollAnswer", "ExtendedDataString", c => c.String());
            AddColumn("dbo.TopicTag", "ExtendedDataString", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TopicTag", "ExtendedDataString");
            DropColumn("dbo.PollAnswer", "ExtendedDataString");
            DropColumn("dbo.Poll", "ExtendedDataString");
            DropColumn("dbo.Post", "ExtendedDataString");
            DropColumn("dbo.Topic", "ExtendedDataString");
            DropColumn("dbo.Settings", "ExtendedDataString");
            DropColumn("dbo.Category", "ExtendedDataString");
            DropColumn("dbo.Badge", "ExtendedDataString");
        }
    }
}
