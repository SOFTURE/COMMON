### SOFTURE

### Configuration - TBD

...

### Extracting settings from appsettings.json
```csharp
 private readonly MySettings _mySettings;

public MyController(IOptions<MySettings> options)
{
    _mySettings = options.Value;
}
```