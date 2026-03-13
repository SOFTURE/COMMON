# SOFTURE COMMON

A collection of shared .NET libraries providing foundational building blocks for domain-driven design, result handling, contracts, settings management, and common language primitives.

Supported frameworks: `net6.0`, `net8.0`, `net9.0`, `net10.0`

## Projects

### SOFTURE.Domain
Domain-driven design library providing base entity and repository abstractions. Includes `Entity<TIdentifier>` with domain event support, `AuditableEntity<TIdentifier>` with audit tracking, `StatefulEntity<TIdentifier, TState, TAction>` implementing the state machine pattern, `IAggregateRoot` marker interface, and repository contracts (`IBaseRepository`, `IReadBaseRepository`) for entity persistence.

### SOFTURE.Results
Functional error handling library built on top of `CSharpFunctionalExtensions`. Provides the `Error` record type with formatting/parsing support and predefined `CommonErrors` constants for consistent error representation across the solution.

### SOFTURE.Contract.Common
Common request/response contracts and messaging infrastructure. Includes `RequestBase`, `GetRequestBase` (with query string generation), `PostRequestBase`, `DeleteRequestBase`, `PaginationRequestBase` (with page/element properties), and the `IMessage` marker interface for messaging patterns.

### SOFTURE.Settings
Configuration management extensions for `IServiceCollection`. Provides the `GetSettings<TSettings, TResult>` extension method for resolving, binding, and selecting settings from the dependency injection container using `IOptions<T>`.

### SOFTURE.Language.Common
Primitive language constructs and assembly markers. Includes `IdentifierBase<T>` abstract record for strongly-typed identifiers, `IIdentifier` marker interface, and `IAssemblyMarker` for assembly discovery.

## Usage

### SOFTURE.Language.Common
```csharp
public sealed record UserId(Guid Value) : IdentifierBase<Guid>(Value);
```

### SOFTURE.Domain
```csharp
public sealed class User : AuditableEntity<UserId>, IAggregateRoot
{
    public string Name { get; private set; }
    public string Email { get; private set; }
}

public interface IUserRepository : IBaseRepository<User, UserId> { }

public interface IUserReadRepository : IReadBaseRepository<User, UserId> { }
```

### SOFTURE.Domain — StatefulEntity
```csharp
public sealed class Order : StatefulEntity<OrderId, OrderState, OrderAction>
{
    public Order()
    {
        StateMachine.Configure(OrderState.New)
            .Permit(OrderAction.Confirm, OrderState.Confirmed);

        StateMachine.Configure(OrderState.Confirmed)
            .Permit(OrderAction.Ship, OrderState.Shipped);
    }

    public Result Confirm() => PerformIfPossible(
        OrderAction.Confirm,
        new Error("Order.CannotConfirm", "Order cannot be confirmed."));
}
```

### SOFTURE.Results
```csharp
public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User was not found.");
    public static readonly Error AlreadyExists = new("User.AlreadyExists", "User already exists.");
}

var error = Error.Parse("User.NotFound|User was not found.");
string message = UserErrors.NotFound.Build();
```

### SOFTURE.Contract.Common
```csharp
public sealed class GetUsersRequest : PaginationRequestBase
{
    public string? Search { get; set; }
}

public sealed class CreateUserRequest : PostRequestBase
{
    public string Name { get; set; }
    public string Email { get; set; }

    public override void Clear()
    {
        Name = string.Empty;
        Email = string.Empty;
    }
}
```

### SOFTURE.Settings
```csharp
var connectionString = services.GetSettings<DatabaseSettings, string>(s => s.ConnectionString);
```

```csharp
private readonly MySettings _mySettings;

public MyController(IOptions<MySettings> options)
{
    _mySettings = options.Value;
}
```
