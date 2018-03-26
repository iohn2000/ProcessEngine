CREATE TABLE [dbo].[WorkflowDefinition]
(
[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_WorkflowDefinition_Guid] DEFAULT (newid()),
[WFD_ID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFD_Name] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFD_Definition] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFD_Description] [nvarchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFD_Version] [int] NOT NULL,
[WFD_CheckedOutBy] [nvarchar] (37) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFD_Created] [datetime2] NOT NULL CONSTRAINT [DF_WorkflowDefinition_WFD_Created] DEFAULT (getdate()),
[WFD_ValidFrom] [datetime2] NOT NULL CONSTRAINT [DF_WorkflowDefinition_WFD_ValidFrom] DEFAULT (getdate()),
[WFD_ValidTo] [datetime2] NULL CONSTRAINT [DF_WorkflowDefinition_WFD_ValidTo] DEFAULT (NULL)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[WorkflowDefinition] ADD CONSTRAINT [PK_WorkflowDefinition] PRIMARY KEY CLUSTERED  ([Guid]) ON [PRIMARY]
GO
