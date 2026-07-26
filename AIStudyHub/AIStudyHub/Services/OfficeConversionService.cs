using System;
using System.IO;
using MiniSoftware;

namespace AIStudyHub.Services
{
    /// <summary>
    /// Converts Office documents (docx, pptx, xlsx) to PDF offline using MiniPdf.
    /// PDF, TXT and MD files are returned as-is.
    /// </summary>
    public static class OfficeConversionService
    {
        private static readonly string CacheDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "AIStudyHub", "Cache");

        /// <summary>
        /// Returns a path to a PDF version of the given file.
        /// For PDF/TXT/MD the original path is returned unchanged.
        /// Office files are converted to PDF and cached.
        /// </summary>
        public static string ConvertToPdfForUpload(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source file not found.", sourceFilePath);

            var ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();

            // These are natively supported by the Gemini File API — no conversion needed
            if (ext is ".pdf" or ".txt" or ".md")
                return sourceFilePath;

            // Office formats — convert to PDF offline via MiniPdf
            Directory.CreateDirectory(CacheDir);

            // Use a stable cache name based on the source path + last-write time
            var cacheKey = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{File.GetLastWriteTime(sourceFilePath):yyyyMMddHHmmss}.pdf";
            var outputPath = Path.Combine(CacheDir, cacheKey);

            if (!File.Exists(outputPath))
            {
                MiniPdf.ConvertToPdf(sourceFilePath, outputPath);
            }

            return outputPath;
        }

        /// <summary>
        /// Returns the MIME type for the Gemini File API upload based on file extension.
        /// </summary>
        public static string GetMimeType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".pdf"  => "application/pdf",
                ".txt"  => "text/plain",
                ".md"   => "text/plain",
                _       => "application/pdf" // converted files are always PDF
            };
        }
    }
}
