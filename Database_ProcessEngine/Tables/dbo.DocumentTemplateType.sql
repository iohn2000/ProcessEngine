CREATE TABLE [dbo].[DocumentTemplateType]
(
[DTYP_ID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_DocumentTemplateTypes_DTYP_ID] DEFAULT (newid()),
[DTYP_Name] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_DocumentTemplateTypes_DTYP_Name] DEFAULT (N'email'),
[DTYP_Created] [datetime2] NOT NULL CONSTRAINT [DF_DocumentTemplateTypes_DTYP_Created] DEFAULT (getdate())
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DocumentTemplateType] ADD CONSTRAINT [PK_DocumentTemplateTypes] PRIMARY KEY CLUSTERED  ([DTYP_Name]) ON [PRIMARY]
GO
