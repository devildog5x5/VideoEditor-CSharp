using System;
using System.Globalization;
using System.Windows.Data;

namespace VideoEditor.Utils
{
    public class TimeToPixelConverter : IValueConverter
    {
        // Scale: 1 second = 50 pixels (adjustable)
        private const double PixelsPerSecond = 50.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double timeInSeconds)
            {
                return timeInSeconds * PixelsPerSecond;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double pixels)
            {
                return pixels / PixelsPerSecond;
            }
            return 0.0;
        }
    }
}

