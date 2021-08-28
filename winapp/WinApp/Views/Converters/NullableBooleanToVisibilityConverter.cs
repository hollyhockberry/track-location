// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;

namespace WinApp.Views.Converters
{
    public sealed class NullableBooleanToVisibilityConverter : NullableBooleanConverter<Visibility>
    {
        public NullableBooleanToVisibilityConverter()
            : base(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed) { }
    }
}
