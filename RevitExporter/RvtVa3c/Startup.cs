using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RvtVa3c
{
    public class Startup : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication app)
        {

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication app)
        {
            string tabName = "DotBim";
            app.CreateRibbonTab(tabName);

            RibbonPanel ribbonPanel = app.CreateRibbonPanel(tabName, "DotBim Export");

            AddPushButton(ribbonPanel, "Export to .bim", "CategoriesCmd");

            return Result.Succeeded;
        }

        private static readonly string Description = "Export revit to .bim";
        private void AddPushButton(RibbonPanel ribbonPanel, string itemName, string className)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            string assetsFolderPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Assets");

            string imagePath = Path.Combine(assetsFolderPath, "DotBim.png");

            PushButtonData pushButtonData = new PushButtonData(itemName, itemName, assemblyPath, $"RvtVa3c.{className}");

            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;

            pushButton.LargeImage = new BitmapImage(new Uri(imagePath));

            pushButton.LongDescription = $"{Description}";
        }
    }
}
