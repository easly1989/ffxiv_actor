using System.Globalization;
using System.Windows.Controls;
using Actor.Core;

namespace ActorGui.ValidationRules
{
    public class PathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return SystemInteractions.IsValidPath((value ?? "").ToString())
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "It is not a valid path.");
        }
    }
}