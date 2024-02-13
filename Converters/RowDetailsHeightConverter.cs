using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Debricked.Converters
{
    internal class RowDetailsHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //DataGridRowHeight / Header height = 25
            double targetHeight = (double)value - 100;
            if (targetHeight < 30)
            {
                return 30;
            }
            if(targetHeight > 400)
            {
                return 400;
            }
            return targetHeight > 30 ? targetHeight : 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
