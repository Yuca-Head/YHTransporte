using Xunit.Abstractions;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties;
using YHTransporte.Testing.Database;

namespace YHTransporte.Testing.OutputsTests;


public class DataBaseQueryTests
{
    private readonly ITestOutputHelper _output;
    public DataBaseQueryTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public async Task Parties_FromRepository_LoadFine()
    {
        // Given
        var repo = new SqlServerThirdPartyRepository(new(Connections.GetConfigurationRoot()));
        // When
        
        var parties = await repo.GetEverythingAsync();
        // Then

        foreach(var party in parties)
            _output.WriteLine(party.Name);

        Assert.True(parties.Any());
    }

    [Fact]
    public async Task PartyQueryHandlerFlies()
    {
        // Given
        var repo = new SqlServerThirdPartyRepository(new(Connections.GetConfigurationRoot()));
        var addressRepo = new SqlServerAddressRepository(new(Connections.GetConfigurationRoot()));
        var handler = new GetThirdPartyHandler(repo, addressRepo);
        // When
        var parties = (await handler.GetThirdPartyDetails([new(1), new(2)])).AsT0.Value.ToList();
        // Then

        parties.ForEach(x => _output.WriteLine($"Name: {x.Name} Extra: {x.Addresses.Select(y =>
        $"Address: {y.Name} Municipality: {y.Municipality.Name} Department: {y.Municipality.Department.Name}").First()}"));
    }
    
    [Fact]
    public async Task GetPartiesAddressesWorks()
    {
        // Given
        var repo = new SqlServerAddressRepository(new(Connections.GetConfigurationRoot())); 
        // When
        var pairs = (await repo.GetAddressesFromThirdParties([1, 2])).ToList();
    
        // Then

        pairs.ForEach(x => _output.WriteLine($"\n\nOwner: {x.ThirdPartyId}, {x.Addresses.Select(y =>
        $"Address: {y.Details} Municipality: {y.Municipality.Name} Department: {y.Municipality.Department.Name}").First()}"));
    }

}