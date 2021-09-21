using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using CUE4Parse.Utils;
using static FortnitePorting.Program;
using static FortnitePorting.Utils.SimpleLogger;

namespace FortnitePorting.Utils
{
    public static class UpdateKeysMappings
    {
        private static WebClient client = new WebClient();
            
        public static void UpdateKeys() 
        {
            var response = JsonSerializer.Deserialize<Aes>(client.DownloadString("https://benbot.app/api/v2/aes"));
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath));
            
            Logger.Log($"Getting Keys for {response.version}");
            
            config.MainKey = response.mainKey;
            while (config.DynamicKeys.Count != 0) config.DynamicKeys.RemoveAt(0);

            foreach (var newEntry in response.dynamicKeys.Select(entry => new Config.KeyEntry {FileName = entry.fileName.Replace("FortniteGame/Content/Paks/", ""), Key = entry.key }))
            {
                config.DynamicKeys.Add(newEntry);
            }
            
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true}));
            Logger.Log($"Filled {config.DynamicKeys.Count} Keys");
        }

        private class Aes
        {
            public string version { get; set; }
            public string mainKey { get; set; }
            public List<DynamicKey> dynamicKeys { get; set; }

            public class DynamicKey
            {
                public string fileName { get; set; }
                public string key { get; set; }
            }
        }

        public static void UpdateMappings()
        {
            var response =
                JsonSerializer.Deserialize<Mappings[]>(client.DownloadString("https://benbot.app/api/v1/mappings"));

            var selectedMappings = response.FirstOrDefault(entry => entry.meta.compressionMethod == "Oodle");
            if (selectedMappings == null) Logger.Log("Failed to get updated mappings", ELogLevel.Critical);
            Logger.Log($"Getting Mappings for {selectedMappings.meta.version}");

            if (!Directory.Exists(MappingsPath)) Directory.CreateDirectory(MappingsPath);

            var mappingsFilePath = Path.Combine(MappingsPath, selectedMappings.fileName);
            if (File.Exists(mappingsFilePath))
            {
                HashAlgorithm hashAlg = new SHA1Managed();
                var hash = hashAlg.ComputeHash(File.ReadAllBytes(mappingsFilePath));
                if (hash.SequenceEqual(selectedMappings.hash.ParseHexBinary()))
                {
                    Logger.Log("Mappings are up to date");
                    return;
                }
            }
            
            var usmap = client.DownloadData(selectedMappings.url);
            File.WriteAllBytes(mappingsFilePath, usmap);
            Logger.Log($"Saved Mappings to {mappingsFilePath}.");
        }
        public class Mappings
        {
            public string url { get; set; }
            public string fileName  { get; set; }
            public string hash  { get; set; }
            public Meta meta  { get; set; }

            public class Meta
            {
                public string version  { get; set; }
                public string compressionMethod  { get; set; }
            }
        }
        
        public static string GetNewestUsmap(string mappingsFolder)
        {
            DirectoryInfo directory = new DirectoryInfo(mappingsFolder);
            string selectedFilePath = String.Empty;
            long modifiedTime = long.MinValue;
            foreach (var file in directory.GetFiles())
            {
                if (file.Name.EndsWith(".usmap") && file.LastWriteTime.ToFileTimeUtc() > modifiedTime)
                {
                    selectedFilePath = file.FullName;
                    modifiedTime = file.LastWriteTime.ToFileTimeUtc();
                }
            }

            return selectedFilePath;
        }
    }
}