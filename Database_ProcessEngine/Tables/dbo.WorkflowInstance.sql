CREATE TABLE [dbo].[WorkflowInstance]
(
[WFI_ID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFI_WFD_ID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFI_Xml] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFI_Status] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFI_LockedByProcess] [uniqueidentifier] NOT NULL CONSTRAINT [DF_WorkflowInstance_WFI_LockedByProcess] DEFAULT ('00000000-0000-0000-0000-000000000000'),
[WFI_CurrentActivity] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFI_ParentWF] [nvarchar] (37) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFI_NextActivity] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFI_Created] [datetime2] NULL,
[WFI_Updated] [datetime2] NULL,
[WFI_Finished] [datetime2] NULL,
[WFI_ProcessTime] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[WorkflowInstance] ADD CONSTRAINT [PK_WorkflowInstance] PRIMARY KEY CLUSTERED  ([WFI_ID]) ON [PRIMARY]
GO
