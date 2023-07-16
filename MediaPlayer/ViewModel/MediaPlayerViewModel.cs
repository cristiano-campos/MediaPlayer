namespace MediaPlayer.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;
using System.Windows;
using System.Windows.Media; 
using System.IO;
using System.Linq;
using MediaPlayer.Model;
using Microsoft.Win32;


public class MediaPlayerViewModel : ObservableObject
{
    private MediaPlayer mediaPlayer;
    private MediaState mediaState = MediaState.Stopped;
    private MediaFile currentMedia;
    public MediaFile CurrentMedia
    {
        get { return currentMedia; }
        set 
        { 
            SetProperty(ref currentMedia, value);
            NotifyCommandsCanExecuteChanged();
            
         
        }
    }

    private ObservableCollection<MediaFile> playlist;
    public ObservableCollection<MediaFile> Playlist
    {
        get { return playlist; }
        set { SetProperty(ref playlist, value); }
    }

    public IRelayCommand AddCommand {get; }
    public IRelayCommand RemoveCommand {get; }

    public IRelayCommand PlayCommand { get; }
    public IRelayCommand PauseCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IRelayCommand NextCommand { get; }
    public IRelayCommand PreviousCommand { get; }

    public MediaPlayerViewModel()
    {
        mediaPlayer = new MediaPlayer();
        mediaPlayer.MediaEnded += (s, e) => 
        {
            mediaState = MediaState.Stopped;
            NotifyCommandsCanExecuteChanged();
        };
        
        Playlist = new ObservableCollection<MediaFile>();

        AddCommand = new RelayCommand(AddMedia);
        RemoveCommand = new RelayCommand(RemoveMedia,CanExecuteRemove);
        PlayCommand = new RelayCommand(Play, CanExecutePlay);
        PauseCommand = new RelayCommand(Pause, CanExecutePause);
        StopCommand = new RelayCommand(Stop, CanExecuteStop);
        NextCommand = new RelayCommand(Next, CanExecuteNext);
        PreviousCommand = new RelayCommand(Previous, CanExecutePrevious);

        //LoadPlaylist(@"C:\Users\crist\OneDrive\Documentos\msft\INF-0996\mediaplayer\MediaPlayer\Media");
    }

    private void NotifyCommandsCanExecuteChanged()
    {
        ((RelayCommand)PlayCommand).NotifyCanExecuteChanged();
        ((RelayCommand)PauseCommand).NotifyCanExecuteChanged();
        ((RelayCommand)StopCommand).NotifyCanExecuteChanged();
        ((RelayCommand)NextCommand).NotifyCanExecuteChanged();
        ((RelayCommand)PreviousCommand).NotifyCanExecuteChanged();
        ((RelayCommand)RemoveCommand).NotifyCanExecuteChanged();
    }

    private void AddMedia()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "MP3 files (*.mp3)|*.mp3",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            foreach (var filePath in openFileDialog.FileNames)
            {
                var mediafile = new MediaFile { FilePath = filePath, Title = Path.GetFileName(filePath)};
                Playlist.Add(mediafile);
            }
        }
    }

    private void RemoveMedia()
    {
        Playlist.Remove(CurrentMedia);
        CurrentMedia = Playlist.FirstOrDefault();
    }

    private bool CanExecuteRemove()
    {
        return CurrentMedia != null;
    }

    private void Play()
    {
        // Implementação do comando Play
        mediaPlayer.Open( new Uri(CurrentMedia.FilePath) );
        mediaPlayer.Play();
        mediaState = MediaState.Playing; 
        NotifyCommandsCanExecuteChanged();
        
    }

    private bool CanExecutePlay()
    {
        return CurrentMedia != null && mediaState != MediaState.Playing; 
    }

    private void Pause()
    {
        // Implementação do comando Pause
        mediaPlayer.Pause();
        mediaState = MediaState.Paused;
        NotifyCommandsCanExecuteChanged();
    }

    private bool CanExecutePause()
    {
        return mediaState == MediaState.Playing;
    }

    private void Stop()
    {
        // Implementação do comando Stop
        mediaPlayer.Close();
        mediaState = MediaState.Stopped;
        NotifyCommandsCanExecuteChanged(); 
    }

    private bool CanExecuteStop()
    {
        return mediaState != MediaState.Stopped;
    }

    private void Next()
    {
        // Implementação do comando Next
        int currentIndex = Playlist.IndexOf(CurrentMedia);
        CurrentMedia = Playlist[currentIndex + 1];
        ((RelayCommand)PlayCommand).NotifyCanExecuteChanged(); 
        Play();
        NotifyCommandsCanExecuteChanged();
    }

    private bool CanExecuteNext()
    {
        //Pode executar o next se um mediafile é selecionado e n e o ultimo da playlist
        return CurrentMedia != null && Playlist.IndexOf(CurrentMedia) < Playlist.Count - 1;


    }

    private void Previous()
    {
        // Implementação do comando Previous
        int currentIndex = Playlist.IndexOf(CurrentMedia);
        CurrentMedia = Playlist[currentIndex - 1];
        ((RelayCommand)PlayCommand).NotifyCanExecuteChanged(); 
        Play(); 
        NotifyCommandsCanExecuteChanged();
    }

    private bool CanExecutePrevious()
    {
        return CurrentMedia != null && Playlist.IndexOf(CurrentMedia) > 0;
    }

    public void LoadPlaylist(string directoryPath)
    {
        var mediaFiles = Directory.GetFiles(directoryPath)
                                  .Select(filePath => new MediaFile { FilePath = filePath, Title = Path.GetFileName(filePath)})
                                  .ToList();
        Playlist = new ObservableCollection<MediaFile>(mediaFiles);

        CurrentMedia = Playlist.FirstOrDefault();

    }
}
