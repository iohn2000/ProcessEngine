CREATE TABLE [dbo].[ActivityDefinition]
(
[WFAD_ID] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[WFAD_Name] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFAD_Description] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFAD_ConfigTemplate] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WFAD_HostLoad] [int] NULL,
[WFAD_Created] [datetime2] NOT NULL,
[WFAD_Updated] [datetime2] NULL,
[WFAD_ValidFrom] [datetime2] NOT NULL,
[WFAD_ValidTo] [datetime2] NOT NULL,
[WFAD_IsStartActivity] [bit] NOT NULL CONSTRAINT [DF_ActivityDefinition_WFAD_IsStartActivity] DEFAULT ((0)),
[WFAD_Type] [int] NOT NULL CONSTRAINT [DF_ActivityDefinition_WFAD_Type] DEFAULT ((0))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ActivityDefinition] ADD CONSTRAINT [PK_ActivityDefinition] PRIMARY KEY CLUSTERED  ([WFAD_ID]) ON [PRIMARY]
GO
