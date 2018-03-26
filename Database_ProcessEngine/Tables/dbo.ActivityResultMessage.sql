CREATE TABLE [dbo].[ActivityResultMessage]
(
[ARM_ID] [uniqueidentifier] NOT NULL,
[ARM_Woin] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ARM_ActivityInstanceId] [nvarchar] (150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ARM_ResultMessage] [nvarchar] (4000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ARM_Created] [datetime2] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ActivityResultMessage] ADD CONSTRAINT [PK_ActivityResultMessage] PRIMARY KEY CLUSTERED  ([ARM_ID]) ON [PRIMARY]
GO
