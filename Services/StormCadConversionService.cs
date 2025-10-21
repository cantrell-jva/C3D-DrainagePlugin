using System;
using System.IO;
using JVA.C3D.DrainagePlugin.Models;

namespace JVA.C3D.DrainagePlugin.Services
{
    /// <summary>
    /// Orchestrates the conversion from Civil 3D LandXML to StormCAD Excel format
    /// </summary>
    public class StormCadConversionService
    {
        private readonly LandXmlParser _xmlParser;

        public StormCadConversionService()
        {
            _xmlParser = new LandXmlParser();
        }

        /// <summary>
        /// Convert a LandXML file to StormCAD Model Builder Excel format
        /// </summary>
        /// <param name="landXmlPath">Path to the LandXML file exported from Civil 3D</param>
        /// <param name="excelOutputPath">Path for the output Excel file</param>
        /// <returns>Summary of the conversion</returns>
        public ConversionResult ConvertLandXmlToStormCad(string landXmlPath, string excelOutputPath)
        {
            var result = new ConversionResult
            {
                Success = false,
                LandXmlPath = landXmlPath,
                ExcelOutputPath = excelOutputPath
            };

            try
            {
                // Validate input file exists
                if (!File.Exists(landXmlPath))
                {
                    result.ErrorMessage = $"LandXML file not found: {landXmlPath}";
                    return result;
                }

                // Parse LandXML
                result.Message = "Parsing LandXML file...";
                var parsedNetwork = _xmlParser.ParseLandXml(landXmlPath);
                result.NetworkName = parsedNetwork.NetworkName;
                result.StructureCount = parsedNetwork.Structures.Count;
                result.PipeCount = parsedNetwork.Pipes.Count;

                // Generate Excel file
                result.Message = "Generating StormCAD Excel file...";
                using (var excelGenerator = new StormCadExcelGenerator())
                {
                    excelGenerator.GenerateStormCadExcel(
                        parsedNetwork.Structures,
                        parsedNetwork.Pipes,
                        excelOutputPath);
                }

                result.Success = true;
                result.Message = $"Successfully converted {result.StructureCount} structures and {result.PipeCount} pipes to StormCAD format.";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Message = "Conversion failed.";
                return result;
            }
        }

        /// <summary>
        /// Updates an existing StormCAD Excel file with data from a LandXML file
        /// This allows for round-trip updates when the Civil 3D model changes
        /// </summary>
        public ConversionResult UpdateStormCadExcel(string landXmlPath, string existingExcelPath)
        {
            // For updating, we use the same conversion process
            // The StormCadExcelGenerator will overwrite the data sheets while preserving the file
            return ConvertLandXmlToStormCad(landXmlPath, existingExcelPath);
        }
    }

    /// <summary>
    /// Result of a LandXML to StormCAD conversion
    /// </summary>
    public class ConversionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string LandXmlPath { get; set; }
        public string ExcelOutputPath { get; set; }
        public string NetworkName { get; set; }
        public int StructureCount { get; set; }
        public int PipeCount { get; set; }
    }
}
