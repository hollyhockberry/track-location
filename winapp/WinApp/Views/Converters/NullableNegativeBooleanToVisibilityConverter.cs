// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;

namespace WinApp.Views.Converters
{
    public sealed class NullableNegativeBooleanToVisibilityConverter : NullableBooleanConverter<Visibility>
    {
        public NullableNegativeBooleanToVisibilityConverter()
            : base(Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed) { }
    }
}
