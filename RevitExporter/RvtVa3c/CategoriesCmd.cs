#region Namespaces
using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using RvtVa3c.ViewModel;
using System.Collections.Generic;
using System.Linq;
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

        public void ExportView3D(View3D view3d, string filename, List<Category> categories,bool mergeFile)
        {
            AppDomain.CurrentDomain.AssemblyResolve
              += CurrentDomain_AssemblyResolve;

            Document doc = view3d.Document;


            Context context
              = new Context(doc, filename, categories, mergeFile);

            CustomExporter exporter = new CustomExporter(
              doc, context);


            exporter.ShouldStopOnError = false;

            exporter.Export(view3d);
        }

      

        

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
            ExportViewModel viewModel = new ExportViewModel( doc);
            ExportWindow window
                 = new ExportWindow(viewModel);
            bool? showDialog = window.ShowDialog();
            if (showDialog == null || showDialog == false)
            {
                if (viewModel.IsOK)
                {
                    try
                    {
                        List<Category> categories = viewModel.AllCategories.Where(c => c.Checked).Select(c => c.Category).ToList();
                        ExportView3D(view, viewModel.OutputFile, categories,viewModel.MergeFile);
                        return Result.Succeeded;
                    }
                    catch (Exception e)
                    {

                        TaskDialog.Show("Error",
                            e.Message);
                        return Result.Failed;
                    }
                }
                return Result.Cancelled;
            }



            return Result.Succeeded;
        }
    }
}
