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

using Avalonia.Media;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia font.
    /// </summary>
    internal sealed class FontAdapter : RFont
    {
        public RFontStyle Style { get; }

        #region Fields and Consts


        /// <summary>
        /// the size of the font
        /// </summary>
        private readonly double _size;

        /// <summary>
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private readonly double _underlineOffset = -1;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private readonly double _height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double _whitespaceWidth = -1;
        

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public FontAdapter(string fontFamily, double size, RFontStyle style)
        {
            Style = style;
            Name = fontFamily;
            _size = size;
            //TODO: Somehow get proper line spacing and underlinePosition
            var lineSpacing = 1;
            var underlinePosition = 0;

            _height = 96d / 72d * _size * lineSpacing;
            _underlineOffset = 96d / 72d * _size * (lineSpacing + underlinePosition);

        }

        public string Name { get; set; }


        public override double Size
        {
            get { return _size; }
        }

        public override double UnderlineOffset
        {
            get { return _underlineOffset; }
        }

        public override double Height
        {
            get { return _height; }
        }

        public override double LeftPadding
        {
            get { return _height / 6f; }
        }

        public override double GetWhitespaceWidth(RGraphics graphics)
        {
            if (_whitespaceWidth < 0)
            {
                _whitespaceWidth = graphics.MeasureString(" ", this).Width;
            }
            return _whitespaceWidth;
        }

        public FontStyle FontStyle => Style.HasFlag(RFontStyle.Italic) ? FontStyle.Italic : FontStyle.Normal;

        public FontWeight Weight => Style.HasFlag(RFontStyle.Bold) ? FontWeight.Bold : FontWeight.Normal;
    }
}