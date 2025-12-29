using System;
using System.IO;
using System.Text.Json;

namespace VideoEditor.Services
{
    public class ProjectManager
    {
        public void NewProject()
        {
            // Initialize new project
        }

        public void SaveProject(string filepath)
        {
            var project = new
            {
                Version = "1.0",
                Created = DateTime.Now,
                Modified = DateTime.Now
            };

            var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filepath, json);
        }

        public void LoadProject(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("Project file not found", filepath);

            var json = File.ReadAllText(filepath);
            // Deserialize and load project data
        }
    }
}

