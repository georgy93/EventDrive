USE [master]
GO

IF DB_ID('EventDriveDB') IS NOT NULL
  set noexec on               -- prevent creation when already exists


CREATE DATABASE [EventDriveDB];
GO

USE [EventDriveDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Items](
    [Id] [UNIQUEIDENTIFIER] DEFAULT NEWID() PRIMARY KEY NOT NULL,
	[Name] [varchar](MAX) NOT NULL
)
GO

CREATE LOGIN [user] WITH PASSWORD = 'simplePWD123!'
GO

CREATE USER [user] FOR LOGIN [user] WITH DEFAULT_SCHEMA=[dbo]
GO

GRANT ALL ON Items TO [user];
GO