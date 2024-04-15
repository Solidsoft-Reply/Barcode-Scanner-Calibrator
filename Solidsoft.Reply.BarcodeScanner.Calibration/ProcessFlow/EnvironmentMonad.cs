// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnvironmentMonad.cs" company="Solidsoft Reply Ltd">
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
// Extension methods and delegates that define a simple Environment monad.
// for any type marked with the IEnvironment&lt;T&gt; interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.ProcessFlow;

using System;
using System.Diagnostics.Contracts;

/// <summary>
///   An Environment monad implemented as a delegate over an environment type.
///   Environment monads invoke a sequence of functions, threading the environment
///   data object through each one. The functions are lazy-evaluated.
/// </summary>
/// <typeparam name="TEnv">The type of the environment data.</typeparam>
/// <returns>A lazy-evaluated function that returns the environment data object.</returns>
internal delegate Lazy<TEnv> Environment<TEnv>()
    where TEnv : IEnvironment<TEnv>;

/// <summary>
///   Extension methods and delegates that define an Environment monad.  The monad
///   manages the threading of an environment of data through sequences of functions.
/// </summary>
// ReSharper disable once UnusedMember.Global
internal static class EnvironmentMonad {
    /// <summary>
    ///   Extended Bind function for conditional composition of functions within the
    ///   Environment monad. The environment is threaded through each function.
    /// </summary>
    /// <typeparam name="TEnv">
    ///   The type of object used to store environment data.
    /// </typeparam>
    /// <param name="sourceEnvironment">The source Environment monad.</param>
    /// <param name="predicate">
    ///   Transforms the source environment data object into a new Environment monad if
    ///   the predicate returns 'true'.
    /// </param>
    /// <returns>The next Environment monad.</returns>
    /// <remarks>
    ///   The environment data object must implement the <see cref="IEnvironment&lt;T&gt;" />
    ///   interface.
    /// </remarks>
    [Pure]
    public static If<TEnv> If<TEnv>(
        this Environment<TEnv> sourceEnvironment,
        Func<TEnv, bool> predicate)
        where TEnv : IEnvironment<TEnv> {
        // Must get value of monad first to maintain correct
        // order of execution for predicate.
        var env = sourceEnvironment.End();

        return new If<TEnv>(
            () => new Lazy<TEnv>(env),
            predicate(env));
    }

    /// <summary>
    ///   Bind function for composing functions within the Environment monad.
    ///   The environment is threaded through each function.
    /// </summary>
    /// <typeparam name="TEnv">
    ///   The type of object used to store environment data.
    /// </typeparam>
    /// <param name="sourceEnvironment">The source Environment monad.</param>
    /// <param name="transformation">
    ///   Transforms the source environment data object into a new Environment monad.
    /// </param>
    /// <returns>The next Environment monad.</returns>
    /// <remarks>
    ///   The environment data object must implement the <see cref="IEnvironment&lt;T&gt;" />
    ///   interface. The bind function is lazy-evaluated.
    /// </remarks>
    [Pure]
    public static Environment<TEnv> Do<TEnv>(
        this Environment<TEnv> sourceEnvironment,
        Func<TEnv, Environment<TEnv>> transformation)
        where TEnv : IEnvironment<TEnv> =>
        () => new Lazy<TEnv>(() => transformation(sourceEnvironment.End()).End());

    /// <summary>
    ///   Unit function used to obtain an Environment monad from an object
    ///   used to store environment data.
    /// </summary>
    /// <typeparam name="TEnv">
    ///   The type of object used to store environment data.
    /// </typeparam>
    /// <param name="data">The source environment data object.</param>
    /// <returns>An Environment monad over the environment data object.</returns>
    /// <remarks>
    ///   The environment data object must implement the <see cref="IEnvironment&lt;T&gt;" />
    ///   interface.
    /// </remarks>
    [Pure]
    public static Environment<TEnv> StartProcess<TEnv>(this TEnv data)
        where TEnv : IEnvironment<TEnv> =>
        () => new Lazy<TEnv>(() => data);

    /// <summary>
    ///   Unit function used to obtain an Environment monad from an object
    ///   used to store environment data.
    /// </summary>
    /// <typeparam name="TEnv">
    ///   The type of object used to store environment data.
    /// </typeparam>
    /// <param name="data">The source environment data object.</param>
    /// <returns>An Environment monad over the environment data object.</returns>
    /// <remarks>
    ///   The environment data object must implement the <see cref="IEnvironment&lt;T&gt;" />
    ///   interface.  This synonym for 'StartProcess' should be used when continuing
    ///   a started process on a token.
    /// </remarks>
    [Pure]
    public static Environment<TEnv> Continue<TEnv>(this TEnv data)
        where TEnv : IEnvironment<TEnv> =>
        () => new Lazy<TEnv>(() => data);

    /// <summary>
    ///   Obtains a data object from an Environment monad.
    /// </summary>
    /// <typeparam name="TEnv">
    ///   The type of object used to store environment data.
    /// </typeparam>
    /// <param name="environment">The Environment monad.</param>
    /// <returns>The data object.</returns>
    [Pure]
    public static TEnv End<TEnv>(this Environment<TEnv> environment)
        where TEnv : IEnvironment<TEnv> =>
        environment().Value;
}