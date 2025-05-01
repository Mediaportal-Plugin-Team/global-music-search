using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using System.Windows.Forms;

namespace MediaPortal.Search
{  
    [XmlRoot("GlobalSearchSettings")]
    public class GlobalSearchSettings
    {
        #region Instance code
        private GlobalSearchSettings()
        {
            currentFilename = Config.GetFile(Config.Dir.Config, "GlobalSearch.xml");
            this.lastSearchItems = new LastSearchItemList(this, currentFilename);
        }

        private static GlobalSearchSettings globalSearchSettings;
        public static GlobalSearchSettings Instance()
        {
            if (globalSearchSettings == null)
            {
                globalSearchSettings = new GlobalSearchSettings();
            }

            return globalSearchSettings;
        }
        #endregion

        #region Input / Output
        private string currentFilename;
        public bool LoadFromFile()
        {
            return this.LoadFromFile(currentFilename);
        }
        public bool LoadFromFile(string filename)
        {
            using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(filename))
            {
                try
                {
                    this.NumberOfLastSearches = reader.GetValueAsInt("GlobalSearch.Misc", "NumberOfLastSearches", this.NumberOfLastSearches);
                    this.PluginName = reader.GetValueAsString("GlobalSearch.Misc", "PluginName", this.PluginName);
                    this.lastSearchItems.readFromConfig();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }            
        }
        public bool SaveToFile()
        {
            return this.SaveToFile(currentFilename);
        }
        public bool SaveToFile(string filename)
        {
            using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings(filename))
            {
                try
                {
                    writer.SetValue("GlobalSearch.Misc", "NumberOfLastSearches", Convert.ToString(this.NumberOfLastSearches));
                    writer.SetValue("GlobalSearch.Misc", "PluginName", this.PluginName);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }           
        }
        #endregion        

        [XmlElement("SaveResultCount")]
        private int numberOfLastSearches = 10;
        public int NumberOfLastSearches
        {
            get { return numberOfLastSearches; }
            set { numberOfLastSearches = value; }
        }

        [XmlElement("LastSearches")]
        private LastSearchItemList lastSearchItems;
        public LastSearchItemList LastSearchItems
        {
            get { return lastSearchItems; }
        }

        [XmlElement("PluginName")]
        private string pluginName = "Search music";
        public string PluginName
        {
            get { return pluginName; }
            set { pluginName = value; }
        }

    }
}
