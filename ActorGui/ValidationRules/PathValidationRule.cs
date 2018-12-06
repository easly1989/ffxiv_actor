using System.Globalization;
using System.Windows.Controls;
using Actor.Core;

namespace ActorGui.ValidationRules
{
    public class PathValidationRule : ValidationRuleBase<string>
    {
        protected override ValidationResult OnValidate(string value, CultureInfo cultureInfo)
        {
            return SystemInteractions.IsValidPath(value ?? "", false)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "It is not a valid path.");
        }
    }
}