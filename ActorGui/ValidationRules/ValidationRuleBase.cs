using System.Globalization;
using System.Windows.Controls;

namespace ActorGui.ValidationRules
{
    public abstract class ValidationRuleBase<T> : ValidationRule 
    {
        public sealed override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value is T variable)
                return OnValidate(variable, cultureInfo);

            return new ValidationResult(false, "Unable to validate the given data!");
        }

        protected abstract ValidationResult OnValidate(T value, CultureInfo cultureInfo);
    }
}