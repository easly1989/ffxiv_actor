using System.Windows;

namespace ActorGui.Converters
{
    public class BoolToVisibilityConverter : ValueConverterBase<bool, Visibility>
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        protected override Visibility OnConvert(bool actualValue)
        {
            return actualValue ? TrueValue : FalseValue;
        }
    }
}
