using System.Collections.Generic;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

/// <summary>
/// Mensaje para cualquier cambio en conductores (creación, modificación o cambio de "En uso").
/// Las claves son los códigos (DRV-0000); si son null el contexto recarga todo.
/// </summary>
public sealed record DriverUpdateMessage
(IEnumerable<string>? Codes, Enums.ContextChangeType ChangeType) : UpdateMessage<string>(Codes, ChangeType);
