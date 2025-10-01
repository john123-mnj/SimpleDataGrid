using System.Globalization;
using System.Windows.Data;

namespace SimpleDataGrid.Example.Converters;

public class ItemCountConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 4 && values[0] is int count && values[1] is int page && values[2] is int total && values[3] is int pageSize)
        {
            var start = ((page - 1) * pageSize) + 1;
            var end = start + count - 1;
            return $"Showing items {start}-{end} of {total}";
        }
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
