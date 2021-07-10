﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ToDoList.Converters
{
    public class StringFormatToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var newString = "";

            DateTime dt;
            if (value != null && value is string)
            {
                var stringToConvert = value as string;
                bool success = DateTime.TryParse(stringToConvert, out dt);

                if (success)
                {
                    newString = dt.ToString("hh:mm tt"); // EX: 12:34 AM
                    return newString;
                }
            }
            return newString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string time;
            if (value != null && value is DateTimeOffset)
            {
                var valueToConvert = (DateTimeOffset)value;
                time = new DateTime(valueToConvert.Ticks).ToString("yyyy.MM.dd");
            }
            else
                time = string.Empty;
            return time;
        }
    }
}
