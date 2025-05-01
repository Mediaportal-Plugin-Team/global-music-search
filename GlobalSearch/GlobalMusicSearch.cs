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
using MediaPortal.Music.Database;
using SQLite.NET;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//using MyLyrics;
using System.Text.RegularExpressions;

namespace MediaPortal.Search
{
    public class GlobalMusicSearch : CustomSearchMethod
    {
        internal const string LyricsDBName = "LyricsDatabaseV2.db";
        /*
        private void LoadLyricsDatabase()
        {
            string path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, LyricsDBName);

            // Open database to read data from
            FileStream fs = new FileStream(path, FileMode.Open);

            // Create a BinaryFormatter object to perform the deserialization
            BinaryFormatter bf = new BinaryFormatter();

            // Use the BinaryFormatter object to deserialize the database
            lyricsDatabase = (LyricsDatabase)bf.Deserialize(fs);
            fs.Close();
        }
        */

        private string getSongFileByArtistAndTitle(string artist, string title)
        {
            StringList sql = new StringList();
            sql.Add("SELECT strPath FROM tracks WHERE");
            sql.Add(string.Format(@"Upper(strArtist) GLOB ""*{0}*"" AND", artist.ToUpper()));
            sql.Add(string.Format(@"Upper(strTitle) = ""{0}"";", title.ToUpper()));
            SQLiteResultSet rs = MusicDatabase.DirectExecute(sql.Text);
            if (rs.Rows.Count > 0) 
            {
               return rs.Rows[0].fields[0];
            }

            return "";
        }

        private string stripStrangeCharacters(string source)
        {
            return source.Replace("|", "").Trim();
        }
        
        public enum MusicSearchFields
        {
            Artist = 1,
            Album = 2,
            Genre = 4,
            Songname = 8,
            Year = 16,
            Filename = 32,
            Lyrics = 64
        }

        public static string STR_MUSIC = MediaPortal.GUI.Library.GUILocalizeStrings.Get(249);
        public static string STR_ARTIST = MediaPortal.GUI.Library.GUILocalizeStrings.Get(484);
        public static string STR_ALBUM = MediaPortal.GUI.Library.GUILocalizeStrings.Get(483);
        public static string STR_GENRE = MediaPortal.GUI.Library.GUILocalizeStrings.Get(174).Replace(":", "");
        public static string STR_SONGNAME = MediaPortal.GUI.Library.GUILocalizeStrings.Get(179);
        public static string STR_YEAR = MediaPortal.GUI.Library.GUILocalizeStrings.Get(345);
        public static string STR_FILENAME = MediaPortal.GUI.Library.GUILocalizeStrings.Get(863);
        public static string STR_PATH = "Path";
        public static string STR_LYRICS = "Lyrics";

        public static List<string> SearchFieldStrings = new List<string>(new string[] { MusicSearchFields.Artist.ToString(), MusicSearchFields.Album.ToString(), MusicSearchFields.Songname.ToString(), MusicSearchFields.Lyrics.ToString(), MusicSearchFields.Genre.ToString(), MusicSearchFields.Year.ToString(), MusicSearchFields.Filename.ToString() });
        public static List<string> SearchFieldTranslatedStrings = new List<string>(new string[] { STR_ARTIST, STR_ALBUM, STR_SONGNAME, STR_LYRICS, STR_GENRE, STR_YEAR, STR_FILENAME });
        public static List<MusicSearchFields> SearchFieldValues = new List<MusicSearchFields>(new MusicSearchFields[] { MusicSearchFields.Artist, MusicSearchFields.Album, MusicSearchFields.Songname, MusicSearchFields.Lyrics, MusicSearchFields.Genre, MusicSearchFields.Year, MusicSearchFields.Filename });

        #region MusicSearchFields - Used to determine in which fields we need to search
        private MusicSearchFields _searchFields =
            MusicSearchFields.Artist | MusicSearchFields.Album |
            MusicSearchFields.Genre | MusicSearchFields.Songname |
            MusicSearchFields.Year | MusicSearchFields.Filename;

        public MusicSearchFields GetSearchFields()
        {
            return _searchFields;
        }
        public void IncludeSearchFields(MusicSearchFields fields)
        {
            _searchFields = _searchFields | fields;
        }
        public void ExcludeSearchFields(MusicSearchFields fields)
        {
            _searchFields = _searchFields & fields;
        }
        public bool ContainsSearchField(MusicSearchFields fields)
        {
            return CSet.In(fields, _searchFields);
        }
        public void SetSearchFields(MusicSearchFields fields)
        {
            _searchFields = fields;
        }
        public void EmptySearchFields()
        {
            _searchFields = 0;
        }
        #endregion

        private string _lastError = "";
        private string _statement = "";
        private int _curRow = 0;
        private bool _datasetSearchDone = false;
        //private IEnumerator<KeyValuePair<String, LyricsItem>> _lyricsEnumerator = null;
        private List<SearchResult> _lyricsList = new List<SearchResult>();

        private SQLiteResultSet _resultSet = null;
        //private LyricsDatabase lyricsDatabase = null;
        private StringList _terms = new StringList();

        /* Check whether the given result matches the searchphrase.
         * This method handles the case sensitivity and the position where to find the match
         * (Equals, StartsWith, etc). Returns true when it's a hit.
         */
        private bool searchMatches(string resultToSearch)
        {
            if (resultToSearch.StartsWith("| ")) resultToSearch = resultToSearch.Substring(2);
            if (resultToSearch.EndsWith(" |")) resultToSearch = resultToSearch.Substring(0, resultToSearch.Length - 2);
            
            String _phrase = new String(SearchPhrase.ToCharArray());

            if (!(_options == SearchGlobals.SearchOptions.CaseSensitive))
            {
                resultToSearch = resultToSearch.ToUpper();
                _phrase = _phrase.ToUpper();
            }

            switch (_searchType)
            {
                case SearchGlobals.SearchType.Equals:
                    return resultToSearch.Equals(_phrase);

                case SearchGlobals.SearchType.StartsWith:
                    return resultToSearch.StartsWith(_phrase);

                case SearchGlobals.SearchType.Contains:
                    {
                        StringList phraseList = new StringList();
                        phraseList.Delimiter = ' ';
                        phraseList.DelimitedText = _phrase;
                        bool bResult = true;

                        for (int i = 0; i < phraseList.Count; i++)
                        {
                            bResult = resultToSearch.Contains(phraseList.StringItem(i));
                            if (!bResult)
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                case SearchGlobals.SearchType.EndsWith:
                    return resultToSearch.EndsWith(_phrase);
            }

            return false;
        }

        public override string GetSearchName()
        {
            return GlobalMusicSearch.STR_MUSIC;
        }

        private string generatePartitialWhereClause(string fieldName, string phrase)
        {
            return generatePartitialWhereClause(fieldName, phrase, "", "");
        }

        private string generatePartitialWhereClause(string fieldName, string phrase, string prefix, string postfix)
        {
            if (_searchType == SearchGlobals.SearchType.Contains)
            {
                StringList phraseList = new StringList();
                phraseList.Delimiter = ' ';
                phraseList.DelimitedText = phrase;
                string sResult = "(";

                for (int i = 0; i < phraseList.Count; i++)
                {
                    string realPhrase = string.Format("GLOB {0}\"{3}*{1}*{4}\"{2}", (_options == SearchGlobals.SearchOptions.CaseInsensitive ? "UPPER(" : ""), phraseList.StringItem(i), (_options == SearchGlobals.SearchOptions.CaseInsensitive ? ")" : ""), prefix, postfix);
                    sResult += string.Format(" ({0}{2}{1} " + realPhrase + ")", (_options == SearchGlobals.SearchOptions.CaseInsensitive ? "UPPER(" : ""), (_options == SearchGlobals.SearchOptions.CaseInsensitive ? ")" : ""), fieldName);
                    if (i < phraseList.Count - 1)
                    {
                        sResult += " AND ";
                    }
                }
                sResult += ")";

                return sResult;
            }
            else
            {
                string realPhrase = "";
                if (_options == SearchGlobals.SearchOptions.CaseInsensitive) phrase = phrase.ToUpper();

                switch (_searchType)
                {
                    case SearchGlobals.SearchType.Equals: realPhrase = string.Format("= \"{1}{0}{2}\"", phrase, prefix, postfix); break;
                    case SearchGlobals.SearchType.StartsWith: realPhrase = string.Format("GLOB \"{1}{0}*{2}\"", phrase, prefix, postfix); break;
                    case SearchGlobals.SearchType.EndsWith: realPhrase = string.Format("GLOB \"{1}{0}*{2}\"", phrase, prefix, postfix); break;
                }

                return string.Format(" ({0}{2}{1}) " + realPhrase, (_options == SearchGlobals.SearchOptions.CaseInsensitive ? "UPPER(" : ""), (_options == SearchGlobals.SearchOptions.CaseInsensitive ? ")" : ""), fieldName);
            }
        }

        public override void InitializeSearch()
        {
            // Load lyrics database if it exists.
            if (File.Exists(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, LyricsDBName)))
            {
                /*
                try
                {
                    LoadLyricsDatabase();
                }
                catch (Exception)
                {
                    lyricsDatabase = null;
                }
                 */
            }

            string realPhrase = SearchPhrase;

            
            _terms.Clear();
            _terms.Delimiter = ' ';
            string s = Regex.Replace(realPhrase, @"[^a-zA-Z0-9 ]", "");
            while (s.IndexOf("  ") > -1) {
                s = s.Replace("  ", " ");
            }
            _terms.DelimitedText = s.ToLower();
            
            StringList sql = new StringList();

            // Generate default statement
            sql.Add("SELECT");

            sql.Add("  idTrack as Id, ");
            sql.Add("  strArtist as " + MusicSearchFields.Artist.ToString() + ", ");
            sql.Add("  strAlbum as " + MusicSearchFields.Album.ToString() + ", ");
            sql.Add("  strGenre as " + MusicSearchFields.Genre.ToString() + ", ");
            sql.Add("  strTitle as " + MusicSearchFields.Songname.ToString() + ", ");
            sql.Add("  iYear as " + MusicSearchFields.Year.ToString() + ", ");
            sql.Add("  strPath as " + MusicSearchFields.Filename.ToString());
            sql.Add("FROM");
            sql.Add("  TRACKS");
            sql.Add("WHERE");

            bool bNeedOr = false;

            if (CSet.In(MusicSearchFields.Album, _searchFields))
            {
                sql.Add((bNeedOr ? "OR" : "") + generatePartitialWhereClause(MusicSearchFields.Album.ToString(), realPhrase));
                bNeedOr = true;
            }
            if (CSet.In(MusicSearchFields.Artist, _searchFields))
            {
                sql.Add((bNeedOr ? "OR" : "") + generatePartitialWhereClause(MusicSearchFields.Artist.ToString(), realPhrase, "| ", " |"));
                bNeedOr = true;
            }
            if (CSet.In(MusicSearchFields.Filename, _searchFields))
            {
                sql.Add((bNeedOr ? "OR" : "") + generatePartitialWhereClause(MusicSearchFields.Filename.ToString(), realPhrase));
                bNeedOr = true;
            }
            if (CSet.In(MusicSearchFields.Genre, _searchFields))
            {
                sql.Add((bNeedOr ? "OR" : "") + generatePartitialWhereClause(MusicSearchFields.Genre.ToString(), realPhrase, "| ", " |"));
                bNeedOr = true;
            }
            if (CSet.In(MusicSearchFields.Songname, _searchFields))
            {
                sql.Add((bNeedOr ? "OR" : "") + generatePartitialWhereClause(MusicSearchFields.Songname.ToString(), realPhrase));
                bNeedOr = true;
            }
            if (CSet.In(MusicSearchFields.Year, _searchFields))
            {
                sql.Add((bNeedOr ? "OR" : "") + generatePartitialWhereClause(MusicSearchFields.Year.ToString(), realPhrase));
            }

            _statement = sql.Text;
        }


        public override bool DoSearch()
        {
            try
            {
                _resultSet = MusicDatabase.DirectExecute(_statement);
                return true;
            }
            catch (SQLiteException e)
            {
                _lastError = e.Message;
                return false;
            }
        }

        public override bool LocateFirst()
        {
            _curRow = 0;
            _datasetSearchDone = false;
            return true;
        }

        public override SearchResult NextResult()
        {
            if (_curRow < _resultSet.Rows.Count)
            {
                SearchResult _searchResult = new SearchResult();
                SQLiteResultSet.Row _row = _resultSet.Rows[_curRow];
                _searchResult.ResultId = _row.fields[_resultSet.ColumnNames.IndexOf("Id")];

                foreach (string fieldName in SearchFieldStrings)
                {
                    if (_resultSet.ColumnNames.IndexOf(fieldName) == -1)
                    {
                        continue;
                    }
                    
                    bool bSkip = true;
                    foreach (MusicSearchFields field in SearchFieldValues)
                    {
                        if ((CSet.In(field, _searchFields)) && (fieldName.Equals(field.ToString())))
                        {
                            bSkip = false;
                            break;
                        }
                    }

                    if (bSkip)
                    {
                        continue;
                    }

                    string value = _row.fields[_resultSet.ColumnNames.IndexOf(fieldName)];
                    if (searchMatches(value))
                    {
                        _searchResult.Item = stripStrangeCharacters(_row.fields[_resultSet.ColumnNames.IndexOf(MusicSearchFields.Artist.ToString())]) + " - " + stripStrangeCharacters(_row.fields[_resultSet.ColumnNames.IndexOf(MusicSearchFields.Songname.ToString())]);
                        _searchResult.Source = fieldName;
                        _searchResult.Filename = _row.fields[_resultSet.ColumnNames.IndexOf(MusicSearchFields.Filename.ToString())];
                        break;
                    }
                }

                _curRow++;
                return _searchResult;
            }
            else
            {
                if (!this._datasetSearchDone)
                {
                    this._datasetSearchDone = true;
                    /*
                    if (lyricsDatabase != null)
                    {
                        this._lyricsEnumerator = lyricsDatabase.GetEnumerator();
                        this._lyricsList.Clear();
                    }
                     */
                }
            }

            if (this._datasetSearchDone)
            {
                /*
                if (CSet.In(MusicSearchFields.Lyrics, _searchFields) && (lyricsDatabase != null))
                {

                    SearchResult sr = null;
                    while ((_lyricsEnumerator.MoveNext()) && (sr == null))
                    {
                        bool lyricFound = true;
                        string checkString = Regex.Replace(_lyricsEnumerator.Current.Value.Artist + " " + _lyricsEnumerator.Current.Value.Title + " " + _lyricsEnumerator.Current.Value.Source + " " + _lyricsEnumerator.Current.Value.Lyrics.Replace("\r\n", " "), @"[^a-zA-Z0-9 ]", "").ToLower();
                        for (int i = 0; i < _terms.Count; i++)
                        {
                            lyricFound = checkString.IndexOf(_terms[i]) > -1;
                            if (!lyricFound)
                            {
                                break;
                            }
                        }

                        if (lyricFound)
                        {
                            SearchResult _searchResult = new SearchResult();
                            _searchResult.Filename = string.Format(@"(Upper(strArtist) GLOB ""*{0}*"" AND ", _lyricsEnumerator.Current.Value.Artist.ToUpper()) + string.Format(@"Upper(strTitle) = ""{0}"")", _lyricsEnumerator.Current.Value.Title.ToUpper());//getSongFileByArtistAndTitle(_lyricsEnumerator.Current.Value.Artist, _lyricsEnumerator.Current.Value.Title);
                                
                            _searchResult.Item = _lyricsEnumerator.Current.Value.Artist + " - " + _lyricsEnumerator.Current.Value.Title;
                            _searchResult.Source = "Lyrics";
                            _lyricsList.Add(_searchResult);
                            return _searchResult;
                        }

                    }

                    
                    // We're done, now get the filenames of the lyrics found.
                    if (_lyricsList.Count > 0)
                    {
                        StringList sql = new StringList();
                        sql.Add("SELECT strArtist, strTitle, strPath FROM tracks WHERE (");

                        for (int i = 0; i < _lyricsList.Count; i++)
                        {
                            sql.Add(_lyricsList[i].Filename + (i < _lyricsList.Count - 1 ? " OR" : ""));
                        }

                        sql.Add(")");
                        SQLiteResultSet rs = MusicDatabase.DirectExecute(sql.Text);
                        for (int i = 0; i < _lyricsList.Count; i++)
                        {
                            bool found = false;
                            string checkValue = Regex.Replace(_lyricsList[i].Item, @"[^a-zA-Z0-9 ]", "").ToUpper();
                            while (checkValue.IndexOf("  ") > -1)
                            {
                                checkValue = checkValue.Replace("  ", " ");
                            }

                            for (int j = 0; j < rs.Rows.Count; j++)
                            {
                                string s = Regex.Replace(rs.Rows[j].fields[0].Trim() + " - " + rs.Rows[j].fields[1].Trim(), @"[^a-zA-Z0-9 ]", "").Trim();
                                while (s.IndexOf("  ") > -1)
                                {
                                    s = s.Replace("  ", " ");
                                }

                                if (s.ToUpper() == checkValue)
                                {
                                    _lyricsList[i].Filename = rs.Rows[j].fields[2];
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                _lyricsList[i].Filename = "No file found for this track!";
                            }
                        }
                    }
                }
                */
            }
            
            return null;
        }
    }
}