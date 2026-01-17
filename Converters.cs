using System;
using System.Globalization;
using System.Windows.Data;

namespace BluetoothAudioReceiver;

/// <summary>
/// Converter for slider fill width calculation.
/// </summary>
public class SliderFillConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3) return 0.0;
        
        if (values[0] is double value && 
            values[1] is double maximum && 
            values[2] is double width && 
            maximum > 0)
        {
            return (value / maximum) * width;
        }
        return 0.0;
    }
    
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
