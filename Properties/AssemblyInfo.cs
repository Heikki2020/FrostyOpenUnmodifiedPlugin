using Frosty.Core.Attributes;
using OpenUnmodifiedPlugin;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: ComVisible(false)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]

[assembly: Guid("7d2f6c1e-3b1a-4f7e-9a53-5b0c2e8a4d11")]

[assembly: PluginDisplayName("Open Unmodified")]
[assembly: PluginAuthor("Backport of the 'Open Unmodified' option from Frosty 1.0.7")]
[assembly: PluginVersion("1.0.0.0")]

[assembly: RegisterDataExplorerContextMenu(typeof(OpenUnmodifiedContextMenuItem))]
