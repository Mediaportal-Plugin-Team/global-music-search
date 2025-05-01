using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace MediaPortal.Search
{
    public partial class FormGlobalSearchSettings : Form
    {
        protected GlobalSearchSettings globalSearchSettings;

        public FormGlobalSearchSettings()
        {
            InitializeComponent();
        }

        private void FormGlobalSearchSettings_Load(object sender, EventArgs e)
        {
            GlobalSearchSettings.Instance().LoadFromFile();
            this.globalSearchSettings = GlobalSearchSettings.Instance();

            pluginNameTextBox.Text = globalSearchSettings.PluginName;
            numMaxSearches.Value = globalSearchSettings.NumberOfLastSearches;

            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // save settings
            globalSearchSettings.PluginName = pluginNameTextBox.Text;
            globalSearchSettings.NumberOfLastSearches = (int)numMaxSearches.Value;

            globalSearchSettings.SaveToFile();
            Close();            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}