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
