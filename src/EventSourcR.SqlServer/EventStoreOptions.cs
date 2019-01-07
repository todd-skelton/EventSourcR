namespace EventSourcR.SqlServer
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; }
        public string EventsTableName { get; set; } = "Events";
    }
}
