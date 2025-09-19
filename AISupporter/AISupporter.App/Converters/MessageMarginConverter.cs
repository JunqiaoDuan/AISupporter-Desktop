using AISupporter.ExternalService.AI.Interfaces.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace AISupporter.App.Converters
{
    public class MessageMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AIChatMessageRole role)
            {
                return role switch
                {
                    AIChatMessageRole.User => new Thickness(60, 3, 8, 3),
                    AIChatMessageRole.Assistant => new Thickness(8, 3, 60, 3),
                    AIChatMessageRole.System => new Thickness(8, 3, 60, 3),
                    _ => new Thickness(8, 3, 8, 3)
                };
            }
            return new Thickness(8, 3, 8, 3);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
