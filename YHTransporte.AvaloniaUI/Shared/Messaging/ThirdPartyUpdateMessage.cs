using System;
using System.Collections.Generic;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

public sealed record ThirdPartyUpdateMessage
(IEnumerable<int>? Keys, Enums.ContextChangeType ChangeType) : UpdateMessage<int>(Keys, ChangeType);