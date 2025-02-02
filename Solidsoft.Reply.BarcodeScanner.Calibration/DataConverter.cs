// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataConverter.cs" company="Solidsoft Reply Ltd">
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
// Converts a Data object to and from JSON.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/// <summary>
/// Converts a Data object to and from JSON.
/// </summary>
internal class DataConverter : JsonConverter<Data> {
    /// <summary>
    /// Writes the JSON representation of the Data object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, Data? value, JsonSerializer serializer) {
        if (value == null) return;

        var obj = new JObject();
        if (value.CharacterMap?.Count != 0) obj["characterMap"] = JToken.FromObject(value.CharacterMap ?? new Dictionary<char, char>(), serializer);
        if (value.DeadKeysMap?.Count != 0) {
            // Compact by removing ASCII 0
            var compactDeadKeysMap = value.DeadKeysMap?.ToDictionary(kvp => kvp.Key.Length > 1 && kvp.Key[0] == char.MinValue ? kvp.Key[1..] : kvp.Key, kvp => kvp.Value);
            obj["deadKeysMap"] = JToken.FromObject(compactDeadKeysMap ?? [], serializer);
        }

        if (value.DeadKeyCharacterMap?.Count != 0) {
            // Compact by removing ASCII 0
            var compactDeadKeyCharacterMap = value.DeadKeyCharacterMap?.ToDictionary(kvp => kvp.Key.Length > 1 && kvp.Key[0] == char.MinValue ? kvp.Key[1..] : kvp.Key, kvp => kvp.Value);
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
            obj["deadKeyCharacterMap"] = JToken.FromObject(compactDeadKeyCharacterMap ?? [], serializer);
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly
        }

        if (value.LigatureMap?.Count != 0) obj["ligatureMap"] = JToken.FromObject(value.LigatureMap ?? new Dictionary<string, char>(), serializer);
        if (value.ScannerDeadKeysMap?.Count != 0) obj["scannerDeadKeysMap"] = JToken.FromObject(value.ScannerDeadKeysMap ?? new Dictionary<string, string>(), serializer);
        if (value.ScannerUnassignedKeys?.Count != 0)
            obj["scannerUnassignedKeys"] = JToken.FromObject(value.ScannerUnassignedKeys ?? [], serializer);
        if (!string.IsNullOrWhiteSpace(value.ReportedCharacters))
            obj["reportedCharacters"] = JToken.FromObject(value.ReportedCharacters, serializer);
        if (!string.IsNullOrWhiteSpace(value.AimFlagCharacterSequence))
            obj["aimFlagCharacterSequence"] = JToken.FromObject(value.AimFlagCharacterSequence, serializer);
        if (!string.IsNullOrWhiteSpace(value.Prefix))
            obj["prefix"] = JToken.FromObject(value.Prefix, serializer);
        if (!string.IsNullOrWhiteSpace(value.Code))
            obj["code"] = JToken.FromObject(value.Code, serializer);
        if (!string.IsNullOrWhiteSpace(value.Suffix))
            obj["suffix"] = JToken.FromObject(value.Suffix, serializer);
        if (!string.IsNullOrWhiteSpace(value.KeyboardScript))
            obj["keyboardScript"] = JToken.FromObject(value.KeyboardScript, serializer);
        if (!string.IsNullOrWhiteSpace(value.LineFeedCharacter))
            obj["lineFeedCharacter"] = JToken.FromObject(value.LineFeedCharacter, serializer);

        obj.WriteTo(writer);
    }

    /// <summary>
    /// Reads the JSON representation of the Data object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The Data object value.</returns>
    public override Data ReadJson(JsonReader reader, Type objectType, Data? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType != JsonToken.StartObject) return existingValue ?? new Data("{}");

        var obj = JObject.Load(reader);

        // Expand dead keys map
        var tokenDeadKeysMap = obj["deadKeysMap"];
        if (tokenDeadKeysMap is not null && tokenDeadKeysMap.HasValues) {
            var deadKeysMap = (
                from valueToken in tokenDeadKeysMap
                select valueToken).ToDictionary(
                    valueToken => char.MinValue + ((JProperty)valueToken).Name,
                    valueToken => valueToken.Last?.ToString());

            obj["deadKeysMap"] = JToken.FromObject(deadKeysMap, serializer);
        }

        // Expand dead key character map
        var tokenDeadKeyCharacterMap = obj["deadKeyCharacterMap"];
        if (tokenDeadKeyCharacterMap is null || !tokenDeadKeyCharacterMap.HasValues) return obj.ToObject<Data>() ?? new Data(string.Empty);
        var deadKeyCharacterMap = (
                from valueToken in tokenDeadKeyCharacterMap
                select valueToken).ToDictionary(
                valueToken => char.MinValue + ((JProperty)valueToken).Name,
                valueToken => valueToken.Last?.ToString());

        obj["deadKeyCharacterMap"] = JToken.FromObject(deadKeyCharacterMap, serializer);

        return obj.ToObject<Data>() ?? new Data(string.Empty);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
#pragma warning disable CA1822 // Mark members as static
    public new bool CanConvert(Type objectType) => objectType == typeof(Data);
#pragma warning restore CA1822 // Mark members as static
}