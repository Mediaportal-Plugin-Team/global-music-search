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
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Search;
using System.Windows.Forms;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using System.Collections;
using System.IO;
using GlobalSearch;
using MediaPortal.Configuration;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.GlobalSearch
{
    [PluginIcons("GlobalSearch.Resources.Images.search_music.png", "GlobalSearch.Resources.Images.search_music_disabled.png")]
    public class GUIGlobalMusicSearch : GUIWindow, ISetupForm, IShowPlugin
    {
        #region Constants
        public const int UID = 30885;
        public const string GlobalSearchPluginName = "Search music";
        #endregion

        #region Skin Controls
        [SkinControlAttribute(2)]
        protected GUIButtonControl btnSearchPhrase = null;

        [SkinControlAttribute(7)]
        protected GUIButtonControl btnLastSearches = null;

        [SkinControlAttribute(3)]
        protected GUICheckButton btnCaseSensitive = null;

        [SkinControlAttribute(6)]
        protected GUIButtonControl btnSearchFields = null;

        [SkinControlAttribute(8)]
        protected GUIButtonControl btnMyMusic = null;

        [SkinControlAttribute(9)]
        protected GUIButtonControl btnPlayingNow = null;

        [SkinControlAttribute(50)]
        protected GUIFacadeControl lstResults = null;

        [SkinControlAttribute(10)]
        protected GUIButtonControl btnSearchTypeMenu = null;
        #endregion

        #region Private Declarations
        private bool _enteredPhrase = false;
        private GlobalMusicSearch _search = new GlobalMusicSearch();
        private bool pageInitialized = false;
        private bool autofillSearchResults = false;
        private List<GUIListItem> lastSearchResults = new List<GUIListItem>();
        private string currentSearchString = string.Empty;
        private SearchGlobals.SearchType currentSearchType = SearchGlobals.SearchType.Contains;
        private int previousSelectedIndex = 0;
        private static bool _playlistIsCurrent = false;
        private static PlayListPlayer _playListPlayer;
        private static MusicDatabase _mdb;
        #endregion

        #region Overrides

        public override string GetModuleName()
        {
            return GlobalSearchSettings.Instance().PluginName;
        }

        protected override void OnPageLoad()
        {
            if (!pageInitialized)
            {
                _search.Type = SearchGlobals.SearchType.Contains;
                pageInitialized = true;
                previousSelectedIndex = 0;
                GUIUtils.SetProperty("#itemcount", "0");
            }
            else
            {
                _search.Type = currentSearchType;
                btnCaseSensitive.Selected = _search.Options == SearchGlobals.SearchOptions.CaseSensitive;
            }

            if (autofillSearchResults)
            {
                autofillSearchResults = false;
                if (lastSearchResults.Count > 0) 
                    fillSearchResults();
            }

            base.OnPageLoad();
        }

        public override bool Init()
        {
            LoadMPSettings();
            _playListPlayer = PlayListPlayer.SingletonPlayer;
            _mdb = MusicDatabase.Instance;
            Translation.Init();
            GlobalSearchSettings.Instance().LoadFromFile();
            return Load(GUIGraphicsContext.Skin + @"\GlobalSearch.Music.xml");
        }

        public override int GetID
        {
            get
            {
                return GUIGlobalMusicSearch.UID;
            }
        }

        public override void OnAction(Action action)
        {
            base.OnAction(action);
        }        

        protected override void OnShowContextMenu()
        {
            GUIListItem _item = lstResults.SelectedListItem;
            GUIDialogMenu _dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            _dlg.SetHeading(424);

            _dlg.AddLocalizedString(4552); // Play now
            if (g_Player.Playing && g_Player.IsMusic)
            {
              _dlg.AddLocalizedString(4551); // Play next
            }

            // only offer to queue items if
            // (a) playlist screen shows now playing list (_playlistIsCurrent is true) OR
            // (b) playlist screen is showing playlist (not what is playing) but music that is being played
            // is not from playlist (TEMP playlist is being used)
            if (_playlistIsCurrent || _playListPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
            {
              _dlg.AddLocalizedString(1225); // Queue item
              if (!_item.IsFolder)
              {
                _dlg.AddLocalizedString(1226); // Queue all items
              }
            }

            if (!_playlistIsCurrent)
            {
              _dlg.AddLocalizedString(926); // add to playlist
              _dlg.AddLocalizedString(4557); // Add all to playlist
            }
            
            _dlg.AddLocalizedString(33041); // Add album to playlist
            _dlg.AddLocalizedString(930); // Add to favorites
            _dlg.DoModal(GUIWindowManager.ActiveWindow);

            if (_dlg.SelectedLabel == -1)
                return;

            Song _song = new Song();
            _mdb.GetSongByFileName(_item.Path, ref _song);

            if (_song == null)
                return;

            switch (_dlg.SelectedId)
            {
                case 4552: // Play Now
                    {
                        PlayNow(_song);
                        break;
                    }

                case 4551: // Play Next
                    {
                        PlayNext(_song);
                        break;
                    }

                case 1225: // Queue Item
                    {
                      AddSongToPlaylist(_song, GetPlaylistType());
                      break;
                    }

                case 1226: // Queue All Items
                    {
                      AddAllToPlaylist(GetPlaylistType());
                      break;
                    }

                case 926:  // Add To Playlist
                    {
                        AddSongToPlaylist(_song, PlayListType.PLAYLIST_MUSIC);
                        break;
                    }

                case 4557: // Add All To Playlist
                    {
                        AddAllToPlaylist(PlayListType.PLAYLIST_MUSIC);
                        break;
                    }

                case 33041: // Add Album To Playlist
                    {
                        AddAlbumToPlayList(_song);
                        break;
                    }

                case 930: // Add Item To Favourites
                    {
                        AddSongToFavorites(_song);
                        break;
                    }
            }
        }        

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            if (control == btnSearchPhrase)
            {
                string refvalue = currentSearchString;                
                if (GUIUtils.GetStringFromKeyboard(ref refvalue))
                {
                    _search.SearchPhrase = refvalue;
                    GUIPropertyManager.SetProperty("#GlobalSearch.Music.SearchPhrase", refvalue);
                    currentSearchString = refvalue;
                    _enteredPhrase = true;
                    previousSelectedIndex = 0;
                    DoSearch();
                }
            }

            if (control == btnCaseSensitive)
            {
                _search.Options = (btnCaseSensitive.Selected ? SearchGlobals.SearchOptions.CaseSensitive : SearchGlobals.SearchOptions.CaseInsensitive);
            }

            if (control == btnMyMusic)
            {
                autofillSearchResults = true;
                OpenMyMusic();
                return;
            }

            if (control == btnPlayingNow)
            {
                autofillSearchResults = true;
                OpenPlayingNow();
                return;
            }

            if (control == btnCaseSensitive)
            {
                _search.Options = (btnCaseSensitive.Selected ? SearchGlobals.SearchOptions.CaseSensitive : SearchGlobals.SearchOptions.CaseInsensitive);
            }

            if (control == btnLastSearches)
            {
                GUIDialogMenu _dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                _dlg.SetHeading(424);
                foreach (LastSearchItem lsi in GlobalSearchSettings.Instance().LastSearchItems)
                {
                    _dlg.Add(lsi.Phrase);
                }
                _dlg.DoModal(GUIWindowManager.ActiveWindow);

                if (_dlg.SelectedLabel == -1)
                    return;

                LastSearchItem myLsi = GlobalSearchSettings.Instance().LastSearchItems[_dlg.SelectedLabel];
                _search.SearchPhrase = myLsi.Phrase;
                _search.Options = myLsi.SearchOptions;
                _search.Type = myLsi.SearchType;
                
                currentSearchString = _search.SearchPhrase;
                currentSearchType = _search.Type;
                
                GUIPropertyManager.SetProperty("#GlobalSearch.Music.SearchPhrase", _search.SearchPhrase);
                
                btnCaseSensitive.Selected = (_search.Options == SearchGlobals.SearchOptions.CaseSensitive);
                _enteredPhrase = true;
                previousSelectedIndex = 0;
                DoSearch();
            }

            if (control == btnSearchFields)
            {
                GUIDialogMultiSelect _dialog = (GUIDialogMultiSelect)GUIWindowManager.GetWindow(2100);
                _dialog.SetHeading(Translation.SelectSearchFields);
                _dialog.Reset();

                GUIListItem _item = null;

                for (int i = 0; i < GlobalMusicSearch.SearchFieldValues.Count; i++)
                {
                    _item = new GUIListItem(GlobalMusicSearch.SearchFieldTranslatedStrings[i]);
                    _item.MusicTag = GlobalMusicSearch.SearchFieldValues[i];
                    _item.Selected = _search.ContainsSearchField((GlobalMusicSearch.MusicSearchFields)_item.MusicTag);
                    _dialog.Add(_item);
                }

                _dialog.DoModal(GUIWindowManager.ActiveWindow);

                if (_dialog.DialogModalResult == ModalResult.OK)
                {

                    _search.EmptySearchFields();
                    for (int i = 0; i < _dialog.ListItems.Count; i++)
                    {
                        if (_dialog.ListItems[i].Selected)
                        {
                            _search.IncludeSearchFields((GlobalMusicSearch.MusicSearchFields)_dialog.ListItems[i].MusicTag);
                        }
                    }
                }
            }

            if (control == btnSearchTypeMenu)
            {
                GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dlg.Reset();
                dlg.SetHeading(Translation.SearchType);

                foreach (SearchGlobals.SearchType type in Enum.GetValues(typeof(SearchGlobals.SearchType)))
                {
                    string menuItem = GetTypeTranslation(type);
                    GUIListItem pItem = new GUIListItem(menuItem);
                    if (type == currentSearchType) pItem.Selected = true;
                    dlg.Add(pItem);
                }

                dlg.DoModal(GUIWindowManager.ActiveWindow);

                if (dlg.SelectedLabel >= 0)
                {
                    _search.Type = (SearchGlobals.SearchType)dlg.SelectedLabel;
                    currentSearchType = _search.Type;
                }
            }

            if (controlId == 50)
            {
                HandleListViewClick(control);
            }

            base.OnClicked(controlId, control, actionType);
        }
        #endregion

        #region Public Methods

        public static void ClearPlaylist()
        {
            PlayList playList = _playListPlayer.GetPlaylist(GetPlaylistType());
            playList.Clear();
        }

        public static void AddSongToPlaylist(Song song, bool autoPlayWhenNothingInList, PlayListType playListType )
        {
            PlayList playList = _playListPlayer.GetPlaylist(playListType);
            PlayListItem _pi = song.ToPlayListItem();
            playList.Add(_pi);
            if ((autoPlayWhenNothingInList) && (!g_Player.Playing))
            {
                _playListPlayer.CurrentPlaylistType = playListType;
                _playListPlayer.Play(0);
            }
        }
        public static void AddSongToPlaylist(Song song, PlayListType playListType)
        {
            AddSongToPlaylist(song, true, playListType);
        }

        public static void PlayNow(Song song)
        {
            g_Player.Stop();
            ClearPlaylist();
            AddSongToPlaylist(song, GetPlaylistType());
        }

        public static void PlayNext(Song song)
        {
            PlayList playList = _playListPlayer.GetPlaylist(GetPlaylistType());

            int index = Math.Max(_playListPlayer.CurrentSong, 0);

            if (playList.Count == 1)
            {
                AddSongToPlaylist(song, GetPlaylistType());
            }

            if (playList.Count > 1)
            {
                PlayListItem _pi = song.ToPlayListItem();
                playList.Insert(_pi, index);
            }
            else
            {
                _playListPlayer.CurrentPlaylistType = GetPlaylistType();
                AddSongToPlaylist(song, GetPlaylistType());
            }
        }

        public static void AddAlbumToPlayList(Song song)
        {
            PlayList playList = _playListPlayer.GetPlaylist(GetPlaylistType());
            if (GUIUtils.ShowYesNoDialog(GUILocalizeStrings.Get(136), Translation.ClearPlaylist, true))
            {
                ClearPlaylist();
                g_Player.Stop();
            }

            List<Song> songs = new List<Song>();
            MusicDatabase mdb = MusicDatabase.Instance;

            if (mdb.GetSongsByAlbumArtistAlbum(song.AlbumArtist, song.Album, ref songs))
            {
                foreach (Song s in songs)
                {
                    playList.Remove(s.FileName);
                    AddSongToPlaylist(s, false, GetPlaylistType());
                }

                if (!g_Player.Playing)
                {
                    _playListPlayer.CurrentPlaylistType = GetPlaylistType();
                    _playListPlayer.Play(0);
                }
            }
        }

        public static void AddSongToFavorites(Song song)
        {
            song.Favorite = true;
            _mdb.SetFavorite(song);
        }

        public static void OpenPlayingNow()
        {
            int iWindowId = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
            GUIWindowManager.ActivateWindow(iWindowId);
        }

        public static void OpenMyMusic()
        {
            int iWindowId = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;
            GUIWindowManager.ActivateWindow(iWindowId);
        }
        #endregion

        #region Private Methods
        private void DoSearch()
        {
            if (!_enteredPhrase)
            {
                GUIDialogOK _dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                _dialog.SetHeading(257);
                _dialog.SetLine(1, 2500);
                _dialog.DoModal(GUIWindowManager.ActiveWindow);
            }
            else
            {
                GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
                if (dlgProgress != null)
                {
                    dlgProgress.Reset();
                    dlgProgress.SetHeading(194);
                    dlgProgress.SetLine(1, Translation.PleaseWait);
                    dlgProgress.StartModal(GUIWindowManager.ActiveWindow);
                    dlgProgress.Progress();
                    dlgProgress.Process();
                }

                MediaPortal.Search.GlobalSearch mySearcher = new MediaPortal.Search.GlobalSearch();
                mySearcher.Methods.Add(_search);
                mySearcher.Search();
                
                GlobalSearchSettings.Instance().LastSearchItems.registerNewSearch(_search.SearchPhrase, _search.Type, _search.Options);

                int itemId = 0;
                lastSearchResults.Clear();
                foreach (SearchResult searchItem in mySearcher.Results)
                {
                    try
                    {
                        GUIListItem item = new GUIListItem();
                        item.Label = searchItem.Item;
                        item.Label2 = searchItem.Source;
                        item.Path = searchItem.Filename;
                        item.ItemId = itemId++;
                        item.IconImage = "defaultAudio.png";
                        item.IconImageBig = "defaultAudioBig.png";
                        item.ThumbnailImage = "defaultAudioBig.png";

                        Song song = new Song();
                        if (MusicDatabase.Instance.GetSongByFileName(searchItem.Filename, ref song))
                        {
                            string coverArt = GUIGlobalMusicSearchDetail.GetAlbumOrFolderThumb(song);
                            if (!string.IsNullOrEmpty(coverArt))
                            {
                                item.IconImage = coverArt;
                                item.IconImageBig = coverArt;
                                item.ThumbnailImage = coverArt;
                            }
                            item.MusicTag = song;
                        }
                        else
                            item.MusicTag = null;

                        lastSearchResults.Add(item);
                    }
                    catch (Exception e)
                    {
                        dlgProgress.SetLine(1, Translation.SearchFailed);
                        dlgProgress.SetLine(2, e.Message);
                        throw;
                    }
                }

                fillSearchResults();

                if (dlgProgress != null) dlgProgress.Close();
                
            }
        }

        private void AddAllToPlaylist(PlayListType playListType)
        {
            PlayList playList = _playListPlayer.GetPlaylist(playListType);
            Song firstSong = null;

            if (GUIUtils.ShowYesNoDialog(GUILocalizeStrings.Get(136), Translation.ClearPlaylist, true))
            {
                ClearPlaylist();
                g_Player.Stop();
            }

            foreach (GUIListItem li in lastSearchResults)
            {
                Song _song = new Song();
                _mdb.GetSongByFileName(li.Path, ref _song);

                PlayListItem _pi = _song.ToPlayListItem();
                playList.Add(_pi);

                if (firstSong == null)
                {
                    firstSong = _song;
                }
            }

            if ((firstSong != null) && (!g_Player.Playing))
            {
                _playListPlayer.CurrentPlaylistType = playListType;
                _playListPlayer.Play(0);
            }
        }

        protected void fillSearchResults()
        {
            GUIUtils.SetProperty("#selecteditem", string.Empty);
            GUIUtils.SetProperty("#selectedthumb", string.Empty);
            GUIUtils.SetProperty("#itemcount", lastSearchResults.Count.ToString());

            GUIControl.ClearControl(GetID, 50);

            if (lastSearchResults.Count == 0)
            {
                GUIUtils.ShowNotifyDialog(Translation.MusicSearch, Translation.NoSearchResultsFound);
                return;
            }

            foreach (GUIListItem searchItem in lastSearchResults)
            {
                GUIControl.AddListItemControl(GetID, 50, searchItem);
            }

            GUIControl.FocusControl(GetID, lstResults.GetID);
            lstResults.SelectedListItemIndex = previousSelectedIndex;

            GUIUtils.SetProperty("#selecteditem", lstResults.SelectedListItem.Label);
            GUIUtils.SetProperty("#selectedthumb", lstResults.SelectedListItem.IconImageBig);
        }

        private void HandleListViewClick(GUIControl control)
        {
            if (control != null)
            {
                previousSelectedIndex = lstResults.SelectedListItemIndex;

                GUIGlobalMusicSearchDetail searchDetail = (GUIGlobalMusicSearchDetail)GUIWindowManager.GetWindow(GUIGlobalMusicSearchDetail.UID);
                GUIListItem item = ((GUIFacadeControl)control).SelectedListItem;

                Song song = item.MusicTag as Song;

                if (song != null)
                {
                    autofillSearchResults = true;
                    GUIWindowManager.ActivateWindow(GUIGlobalMusicSearchDetail.UID);
                    searchDetail.SetSong(song);
                }
                else
                {
                    GUIDialogOK seldialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                    seldialog.SetHeading(1020);
                    seldialog.SetLine(1, Translation.FileNotFound);
                    seldialog.DoModal(GUIWindowManager.ActiveWindow);
                }
            }
        }

        private string GetTypeTranslation(SearchGlobals.SearchType type)
        {
            string strLine = string.Empty;
            switch (type)
            {
                case SearchGlobals.SearchType.Contains:
                    strLine = Translation.SearchTypeContains;
                    break;
                case SearchGlobals.SearchType.EndsWith:
                    strLine = Translation.SearchTypeEnds;
                    break;
                case SearchGlobals.SearchType.Equals:
                    strLine = Translation.SearchTypeEquals;
                    break;
                case SearchGlobals.SearchType.StartsWith:
                    strLine = Translation.SearchTypeStarts;
                    break;
            }
            return strLine;
        }

        private void LoadMPSettings()
        {
          using (Profile.Settings xmlreader = new Profile.MPSettings())
          {
            _playlistIsCurrent = xmlreader.GetValueAsBool("musicfiles", "playlistIsCurrent", true);
          }
        }

        public static PlayListType GetPlaylistType()
        {
          if (_playlistIsCurrent)
          {
            return PlayListType.PLAYLIST_MUSIC;
          }
          return PlayListType.PLAYLIST_MUSIC_TEMP;
        }

        #endregion

        #region ISetupForm Members
        public string Author()
        {
            return "d-fader, trevor, ltfearme";
        }

        public bool CanEnable()
        {
            return true;
        }

        public bool DefaultEnabled()
        {
            return true;
        }

        public string Description()
        {
            return Translation.PluginDescription;
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = GlobalSearchSettings.Instance().PluginName;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }

        public int GetWindowId()
        {
            return UID;
        }

        public bool HasSetup()
        {
            return true;
        }

        public string PluginName()
        {
            return GUIGlobalMusicSearch.GlobalSearchPluginName;
        }

        public void ShowPlugin()
        {
            FormGlobalSearchSettings form = new FormGlobalSearchSettings();
            form.ShowDialog();
        }
        #endregion

        #region IShowPlugin Members

        public bool ShowDefaultHome()
        {
            return true;
        }

        #endregion
    }
}
