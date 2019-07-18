using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ActorWizard.Converters
{
    public abstract class ValueConverterBase<TValue, TTarget> : MarkupExtension, IValueConverter
    {
        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            return OnProvideValue(serviceProvider);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ParamsActualization<TValue>(value, parameter, out var actualValue, out var actualParameter);
            return OnConvert(actualValue, actualParameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ParamsActualization<TTarget>(value, parameter, out var actualValue, out var actualParameter);
            return OnConvertBack(actualValue, actualParameter, culture);
        }

        protected abstract ValueConverterBase<TValue, TTarget> OnProvideValue(IServiceProvider serviceProvider);
        protected abstract TTarget OnConvert(TValue value, string parameter, CultureInfo culture);
        protected abstract TValue OnConvertBack(TTarget value, string parameter, CultureInfo culture);

        private static void ParamsActualization<T>(object value, object parameter, out T actualValue, out string actualParameter)
        {
            try
            {
                // ReSharper disable MergeCastWithTypeCheck
                actualValue = value is T ? (T)value : default(T);
                actualParameter = parameter as string;
                // ReSharper restore MergeCastWithTypeCheck
            }
            catch (Exception)
            {
                actualValue = default(T);
                actualParameter = default(string);
            }
        }
    }
}
