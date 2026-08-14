using Xunit;

namespace Vitabu.Api.Tests;

[CollectionDefinition("Api")]
public sealed class ApiCollection : ICollectionFixture<VitabuWebApplicationFactory>;
