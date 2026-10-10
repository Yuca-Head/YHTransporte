using System.Collections.Generic;
using YHTransporte.Application.Addresses.Dto;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

/// <summary>
/// A message for any address update
/// </summary>
/// <param name="Keys"></param>
/// <param name="ChangeType"></param>
/// <remarks>
/// File name: AddressTypesUpdateMessage
/// </remarks>
public sealed record AddressUpdateMessage
(IEnumerable<int> Keys, Enums.ContextChangeType ChangeType) :
UpdateMessage<int>(Keys, ChangeType);

/// <summary>
/// A message for any Municiaplity update
/// </summary>
/// <param name="Keys"></param>
/// <param name="ChangeType"></param>
/// <remarks>
/// File name: AddressTypesUpdateMessage
/// </remarks>
public sealed record MuinicipalityUpdateMessage
(IEnumerable<int> Keys, Enums.ContextChangeType ChangeType) :
UpdateMessage<int>(Keys, ChangeType);

/// <summary>
/// A message for any Department update
/// </summary>
/// <param name="Keys"></param>
/// <param name="ChangeType"></param>
/// <remarks>
/// File name: AddressTypesUpdateMessage
/// </remarks>
public sealed record DepartmentUpdateMessage
(IEnumerable<int> Keys, Enums.ContextChangeType ChangeType) :
UpdateMessage<int>(Keys, ChangeType);   