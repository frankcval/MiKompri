using Xunit;

// WebApplicationFactory hosts share Serilog's static bootstrap logger, as in
// the Users/ShoppingList suites. HTTP concurrency remains testable within a host.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
