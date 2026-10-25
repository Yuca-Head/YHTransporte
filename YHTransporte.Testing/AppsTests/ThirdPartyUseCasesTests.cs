using System.Runtime.InteropServices;
using Moq;
using Xunit.Abstractions;
using Xunit.Sdk;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.Core.Entities;

namespace YHTransporte.Testing.AppsTests;

public class ThirdPartyUseCasesTests
{
    private Mock<IThirdPartyRepository> RepositoryMock
    {get;} = new();

    private Mock<IAddressRepository> AddressRepository
    {get;} = new();

    private ITestOutputHelper _output;
    private Dictionary<int, ThirdParty> _thirdParties = new()
    {
        {1, new("Pancho"){Key = 1}},
        {2, new("Pedro"){Key = 2}},
        {3, new("Juliana"){Key = 3}}
    };
    public ThirdPartyUseCasesTests(ITestOutputHelper testOutput)
    {
        RepositoryMock = new();
        _output = testOutput;
        RepositoryMock
        .Setup(repository => repository.FindExistingNamesAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync((IEnumerable<string> names, CancellationToken e) =>
        names.Where(name => _existingNames.Contains(name)));

        RepositoryMock.Setup(r => r.GetByKeysAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
        {
            List<ThirdParty> parties = [];
            foreach(var id in ids)
            {
                var party = _thirdParties.GetValueOrDefault(id);
                if(party is not null)
                    parties.Add(party);
            }
            return parties;
        }
        );
    }

 
    private readonly HashSet<string> _existingNames = new(StringComparer.OrdinalIgnoreCase);

    [Theory]
    [InlineData(1,2,5)]
    [InlineData(-4,5,59)]
    [InlineData(4,2,1)]
    public async Task GetParties_ReturnsNotFound_WhenAtLeastOne_DoesNotExist(params int[] ids)
    {
        var handler = new GetThirdPartyHandler(RepositoryMock.Object, AddressRepository.Object);

        var query = ids.Select(x => new GetThirdPartyQuery(x)).ToArray();

        var result = await handler.GetThirdPartyDetails(query);



        Assert.True(result.IsT1);
        Assert.Equivalent(ids.Except(_thirdParties.Select(x => x.Key)), result.AsT1.Select(x => x.Key));
        RepositoryMock.Verify(r => r.GetByKeysAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
        Times.Once());
    }

    [Theory]
    [InlineData(2,1,3)]
    [InlineData(1,2,3)]
    [InlineData(3,2,1)]
    [InlineData(1)]
    public async Task GetParties_ReturnsValues_WhenEverythingIs_AllRight(params int[] ids)
    {
        // Given    
        var handler = new GetThirdPartyHandler(RepositoryMock.Object, AddressRepository.Object);
        var query = ids.Select(x => new GetThirdPartyQuery(x)).ToArray();

        // When
        
        var result = await handler.GetThirdPartyDetails(query);
        
        // Then

        Assert.True(result.IsT0);
        Assert.Equivalent(ids, result.AsT0.Value.Select(x => x.Key));
        Assert.Equal(result.AsT0.Value.Count(), ids.Length);
        RepositoryMock.Verify(r => r.GetByKeysAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
        Times.Once());
    }
    

    [Theory]
    [InlineData("empresa A")]
    [InlineData("EMPRESA B")]
    [InlineData("empREsa c")]
    public async Task Validate_WhenNameAlreadyExists_ReturnsAlreadyExists(string repeatedName)
    {
        // Arrange
        _existingNames.Add(repeatedName);

        var validator = new CreateThirdPartyValidator(
            RepositoryMock.Object);

        var commands = new[]
        {
            new CreateThirdPartyCommand(repeatedName.ToLower())
        };

        // Act
        var result = await validator.Validate(commands);

        // Assert
        Assert.False(result.IsT0);
        Assert.True(result.IsT1);
        Assert.False(result.IsT2);
        Assert.IsType<IEnumerable<string>>(
        result.AsT1.Argument, exactMatch: false);
    }


    public static TheoryData<CreateThirdPartyCommand, List<string>> ValidationData 
    => new()
    {
        {new("TIENDICA"), ["Papita", "tiendica"]},
        {new("Don señor"), ["American Pais", "don seÑOr"]}
    };

    [Theory]
    [MemberData(nameof(ValidationData))]
    public async Task NeverCalls_AddToRepository_WhenValidationFails
    (CreateThirdPartyCommand command, List<string> names)
    {
        // Arrange
        names.ForEach(n => _existingNames.Add(n));

        CreateThirdPartyValidator validator = new(RepositoryMock.Object);
        CreateThirdPartyHandler handler = new(RepositoryMock.Object);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsT1);

        RepositoryMock.Verify(r =>
        r.AddAsync(It.IsAny<IEnumerable<ThirdParty>>()), Times.Never);
    }

}