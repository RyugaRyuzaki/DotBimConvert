using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using RvtVa3c.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RvtVa3c.Model
{
    public class CategoryModel:BaseViewModel
    {
        public Document Doc { get; }
        private Category _Category;

		public Category Category
        {
			get { return _Category; }
			set { _Category = value;
                OnPropertyChanged();
            }
		}
        private bool _Checked=true;

        public bool Checked
        {
            get { return _Checked; }
            set { _Checked = value; OnPropertyChanged(); }
        }

        public CategoryModel(Category category)
        {
            Category = category; 
        }
		private List<Element> _Elements;

		public List<Element> Elements
        {
			get { return _Elements; }
			set { _Elements = value; OnPropertyChanged(); }
		}

        public CategoryModel(Document doc,Category category)
        {
            Doc=doc;
            Category=category;
            Elements = new FilteredElementCollector(Doc).WhereElementIsNotElementType().OfCategory((BuiltInCategory)(category.Id.IntegerValue)).Cast<Element>().ToList();
        }


    }
}
