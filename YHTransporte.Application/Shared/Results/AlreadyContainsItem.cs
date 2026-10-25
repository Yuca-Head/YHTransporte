namespace YHTransporte.Application.Shared.Results;

/// <summary>
/// Use to notify that an object already contains other in its collection of repeated item type.
/// </summary>
public record AlreadyContainsItem<TOwner, TRepeated>(TOwner Owner, TRepeated Repeated);