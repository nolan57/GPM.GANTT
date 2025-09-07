using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GPM.Gantt.Utilities
{
    /// <summary>
    /// Value converter that converts between double values and GridLength objects.
    /// Used for binding numeric values to grid row/column dimensions.
    /// Conversion rules:
    /// - 0 or negative values → GridLength.Auto
    /// - Positive values → GridLength with pixel units
    /// </summary>
    public class DoubleToGridLengthConverter : MarkupExtension, IValueConverter
    {
        private static readonly DoubleToGridLengthConverter _instance = new DoubleToGridLengthConverter();
        
        /// <summary>
        /// Provides a static instance of the converter for XAML markup extension usage.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The converter instance.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider) => _instance;

        /// <summary>
        /// Converts a value from the source type to the target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">The culture to use for conversion.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle conversion from GridLength to double (for two-way binding)
            if (value is GridLength gridLength)
            {
                if (gridLength.IsAuto) 
                    return 0d;
                if (gridLength.GridUnitType == GridUnitType.Pixel) 
                    return gridLength.Value;
                // For Star or other unit types, return 0 to indicate Auto behavior
                return 0d;
            }
            
            // Handle conversion from double to GridLength
            if (value is double doubleValue)
            {
                return doubleValue <= 0 ? GridLength.Auto : new GridLength(doubleValue, GridUnitType.Pixel);
            }
            
            // Handle conversion from other numeric types
            if (value != null && IsNumericType(value.GetType()))
            {
                var numericValue = System.Convert.ToDouble(value);
                return numericValue <= 0 ? GridLength.Auto : new GridLength(numericValue, GridUnitType.Pixel);
            }
            
            // Default fallback
            return GridLength.Auto;
        }

        /// <summary>
        /// Converts a value from the target type back to the source type.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">The culture to use for conversion.</param>
        /// <returns>The converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
            {
                if (gridLength.IsAuto) 
                    return 0d;
                if (gridLength.GridUnitType == GridUnitType.Pixel) 
                    return gridLength.Value;
                // For other unit types, return 0
                return 0d;
            }
            
            if (value is double doubleValue)
            {
                return doubleValue <= 0 ? GridLength.Auto : new GridLength(doubleValue, GridUnitType.Pixel);
            }
            
            return GridLength.Auto;
        }
        
        /// <summary>
        /// Determines if a type is a numeric type that can be converted to double.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is numeric; otherwise, false.</returns>
        private static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(double) || type == typeof(float) ||
                   type == typeof(decimal) || type == typeof(long) || type == typeof(short) ||
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(ushort) || type == typeof(sbyte);
        }
    }
}