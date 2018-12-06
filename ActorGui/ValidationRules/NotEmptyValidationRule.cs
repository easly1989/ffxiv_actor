using System.Globalization;
using System.Windows.Controls;

namespace ActorGui.ValidationRules
{
    public class NotEmptyValidationRule : ValidationRuleBase<string>
    {
        protected override ValidationResult OnValidate(string value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace(value ?? "")
                ? new ValidationResult(false, "Field is required.")
                : ValidationResult.ValidResult;
        }
    }
}
