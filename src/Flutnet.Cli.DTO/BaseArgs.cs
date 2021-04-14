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