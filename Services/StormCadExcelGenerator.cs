using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using JVA.C3D.DrainagePlugin.Models;

namespace JVA.C3D.DrainagePlugin.Services
{
    /// <summary>
    /// Generates Excel files formatted for StormCAD Model Builder
    /// </summary>
    public class StormCadExcelGenerator : IDisposable
    {
        private Excel.Application _excelApp;
        private Excel.Workbook _workbook;
        private bool _disposed = false;

        public StormCadExcelGenerator()
        {
            _excelApp = new Excel.Application();
            _excelApp.DisplayAlerts = false;
            _excelApp.Visible = false;
        }

        /// <summary>
        /// Creates or updates a StormCAD Model Builder Excel file
        /// </summary>
        public void GenerateStormCadExcel(
            List<DrainageStructure> structures,
            List<DrainagePipe> pipes,
            string outputPath)
        {
            try
            {
                // Check if file exists to update or create new
                if (File.Exists(outputPath))
                {
                    _workbook = _excelApp.Workbooks.Open(outputPath);
                }
                else
                {
                    _workbook = _excelApp.Workbooks.Add();
                }

                // Create/update Nodes worksheet
                CreateNodesWorksheet(structures);

                // Create/update Conduits worksheet
                CreateConduitsWorksheet(pipes);

                // Save the workbook
                _workbook.SaveAs(outputPath);
                _workbook.Close();

                Marshal.ReleaseComObject(_workbook);
                _workbook = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating StormCAD Excel file: {ex.Message}", ex);
            }
        }

        private void CreateNodesWorksheet(List<DrainageStructure> structures)
        {
            Excel.Worksheet worksheet = GetOrCreateWorksheet("Nodes");

            // Clear existing data but keep headers
            var usedRange = worksheet.UsedRange;
            if (usedRange.Rows.Count > 1)
            {
                var dataRange = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[usedRange.Rows.Count, usedRange.Columns.Count]];
                dataRange.ClearContents();
            }

            // Set up headers (StormCAD Model Builder format)
            worksheet.Cells[1, 1] = "Label";
            worksheet.Cells[1, 2] = "Type";
            worksheet.Cells[1, 3] = "Ground Elevation (ft)";
            worksheet.Cells[1, 4] = "Rim Elevation (ft)";
            worksheet.Cells[1, 5] = "Sump Elevation (ft)";
            worksheet.Cells[1, 6] = "X (ft)";
            worksheet.Cells[1, 7] = "Y (ft)";
            worksheet.Cells[1, 8] = "Max Ponded Depth (ft)";
            worksheet.Cells[1, 9] = "Ponded Area (ft²)";
            worksheet.Cells[1, 10] = "Description";

            // Format headers
            Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 10]];
            headerRange.Font.Bold = true;
            headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightBlue);
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            // Write structure data
            int row = 2;
            foreach (var structure in structures)
            {
                worksheet.Cells[row, 1] = structure.Label ?? structure.Name;
                worksheet.Cells[row, 2] = structure.Type;
                worksheet.Cells[row, 3] = structure.GroundElevation;
                worksheet.Cells[row, 4] = structure.RimElevation;
                worksheet.Cells[row, 5] = structure.SumpElevation;
                worksheet.Cells[row, 6] = structure.Easting;
                worksheet.Cells[row, 7] = structure.Northing;
                worksheet.Cells[row, 8] = structure.MaxPondedDepth;
                worksheet.Cells[row, 9] = structure.PondedArea;
                worksheet.Cells[row, 10] = structure.Description;
                row++;
            }

            // Auto-fit columns
            worksheet.Columns.AutoFit();

            Marshal.ReleaseComObject(worksheet);
        }

        private void CreateConduitsWorksheet(List<DrainagePipe> pipes)
        {
            Excel.Worksheet worksheet = GetOrCreateWorksheet("Conduits");

            // Clear existing data but keep headers
            var usedRange = worksheet.UsedRange;
            if (usedRange.Rows.Count > 1)
            {
                var dataRange = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[usedRange.Rows.Count, usedRange.Columns.Count]];
                dataRange.ClearContents();
            }

            // Set up headers (StormCAD Model Builder format)
            worksheet.Cells[1, 1] = "Label";
            worksheet.Cells[1, 2] = "Start Node";
            worksheet.Cells[1, 3] = "Stop Node";
            worksheet.Cells[1, 4] = "Length (ft)";
            worksheet.Cells[1, 5] = "Diameter (in)";
            worksheet.Cells[1, 6] = "Shape";
            worksheet.Cells[1, 7] = "Material";
            worksheet.Cells[1, 8] = "Manning's n";
            worksheet.Cells[1, 9] = "Upstream Invert (ft)";
            worksheet.Cells[1, 10] = "Downstream Invert (ft)";
            worksheet.Cells[1, 11] = "Slope (ft/ft)";
            worksheet.Cells[1, 12] = "Entrance Loss";
            worksheet.Cells[1, 13] = "Exit Loss";

            // Format headers
            Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 13]];
            headerRange.Font.Bold = true;
            headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            // Write pipe data
            int row = 2;
            foreach (var pipe in pipes)
            {
                worksheet.Cells[row, 1] = pipe.Label ?? pipe.Name;
                worksheet.Cells[row, 2] = pipe.UpstreamStructure;
                worksheet.Cells[row, 3] = pipe.DownstreamStructure;
                worksheet.Cells[row, 4] = pipe.Length;
                worksheet.Cells[row, 5] = pipe.Diameter; // Convert to inches if needed
                worksheet.Cells[row, 6] = pipe.Shape;
                worksheet.Cells[row, 7] = pipe.Material;
                worksheet.Cells[row, 8] = pipe.ManningsN;
                worksheet.Cells[row, 9] = pipe.UpstreamInvert;
                worksheet.Cells[row, 10] = pipe.DownstreamInvert;
                worksheet.Cells[row, 11] = pipe.Slope;
                worksheet.Cells[row, 12] = pipe.EntranceLoss;
                worksheet.Cells[row, 13] = pipe.ExitLoss;
                row++;
            }

            // Auto-fit columns
            worksheet.Columns.AutoFit();

            Marshal.ReleaseComObject(worksheet);
        }

        private Excel.Worksheet GetOrCreateWorksheet(string name)
        {
            // Try to find existing worksheet
            foreach (Excel.Worksheet ws in _workbook.Worksheets)
            {
                if (ws.Name == name)
                {
                    return ws;
                }
                Marshal.ReleaseComObject(ws);
            }

            // Create new worksheet
            Excel.Worksheet newSheet = _workbook.Worksheets.Add();
            newSheet.Name = name;
            return newSheet;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_workbook != null)
                    {
                        _workbook.Close(false);
                        Marshal.ReleaseComObject(_workbook);
                        _workbook = null;
                    }

                    if (_excelApp != null)
                    {
                        _excelApp.Quit();
                        Marshal.ReleaseComObject(_excelApp);
                        _excelApp = null;
                    }
                }

                _disposed = true;
            }
        }

        ~StormCadExcelGenerator()
        {
            Dispose(false);
        }
    }
}
