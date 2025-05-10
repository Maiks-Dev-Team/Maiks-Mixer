using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Converts a category string to a brush color for visual representation.
    /// </summary>
    public class CategoryToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a category string to a SolidColorBrush.
        /// </summary>
        /// <param name="value">The category string.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A SolidColorBrush representing the category.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            string category = value.ToString().ToLowerInvariant();

            return category switch
            {
                "default" => new SolidColorBrush(Color.FromRgb(0, 120, 215)),    // Blue
                "recording" => new SolidColorBrush(Color.FromRgb(0, 153, 76)),   // Green
                "mixing" => new SolidColorBrush(Color.FromRgb(209, 52, 56)),     // Red
                "mastering" => new SolidColorBrush(Color.FromRgb(170, 0, 255)),  // Purple
                "live" => new SolidColorBrush(Color.FromRgb(240, 150, 9)),       // Orange
                "custom" => new SolidColorBrush(Color.FromRgb(155, 155, 155)),   // Gray
                _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))           // Dark Gray
            };
        }

        /// <summary>
        /// Converts a brush back to a category string (not implemented).
        /// </summary>
        /// <param name="value">The brush value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The original value (conversion not supported).</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter doesn't support converting back
            return value;
        }
    }
}
