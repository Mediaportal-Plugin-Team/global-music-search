using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Localisation;

namespace MediaPortal.GUI.GlobalSearch
{
    public static class Translation
    {
        #region Private variables

        private static Dictionary<string, string> translations;
        private static Regex translateExpr = new Regex(@"\$\{([^\}]+)\}");
        private static string path = string.Empty;

        #endregion

        #region Constructor

        static Translation()
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the translated strings collection in the active language
        /// </summary>
        public static Dictionary<string, string> Strings
        {
            get
            {
                if (translations == null)
                {
                    translations = new Dictionary<string, string>();
                    Type transType = typeof(Translation);
                    FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo field in fields)
                    {
                        translations.Add(field.Name, field.GetValue(transType).ToString());
                    }
                }
                return translations;
            }
        }

        public static string CurrentLanguage
        {
            get
            {
                string language = string.Empty;
                try
                {
                    language = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
                }
                catch (Exception)
                {
                    language = CultureInfo.CurrentUICulture.Name;
                }
                return language;
            }
        }
        public static string PreviousLanguage { get; set; }

        #endregion

        #region Public Methods

        public static void Init()
        {
            translations = null;
            Log.Info("[GlobalSearch] Using language " + CurrentLanguage);

            path = Config.GetSubFolder(Config.Dir.Language, "GlobalSearch");

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            string lang = PreviousLanguage = CurrentLanguage;
            LoadTranslations(lang);

            // publish all available translation strings
            // so skins have access to them
            foreach (string name in Strings.Keys)
            {
                GUIPropertyManager.SetProperty("#GlobalSearch.Translation." + name + ".Label", Translation.Strings[name]);
                Log.Debug("[GlobalSearch] Translation Property Added: #GlobalSearch.Translation." + name + ".Label");
            }
        }

        public static int LoadTranslations(string lang)
        {
            XmlDocument doc = new XmlDocument();
            Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
            string langPath = string.Empty;
            try
            {
                langPath = Path.Combine(path, lang + ".xml");
                doc.Load(langPath);
            }
            catch (Exception e)
            {
                if (lang == "en")
                    return 0; // otherwise we are in an endless loop!

                if (e.GetType() == typeof(FileNotFoundException))
                    Log.Warn("[GlobalSearch] Cannot find translation file {0}. Falling back to English", langPath);
                else
                    Log.Error("[GlobalSearch] Error in translation xml file: {0}. Falling back to English", lang);

                return LoadTranslations("en");
            }
            foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
            {
                if (stringEntry.NodeType == XmlNodeType.Element)
                    try
                    {
                        TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("name").Value, stringEntry.InnerText.NormalizeTranslation());
                    }
                    catch (Exception ex)
                    {
                        Log.Error("[GlobalSearch] Error in Translation Engine", ex.Message);
                    }
            }

            Type TransType = typeof(Translation);
            FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fieldInfos)
            {
                if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
                    TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
                else
                    Log.Info("[GlobalSearch] Translation not found for : {0}. Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }

        public static string GetByName(string name)
        {
            if (!Strings.ContainsKey(name))
                return name;

            return Strings[name];
        }

        public static string GetByName(string name, params object[] args)
        {
            return String.Format(GetByName(name), args);
        }

        /// <summary>
        /// Takes an input string and replaces all ${named} variables with the proper translation if available
        /// </summary>
        /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
        /// <returns>translated input string</returns>
        public static string ParseString(string input)
        {
            MatchCollection matches = translateExpr.Matches(input);
            foreach (Match match in matches)
            {
                input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
            }
            return input;
        }

        /// <summary>
        /// Temp workaround to remove unwatched chars from Transifex
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NormalizeTranslation(this string input)
        {
            input = input.Replace("\\'", "'");
            input = input.Replace("\\\"", "\"");
            return input;
        }
        #endregion

        #region Translations / Strings

        /// <summary>
        /// These will be loaded with the language files content
        /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
        /// if that also fails it will use the hardcoded strings as a last resort.
        /// </summary>

        // A
        

        // B


        // C
        public static string ClearPlaylist = "Clear Playlist?";
        public static string CaseSensitive = "Case Sensitive";

        // D


        // E


        // F
        public static string FileNotFound = "File not found!";

        // G


        // H


        // I
        

        // I


        // J
        

        // L
       

        // M
        public static string MusicSearch = "Music Search";

        // N
        public static string NoSearchResultsFound = "No search results found! Try refining your search.";

        // O
        

        // P
        public static string PleaseWait = "Please wait...";
        public static string PluginDescription = "Find everything music related in your MP Database";

        // R
        

        // S
        public static string SearchFailed = "Search Failed!";
        public static string SelectSearchFields = "Select search fields";
        public static string SearchPhrase = "Search Phrase";
        public static string SearchHistory = "Search History";
        public static string SearchFields = "Search Fields";
        public static string SearchType = "Search Type";
        public static string SearchMusic = "Search Music";
        public static string SearchTypeEquals = "Equals";
        public static string SearchTypeStarts = "Starts With";
        public static string SearchTypeContains = "Contains";
        public static string SearchTypeEnds = "Ends With";

        // T
        

        // U
        

        // V
        

        // W
        

        // Y
        

        #endregion

    }

}