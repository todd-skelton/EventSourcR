using EventSourcR;
using EventSourcR.EntityFrameworkCore;
using EventSourcR.JsonEventSerializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsoleApp
{
    public class EfCoreEventStoreFactory : IDesignTimeDbContextFactory<EventStore>
    {
        public EventStore CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<EventStore>().UseSqlServer("Server=(local);Database=ConsoleApp;Trusted_Connection=true", x => x.MigrationsAssembly("EventSourcR.EntityFrameworkCore.SqlServer"));
            return new EventStore(new TypeMapper(), new JsonEventSerializer(), options.Options);
        }
    }
}
