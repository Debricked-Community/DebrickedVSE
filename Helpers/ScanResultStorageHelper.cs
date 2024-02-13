using Community.VisualStudio.Toolkit;
using Debricked.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal static class ScanResultStorageHelper
    {
        private const string MappingFile = "mappings";
        private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions() { MaxDepth=256, ReferenceHandler = ReferenceHandler.IgnoreCycles};
        public static void Store(ScanResult scanResult, string appDataFolderPath)
        {
            if (scanResult!=null)
            {
                if(scanResult.Project != null)
                {
                    storeScanResult(scanResult, scanResult.Project.Name, scanResult.Project.FullPath, appDataFolderPath);
                } else if(scanResult.Solution != null)
                {
                    storeScanResult(scanResult, scanResult.Solution.Name, scanResult.Solution.FullPath, appDataFolderPath);
                }
            }
        }

        public static ScanResult Load(Project project, string appDataFolderPath)
        {
            if(project != null)
            {
                string projectFolder = Path.Combine(appDataFolderPath, project.Name);
                if (!Directory.Exists(projectFolder))
                {
                    return null;
                }
                string mappingsFile = Path.Combine(projectFolder, MappingFile);
                var mappings = loadMappings(mappingsFile);
                if (mappings != null && mappings.ContainsKey(project.FullPath))
                {
                    string filePath = Path.Combine(projectFolder, mappings[project.FullPath].ToString());
                    var sr= Load(filePath);
                    sr.Project = project;
                    return sr;
                }
            }
            return null;
        }

        public static ScanResult Load(Solution solution, string appDataFolderPath)
        {
            if (solution != null)
            {
                string projectFolder = Path.Combine(appDataFolderPath, solution.Name);
                if (!Directory.Exists(projectFolder))
                {
                    return null;
                }
                string mappingsFile = Path.Combine(projectFolder, MappingFile);
                var mappings = loadMappings(mappingsFile);
                if (mappings != null && mappings.ContainsKey(solution.FullPath))
                {
                    string filePath = Path.Combine(projectFolder, mappings[solution.FullPath].ToString());
                    var sr = Load(filePath);
                    sr.Solution = solution;
                    return sr;
                }
            }
            return null;
        }

        private static ScanResult Load(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<ScanResult>(json);
            }
        }

        private static void storeScanResult(ScanResult scanResult, string folderName, string fullPath, string appDataFolderPath)
        {
            Guid filename = Guid.NewGuid(); 
            Dictionary<string, Guid> map = new Dictionary<string, Guid>();
            map.Add(fullPath, filename);
            string path = Path.Combine(appDataFolderPath, folderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //check if there is already a stored scan for the project, best unique id is the fullpath
            string mappingsFile = Path.Combine(path, MappingFile);
            if (File.Exists(mappingsFile))
            {
                map = loadMappings(mappingsFile);
                if(map!=null)
                {
                    if(map.ContainsKey(fullPath))
                    {
                        filename = map[fullPath];
                    } else
                    {
                        map.Add(fullPath, filename);
                    }
                } else
                {
                    map = new Dictionary<string, Guid>();
                    map.Add(fullPath, filename);
                }
            }

            string filePath = Path.Combine(path, filename.ToString());
            File.WriteAllText(filePath, JsonSerializer.Serialize(scanResult, SerializerOptions));
            File.WriteAllText(mappingsFile, JsonSerializer.Serialize(map));
        }

        private static Dictionary<string, Guid> loadMappings(string filePath)
        {
            if(!File.Exists(filePath)) { return null; }
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<Dictionary<string, Guid>>(json);
            }
        }

        private static void storeMappings(Dictionary<string, Guid> mappings, string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(mappings));
        }
    }
}
