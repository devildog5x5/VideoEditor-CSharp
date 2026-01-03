using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace VideoEditor.Services
{
    public class Theme
    {
        public string Name { get; set; } = string.Empty;
        public Brush WindowBackground { get; set; } = Brushes.White;
        public Brush WindowForeground { get; set; } = Brushes.Black;
        public Brush ToolbarBackground { get; set; } = Brushes.LightGray;
        public Brush TimelineBackground { get; set; } = Brushes.DarkGray;
        public Brush StatusBarBackground { get; set; } = Brushes.LightGray;
        public Brush TextColor { get; set; } = Brushes.Black;
        public Brush BorderColor { get; set; } = Brushes.Gray;
        public Brush StatusTextColor { get; set; } = Brushes.Black;
        public Brush ButtonBackground { get; set; } = Brushes.LightBlue;
        public Brush HighlightColor { get; set; } = Brushes.Blue;
    }

    public class ThemeManager
    {
        private const string RegistryKey = @"Software\VideoEditor\Settings";
        private const string ThemeKey = "CurrentTheme";

        public List<Theme> Themes { get; } = new();
        public Theme CurrentTheme { get; private set; }

        public ThemeManager()
        {
            InitializeThemes();
            CurrentTheme = Themes[2]; // Default to Ocean
        }

        private void InitializeThemes()
        {
            // Light Theme - Maximum readability with pure black text on white
            Themes.Add(new Theme
            {
                Name = "Light",
                WindowBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                TextColor = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                BorderColor = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(0, 100, 200))
            });

            // Dark Theme - Professional dark theme with excellent readability
            Themes.Add(new Theme
            {
                Name = "Dark",
                WindowBackground = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(60, 60, 65)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(30, 30, 35)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(55, 55, 60)),
                TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(80, 80, 85)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(0, 120, 215))
            });

            // Ocean Theme - Highly readable dark blue theme (DEFAULT)
            Themes.Add(new Theme
            {
                Name = "Ocean",
                WindowBackground = new SolidColorBrush(Color.FromRgb(35, 50, 65)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(45, 65, 85)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(25, 40, 55)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(40, 60, 80)),
                TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(140, 180, 220)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(70, 100, 140)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(120, 200, 255))
            });

            // Forest Theme - Readable dark green theme
            Themes.Add(new Theme
            {
                Name = "Forest",
                WindowBackground = new SolidColorBrush(Color.FromRgb(35, 50, 40)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(45, 65, 50)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(25, 40, 30)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(40, 60, 45)),
                TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(120, 180, 130)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(70, 110, 85)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(100, 220, 140))
            });

            // Sunset Theme - Readable dark orange theme
            Themes.Add(new Theme
            {
                Name = "Sunset",
                WindowBackground = new SolidColorBrush(Color.FromRgb(55, 40, 30)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(70, 50, 40)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(40, 30, 20)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(65, 45, 35)),
                TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(200, 150, 120)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(120, 90, 70)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(255, 180, 100))
            });

            // Midnight Theme - Readable dark purple theme
            Themes.Add(new Theme
            {
                Name = "Midnight",
                WindowBackground = new SolidColorBrush(Color.FromRgb(40, 35, 50)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(50, 45, 65)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(30, 25, 40)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(45, 40, 55)),
                TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(150, 140, 180)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(70, 65, 90)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(200, 180, 255))
            });

            CurrentTheme = Themes[2]; // Default to Ocean
        }

        public void ApplyTheme(Theme theme)
        {
            CurrentTheme = theme;
            var app = Application.Current;
            if (app == null) return;

            var resources = app.Resources;
            resources["WindowBackground"] = theme.WindowBackground;
            resources["WindowForeground"] = theme.WindowForeground;
            resources["ToolbarBackground"] = theme.ToolbarBackground;
            resources["TimelineBackground"] = theme.TimelineBackground;
            resources["StatusBarBackground"] = theme.StatusBarBackground;
            resources["TextColor"] = theme.TextColor;
            resources["BorderColor"] = theme.BorderColor;
            resources["StatusTextColor"] = theme.StatusTextColor;
            resources["ButtonBackground"] = theme.ButtonBackground;
            resources["HighlightColor"] = theme.HighlightColor;

            SaveTheme();
        }

        public void ApplySavedTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey);
                var themeName = key?.GetValue(ThemeKey)?.ToString();
                if (!string.IsNullOrEmpty(themeName))
                {
                    var theme = Themes.Find(t => t.Name == themeName);
                    if (theme != null)
                    {
                        ApplyTheme(theme);
                        return;
                    }
                }
            }
            catch { }

            ApplyTheme(CurrentTheme);
        }

        private void SaveTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryKey);
                key?.SetValue(ThemeKey, CurrentTheme.Name);
            }
            catch { }
        }
    }
}

