namespace JVA.C3D.DrainagePlugin.Models
{
    /// <summary>
    /// Represents a drainage pipe (conduit) from Civil 3D
    /// </summary>
    public class DrainagePipe
    {
        public string Name { get; set; }
        public string UpstreamStructure { get; set; }
        public string DownstreamStructure { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
        public string Material { get; set; }
        public double UpstreamInvert { get; set; }
        public double DownstreamInvert { get; set; }
        public double Slope { get; set; }
        public string Shape { get; set; } // Circular, Rectangular, etc.

        // For non-circular pipes
        public double Width { get; set; }
        public double Height { get; set; }

        // StormCAD specific
        public string Label { get; set; }
        public double ManningsN { get; set; }
        public double EntranceLoss { get; set; }
        public double ExitLoss { get; set; }
    }
}
