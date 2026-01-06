// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Information.cs" company="Solidsoft Reply Ltd">
// Copyright (c) 2018-2024 Solidsoft Reply Ltd. All rights reserved.
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
// </copyright>
// <summary>
// Information provided during keyboard calibration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   Information provided during keyboard calibration.
/// </summary>
/// <param name="Level">The calibration information level.</param>
/// <param name="InformationType">The additional advisory information for the calibration information.</param>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "<Approved>")]
public sealed record Information(
    [JsonProperty("level", Order = 0)]
    InformationLevel Level,
    [JsonProperty("informationType", Order = 1)]
    InformationType InformationType) {

    /// <summary>
    ///   Gets or sets the calibration information.
    /// </summary>
    [JsonProperty("description", Order = 2)]
    public string? Description { get; set; }

    /// <summary>
    ///   Initializes the token data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    /// <returns>The deserialised information.</returns>
    // ReSharper disable once UnusedMember.Global
    public static Information? FromJson(string json) {
        if (string.IsNullOrWhiteSpace(json)) return null;

        var adviceItemToken = JsonConvert.DeserializeObject<Information>(json);

        if (adviceItemToken is null) return null;

        return new Information(
            adviceItemToken.Level,
            adviceItemToken.InformationType) { Description = adviceItemToken.Description };
    }

    /// <summary>
    ///   Returns a JSON representation of the calibration information.
    /// </summary>
    /// <returns>A JSON representation of the calibration information.</returns>
    public override string ToString() =>
        ToJson();

    /// <summary>
    ///   Returns a JSON representation of the calibration information.
    /// </summary>
    /// <param name="formatting">Specifies the formatting to be applied to the JSON.</param>
    /// <returns>A JSON representation of the calibration data.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public string ToJson(Formatting formatting = Formatting.None) =>
        JsonConvert.SerializeObject(
            this,
            formatting,
            new JsonSerializerSettings {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                ContractResolver = new DataIgnoreEmptyEnumerableResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            });
}