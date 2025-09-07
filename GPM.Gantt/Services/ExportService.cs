using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service for exporting Gantt charts to various formats
    /// </summary>
    public class ExportService : IExportService
    {
        public async Task<bool> ExportAsync(FrameworkElement element, string filePath, ExportOptions options)
        {
            try
            {
                await Task.Run(() =>
                {
                    switch (options.Format)
                    {
                        case ExportFormat.PNG:
                            ExportToPng(element, filePath, options);
                            break;
                        case ExportFormat.PDF:
                            ExportToPdf(element, filePath, options);
                            break;
                        case ExportFormat.JPEG:
                            ExportToJpeg(element, filePath, options);
                            break;
                        case ExportFormat.BMP:
                            ExportToBmp(element, filePath, options);
                            break;
                        case ExportFormat.SVG:
                            ExportToSvg(element, filePath, options);
                            break;
                        default:
                            throw new NotSupportedException($"Export format {options.Format} is not supported");
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                // Log error (in a real implementation, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Export failed: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> ExportToBytesAsync(FrameworkElement element, ExportOptions options)
        {
            return await Task.Run(() =>
            {
                switch (options.Format)
                {
                    case ExportFormat.PNG:
                        return ExportToPngBytes(element, options);
                    case ExportFormat.PDF:
                        return ExportToPdfBytes(element, options);
                    case ExportFormat.JPEG:
                        return ExportToJpegBytes(element, options);
                    case ExportFormat.BMP:
                        return ExportToBmpBytes(element, options);
                    default:
                        throw new NotSupportedException($"Export format {options.Format} is not supported for byte array export");
                }
            });
        }

        public async Task<bool> ExportGanttChartAsync(GanttContainer ganttContainer, string filePath, ExportOptions options)
        {
            return await ExportAsync(ganttContainer, filePath, options);
        }

        public string GetDefaultFileName(ExportFormat format)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = format.ToString().ToLower();
            return $"GanttChart_{timestamp}.{extension}";
        }

        public string[] GetSupportedFormats()
        {
            return new[] { "PNG", "PDF", "JPEG", "BMP", "SVG" };
        }

        public bool ValidateExportOptions(ExportOptions options)
        {
            if (options == null) return false;
            if (options.DPI <= 0) return false;
            if (options.Quality < 0 || options.Quality > 100) return false;
            return true;
        }

        public string GetFileFilter()
        {
            return "PNG Files (*.png)|*.png|" +
                   "PDF Files (*.pdf)|*.pdf|" +
                   "JPEG Files (*.jpg)|*.jpg|" +
                   "BMP Files (*.bmp)|*.bmp|" +
                   "SVG Files (*.svg)|*.svg|" +
                   "All Files (*.*)|*.*";
        }

        private void ExportToPng(FrameworkElement element, string filePath, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }

        private byte[] ExportToPngBytes(FrameworkElement element, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void ExportToJpeg(FrameworkElement element, string filePath, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            var encoder = new JpegBitmapEncoder { QualityLevel = options.Quality };
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }

        private byte[] ExportToJpegBytes(FrameworkElement element, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            var encoder = new JpegBitmapEncoder { QualityLevel = options.Quality };
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void ExportToBmp(FrameworkElement element, string filePath, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }

        private byte[] ExportToBmpBytes(FrameworkElement element, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void ExportToPdf(FrameworkElement element, string filePath, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            
            // For PDF export, we'll create a simple PDF with the image
            // In a production environment, you might want to use a library like iTextSharp or PdfSharp
            // For now, we'll create a basic implementation
            CreateSimplePdf(renderBitmap, filePath, options);
        }

        private byte[] ExportToPdfBytes(FrameworkElement element, ExportOptions options)
        {
            var renderBitmap = CreateRenderBitmap(element, options);
            return CreateSimplePdfBytes(renderBitmap, options);
        }

        private void ExportToSvg(FrameworkElement element, string filePath, ExportOptions options)
        {
            // SVG export would require a specialized library or custom implementation
            // For now, we'll export as PNG with a note
            var pngPath = filePath.Replace(".svg", ".png");
            ExportToPng(element, pngPath, options);
            
            // Write a simple SVG wrapper (basic implementation)
            var svgContent = CreateSimpleSvg(element, options);
            File.WriteAllText(filePath, svgContent);
        }

        private RenderTargetBitmap CreateRenderBitmap(FrameworkElement element, ExportOptions options)
        {
            // Ensure element is measured and arranged
            if (!element.IsMeasureValid)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }
            
            if (!element.IsArrangeValid)
            {
                element.Arrange(new Rect(element.DesiredSize));
            }

            var actualWidth = element.ActualWidth;
            var actualHeight = element.ActualHeight;

            if (actualWidth == 0 || actualHeight == 0)
            {
                actualWidth = element.DesiredSize.Width;
                actualHeight = element.DesiredSize.Height;
            }

            var width = options.Width > 0 ? options.Width : (int)actualWidth;
            var height = options.Height > 0 ? options.Height : (int)actualHeight;

            var renderBitmap = new RenderTargetBitmap(
                width, height, options.DPI, options.DPI, PixelFormats.Pbgra32);

            if (options.IncludeBackground)
            {
                var backgroundRect = new System.Windows.Shapes.Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = Brushes.White
                };
                backgroundRect.Arrange(new Rect(0, 0, width, height));
                renderBitmap.Render(backgroundRect);
            }

            renderBitmap.Render(element);
            return renderBitmap;
        }

        private void CreateSimplePdf(RenderTargetBitmap bitmap, string filePath, ExportOptions options)
        {
            // This is a simplified PDF creation
            // In production, use a proper PDF library like PdfSharp or iTextSharp
            
            // For now, we'll create a PNG and save it as PDF (not a real PDF)
            // This is a placeholder implementation
            using var fileStream = new FileStream(filePath, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            
            // Write PDF header (simplified)
            var pdfHeader = "%PDF-1.4\n";
            var headerBytes = System.Text.Encoding.ASCII.GetBytes(pdfHeader);
            fileStream.Write(headerBytes, 0, headerBytes.Length);
            
            // Write image data (this is a very basic implementation)
            encoder.Save(fileStream);
        }

        private byte[] CreateSimplePdfBytes(RenderTargetBitmap bitmap, ExportOptions options)
        {
            using var memoryStream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private string CreateSimpleSvg(FrameworkElement element, ExportOptions options)
        {
            var width = options.Width > 0 ? options.Width : (int)element.ActualWidth;
            var height = options.Height > 0 ? options.Height : (int)element.ActualHeight;
            
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""{width}"" height=""{height}"" xmlns=""http://www.w3.org/2000/svg"">
  <rect width=""100%"" height=""100%"" fill=""white""/>
  <text x=""50%"" y=""50%"" dominant-baseline=""middle"" text-anchor=""middle"">
    Gantt Chart Export - SVG format requires specialized implementation
  </text>
</svg>";
        }
    }
}