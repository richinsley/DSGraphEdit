using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DaggerLib.DSGraphEdit
{
    public partial class URLDialog : Form
    {
        public URLDialog()
        {
            InitializeComponent();
            _okButton.DialogResult = DialogResult.OK;
            _cancelButton.DialogResult = DialogResult.Cancel;
            
            // (re)populate the combobox with previous URLS
            comboBox1.Items.AddRange(URLItemsSingleton.Instance.Items.ToArray());
            if (comboBox1.Items.Count != 0)
            {
                comboBox1.SelectedIndex = 0;
            }

            this.FormClosing += new FormClosingEventHandler(URLDialog_FormClosing);
        }

        void URLDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // store the chosen URL in the URLItemsSingleton
            if (this.DialogResult == DialogResult.OK)
            {
                if (!URLItemsSingleton.Instance.Items.Contains(comboBox1.Text))
                {
                    URLItemsSingleton.Instance.Items.Add(comboBox1.Text);
                }
            }
        }

        public string URL
        {
            get
            {
                return comboBox1.Text;
            }
        }
    }

    /// <summary>
    /// A singleton pattern to store URLS in the running App instance and maintain them across
    /// different instances of URLDialogs.  It is most definitely not thread-safe though.
    /// </summary>
    internal sealed class URLItemsSingleton
    {
        static URLItemsSingleton instance = null;
        static private List<string> items;

        /// <summary>
        /// we only want the contructor available to the static Instance property
        /// </summary>
        private URLItemsSingleton()
        {
            if(items == null)
            {
                items = new List<string>();

                // see if there are any items stored in the registry
                RegistryKey MyKey = Registry.CurrentUser.OpenSubKey(@"Software\DSGraphEdit\URLS\");
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
        ~URLItemsSingleton()
        {
            if (items.Count == 0)
            {
                return;
            }

            // store at most 10 items in the Registry
            if (items.Count > 10)
            {
                items = items.GetRange(items.Count - 10, 10);
            }

            // store them under CurrentUser\Software\DSGraphEdit\URLS
            RegistryKey MyKey = Registry.CurrentUser.CreateSubKey(@"Software\DSGraphEdit\URLS\");
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
        public static URLItemsSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new URLItemsSingleton();
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