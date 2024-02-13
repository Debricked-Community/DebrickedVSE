using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Debricked.Converters
{
    public class MultiBindingStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count()<3 || String.IsNullOrEmpty((string)values[2]))
            {
                return String.Format("{0}:{1}", values);
            }
            return String.Format("{0}:{1} >>", values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
