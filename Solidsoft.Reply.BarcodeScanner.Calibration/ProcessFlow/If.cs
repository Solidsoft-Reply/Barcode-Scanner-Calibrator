﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="If.cs" company="Solidsoft Reply Ltd">
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
// Represents an If condition in the Environment monad.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.ProcessFlow;

using System;

/// <summary>
///   Represents an If condition in the Environment monad.
/// </summary>
/// <typeparam name="TEnv">
///   The type of object used to store environment data.
/// </typeparam>
/// <remarks>
///   Initializes a new instance of the <see cref="If{TEnv}"/> class.
/// </remarks>
/// <param name="env">The object used to store environment data.</param>
/// <param name="condition">The condition value - true or false.</param>
internal sealed class If<TEnv>(Environment<TEnv> env, bool condition)
    where TEnv : IEnvironment<TEnv> {
    /// <summary>
    ///   Gets the original environment monad, effectively marking
    ///   the end of the If block.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Environment<TEnv> EndIf { get; private set; } = env;

    /// <summary>
    ///   Gets a value indicating whether the condition value is true.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Condition { get; } = condition;

    /// <summary>
    ///   Invoke a Then block.  Actions in the Then block are only invoked if the
    ///   condition value is true.
    /// </summary>
    /// <param name="func">
    ///   Any function that takes a data object and returns
    ///   an Environment for the data object type.
    /// </param>
    /// <returns>A Then block for the If condition in the Environment monad.</returns>
    public Then<TEnv> Then(Func<TEnv, Environment<TEnv>> func) {
        if (Condition) {
            EndIf = EndIf.Do(func);
        }

        return new Then<TEnv>(EndIf, Condition);
    }
}