# SOFTURE COMMON

A collection of shared .NET libraries providing foundational building blocks for domain-driven design, result handling, contracts, settings management, and common language primitives.

Supported frameworks: `net6.0`, `net8.0`, `net10.0`

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

### SOFTURE.Domain — Repository Interfaces

#### `IBaseRepository<TEntity, TIdentifier>` — full CRUD repository for aggregate roots

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetAsync(TIdentifier id)` | `Task<Result<TEntity>>` | Get entity by identifier |
| `AddAsync(TEntity entity)` | `Task<Result<TEntity>>` | Add a new entity |
| `UpdateAsync(TEntity entity)` | `Task<Result<TEntity>>` | Update an existing entity |
| `RemoveAsync(TEntity entity)` | `Task<Result>` | Remove an entity |

#### `IReadBaseRepository<TEntity, TIdentifier>` — read-only repository

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetAsync(TIdentifier id)` | `Task<Maybe<TEntity>>` | Get entity by identifier |
| `GetAllAsync()` | `Task<Maybe<IEnumerable<TEntity>>>` | Get all entities |
| `FindAsync(predicate)` | `Task<Maybe<IEnumerable<TEntity>>>` | Find entities matching a predicate |
| `PaginateAsync(page, elements, predicate)` | `Task<Maybe<IEnumerable<TEntity>>>` | Paginate entities with a predicate |
| `CountAsync(predicate?)` | `Task<Maybe<int>>` | Count entities with an optional predicate |
| `ProjectAsync(predicate, selector, limit?)` | `Task<IReadOnlyList<TResult>>` | Project entities matching a predicate into `TResult`, optionally capped at `limit` |
| `ProjectAsync(predicate, selector, orderBy, limit?)` | `Task<IReadOnlyList<TResult>>` | Same as above, ordered ascending by `orderBy` before the cap is applied |

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

## Releasing a package

Every package is versioned and released on its own by the single workflow
[`release.yml`](.github/workflows/release.yml). One run releases one package to nuget.org and creates a
GitHub Release with the `.nupkg` attached.

| Short name | Package |
|------------|---------|
| `contract` | `SOFTURE.Contract.Common` |
| `domain`   | `SOFTURE.Domain` |
| `language` | `SOFTURE.Language.Common` |
| `results`  | `SOFTURE.Results` |
| `settings` | `SOFTURE.Settings` |

### Release

A release starts from a tag `<package>@<version>` (e.g. `results@0.5.1`) created by hand; the workflow never
creates tags. In GitHub: Releases → **Draft a new release** → **Choose a tag** → type `results@0.5.1` →
**Create new tag on publish**, target `master` → **Publish release**. The new tag starts the workflow, which
publishes the package to nuget.org and attaches the `.nupkg` to that release. Pushing the tag from a
terminal works the same way (the workflow then creates the GitHub Release itself):

```bash
git tag -a results@0.5.1 -m results@0.5.1
git push origin results@0.5.1
```

The tag must point at a commit on `master`. A version with a suffix (`0.6.0-beta.1`) is published as a
prerelease. To retry a failed release, use **Run workflow** with the tag selected as the ref; the tag decides
the package and version.

### Dry run

Actions → **release** → **Run workflow** on a branch, pick the package: build, vulnerability gate and pack,
nothing published.

### What a release checks

1. The commit is on `master`.
2. The whole solution builds for `net6.0`, `net8.0` and `net10.0`.
3. `dotnet list package --vulnerable --include-transitive` finds nothing for the released package.
4. The package contains `lib/net6.0`, `lib/net8.0` and `lib/net10.0`.

A pull request that changes `release.yml` or a `.csproj` runs the same checks for every package as a dry run.

### Order of releases

A package that depends on another COMMON package references a version that is already on nuget.org,
because the release restores the whole solution. Release the dependency first, wait until nuget.org lists
it (a few minutes), then raise the reference and release the dependent package.

### Trusted publishing (one-time setup)

The workflow publishes through [nuget.org trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing):
GitHub's OIDC token is exchanged for an API key that lives one hour. No long-lived API key is stored.

1. nuget.org → your user → **Trusted Publishing** → add a policy:
   - Package Owner: `SOFTURE`
   - Repository Owner: `SOFTURE`, Repository: `COMMON`
   - Workflow File: `release.yml`
   - Environment: empty
2. GitHub → SOFTURE/COMMON → Settings → Secrets and variables → Actions → secret `NUGET_USER` with the
   nuget.org **profile name** (not the e-mail) of the user who created the policy.

Keep the workflow file name: the policy is bound to `release.yml`. The old `NUGET_API_KEY` secret is no
longer used.
