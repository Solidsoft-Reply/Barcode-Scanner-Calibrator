// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationBaseRecord.cs" company="Solidsoft Reply Ltd.">
//   (c) 2023 Solidsoft Reply Ltd. All rights reserved.
// </copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>
// <summary>
// A base record for JSON-serialisable records.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System.Runtime.Serialization;

using Newtonsoft.Json.Serialization;

using Newtonsoft.Json;

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
/// 
/// </summary>
public abstract record CalibrationBaseRecord
{
    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; } = string.Empty;

    /// <summary>
    ///   Returns an object representing a JSON representation of the object.
    /// </summary>
    /// <typeparam name="T">The record type to be deserialised.</typeparam>
    /// <param name="json">A JSON string representing the serialized data.</param>
    /// <returns>The deserialised object.</returns>
    // ReSharper disable once UnusedMember.Global
    public static T? FromJson<T>(string json) where T : CalibrationBaseRecord
    {
        var deserialisedObject = JsonConvert.DeserializeObject<T>(json);

        return string.IsNullOrWhiteSpace(json) || deserialisedObject is null
            ? default
            : deserialisedObject;
    }

    /// <summary>
    ///   Returns a JSON representation of the object.
    /// </summary>
    /// <returns>A JSON representation of the calibration information.</returns>
    public override string ToString() =>
        ToJson();

    /// <summary>
    ///   Returns a JSON representation of the object.
    /// </summary>
    /// <param name="formatting">Specifies the formatting to be applied to the JSON.</param>
    /// <returns>A JSON representation of the calibration data.</returns>

    // ReSharper disable once MemberCanBePrivate.Global
    public string ToJson(Formatting formatting = Formatting.None) =>
        JsonConvert.SerializeObject(
            this,
            formatting,
            new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
            });

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedParameter.Global
#pragma warning disable IDE0060
    internal void OnError(StreamingContext context, ErrorContext errorContext)
#pragma warning restore IDE0060
    {
        var settings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
        };

        LatestError = JsonConvert.SerializeObject(errorContext, settings);
        errorContext.Handled = true;
    }
}