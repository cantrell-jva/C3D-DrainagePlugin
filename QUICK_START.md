# Quick Start Guide - StormCAD Export

## Installation

1. Load the plugin in Civil 3D:
   ```
   Command: NETLOAD
   [Select: JVA.C3D.DrainagePlugin.dll]
   ```

2. You should see the welcome message with available commands.

## Basic Workflow

### Export to StormCAD (First Time)

**Step 1: Export from Civil 3D**
```
1. Select your pipe network in Civil 3D
2. Right-click → Export to LandXML
3. Save as: MyProject_Drainage.xml
```

**Step 2: Convert to Excel**
```
Command: JVA_ExportToStormCAD

1. Select the LandXML file you just created
2. Save Excel file as: MyProject_StormCAD.xlsx
3. Wait for conversion (you'll see structure/pipe count)
4. Choose Yes to open Excel and verify the data
```

**Step 3: Import to StormCAD**
```
1. Open StormCAD
2. Use Model Builder
3. Import from Excel: MyProject_StormCAD.xlsx
4. Verify the model
```

### Update After Changes

When you modify the Civil 3D model:

**Step 1: Re-export LandXML**
```
1. Export pipe network to LandXML again
2. Save as: MyProject_Drainage_Updated.xml
```

**Step 2: Update Excel**
```
Command: JVA_UpdateStormCAD

1. Select the new LandXML file
2. Select your EXISTING Excel file: MyProject_StormCAD.xlsx
3. The data will be updated
```

**Step 3: Re-import to StormCAD**
```
1. In StormCAD, use Model Builder again
2. Import the updated Excel file
3. Model is updated with changes
```

## Excel File Format

The plugin creates two worksheets:

### Nodes Sheet
- Label, Type (Manhole/Inlet/Outlet)
- Ground/Rim/Sump Elevations
- X, Y Coordinates
- Ponding properties

### Conduits Sheet
- Label, Start/Stop Nodes
- Length, Diameter, Shape, Material
- Manning's n, Slopes, Inverts
- Loss coefficients

## Customization

Edit `DrainageConfig.json` in the plugin folder to customize:
- Manning's n values by material
- Loss coefficients
- Default ponding depths
- Auto-open Excel setting

## Common Issues

**"No PipeNetworks found in LandXML file"**
- Make sure you exported a PIPE NETWORK, not just pipes
- Verify the export completed successfully

**"Excel generation failed"**
- Close the Excel file if it's open
- Check you have write permissions to the folder

**Manning's n seems wrong**
- Check the material name in your Civil 3D parts list
- Update DrainageConfig.json if needed

## Tips

1. Use consistent naming for structures (MH-1, MH-2, etc.)
2. Verify elevations in Civil 3D before exporting
3. Keep LandXML and Excel files together in a project folder
4. Make a backup before updating existing Excel files
5. Review the Excel data before importing to StormCAD

## Commands Summary

| Command | Purpose |
|---------|---------|
| `JVA_ExportToStormCAD` | Create new Excel file from LandXML |
| `JVA_UpdateStormCAD` | Update existing Excel file |

---

For detailed documentation, see README.md
