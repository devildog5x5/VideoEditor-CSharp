using System;
using System.Diagnostics;
using System.IO;

namespace VideoEditor.Services
{
    public static class FFmpegHelper
    {
        private static bool? _isAvailable = null;
        private static string? _ffmpegPath = null;

        /// <summary>
        /// Checks if FFmpeg is available on the system
        /// </summary>
        public static bool IsFFmpegAvailable()
        {
            if (_isAvailable.HasValue)
                return _isAvailable.Value;

            try
            {
                // Try to find ffmpeg.exe
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit(2000); // Wait max 2 seconds
                        _isAvailable = process.ExitCode == 0;
                        if (_isAvailable.Value)
                        {
                            _ffmpegPath = "ffmpeg"; // Found in PATH
                        }
                    }
                    else
                    {
                        _isAvailable = false;
                    }
                }
            }
            catch
            {
                _isAvailable = false;
            }

            return _isAvailable.Value;
        }

        /// <summary>
        /// Gets the path to FFmpeg executable, or null if not found
        /// </summary>
        public static string? GetFFmpegPath()
        {
            if (!IsFFmpegAvailable())
                return null;

            return _ffmpegPath ?? "ffmpeg";
        }

        /// <summary>
        /// Resets the cached availability check (useful after installation)
        /// </summary>
        public static void ResetCache()
        {
            _isAvailable = null;
            _ffmpegPath = null;
        }

        /// <summary>
        /// Gets a user-friendly message about FFmpeg availability
        /// </summary>
        public static string GetAvailabilityMessage()
        {
            if (IsFFmpegAvailable())
            {
                return "FFmpeg is installed and ready.";
            }
            else
            {
                return "FFmpeg is not installed. Preview frames and video processing features will be limited.\n\n" +
                       "To install FFmpeg:\n" +
                       "1. Download from https://ffmpeg.org/download.html\n" +
                       "2. Or use: winget install --id=Gyan.FFmpeg\n" +
                       "3. Add FFmpeg to your system PATH\n" +
                       "4. Restart this application";
            }
        }
    }
}

