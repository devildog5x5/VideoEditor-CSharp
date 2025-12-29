using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.Enums;
using VideoEditor.Models;

namespace VideoEditor.Services
{
    public class ExportService
    {
        public void Export(List<VideoClip> clips, string outputPath, Action<int>? progressCallback = null)
        {
            if (clips == null || clips.Count == 0)
                throw new ArgumentException("No clips to export");

            try
            {
                progressCallback?.Invoke(10);

                // Simple concatenation approach
                if (clips.Count == 1)
                {
                    // Single clip - just copy/convert
                    var clip = clips[0];
                    FFMpegArguments
                        .FromFileInput(clip.FilePath)
                        .OutputToFile(outputPath, overwrite: true)
                        .ProcessSynchronously();
                }
                else
                {
                    // Multiple clips - create concat file
                    var concatFile = Path.Combine(Path.GetTempPath(), "concat.txt");
                    using (var writer = new StreamWriter(concatFile))
                    {
                        foreach (var clip in clips)
                        {
                            writer.WriteLine($"file '{clip.FilePath.Replace("'", "'\\''")}'");
                        }
                    }

                    // Use concat demuxer
                    FFMpegArguments
                        .FromFileInput(concatFile, false, options => options.WithCustomArgument("-f concat -safe 0"))
                        .OutputToFile(outputPath, overwrite: true)
                        .ProcessSynchronously();

                    if (File.Exists(concatFile))
                        File.Delete(concatFile);
                }

                progressCallback?.Invoke(100);
            }
            catch (Exception ex)
            {
                throw new Exception($"Export failed: {ex.Message}", ex);
            }
        }
    }
}

