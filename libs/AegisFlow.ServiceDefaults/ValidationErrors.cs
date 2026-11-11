namespace AegisFlow.ServiceDefaults;

// Collects request validation errors (field -> messages) for a 400 ValidationProblem response.
// Usage: new ValidationErrors().Required("name", name, 200).Rule(price > 0, "price", "...")
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public ValidationErrors Rule(bool isValid, string field, string message)
    {
        if (!isValid)
        {
            if (!_errors.TryGetValue(field, out var messages))
            {
                messages = [];
                _errors[field] = messages;
            }

            messages.Add(message);
        }

        return this;
    }

    public ValidationErrors Required(string field, string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value)
            ? Rule(false, field, $"'{field}' is required.")
            : MaxLength(field, value, maxLength);

    public ValidationErrors MaxLength(string field, string? value, int maxLength) =>
        Rule(value is null || value.Trim().Length <= maxLength, field, $"'{field}' must be at most {maxLength} characters.");

    public ValidationErrors OneOf<TEnum>(string field, string? value) where TEnum : struct, Enum =>
        Rule(TryParseEnum<TEnum>(value, out _), field, $"'{field}' must be one of: {string.Join(", ", Enum.GetNames<TEnum>())}.");

    public Dictionary<string, string[]> ToDictionary() => _errors.ToDictionary(e => e.Key, e => e.Value.ToArray());

    // Accepts enum names only (case-insensitive), so values like "1" or "Card,Wallet" are rejected.
    public static bool TryParseEnum<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        result = default;
        return !string.IsNullOrWhiteSpace(value)
            && char.IsLetter(value.Trim()[0])
            && Enum.TryParse(value.Trim(), ignoreCase: true, out result)
            && Enum.IsDefined(result);
    }
}
