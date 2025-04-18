using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;


namespace GabIA.WPF.Converter
{
    public class SubstringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string stringValue))
            {
                return string.Empty;
            }

            int maxLength = 15;

            if (parameter != null && int.TryParse(parameter.ToString(), out int parsedMaxLength))
            {
                maxLength = parsedMaxLength;
            }

            return stringValue.Length > maxLength ? stringValue.Substring(0, maxLength) : stringValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
