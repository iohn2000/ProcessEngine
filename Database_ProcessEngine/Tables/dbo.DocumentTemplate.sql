CREATE TABLE [dbo].[DocumentTemplate]
(
[TMPL_ID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_DocumentTemplates_TMPL_ID] DEFAULT (newid()),
[TMPL_Name] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[TMPL_Description] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[TMPL_Content] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[TMPL_Category] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_DocumentTemplates_TMPL_Category] DEFAULT (N'EMAIL'),
[TMPL_Created] [datetime2] NOT NULL CONSTRAINT [DF_DocumentTemplates_TMPL_Created] DEFAULT (getdate()),
[TMPL_Updated] [datetime2] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[DocumentTemplate] ADD CONSTRAINT [PK_DocumentTemplates] PRIMARY KEY CLUSTERED  ([TMPL_Name], [TMPL_Category]) ON [PRIMARY]
GO
