namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreaseAccessTokenLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MembershipUser", "FacebookAccessToken", c => c.String(maxLength: 1000));
            AlterColumn("dbo.MembershipUser", "TwitterAccessToken", c => c.String(maxLength: 1000));
            AlterColumn("dbo.MembershipUser", "GoogleAccessToken", c => c.String(maxLength: 1000));
            AlterColumn("dbo.MembershipUser", "MicrosoftAccessToken", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MembershipUser", "MicrosoftAccessToken", c => c.String(maxLength: 450));
            AlterColumn("dbo.MembershipUser", "GoogleAccessToken", c => c.String(maxLength: 300));
            AlterColumn("dbo.MembershipUser", "TwitterAccessToken", c => c.String(maxLength: 300));
            AlterColumn("dbo.MembershipUser", "FacebookAccessToken", c => c.String(maxLength: 300));
        }
    }
}
