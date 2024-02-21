using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtVa3c.Model
{
    public class BuiltInCategoryID
    {
        public static List<BuiltInCategory> Ignores = new List<BuiltInCategory> { 
             BuiltInCategory.OST_TitleBlocks,
             BuiltInCategory.OST_GenericAnnotation,
             BuiltInCategory.OST_DetailComponents,
             BuiltInCategory.OST_Cameras,
             BuiltInCategory.OST_Views ,
             BuiltInCategory.OST_RvtLinks   ,
             BuiltInCategory.OST_Tags ,
             BuiltInCategory.OST_HVAC_Zones ,
             BuiltInCategory.OST_Lines ,
             BuiltInCategory.OST_ShaftOpening ,
             BuiltInCategory.OST_Sheets ,
        };

    }
}
