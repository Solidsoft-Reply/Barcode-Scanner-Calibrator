// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdviceItem.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020 Solidsoft Reply Ltd. All rights reserved.
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
// Represents an individual item of advice for a given condition.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   Represents an individual item of advice for a given condition.
/// </summary>
public sealed class AdviceItem : IEquatable<AdviceItem>, IAdviceItem<AdviceType>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="AdviceItem"/> class;
    /// </summary>
    /// <param name="adviceType">The type of advice.</param>
    /// <param name="substitutions">Substituted text items for formatted strings.</param>
    public AdviceItem(AdviceType adviceType, params object[] substitutions)
    {
        Condition = adviceType switch
        {
            AdviceType.NoSupportForCase => string.Format(
                CultureInfo.InvariantCulture,
                Properties.Advice.ResourceManager.GetString(
                    $"Condition_{(int)adviceType}",
                    CultureInfo.CurrentCulture) ?? string.Empty,
                substitutions),
            _ => Properties.Advice.ResourceManager.GetString(
                $"Condition_{(int)adviceType}",
                CultureInfo.CurrentCulture) ?? string.Empty
        };

        Description = adviceType switch
        {
            AdviceType.NoSupportForCase => string.Format(
                CultureInfo.InvariantCulture,
                Properties.Advice.ResourceManager.GetString(
                    $"Description_{(int)adviceType}",
                    CultureInfo.CurrentCulture) ?? string.Empty,
                substitutions),
            _ => Properties.Advice.ResourceManager.GetString(
                $"Description_{(int)adviceType}",
                CultureInfo.CurrentCulture) ?? string.Empty
        };

        Advice = Properties.Advice.ResourceManager.GetString(
                $"Advice_{(int)adviceType}",
                CultureInfo.CurrentCulture)
            ?.Split(";;", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList() ?? new List<string>();

        Severity = (int)adviceType switch
        {
            < 200 and >= 100 => ConditionSeverity.Low,
            < 300 and >= 200 => ConditionSeverity.Medium,
            >= 300 => ConditionSeverity.High,
            _ => 0
        };

        AdviceType = adviceType;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="AdviceItem"/> class;
    /// </summary>
    /// <param name="adviceType">The type of advice.</param>
    /// <param name="condition">The condition for which advice is provided.</param>
    /// <param name="description">Supplemental description of the condition.</param>
    /// <param name="advice">The advice for the condition.</param>
    /// <param name="severity">The severity of the condition.</param>
    [JsonConstructor]
    internal AdviceItem(
        AdviceType adviceType,
        string condition,
        string description,
        IList<string> advice,
        ConditionSeverity severity)
    {
        AdviceType = adviceType;
        Condition = condition;
        Description = description;
        Advice = advice;
        Severity = severity;
    }

    /// <summary>
    ///   The type of advice.
    /// </summary>
    [JsonProperty("adviceType", Order = 0)]
    public AdviceType AdviceType { get; private set; }

    /// <summary>
    ///   Gets the condition for which advice is provided.
    /// </summary>
    [JsonProperty("condition", Order = 1)]
    public string Condition { get; private set; }

    /// <summary>
    ///   Gets the condition for which advice is provided.
    /// </summary>
    [JsonProperty("description", Order = 2)]
    public string Description { get; private set; }

    /// <summary>
    ///   Gets the advice for the condition.
    /// </summary>
    [JsonProperty("advice", Order = 3)]
    public IList<string> Advice{ get; private set; }

    /// <summary>
    ///   Gets the severity of the condition.
    /// </summary>
    [JsonProperty("severity", Order = 4)]
    public ConditionSeverity Severity { get; private set; }

    /// <summary>
    ///   Gets the latest serialization or deserialization error.
    /// </summary>
    [JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string LatestError { get; private set; } = string.Empty;

    /// <summary>
    ///   Creates an advice item from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    // ReSharper disable once UnusedMember.Global
    public static AdviceItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        var adviceItem = JsonConvert.DeserializeObject<AdviceItem>(json);

        if (adviceItem is null) return null;

        return new AdviceItem(
            adviceItem.AdviceType,
            adviceItem.Condition,
            adviceItem.Description,
            adviceItem.Advice,
            adviceItem.Severity);
    }

    /// <summary>
    ///   Override for the equality operator.
    /// </summary>
    /// <param name="item1">The first advice item.</param>
    /// <param name="item2">The second advice item.</param>
    /// <returns>True, if the advice items are equal; otherwise false.</returns>
    public static bool operator ==(AdviceItem? item1, AdviceItem item2) =>
        item1?.Equals(item2) ?? false;

    /// <summary>
    ///   Override for the inequality operator.
    /// </summary>
    /// <param name="item1">The first advice item.</param>
    /// <param name="item2">The second advice item.</param>
    /// <returns>True, if the advice items are not equal; otherwise false.</returns>
    public static bool operator !=(AdviceItem? item1, AdviceItem item2) =>
        !item1?.Equals(item2) ?? false;

    /// <summary>
    ///   Tests the equality of this advice item with another.
    /// </summary>
    /// <param name="other">The advice item to be tested.</param>
    /// <returns>True, if the advice items are not equal; otherwise false.</returns>
    public bool Equals(AdviceItem? other) =>
        other is not null &&
        (ReferenceEquals(this, other) || AdviceType == other.AdviceType &&
         string.Equals(Condition, other.Condition, StringComparison.Ordinal) &&
         string.Equals(Description, other.Description, StringComparison.Ordinal) &&
         Advice.SequenceEqual(other.Advice) &&
         Severity == other.Severity);


    /// <summary>
    ///   Tests the equality of this advice item with another.
    /// </summary>
    /// <param name="obj">The advice item to be tested.</param>
    /// <returns>True, if the advice items are not equal; otherwise false.</returns>
    public override bool Equals(object? obj) =>
        obj is not null && 
        (ReferenceEquals(this, obj) || obj is AdviceItem token && Equals(token));

    /// <summary>
    ///   Returns a hash value for the current advice item.
    /// </summary>
    /// <returns>The hash value.</returns>
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() =>
        Fnv.CreateHashFnv1A(
            AdviceType, 
            Condition, 
            Description,
            Advice,
            Severity);

    /// <summary>
    ///   Returns a JSON representation of the advice item.
    /// </summary>
    /// <returns>A JSON representation of the advice item.</returns>
    public override string ToString() =>
        ToJson();

    /// <summary>
    ///   Returns a JSON representation of the advice item.
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
                           ContractResolver = new CalibrationDataIgnoreEmptyEnumerableResolver()
                       };

        LatestError = JsonConvert.SerializeObject(errorContext, settings);
        errorContext.Handled = true;
    }
}