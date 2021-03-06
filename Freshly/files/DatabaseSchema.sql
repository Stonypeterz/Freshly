SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationGroups]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ApplicationGroups](
	[GroupName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_ApplicationGroups] PRIMARY KEY CLUSTERED 
(
	[GroupName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ApplicationPolicies]    Script Date: 11/3/2018 1:49:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationPolicies]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ApplicationPolicies](
	[PolicyName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_ApplicationPolicies] PRIMARY KEY CLUSTERED 
(
	[PolicyName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ApplicationUsers]    Script Date: 11/3/2018 1:49:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationUsers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ApplicationUsers](
	[UserId] [nvarchar](128) NOT NULL,
	[FirstName] [nvarchar](256) NOT NULL,
	[LastName] [nvarchar](256) NOT NULL,
	[DateOfBirth] [datetime] NULL,
	[Gender] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[PhoneNumber] [nvarchar](128) NULL,
	[Password] [nvarchar](max) NOT NULL,
	[IsLogedIn] [bit] NOT NULL,
	[LastLoginDate] [datetime] NULL,
	[AccountStatus] [nvarchar](256) NOT NULL,
	[OnlineStatus] [nvarchar](16) NOT NULL,
	[LastLockDate] [datetime] NULL,
	[LastActivityDate] [datetime] NOT NULL,
	[RegDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ApplicationUsers] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[GroupPolicies]    Script Date: 11/3/2018 1:50:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GroupPolicies]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GroupPolicies](
	[GroupName] [nvarchar](128) NOT NULL,
	[PolicyName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_GroupPolicies] PRIMARY KEY CLUSTERED 
(
	[GroupName] ASC,
	[PolicyName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[UserAvatars]    Script Date: 11/3/2018 1:50:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserAvatars]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserAvatars](
	[UserId] [nvarchar](128) NOT NULL,
	[Avatar] [varbinary](max) NOT NULL,
	[Extension] [varchar](10) NOT NULL,
	[MimeType] [varchar](128) NOT NULL,
	[DateUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_UserAvatars] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[UserGroups]    Script Date: 11/3/2018 1:50:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserGroups]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserGroups](
	[UserId] [nvarchar](128) NOT NULL,
	[GroupName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_UserGroups] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[GroupName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_ApplicationUsers_IsLogedIn]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ApplicationUsers] ADD  CONSTRAINT [DF_ApplicationUsers_IsLogedIn]  DEFAULT ((0)) FOR [IsLogedIn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_ApplicationUsers_AccountStatus]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ApplicationUsers] ADD  CONSTRAINT [DF_ApplicationUsers_AccountStatus]  DEFAULT (N'Pending') FOR [AccountStatus]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_ApplicationUsers_LastActivityDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ApplicationUsers] ADD  CONSTRAINT [DF_ApplicationUsers_LastActivityDate]  DEFAULT (getdate()) FOR [LastActivityDate]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_ApplicationUsers_RegDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ApplicationUsers] ADD  CONSTRAINT [DF_ApplicationUsers_RegDate]  DEFAULT (getdate()) FOR [RegDate]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_UserAvatars_DateUpdated]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UserAvatars] ADD  CONSTRAINT [DF_UserAvatars_DateUpdated]  DEFAULT (getdate()) FOR [DateUpdated]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GroupPolicies_ApplicationGroups]') AND parent_object_id = OBJECT_ID(N'[dbo].[GroupPolicies]'))
ALTER TABLE [dbo].[GroupPolicies]  WITH CHECK ADD  CONSTRAINT [FK_GroupPolicies_ApplicationGroups] FOREIGN KEY([GroupName])
REFERENCES [dbo].[ApplicationGroups] ([GroupName])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GroupPolicies_ApplicationGroups]') AND parent_object_id = OBJECT_ID(N'[dbo].[GroupPolicies]'))
ALTER TABLE [dbo].[GroupPolicies] CHECK CONSTRAINT [FK_GroupPolicies_ApplicationGroups]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GroupPolicies_ApplicationPolicies]') AND parent_object_id = OBJECT_ID(N'[dbo].[GroupPolicies]'))
ALTER TABLE [dbo].[GroupPolicies]  WITH CHECK ADD  CONSTRAINT [FK_GroupPolicies_ApplicationPolicies] FOREIGN KEY([PolicyName])
REFERENCES [dbo].[ApplicationPolicies] ([PolicyName])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GroupPolicies_ApplicationPolicies]') AND parent_object_id = OBJECT_ID(N'[dbo].[GroupPolicies]'))
ALTER TABLE [dbo].[GroupPolicies] CHECK CONSTRAINT [FK_GroupPolicies_ApplicationPolicies]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserAvatars_ApplicationUsers]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserAvatars]'))
ALTER TABLE [dbo].[UserAvatars]  WITH CHECK ADD  CONSTRAINT [FK_UserAvatars_ApplicationUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([UserId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserAvatars_ApplicationUsers]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserAvatars]'))
ALTER TABLE [dbo].[UserAvatars] CHECK CONSTRAINT [FK_UserAvatars_ApplicationUsers]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserGroups_ApplicationGroups]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserGroups]'))
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD  CONSTRAINT [FK_UserGroups_ApplicationGroups] FOREIGN KEY([GroupName])
REFERENCES [dbo].[ApplicationGroups] ([GroupName])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserGroups_ApplicationGroups]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserGroups]'))
ALTER TABLE [dbo].[UserGroups] CHECK CONSTRAINT [FK_UserGroups_ApplicationGroups]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserGroups_ApplicationUsers]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserGroups]'))
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD  CONSTRAINT [FK_UserGroups_ApplicationUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([UserId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserGroups_ApplicationUsers]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserGroups]'))
ALTER TABLE [dbo].[UserGroups] CHECK CONSTRAINT [FK_UserGroups_ApplicationUsers]
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUsersList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetUsersList] AS' 
END
GO
ALTER PROCEDURE [dbo].[GetUsersList]
	@PageNo [int],
	@PageSize [int],
	@GroupName [nvarchar](128),
	@filterQ [nvarchar](256)
AS
Begin
	Set NOCOUNT ON;
	Declare @Total int;
	If(@filterQ = '0')
	Begin
		With PageObject As (Select Row_Number() Over (Order By [FirstName]) As RowNumber, [UserId], [FirstName], [LastName], [IsLogedIn], [AccountStatus], [OnlineStatus] From dbo.[ApplicationUsers] A Where A.UserId IN (select B.UserId From dbo.UserGroups B Where B.GroupName = @GroupName) Or A.UserId = Case When @GroupName = 'All' Then A.UserId Else @GroupName End)
		Select [UserId], [FirstName], [LastName], [IsLogedIn], [AccountStatus], [OnlineStatus] From PageObject Q Where Q.RowNumber <= (@PageNo * @PageSize) And Q.RowNumber > ((@PageNo * @PageSize) - @PageSize);
		Select @Total = Count(*) From dbo.[ApplicationUsers] A Where A.UserId IN (select B.UserId From dbo.UserGroups B Where B.GroupName = @GroupName) Or A.UserId = Case When @GroupName = 'All' Then A.UserId Else @GroupName End;
		If(@Total % @PageSize = 0) Select @Total / @PageSize As 'PageCount' Else Select ((@Total - (@Total % @PageSize))/@PageSize) + 1 As 'PageCount';
	End
	Else 
	Begin
		With PageObject As (Select Row_Number() Over (Order By [FirstName]) As RowNumber, [UserId], [FirstName], [LastName], [IsLogedIn], [AccountStatus], [OnlineStatus] From dbo.[ApplicationUsers] A Where (A.UserId IN (select B.UserId From dbo.UserGroups B Where B.GroupName = @GroupName) Or A.UserId = Case When @GroupName = 'All' Then A.UserId Else @GroupName End) And ([UserId] Like '%' + @filterQ + '%' Or [FirstName] Like '%' + @filterQ + '%' Or [LastName] Like '%' + @filterQ + '%' Or [Gender] Like '%' + @filterQ + '%' Or [Email] Like '%' + @filterQ + '%' Or [PhoneNumber] Like '%' + @filterQ + '%' Or [Password] Like '%' + @filterQ + '%' Or [AccountStatus] Like '%' + @filterQ + '%' Or [OnlineStatus] = @filterQ))
		Select [UserId], [FirstName], [LastName], [IsLogedIn], [AccountStatus], [OnlineStatus] From PageObject Q Where Q.RowNumber <= (@PageNo * @PageSize) And Q.RowNumber > ((@PageNo * @PageSize) - @PageSize);
		Select @Total = Count(*) From dbo.[ApplicationUsers] A Where (A.UserId IN (select B.UserId From dbo.UserGroups B Where B.GroupName = @GroupName) Or A.UserId = Case When @GroupName = 'All' Then A.UserId Else @GroupName End) And ([UserId] Like '%' + @filterQ + '%' Or [FirstName] Like '%' + @filterQ + '%' Or [LastName] Like '%' + @filterQ + '%' Or [Gender] Like '%' + @filterQ + '%' Or [Email] Like '%' + @filterQ + '%' Or [PhoneNumber] Like '%' + @filterQ + '%' Or [Password] Like '%' + @filterQ + '%' Or [AccountStatus] Like '%' + @filterQ + '%' Or [OnlineStatus] = @filterQ);
		If(@Total % @PageSize = 0) Select @Total / @PageSize As 'PageCount' Else Select ((@Total - (@Total % @PageSize))/@PageSize) + 1 As 'PageCount';
	End
End
GO

