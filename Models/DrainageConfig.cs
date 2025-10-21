using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace JVA.C3D.DrainagePlugin.Models
{
    /// <summary>
    /// Configuration settings for drainage design following Mile High Flood District standards
    /// </summary>
    public class DrainageConfig
    {
        public string ProjectName { get; set; } = "Colorado Site Development";
        public string Designer { get; set; } = "";
        public string MunicipalityStandard { get; set; } = "Mile High Flood District";

        // Manning's n values by material (MHFD standards)
        public Dictionary<string, double> ManningsNValues { get; set; } = new Dictionary<string, double>
        {
            { "RCP", 0.013 },           // Reinforced Concrete Pipe
            { "CONCRETE", 0.013 },
            { "PVC", 0.010 },           // PVC
            { "HDPE", 0.012 },          // High-Density Polyethylene
            { "CMP", 0.024 },           // Corrugated Metal Pipe
            { "STEEL", 0.012 },
            { "DEFAULT", 0.013 }
        };

        // Default loss coefficients
        public double DefaultEntranceLoss { get; set; } = 0.5;
        public double DefaultExitLoss { get; set; } = 1.0;

        // Structure defaults
        public double DefaultMaxPondedDepth { get; set; } = 1.0; // feet
        public double DefaultPondedArea { get; set; } = 0.0; // square feet

        // File paths
        public string DefaultLandXmlPath { get; set; } = "";
        public string DefaultExcelOutputPath { get; set; } = "";

        // Conversion settings
        public bool AutoOpenExcelAfterExport { get; set; } = true;
        public bool ShowDetailedMessages { get; set; } = true;

        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        public static DrainageConfig Load(string configPath)
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    return JsonSerializer.Deserialize<DrainageConfig>(json) ?? new DrainageConfig();
                }
            }
            catch (Exception)
            {
                // If config fails to load, return defaults
            }

            return new DrainageConfig();
        }

        /// <summary>
        /// Save configuration to JSON file
        /// </summary>
        public void Save(string configPath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get the default configuration file path
        /// </summary>
        public static string GetDefaultConfigPath()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(assemblyPath);
            return Path.Combine(directory, "DrainageConfig.json");
        }
    }
}
