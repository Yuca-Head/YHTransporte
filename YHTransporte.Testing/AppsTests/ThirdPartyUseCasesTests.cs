using System.Runtime.InteropServices;
using Moq;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.Core.Entities;

namespace YHTransporte.Testing.AppsTests;

public class ThirdPartyUseCasesTests
{
    private Mock<IThirdPartyRepository> RepositoryMock
    {get;} = new();
    public ThirdPartyUseCasesTests()
    {
        RepositoryMock = new();
        RepositoryMock
        .Setup(repository => repository.FindExistingNamesAsync(It.IsAny<IEnumerable<string>>()))
        .ReturnsAsync((IEnumerable<string> names, CancellationToken e) =>
        names.Where(name => _existingNames.Contains(name)));
    }
 
    private readonly HashSet<string> _existingNames = new(StringComparer.OrdinalIgnoreCase);

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
        CreateThirdPartyHandler handler = new(RepositoryMock.Object, validator);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsT1);

        RepositoryMock.Verify(r =>
        r.AddAsync(It.IsAny<IEnumerable<ThirdParty>>()), Times.Never);
    }

}