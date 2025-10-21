using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

[assembly: ExtensionApplication(typeof(JVA.C3D.DrainagePlugin.PluginEntry))]

namespace JVA.C3D.DrainagePlugin
{
    /// <summary>
    /// Entry point for JVA Civil 3D Drainage Plugin
    /// Provides drainage workflow automation for Colorado projects following Mile High Flood District standards
    /// </summary>
    public class PluginEntry : IExtensionApplication
    {
        public void Initialize()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;

            if (ed != null)
            {
                ed.WriteMessage("\n");
                ed.WriteMessage("╔════════════════════════════════════════════════════════╗\n");
                ed.WriteMessage("║   JVA Civil 3D Drainage Plugin                        ║\n");
                ed.WriteMessage("║   Version 1.0                                          ║\n");
                ed.WriteMessage("║   Mile High Flood District Standards                   ║\n");
                ed.WriteMessage("╚════════════════════════════════════════════════════════╝\n");
                ed.WriteMessage("\nAvailable Commands:\n");
                ed.WriteMessage("  JVA_ExportToStormCAD    - Export LandXML to StormCAD Excel\n");
                ed.WriteMessage("  JVA_UpdateStormCAD      - Update existing StormCAD Excel\n");
                ed.WriteMessage("  JVA_ImportMunicipality  - Import municipality data\n");
                ed.WriteMessage("  JVA_SyncCatchments      - Sync catchment basins\n");
                ed.WriteMessage("  JVA_QAReport            - Generate QA/QC report\n");
                ed.WriteMessage("  JVA_ExportDrainage      - Export drainage to CSV\n");
                ed.WriteMessage("  JVA_UpdateBasins        - Update basin properties\n");
                ed.WriteMessage("\n");
            }
        }

        public void Terminate()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;

            if (ed != null)
            {
                ed.WriteMessage("\nJVA Civil 3D Drainage Plugin unloaded.\n");
            }
        }
    }
}
