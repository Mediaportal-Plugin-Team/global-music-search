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
using MediaPortal.Search;
using System.IO;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.GUI.Music;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.GlobalSearch
{
  public class GUIGlobalMusicSearchDetail : GUIWindow
  {
    #region Constants
    public const int UID = 30886;
    #endregion

    #region Skin Controls
    [SkinControlAttribute(11)]
    protected GUIButtonControl btnPlayNow = null;
    [SkinControlAttribute(12)]
    protected GUIButtonControl btnPlayNext = null;
    [SkinControlAttribute(13)]
    protected GUIButtonControl btnAddToPlayList = null;
    [SkinControlAttribute(14)]
    protected GUIButtonControl btnAddAlbumToPlaylist = null;
    [SkinControlAttribute(15)]
    protected GUIButtonControl btnAddToFavorites = null;
    #endregion

    #region Private Declarations
    private Song currentSong = null;
    #endregion

    #region Overrides

    public override string GetModuleName()
    {
        return GlobalSearchSettings.Instance().PluginName;
    }    

    public override int GetID
    {
      get
      {
        return UID;
      }
      set
      {
        base.GetID = value;
      }
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\GlobalSearch.Music.Details.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnPlayNow)
      {
        GUIGlobalMusicSearch.PlayNow(currentSong);
        GUIWindowManager.ShowPreviousWindow();
      } else
      if (control == btnPlayNext)
      {
        GUIGlobalMusicSearch.PlayNext(currentSong);
        GUIWindowManager.ShowPreviousWindow();
      }
      else
      if (control == btnAddToPlayList)
      {
        GUIGlobalMusicSearch.AddSongToPlaylist(currentSong, GUIGlobalMusicSearch.GetPlaylistType());
        GUIWindowManager.ShowPreviousWindow();
      }
      else
      if (control == btnAddAlbumToPlaylist)
      {
        GUIGlobalMusicSearch.AddAlbumToPlayList(currentSong);
        GUIWindowManager.ShowPreviousWindow();
      }
      else
      if (control == btnAddToFavorites)
      {
        GUIGlobalMusicSearch.AddSongToFavorites(currentSong);
        GUIWindowManager.ShowPreviousWindow();
      }

      base.OnClicked(controlId, control, actionType);
    }
    #endregion

    #region Public Methods
    public void SetSong(Song song)
    {
        // try get song info
        this.currentSong = song;
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Artist", song.Artist, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Album", song.Album, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Title", song.Title, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Genre", song.Genre, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Year", song.Year.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.FileName", song.FileName, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.FileType", song.FileType, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.DateTimePlayed", (((song.TimesPlayed == 0) || (song.DateTimePlayed.Year < 1900)) ? GUILocalizeStrings.Get(512) : song.DateTimePlayed.ToString()), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Duration", Util.Utils.SecondsToHMSString(song.Duration), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.TimesPlayed", song.TimesPlayed.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Rating", (Convert.ToDecimal(2 * song.Rating + 1)).ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumArtist", song.AlbumArtist, true);        
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.BitRate", song.BitRate <= 0 ? string.Empty : song.BitRate.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.BitRateMode", song.BitRateMode, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.BPM", song.BPM.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Channels", song.Channels <= 0 ? string.Empty : song.Channels.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Codec", song.Codec, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Comment", song.Comment, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Composer", song.Composer, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Conductor", song.Conductor, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.DateAdded", (song.DateTimeModified.Year < 1900) ? GUILocalizeStrings.Get(512) : song.DateTimeModified.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.DiscId", song.DiscId <= 0 ? string.Empty : song.DiscId.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.DiscTotal", song.DiscTotal <= 0 ? string.Empty : song.DiscTotal.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.IsFavorite", song.Favorite.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Lyrics", song.Lyrics, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.SampleRate", song.SampleRate <= 0 ? string.Empty : song.SampleRate.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.SampleRateFormatted", song.SampleRate <= 0 ? string.Empty : string.Format("{0}kHz", song.SampleRate / 1000.0), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.Track", song.Track <= 0 ? string.Empty : song.Track.ToString(), true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.TrackTotal", song.TrackTotal <= 0 ? string.Empty : song.TrackTotal.ToString(), true);

        // try to get album info
        AlbumInfo albumInfo = new AlbumInfo();
        bool hasAlbumInfo = MusicDatabase.Instance.GetAlbumInfo(song.Album, song.AlbumArtist, ref albumInfo);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Album", hasAlbumInfo ? albumInfo.Album : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.AlbumArtist", hasAlbumInfo ? albumInfo.AlbumArtist : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Artist", hasAlbumInfo ? albumInfo.Artist : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Review", hasAlbumInfo ? albumInfo.Review : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Rating", hasAlbumInfo ? albumInfo.Rating.ToString() : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Genre", hasAlbumInfo ? albumInfo.Genre : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Styles", hasAlbumInfo ? albumInfo.Styles : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Tones", hasAlbumInfo ? albumInfo.Tones : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Tracks", hasAlbumInfo ? albumInfo.Tracks : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.AlbumInfo.Year", hasAlbumInfo ? albumInfo.Year.ToString() : string.Empty, true);
       
        // try to get artist info
        ArtistInfo artistInfo = new ArtistInfo();
        string artist = string.IsNullOrEmpty(song.Artist) || song.Artist.Contains("|") ? song.AlbumArtist : song.Artist;
        bool hasArtistInfo = MusicDatabase.Instance.GetArtistInfo(artist, ref artistInfo);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Albums", hasArtistInfo ? artistInfo.Albums : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Artist", hasArtistInfo ? artistInfo.Artist : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Bio", hasArtistInfo ? artistInfo.AMGBio : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Born", hasArtistInfo ? artistInfo.Born : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Compilations", hasArtistInfo ? artistInfo.Compilations : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Genres", hasArtistInfo ? artistInfo.Genres : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Instruments", hasArtistInfo ? artistInfo.Instruments : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Misc", hasArtistInfo ? artistInfo.Misc : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Singles", hasArtistInfo ? artistInfo.Singles : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Styles", hasArtistInfo ? artistInfo.Styles : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.Tones", hasArtistInfo ? artistInfo.Tones : string.Empty, true);
        GUIUtils.SetProperty("#GlobalSearch.Music.Details.ArtistInfo.YearsActive", hasArtistInfo ? artistInfo.YearsActive : string.Empty, true);

        string thumbnail = GetAlbumOrFolderThumb(song);
        if (!string.IsNullOrEmpty(thumbnail))
        {
            GUIUtils.SetProperty("#GlobalSearch.Music.Details.CoverArt", thumbnail);
        }
        else
        {
            GUIUtils.SetProperty("#GlobalSearch.Music.Details.CoverArt", GUIGraphicsContext.Skin + @"\media\missing_coverart.png");
        }

    }
    
    public static string GetAlbumOrFolderThumb(Song song)
    {
        string smallCoverArt = string.Empty;
        string largeCoverArt = string.Empty;

        // get from album thumbs directory
        smallCoverArt = Util.Utils.GetAlbumThumbName(song.Artist, song.Album);

        if (!string.IsNullOrEmpty(smallCoverArt))
        {
            if (Util.Utils.FileExistsInCache(smallCoverArt))
            {
                largeCoverArt = Util.Utils.ConvertToLargeCoverArt(smallCoverArt);
                if (Util.Utils.FileExistsInCache(largeCoverArt))
                {
                    return largeCoverArt;
                }
                else
                {
                    return smallCoverArt;
                }
            }
        }

        // no album artwork found, look for folder thumb
        smallCoverArt = Util.Utils.GetLocalFolderThumb(song.FileName);

        if (Util.Utils.FileExistsInCache(smallCoverArt))
        {
            largeCoverArt = Util.Utils.ConvertToLargeCoverArt(smallCoverArt);
            if (Util.Utils.FileExistsInCache(largeCoverArt))
            {
                return largeCoverArt;
            }
            else
            {
                return smallCoverArt;
            }
        }

        // last chance, try get folder.* from path
        return Util.Utils.GetCoverArt(Path.GetDirectoryName(song.FileName), "folder");
    }

    #endregion
  }
}