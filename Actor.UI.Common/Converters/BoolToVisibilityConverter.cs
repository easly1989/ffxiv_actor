using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ActorWizard.Converters;

namespace Actor.UI.Common.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : ValueConverterBase<bool, Visibility>
    {
        private static BoolToVisibilityConverter _instance;

        protected override ValueConverterBase<bool, Visibility> OnProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolToVisibilityConverter());
        }

        protected override Visibility OnConvert(bool value, string parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(parameter) && parameter.Equals("!") 
                ? value 
                    ? Visibility.Collapsed 
                    : Visibility.Visible
                : value 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
        }

        protected override bool OnConvertBack(Visibility value, string parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}