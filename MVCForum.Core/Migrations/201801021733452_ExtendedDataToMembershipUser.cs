namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendedDataToMembershipUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MembershipUser", "ExtendedDataString", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MembershipUser", "ExtendedDataString");
        }
    }
}
