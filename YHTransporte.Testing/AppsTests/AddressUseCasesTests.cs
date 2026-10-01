using Moq;
using OneOf.Types;
using Xunit.Abstractions;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Addresses.Results;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.Application.Addresses.UseCases.GetAddress;
using YHTransporte.Application.Shared.Results;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.Core.Entities;

namespace YHTransporte.Testing.AppsTests;

public class CreateAddressHandlerTests
{
    private readonly Mock<IAddressRepository> _repository = new();
    private readonly CreateAddressHandler _handler;

    private readonly ITestOutputHelper _output;

    public CreateAddressHandlerTests(ITestOutputHelper output)
    {
        _handler = new CreateAddressHandler(_repository.Object);
        _output = output;
    }


    // ============================================================
    // Address
    // ============================================================

    [Fact]
    public async Task HandleAddress_WhenCommandsAreValid_ReturnsSuccess()
    {
        var commands = new[]
        {
            new AddressKey("Casa", 1),
            new AddressKey("Oficina", 1)
        };

        _repository
            .Setup(r => r.GetAddressesByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _repository
            .Setup(r => r.GetMunicipalitiesByIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Municipality("Managua", new("Managua"){Key = 1}){Key = 1}
            ]);

        var result = await _handler.HandleAddress(commands);

        Assert.True(result.IsT0);
        Assert.IsType<Success>(result.AsT0);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<IEnumerable<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task HandleAddress_WhenNameIsEmpty_ReturnsValidationError()
    {
        var commands = new[]
        {
            new AddressKey("", 1)
        };

        var result = await _handler.HandleAddress(commands);

        Assert.True(result.IsT1);
        Assert.IsType<ValidationError>(result.AsT1);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<IEnumerable<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddressContextWorks()
    {
        // Given
        var commands = new[]
        {
            new Address("Casa", new("Managua", new("Managua"))){Key = 1},
            new Address("Oficina", new("Managua", new("Managua"))){Key = 2}
        };

        _repository
            .Setup(r => r.GetByKeysAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IEnumerable<int> ids, CancellationToken _) => [commands.FirstOrDefault(x => x.Key == ids.First())]
            );
        _repository.Setup(r => r.GetEverythingAsync(It.IsAny<CancellationToken>())).
        ReturnsAsync((CancellationToken _)=> commands);
        AddressContext context = new(_handler, new(_repository.Object));
    
        // When
        var result = await context.GetAddressesByKeysAsync([new (2)]);
    
        // Then
        Assert.True(result.IsT0);
        //result.AsT0.Value.ToList().ForEach(x => _output.WriteLine(x.Name));

        Assert.NotNull(result.AsT0.Value);
        Assert.Equal(2, result.AsT0.Value.First().Id);
        Assert.Equal(2, context.Addresses.ToList()[1].Id);  

        _repository.Verify(r => r.GetByKeysAsync(It.IsAny<IEnumerable<int>>()), Times.Never());
    }


    [Fact]
    public async Task HandleAddress_WhenCommandsAreRepeated_ReturnsRepeatedValue()
    {
        var commands = new[]
        {
            new AddressKey("Casa", 1),
            new AddressKey("casa", 1)
        };

        var result = await _handler.HandleAddress(commands);

        Assert.True(result.IsT3);
        Assert.IsType<
            RepeatedValue<
                IEnumerable<
                    RepeatedValue<AddressKey>.RepeatedKeyInformation
                >
            >
        >(result.AsT3);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<IEnumerable<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task HandleAddress_WhenMunicipalityDoesNotExist_ReturnsMunicipalityNotFound()
    {
        var commands = new[]
        {
            new AddressKey("Casa", 99)
        };

        _repository
            .Setup(r => r.GetAddressesByNamesAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _repository
            .Setup(r => r.GetMunicipalitiesByIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAddress(commands);

        Assert.True(result.IsT4);
        Assert.IsType<MunicipalityNotFound>(result.AsT4);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<IEnumerable<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task HandleAddress_WhenAddressAlreadyExistsInSameMunicipality_ReturnsAlreadyExists()
    {
        var command = new AddressKey(
            "Calle Principal",
            1
        );

        var existingAddress = new Address(
            "Calle Principal",
            new Municipality("Managua", new("Managua")){Key = 1}
        );

        _repository
            .Setup(r => r.GetAddressesByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingAddress]);

        _repository
            .Setup(r => r.GetMunicipalitiesByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingAddress.Municipality]);

        
        var result = await _handler.HandleAddress([command]);

        Assert.True(result.IsT2);
        Assert.IsType<AlreadyExists<IEnumerable<(string Details, string Municipality)>>>(result.AsT2);

        Assert.Contains(
            ("Calle Principal", "Managua"),
            result.AsT2.Argument);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<IEnumerable<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task HandleAddress_WhenAddressExistsButInDifferentMunicipality_ReturnsSuccess()
    {
        var commands = new[]
        {
            new AddressKey("Casa", 1)
        };

        var existingAddress = new Address(
            "Casa",
            new Municipality("León", new("Managua"){Key = 1}){Key = 2}
        );

        _repository
            .Setup(r => r.GetAddressesByNamesAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingAddress]);

        _repository
            .Setup(r => r.GetMunicipalitiesByIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Municipality("Managua", new("Managua"){Key = 1}){Key = 1}
            ]);

        var result = await _handler.HandleAddress(commands);

        Assert.True(result.IsT0);
        Assert.IsType<Success>(result.AsT0);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<IEnumerable<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    // ============================================================
    // Municipality
    // ============================================================

    [Fact]
    public async Task HandleMunicipality_WhenCommandsAreValid_ReturnsSuccess()
    {
        var commands = new[]
        {
            new AddressKey("Managua", 1),
            new AddressKey("León", 1)
        };

        _repository
            .Setup(r => r.GetMunicipalitiesByNameAsync(
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);

        _repository
            .Setup(r => r.GetDepartmentsByIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Department("Managua"){Key = 1}
            ]);

        var result = await _handler.HandleMunicipality(commands);

        Assert.True(result.IsT0);
        Assert.IsType<Success>(result.AsT0);

        _repository.Verify(
            r => r.AddMunicipalitiesAsync(
                It.IsAny<IEnumerable<Municipality>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task HandleMunicipality_WhenDepartmentDoesNotExist_ReturnsDepartmentNotFound()
    {
        var commands = new[]
        {
            new AddressKey("Managua", 99)
        };

        _repository
            .Setup(r => r.GetMunicipalitiesByNameAsync(
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);

        _repository
            .Setup(r => r.GetDepartmentsByIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleMunicipality(commands);

        Assert.True(result.IsT4);
        Assert.IsType<DepartmentNotFound>(result.AsT4);

        _repository.Verify(
            r => r.AddMunicipalitiesAsync(
                It.IsAny<IEnumerable<Municipality>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    // ============================================================
    // Department
    // ============================================================

    [Fact]
    public async Task HandleDepartment_WhenCommandsAreValid_ReturnsSuccess()
    {
        var commands = new[]
        {
            new AddressKey("Managua", -1),
            new AddressKey("León", -1)
        };

        _repository
            .Setup(r => r.GetDepartmentsByNamesAsync(
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleDepartment(commands);

        Assert.True(result.IsT0);
        Assert.IsType<Success>(result.AsT0);

        _repository.Verify(
            r => r.AddDepartmentsAsync(
                It.IsAny<IEnumerable<Department>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task HandleDepartment_WhenDepartmentAlreadyExists_ReturnsAlreadyExists()
    {
        var commands = new[]
        {
            new AddressKey("Managua", -1)
        };

        _repository
            .Setup(r => r.GetDepartmentsByNamesAsync(
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Department("Managua"){Key = 1}
            ]);

        var result = await _handler.HandleDepartment(commands);

        Assert.True(result.IsT2);
        Assert.IsType<AlreadyExists<IEnumerable<string>>>(result.AsT2);

        Assert.Contains("Managua", result.AsT2.Argument);

        _repository.Verify(
            r => r.AddDepartmentsAsync(
                It.IsAny<IEnumerable<Department>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    // ============================================================
    // Constructor
    // ============================================================

    [Fact]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CreateAddressHandler(null!));
    }
}