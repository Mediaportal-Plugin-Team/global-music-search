#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;

/*
 * void InitializeSearch		-> Initialize e.g. SQL data connections
 * bool DoSearch						-> True if successful, false if not (
 * bool LocateFirst					-> True if >0 results, false if not
 * SearchResult NextResult	-> null if there are no results anymore
 * void FinalizeSearch			-> Clean up all allocated blocks and finalize data connections
 */

namespace MediaPortal.Search
{    
    [XmlRoot("LastSearchItem")]
    public class LastSearchItem
    {
        [XmlElement("Phrase")]
        private string phrase = "";
        public string Phrase
        {
            get { return phrase; }
        }

        [XmlElement("SearchType")]
        private SearchGlobals.SearchType searchType = SearchGlobals.SearchType.Contains;
        public SearchGlobals.SearchType SearchType
        {
            get { return searchType; }
        }

        [XmlElement("SearchOptions")]
        private SearchGlobals.SearchOptions searchOptions = SearchGlobals.SearchOptions.CaseInsensitive;
        public SearchGlobals.SearchOptions SearchOptions
        {
            get { return searchOptions; }
        }

        public bool writeToConfig(MediaPortal.Profile.Settings settings, String sectionId)
        {
            try
            {
                settings.SetValue(sectionId, "Phrase", this.phrase);
                settings.SetValue(sectionId, "SearchType", Convert.ToString((int)this.searchType));
                settings.SetValue(sectionId, "SearchOptions", Convert.ToString((int)this.searchOptions));
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool readFromConfig(MediaPortal.Profile.Settings settings, String sectionId)
        {
            try
            {
                this.phrase = settings.GetValueAsString(sectionId, "Phrase", this.phrase);
                this.searchType = (SearchGlobals.SearchType)settings.GetValueAsInt(sectionId, "SearchType", (int)this.searchType);
                if (!Enum.IsDefined(typeof(SearchGlobals.SearchType), this.searchType)) this.searchType = SearchGlobals.SearchType.Contains;
                this.searchOptions = (SearchGlobals.SearchOptions)settings.GetValueAsInt(sectionId, "SearchOptions", (int)this.searchOptions);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public LastSearchItem(string phrase, SearchGlobals.SearchType searchType, SearchGlobals.SearchOptions searchOptions)
        {
            this.phrase = phrase;
            this.searchType = searchType;
            this.searchOptions = searchOptions;
        }

        public LastSearchItem()
        {
        }
    }

    public class LastSearchItemList : List<LastSearchItem>
    {
        private GlobalSearchSettings globalSearchSettings;
        protected GlobalSearchSettings GlobalSearchSettings
        {
            get { return globalSearchSettings; }
        }

        protected string filename = "";
        public string Filename
        {
            get { return filename; }
        }


        public bool readFromConfig()
        {
            return readFromConfig(this.Filename);
        }
        public bool readFromConfig(String filename)
        {
            using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(filename))
            {
                try
                {
                    this.Clear();
                    for (int i = 0; i < globalSearchSettings.NumberOfLastSearches; i++)
                    {
                        LastSearchItem lsi = new LastSearchItem();
                        lsi.readFromConfig(settings, "GlobalSearch.LastSearch." + Convert.ToString(i));
                        if (lsi.Phrase.Trim() != "")
                        {
                            this.Add(lsi);
                        }
                        else
                        {
                            break;
                        }
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool writeToConfig()
        {
            return writeToConfig(this.filename);
        }
        public bool writeToConfig(string filename)
        {
            using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(filename))
            {
                try
                {

                    for (int i = 0; i < globalSearchSettings.NumberOfLastSearches; i++)
                    {
                        try
                        {
                            settings.RemoveEntry("GlobalSearch.LastSearch." + Convert.ToString(i), "Phrase");
                            settings.RemoveEntry("GlobalSearch.LastSearch." + Convert.ToString(i), "SearchType");
                            settings.RemoveEntry("GlobalSearch.LastSearch." + Convert.ToString(i), "SearchOptions");

                        }
                        catch (Exception)
                        {
                            ;
                        }
                    }

                    for (int i = 0; i < Math.Min(this.Count, globalSearchSettings.NumberOfLastSearches); i++)
                    {
                        LastSearchItem lsi = this[i];
                        lsi.writeToConfig(settings, "GlobalSearch.LastSearch." + Convert.ToString(i));
                    }
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Godsamme -> " + e.Message); 
                    return false;
                }
            }
        }
        public void registerNewSearch(string phrase, SearchGlobals.SearchType searchType, SearchGlobals.SearchOptions searchOptions)
        {
            LastSearchItem lsi = new LastSearchItem(phrase.Trim(), searchType, searchOptions);

            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].Phrase.Trim().ToLower() == phrase.Trim().ToLower())
                {
                    this.RemoveAt(i);
                }
            }

            this.Insert(0, lsi);
            writeToConfig();
        }

        public void clearLastSearches()
        {
            this.Clear();
            writeToConfig();
        }

        public LastSearchItemList(GlobalSearchSettings globalSearchSettings, string configFilename)
        {
            this.globalSearchSettings = globalSearchSettings;
            this.filename = configFilename;
        }

    }

    public class SearchGlobals
    {
        public enum SearchType
        {
            Equals,
            StartsWith,
            Contains,
            EndsWith,
        }

        public enum SearchOptions
        {
            CaseSensitive = 1,
            CaseInsensitive = 2
        }
    }

    public class SearchResult
    {
        #region Private members
        private string _resultId;
        private string _item;
        private string _source;
        private string _filename;
        #endregion

        #region Public properties
        public string ResultId
        {
            get { return _resultId; }
            set { _resultId = value; }
        }

        public string Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public override string ToString()
        {
            return string.Format("item={0},source={1},filename={2}", _item, _source, _filename);
        }
        #endregion
    }

    public abstract class CustomSearchMethod
    {
        #region Private members
        protected string _searchPhrase = "";
        protected SearchGlobals.SearchOptions _options = SearchGlobals.SearchOptions.CaseInsensitive;
        protected SearchGlobals.SearchType _searchType = SearchGlobals.SearchType.Contains;
        #endregion

        #region Public properties
        public string SearchPhrase
        {
            get { return _searchPhrase; }
            set { _searchPhrase = value; }
        }

        public SearchGlobals.SearchOptions Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public SearchGlobals.SearchType Type
        {
            get { return _searchType; }
            set { _searchType = value; }
        }
        #endregion

        #region Methods
        public virtual string GetSearchName()
        {
            return "Undefined";
        }

        public virtual void InitializeSearch()
        {
            // No implementation yet, but there is no obligation to implement this in derived classes.
        }

        public abstract bool DoSearch();
        public abstract bool LocateFirst();
        public abstract SearchResult NextResult();
        public virtual void FinalizeSearch()
        {
            // No implementation yet, but there is no obligation to implement this in derived classes.
        }
        #endregion

    }

    public class GlobalSearch
    {
        public List<SearchResult> Results = new List<SearchResult>();
        public List<CustomSearchMethod> Methods = new List<CustomSearchMethod>();
        public List<String> Log = new List<string>();

        protected void DoLog(string message)
        {
            Log.Add(DateTime.Now.ToString("[yyyy-MM-dd][hh:mm:ss] - ") + message);
        }

        // Search may be (further) implemented by the derived classes.
        public virtual int Search()
        {
            int count = 0;
            DoLog(String.Format("Performing search for (D) methods...", Methods.Count));

            foreach (CustomSearchMethod method in Methods)
            {                
                StringList resultIdList = new StringList();
                
                // First initialize this search
                DoLog("Performing a search for type '" + method.GetSearchName() + "' with phrase '" + method.SearchPhrase + "'...");
                method.InitializeSearch();

                // When initialized actually search for the results
                DoLog("Search initialized, performing actual search...");
                method.DoSearch();

                // After the search is done, locate the first result
                DoLog("Search completed, checking for results...");
                if (method.LocateFirst())
                {
                    // There were some results, so get all the results
                    DoLog("Results found, retrieving data...");
                    SearchResult sr = method.NextResult();

                    while (sr != null)
                    {
                        if (resultIdList.IndexOf(sr.ResultId) == -1)
                        {
                            resultIdList.Add(sr.ResultId);
                            count++;
                            Results.Add(sr);
                        }
                        else
                        {
                            DoLog(String.Format("Skipping track with Id {0}, since it already was found earlier!", sr.ResultId));
                        }
                            
                        sr = method.NextResult();
                    }

                    DoLog(String.Format("Added {0} results to the results list...", count));
                }
                else
                {
                    // This search method didn't result in any results.
                    DoLog("No results found...");
                }

                // Finally, finalize the search and we're done!
                DoLog("Finalizing search.");
                method.FinalizeSearch();
            }
         
            return count;
        }
    }
}
