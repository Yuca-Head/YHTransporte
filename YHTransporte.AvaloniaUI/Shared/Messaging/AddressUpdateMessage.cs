using System.Collections.Generic;
using YHTransporte.Application.Addresses.Dto;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

public sealed record AddressUpdateMessage
(IEnumerable<int> Keys, Enums.ContextChangeType ChangeType) :
UpdateMessage<int>(Keys, ChangeType);