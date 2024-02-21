﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RvtVa3c
{
    public class Utils

    {
        const string _caption = "DotBimConvert";
        public static void ErrorMsg(string msg)
        {
            Debug.WriteLine(msg);
            MessageBox.Show(msg,
              _caption,
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);
        }
        /// <summary>
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }

        /// <summary>
        /// Return a string for an XYZ point
        /// or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        /// <summary>
        /// Return an integer value for a Revit Color.
        /// </summary>
        public static string ColorToInt(Color color)
        {
            //int intColor = ((int)color.Red) << 16
            //  | ((int)color.Green) << 8
            //  | (int)color.Blue;

            string hex = RGBToHexadecimal((int)color.Red, (int)color.Green, (int)color.Blue);
            return hex;
        }

        public static string RGBToHexadecimal(int r, int g, int b)
        {
            string rs = DecimalToHexadecimal(r);
            string gs = DecimalToHexadecimal(g);
            string bs = DecimalToHexadecimal(b);

            return "0x" + rs + gs + bs;
        }

        private static string DecimalToHexadecimal(int dec)
        {
            if (dec <= 0)
                return "00";

            int hex = dec;
            string hexStr = string.Empty;

            while (dec > 0)
            {
                hex = dec % 16;

                if (hex < 10)
                    hexStr = hexStr.Insert(0, Convert.ToChar(hex + 48).ToString());
                else
                    hexStr = hexStr.Insert(0, Convert.ToChar(hex + 55).ToString());

                dec /= 16;
            }

            return hexStr;
        }


        /// <summary>
        /// Extract a true or false value from the given
        /// string, accepting yes/no, Y/N, true/false, T/F
        /// and 1/0. We are extremely tolerant, i.e., any
        /// value starting with one of the characters y, n,
        /// t or f is also accepted. Return false if no 
        /// valid Boolean value can be extracted.
        /// </summary>
        public static bool GetTrueOrFalse(
      string s,
      out bool val)
        {
            val = false;

            if (s.Equals(Boolean.TrueString,
              StringComparison.OrdinalIgnoreCase))
            {
                val = true;
                return true;
            }
            if (s.Equals(Boolean.FalseString,
              StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (s.Equals("1"))
            {
                val = true;
                return true;
            }
            if (s.Equals("0"))
            {
                return true;
            }
            s = s.ToLower();

            if ('t' == s[0] || 'y' == s[0])
            {
                val = true;
                return true;
            }
            if ('f' == s[0] || 'n' == s[0])
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return a string describing the given element:
        /// .NET type name,
        /// category name,
        /// family and symbol name for a family instance,
        /// element id and element name.
        /// </summary>
        public static string ElementDescription(
          Element e)
        {
            if (null == e)
            {
                return "<null>";
            }

            // For a wall, the element name equals the
            // wall type name, which is equivalent to the
            // family name ...

            FamilyInstance fi = e as FamilyInstance;

            string typeName = e.GetType().Name;

            string categoryName = (null == e.Category)
              ? string.Empty
              : e.Category.Name + " ";

            string familyName = (null == fi)
              ? string.Empty
              : fi.Symbol.Family.Name + " ";

            string symbolName = (null == fi
              || e.Name.Equals(fi.Symbol.Name))
                ? string.Empty
                : fi.Symbol.Name + " ";

            return string.Format("{0} {1}{2}{3}<{4} {5}>",
              typeName, categoryName, familyName,
              symbolName, e.Id.IntegerValue, e.Name);
        }

        /// <summary>
        /// Return a dictionary of all the given 
        /// element parameter names and values.
        /// </summary>
        public static Dictionary<string, string>
          GetElementProperties(
            Element e)
        {
            IList<Parameter> parameters
              = e.GetOrderedParameters();
            Dictionary<string, string> a
              = new Dictionary<string, string>(
                parameters.Count);
            if (e.Category != null)
            {
                if (e.Category.Name != null)
                {
                    a.Add("Category", e.Category.Name);
                } 
                if (e.Category.CategoryType!=CategoryType.Invalid)
                {
                    a.Add("BuiltInCategory", e.Category.BuiltInCategory.ToString());
                }
               
            } 
            
            a.Add("ElementID", e.Id.IntegerValue.ToString());
            string key;
            string val;
            foreach (Parameter p in parameters)
            {
                key = p.Definition.Name;
                if (!a.ContainsKey(key))
                {
                    if (StorageType.String == p.StorageType)
                    {
                        val = p.AsString();
                    }
                    else
                    {
                        val = p.AsValueString();
                    }

                    if (!string.IsNullOrEmpty(val))
                    {
                        a.Add(key, val);
                    }
                }
            }

            return a;
        }
    }
}
