using System.Collections.Generic;

namespace YHTransporte.AvaloniaUI.Shared.Messaging;


/// <summary>
/// 
/// </summary>
/// <typeparam name="T">Key</typeparam>
/// <param name="Values">Changed Keys</param>
/// <param name="ChangeType"></param>
public abstract record UpdateMessage<T>(IEnumerable<T>? Values, Enums.ContextChangeType ChangeType);