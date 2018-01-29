using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using WeifenLuo.WinFormsUI.Docking;
using DaggerLib.DSGraphEdit;
using DirectShowLib;

namespace DSGraphEdit
{
    public partial class Form1 : Form
    {
        private GraphNavigatorForm _graphNavigator;
        private FiltersForm _filtersForm;
        private GraphForm _recentFocus;
        private DSGraphEditPanelProperties _defaultProperties;
        private List<GraphForm> _Graphs = new List<GraphForm>();

        public Form1()
        {
            InitializeComponent();

            // hook the DockPanel's ActiveContentChanged event so we can update the Graph Navigator
            _dockPanel.ActiveContentChanged += new EventHandler(_dockPanel_ActiveContentChanged);

            // create the default tool windows
            _filtersForm = new FiltersForm();
            _graphNavigator = new GraphNavigatorForm();

            if (System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)) == 8)
            {
                this.Text += " (64-bit)";
            }
            else
            {
                this.Text += " (32-bit)";
            }
        }

        private GraphForm NewGraph()
        {
            GraphForm graphForm = new GraphForm();
            graphForm.DSGraphEditPanel.DSDaggerUIGraph.BackColor = Color.Teal;
            graphForm.FormClosing += new FormClosingEventHandler(graphForm_FormClosing);
            graphForm.DSGraphEditPanel.DSGraphEditPanelProperties = _defaultProperties;
            _Graphs.Add(graphForm);
            return graphForm;
        }

        void graphForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            (sender as GraphForm).DSGraphEditPanel.Dispose();
            _graphNavigator.AssociatedUIGraph = null;
            _filtersForm.AssociatedGraphPanel = null;
            _Graphs.Remove((GraphForm)sender);
            _recentFocus = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _filtersForm.Show(_dockPanel, DockState.DockLeft);
            _graphNavigator.Show(_filtersForm.Pane, DockAlignment.Top, 0.25);

            // load or create default graph options
            if (File.Exists("DSGraphEdit.options"))
            {
                try
                {
                    Stream filestream = File.OpenRead("DSGraphEdit.options");
                    BinaryFormatter bformatter = new BinaryFormatter();
                    _defaultProperties = (DSGraphEditPanelProperties)bformatter.Deserialize(filestream);
                    filestream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error reading default options");
                    _defaultProperties = new DSGraphEditPanelProperties();
                }
            }
            else
            {
                _defaultProperties = new DSGraphEditPanelProperties();
            }

            // create the initial blank graph
            GraphForm current = NewGraph();
            current.Show(_dockPanel, DockState.Document);
        }

        void _dockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            // see if the user has navigated to another GraphForm
            if (_dockPanel.ActiveContent is GraphForm)
            {
                _recentFocus = (GraphForm)_dockPanel.ActiveContent;
                _filtersForm.AssociatedGraphPanel = (_dockPanel.ActiveContent as GraphForm).DSGraphEditPanel;
                _graphNavigator.AssociatedUIGraph = (_dockPanel.ActiveContent as GraphForm).DSGraphEditPanel.DSDaggerUIGraph;
            }
        }

        #region Menu Events

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GraphForm current = NewGraph();
            current.Show(_dockPanel, DockState.Document);
        }

        private void renderURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            URLDialog ud = new URLDialog();
            if (ud.ShowDialog() == DialogResult.OK)
            {
                GraphForm current = _dockPanel.ActiveContent as GraphForm;
                if (current == null)
                {
                    current = NewGraph();
                }

                // we don't want to create a new DSGraphEdit panel, just render
                // the media file onto the existing graph
                try
                {
                    // show we're busy rendering and building up the ui for the graph
                    Cursor = Cursors.WaitCursor;

                    int hr = current.DSGraphEditPanel.RenderMediaFile(ud.URL);
                    if (hr != 0)
                    {
                        MessageBox.Show(DsError.GetErrorText(hr));
                    }
                    current.Show(_dockPanel, DockState.Document);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            ud.Dispose();
            ud = null;
        }

        private void renderMediaFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                GraphForm current = _dockPanel.ActiveContent as GraphForm;
                if (current == null)
                {
                    current = NewGraph();
                }
            
                // we don't want to create a new DSGraphEdit panel, just render
                // the media file onto the existing graph
                try
                {
                    // show we're busy rendering and building up the ui for the graph
                    Cursor = Cursors.WaitCursor;

                    int hr = current.DSGraphEditPanel.RenderMediaFile(ofd.FileName);
                    if (hr != 0)
                    {
                        MessageBox.Show(DsError.GetErrorText(hr));
                    }

                    current.Show(_dockPanel, DockState.Document);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            ofd.Dispose();
            ofd = null;
        }

        private void saveGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GraphForm current = _dockPanel.ActiveContent as GraphForm;
            if (current != null)
            {
                if (current.FilePath == "Untitled.grf")
                {
                    // hasn't been "Saved As" yet
                    saveGraphAsToolStripMenuItem_Click(null, null);
                }

                try
                {
                    current.SaveGraph(current.FilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving FilterGraph");
                }
            }
        }

        private void saveGraphAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GraphForm current = _dockPanel.ActiveContent as GraphForm;
            if (current != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = current.FilePath;
                sfd.DefaultExt = "grf";
                sfd.Filter = "Graph Files (*.grf)|*.grf";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        current.SaveGraph(sfd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Saving FilterGraph");
                    }
                }
            }
        }

        private void openGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "grf";
            ofd.Filter = "Graph Files (*.grf)|*.grf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    GraphForm graphForm = new GraphForm(ofd.FileName);
                    graphForm.DSGraphEditPanel.DSGraphEditPanelProperties = _defaultProperties;
                    graphForm.Show(_dockPanel, DockState.Document);
                }
                catch (Exception ex)
                {
                    // we failed, show the error and create a new blank GraphEditPanel
                    MessageBox.Show(ex.Message, "Error loading graph file");
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void connectToRemoteGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // attempt connection to a remote graph
                DSGraphEditPanel newpanel = DSGraphEditPanel.ConnectToRemoteGraph();

                // create a GraphForm for the panel
                if (newpanel != null)
                {
                    // get the RemoteGraphTile image from the assembly
                    GraphForm graphForm = new GraphForm(newpanel);
                    _Graphs.Add(graphForm);

                    Stream s = this.GetType().Assembly.GetManifestResourceStream("DSGraphEdit.RemoteGraphTile.jpg");
                    // seems if you close a stream after loading an image, TextureBrush constructors die a horrible death
                    newpanel.DSDaggerUIGraph.BackgroundImage = new Bitmap(s);
                    newpanel.DSGraphEditPanelProperties = _defaultProperties;
                    graphForm.Show(_dockPanel, DockState.Document);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error connecting to remote graph");
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void aboutDSGraphEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm ab = new AboutForm();
            ab.ShowDialog(this);
            ab.Dispose();
            ab = null;
        }

        private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // check the tool window menu items if they're visible
            filtersToolStripMenuItem.Checked = _filtersForm.Visible;
            graphNavigatorToolStripMenuItem.Checked = _graphNavigator.Visible;
        }

        private void graphNavigatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_graphNavigator.Visible)
            {
                _graphNavigator.Show(_dockPanel);
            }
        }

        private void filtersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_filtersForm.Visible)
            {
                _filtersForm.Show(_dockPanel);
            }
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // mark the Save menu items ebabled/disabled depending on if there is a Graph in focus
            if (_recentFocus == null)
            {
                saveGraphAsToolStripMenuItem.Enabled = false;
                saveGraphToolStripMenuItem.Enabled = false;
                saveCanvasImageToolStripMenuItem.Enabled = false;
            }
            else
            {
                saveGraphAsToolStripMenuItem.Enabled = true;
                saveGraphToolStripMenuItem.Enabled = true;
                saveCanvasImageToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// Save the Graph's Canvas as a Bitmap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveCanvasImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GraphForm current = _dockPanel.ActiveContent as GraphForm;
            if (current != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = Path.GetFileNameWithoutExtension(current.FilePath) + ".bmp";
                sfd.DefaultExt = "bmp";
                sfd.Filter = "Bitmap Files (*.bmp)|*.bmp";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Bitmap b = current.DSGraphEditPanel.CanvasImage;
                        b.Save(sfd.FileName);

                        // this is a Copy, not the actual Canvas image, so we should dispose of it
                        b.Dispose();
                        b = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Saving Canvas Image");
                    }
                }
            }
        }

        private void defaultGraphOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PropertiesDialog pd = new PropertiesDialog(_defaultProperties);
            if (pd.ShowDialog() == DialogResult.OK)
            {
                _defaultProperties = pd.DSGraphEditPanelProperties;

                //set the properties of any open Graphs
                foreach (GraphForm graph in _Graphs)
                {
                    graph.DSGraphEditPanel.DSGraphEditPanelProperties = _defaultProperties;
                }

                // serialize the properties
                try
                {
                    FileStream fs = new FileStream("DSGraphEdit.options", FileMode.Create);
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(fs, _defaultProperties);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error writing default options");
                }
            }
            pd.Dispose();
            pd = null;
        }

        #endregion
    }
}