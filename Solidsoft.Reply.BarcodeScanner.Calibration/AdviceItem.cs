// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdviceItem.cs" company="Solidsoft Reply Ltd">
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
// Represents an individual item of advice for a given condition.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// Ignore Spelling: Json
namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   Represents an individual item of advice for a given condition.
/// </summary>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "<Approved>")]
[method: JsonConstructor]
public sealed record AdviceItem(
    [property: JsonProperty("adviceType", Order = 0)] AdviceType AdviceType,
    [property: JsonProperty("condition", Order = 1)] string Condition,
    [property: JsonProperty("description", Order = 2)] string Description,
    [property: JsonProperty("advice", Order = 3)] IList<string> Advice,
    [property: JsonProperty("severity", Order = 4)] ConditionSeverity Severity)
    : IAdviceItem<AdviceType> {
    /// <summary>
    ///   Initializes a new instance of the <see cref="AdviceItem"/> class;.
    /// </summary>
    /// <param name="adviceType">The type of advice.</param>
    /// <param name="substitutions">Substituted text items for formatted strings.</param>
    public AdviceItem(AdviceType adviceType, params object[] substitutions)
    : this(
        adviceType,
        adviceType switch {
            AdviceType.NoSupportForCase => string.Format(
                CultureInfo.InvariantCulture,
                Properties.Advice.ResourceManager.GetString(
                    $"Condition_{(int)adviceType}",
                    Thread.CurrentThread.CurrentUICulture) ?? string.Empty,
                substitutions),
            _ => Properties.Advice.ResourceManager.GetString(
                $"Condition_{(int)adviceType}",
                Thread.CurrentThread.CurrentUICulture) ?? string.Empty
        },
        adviceType switch {
            AdviceType.NoSupportForCase => string.Format(
                CultureInfo.InvariantCulture,
                Properties.Advice.ResourceManager.GetString(
                    $"Description_{(int)adviceType}",
                    Thread.CurrentThread.CurrentUICulture) ?? string.Empty,
                substitutions),
            _ => Properties.Advice.ResourceManager.GetString(
                $"Description_{(int)adviceType}",
                Thread.CurrentThread.CurrentUICulture) ?? string.Empty
        },
        Properties.Advice.ResourceManager.GetString(
                $"Advice_{(int)adviceType}",
                Thread.CurrentThread.CurrentUICulture)
            ?.Split(";;", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList() ?? [],
        (int)adviceType switch {
            < 200 and >= 100 => ConditionSeverity.Low,
            < 300 and >= 200 => ConditionSeverity.Medium,
            >= 300 => ConditionSeverity.High,
            _ => 0
        }) {
    }

    /// <summary>
    ///   Creates an advice item from a JSON string representing the serialized data.
    /// </summary>
    /// <param name="json">A JSON string representing the serialized data.</param>
    /// <returns>The deserialised advice item.</returns>
    // ReSharper disable once UnusedMember.Global
    public static AdviceItem? FromJson(string json) {
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
            new JsonSerializerSettings {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                ContractResolver = new DataIgnoreEmptyEnumerableResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            });
}