namespace finora_dashboard.Models;

public sealed class DemoProfile
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string City { get; init; }
    public string FullName => $"{FirstName} {LastName}";
    public string Initials => $"{FirstName[0]}{LastName[0]}";
}
