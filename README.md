[![](https://img.shields.io/nuget/v/EventSourcR.svg)](https://www.nuget.org/packages/EventSourcR) [![](https://img.shields.io/nuget/vpre/EventSourcR.svg)](https://www.nuget.org/packages/EventSourcR)

# EventSourcR
Simple event sourcing library

Documentation is coming in the near future. This library is still being developed. The Sample projects can help you get started if you are interested in trying it out before it's ready for production.

If you want to use the SQL Server provider. You can setup the database by running this script.

```sql
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE DATABASE [EventStore]
GO

ALTER DATABASE EventStore SET READ_COMMITTED_SNAPSHOT ON
GO

ALTER DATABASE EventStore SET ALLOW_SNAPSHOT_ISOLATION ON
GO

USE [EventStore]
GO

CREATE TABLE [dbo].[Events](
	[EventNumber] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[EventType] [nvarchar](450) NULL,
	[AggregateId] [uniqueidentifier] NOT NULL,
	[AggregateType] [nvarchar](450) NULL,
	[AggregateVersion] [bigint] NOT NULL,
	[SerializedData] [nvarchar](max) NULL,
	[SerializedMetadata] [nvarchar](max) NULL,
	[Recorded] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[EventNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [AK_Events_AggregateId_AggregateVersion] UNIQUE NONCLUSTERED 
(
	[AggregateId] ASC,
	[AggregateVersion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [AK_Events_EventId] UNIQUE NONCLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
```
