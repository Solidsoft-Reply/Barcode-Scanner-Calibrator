﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEnvironment.cs" company="Solidsoft Reply Ltd.">
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
// Marks a type as an environment data object, for use with the Environment monad.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.ProcessFlow;

/// <summary>
///   Marks a type as an environment data object, for use with the Environment monad.
/// </summary>
/// <typeparam name="T">The type of the environment data object.</typeparam>
// ReSharper disable once UnusedTypeParameter
#pragma warning disable S2326
internal interface IEnvironment<T>;
#pragma warning restore S2326
