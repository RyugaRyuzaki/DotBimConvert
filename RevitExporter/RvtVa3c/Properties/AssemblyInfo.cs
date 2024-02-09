#region Namespaces
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endregion // Namespaces

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "RvtVa3c" )]
[assembly: AssemblyDescription( "Revit custom exporter add-in generating JSON output for the va3c three.js AEC viewer" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Autodesk Inc." )]
[assembly: AssemblyProduct( "RvtVa3c" )]
[assembly: AssemblyCopyright( "Copyright 2014-2020 � Jeremy Tammik Autodesk Inc." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "321044f7-b0b2-4b1c-af18-e71a19252be0" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//
// History:
// 2014-09-02 2015.0.0.19 minor cleanup before removing scene definition
// 2014-09-03 2015.0.0.20 fixed bug in SelectFile, need to determine full output path
// 2014-09-03 2015.0.0.21 replace top level json container Scene for Object3D
// 2014-09-04 2015.0.0.23 added new models, theo confirmed it works, added name property to materials
// 2014-09-22 2015.0.0.24 added all six standard revit sample models
// 2014-10-28 2015.0.0.25 added support for runtime reading of user settings and switch between indented JSON or not to optionally reduce file size
// 2014-10-29 2015.0.0.26 user setting enhancements
// 2014-11-24 2015.0.0.27 skip elements with null category
// 2014-11-25 2015.0.0.28 skip elements with null category in OnElementEnd as well
// 2015-02-15 2015.0.0.29 incremented copyright year
// 2015-03-04 2015.0.0.30 integrated anapujol's UI to filter parameters, cf. description in https://github.com/va3c/RvtVa3c/pull/6
// 2015-04-13 2015.0.0.31 cleaned up after ana
// 2016-12-10 2017.0.0.0 flat migration to Revit 2017
// 2017-06-29 2018.0.0.0 flat migration to Revit 2018
// 2019-10-02 2020.0.0.0 flat migration to Revit 2020
// 2020-07-10 2020.0.0.1 integrated updated build events and debug settings for Revit 2020 from pull request #16 by @pabloderen
// 2020-07-10 2021.0.0.0 flat migration to Revit 2021
// 2020-12-14 2021.0.0.1 check for null category to avoid null reference exception in issue #18 (and #17?)
//
[assembly: AssemblyVersion( "2021.0.0.1" )]
[assembly: AssemblyFileVersion( "2021.0.0.1" )]
