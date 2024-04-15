// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionSeverity.cs" company="Solidsoft Reply Ltd">
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
// The severity of a condition for which advice is provided.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   The severity of a condition for which advice is provided.
/// </summary>
public enum ConditionSeverity {
    /// <summary>
    /// <p>No severity specified.</p>
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    None = 0,

    /// <summary>
    ///   Low severity - green.
    /// </summary>
    Low = 100,

    /// <summary>
    ///   Medium severity - amber.
    /// </summary>
    Medium = 200,

    /// <summary>
    ///   High severity - red.
    /// </summary>
    High = 300,
}