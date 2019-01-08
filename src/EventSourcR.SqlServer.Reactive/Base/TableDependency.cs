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

using EventSourcR.SqlServer.Reactive.Base.Abstracts;
using EventSourcR.SqlServer.Reactive.Base.Enums;
using EventSourcR.SqlServer.Reactive.Base.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcR.SqlServer.Reactive.Base
{
    public abstract class TableDependency : ITableDependency
    {
        #region Instance Variables

        protected CancellationTokenSource _cancellationTokenSource;
        protected string _connectionString;
        protected string _tableName;
        protected string _schemaName;
        protected string _server;
        protected string _database;
        protected Task _task;
        protected ReactorStatus _status;
        protected ITableDependencyFilter _filter;
        protected bool _disposed;
        protected string _dataBaseObjectsNamingConvention;
        protected bool _databaseObjectsCreated;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the trace switch.
        /// </summary>
        /// <value>
        /// The trace switch.
        /// </value>
        public TraceLevel TraceLevel { get; set; } = TraceLevel.Off;

        /// <summary>
        /// Gets or Sets the TraceListener.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public TraceListener TraceListener { get; set; }

        /// <summary>
        /// Gets or sets the culture info.
        /// </summary>
        /// <value>
        /// The culture information five letters iso code.
        /// </value>
        public CultureInfo CultureInfo { get; set; } = new CultureInfo("en-US");

        /// <summary>
        /// Gets or sets the encoding use to convert database strings.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Return the database objects naming convention for created objects used to receive notifications. 
        /// </summary>
        /// <value>
        /// The data base objects naming.
        /// </value>
        public string DataBaseObjectsNamingConvention => new string(_dataBaseObjectsNamingConvention.ToCharArray());

        /// <summary>
        /// Gets the EventReactor status.
        /// </summary>
        /// <value>
        /// The TableDependencyStatus enumeration status.
        /// </value>
        public ReactorStatus Status => _status;

        /// <summary>
        /// Gets name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName => _tableName;

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        /// <value>
        /// The name of the schema.
        /// </value>
        public string SchemaName => _schemaName;

        #endregion

        #region Constructors

        protected TableDependency(
            string connectionString,
            string tableName = null,
            string schemaName = null,
            ITableDependencyFilter filter = null)
        {
            _connectionString = connectionString;

            _tableName = GetTableName(tableName);
            _schemaName = GetSchemaName(schemaName);
            _server = GetServerName();
            _database = GetDataBaseName();

            CheckRdbmsDependentImplementation();

            _dataBaseObjectsNamingConvention = GetBaseObjectsNamingConvention();
            _filter = filter;
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
        public virtual void Start(int timeOut = 120, int watchDogTimeOut = 180)
        {
            if (timeOut < 60) throw new ArgumentException("timeOut must be greater or equal to 60 seconds");
            if (watchDogTimeOut < 60 || watchDogTimeOut < (timeOut + 60)) throw new WatchDogTimeOutException("watchDogTimeOut must be at least 60 seconds bigger then timeOut");

            if (_task != null)
            {
                Trace.TraceInformation("Already called Start() method.");
                return;
            }

            _disposed = false;
            CreateDatabaseObjects(timeOut, watchDogTimeOut);
        }

        /// <summary>
        /// Stops monitoring table's content changes.
        /// </summary>
        public virtual void Stop()
        {
            if (_task != null)
            {
                _cancellationTokenSource.Cancel(true);
                _task?.Wait();
            }

            _task = null;

            DropDatabaseObjects();

            _disposed = true;

            WriteTraceMessage(TraceLevel.Info, "Stopped waiting for notification.");
        }

#if DEBUG
        public virtual void StopWithoutDisposing()
        {
            if (_task != null)
            {
                _cancellationTokenSource.Cancel(true);
                _task?.Wait();
            }

            _task = null;

            _disposed = true;

            WriteTraceMessage(TraceLevel.Info, "Stopped waiting for notification.");
        }
#endif

        #endregion

        #region Logging

        protected virtual string FormatTraceMessageHeader()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Server: {_server} Database: {_database}]";
        }

        protected string DumpException(Exception exception)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("EXCEPTION:");
            sb.AppendLine(exception.GetType().Name);
            sb.AppendLine(exception.Message);
            sb.AppendLine(exception.StackTrace);

            var innerException = exception.InnerException;
            if (innerException != null) AddInnerException(sb, innerException);

            return sb.ToString();
        }

        protected static void AddInnerException(StringBuilder sb, Exception exception)
        {
            while (true)
            {
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine("INNER EXCEPTION:");
                sb.AppendLine(exception.GetType().Name);
                sb.AppendLine(exception.Message);
                sb.AppendLine(exception.StackTrace);

                var innerException = exception.InnerException;
                if (innerException != null)
                {
                    exception = innerException;
                    continue;
                }

                break;
            }
        }

        protected virtual void WriteTraceMessage(TraceLevel traceLevel, string message, Exception exception = null)
        {
            try
            {
                if (TraceListener == null) return;
                if (TraceLevel < TraceLevel.Off || TraceLevel > TraceLevel.Verbose) return;

                if (TraceLevel >= traceLevel)
                {
                    var messageToWrite = new StringBuilder(message);
                    if (exception != null) messageToWrite.Append(DumpException(exception));
                    TraceListener.WriteLine($"{FormatTraceMessageHeader()}{messageToWrite}");
                    TraceListener.Flush();
                }
            }
            catch
            {
                // Intentionally ignored
            }
        }

        #endregion

        #region Checks

        protected virtual void CheckRdbmsDependentImplementation() { }

        #endregion

        #region Get infos

        protected abstract string GetBaseObjectsNamingConvention();

        protected abstract string GetDataBaseName();

        protected abstract string GetServerName();

        protected abstract string GetTableName(string tableName);

        protected abstract string GetSchemaName(string schimaName);

        #endregion

        #region Database object generation/disposition

        protected abstract void CreateDatabaseObjects(int timeOut, int watchDogTimeOut);

        protected abstract void DropDatabaseObjects();

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();

                TraceListener?.Dispose();
            }

            _disposed = true;
        }

        ~TableDependency()
        {
            Dispose(false);
        }

        #endregion
    }
}