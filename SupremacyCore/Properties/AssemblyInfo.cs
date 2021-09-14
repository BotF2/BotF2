using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

using Supremacy.Scripting.Runtime;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SupremacyCore")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Rise of the UFP Team")]
[assembly: AssemblyProduct("Rise of the UFP")]
[assembly: AssemblyCopyright("Copyright (c) 2021 Rise of the UFP Team")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a design in this assembly from 
// COM, set the ComVisible attribute to true on that design.
[assembly: ComVisible(false)]

[assembly: CLSCompliant(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6ed9a9ed-b062-4746-9eff-51b5d95eb377")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.7.0.3")]
[assembly: AssemblyFileVersion("1.7.0.3")]

[assembly: InternalsVisibleTo("SupremacyClient")]
[assembly: InternalsVisibleTo("SupremacyEditor")]
[assembly: InternalsVisibleTo("SupremacyEditor2")]
[assembly: InternalsVisibleTo("SupremacyEditor3")]
[assembly: InternalsVisibleTo("SupremacyTests")]
[assembly: InternalsVisibleTo("mscorlib")]
//[assembly: InternalsVisibleTo("SupremacyClient, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ed369a67489cfda80aabb5298d4e9be48501fc020a2bb64c72df0084d0e1d9e0ceabd98e63c80c707516a12299dc6eb6f2701249f3de8496cac2557572c3630b045564c986cfbe77e1b9c72cabe90e5b99485a2c4c745ad1b242605465988f5f9022315ca56f6f9b29fea9da505c7f2f021859bb4c7c61e4b670ce507d74baaa")]

[assembly: XmlnsPrefix("http://schemas.startreksupremacy.com/xaml/core", "s")]
[assembly: XmlnsPrefix("http://schemas.startreksupremacy.com/xaml/events", "se")]
[assembly: XmlnsPrefix("http://schemas.startreksupremacy.com/xaml/core/markup", "xs")]

[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Buildings")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Client")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Combat")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Diplomacy")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Economy")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Encyclopedia")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Entities")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Game")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core/markup", "Supremacy.Markup")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Orbitals")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Scripting")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/events", "Supremacy.Scripting.Events")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Tech")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Text")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Types")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Universe")]
[assembly: XmlnsDefinition("http://schemas.startreksupremacy.com/xaml/core", "Supremacy.Utility")]

[assembly: ScriptVisibleNamespace("Supremacy.Buildings")]
[assembly: ScriptVisibleNamespace("Supremacy.Client")]
[assembly: ScriptVisibleNamespace("Supremacy.Combat")]
[assembly: ScriptVisibleNamespace("Supremacy.Diplomacy")]
[assembly: ScriptVisibleNamespace("Supremacy.Economy")]
[assembly: ScriptVisibleNamespace("Supremacy.Effects")]
[assembly: ScriptVisibleNamespace("Supremacy.Encyclopedia")]
[assembly: ScriptVisibleNamespace("Supremacy.Entities")]
[assembly: ScriptVisibleNamespace("Supremacy.Game")]
[assembly: ScriptVisibleNamespace("Supremacy.Game", "SupremacyNative")]
[assembly: ScriptVisibleNamespace("Supremacy.Orbitals")]
[assembly: ScriptVisibleNamespace("Supremacy.Tech")]
[assembly: ScriptVisibleNamespace("Supremacy.Text")]
[assembly: ScriptVisibleNamespace("Supremacy.Types")]
[assembly: ScriptVisibleNamespace("Supremacy.Universe")]
[assembly: ScriptVisibleNamespace("Supremacy.Utility")]
[assembly: ScriptVisibleNamespace("Supremacy.Types", "SupremacyNative")]
[assembly: ScriptVisibleNamespace("Supremacy.Universe", "SupremacyNative")]
[assembly: ScriptVisibleNamespace("Supremacy.Utility", "SupremacyNative")]

[assembly: ScriptNamespaceAlias("s", "Supremacy.Buildings")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Client")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Combat")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Diplomacy")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Economy")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Effects")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Encyclopedia")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Entities")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Game")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Orbitals")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Tech")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Text")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Types")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Universe")]
[assembly: ScriptNamespaceAlias("s", "Supremacy.Utility")]