using System;
using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using JVA.C3D.DrainagePlugin.Services;

[assembly: CommandClass(typeof(JVA.C3D.DrainagePlugin.Commands.ExportToStormCadCommand))]

namespace JVA.C3D.DrainagePlugin.Commands
{
    /// <summary>
    /// Command to export Civil 3D pipe network to StormCAD Model Builder Excel format
    /// </summary>
    public class ExportToStormCadCommand
    {
        [CommandMethod("JVA_ExportToStormCAD")]
        public void ExportToStormCAD()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage("\n=== Export to StormCAD Model Builder ===\n");

                // Prompt for LandXML file
                string landXmlPath = GetLandXmlFilePath(ed);
                if (string.IsNullOrEmpty(landXmlPath))
                {
                    ed.WriteMessage("\nCommand cancelled.\n");
                    return;
                }

                // Prompt for output Excel file
                string excelPath = GetExcelOutputPath(ed, landXmlPath);
                if (string.IsNullOrEmpty(excelPath))
                {
                    ed.WriteMessage("\nCommand cancelled.\n");
                    return;
                }

                // Perform conversion
                ed.WriteMessage("\nConverting LandXML to StormCAD Excel format...\n");

                var conversionService = new StormCadConversionService();
                var result = conversionService.ConvertLandXmlToStormCad(landXmlPath, excelPath);

                // Display results
                if (result.Success)
                {
                    ed.WriteMessage($"\n*** SUCCESS ***\n");
                    ed.WriteMessage($"Network: {result.NetworkName}\n");
                    ed.WriteMessage($"Structures: {result.StructureCount}\n");
                    ed.WriteMessage($"Pipes: {result.PipeCount}\n");
                    ed.WriteMessage($"Output: {result.ExcelOutputPath}\n");
                    ed.WriteMessage($"\n{result.Message}\n");

                    // Ask if user wants to open the Excel file
                    var openResult = MessageBox.Show(
                        $"Conversion successful!\n\n" +
                        $"Structures: {result.StructureCount}\n" +
                        $"Pipes: {result.PipeCount}\n\n" +
                        $"Would you like to open the Excel file?",
                        "Export to StormCAD",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (openResult == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = result.ExcelOutputPath,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    ed.WriteMessage($"\n*** ERROR ***\n");
                    ed.WriteMessage($"{result.ErrorMessage}\n");
                    MessageBox.Show(
                        $"Conversion failed:\n\n{result.ErrorMessage}",
                        "Export to StormCAD",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError: {ex.Message}\n");
                MessageBox.Show(
                    $"An error occurred:\n\n{ex.Message}",
                    "Export to StormCAD",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        [CommandMethod("JVA_UpdateStormCAD")]
        public void UpdateStormCAD()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage("\n=== Update StormCAD Model Builder Excel ===\n");

                // Prompt for LandXML file
                string landXmlPath = GetLandXmlFilePath(ed);
                if (string.IsNullOrEmpty(landXmlPath))
                {
                    ed.WriteMessage("\nCommand cancelled.\n");
                    return;
                }

                // Prompt for existing Excel file to update
                string excelPath = GetExistingExcelPath(ed);
                if (string.IsNullOrEmpty(excelPath))
                {
                    ed.WriteMessage("\nCommand cancelled.\n");
                    return;
                }

                // Perform update
                ed.WriteMessage("\nUpdating StormCAD Excel file...\n");

                var conversionService = new StormCadConversionService();
                var result = conversionService.UpdateStormCadExcel(landXmlPath, excelPath);

                // Display results
                if (result.Success)
                {
                    ed.WriteMessage($"\n*** SUCCESS ***\n");
                    ed.WriteMessage($"Updated {result.StructureCount} structures and {result.PipeCount} pipes.\n");

                    MessageBox.Show(
                        $"Update successful!\n\n" +
                        $"Structures: {result.StructureCount}\n" +
                        $"Pipes: {result.PipeCount}\n\n" +
                        $"The Excel file has been updated with the latest data from Civil 3D.",
                        "Update StormCAD",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    ed.WriteMessage($"\n*** ERROR ***\n");
                    ed.WriteMessage($"{result.ErrorMessage}\n");
                    MessageBox.Show(
                        $"Update failed:\n\n{result.ErrorMessage}",
                        "Update StormCAD",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError: {ex.Message}\n");
                MessageBox.Show(
                    $"An error occurred:\n\n{ex.Message}",
                    "Update StormCAD",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string GetLandXmlFilePath(Editor ed)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select LandXML File from Civil 3D";
                dialog.Filter = "LandXML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.CheckFileExists = true;

                // Try to default to the current drawing's directory
                string dwgPath = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Name;
                if (!string.IsNullOrEmpty(dwgPath) && File.Exists(dwgPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(dwgPath);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }

            return null;
        }

        private string GetExcelOutputPath(Editor ed, string landXmlPath)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Save StormCAD Excel File";
                dialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.DefaultExt = "xlsx";

                // Suggest a filename based on the LandXML file
                string suggestedName = Path.GetFileNameWithoutExtension(landXmlPath) + "_StormCAD.xlsx";
                dialog.FileName = suggestedName;
                dialog.InitialDirectory = Path.GetDirectoryName(landXmlPath);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }

            return null;
        }

        private string GetExistingExcelPath(Editor ed)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Existing StormCAD Excel File to Update";
                dialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.CheckFileExists = true;

                // Try to default to the current drawing's directory
                string dwgPath = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Name;
                if (!string.IsNullOrEmpty(dwgPath) && File.Exists(dwgPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(dwgPath);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }

            return null;
        }
    }
}
