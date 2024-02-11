#region Namespaces
using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using DialogResult = System.Windows.Forms.DialogResult;
#endregion // Namespaces

namespace RvtVa3c
{
    [Transaction(TransactionMode.Manual)]
    public class CategoriesCmd : IExternalCommand
    {
        /// <summary>
        /// Custom assembly resolver to find our support
        /// DLL without being forced to place our entire 
        /// application in a subfolder of the Revit.exe
        /// directory.
        /// </summary>
        /// 
        System.Reflection.Assembly
          CurrentDomain_AssemblyResolve(
            object sender,
            ResolveEventArgs args)
        {
            if (args.Name.Contains("Newtonsoft"))
            {
                string filename = Path.GetDirectoryName(
                  System.Reflection.Assembly
                    .GetExecutingAssembly().Location);

                filename = Path.Combine(filename,
                  "Newtonsoft.Json.dll");

                if (File.Exists(filename))
                {
                    return System.Reflection.Assembly
                      .LoadFrom(filename);
                }
            }
            return null;
        }

        public void ExportView3D(View3D view3d, string filename)
        {
            AppDomain.CurrentDomain.AssemblyResolve
              += CurrentDomain_AssemblyResolve;

            Document doc = view3d.Document;


            Context context
              = new Context(doc, filename);

            CustomExporter exporter = new CustomExporter(
              doc, context);

            //// Note: Excluding faces just suppresses the 
            //// OnFaceBegin calls, not the actual processing 
            //// of face tessellation. Meshes of the faces 
            //// will still be received by the context.
            ////

            exporter.ShouldStopOnError = false;

            exporter.Export(view3d);
        }

        #region SelectFile
        /// <summary>
        /// Store the last user selected output folder
        /// in the current editing session.
        /// </summary>
        static string _output_folder_path = null;

        /// <summary>
        /// Return true is user selects and confirms
        /// output file name and folder.
        /// </summary>
        static bool SelectFile(
          ref string folder_path,
          ref string filename)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "Select JSON Output File";
            dlg.Filter = "BIM files|*.bim";

            if (null != folder_path
              && 0 < folder_path.Length)
            {
                dlg.InitialDirectory = folder_path;
            }

            dlg.FileName = filename;

            bool rc = DialogResult.OK == dlg.ShowDialog();

            if (rc)
            {
                filename = Path.Combine(dlg.InitialDirectory,
                  dlg.FileName);

                folder_path = Path.GetDirectoryName(
                  filename);
            }
            return rc;
        }
        #endregion // SelectFile

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app
              = uiapp.Application;
            Document doc = uidoc.Document;

            // Check that we are in a 3D view.

            View3D view = doc.ActiveView as View3D;

            if (null == view)
            {
                Utils.ErrorMsg(
                  "You must be in a 3D view to export.");

                return Result.Failed;
            }

            // Prompt for output filename selection.

            string filename = doc.PathName;

            if (0 == filename.Length)
            {
                filename = doc.Title;
            }

            if (null == _output_folder_path)
            {
                // Sometimes the command fails if the file is 
                // detached from central and not saved locally

                try
                {
                    _output_folder_path = Path.GetDirectoryName(
                      filename);
                }
                catch
                {
                    TaskDialog.Show("Folder not found",
                      "Please save the file and run the command again.");
                    return Result.Failed;
                }
            }
            filename = Path.GetFileNameWithoutExtension(filename);

            if (!SelectFile(ref _output_folder_path,
              ref filename))
            {
                return Result.Cancelled;
            }


            ExportView3D(doc.ActiveView as View3D,
              filename);
            return Result.Succeeded;
        }
    }
}
