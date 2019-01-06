namespace EventSourcR
{
    /// <summary>
    /// A command.
    /// </summary>
    /// <typeparam name="T">The type of aggregate the command belongs to.</typeparam>
    public interface ICommand<T> where T : IAggregate<T>
    {

    }
}
