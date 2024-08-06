namespace SOFTURE.Language.Common;

public abstract record IdentifierBase<T>(T Value) : IIdentifier;