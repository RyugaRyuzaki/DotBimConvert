
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Document = Autodesk.Revit.DB.Document;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using RvtVa3c.Model;
using Autodesk.Revit.Creation;
using Newtonsoft.Json.Linq;
using System;

namespace RvtVa3c.ViewModel
{



    public class ExportViewModel : BaseViewModel
    {
        public static readonly string navigateUri = "https://dotbim.net/#documentation";
        private static readonly CategoryType CategoryModelID = CategoryType.Model;

        public Document Doc { get; }

        private string _OutputFile;

        public string OutputFile
        {
            get { return _OutputFile; }
            set { _OutputFile = value; OnPropertyChanged(); }
        }
        private string _OutputFolder;

        public string OutputFolder
        {
            get { return _OutputFolder; }
            set { _OutputFolder = value; OnPropertyChanged(); }
        }

      


        private bool _CheckAllCategories;

        public bool CheckAllCategories
        {
            get { return _CheckAllCategories; }
            set
            {
                _CheckAllCategories = value;
                if (AllCategories == null && AllCategories.Count == 0) CanExportAll = false;
                if (AllCategories != null && AllCategories.Count > 0)
                {
                    foreach (var category in AllCategories)
                    {
                        category.Checked = value;
                    }
                }
                CanExportAll = AllCategories.All(cat => cat.Checked);
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CategoryModel> _AllCategories;

        public ObservableCollection<CategoryModel> AllCategories
        {
            get { return _AllCategories; }
            set
            {
                _AllCategories = value;
                OnPropertyChanged();
            }
        }


        private CategoryModel _SelectedCategory;

        public CategoryModel SelectedCategory
        {
            get { return _SelectedCategory; }
            set
            {
                _SelectedCategory = value;
                OnPropertyChanged();

            }
        }
        private bool _CanExportAll;

        public bool CanExportAll
        {
            get { return _CanExportAll; }
            set { _CanExportAll = value; OnPropertyChanged(); }
        }

        private bool _MergeFile = true;

        public bool MergeFile
        {
            get { return _MergeFile; }
            set { _MergeFile = value; OnPropertyChanged(); }
        }








        public bool IsOK { get; set; }

        public ICommand OpenWebCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand BrowseFileCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ExportViewModel(Document doc)
        {
            Doc = doc;

            OutputFile = GetSaveFileName();

            AllCategories = GetAllCategory();
            CheckAllCategories = true;
            BrowseFileCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                BrowseFile();

            });
            OpenWebCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                Process.Start(new ProcessStartInfo(navigateUri));

            });
            CloseWindowCommand = new RelayCommand<ExportWindow>((p) => { return true; }, (p) =>

            {
                IsOK = false;
                p.DialogResult = false;

            });


            ExportCommand = new RelayCommand<ExportWindow>((p) => { return true; }, (p) =>
            {
                List<CategoryModel> list = AllCategories.Where(c => c.Checked).ToList();
                if (list.Count == 0)
                {
                    TaskDialog.Show("Error", "None Selected Category!");
                }
                else
                {

                    IsOK = true;
                    p.DialogResult = false;
                }
            });
        }



        private string GetSaveFileName()
        {
            string filename = Doc.PathName;

            if (0 == filename.Length)
            {
                filename = Doc.Title;
            }
            if (null == OutputFolder)
            {

                try
                {
                    OutputFolder = Path.GetDirectoryName(
                   filename);
                }
                catch
                {
                    return string.Empty;
                }
            }

            filename = Path.GetFileNameWithoutExtension(filename);

            filename = Path.Combine(OutputFolder,
              filename);
            return filename;
        }



        private void BrowseFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "Select  Output File";
            dlg.Filter = "BIM files|*.bim";
            //dlg.Filter = "JSON files|*.json";

            if (null != OutputFolder
              && 0 < OutputFolder.Length)
            {
                dlg.InitialDirectory = OutputFolder;
            }

            dlg.FileName = OutputFile;

            bool rc = DialogResult.OK == dlg.ShowDialog();

            if (rc)
            {
                OutputFile = Path.Combine(dlg.InitialDirectory,
                  dlg.FileName);

                OutputFolder = Path.GetDirectoryName(
                  OutputFile);
            }

        }
        private ObservableCollection<CategoryModel> GetAllCategory()
        {
            List<Category> categories = new List<Category>();

            Categories categoriesSet = Doc.Settings.Categories;
            foreach (Category item in categoriesSet)
            {
                if (item.CategoryType == CategoryModelID && item.CanAddSubcategory) categories.Add(item);
            }

            List<Element> elements = new FilteredElementCollector(Doc).WhereElementIsNotElementType().Cast<Element>().ToList();

            List<Category> categories1 = new List<Category>();
            foreach (Category item in categories)
            {
                foreach (var item1 in elements)
                {
                    Category category = item1.Category;
                    if ((category != null) && (category.Name.Equals(item.Name))) categories1.Add(item);
                }
            }
            categories1 = new List<Category>(categories1.Distinct(new DistinctCategory()));
            return new ObservableCollection<CategoryModel>(categories1.Where(c => !BuiltInCategoryID.Ignores.Contains(c.BuiltInCategory)).OrderBy(x => x.Name).Select(c => new CategoryModel(Doc, c)).ToList());
        }

    }

}
