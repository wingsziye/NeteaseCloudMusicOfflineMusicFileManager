using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace 文件目录整合
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> slist = new ObservableCollection<string>();
            if (value.GetType()==typeof(FileClassifyRules[]))
            {
                foreach (var item in value as FileClassifyRules[])
                {
                    slist.Add(enumToRuleString(item));
                }
            }
            else if (value.GetType() == typeof(FileNameRules[]))
            {
                foreach (var item in value as FileNameRules[])
                {
                    slist.Add(enumToRuleString(item));
                }
            }
            return slist;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null ;
        }

        private static string enumToRuleString(FileClassifyRules e)
        {
            var s = e.ToString().Split('x');
            if (s.Length == 2)
            {
                var v = $"{s[0]}{FileNameSeparator.Separator}{s[1]}";
                return v;
            }
            else
            {
                return e.ToString();
            }
        }

        private static string enumToRuleString(FileNameRules e)
        {
            var s = e.ToString().Split('x');
            if (s.Length == 2)
            {
                var v = $"{s[0]}{FileNameSeparator.Separator}{s[1]}";
                return v;
            }
            else
            {
                return e.ToString();
            }
        }
    }

}
