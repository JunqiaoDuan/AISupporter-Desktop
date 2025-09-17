using AISupporter.ExternalService.AI.Interfaces.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AISupporter.App.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AIChatMessageRole role)
            {
                return role switch
                {
                    AIChatMessageRole.User => "#E3F2FD",      // Light blue
                    AIChatMessageRole.Assistant => "#E8F5E8",  // Light green
                    AIChatMessageRole.System => "#FFF3E0",     // Light orange
                    _ => "#F5F5F5"                             // Light gray
                };
            }
            return "#F5F5F5";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
