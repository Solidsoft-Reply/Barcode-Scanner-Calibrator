// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationInformation.cs" company="Solidsoft Reply Ltd.">
//   (c) 2018-2024 Solidsoft Reply Ltd. All rights reserved.
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
// Information provided during keyboard calibration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   Information provided during keyboard calibration.
/// </summary>
public sealed class Information : IEquatable<Information>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="Information" /> class.
    /// </summary>
    /// <param name="level">The calibration information level.</param>
    /// <param name="type">The additional advisory information for the calibration information.</param>
    /// <param name="description">The calibration information.</param>
    public Information(
        InformationLevel level,
        InformationType type,
        string? description)
    {
        (Level, InformationType, Description) 
            = (level, type, description ?? string.Empty);
    }

    /// <summary>
    ///   Gets or sets the calibration information level.
    /// </summary>
    [JsonProperty("level", Order = 0)]
    public InformationLevel Level { get; private set; }

    /// <summary>
    ///   Gets or sets the calibration information type.
    /// </summary>
    [JsonProperty("informationType", Order = 1)]
    public InformationType InformationType { get; private set; }

    /// <summary>
    ///   Gets or sets the calibration information.
    /// </summary>
    [JsonProperty("description", Order = 2)]
    public string Description { get; set; }

    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; } = string.Empty;

    /// <summary>
    ///   Initializes the token data from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    // ReSharper disable once UnusedMember.Global
    public static Information? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        var adviceItemToken = JsonConvert.DeserializeObject<Information>(json);

        if (adviceItemToken is null) return null;

        return new Information(
            adviceItemToken.Level,
            adviceItemToken.InformationType,
            adviceItemToken.Description);
    }

    /// <summary>
    ///   Override for the equality operator.
    /// </summary>
    /// <param name="information1">The first calibration information.</param>
    /// <param name="information2">The second calibration information.</param>
    /// <returns>True, if the calibration information is equal; otherwise false.</returns>
    public static bool operator ==(Information? information1, Information information2) =>
        information1?.Equals(information2) ?? false;

    /// <summary>
    ///   Override for the inequality operator.
    /// </summary>
    /// <param name="information1">The first calibration information.</param>
    /// <param name="information2">The second calibration information.</param>
    /// <returns>True, if the calibration information is not equal; otherwise false.</returns>
    public static bool operator !=(Information? information1, Information information2) =>
        !information1?.Equals(information2) ?? false;

    /// <summary>
    ///   Tests the equality of this calibration information with another.
    /// </summary>
    /// <param name="other">The calibration information to be tested.</param>
    /// <returns>True, if the calibration information is not equal; otherwise false.</returns>
    public bool Equals(Information? other) =>
        other is not null &&
        (ReferenceEquals(this, other) || (Level.Equals(other.Level) &&
                                          InformationType.Equals(other.InformationType)));

    /// <summary>
    ///   Tests the equality of this calibration information with another.
    /// </summary>
    /// <param name="obj">The calibration information to be tested.</param>
    /// <returns>True, if the calibration information is not equal; otherwise false.</returns>
    public override bool Equals(object? obj) =>
        obj is not null && 
        (ReferenceEquals(this, obj) || (obj is Information token && Equals(token)));

    /// <summary>
    ///   Returns a hash value for the current calibration information.
    /// </summary>
    /// <returns>The hash value.</returns>

    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() =>
        Fnv.CreateHashFnv1A(
            Level,
            InformationType);

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
            new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new DataIgnoreEmptyEnumerableResolver()
            });

    /// <summary>
    ///   Handles errors in serialization and deserialization
    /// </summary>
    /// <param name="context">The streaming context.</param>
    /// <param name="errorContext">The error context</param>
    [OnError, SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedParameter.Global
#pragma warning disable CA1801 // Review unused parameters
    internal void OnError(StreamingContext context, ErrorContext errorContext)
#pragma warning restore CA1801 // Review unused parameters
    {
        var settings = new JsonSerializerSettings
                       {
                           StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                           DefaultValueHandling = DefaultValueHandling.Ignore,
                           ContractResolver = new DataIgnoreEmptyEnumerableResolver()
                       };

        LatestError = JsonConvert.SerializeObject(errorContext, settings);
        errorContext.Handled = true;
    }
}