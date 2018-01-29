using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DaggerLib.DSGraphEdit
{
    public partial class DSFiltersPanel : UserControl
    {
        #region Fields

        private DSGraphEditPanel _associatedGraphPanel;

        #endregion

        #region ctor

        public DSFiltersPanel()
        {
            InitializeComponent();

            // Add the additional filter categories to the DSFilterPanel and sync the filters
            dsFilterTreeView1.AddCategory(new DMOAudioCaptureEffectsCategory());
            dsFilterTreeView1.AddCategory(new DMOAudioEffectsCategory());
            dsFilterTreeView1.AddCategory(new DMOVideoEffectsCategory());
            dsFilterTreeView1.AddCategory(new EncAPIEncodersCategory());
            dsFilterTreeView1.AddCategory(new EncAPIMultiplexersCategory());

            dsFilterTreeView1.DoubleClick += new EventHandler(dsFilterTreeView1_DoubleClick);
            // uncomment to allow DirectX Transform Categories
            // (they don't work very well outside of a DES graph)
            /*
            dsFilterTreeView1.AddCategory(new VideoEffects1Category());
            dsFilterTreeView1.AddCategory(new VideoEffects2Category());
            dsFilterTreeView1.AddCategory(new AudioEffects1Category());
            dsFilterTreeView1.AddCategory(new AudioEffects2Category());
            */

            dsFilterTreeView1.SyncFilters();

            // set the textbox to the last entry in the Singleton
            SearchItemsSingleton sitems = SearchItemsSingleton.Instance;
            if (sitems.Items.Count != 0)
            {
                _findFilterTextBox.Text = sitems.Items[sitems.Items.Count - 1];
                _findFilterTextBox.SelectAll();
            }
        }

        #endregion

        #region Private Methods

        void dsFilterTreeView1_DoubleClick(object sender, EventArgs e)
        {
            if (_associatedGraphPanel != null)
            {
                DSFilterTreeViewNode tn = dsFilterTreeView1.SelectedNode as DSFilterTreeViewNode;
                if (tn != null)
                {
                    _associatedGraphPanel.AddFilter(tn);
                }
            }
        }

        /// <summary>
        /// Show or hide the filters properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (splitContainer1.Panel2Collapsed)
            {
                _filterPropertiesButton.ImageIndex = 0;
                splitContainer1.Panel2Collapsed = false;
            }
            else
            {
                _filterPropertiesButton.ImageIndex = 1;
                splitContainer1.Panel2Collapsed = true;
            }
        }

        /// <summary>
        /// Updates the filter's property panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dsFilterTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            splitContainer1.Panel2.Controls.Clear();

            // do wee need to create a new properties panel?
            DSFilterTreeViewNode node = dsFilterTreeView1.SelectedNode as DSFilterTreeViewNode;
            if (node != null)
            {
                FilterPropertiesPanel p = new FilterPropertiesPanel(node);
                p.Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Add(p);

                _insertFilterButton.Enabled = true;
            }
            else
            {
                _insertFilterButton.Enabled = false;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the DSFilterTreeView
        /// </summary>
        [Browsable(false)]
        public DSFilterTreeView TreeView
        {
            get
            {
                return dsFilterTreeView1;
            }
        }

        /// <summary>
        /// Gets or sets the DSGraphEditPanel filters are inserted to
        /// </summary>
        [Category("Filter Panel")]
        [Description("Gets or sets the DSGraphEditPanel filters are inserted to")]
        public DSGraphEditPanel AssociatedGraphPanel
        {
            get
            {
                return _associatedGraphPanel;
            }
            set
            {
                _associatedGraphPanel = value;
                _insertFilterButton.Visible = (value != null);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the filters in the treeview
        /// </summary>
        public void SyncFilters()
        {
            dsFilterTreeView1.SyncFilters();
        }

        public DSFilterTreeViewNode FindFilter(string searchText)
        {
            // store the search text in the Singleton
            SearchItemsSingleton sitems = SearchItemsSingleton.Instance;
            if (!sitems.Items.Contains(searchText))
            {
                sitems.Items.Add(searchText);
            }

            searchText = searchText.ToLower();

            int startparent = 0;
            int startchild = 0;

            if(dsFilterTreeView1.SelectedNode != null)
            {
                if (dsFilterTreeView1.SelectedNode is DSFilterTreeViewNode)
                {
                    startparent = dsFilterTreeView1.SelectedNode.Parent.Index;
                    startchild = dsFilterTreeView1.SelectedNode.Index;
                }
                else
                {
                    startparent = dsFilterTreeView1.SelectedNode.Index;
                    startchild = 0;
                }
            }

            // search from selected to end
            DSFilterTreeViewNode found = FindFilterRange(searchText, startparent, startchild + 1, dsFilterTreeView1.Nodes.Count - 1, dsFilterTreeView1.Nodes[dsFilterTreeView1.Nodes.Count - 1].Nodes.Count - 1);
            if (found == null)
            {
                if (startparent != 0 && startchild != 0)
                {
                    // search from top to selected
                    found = FindFilterRange(searchText, 0, 0, startparent, startchild);
                }
            }
            
            // if a node was found, select it and expand it
            if (found != null)
            {
                dsFilterTreeView1.Select();
                found.Parent.Expand();
                dsFilterTreeView1.SelectedNode = found;
            }

            return found;
        }

        private DSFilterTreeViewNode FindFilterRange(string searchText, int startparentIndex, int startchildIndex, int endparentIndex, int endchildIndex)
        {
            for (int i = startparentIndex; i < dsFilterTreeView1.Nodes.Count; i++)
            {
                for (int u = startchildIndex; u < dsFilterTreeView1.Nodes[i].Nodes.Count; u++)
                {
                    if (dsFilterTreeView1.Nodes[i].Nodes[u].Text.ToLower().Contains(searchText))
                    {
                        return dsFilterTreeView1.Nodes[i].Nodes[u] as DSFilterTreeViewNode;
                    }

                    if (u == endchildIndex && i == endparentIndex)
                    {
                        return null;
                    }
                }

                // reset startchildIndex to zero after first iteration
                startchildIndex = 0;
            }
            return null;
        }

        #endregion

        #region Toolbar Buttons

        private void _refreshFiltersButton_Click(object sender, EventArgs e)
        {
            dsFilterTreeView1.SyncFilters();
        }

        #endregion

        private void _insertFilterButton_Click(object sender, EventArgs e)
        {
            DSFilterTreeViewNode node = dsFilterTreeView1.SelectedNode as DSFilterTreeViewNode;
            if (node != null)
            {
                _associatedGraphPanel.AddFilter(node);
            }
        }

        private void _expandAllButton_Click(object sender, EventArgs e)
        {
            dsFilterTreeView1.BeginUpdate();
            dsFilterTreeView1.ExpandAll();
            dsFilterTreeView1.EndUpdate();
        }

        private void _collapseAllButton_Click(object sender, EventArgs e)
        {
            dsFilterTreeView1.BeginUpdate();
            dsFilterTreeView1.CollapseAll();
            dsFilterTreeView1.EndUpdate(); ;
        }

        private void _findFilterTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (int)Keys.Enter)
            {
                if (_findFilterTextBox.Text != "")
                {
                    FindFilter(_findFilterTextBox.Text);
                }
            }
        }

        private void _findFilterButton_Click(object sender, EventArgs e)
        {
            if (_findFilterTextBox.Text != "")
            {
                FindFilter(_findFilterTextBox.Text);
            }
        }

        private void _findFilterTextBox_DropDown(object sender, EventArgs e)
        {
            // repopulate entries from the singleton
            SearchItemsSingleton sitems = SearchItemsSingleton.Instance;
            _findFilterTextBox.Items.Clear();
            _findFilterTextBox.Items.AddRange(sitems.Items.ToArray());
        }
    }

    /// <summary>
    /// A singleton pattern to store search string in the running App instance and maintain them across
    /// different instances of DSFilterPanels.  It is most definitely not thread-safe though.
    /// </summary>
    internal sealed class SearchItemsSingleton
    {
        static SearchItemsSingleton instance = null;
        static private List<string> items;

        /// <summary>
        /// we only want the contructor available to the static Instance property
        /// </summary>
        private SearchItemsSingleton()
        {
            if (items == null)
            {
                items = new List<string>();

                // see if there are any items stored in the registry
                RegistryKey MyKey = Registry.CurrentUser.OpenSubKey(@"Software\DSGraphEdit\SearchItems\");
                if (MyKey != null)
                {
                    try
                    {
                        string[] urls = MyKey.GetValueNames();
                        for (int i = 0; i < urls.Length; i++)
                        {
                            items.Add((string)MyKey.GetValue(urls[i]));
                        }
                    }
                    catch { }
                    finally
                    {
                        MyKey.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Destructor - Store the items in the registry
        /// </summary>
        ~SearchItemsSingleton()
        {
            if (items.Count == 0)
            {
                return;
            }

            // store at most 20 items in the Registry
            if (items.Count > 20)
            {
                items = items.GetRange(items.Count - 20, 20);
            }

            // store them under CurrentUser\Software\DSGraphEdit\SearchItems
            RegistryKey MyKey = Registry.CurrentUser.CreateSubKey(@"Software\DSGraphEdit\SearchItems\");
            if (MyKey != null)
            {
                try
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        MyKey.SetValue(i.ToString(), items[i]);
                    }
                }
                catch { }
                finally
                {
                    MyKey.Close();
                }
            }
        }

        /// <summary>
        /// Static property to get the actual Instance of of the class
        /// </summary>
        public static SearchItemsSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SearchItemsSingleton();
                }
                return instance;
            }
        }

        /// <summary>
        /// Get the Items stored in the singleton
        /// </summary>
        public List<string> Items
        {
            get
            {
                return items;
            }
        }
    }
}
