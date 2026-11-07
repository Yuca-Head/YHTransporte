
using System.Collections.Generic;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;

/// <summary>
/// Un mensaje para cualquier actualización de envío. Las claves pueden ser nulas, lo que indica al contexto que recargue todo.
/// </summary>
public sealed record ShipmentUpdateMessage
(IEnumerable<string>? Keys, Enums.ContextChangeType ChangeType) : UpdateMessage<string>(Keys, ChangeType);