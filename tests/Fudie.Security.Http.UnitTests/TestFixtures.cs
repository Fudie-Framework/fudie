namespace Fudie.Security.Http.UnitTests.FakeAggregate
{
    /// <summary>
    /// Public IAggregateDescription with parameterless constructor.
    /// Used by BuildAggregateDescriptions to exercise the full pipeline (line 42).
    /// </summary>
    public class FakeAggregateDescription : IAggregateDescription
    {
        public string Id => "fake";
        public string DisplayName => "Fake";
        public string? Icon => null;
        public string ReadDescription => "Read";
        public string WriteDescription => "Write";
    }
}

namespace Fudie.Security.Http.UnitTests.NoCtorAggregate
{
    /// <summary>
    /// Public IAggregateDescription WITHOUT parameterless constructor.
    /// Exercises the <c>GetConstructor(Type.EmptyTypes) is not null</c> branch (false case).
    /// </summary>
    public class NoCtorAggregateDescription : IAggregateDescription
    {
        public NoCtorAggregateDescription(string required) => Id = required;
        public string Id { get; }
        public string DisplayName => "NoCtor";
        public string? Icon => null;
        public string ReadDescription => "Read";
        public string WriteDescription => "Write";
    }
}
