// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAdviceItem.cs" company="Solidsoft Reply Ltd">
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

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using Newtonsoft.Json;

/// <summary>
/// Represents an individual item of advice for a given condition.
/// </summary>
/// <typeparam name="TAdviceType">The advice type.</typeparam>
public interface IAdviceItem<out TAdviceType>
    where TAdviceType : Enum {
    /// <summary>
    ///   Gets the type of advice.
    /// </summary>
    [JsonProperty("adviceType", Order = 0)]
    public TAdviceType AdviceType { get; }

    /// <summary>
    ///   Gets the condition for which advice is provided.
    /// </summary>
    [JsonProperty("condition", Order = 1)]
    public string Condition { get; }

    /// <summary>
    ///   Gets the condition for which advice is provided.
    /// </summary>
    [JsonProperty("description", Order = 2)]
    public string Description { get; }

    /// <summary>
    ///   Gets the advice for the condition.
    /// </summary>
    [JsonProperty("advice", Order = 3)]
    public IList<string> Advice { get; }

    /// <summary>
    ///   Gets the severity of the condition.
    /// </summary>
    [JsonProperty("severity", Order = 4)]
    public ConditionSeverity Severity { get; }
}