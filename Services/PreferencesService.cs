using System.Text.Json;
using finora_dashboard.Models;
using Microsoft.JSInterop;

namespace finora_dashboard.Services;

public sealed class PreferencesService(IJSRuntime js)
{
    public const string StorageKey = "finora.preferences.v1";
    public const string AppVersion = "1.0.0";

    private bool _loaded;
    private UserPreferencesDocument _document = CreateDefault();

    public bool IsReady => _loaded;
    public bool LastWriteFailed { get; private set; }
    public event Action? Changed;

    public DemoProfile Profile => new()
    {
        DisplayName = _document.DisplayName,
        City = _document.City
    };

    public AppTheme Theme => _document.Theme;
    public SarDisplayFormat SarFormat => _document.SarFormat;
    public DateDisplayFormat DateFormat => _document.DateFormat;

    public UserPreferencesDocument ExportDocument() => Clone(_document);

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            var json = await js.InvokeAsync<string?>("finora.storageGet", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var document = JsonSerializer.Deserialize<UserPreferencesDocument>(json, FinoraJson.Options);
                if (document is not null)
                {
                    _document = Normalize(document);
                }
            }
        }
        catch (JSException)
        {
            // Keep defaults when storage is unavailable.
        }
        catch (JsonException)
        {
            _document = CreateDefault();
        }

        Apply();
        _loaded = true;
        Changed?.Invoke();
    }

    public Task<bool> UpdateProfileAsync(string displayName, string city) =>
        SaveAsync(doc =>
        {
            doc.DisplayName = displayName.Trim();
            doc.City = city.Trim();
        });

    public Task<bool> UpdateThemeAsync(AppTheme theme) =>
        SaveAsync(doc => doc.Theme = theme);

    public Task<bool> UpdateDisplayAsync(SarDisplayFormat sarFormat, DateDisplayFormat dateFormat) =>
        SaveAsync(doc =>
        {
            doc.SarFormat = sarFormat;
            doc.DateFormat = dateFormat;
        });

    public async Task<bool> ReplaceFromDocumentAsync(UserPreferencesDocument document)
    {
        _document = Normalize(document);
        Apply();
        var ok = await PersistAsync();
        Changed?.Invoke();
        return ok;
    }

    public Task<bool> ResetAsync() => ReplaceFromDocumentAsync(CreateDefault());

    private async Task<bool> SaveAsync(Action<UserPreferencesDocument> mutate)
    {
        mutate(_document);
        _document = Normalize(_document);
        Apply();
        var ok = await PersistAsync();
        Changed?.Invoke();
        return ok;
    }

    private void Apply()
    {
        Money.Style = _document.SarFormat;
        Dates.Style = _document.DateFormat;
        _ = ApplyThemeAsync();
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            await js.InvokeVoidAsync("finora.setTheme", _document.Theme == AppTheme.Dark ? "dark" : "light");
        }
        catch (JSException)
        {
            // Theme still applies via CSS after a reload if localStorage is intact.
        }
    }

    private async Task<bool> PersistAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Normalize(_document), FinoraJson.Options);
            var ok = await js.InvokeAsync<bool>("finora.storageSet", StorageKey, json);
            LastWriteFailed = !ok;
            return ok;
        }
        catch (JSException)
        {
            LastWriteFailed = true;
            return false;
        }
    }

    private static UserPreferencesDocument CreateDefault() => Normalize(new UserPreferencesDocument());

    private static UserPreferencesDocument Normalize(UserPreferencesDocument document)
    {
        var name = string.IsNullOrWhiteSpace(document.DisplayName) ? DemoPersona.DisplayName : document.DisplayName.Trim();
        if (name.Length > 80)
        {
            name = name[..80].Trim();
        }

        var city = string.IsNullOrWhiteSpace(document.City) ? DemoPersona.City : document.City.Trim();
        if (city.Length > 80)
        {
            city = city[..80].Trim();
        }

        return new UserPreferencesDocument
        {
            Version = 1,
            DisplayName = name,
            City = city,
            Theme = Enum.IsDefined(document.Theme) ? document.Theme : AppTheme.Light,
            SarFormat = Enum.IsDefined(document.SarFormat) ? document.SarFormat : SarDisplayFormat.CodePrefix,
            DateFormat = Enum.IsDefined(document.DateFormat) ? document.DateFormat : DateDisplayFormat.Medium
        };
    }

    private static UserPreferencesDocument Clone(UserPreferencesDocument document) => new()
    {
        Version = document.Version,
        DisplayName = document.DisplayName,
        City = document.City,
        Theme = document.Theme,
        SarFormat = document.SarFormat,
        DateFormat = document.DateFormat
    };
}
