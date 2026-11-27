using Frosty.Core;
using FrostySdk.Managers;
using System.Windows.Media;

namespace OpenUnmodifiedPlugin
{
    /// <summary>
    /// Adds "Open Unmodified" to the Data Explorer's right-click menu.
    /// </summary>
    public class OpenUnmodifiedContextMenuItem : DataExplorerContextMenuExtension
    {
        // The editor reads these properties once while building the menu, so keep one instance of each.
        private static readonly ImageSource icon = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/OpenAsset.png") as ImageSource;

        private readonly RelayCommand command = new RelayCommand(
            execute: o => UnmodifiedAssetViewer.Open(App.SelectedAsset),
            canExecute: o => CanOpen(App.SelectedAsset));

        public override string ContextItemName => "Open Unmodified";

        public override ImageSource Icon => icon;

        public override RelayCommand ContextItemClicked => command;

        // Added assets have no original data to show (same rule as 1.0.7). Only EBX is supported.
        private static bool CanOpen(EbxAssetEntry entry) => entry != null && !entry.IsAdded && entry.Type != "EncryptedAsset";
    }
}
