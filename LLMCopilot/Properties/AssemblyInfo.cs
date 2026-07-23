using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LLMCopilot")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LLMCopilot")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Buffers",
    NewVersion = "4.0.5.0",
    OldVersionLowerBound = "4.0.0.0",
    OldVersionUpperBound = "4.0.5.0",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Diagnostics.DiagnosticSource",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.IO.Pipelines",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Net.ServerSentEvents",
    NewVersion = "10.0.0.2",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.2",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Memory",
    NewVersion = "4.0.5.0",
    OldVersionLowerBound = "4.0.2.0",
    OldVersionUpperBound = "4.0.5.0",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Memory.Data",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Numerics.Vectors",
    NewVersion = "4.1.6.0",
    OldVersionLowerBound = "4.1.0.0",
    OldVersionUpperBound = "4.1.6.0",
    PublicKeyToken = "b03f5f7f11d50a3a")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Runtime.CompilerServices.Unsafe",
    NewVersion = "6.0.3.0",
    OldVersionLowerBound = "6.0.0.0",
    OldVersionUpperBound = "6.0.3.0",
    PublicKeyToken = "b03f5f7f11d50a3a")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Text.Encodings.Web",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Text.Json",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "7.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "System.Threading.Tasks.Extensions",
    NewVersion = "4.2.4.0",
    OldVersionLowerBound = "4.2.1.0",
    OldVersionUpperBound = "4.2.4.0",
    PublicKeyToken = "cc7b13ffcd2ddd51")]

[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Bcl.AsyncInterfaces",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "cc7b13ffcd2ddd51")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.Diagnostics.Abstractions",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.Configuration.Abstractions",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.DependencyInjection.Abstractions",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.FileProviders.Abstractions",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.Hosting.Abstractions",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.Logging.Abstractions",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.Options",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
[assembly: ProvideBindingRedirection(
    AssemblyName = "Microsoft.Extensions.Primitives",
    NewVersion = "10.0.0.3",
    OldVersionLowerBound = "10.0.0.0",
    OldVersionUpperBound = "10.0.0.3",
    PublicKeyToken = "adb9793829ddae60")]
