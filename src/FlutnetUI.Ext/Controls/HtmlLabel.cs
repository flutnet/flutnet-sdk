// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Avalonia;
using Avalonia.Media;
using FlutnetUI.Ext.Adapters;
using FlutnetUI.Ext.Utilities;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace FlutnetUI.Ext.Controls
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
    /// Using <see cref="AutoSize"/> and <see cref="AutoSizeHeightOnly"/> client can control how the html content effects the
    /// size of the label. Either case scrollbars are never shown and html content outside of client bounds will be clipped.
    /// MaxWidth/MaxHeight and MinWidth/MinHeight with AutoSize can limit the max/min size of the control<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// </summary>
    /// <remarks>
    /// See <see cref="HtmlControl"/> for more info.
    /// </remarks>
    public class HtmlLabel : HtmlControl
    {
        #region Dependency properties

        public static readonly AvaloniaProperty AutoSizeProperty = PropertyHelper.Register<HtmlLabel, bool>("AutoSize", true, OnAvaloniaProperty_valueChanged);
        public static readonly AvaloniaProperty AutoSizeHeightOnlyProperty = PropertyHelper.Register<HtmlLabel, bool>("AutoSizeHeightOnly", false, OnAvaloniaProperty_valueChanged);

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        static HtmlLabel()
        {
            BackgroundProperty.OverrideDefaultValue<HtmlLabel>(Brushes.Transparent);
        }

        #region Private methods

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (_htmlContainer != null)
            {
                using (var ig = new GraphicsAdapter())
                {
                    var horizontal = Padding.Left + Padding.Right + BorderThickness.Left + BorderThickness.Right;
                    var vertical = Padding.Top + Padding.Bottom + BorderThickness.Top + BorderThickness.Bottom;

                    var size = new Size(Math.Min(MaxWidth, constraint.Width), Math.Min(MaxHeight, constraint.Height));
                    var maxSize = new RSize(size.Width < Double.PositiveInfinity ? size.Width - horizontal : 0, size.Height < Double.PositiveInfinity ? size.Height - vertical : 0);
                    _htmlContainer.HtmlContainerInt.MaxSize = maxSize;

                    _htmlContainer.HtmlContainerInt.PerformLayout(ig);
                    var newSize = _htmlContainer.ActualSize;
                    constraint = new Size(newSize.Width + horizontal, newSize.Height + vertical);
                    _htmlContainer.HtmlContainerInt.PageSize = _htmlContainer.HtmlContainerInt.ActualSize;
                }
            }

            if (double.IsPositiveInfinity(constraint.Width) || double.IsPositiveInfinity(constraint.Height))
                constraint = Size.Empty;

            return constraint;
        }

        /// <summary>
        /// Handle when dependency property value changes to update the underline HtmlContainer with the new value.
        /// </summary>
        private static void OnAvaloniaProperty_valueChanged(AvaloniaObject AvaloniaObject, AvaloniaPropertyChangedEventArgs e)
        {
            var control = AvaloniaObject as HtmlLabel;
            if (control != null)
            {
                if (e.Property == AutoSizeProperty)
                {
                    if ((bool)e.NewValue)
                    {
                        AvaloniaObject.SetValue(AutoSizeHeightOnlyProperty, false);
                        control.InvalidateMeasure();
                        control.InvalidateVisual();
                    }
                }
                else if (e.Property == AutoSizeHeightOnlyProperty)
                {
                    if ((bool)e.NewValue)
                    {
                        AvaloniaObject.SetValue(AutoSizeProperty, false);
                        control.InvalidateMeasure();
                        control.InvalidateVisual();
                    }
                }
            }
        }

        #endregion
    }
}