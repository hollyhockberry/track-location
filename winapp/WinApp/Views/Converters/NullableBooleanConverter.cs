// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Globalization;
using System.Windows.Data;

namespace WinApp.Views.Converters
{
    public class NullableBooleanConverter<T> : IValueConverter
    {
        public NullableBooleanConverter(T trueValue, T falseValue, T nullValue)
        {
            True = trueValue;
            False = falseValue;
            Null = nullValue;
        }

        public T True { get; set; }

        public T False { get; set; }

        public T Null { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? Null : value is bool boolean && boolean ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
