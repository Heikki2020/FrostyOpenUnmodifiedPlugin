using Frosty.Core;
using Frosty.Core.Controls;
using FrostySdk.IO;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenUnmodifiedPlugin
{
    /// <summary>
    /// Asset editor that shows the original (unmodified) EBX data.
    ///
    /// It is a FrostyAssetEditor subclass because FrostyPointerRefEditor walks up the visual tree looking for
    /// one and throws if it can't find it. All edits are rejected, and the property grid's Modified flag is not
    /// bound in the template, so nothing is ever written back through AssetManager.ModifyEbx().
    /// </summary>
    public class UnmodifiedAssetEditor : FrostyAssetEditor
    {
        static UnmodifiedAssetEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnmodifiedAssetEditor), new FrameworkPropertyMetadata(typeof(UnmodifiedAssetEditor)));
        }

        public UnmodifiedAssetEditor(ILogger logger) : base(logger)
        {
        }

        // 1.0.7's "openUnmodifiedData" path: skip ModifiedEntry.DataObject and read the original data
        protected override EbxAsset LoadAsset(EbxAssetEntry entry)
        {
            return App.AssetManager.GetEbx(entry, true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_AssetPropertyGrid") is FrostyPropertyGrid grid)
            {
                // belt and braces: veto every edit (assign, add, insert, remove, clear)
                grid.OnPreModified += (s, e) => e.Ignore = true;
            }
        }

        // no "View Instances" button: it can create/delete objects
        public override List<ToolbarItem> RegisterToolbarItems() => new List<ToolbarItem>();
    }

    /// <summary>
    /// Tab content for the editor window. IEditorWindow.OpenEditor() only takes a FrostyBaseEditor,
    /// so this just hosts the read-only asset editor.
    /// </summary>
    public class UnmodifiedAssetViewer : FrostyBaseEditor
    {
        private static readonly Dictionary<string, UnmodifiedAssetViewer> openViewers = new Dictionary<string, UnmodifiedAssetViewer>();
        private static readonly ImageSource icon = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/OpenAsset.png") as ImageSource;

        static UnmodifiedAssetViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnmodifiedAssetViewer), new FrameworkPropertyMetadata(typeof(UnmodifiedAssetViewer)));
        }

        public EbxAssetEntry Entry { get; }
        public UnmodifiedAssetEditor Editor { get; }

        public override ImageSource Icon => icon;

        public UnmodifiedAssetViewer(EbxAssetEntry entry)
        {
            Entry = entry;
            Editor = new UnmodifiedAssetEditor(App.Logger);
            Editor.SetAsset(entry); // shows the "Opening Asset" task window, calls LoadAsset()
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Host") is Border host && host.Child == null)
                host.Child = Editor;
        }

        public override void Closed()
        {
            Editor.Closed();
            openViewers.Remove(Entry.Name);
        }

        #region -- Open / de-duplicate --

        /// <summary>
        /// Opens (or re-selects) the read-only tab for an asset.
        /// </summary>
        public static void Open(EbxAssetEntry entry)
        {
            if (openViewers.TryGetValue(entry.Name, out UnmodifiedAssetViewer existing))
            {
                // already open: select its tab instead of opening a second one
                DependencyObject d = existing;
                while (d != null && !(d is TabItem))
                    d = VisualTreeHelper.GetParent(d);
                if (d is TabItem tab)
                {
                    tab.IsSelected = true;
                    return;
                }
                openViewers.Remove(entry.Name); // stale entry
            }

            UnmodifiedAssetViewer viewer;
            try
            {
                viewer = new UnmodifiedAssetViewer(entry);
            }
            catch (System.Exception ex)
            {
                App.Logger.LogError("Open Unmodified: unable to open {0}: {1}", entry.Name, ex.Message);
                Frosty.Controls.FrostyMessageBox.Show("Unable to open asset", "Frosty Editor");
                return;
            }

            openViewers[entry.Name] = viewer;
            App.EditorWindow.OpenEditor(entry.DisplayName + " (Read-only)", viewer);
        }

        #endregion
    }
}
