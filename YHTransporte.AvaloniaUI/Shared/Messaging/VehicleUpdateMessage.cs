using System.Collections.Generic;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

/// <summary>
/// Mensaje para cualquier cambio en vehículos (creación, modificación o cambio de "En uso").
/// Si las claves son null el contexto recarga todo.
/// </summary>
public sealed record VehicleUpdateMessage
(IEnumerable<int>? Keys, Enums.ContextChangeType ChangeType) : UpdateMessage<int>(Keys, ChangeType);
