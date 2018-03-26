CREATE TABLE [dbo].[WorkflowXmlDataRepository]
(
[Guid] [nvarchar] (37) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFI_ID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[XML] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Created] [datetime2] NULL CONSTRAINT [DF_WorkflowXmlDataRepository_Created] DEFAULT (getdate())
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[WorkflowXmlDataRepository] ADD CONSTRAINT [PK_WorkflowXmlDataRepository] PRIMARY KEY CLUSTERED  ([Guid]) ON [PRIMARY]
GO
