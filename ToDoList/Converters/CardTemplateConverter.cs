using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ToDoList.Converters
{
    public class CardTemplateConverter : DependencyObject, IValueConverter
    {

        public object column
        {
            get { return (object)GetValue(columnProperty); }
            set { SetValue(columnProperty, value); }
        }

        
        public static readonly DependencyProperty columnProperty =
            DependencyProperty.Register("column", typeof(object), typeof(CardTemplateConverter), new PropertyMetadata(null));


        public object Convert(object value, Type targetType, object parameter, string language)
        {


            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {


            return value;
        }
    }
}
