using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GopherClient.Converter
{
    class TypeToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = (string)value;
            switch (type)
            {
                // Text file
                case "0":
                    return "\uf15c \t";
                // Submenu
                case "1":
                    return "\uf178 \t";
                // Error code
                case "3":
                    return "\uf6f7 \t";
                // BinHex file
                case "4":
                    return "\uf179 \t";
                // DOS file
                case "5":
                    return "\ue629 \t";
                // Text search
                case "7":
                    return "\uf002 \t";
                // Telnet
                case "8":
                    return "\uf6ff \t";
                //Binary file
                case "9":
                    return "\uf471 \t";
                // GIF
                case "g":
                    return "\uf0fb \t";
                // Image file
                case "I":
                    return "\uf1c5 \t";
                // HTML
                case "h":
                    return "\uf13b \t";
                default:
                    return "    \t";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
