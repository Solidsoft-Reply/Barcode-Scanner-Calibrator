// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Barcode.cs" company="Solidsoft Reply Ltd">
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
// Creates a data matrix EC200 barcode as a stream of image data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

/* This code uses the ImageSharp library from SixLabors. This is a platform-independent 2D graphics library
 * with no reliance on unmanaged code. However, there are some drawbacks. It is not CLS-compliant, so there
 * could be issues when calling this code from certain languages. In addition, at the time of writing
 * (August 2023), there are significant performance issues when calling this code in V8's implementation of
 * WebAssembly (Chrome, Edge, Opera, etc.). ImageSharp depends on SIMD for high performance, but this is not
 * yet supported on WASM. NB. performance was not tested on other WASM implementations (e.g., Firefox), but
 * the same issues can be expected. In a WASM environment, use a Javascript library (e.g., bwip-js) to draw
 * barcodes.
 */

namespace Solidsoft.Reply.BarcodeScanner.Calibration.DataMatrix;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using ZXing;
using ZXing.Datamatrix;

using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;

/// <summary>
///   Creates a data matrix EC200 barcode as a stream of image data.
/// </summary>
[ExcludeFromCodeCoverage]

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
internal class Barcode : IDisposable {
    /// <summary>
    ///   Used to lock when creating a barcode;.
    /// </summary>
    private static readonly object CreateBarcodeLockObject = new ();

    /// <summary>
    ///   Indicates whether Dispose already been called.
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///   Barcode module width multiplier.
    /// </summary>
    private float _multiplier = 1.0F;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Barcode" /> class.
    /// </summary>
    public Barcode() {
    }

    /// <summary>
    ///   Finalizes an instance of the <see cref="Barcode" /> class.
    /// </summary>
    ~Barcode() {
        Dispose(false);
    }

    /// <summary>
    ///   Gets or sets the barcode foreground color.
    /// </summary>
    public Color ForegroundColor { get; set; } = Color.Black;

    /// <summary>
    ///   Gets or sets the barcode background color.
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.White;

    /// <summary>
    ///   Gets or sets the image format.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public IImageFormat ImageFormat { get; set; } = PngFormat.Instance;

    /// <summary>
    ///   Gets or sets the size multiplier factor.
    /// </summary>
    public float Multiplier {
        // ReSharper disable once UnusedMember.Global
        get {
            lock (CreateBarcodeLockObject) {
                return _multiplier;
            }
        }

        set {
            value = value switch {
                < 0.8f => 0.8f,
                > 20.0f => 20,
                _ => (float)Math.Round(value, 2)
            };

            lock (CreateBarcodeLockObject) {
                _multiplier = value;
            }
        }
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly PNG stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using Zint rules.</param>
    /// <returns>A stream containing PNG content.</returns>
    public Stream CreateBarcode(string barcodeData) {
        return CreateBarcode(barcodeData, BackgroundColor, ForegroundColor, ImageFormat);
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using Zint rules.</param>
    /// <param name="imageFormat">The image format.</param>
    /// <returns>A stream containing the image content.</returns>
    // ReSharper disable once UnusedMember.Global
    public Stream CreateBarcode(string barcodeData, IImageFormat imageFormat) {
        return CreateBarcode(barcodeData, BackgroundColor, ForegroundColor, imageFormat);
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using ZXing rules.</param>
    /// <param name="backgroundColor">The background colour of the barcode.</param>
    /// <param name="foregroundColor">The foreground colour of the barcode.</param>
    /// <returns>A stream containing PNG content.</returns>
    // ReSharper disable once UnusedMember.Global
    public Stream CreateBarcode(string barcodeData, Color backgroundColor, Color foregroundColor) {
        return CreateBarcode(barcodeData, backgroundColor, foregroundColor, ImageFormat);
    }

    /// <summary>
    ///   Creates a barcode and returns it as a readonly stream.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using ZXing rules.</param>
    /// <param name="backgroundColor">The background colour of the barcode.</param>
    /// <param name="foregroundColor">The foreground colour of the barcode.</param>
    /// <param name="imageFormat">The image format.</param>
    /// <returns>A stream containing the image content.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public Stream CreateBarcode(
        string barcodeData,
        Color backgroundColor,
        Color foregroundColor,
        IImageFormat? imageFormat) {
        // Creating a barcode is not thread-safe, so this method is synchronized.
        lock (CreateBarcodeLockObject) {
            if (string.IsNullOrEmpty(barcodeData)) {
                return new MemoryStream([], false);
            }

            var (modulesX, modulesY) = CalculateDataMatrixModuleSize(barcodeData);

            // Workaround for issues with zXing
            if (modulesY != modulesX) {
                modulesX = modulesY = Convert.ToInt32((modulesY > modulesX ? modulesY : modulesX) * 0.85);
            }

            // Create a Data Matrix barcode writer
            var barcodeWriter = new BarcodeWriterPixelData {
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new DatamatrixEncodingOptions {
                    Width = (modulesX + 2) * (int)_multiplier,
                    Height = (modulesY + 2) * (int)_multiplier,
                    Margin = 1,
                    SymbolShape = ZXing.Datamatrix.Encoder.SymbolShapeHint.FORCE_SQUARE,
                },
            };

            // Generate the barcode
            var pixelData = barcodeWriter.Write(barcodeData);

            // Create a new image with the desired background color
            using var backgroundImage = new Image<Rgba32>(Configuration.Default, pixelData.Width, pixelData.Height, backgroundColor);

            // Draw the barcode on the background image using the desired foreground color
            backgroundImage.Mutate(context => {
                // Load the barcode image from the pixel data
                using var barcodeImage = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

                for (var y = 0; y < pixelData.Height; y++) {
                    for (var x = 0; x < pixelData.Width; x++) {
                        var currentPixel = barcodeImage[x, y];

                        barcodeImage[x, y] = currentPixel switch {
                            _ when currentPixel.Equals(Rgba32.ParseHex("000000FF")) => foregroundColor,
                            _ when currentPixel.Equals(Rgba32.ParseHex("FFFFFFFF")) => backgroundColor,
                            _ => backgroundColor
                        };
                    }
                }

                context.DrawImage(barcodeImage, new Point(0, 0), 1);
            });

            // Write the image to a MemoryStream
            var memoryStream = new MemoryStream();

            if (imageFormat != null) {
#pragma warning disable SA1118 // Parameter should not span multiple lines
                backgroundImage.Save(
                    memoryStream,
                    imageFormat.Name.ToUpper(CultureInfo.InvariantCulture) switch {
                        "PNG" => new PngEncoder(),
                        "BMP" => new BmpEncoder(),
                        "GIF" => new GifEncoder(),
                        "JPEG" => new JpegEncoder(),
                        "PBM" => new PbmEncoder(),
                        "TGA" => new TgaEncoder(),
                        "TIFF" => new TiffEncoder(),
                        "WEBP" => new WebpEncoder(),
                        _ => new PngEncoder()
                    });
#pragma warning restore SA1118 // Parameter should not span multiple lines
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    /// <summary>
    ///   Creates a barcode and returns it as SVG content.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using ZXing rules.</param>
    /// <returns>A stream containing SVG content.</returns>
    public string CreateBarcodeSvg(string barcodeData) {
        return CreateBarcodeSvg(barcodeData, BackgroundColor, ForegroundColor);
    }

    /// <summary>
    ///   Creates a barcode and returns it as SVG content.
    /// </summary>
    /// <param name="barcodeData">The barcode data. Encoded using ZXing rules.</param>
    /// <param name="backgroundColor">The background colour of the barcode.</param>
    /// <param name="foregroundColor">The foreground colour of the barcode.</param>
    /// <returns>A stream containing SVG content.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public string CreateBarcodeSvg(
        string barcodeData,
        Color backgroundColor,
        Color foregroundColor) {
        // Creating a barcode is not thread-safe, so this method is synchronized.
        lock (CreateBarcodeLockObject) {
            if (string.IsNullOrEmpty(barcodeData)) return string.Empty;

            var (modulesX, modulesY) = CalculateDataMatrixModuleSize(barcodeData);

            // Workaround for issues with ZXing
            if (modulesY != modulesX) {
                modulesX = modulesY = Convert.ToInt32((modulesY > modulesX ? modulesY : modulesX) * 0.85);
            }

            // Create a Data Matrix barcode writer
            var barcodeWriter = new BarcodeWriterSvg {
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new DatamatrixEncodingOptions {
                    Width = (modulesX + 2) * (int)_multiplier,
                    Height = (modulesY + 2) * (int)_multiplier,
                    Margin = 1,
                    SymbolShape = ZXing.Datamatrix.Encoder.SymbolShapeHint.FORCE_SQUARE,
                },
            };

            var svgContent = barcodeWriter.Write(barcodeData).Content;

            var backgroundColourHex = backgroundColor.ToHex();
            var foregroundColorHex = foregroundColor.ToHex();

            if (backgroundColourHex != "#FFFFFF") {
                var xDoc = XDocument.Parse(svgContent);
                xDoc.Root?.Add(new XAttribute("preserveAspectRatio", "xMidYMid meet"));

                if (foregroundColorHex != "#000000" && xDoc.Root is not null) {
                    if (xDoc.Root.Attribute("fill") is null) {
                        xDoc.Root.Add(new XAttribute("fill", $"#{foregroundColorHex}"));
                    }
                    else {
                        xDoc.Root.Attribute("fill")!.Value = $"#{foregroundColorHex}";
                    }
                }

                if (xDoc.Root is null) return xDoc.ToString();

                if (xDoc.Root.Attribute("style") is null) {
                    xDoc.Root.Add(new XAttribute("style", $"background-color:#{backgroundColourHex};"));
                }
                else {
                    xDoc.Root.Attribute("style")!.Value = $"background-color:#{backgroundColourHex};";
                }

                return xDoc.Root.ToString(SaveOptions.DisableFormatting);
            }

            if (foregroundColorHex == "#000000") return svgContent;
            var xDocForegroundColor = XDocument.Parse(svgContent);
            xDocForegroundColor.Root?.Add(new XAttribute("preserveAspectRatio", "xMidYMid meet"));

            if (xDocForegroundColor.Root is null) return xDocForegroundColor.ToString();

            if (xDocForegroundColor.Root.Attribute("fill") is null) {
                xDocForegroundColor.Root.Add(new XAttribute("fill", $"#{foregroundColorHex}"));
            }
            else {
                xDocForegroundColor.Root.Attribute("fill")!.Value = $"#{foregroundColorHex}";
            }

            return xDocForegroundColor.ToString();
        }
    }

    /// <summary>
    ///   Public implementation of Dispose method for the <see cref="Barcode"/> class.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Protected implementation of Dispose method for the <see cref="Barcode"/> class.
    /// </summary>
    /// <param name="disposing">Indicates whether the object is being disposed explicitly.</param>
    protected virtual void Dispose(bool disposing) {
        if (_disposed) {
            return;
        }

        if (disposing) {
            // Free any other managed objects here.
        }

        // Free any unmanaged objects here.
        _disposed = true;
    }

    /// <summary>
    ///   Calculate the Data Matrix module size.
    /// </summary>
    /// <param name="barcodeData">The barcode data.</param>
    /// <returns>The number of horizontal and vertical modules in the barcode.</returns>
    private static (int modulesX, int modulesY) CalculateDataMatrixModuleSize(string barcodeData) {
        // Create an instance of the DataMatrixWriter
        var dataMatrixWriter = new DataMatrixWriter();

        // Encode the barcode data without specifying the size
        var encodedData = dataMatrixWriter.encode(barcodeData, BarcodeFormat.DATA_MATRIX, 0, 0, null);

        return (encodedData.Width, encodedData.Height);
    }
}