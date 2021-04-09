using System;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Base class that holds all the input arguments of a CLI 'exec' command.
    /// </summary>
    [JsonConverter(typeof(JsonKnownTypesConverter<InArg>))]
    public abstract class InArg
    {
    }

    /// <summary>
    /// Base class that holds the result details of a CLI 'exec' command.
    /// </summary>
    [JsonConverter(typeof(JsonKnownTypesConverter<OutArg>))]
    public abstract class OutArg
    {
        public bool Success { get; set; } = true;
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string SourceError { get; set; }
    }
}