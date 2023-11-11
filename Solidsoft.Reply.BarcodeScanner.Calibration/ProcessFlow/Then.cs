﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Then.cs" company="Solidsoft Reply Ltd.">
//   (c) 2021 Solidsoft Reply Ltd.  All rights reserved.
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
// Represents a Then block in an If condition in the Environment monad.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.ProcessFlow;

using System;

/// <summary>
///   Represents a Then block in an If condition in the Environment monad.
/// </summary>
/// <typeparam name="TEnv">
///   The type of object used to store environment data.
/// </typeparam>
internal sealed class Then<TEnv> where TEnv : IEnvironment<TEnv>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="Then{TEnv}"/> class
    /// </summary>
    /// <param name="env">The object used to store environment data.</param>
    /// <param name="condition">The condition value - true or false.</param>
    public Then(Environment<TEnv> env, bool condition)
    {
        EndIf = env;
        Condition = condition;
    }

    /// <summary>
    ///   Gets the original environment monad, effectively marking
    ///   the end of the If block. 
    /// </summary>
    public Environment<TEnv> EndIf { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether the condition value is true.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Condition { get; }

    /// <summary>
    ///   Invoke an Else block.  Actions in the Else block are only invoked if the
    ///   condition value is false. 
    /// </summary>
    /// <param name="func">
    ///   Any function that takes a a data object and returns
    ///   an Environment for the data object type.
    /// </param>
    /// <returns>An Else block for the If condition in the Environment monad.</returns>
    public Else<TEnv> Else(Func<TEnv, Environment<TEnv>> func)
    {
        if (!Condition)
        {
            EndIf = EndIf.Do(func);
        }

        return new Else<TEnv>(EndIf);
    }
}