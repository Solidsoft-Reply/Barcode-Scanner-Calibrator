// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportedPlatform.cs" company="Solidsoft Reply Ltd.">
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
// Platforms supported for calibration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace Solidsoft.Reply.BarcodeScanner.Calibration;

/// <summary>
///   Platforms supported for calibration.
/// </summary>
/// <remarks>At this time, we do not support iOS or Android.</remarks>
public enum SupportedPlatform
{
    /// <summary>
    ///   The OS family cannot be determined.
    /// </summary>
    Unknown,

    /// <summary>
    ///   Linux. We do not differentiate between different distributions.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    Linux,

    /// <summary>
    ///   The Apple Macintosh macOS. NB. This does not include iOS
    /// </summary>
    Macintosh,

    /// <summary>
    ///   Windows (NT family including Windows 10)
    /// </summary>
    Windows,

    /// <summary>
    ///   Windows 98.
    /// </summary>
    Windows98,

    /// <summary>
    ///   The Windows Phone (now discontinued)
    /// </summary>
    WindowsPhone,

    /// <summary>
    ///   A mobile edition of Linux.
    /// </summary>
    MobileLinux,

    /// <summary>
    ///   Chrome OS.
    /// </summary>
    ChromeOs,

    /// <summary>
    ///   A UNIX-based OS (e.g., OpenBSD, FreeBSD). This also includes Cygwin.
    /// </summary>
    Unix,

    /// <summary>
    ///   Blackberry Tablet OS.
    /// </summary>
    TabletOs,

    /// <summary>
    ///   Web OS.
    /// </summary>
    WebOs,

    /// <summary>
    ///   Haiku (BeOS clone)
    /// </summary>
    Haiku,

    /// <summary>
    ///   The Android OS.
    /// </summary>
    Android,

    /// <summary>
    ///   The Symbian OS.
    /// </summary>
    SymbianOs
}