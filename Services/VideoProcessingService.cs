using System;
using System.IO;
using FFMpegCore;
using FFMpegCore.Enums;

namespace VideoEditor.Services
{
    public class VideoProcessingService
    {
        public bool IsFFmpegAvailable => FFmpegHelper.IsFFmpegAvailable();

        public TimeSpan GetVideoDuration(string filepath)
        {
            if (!IsFFmpegAvailable)
            {
                throw new InvalidOperationException("FFmpeg is not installed. Please install FFmpeg to use video processing features.");
            }

            try
            {
                var info = FFProbe.Analyse(filepath);
                return info.Duration;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get video duration: {ex.Message}", ex);
            }
        }

        public void ExtractFrame(string videoPath, string outputPath, TimeSpan time)
        {
            if (!IsFFmpegAvailable)
            {
                throw new InvalidOperationException("FFmpeg is not installed. Preview frames require FFmpeg.");
            }

            try
            {
                // Extract frame using FFmpeg
                FFMpegArguments
                    .FromFileInput(videoPath)
                    .OutputToFile(outputPath, overwrite: true, options => options
                        .Seek(time)
                        .WithVideoCodec("png")
                        .WithFrameOutputCount(1))
                    .ProcessSynchronously();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract frame: {ex.Message}", ex);
            }
        }
    }
}

