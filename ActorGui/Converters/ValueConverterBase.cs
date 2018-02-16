using System;
using System.Globalization;
using System.Windows.Data;

namespace ActorGui.Converters
{
    public abstract class ValueConverterBase<TValue, TTarget> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TValue actualValue;
            try
            {
                actualValue = (TValue)value;
            }
            catch (Exception)
            {
                actualValue = default(TValue);
            }

            return OnConvert(actualValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TTarget actualValue;
            try
            {
                actualValue = (TTarget)value;
            }
            catch (Exception)
            {
                actualValue = default(TTarget);
            }

            return OnConvertBack(actualValue);
        }

        protected virtual TValue OnConvertBack(TTarget actualValue)
        {
            return default(TValue);
        }

        protected abstract TTarget OnConvert(TValue actualValue);
    }
}