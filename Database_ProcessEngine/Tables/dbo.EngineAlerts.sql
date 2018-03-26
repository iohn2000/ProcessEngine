CREATE TABLE [dbo].[EngineAlerts]
(
[EA_ID] [int] NOT NULL IDENTITY(1, 1),
[EA_WFI_ID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EA_StartActivity] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EA_InputParameters] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EA_Created] [datetime2] NULL,
[EA_Updated] [datetime2] NULL,
[EA_Status] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EA_CallbackID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EA_Type] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EA_LastPolling] [datetime2] NULL,
[EA_PollingIntervalSeconds] [int] NULL,
[EA_LockedByProcess] [uniqueidentifier] NULL,
[EA_ProcessEngineInstance] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[EngineAlerts] ADD CONSTRAINT [PK_EngineAlerts] PRIMARY KEY CLUSTERED  ([EA_ID]) ON [PRIMARY]
GO
EXEC sp_addextendedproperty N'MS_Description', N'normal OR polling', 'SCHEMA', N'dbo', 'TABLE', N'EngineAlerts', 'COLUMN', N'EA_Type'
GO
