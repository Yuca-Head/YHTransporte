using YHTransporte.Core.Entities;
using YHTransporte.Core.Exceptions;

namespace YHTransporte.Testing.DomainTests;

public class DataValidations
{
    public static IEnumerable<object[]> AddressData
    => 
    [
        ["", "Dist-7", "Managua"],
        ["Street 13", "", "Granada"],
        ["From La Chelita Three Vars Squares", "Ticuantepec", ""],
        [null!, null!, null!]
    ];

    [Theory]
    [MemberData(nameof(AddressData))]
    public void Address_Validation_IsWorking(string details, string municipalityName, string departmentName)
    => Assert.Throws<AddressException>(
        ()=> new Address(details, new(municipalityName, new(departmentName)))
    );
    
}