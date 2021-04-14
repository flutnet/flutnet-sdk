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

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class FlutterDoctorReport
    {
        public FlutterDoctorReport()
        {
            Items = new List<FlutterDoctorReportItem>();
        }

        public List<FlutterDoctorReportItem> Items { get; }
    }

    internal class FlutterDoctorReportItem
    {
        public FlutterDoctorReportItemType Type { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    internal class FlutterDoctorReportItemBuilder
    {
        string _description;
        FlutterDoctorReportItemType _type;
        List<string> _content = new List<string>();

        public FlutterDoctorReportItemBuilder(string description)
        {
            _description = description;
        }

        public FlutterDoctorReportItemBuilder SetType(FlutterDoctorReportItemType type)
        {
            _type = type;
            return this;
        }

        public FlutterDoctorReportItemBuilder AddContent(string content)
        {
            _content.Add(content);
            return this;
        }

        public FlutterDoctorReportItem Build()
        {
            return new FlutterDoctorReportItem
            {
                Type = _type,
                Description = _description,
                Details = string.Join(Environment.NewLine, _content)
            };
        }
    }

    internal enum FlutterDoctorReportItemType
    {
        Check,
        Warning,
        Error
    }
}
