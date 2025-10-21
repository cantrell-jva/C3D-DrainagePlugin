# JVA Civil 3D Drainage Plugin

A comprehensive .NET 8 plugin for Civil 3D 2025 to automate drainage design workflows for Colorado site development projects following Mile High Flood District (MHFD) standards.

## Overview

This plugin suite provides tools to improve the workflow between Civil 3D and StormCAD, enabling efficient round-trip updates of drainage networks.

## Key Features

### StormCAD Integration (Primary Feature)
- **Export to StormCAD**: Convert Civil 3D pipe networks from LandXML to StormCAD Model Builder Excel format
- **Update Existing Models**: Update existing StormCAD Excel files with changes from Civil 3D
- **Round-Trip Workflow**: Maintain synchronization between Civil 3D and StormCAD models

### Why This Workflow?

**The Problem**:
- Civil 3D can export gravity pipe networks as LandXML
- StormCAD can only import LandXML into NEW models
- Cannot update EXISTING StormCAD models with LandXML

**The Solution**:
- Use StormCAD's Model Builder with Excel files
- This plugin converts LandXML → Excel format
- Excel files can create AND update pipes/structures in StormCAD
- Enables true round-trip workflow for model updates

## Installation

### Prerequisites
- Civil 3D 2025
- .NET 8 Runtime
- Microsoft Excel (for StormCAD integration)
- Visual Studio 2022 or later (for building from source)

### Build Instructions

1. Clone or download this repository
2. Open `JVA.C3D.DrainagePlugin.csproj` in Visual Studio 2022
3. Update the ObjectARX 2025 DLL reference paths in the .csproj file to match your installation
4. Build the project (Release or Debug configuration)
5. The output DLL will be in `bin/Release/net8.0-windows/` or `bin/Debug/net8.0-windows/`

### Loading the Plugin

1. Open Civil 3D 2025
2. Type `NETLOAD` at the command line
3. Browse to and select `JVA.C3D.DrainagePlugin.dll`
4. The plugin will display a welcome message with available commands

For automatic loading, add the NETLOAD command to your `acad.lsp` startup script.

## Commands

### StormCAD Export Commands

#### `JVA_ExportToStormCAD`
Export a Civil 3D pipe network to StormCAD Model Builder Excel format.

**Workflow**:
1. In Civil 3D, export your pipe network to LandXML:
   - Select the pipe network
   - Right-click → Export to LandXML
   - Save the .xml file
2. Run `JVA_ExportToStormCAD` command
3. Select the LandXML file you just exported
4. Choose location and name for the output Excel file
5. The plugin will:
   - Parse all pipes and structures from the LandXML
   - Apply Mile High Flood District standards (Manning's n, etc.)
   - Create an Excel file with "Nodes" and "Conduits" worksheets
   - Format the data for StormCAD Model Builder

**Excel Output Format**:
- **Nodes Sheet**: Structures (manholes, inlets, outlets)
  - Label, Type, Elevations (Ground, Rim, Sump)
  - Coordinates (X, Y)
  - Ponding properties
- **Conduits Sheet**: Pipes
  - Label, Start/Stop Nodes
  - Length, Diameter, Shape, Material
  - Manning's n, Slopes, Inverts
  - Loss coefficients

#### `JVA_UpdateStormCAD`
Update an existing StormCAD Excel file with changes from Civil 3D.

**Use Case**: You've modified your pipe network in Civil 3D and need to update StormCAD.

**Workflow**:
1. Export updated pipe network to LandXML from Civil 3D
2. Run `JVA_UpdateStormCAD` command
3. Select the new LandXML file
4. Select the EXISTING StormCAD Excel file to update
5. The plugin will overwrite the data with updated information

**Note**: This preserves the Excel file structure while updating the pipe/structure data.

### Other Commands (Future Implementation)

- `JVA_ImportMunicipality` - Import municipality-specific standards
- `JVA_SyncCatchments` - Synchronize catchment basin data
- `JVA_QAReport` - Generate QA/QC reports for drainage design
- `JVA_ExportDrainage` - Export drainage data to CSV
- `JVA_UpdateBasins` - Batch update basin properties

## Configuration

The plugin uses `DrainageConfig.json` for project-specific settings.

### Default Configuration

```json
{
  "ProjectName": "Colorado Site Development",
  "MunicipalityStandard": "Mile High Flood District",
  "ManningsNValues": {
    "RCP": 0.013,
    "CONCRETE": 0.013,
    "PVC": 0.010,
    "HDPE": 0.012,
    "CMP": 0.024,
    "STEEL": 0.012
  },
  "DefaultEntranceLoss": 0.5,
  "DefaultExitLoss": 1.0,
  "AutoOpenExcelAfterExport": true
}
```

### Customization

Edit `DrainageConfig.json` in the plugin directory to:
- Set default Manning's n values by pipe material
- Adjust loss coefficients
- Configure default file paths
- Set municipality-specific standards

## Mile High Flood District Standards

The plugin incorporates MHFD design standards including:

**Manning's Roughness Coefficients**:
- RCP (Reinforced Concrete Pipe): 0.013
- PVC: 0.010
- HDPE: 0.012
- CMP (Corrugated Metal Pipe): 0.024

**Loss Coefficients**:
- Default entrance loss: 0.5
- Default exit loss: 1.0

**Structure Defaults**:
- Maximum ponded depth: 1.0 ft
- Ponded area: 0.0 ft²

These values can be customized in `DrainageConfig.json`.

## Architecture

### Project Structure

```
JVA.C3D.DrainagePlugin/
├── Commands/              # Civil 3D command implementations
│   └── ExportToStormCadCommand.cs
├── Services/              # Business logic services
│   ├── LandXmlParser.cs              # Parse LandXML files
│   ├── StormCadExcelGenerator.cs     # Generate Excel files
│   └── StormCadConversionService.cs  # Orchestrate conversion
├── Models/                # Data models
│   ├── DrainageStructure.cs
│   ├── DrainagePipe.cs
│   └── DrainageConfig.cs
├── UI/                    # User interface components
├── PluginEntry.cs         # Plugin entry point
└── DrainageConfig.json    # Configuration file
```

### Technology Stack

**Language**: C# (.NET 8)

**Why C#?**
- Native integration with Civil 3D (.NET API)
- Direct access to AutoCAD/Civil 3D object model
- Excellent XML parsing (System.Xml.Linq)
- Built-in Excel interop (Microsoft.Office.Interop.Excel)
- Single DLL deployment
- Can create custom ribbon buttons

**Key Libraries**:
- AutoCAD .NET API (ObjectARX 2025)
- System.Xml.Linq (LandXML parsing)
- Microsoft.Office.Interop.Excel (Excel generation)
- System.Text.Json (Configuration)

## Workflow Diagram

```
┌─────────────────┐
│   Civil 3D      │
│  Pipe Network   │
└────────┬────────┘
         │ Export LandXML
         ▼
┌─────────────────┐
│  LandXML File   │
└────────┬────────┘
         │ JVA_ExportToStormCAD
         ▼
┌─────────────────┐
│  Excel File     │ ◄── JVA_UpdateStormCAD (for updates)
│ (Model Builder) │
└────────┬────────┘
         │ Import via Model Builder
         ▼
┌─────────────────┐
│    StormCAD     │
│      Model      │
└─────────────────┘
```

## Usage Example

### Initial Export

1. In Civil 3D:
   ```
   - Select pipe network
   - Right-click → Export to LandXML
   - Save as "SiteA_Drainage.xml"
   ```

2. In Civil 3D command line:
   ```
   Command: JVA_ExportToStormCAD
   [Select LandXML file: SiteA_Drainage.xml]
   [Save Excel as: SiteA_Drainage_StormCAD.xlsx]

   Result: Excel file created with all pipes and structures
   ```

3. In StormCAD:
   ```
   - Use Model Builder
   - Import from Excel: SiteA_Drainage_StormCAD.xlsx
   - Model is created/updated
   ```

### Updating After Changes

1. Modify pipe network in Civil 3D
2. Export to LandXML again
3. Run `JVA_UpdateStormCAD`
4. Select new LandXML and existing Excel file
5. Re-import in StormCAD via Model Builder

## Troubleshooting

### Plugin won't load
- Verify .NET 8 runtime is installed
- Check ObjectARX 2025 DLL paths in .csproj
- Ensure Civil 3D 2025 compatibility

### LandXML parsing errors
- Verify the file is a valid LandXML 1.2 file
- Ensure it contains PipeNetworks element
- Check that structures and pipes have required attributes

### Excel generation fails
- Ensure Microsoft Excel is installed
- Check file permissions for output directory
- Close the target Excel file if it's open

### Manning's n values incorrect
- Edit `DrainageConfig.json`
- Update material mappings in ManningsNValues section
- Restart Civil 3D after changes

## Future Enhancements

- [ ] Direct Civil 3D API integration (export without LandXML intermediate)
- [ ] Custom ribbon tab with buttons
- [ ] Catchment basin area calculations
- [ ] QA/QC validation rules
- [ ] Import municipality design standards
- [ ] Automated rational method calculations
- [ ] Integration with HEC-RAS for complex hydraulics
- [ ] Support for additional StormCAD features (pumps, storage, etc.)

## Support

For issues, questions, or feature requests, please contact the development team.

## License

See LICENSE file for details.

## Version History

### Version 1.0 (Current)
- Initial release
- LandXML to StormCAD Excel conversion
- Mile High Flood District standards
- Export and Update commands
- Configuration support

---

**Developed for Colorado site development projects following Mile High Flood District drainage design standards.**
