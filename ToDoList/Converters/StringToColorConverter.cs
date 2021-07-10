using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ToDoList.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            string colorKey = value?.ToString() ?? "Normal";
            RevealBorderBrush revealBorderBrush = null;

            switch (colorKey)
            {
                case "Normal":
                    // Normal Priority - Reveal Brush
                    revealBorderBrush = new RevealBorderBrush
                    {
                        Color = Colors.Orange,
                        FallbackColor = Colors.Orange,
                        Opacity = 0.8,
                        TargetTheme = ApplicationTheme.Light
                    };

                    return revealBorderBrush;

                case "Low":
                    // Low Priority - Reveal Brush
                    revealBorderBrush = new RevealBorderBrush
                    {
                        Color = Colors.Green,
                        FallbackColor = Colors.Green,
                        Opacity = 0.8,
                        TargetTheme = ApplicationTheme.Light
                    };
                    return revealBorderBrush;

                case "High":
                    // High Priority - Reveal Brush
                    revealBorderBrush = new RevealBorderBrush
                    {
                        Color = Colors.Red,
                        FallbackColor = Colors.Red,
                        Opacity = 0.8,
                        TargetTheme = ApplicationTheme.Light
                    };
                    return revealBorderBrush;

                default:
                    return new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
