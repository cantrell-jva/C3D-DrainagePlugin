namespace JVA.C3D.DrainagePlugin.Models
{
    /// <summary>
    /// Represents a drainage structure (manhole, inlet, outlet) from Civil 3D
    /// </summary>
    public class DrainageStructure
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double RimElevation { get; set; }
        public double SumpElevation { get; set; }
        public double Northing { get; set; }
        public double Easting { get; set; }
        public string Description { get; set; }
        public double GroundElevation { get; set; }

        // StormCAD specific fields
        public string Label { get; set; }
        public double MaxPondedDepth { get; set; }
        public double PondedArea { get; set; }
    }
}
