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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using FlutnetUI.Ext.Utilities;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia graphic.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// The wrapped Avalonia graphics object
        /// </summary>
        private readonly DrawingContext _g;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        #endregion


        private readonly Stack<IDisposable> _clipStack = new Stack<IDisposable>();


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the Avalonia graphics object to use</param>
        /// <param name="initialClip">the initial clip of the graphics</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(DrawingContext g, RRect initialClip, bool releaseGraphics = false)
            : base(AvaloniaAdapter.Instance, initialClip)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            _g = g;
            _releaseGraphics = releaseGraphics;
        }

        /// <summary>
        /// Init.
        /// </summary>
        public GraphicsAdapter()
            : base(AvaloniaAdapter.Instance, RRect.Empty)
        {
            _g = null;
            _releaseGraphics = false;
        }
        
        

        public override void PopClip()
        {
            _clipStack.Pop()?.Dispose();
        }

        public override void PushClip(RRect rect)
        {
            _clipStack.Push(_g.PushClip(HtmlUtil.Convert(rect)));
            //_clipStack.Push(rect);
            //_g.PushClip(new RectangleGeometry(Utils.Convert(rect)));
        }

        public override void PushClipExclude(RRect rect)
        {
            _clipStack.Push(null);

            //TODO: Implement exclude rect, see #128
            //var geometry = new CombinedGeometry();
            //geometry.Geometry1 = new RectangleGeometry(Utils.Convert(_clipStack.Peek()));
            //geometry.Geometry2 = new RectangleGeometry(Utils.Convert(rect));
            //geometry.GeometryCombineMode = GeometryCombineMode.Exclude;

            //_clipStack.Push(_clipStack.Peek());
            //_g.PushClip(geometry);
        }

        public override Object SetAntiAliasSmoothingMode()
        {
            return null;
        }

        public override void ReturnPreviousSmoothingMode(Object prevMode)
        { }

        public override RSize MeasureString(string str, RFont font)
        {
            var text = GetText(str, font);
            var measure = text.Bounds;
            return new RSize(measure.Width, measure.Height);
            
        }

        private FormattedText GetText(string str, RFont font)
        {
            var f = ((FontAdapter)font);
            return new FormattedText
            {
                Text = str,
                Typeface = new Typeface(f.Name, font.Size, f.FontStyle, f.Weight),
            };
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            var text = GetText(str, font);
            var fullLength = text.Bounds.Width;
            if (fullLength < maxWidth)
            {
                charFitWidth = fullLength;
                charFit = str.Length;
                return;
            }

            int lastLen = 0;
            double lastMeasure = 0;
            BinarySearch(len =>
            {
                text = GetText(str.Substring(0, len), font);
                var size = text.Bounds.Width;
                lastMeasure = size;
                lastLen = len;
                if (size <= maxWidth)
                    return -1;
                return 1;

            }, 0, str.Length);
            if (lastMeasure > maxWidth)
            {
                lastLen--;
                lastMeasure = GetText(str.Substring(0, lastLen), font).Bounds.Width;
            }
            charFit = lastLen;
            charFitWidth = lastMeasure;

        }

        private static int BinarySearch(Func<int, int> condition, int start, int end)
        {
            do
            {
                int ind = start + (end - start)/2;
                int res = condition(ind);
                if (res == 0)
                    return ind;
                else if (res > 0)
                {
                    if (start != ind)
                        start = ind;
                    else
                        start = ind + 1;
                }
                else
                    end = ind;

            } while (end > start);
            return -1;
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var text = GetText(str, font);
            text.Constraint = HtmlUtil.Convert(size);
            _g.DrawText(new SolidColorBrush(HtmlUtil.Convert(color)), HtmlUtil.Convert(point), text);
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var brush = new ImageBrush(((ImageAdapter)image).Image);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.Tile;
            brush.DestinationRect = new RelativeRect(HtmlUtil.Convert(dstRect).Translate(HtmlUtil.Convert(translateTransformLocation) - new Point()), RelativeUnit.Absolute);

            return new BrushAdapter(brush);
        }
        
        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        public override void Dispose()
        {
            while (_clipStack.Count != 0)
                PopClip();
            if (_releaseGraphics)
                _g.Dispose();
        }

    
        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            x1 = (int)x1;
            x2 = (int)x2;
            y1 = (int)y1;
            y2 = (int)y2;

            var adj = pen.Width;
            if (Math.Abs(x1 - x2) < .1 && Math.Abs(adj % 2 - 1) < .1)
            {
                x1 += .5;
                x2 += .5;
            }
            if (Math.Abs(y1 - y2) < .1 && Math.Abs(adj % 2 - 1) < .1)
            {
                y1 += .5;
                y2 += .5;
            }

            _g.DrawLine(((PenAdapter)pen).CreatePen(), new Point(x1, y1), new Point(x2, y2));
        }
        
        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            var adj = pen.Width;
            if (Math.Abs(adj % 2 - 1) < .1)
            {
                x += .5;
                y += .5;
            }
            _g.DrawRectangle(((PenAdapter) pen).CreatePen(), new Rect(x, y, width, height));
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            _g.FillRectangle(((BrushAdapter) brush).Brush, new Rect(x, y, width, height));
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            _g.DrawImage(((ImageAdapter) image).Image, 1, HtmlUtil.Convert(srcRect), HtmlUtil.Convert(destRect));
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            _g.DrawImage(((ImageAdapter) image).Image, 1, new Rect(0, 0, image.Width, image.Height), HtmlUtil.Convert(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            _g.DrawGeometry(null, ((PenAdapter)pen).CreatePen(), ((GraphicsPathAdapter)path).GetClosedGeometry());
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            _g.DrawGeometry(((BrushAdapter)brush).Brush, null, ((GraphicsPathAdapter)path).GetClosedGeometry());
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
            {
                var g = new StreamGeometry();
                using (var context = g.Open())
                {
                    context.BeginFigure(HtmlUtil.Convert(points[0]), true);
                    for (int i = 1; i < points.Length; i++)
                        context.LineTo(HtmlUtil.Convert(points[i]));
                    context.EndFigure(false);
                }

                _g.DrawGeometry(((BrushAdapter)brush).Brush, null, g);
            }
        }

        #endregion
    }
}