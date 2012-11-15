using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Phone.BackgroundAudio;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Threading;
using ResourceLibrary;

namespace AudioPlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        /// 
        private static List<AudioTrack> currentPlaylist;
        private static int currentTrackNumber = 0;
        private int PlayStartNumber = 0;
        //private youtube_org_api_proxy yoa = new youtube_org_api_proxy();
        private youtube_org yoa = new youtube_org();
        //private vidtomp3 yoa = new vidtomp3();
        //private video2mp3_at yoa = new video2mp3_at();
        //ManualResetEvent m_rst = new ManualResetEvent(false);
        private static string playlistname ;

        public AudioPlayer()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += AudioPlayer_UnhandledException;
                });
            }
            //yoa.Completed +=new ApiCompletedEventHandler(yoa_Completed);
            //PopulatePlaylist();
        }

        

        /// Code to execute on Unhandled Exceptions
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                //System.Diagnostics.Debugger.Break();
                //BackgroundAudioPlayer.Instance.Close();
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        /// 
        /// Notable playstate events: 
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackEnded:
                    if (currentPlaylist.Count > 0)
                        player.Track = GetNextTrack();
                    break;
                case PlayState.TrackReady:
                    track.BeginEdit();
                    track.Title = currentPlaylist[currentTrackNumber].Title;
                    track.EndEdit();
                    player.Play();
                    break;
                case PlayState.Shutdown:
                    // TODO: Handle the shutdown state here (e.g. save state)
                    player.Close();
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    track.BeginEdit();
                        track.Title = "Buffering...";
                    track.EndEdit();
                    break;
                case PlayState.BufferingStopped:
                    track.BeginEdit();
                    track.Title = currentPlaylist[currentTrackNumber].Title;
                    track.EndEdit();
                    break;
                case PlayState.Rewinding:
                    track.BeginEdit();
                    track.Title = "Rewinding...";
                    track.EndEdit();
                    break;
                case PlayState.FastForwarding:
                    track.BeginEdit();
                    track.Title = "FastForwarding...";
                    track.EndEdit();
                    break;
            }

            NotifyComplete();
        }


        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.Play:
                    if (player.PlayerState == PlayState.Paused)
                        PopulatePlaylist(false);
                    else
                        PopulatePlaylist(true);
                    if (currentPlaylist != null && currentPlaylist.Count > 0)
                    {
                        player.Track = currentPlaylist[currentTrackNumber];
                    }
                    break;
                case UserAction.Stop:
                    currentPlaylist = null;
                    currentTrackNumber = 0;
                    player.Stop();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.FastForward:
                    player.FastForward();
                    break;
                case UserAction.Rewind:
                    player.Rewind();
                    break;
                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
                case UserAction.SkipNext:
                    if (currentPlaylist != null && currentPlaylist.Count > 0)
                        player.Track = GetNextTrack();
                    break;
                case UserAction.SkipPrevious:
                    if (currentPlaylist != null && currentPlaylist.Count > 0)
                        player.Track = GetPreviousTrack();
                    break;
            }

            NotifyComplete();
        }

        //void yoa_Completed(object sender, APICompletedEventArgs e)
        //{
        //    //m_rst.Set();
        //}

        private void PopulatePlaylist(bool playcommand)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                currentPlaylist = new List<AudioTrack>();
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("playlist.xml", System.IO.FileMode.Open, isf))
                {
                    XDocument xdoc = XDocument.Load(file);
                    playlistname = (string)xdoc.Element("playlist").Attribute("name");
                    PlayStartNumber = (int)xdoc.Element("playlist").Attribute("start");
                    var elements = xdoc.Elements("playlist");
                    foreach (XElement xe in elements.Elements())
                    {
                        string src = (string)xe.Attribute("source");
                        string id = (string)xe.Attribute("id");
                        AudioTrack nad = new AudioTrack(new Uri(src,UriKind.Relative), (string)xe.Attribute("title"), null, null, null, id, EnabledPlayerControls.All);
                        currentPlaylist.Add(nad);
                    }
                    file.Close();
                }
                if (PlayStartNumber == 1 && playcommand)
                     currentTrackNumber = 0;
            }
        }

        //private AudioTrack GetPlayTrack()
        //{
        //    PopulatePlaylist();
        //    string local_filename = playlistname +"/"+ currentPlaylist[currentTrackNumber].Tag+".mp3";
        //    using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        lock (iso)
        //        {
        //            if (iso.FileExists(local_filename))
        //                return new AudioTrack(new Uri(local_filename, UriKind.Relative), "File.." + currentPlaylist[currentTrackNumber].Title, null, null, null);
        //        }
                
        //    }
            
            //yoa.GetStreamURL(currentPlaylist[currentTrackNumber].Source.AbsoluteUri);
            //m_rst.Reset();
            //if (!m_rst.WaitOne())
            //    throw new TimeoutException();
            ////if (yoa.StreamURL != string.Empty)
            ////    return new AudioTrack(new Uri(yoa.StreamURL, UriKind.Absolute), "Streaming.." + currentPlaylist[currentTrackNumber].Title, null, null, null);
            //else
            //    throw new Exception("Unable to Play Track, API Returned an empty string");
        //}

        /// <summary>
        /// Implements the logic to get the next AudioTrack instance.
        /// In a playlist, the source can be from a file, a web request, etc.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if the playback is completed</returns>
        private AudioTrack GetNextTrack()
        {
            PopulatePlaylist(false);
            if (++currentTrackNumber >= currentPlaylist.Count)
            {
                // We've gone past the end, wrap 
                // to the beginning of the list
                currentTrackNumber = 0;
            }

            // Specify the track
            //this.SongRequest();
            return currentPlaylist[currentTrackNumber];
        }

       
        /// <summary>
        /// Implements the logic to get the previous AudioTrack instance.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if previous track is not allowed</returns>
        private AudioTrack GetPreviousTrack()
        {
            PopulatePlaylist(false);
            // TODO: add logic to get the previous audio track
            if (--currentTrackNumber < 0)
            {
                // We've gone past the beginning, 
                // wrap to the end of the list
                currentTrackNumber = currentPlaylist.Count - 1;
            }
            //this.SongRequest();
            // Specify the track
            return currentPlaylist[currentTrackNumber];
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            if (isFatal)
            {
                Abort();
            }
            else
            {
                NotifyComplete();
            }

        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel()
        {

        }
    }
}
