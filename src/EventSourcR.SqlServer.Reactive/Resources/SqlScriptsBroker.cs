#region License
// TableDependency, EventReactor
// Copyright (c) 2015-2019 Christian Del Bianco. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace EventSourcR.SqlServer.Reactive.Resources
{
    public static partial class SqlScripts
    {
        public const string CreateProcedureQueueActivation = @"CREATE PROCEDURE [{2}].[{0}_QueueActivationSender] AS 
BEGIN 
    SET NOCOUNT ON;
    DECLARE @h AS UNIQUEIDENTIFIER;
    DECLARE @mt NVARCHAR(200);

    RECEIVE TOP(1) @h = conversation_handle, @mt = message_type_name FROM [{2}].[{0}_Sender];

    IF @mt = N'http://schemas.microsoft.com/SQL/ServiceBroker/EndDialog'
    BEGIN
        END CONVERSATION @h;
    END

    IF @mt = N'http://schemas.microsoft.com/SQL/ServiceBroker/DialogTimer' OR @mt = N'http://schemas.microsoft.com/SQL/ServiceBroker/Error'
    BEGIN 
        PRINT N'EventReactor: Drop objects {0} started.';

        END CONVERSATION @h;

        {1}

        PRINT N'EventReactor: Drop objects {0} ended.';
    END
END";

        public const string CreateTrigger = @"CREATE TRIGGER [tr_{0}_Sender] ON {1} AFTER INSERT AS 
DECLARE @xml XML
BEGIN
    SET NOCOUNT ON;

	SET @xml = (SELECT [EventNumber],[EventId],[EventType],[AggregateId],[AggregateType],[AggregateVersion],[SerializedData],[SerializedMetadata],[Recorded] 
	FROM INSERTED FOR XML PATH('ReactiveEventEntity'), ROOT('ArrayOfReactiveEventEntity'));

    SEND ON CONVERSATION '{2}'
    MESSAGE TYPE [{0}] (@xml)
END";

        public const string InsertInTableVariableConsideringUpdateOf = @"IF ({0}) 
        BEGIN
            SET @dmlType = '{1}'
            {2}
        END
        ELSE BEGIN
            RETURN;
        END";

        public const string InsertInTableVariable = @"SET @dmlType = '{0}';
            {1}";

        public const string ScriptDropAll = @"DECLARE @conversation_handle UNIQUEIDENTIFIER;
        DECLARE @schema_id INT;
        SELECT @schema_id = schema_id FROM sys.schemas WITH (NOLOCK) WHERE name = N'{2}';

        PRINT N'EventReactor: Dropping trigger [{2}].[tr_{0}_Sender].';
        IF EXISTS (SELECT * FROM sys.triggers WITH (NOLOCK) WHERE object_id = OBJECT_ID(N'[{2}].[tr_{0}_Sender]')) DROP TRIGGER [{2}].[tr_{0}_Sender];

        PRINT N'EventReactor: Deactivating queue [{2}].[{0}_Sender].';
        IF EXISTS (SELECT * FROM sys.service_queues WITH (NOLOCK) WHERE schema_id = @schema_id AND name = N'{0}_Sender') EXEC (N'ALTER QUEUE [{2}].[{0}_Sender] WITH ACTIVATION (STATUS = OFF)');

        PRINT N'EventReactor: Ending conversations {0}.';
        SELECT conversation_handle INTO #Conversations FROM sys.conversation_endpoints WITH (NOLOCK) WHERE far_service LIKE N'{0}_%' ORDER BY is_initiator ASC;
        DECLARE conversation_cursor CURSOR FAST_FORWARD FOR SELECT conversation_handle FROM #Conversations;
        OPEN conversation_cursor;
        FETCH NEXT FROM conversation_cursor INTO @conversation_handle;
        WHILE @@FETCH_STATUS = 0 
        BEGIN
            END CONVERSATION @conversation_handle WITH CLEANUP;
            FETCH NEXT FROM conversation_cursor INTO @conversation_handle;
        END
        CLOSE conversation_cursor;
        DEALLOCATE conversation_cursor;
        DROP TABLE #Conversations;

        PRINT N'EventReactor: Dropping service broker {0}_Receiver.';
        IF EXISTS (SELECT * FROM sys.services WITH (NOLOCK) WHERE name = N'{0}_Receiver') DROP SERVICE [{0}_Receiver];
        PRINT N'EventReactor: Dropping service broker {0}_Sender.';
        IF EXISTS (SELECT * FROM sys.services WITH (NOLOCK) WHERE name = N'{0}_Sender') DROP SERVICE [{0}_Sender];

        PRINT N'EventReactor: Dropping queue {2}.[{0}_Receiver].';
        IF EXISTS (SELECT * FROM sys.service_queues WITH (NOLOCK) WHERE schema_id = @schema_id AND name = N'{0}_Receiver') DROP QUEUE [{2}].[{0}_Receiver];
        PRINT N'EventReactor: Dropping queue {2}.[{0}_Sender].';
        IF EXISTS (SELECT * FROM sys.service_queues WITH (NOLOCK) WHERE schema_id = @schema_id AND name = N'{0}_Sender') DROP QUEUE [{2}].[{0}_Sender];

        PRINT N'EventReactor: Dropping contract {0}.';
        IF EXISTS (SELECT * FROM sys.service_contracts WITH (NOLOCK) WHERE name = N'{0}') DROP CONTRACT [{0}];
        PRINT N'EventReactor: Dropping messages.';
        {1}

        PRINT N'EventReactor: Dropping activation procedure {0}_QueueActivationSender.';
        IF EXISTS (SELECT * FROM sys.objects WITH (NOLOCK) WHERE schema_id = @schema_id AND name = N'{0}_QueueActivationSender') DROP PROCEDURE [{2}].[{0}_QueueActivationSender];";
    }
}