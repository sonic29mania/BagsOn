using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BagsOn.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        // Конвертує шлях з бази даних у картинку для WPF
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? imagePath = value as string;

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            string fullPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                imagePath.Replace("/", "\\")
            );

            if (!File.Exists(fullPath))
            {
                return null;
            }

            BitmapImage image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.EndInit();

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}