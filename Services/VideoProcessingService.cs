using System;
using System.IO;
using FFMpegCore;
using FFMpegCore.Enums;

namespace VideoEditor.Services
{
    public class VideoProcessingService
    {
        public TimeSpan GetVideoDuration(string filepath)
        {
            try
            {
                var info = FFProbe.Analyse(filepath);
                return info.Duration;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        public void ExtractFrame(string videoPath, string outputPath, TimeSpan time)
        {
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

