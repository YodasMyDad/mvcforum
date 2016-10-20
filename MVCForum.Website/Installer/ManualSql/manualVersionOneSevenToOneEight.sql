ALTER TABLE [Settings] ADD [AgreeToTermsAndConditions] bit NULL
GO
ALTER TABLE [Settings] ADD [TermsAndConditions] nvarchar(Max) NULL
GO
ALTER TABLE [Settings] ADD [PointsAllowedForExtendedProfile] int NULL
GO
ALTER TABLE [Settings] ADD [DisableStandardRegistration] int NULL
GO
ALTER TABLE [Post] ADD InReplyTo [uniqueidentifier] NULL
GO
ALTER TABLE [MembershipUser] ADD [HasAgreedToTermsAndConditions] bit NULL
GO
ALTER TABLE [MembershipUserPoints] ADD [Notes] nvarchar(400) NULL
GO
CREATE TABLE [PostEdit](
	[Id] [uniqueidentifier] NOT NULL,
	[DateEdited] [datetime] NOT NULL,
	[OriginalPostContent] [nvarchar](max) NULL,
	[EditedPostContent] [nvarchar](max) NULL,
	[OriginalPostTitle] [nvarchar](500) NULL,
	[EditedPostTitle] [nvarchar](500) NULL,
	[Post_Id] [uniqueidentifier] NOT NULL,
	[MembershipUser_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.PostEdit] PRIMARY KEY CLUSTERED 
([Id] ASC)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON))
GO
ALTER TABLE [PostEdit]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PostEdit_dbo.MembershipUser_MembershipUser_Id] FOREIGN KEY([MembershipUser_Id])
REFERENCES [MembershipUser] ([Id])
GO
ALTER TABLE [PostEdit] CHECK CONSTRAINT [FK_dbo.PostEdit_dbo.MembershipUser_MembershipUser_Id]
GO
ALTER TABLE [PostEdit]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PostEdit_dbo.Post_Post_Id] FOREIGN KEY([Post_Id])
REFERENCES [Post] ([Id])
GO
ALTER TABLE [PostEdit] CHECK CONSTRAINT [FK_dbo.PostEdit_dbo.Post_Post_Id]
GO