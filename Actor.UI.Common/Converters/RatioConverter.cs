using System;
using System.Globalization;
using System.Windows.Data;
using ActorWizard.Converters;

namespace Actor.UI.Common.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class RatioConverter : ValueConverterBase<double, string>
    {
        private static RatioConverter _instance;

        protected override ValueConverterBase<double, string> OnProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RatioConverter());
        }

        protected override string OnConvert(double value, string parameter, CultureInfo culture)
        {
            try
            {
                var actualParameter = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
                var result = Math.Abs(actualParameter) > 0 ? value * actualParameter : value;
                return result.ToString("G0", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return default(string);
            }
        }

        protected override double OnConvertBack(string value, string parameter, CultureInfo culture)
        {
            try
            {
                var doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                var actualParameter = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
                var result = Math.Abs(actualParameter) > 0 ? doubleValue / actualParameter : doubleValue;
                return result;
            }
            catch (Exception)
            {
                return default(double);
            }
        }
    }
}