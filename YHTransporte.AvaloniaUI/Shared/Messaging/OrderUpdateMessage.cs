using System.Collections;
using System.Collections.Generic;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

public sealed record OrderUpdateMessage(IEnumerable<string> Codes, Enums.ContextChangeType ChangeType) 
: UpdateMessage<string>(Codes, ChangeType);