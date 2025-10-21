using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JVA.C3D.DrainagePlugin.Models;

namespace JVA.C3D.DrainagePlugin.Services
{
    /// <summary>
    /// Parses LandXML files exported from Civil 3D to extract pipe network data
    /// </summary>
    public class LandXmlParser
    {
        private static readonly XNamespace ns = "http://www.landxml.org/schema/LandXML-1.2";

        public class ParsedNetwork
        {
            public List<DrainageStructure> Structures { get; set; } = new List<DrainageStructure>();
            public List<DrainagePipe> Pipes { get; set; } = new List<DrainagePipe>();
            public string NetworkName { get; set; }
        }

        /// <summary>
        /// Parse a LandXML file and extract pipe network information
        /// </summary>
        public ParsedNetwork ParseLandXml(string filePath)
        {
            var result = new ParsedNetwork();

            try
            {
                XDocument doc = XDocument.Load(filePath);

                // Find PipeNetworks element
                var pipeNetworks = doc.Descendants(ns + "PipeNetworks").FirstOrDefault();
                if (pipeNetworks == null)
                {
                    throw new Exception("No PipeNetworks found in LandXML file");
                }

                // Parse each pipe network (usually just one)
                foreach (var network in pipeNetworks.Elements(ns + "PipeNetwork"))
                {
                    result.NetworkName = network.Attribute("name")?.Value ?? "Unknown";

                    // Parse structures (manholes, inlets, outlets)
                    ParseStructures(network, result.Structures);

                    // Parse pipes (conduits)
                    ParsePipes(network, result.Pipes);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing LandXML file: {ex.Message}", ex);
            }
        }

        private void ParseStructures(XElement network, List<DrainageStructure> structures)
        {
            var structs = network.Element(ns + "Structs");
            if (structs == null) return;

            foreach (var structElem in structs.Elements(ns + "Struct"))
            {
                var structure = new DrainageStructure
                {
                    Name = structElem.Attribute("name")?.Value ?? "",
                    Description = structElem.Attribute("desc")?.Value ?? ""
                };

                // Get location
                var center = structElem.Element(ns + "Center");
                if (center != null)
                {
                    structure.Northing = ParseDouble(center.Value.Split(' ')[0]);
                    structure.Easting = ParseDouble(center.Value.Split(' ')[1]);
                    if (center.Value.Split(' ').Length > 2)
                    {
                        structure.GroundElevation = ParseDouble(center.Value.Split(' ')[2]);
                    }
                }

                // Get rim elevation
                var rimElev = structElem.Attribute("rimElev")?.Value;
                if (!string.IsNullOrEmpty(rimElev))
                {
                    structure.RimElevation = ParseDouble(rimElev);
                }

                // Get sump elevation
                var sumpElev = structElem.Attribute("sumpElev")?.Value;
                if (!string.IsNullOrEmpty(sumpElev))
                {
                    structure.SumpElevation = ParseDouble(sumpElev);
                }

                // Check if structure has rectangular shape - if so, it's a catch basin
                bool isRectangular = IsRectangularStructure(structElem);

                // Determine structure type from shape, name, or description
                structure.Type = DetermineStructureType(structure.Name, structure.Description, isRectangular);
                structure.Label = structure.Name;

                // Default StormCAD values (can be customized based on Mile High standards)
                structure.MaxPondedDepth = 1.0; // feet
                structure.PondedArea = 0.0;

                structures.Add(structure);
            }
        }

        private void ParsePipes(XElement network, List<DrainagePipe> pipes)
        {
            var pipesElem = network.Element(ns + "Pipes");
            if (pipesElem == null) return;

            foreach (var pipeElem in pipesElem.Elements(ns + "Pipe"))
            {
                var pipe = new DrainagePipe
                {
                    Name = pipeElem.Attribute("name")?.Value ?? "",
                    Label = pipeElem.Attribute("name")?.Value ?? ""
                };

                // Get reference structures
                pipe.UpstreamStructure = pipeElem.Attribute("refStart")?.Value ?? "";
                pipe.DownstreamStructure = pipeElem.Attribute("refEnd")?.Value ?? "";

                // Get pipe geometry
                var length = pipeElem.Attribute("length")?.Value;
                if (!string.IsNullOrEmpty(length))
                {
                    pipe.Length = ParseDouble(length);
                }

                // Get slope
                var slope = pipeElem.Attribute("slope")?.Value;
                if (!string.IsNullOrEmpty(slope))
                {
                    pipe.Slope = ParseDouble(slope);
                }

                // Get circular pipe section
                var circPipe = pipeElem.Element(ns + "CircPipe");
                if (circPipe != null)
                {
                    pipe.Shape = "Circular";
                    var diameter = circPipe.Attribute("diameter")?.Value;
                    if (!string.IsNullOrEmpty(diameter))
                    {
                        pipe.Diameter = ParseDouble(diameter);
                    }

                    pipe.Material = circPipe.Attribute("pipeMaterial")?.Value ?? "RCP";
                }

                // Get rectangular pipe section
                var rectPipe = pipeElem.Element(ns + "RectPipe");
                if (rectPipe != null)
                {
                    pipe.Shape = "Rectangular";
                    pipe.Width = ParseDouble(rectPipe.Attribute("width")?.Value ?? "0");
                    pipe.Height = ParseDouble(rectPipe.Attribute("height")?.Value ?? "0");
                    pipe.Material = rectPipe.Attribute("pipeMaterial")?.Value ?? "RCP";
                }

                // Get invert elevations
                var invertStart = pipeElem.Attribute("startInvert")?.Value;
                if (!string.IsNullOrEmpty(invertStart))
                {
                    pipe.UpstreamInvert = ParseDouble(invertStart);
                }

                var invertEnd = pipeElem.Attribute("endInvert")?.Value;
                if (!string.IsNullOrEmpty(invertEnd))
                {
                    pipe.DownstreamInvert = ParseDouble(invertEnd);
                }

                // Set default Manning's n based on material (Mile High Flood District standards)
                pipe.ManningsN = GetManningsN(pipe.Material);
                pipe.EntranceLoss = 0.5; // Default entrance loss coefficient
                pipe.ExitLoss = 1.0; // Default exit loss coefficient

                pipes.Add(pipe);
            }
        }

        /// <summary>
        /// Check if a structure has a rectangular shape in the LandXML
        /// </summary>
        private bool IsRectangularStructure(XElement structElem)
        {
            // Check for RectStruct element (rectangular structure shape)
            var rectStruct = structElem.Element(ns + "RectStruct");
            if (rectStruct != null)
                return true;

            // Check for shape attribute indicating rectangular
            var shape = structElem.Attribute("shape")?.Value?.ToUpper();
            if (shape != null && (shape.Contains("RECT") || shape.Contains("SQUARE") || shape.Contains("BOX")))
                return true;

            // Check for length/width attributes (indicates rectangular)
            var length = structElem.Attribute("length")?.Value;
            var width = structElem.Attribute("width")?.Value;
            if (!string.IsNullOrEmpty(length) && !string.IsNullOrEmpty(width))
                return true;

            // Check in structure description for rectangular indicators
            var desc = structElem.Attribute("desc")?.Value?.ToUpper() ?? "";
            if (desc.Contains("RECTANGULAR") || desc.Contains("RECT") || desc.Contains("SQUARE"))
                return true;

            return false;
        }

        /// <summary>
        /// Determine structure type based on shape, name, and description
        /// Rectangular structures are automatically classified as catch basins
        /// </summary>
        private string DetermineStructureType(string name, string description, bool isRectangular)
        {
            name = name.ToUpper();
            description = description.ToUpper();

            // Priority 1: Rectangular structures are catch basins
            if (isRectangular)
                return "CatchBasin";

            // Priority 2: Check name and description patterns
            if (name.Contains("MH") || description.Contains("MANHOLE"))
                return "Manhole";
            if (name.Contains("INLET") || description.Contains("INLET"))
                return "Inlet";
            if (name.Contains("OUT") || description.Contains("OUTLET"))
                return "Outlet";
            if (name.Contains("CB") || description.Contains("CATCH BASIN"))
                return "CatchBasin";

            return "Manhole"; // Default
        }

        private double GetManningsN(string material)
        {
            // Mile High Flood District recommended Manning's n values
            return material?.ToUpper() switch
            {
                "RCP" => 0.013,           // Reinforced Concrete Pipe
                "CONCRETE" => 0.013,
                "PVC" => 0.010,           // PVC
                "HDPE" => 0.012,          // High-Density Polyethylene
                "CMP" => 0.024,           // Corrugated Metal Pipe
                "STEEL" => 0.012,
                _ => 0.013                // Default to concrete
            };
        }

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
                return result;
            return 0.0;
        }
    }
}
