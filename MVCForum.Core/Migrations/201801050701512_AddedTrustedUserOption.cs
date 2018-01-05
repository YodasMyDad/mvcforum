namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTrustedUserOption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MembershipUser", "IsTrustedUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MembershipUser", "IsTrustedUser");
        }
    }
}
