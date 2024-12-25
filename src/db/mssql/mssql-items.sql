USE [master]
GO

DROP Database IF EXISTS EventDriveDB
-- IF DB_ID('EventDriveDB') IS NOT NULL
  -- set noexec on               -- prevent creation when already exists

CREATE DATABASE [EventDriveDB];
GO

USE [EventDriveDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Items]
(
	[Id] [int] identity(1,1) NOT NULL, [ItemId] [nvarchar] (MAX) NOT NULL, [ItemName] [nvarchar] (MAX) NOT NULL,
	CONSTRAINT [PK_Items] PRIMARY KEY CLUSTERED ( [ID] ASC )
);
GO

CREATE LOGIN [user] WITH PASSWORD = 'simplePWD123!'
GO

CREATE USER [user] FOR LOGIN [user] WITH DEFAULT_SCHEMA=[dbo]
GO

GRANT CONTROL ON Items TO [user];
GO