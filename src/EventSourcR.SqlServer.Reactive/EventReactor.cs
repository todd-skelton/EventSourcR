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

using EventSourcR.SqlServer.Reactive.Base;
using EventSourcR.SqlServer.Reactive.Base.Abstracts;
using EventSourcR.SqlServer.Reactive.Base.Enums;
using EventSourcR.SqlServer.Reactive.Base.Exceptions;
using EventSourcR.SqlServer.Reactive.Enumerations;
using EventSourcR.SqlServer.Reactive.Exceptions;
using EventSourcR.SqlServer.Reactive.Extensions;
using EventSourcR.SqlServer.Reactive.Messages;
using EventSourcR.SqlServer.Reactive.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EventSourcR.SqlServer.Reactive
{
    /// <summary>
    /// Monitor SQL Server event changes.
    /// </summary>
    public class EventReactor : TableDependency, IEventReactor
    {
        private readonly ITypeMapper _typeMapper;
        private readonly IEventSerializer _serializer;
        private readonly EventStoreOptions _options;
        private readonly Subject<IRecordedEvent> _eventSubject = new Subject<IRecordedEvent>();
        private readonly Subject<ReactorStatus> _statusSubject = new Subject<ReactorStatus>();
        private readonly Subject<Exception> _errorSubject = new Subject<Exception>();
        private Guid ConversationHandle;
        

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether activate database loging and event viewer loging.
        /// </summary>
        /// <remarks>
        /// Only a member of the sysadmin fixed server role or a user with ALTER TRACE permissions can use it.
        /// </remarks>
        /// <value>
        /// <c>true</c> if [activate database loging]; otherwise, <c>false</c>.
        /// </value>
        public bool ActivateDatabaseLoging { get; set; }

        /// <summary>
        /// Specifies the owner of the service to the specified database user.
        /// When a new service is created it is owned by the principal specified in the AUTHORIZATION clause. Server, database, and schema names cannot be specified. The service_name must be a valid sysname.
        /// When the current user is dbo or sa, owner_name may be the name of any valid user or role.
        /// Otherwise, owner_name must be the name of the current user, the name of a user that the current user has IMPERSONATE permission for, or the name of a role to which the current user belongs.
        /// </summary>
        public string ServiceAuthorization { get; set; }

        /// <summary>
        /// Specifies the SQL Server database user account under which the activation stored procedure runs.
        /// SQL Server must be able to check the permissions for this user at the time that the queue activates the stored procedure. For aWindows domain user, the server must be connected to the domain
        /// when the procedure is activated or when activation fails.For a SQL Server user, Service Broker always checks the permissions.EXECUTE AS SELF means that the stored procedure executes as the current user.
        /// </summary>
        public string QueueExecuteAs { get; set; } = "SELF";

        #endregion

        #region Streams

        public IObservable<IRecordedEvent> EventStream() => _eventSubject;

        public IObservable<IRecordedEvent> EventStream<T>() where T : IEvent => _eventSubject.Where(e => e.Data is T);

        public IObservable<IRecordedEvent> AggregateEventStream<T>() where T : IAggregate => _eventSubject.Where(e => e.AggregateType == _typeMapper.GetAggregateName<T>());

        public IObservable<IRecordedEvent> AggregateEventStream<T>(Guid id) where T : IAggregate => _eventSubject.Where(e => e.AggregateId == id);

        public IObservable<ReactorStatus> StatusStream() => _statusSubject;

        public IObservable<Exception> ErrorStream() => _errorSubject;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventReactor" /> class.
        /// </summary>
        public EventReactor(
            ITypeMapper mapper,
            IEventSerializer serializer,
            EventStoreOptions options,
            ITableDependencyFilter filter = null) : base(options.ConnectionString, options.EventsTableName, options.SchemaName, filter)
        {
            _typeMapper = mapper;
            _serializer = serializer;
            _options = options;

            Start();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts monitoring table's content changes.
        /// </summary>
        /// <param name="timeOut">The WAITFOR timeout in seconds.</param>
        /// <param name="watchDogTimeOut">The WATCHDOG timeout in seconds.</param>
        /// <returns></returns>
        /// <exception cref="NoSubscriberException"></exception>
        /// <exception cref="NoSubscriberException"></exception>
        public override void Start(int timeOut = 120, int watchDogTimeOut = 180)
        {
            _statusSubject.OnNext(ReactorStatus.Starting);

            base.Start(timeOut, watchDogTimeOut);

            _cancellationTokenSource = new CancellationTokenSource();

            _task = Task.Run(() => WaitForNotifications(_cancellationTokenSource.Token, timeOut, watchDogTimeOut), _cancellationTokenSource.Token);

            WriteTraceMessage(TraceLevel.Info, $"Waiting for receiving {_tableName}'s records change notifications.");
        }

        #endregion

        #region Protected virtual methods

        protected virtual string Spacer(int numberOrSpaces)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 1; i <= numberOrSpaces; i++) stringBuilder.Append(' ');
            return stringBuilder.ToString();
        }

        protected override string GetDataBaseName()
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            return sqlConnectionStringBuilder.InitialCatalog;
        }

        protected override string GetServerName()
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            return sqlConnectionStringBuilder.DataSource;
        }

        protected override string GetTableName(string tableName)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                return tableName.Replace("[", string.Empty).Replace("]", string.Empty);
            }

            return "Events";
        }

        protected override string GetSchemaName(string schemaName)
        {
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                return schemaName.Replace("[", string.Empty).Replace("]", string.Empty);
            }

            return "dbo";
        }

        protected virtual SqlServerVersion GetSqlServerVersion()
        {
            var sqlConnection = new SqlConnection(_connectionString);

            try
            {
                sqlConnection.Open();

                var serverVersion = sqlConnection.ServerVersion;
                if (string.IsNullOrWhiteSpace(serverVersion)) return SqlServerVersion.Unknown;

                var serverVersionDetails = serverVersion.Split(new[] { "." }, StringSplitOptions.None);
                var versionNumber = int.Parse(serverVersionDetails[0]);

                if (versionNumber < 8) return SqlServerVersion.Unknown;
                if (versionNumber == 8) return SqlServerVersion.SqlServer2000;
                if (versionNumber == 9) return SqlServerVersion.SqlServer2005;
                if (versionNumber == 10) return SqlServerVersion.SqlServer2008;
                if (versionNumber == 11) return SqlServerVersion.SqlServer2012;
            }
            catch
            {
                throw new SqlServerVersionNotSupportedException();
            }
            finally
            {
                sqlConnection.Close();
            }

            return SqlServerVersion.SqlServerLatest;
        }

        protected virtual bool CheckIfDatabaseObjectExists()
        {
            bool result;

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var sqlCommand = new SqlCommand($"SELECT COUNT(*) FROM sys.service_queues WITH (NOLOCK) WHERE name = N'{_dataBaseObjectsNamingConvention}';", sqlConnection);
                result = (int)sqlCommand.ExecuteScalar() > 0;
                sqlConnection.Close();
            }

            return result;
        }

        protected override void CreateDatabaseObjects(int timeOut, int watchDogTimeOut)
        {
            if (CheckIfDatabaseObjectExists() == false)
            {
                CreateSqlServerDatabaseObjects(watchDogTimeOut);
            }
            else
            {
                throw new DbObjectsWithSameNameException(_dataBaseObjectsNamingConvention);
            }
        }

        protected override string GetBaseObjectsNamingConvention()
        {
            var name = $"{_schemaName}_{_tableName}";
            return $"{name}_{Guid.NewGuid()}";
        }

        protected override void DropDatabaseObjects()
        {
            if (!_databaseObjectsCreated) return;

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    var dropAllScript = PrepareScriptDropAll($"IF EXISTS (SELECT * FROM sys.service_message_types WITH (NOLOCK) WHERE name = N'{_dataBaseObjectsNamingConvention}') DROP MESSAGE TYPE [{_dataBaseObjectsNamingConvention}];");

                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = dropAllScript;
                    sqlCommand.ExecuteNonQuery();
                }
            }

            WriteTraceMessage(TraceLevel.Info, "DropDatabaseObjects method executed.");
        }

        protected override void CheckRdbmsDependentImplementation()
        {
            var sqlVersion = GetSqlServerVersion();
            if (sqlVersion < SqlServerVersion.SqlServer2008) throw new SqlServerVersionNotSupportedException(sqlVersion);
        }

        protected virtual void CreateSqlServerDatabaseObjects(int watchDogTimeOut)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                using (var transaction = sqlConnection.BeginTransaction())
                {
                    var sqlCommand = new SqlCommand { Connection = sqlConnection, Transaction = transaction };

                    // Messages
                    sqlCommand.CommandText = $"CREATE MESSAGE TYPE [{_dataBaseObjectsNamingConvention}] VALIDATION = NONE;";
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Message {_dataBaseObjectsNamingConvention} created.");

                    // Contract
                    sqlCommand.CommandText = $"CREATE CONTRACT [{_dataBaseObjectsNamingConvention}] ([{_dataBaseObjectsNamingConvention}] SENT BY INITIATOR)";
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Contract {_dataBaseObjectsNamingConvention} created.");

                    // Queues
                    sqlCommand.CommandText = $"CREATE QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Receiver] WITH STATUS = ON, RETENTION = OFF, POISON_MESSAGE_HANDLING (STATUS = OFF);";
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Queue {_dataBaseObjectsNamingConvention}_Receiver created.");

                    sqlCommand.CommandText = $"CREATE QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Sender] WITH STATUS = ON, RETENTION = OFF, POISON_MESSAGE_HANDLING (STATUS = OFF);";
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Queue {_dataBaseObjectsNamingConvention}_Sender created.");

                    // Services
                    sqlCommand.CommandText = string.IsNullOrWhiteSpace(ServiceAuthorization)
                        ? $"CREATE SERVICE [{_dataBaseObjectsNamingConvention}_Sender] ON QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Sender];"
                        : $"CREATE SERVICE [{_dataBaseObjectsNamingConvention}_Sender] AUTHORIZATION [{ServiceAuthorization}] ON QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Sender];";
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Service broker {_dataBaseObjectsNamingConvention}_Sender created.");

                    sqlCommand.CommandText = string.IsNullOrWhiteSpace(ServiceAuthorization)
                        ? $"CREATE SERVICE [{_dataBaseObjectsNamingConvention}_Receiver] ON QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Receiver] ([{_dataBaseObjectsNamingConvention}]);"
                        : $"CREATE SERVICE [{_dataBaseObjectsNamingConvention}_Receiver] AUTHORIZATION [{ServiceAuthorization}] ON QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Receiver] ([{_dataBaseObjectsNamingConvention}]);";
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Service broker {_dataBaseObjectsNamingConvention}_Receiver created.");

                    // Activation Store Procedure
                    var dropAllScript = PrepareScriptDropAll($"IF EXISTS (SELECT * FROM sys.service_message_types WITH (NOLOCK) WHERE name = N'{_dataBaseObjectsNamingConvention}') DROP MESSAGE TYPE [{_dataBaseObjectsNamingConvention}];");
                    sqlCommand.CommandText = PrepareScriptProcedureQueueActivation(dropAllScript);
                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Procedure {_dataBaseObjectsNamingConvention} created.");

                    // Begin conversation
                    ConversationHandle = BeginConversation(sqlCommand);
                    WriteTraceMessage(TraceLevel.Verbose, $"Conversation with handler {ConversationHandle} started.");

                    sqlCommand.CommandText = string.Format(
                        SqlScripts.CreateTrigger,
                        _dataBaseObjectsNamingConvention,
                        $"[{_schemaName}].[{_tableName}]",
                        ConversationHandle);

                    sqlCommand.ExecuteNonQuery();
                    WriteTraceMessage(TraceLevel.Verbose, $"Trigger {_dataBaseObjectsNamingConvention} created.");

                    // Associate Activation Store Procedure to sender queue
                    sqlCommand.CommandText = $"ALTER QUEUE [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Sender] WITH ACTIVATION (PROCEDURE_NAME = [{_schemaName}].[{_dataBaseObjectsNamingConvention}_QueueActivationSender], MAX_QUEUE_READERS = 1, EXECUTE AS {QueueExecuteAs.ToUpper()}, STATUS = ON);";
                    sqlCommand.ExecuteNonQuery();

                    // Persist all objects
                    transaction.Commit();
                }

                _databaseObjectsCreated = true;

                WriteTraceMessage(TraceLevel.Info, $"All OK! Database objects created with naming {_dataBaseObjectsNamingConvention}.");
            }
        }

        protected virtual Guid BeginConversation(SqlCommand sqlCommand)
        {
            sqlCommand.CommandText = $"DECLARE @h AS UNIQUEIDENTIFIER; BEGIN DIALOG CONVERSATION @h FROM SERVICE [{_dataBaseObjectsNamingConvention}_Sender] TO SERVICE '{_dataBaseObjectsNamingConvention}_Receiver' ON CONTRACT [{_dataBaseObjectsNamingConvention}] WITH ENCRYPTION = OFF; SELECT @h;";
            var conversationHandler = (Guid)sqlCommand.ExecuteScalar();
            if (conversationHandler == Guid.Empty) throw new ServiceBrokerConversationHandlerInvalidException();

            return conversationHandler;
        }

        protected virtual string PrepareTriggerLogScript()
        {
            if (ActivateDatabaseLoging == false) return string.Empty;

            return
                Environment.NewLine + Environment.NewLine + "DECLARE @LogMessage VARCHAR(255);" + Environment.NewLine +
                $"SET @LogMessage = 'EventReactor: Message for ' + @dmlType + ' operation added in Queue [{_dataBaseObjectsNamingConvention}].'" + Environment.NewLine +
                "RAISERROR(@LogMessage, 10, 1) WITH LOG;";
        }

        protected virtual string PrepareScriptProcedureQueueActivation(string dropAllScript)
        {
            var script = string.Format(SqlScripts.CreateProcedureQueueActivation, _dataBaseObjectsNamingConvention, dropAllScript, _schemaName);
            return ActivateDatabaseLoging ? script : RemoveLogOperations(script);
        }

        protected virtual string PrepareScriptDropAll(string dropMessages)
        {
            var script = string.Format(SqlScripts.ScriptDropAll, _dataBaseObjectsNamingConvention, dropMessages, _schemaName);
            return ActivateDatabaseLoging ? script : RemoveLogOperations(script);
        }

        protected virtual string RemoveLogOperations(string source)
        {
            while (true)
            {
                var startPos = source.IndexOf("PRINT N'EventReactor:", StringComparison.InvariantCultureIgnoreCase);
                if (startPos < 1) break;

                var endPos = source.IndexOf(".';", startPos, StringComparison.InvariantCultureIgnoreCase);
                if (endPos < 1) break;

                source = source.Substring(0, startPos) + source.Substring(endPos + ".';".Length);
            }

            return source;
        }

        protected virtual async Task WaitForNotifications(
            CancellationToken cancellationToken,
            int timeOut,
            int timeOutWatchDog)
        {
            WriteTraceMessage(TraceLevel.Verbose, "Get in WaitForNotifications.");

            var waitforSqlScript =
                $"BEGIN CONVERSATION TIMER ('{ConversationHandle.ToString().ToUpper()}') TIMEOUT = " + timeOutWatchDog + ";" +
                $"WAITFOR (RECEIVE [message_type_name], [message_body] FROM [{_schemaName}].[{_dataBaseObjectsNamingConvention}_Receiver]), TIMEOUT {timeOut * 1000};";

            _statusSubject.OnNext(ReactorStatus.Started);

            try
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    await sqlConnection.OpenAsync(cancellationToken);
                    WriteTraceMessage(TraceLevel.Verbose, "Connection opened.");
                    _statusSubject.OnNext(ReactorStatus.WaitingForNotification);
                    var serializer = new XmlSerializer(typeof(List<ReactiveEventEntity>));

                    while (true)
                    {
                        using (var sqlCommand = new SqlCommand(waitforSqlScript, sqlConnection))
                        {
                            sqlCommand.CommandTimeout = 0;
                            WriteTraceMessage(TraceLevel.Verbose, "Executing WAITFOR command.");

                            using (var reader = await sqlCommand.ExecuteReaderAsync(cancellationToken).WithCancellation(cancellationToken))
                            {
                                while (reader.Read())
                                {
                                    var type = reader.GetString(0);

                                    WriteTraceMessage(TraceLevel.Verbose, $"Received message type = {type}.");

                                    if (type == SqlMessageTypes.ErrorType) throw new QueueContainingErrorMessageException();

                                    WriteTraceMessage(TraceLevel.Verbose, "Message ready to be notified.");
                                    using (var eventsStream = reader.GetStream(1))
                                    {
                                        var readerEvents = serializer.Deserialize(eventsStream) as List<ReactiveEventEntity>;

                                        foreach(var readerEvent in readerEvents)
                                        {
                                            _eventSubject.OnNext(Transform(readerEvent));
                                        }
                                    }
                                    WriteTraceMessage(TraceLevel.Verbose, "Message notified.");
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _statusSubject.OnNext(ReactorStatus.StopDueToCancellation);
                WriteTraceMessage(TraceLevel.Info, "Operation canceled.");
            }
            catch (AggregateException aggregateException)
            {
                _statusSubject.OnNext(ReactorStatus.StopDueToError);
                if (cancellationToken.IsCancellationRequested == false) _errorSubject.OnNext(aggregateException.InnerException);
                WriteTraceMessage(TraceLevel.Error, "Exception in WaitForNotifications.", aggregateException.InnerException);
            }
            catch (SqlException sqlException)
            {
                _statusSubject.OnNext(ReactorStatus.StopDueToError);
                if (cancellationToken.IsCancellationRequested == false) _errorSubject.OnNext(sqlException);
                WriteTraceMessage(TraceLevel.Error, "Exception in WaitForNotifications.", sqlException);
            }
            catch (Exception exception)
            {
                _statusSubject.OnNext(ReactorStatus.StopDueToError);
                if (cancellationToken.IsCancellationRequested == false) _errorSubject.OnNext(exception);
                WriteTraceMessage(TraceLevel.Error, "Exception in WaitForNotifications.", exception);
            }
        }

        private IRecordedEvent Transform(ReactiveEventEntity entity)
        {
            return new RecordedEvent(
                    entity.EventNumber,
                    entity.EventId,
                    entity.EventType,
                    entity.AggregateId,
                    entity.AggregateType,
                    entity.AggregateVersion,
                    _serializer.Deserialize(entity.SerializedData, _typeMapper.GetEventType(entity.EventType)) as IEvent,
                    _serializer.Deserialize(entity.SerializedMetadata, _typeMapper.GetMetadataType(entity.EventType)) as IMetadata,
                    DateTimeOffset.Parse(entity.Recorded)
                );
        }
    }
    #endregion
}