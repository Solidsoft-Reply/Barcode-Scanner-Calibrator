// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Else.cs" company="Solidsoft Reply Ltd.">
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
// Represents an Else block in an If condition in the Environment monad.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.ProcessFlow;

using System;

/// <summary>
///   Represents an Else block in an If condition in the Environment monad.
/// </summary>
/// <typeparam name="TEnv">
///   The type of object used to store environment data.
/// </typeparam>
internal sealed class Else<TEnv> where TEnv : IEnvironment<TEnv>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="Else{TEnv}"/> class
    /// </summary>
    /// <param name="env">The object used to store environment data.</param>
    public Else(Environment<TEnv> env) =>
        EndIf = env;

    /// <summary>
    ///   Gets a value indicating whether the condition value is true.
    /// </summary>
    public Environment<TEnv> EndIf { get; }

    /// <summary>
    ///   Returns an <see cref="If{TEnv}"/> class for an inner If block.
    /// </summary>
    /// <param name="predicate">
    ///   A function over the environment that returns a condition value.
    /// </param>
    /// <returns>An <see cref="If{TEnv}"/> class for an inner If block.</returns>
    // ReSharper disable once UnusedMember.Global
    public If<TEnv> ElseIf(Func<TEnv, bool> predicate) =>
        EndIf.If(predicate);
}