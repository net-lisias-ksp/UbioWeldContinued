using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KSP;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("UbioWeldingLtd")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("L Aerospace KSP Division")]
[assembly: AssemblyProduct("UbioWeldingLtd")]
[assembly: AssemblyCopyright("©2018 LisiasT")]
[assembly: AssemblyTrademark("Ubiozur, Makthur, Alexw, LisiasT")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("df819876-c8f8-4c18-8632-dad60efad236")]

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
[assembly: AssemblyVersion(UbioWeldingLtd.Version.Number)]
[assembly: AssemblyFileVersion(UbioWeldingLtd.Version.Number)]
[assembly: KSPAssembly("UbioWeldingLtd", UbioWeldingLtd.Version.major, UbioWeldingLtd.Version.minor)]

[assembly: KSPAssemblyDependency("KSPe", 2, 1)]
[assembly: KSPAssemblyDependency("KSPe.UI", 2, 1)]
[assembly: KSPAssemblyDependency("ToolbarController", 1, 0)]
