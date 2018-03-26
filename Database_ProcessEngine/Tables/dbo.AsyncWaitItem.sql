CREATE TABLE [dbo].[AsyncWaitItem]
(
[AWI_ID] [int] NOT NULL IDENTITY(1, 1),
[AWI_InstanceID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AWI_ActivityInstanceID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AWI_Status] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AWI_StartDate] [datetime2] NULL,
[AWI_DueDate] [datetime2] NULL,
[AWI_CompletedDate] [datetime2] NULL,
[AWI_Config] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AWI_Created] [datetime2] NOT NULL CONSTRAINT [DF_AsyncWaitItem_AWI_Created] DEFAULT (getdate()),
[AWI_Modified] [datetime2] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[AsyncWaitItem] ADD CONSTRAINT [PK_AsyncWaitItem] PRIMARY KEY CLUSTERED  ([AWI_ID]) ON [PRIMARY]
GO
