namespace EventSourcR.SqlServer
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; }
        public string EventsTableName { get; set; } = "Events";
        public string SchemaName { get; set; } = "dbo";
    }
}
