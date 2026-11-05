// ============================================================================
// FakeBackend.cs  -  a PRETEND server that lives inside the app (TEST MODE)
// ----------------------------------------------------------------------------
// While ApiConfig.TestMode = true, ApiService calls this file instead of the
// real API. Nothing is sent over the internet: no API, no database, no Google.
//
// What it does:
//   - Log in / Create account  -> accounts are saved on THIS device only
//   - Continue with Google      -> waits 2 seconds, then logs in a test Google user
//   - Quote form (Contact)      -> requests are saved on THIS device only
//   - My account (dashboard)    -> shows the quotes you sent + sample projects
//   - Home / About information  -> read from Resources/Raw/seed.json
//
// Ready-made test login:   demo@abc.test   /   Demo1234!
//
// When the team's API, database and SSO are ready: set ApiConfig.TestMode = false.
// This file can then be deleted (and the "if (ApiConfig.TestMode)" lines in ApiService.cs).
// ============================================================================
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AndiswaConsultationApp.Models;

namespace AndiswaConsultationApp.Services;

public static class FakeBackend
{
    public const string DemoEmail = "demo@abc.test";
    public const string DemoPassword = "Demo1234!";

    private const string TokenStart = "test-token:";
    private const string AccountsKey = "test_accounts";

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    private static string QuotesPath => Path.Combine(FileSystem.AppDataDirectory, "test-quotes.json");

    // A short wait so the spinners show, like a real server
    private static Task PretendNetworkAsync(CancellationToken cancel = default) =>
        Task.Delay(TimeSpan.FromMilliseconds(500), cancel);

    // ------------------------------------------------------------------
    // ACCOUNTS
    // ------------------------------------------------------------------

    private class TestAccount
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }

    public static async Task<(ClientLoginResult? Result, string Error)> LoginAsync(string email, string password)
    {
        await PretendNetworkAsync();

        var account = LoadAccounts().FirstOrDefault(a => SameEmail(a.Email, email));
        if (account is null || account.PasswordHash != Hash(password))
            return (null, "Incorrect email or password.");

        return (MakeLogin(account.Name, account.Email), "");
    }

    public static async Task<(ClientLoginResult? Result, string Error)> RegisterAsync(string name, string email, string password)
    {
        await PretendNetworkAsync();

        if (!LooksLikeEmail(email))
            return (null, "Please enter a valid email address.");

        var accounts = LoadAccounts();
        if (accounts.Any(a => SameEmail(a.Email, email)))
            return (null, "An account with this email already exists. Please log in instead.");

        accounts.Add(new TestAccount { Name = name.Trim(), Email = email.Trim(), PasswordHash = Hash(password) });
        SaveAccounts(accounts);

        return (MakeLogin(name.Trim(), email.Trim()), "");
    }

    /// <summary>Pretend Google sign-in: no browser, just a 2 second wait (Cancel still works).</summary>
    public static async Task<(ClientLoginResult? Result, string Error)> GoogleSignInAsync(CancellationToken cancel)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancel);
            return (MakeLogin("Google Test User", "google.tester@gmail.com"), "");
        }
        catch (OperationCanceledException)
        {
            return (null, "");   // the user pressed Cancel
        }
    }

    // ------------------------------------------------------------------
    // QUOTES + DASHBOARD
    // ------------------------------------------------------------------

    private class SavedQuote
    {
        public uint Id { get; set; }
        public string Email { get; set; } = "";
        public string Service { get; set; } = "";
        public string Message { get; set; } = "";
        public string Status { get; set; } = "New";
        public DateTime CreatedAt { get; set; }
    }

    public static async Task<SendResult> SendQuoteAsync(QuoteRequest quote)
    {
        await PretendNetworkAsync();

        if (!LooksLikeEmail(quote.Email))
            return new SendResult(false, false, "Please enter a valid email address.");

        var quotes = LoadQuotes();
        quotes.Add(new SavedQuote
        {
            Id = (uint)(quotes.Count == 0 ? 1 : quotes.Max(q => q.Id) + 1),
            Email = quote.Email.Trim(),
            Service = quote.Service,
            Message = quote.Message,
            Status = "New",
            CreatedAt = DateTime.Now
        });
        await File.WriteAllTextAsync(QuotesPath, JsonSerializer.Serialize(quotes, JsonOpts));

        return new SendResult(true, false, "");
    }

    public static async Task<ClientDashboard> GetDashboardAsync(string token)
    {
        await PretendNetworkAsync();

        // A token that did not come from test mode (e.g. from the real API): ask to log in again
        if (!token.StartsWith(TokenStart))
            throw new UnauthorizedAccessException("Please log in again.");

        var email = token[TokenStart.Length..];
        var name = LoadAccounts().FirstOrDefault(a => SameEmail(a.Email, email))?.Name
                   ?? ClientSession.Name;

        var myQuotes = LoadQuotes()
            .Where(q => SameEmail(q.Email, email))
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new ClientQuote
            {
                Id = q.Id,
                Service = q.Service,
                Message = q.Message,
                Status = q.Status,
                CreatedAt = q.CreatedAt
            })
            .ToList();

        // Sample projects so the dashboard has something to show
        var projects = new List<ClientProject>
        {
            new() { Id = 1, Title = "Logo & brand identity", Status = "In Progress" },
            new() { Id = 2, Title = "Business cards", Status = "Completed" }
        };

        return new ClientDashboard { Name = name, Email = email, Quotes = myQuotes, Projects = projects };
    }

    // ------------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------------

    private static ClientLoginResult MakeLogin(string name, string email) =>
        new() { Token = TokenStart + email.Trim().ToLowerInvariant(), Name = name, Email = email };

    private static List<TestAccount> LoadAccounts()
    {
        List<TestAccount> accounts;
        try
        {
            accounts = JsonSerializer.Deserialize<List<TestAccount>>(Preferences.Default.Get(AccountsKey, "[]"), JsonOpts) ?? new();
        }
        catch (JsonException)
        {
            accounts = new();
        }

        // Always have the ready-made demo account
        if (!accounts.Any(a => SameEmail(a.Email, DemoEmail)))
            accounts.Insert(0, new TestAccount { Name = "Ayanda Demo", Email = DemoEmail, PasswordHash = Hash(DemoPassword) });

        return accounts;
    }

    private static void SaveAccounts(List<TestAccount> accounts) =>
        Preferences.Default.Set(AccountsKey, JsonSerializer.Serialize(accounts, JsonOpts));

    private static List<SavedQuote> LoadQuotes()
    {
        if (!File.Exists(QuotesPath))
            return new();
        try
        {
            return JsonSerializer.Deserialize<List<SavedQuote>>(File.ReadAllText(QuotesPath), JsonOpts) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }

    // Passwords are never kept as plain text, even in test mode
    private static string Hash(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    private static bool SameEmail(string a, string b) =>
        string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeEmail(string email) =>
        Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
