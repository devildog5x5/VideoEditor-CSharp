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
            CurrentTheme = Themes[1]; // Default to Dark
        }

        private void InitializeThemes()
        {
            // Light Theme - High contrast for better readability
            Themes.Add(new Theme
            {
                Name = "Light",
                WindowBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(235, 235, 235)),
                TextColor = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                BorderColor = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(235, 235, 235)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(0, 102, 204))
            });

            // Dark Theme - Brighter text for better readability
            Themes.Add(new Theme
            {
                Name = "Dark",
                WindowBackground = new SolidColorBrush(Color.FromRgb(25, 25, 25)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(38, 38, 38)),
                TextColor = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                BorderColor = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(65, 65, 65)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(70, 160, 255))
            });

            // Ocean Theme - Enhanced contrast for readability
            Themes.Add(new Theme
            {
                Name = "Ocean",
                WindowBackground = new SolidColorBrush(Color.FromRgb(18, 35, 55)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(25, 45, 70)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(12, 25, 40)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(28, 50, 75)),
                TextColor = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(70, 110, 150)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(220, 240, 255)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(55, 95, 135)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(100, 200, 255))
            });

            // Forest Theme - Enhanced contrast for readability
            Themes.Add(new Theme
            {
                Name = "Forest",
                WindowBackground = new SolidColorBrush(Color.FromRgb(22, 40, 28)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(28, 50, 32)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(18, 32, 22)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(32, 55, 38)),
                TextColor = new SolidColorBrush(Color.FromRgb(235, 250, 235)),
                BorderColor = new SolidColorBrush(Color.FromRgb(70, 115, 75)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(220, 245, 220)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(60, 100, 68)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(80, 200, 110))
            });

            // Sunset Theme - Enhanced contrast for readability
            Themes.Add(new Theme
            {
                Name = "Sunset",
                WindowBackground = new SolidColorBrush(Color.FromRgb(55, 35, 25)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(65, 45, 32)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(45, 30, 20)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(75, 50, 38)),
                TextColor = new SolidColorBrush(Color.FromRgb(255, 245, 235)),
                BorderColor = new SolidColorBrush(Color.FromRgb(140, 100, 70)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(255, 235, 210)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(130, 90, 65)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(255, 160, 80))
            });

            // Midnight Theme - Enhanced contrast for readability
            Themes.Add(new Theme
            {
                Name = "Midnight",
                WindowBackground = new SolidColorBrush(Color.FromRgb(12, 12, 22)),
                WindowForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                ToolbarBackground = new SolidColorBrush(Color.FromRgb(18, 18, 28)),
                TimelineBackground = new SolidColorBrush(Color.FromRgb(8, 8, 18)),
                StatusBarBackground = new SolidColorBrush(Color.FromRgb(22, 22, 32)),
                TextColor = new SolidColorBrush(Color.FromRgb(240, 240, 255)),
                BorderColor = new SolidColorBrush(Color.FromRgb(70, 70, 85)),
                StatusTextColor = new SolidColorBrush(Color.FromRgb(220, 220, 245)),
                ButtonBackground = new SolidColorBrush(Color.FromRgb(55, 55, 70)),
                HighlightColor = new SolidColorBrush(Color.FromRgb(150, 130, 230))
            });

            CurrentTheme = Themes[1]; // Default to Dark
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

