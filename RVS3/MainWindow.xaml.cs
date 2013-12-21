using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;
using AvalonDock;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Diagnostics;
using SevenZip;
using System.ComponentModel;
using ExtendedCollections;
using RobControls;
using System.Data.SQLite;
using System.Data;
using System.Runtime.InteropServices;

namespace RVS3
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_CONTINUOUS = 0x80000000
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(EXECUTION_STATE esFlags);
        private void KeepAlive(Object sender, EventArgs e) 
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        private SQLiteConnection ObjConnection;
        private SQLiteDataAdapter MoviesDataAdapter;
        private SQLiteDataAdapter BookmarksDataAdapter;
        private DataSet dataSet;
        private  DoubleAnimation animFadeOut = new DoubleAnimation();
        private DoubleAnimation animFadeIn = new DoubleAnimation();

        # region Length dependency definition
        public static readonly DependencyProperty lengthProperty = DependencyProperty.Register("length", typeof(long), typeof(MainWindow), new FrameworkPropertyMetadata(default(long)));
        public long length
        {
            get
            {
                return (long)GetValue(lengthProperty);
            }
            set
            {
                SetValue(lengthProperty, value);
            }
        }
        # endregion
        # region Position dependency definition
        public static readonly DependencyProperty positionProperty = DependencyProperty.Register("Position", typeof(long), typeof(MainWindow), new FrameworkPropertyMetadata(default(long)));
        public long Position
        {
            get
            {
                return (long)GetValue(positionProperty);
            }
            set
            {
                SetValue(positionProperty, value);
            }
        }
        # endregion
        # region Playing dependency definition
        public static readonly DependencyProperty playingProperty = DependencyProperty.Register("Playing", typeof(bool), typeof(MainWindow), new FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(onPlayingChanged)));
        public bool Playing
        {
            get
            {
                return (bool)GetValue(playingProperty);
            }
            set
            {
                SetValue(playingProperty, value);
            }
        }

        private static void onPlayingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && ((MainWindow)sender).VideoControl.Media != null)
            {
                if (((MainWindow)sender).VideoControl.Media != null)
                {
                    string media_path = ((MainWindow)sender).VideoControl.Media.MRL;
                    UriBuilder uri = new UriBuilder(media_path);
                    string Fullpath = Uri.UnescapeDataString(uri.Path);
                    string path = System.IO.Path.GetFileNameWithoutExtension(Fullpath);
                    string ExistingFilter = "movieFilename = '" + path + "'";
                    DataRow[] ExistMovies = ((MainWindow)sender).dataSet.Tables["Movies"].Select(ExistingFilter);
                    bool fileAlreadyINRecent = false;
                    if (ExistMovies.Length > 0)
                    {
                        fileAlreadyINRecent = true;
                    }


                    if (!fileAlreadyINRecent)
                    {
                        if (((MainWindow)sender).dataSet.Tables["Movies"].Rows.Count > 4)
                        {
                            ((MainWindow)sender).dataSet.Tables["Movies"].Rows[0].Delete();
                            ((MainWindow)sender).RecentFiles.RemoveAt(0);
                            ((MainWindow)sender).CommitDBChanges(((MainWindow)sender).MoviesDataAdapter, ((MainWindow)sender).dataSet.Tables["Movies"], DBAction.Delete);

                        }
                        DataRow newMovie = ((MainWindow)sender).dataSet.Tables["Movies"].NewRow();
                        newMovie.SetField<string>("movieFilename", path);
                        newMovie.SetField<DateTime>("movieDate", DateTime.Now);
                        newMovie.SetField<string>("movieFullFilename", ((MainWindow)sender).VideoControl.Media.MRL);
                        ((MainWindow)sender).dataSet.Tables["Movies"].Rows.Add(newMovie);
                        ((MainWindow)sender).CommitDBChanges(((MainWindow)sender).MoviesDataAdapter, ((MainWindow)sender).dataSet.Tables["Movies"], DBAction.Insert);
                    }
                    ((MainWindow)sender).Bookmarks.Clear();
                    string HasBookmarksFilter = "movieFilename = '" + path + "'";
                    DataRow[] Bookmarked = ((MainWindow)sender).dataSet.Tables["Bookmarks"].Select(HasBookmarksFilter);
                    foreach (var book in Bookmarked)
                    {
                        GenericBookmark gb = new GenericBookmark(book.Field<double>("bookmarkTime"));
                        ((MainWindow)sender).Bookmarks.Add(gb);
                    }

                }
                ((MainWindow)sender).VideoControl.Play();
                ((MainWindow)sender).ppbutton.IsChecked = true;
            }
            else
            {
                ((MainWindow)sender).VideoControl.Pause();
                ((MainWindow)sender).ppbutton.IsChecked = false;
            }
        }

        # endregion
        # region RecentFiles dependency definition
        public static readonly DependencyProperty recentFilesProperty = DependencyProperty.Register("RecentFiles", typeof(SortableObservableCollection<RECENTFILE>), typeof(MainWindow), new FrameworkPropertyMetadata(default(SortableObservableCollection<RECENTFILE>)));
        public SortableObservableCollection<RECENTFILE> RecentFiles
        {
            get
            {
                return (SortableObservableCollection<RECENTFILE>)GetValue(recentFilesProperty);
            }
            set
            {
                SetValue(recentFilesProperty, value);
            }
        }
        # endregion
        # region bookmarks dependency definition
        public static readonly DependencyProperty bookmarksProperty = DependencyProperty.Register("Bookmarks", typeof(SortableObservableCollection<GenericBookmark>), typeof(MainWindow), new FrameworkPropertyMetadata(default(SortableObservableCollection<GenericBookmark>)));
        public SortableObservableCollection<GenericBookmark> Bookmarks
        {
            get
            {
                return (SortableObservableCollection<GenericBookmark>)GetValue(bookmarksProperty);
            }
            set
            {
                SetValue(bookmarksProperty, value);
            }
        }
        # endregion


        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        public bool fullscreen = false;
        private Stopwatch stopWatch = new Stopwatch();
        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer alarmClock = new DispatcherTimer();

        public MainWindow()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string appPath = System.IO.Path.GetDirectoryName(path);
            //Set libvlc.dll and libvlccore.dll directory path
            VlcContext.LibVlcDllsPath = appPath;
            //Set the vlc plugins directory path
            VlcContext.LibVlcPluginsPath = appPath + @"\plugins";

            //Set the startup options
            VlcContext.StartupOptions.IgnoreConfig = true;
            VlcContext.StartupOptions.LogOptions.LogInFile = false;
            VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = false;
            VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.None;

            //Initialize the VlcContext
            VlcContext.Initialize();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            alarmClock.Interval = TimeSpan.FromMinutes(5);
            alarmClock.Tick += new EventHandler(KeepAlive);

            RecentFiles = new SortableObservableCollection<RECENTFILE>();
            Bookmarks = new SortableObservableCollection<GenericBookmark>();

            InitializeComponent();
            InitializeUI();
            InitializeDB();
            VolumeUC.Volume = VideoControl.AudioProperties.Volume;
            String[] args = App.mArgs;
            if (args!=null)
            {
                OpenUndefinedFile(args[0]);
            }
        }
        private void InitializeUI()
        {
            alarmClock.Start();
            animFadeOut.To = 0;
            animFadeOut.Duration = new Duration(TimeSpan.FromSeconds(2));
            animFadeIn.From = toolBar.Opacity;
            animFadeIn.To = 1;
            animFadeIn.Duration = new Duration(TimeSpan.FromSeconds(1));
        }
        private void InitializeDB()
        {
            dataSet = new DataSet("Movies");
            string DBPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ObjConnection = new SQLiteConnection(@"Data Source=" + DBPath + @"\Medias.rdf;");
            string queryMovies = @"select * from MOVIES";
            string deleteMovies = @"delete from MOVIES";
            SQLiteCommand MVCommand = new SQLiteCommand(queryMovies, ObjConnection);
            SQLiteCommand DMCommand = new SQLiteCommand(deleteMovies, ObjConnection);
            MVCommand.CommandType = CommandType.Text;
            MoviesDataAdapter = new SQLiteDataAdapter(MVCommand);
            MoviesDataAdapter.DeleteCommand = DMCommand;
            MoviesDataAdapter.TableMappings.Add("Table", "Movies");
            MoviesDataAdapter.Fill(dataSet);
            string queryBookmarks = @"select * from BOOKMARKS";
            string deleteBookmarks = @"delete from BOOKMARKS";
            SQLiteCommand BKCommand = new SQLiteCommand(queryBookmarks, ObjConnection);
            SQLiteCommand DBCommand = new SQLiteCommand(deleteBookmarks, ObjConnection);
            BKCommand.CommandType = CommandType.Text;
            BookmarksDataAdapter = new SQLiteDataAdapter(BKCommand);
            BookmarksDataAdapter.DeleteCommand = DBCommand;
            BookmarksDataAdapter.TableMappings.Add("Table", "Bookmarks");
            BookmarksDataAdapter.Fill(dataSet);
            SQLiteCommandBuilder SQLCBM = new SQLiteCommandBuilder(MoviesDataAdapter);
            SQLiteCommandBuilder SQLCBB = new SQLiteCommandBuilder(BookmarksDataAdapter);
            SuppressOldDBEntries();
            foreach (var row in dataSet.Tables["Movies"].AsEnumerable())
            {
                RECENTFILE rf = new RECENTFILE(row.Field<string>("movieFilename"), row.Field<string>("movieFullFilename"));
                RecentFiles.Add(rf);
            }
            dataSet.Tables["Movies"].RowChanged += new DataRowChangeEventHandler(Movies_RowChanged);
            dataSet.Tables["Movies"].RowDeleting += new DataRowChangeEventHandler(Movies_RowDeleting);
            dataSet.Tables["Movies"].RowDeleted += new DataRowChangeEventHandler(MainWindow_RowDeleted);
            dataSet.Tables["Bookmarks"].RowChanged+=new DataRowChangeEventHandler(Bookmarks_RowChanged);
            dataSet.Tables["Bookmarks"].RowDeleting += new DataRowChangeEventHandler(Bookmarks_RowDeleting);
            dataSet.Tables["Bookmarks"].RowDeleted += new DataRowChangeEventHandler(Bookmarks_RowDeleted);
            CC.Collection = RecentFiles;
            
        }

        private void SuppressOldDBEntries()
        {
            string oldMoviesFilter = "movieDate < '" + DateTime.Now.AddDays(-30) + "'";
            DataRow[] oldMovies = dataSet.Tables["Movies"].Select(oldMoviesFilter);
            if (oldMovies.Length > 0)
            {
                foreach (var oldMovie in oldMovies)
                {
                    dataSet.Tables["Movies"].Rows.Remove(oldMovie);
                }
                CommitDBChanges(MoviesDataAdapter, dataSet.Tables["Movies"], DBAction.Delete);
            }
            string oldBookmarksFilter = "bookmarkDate < '" + DateTime.Now.AddDays(-30) + "'";
            DataRow[] oldBookmarks = dataSet.Tables["Bookmarks"].Select(oldBookmarksFilter);
            if (oldBookmarks.Length > 0)
            {
                foreach (var oldBookmark in oldBookmarks)
                {
                    dataSet.Tables["Bookmarks"].Rows.Remove(oldBookmark);
                }
                CommitDBChanges(BookmarksDataAdapter, dataSet.Tables["Bookmarks"], DBAction.Delete);
            }
        }
        /// <summary>
        /// Save changes
        /// </summary>
        public Boolean CommitDBChanges(SQLiteDataAdapter _DataAdapter, DataTable _DataTable, DBAction _DBaction)
        {
            try
            {
                int numRowsUpdated = 0;
                switch (_DBaction)
                {
                    case DBAction.Update:
                        numRowsUpdated = _DataAdapter.Update(_DataTable);
                        if (numRowsUpdated > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case DBAction.Insert:
                        numRowsUpdated = _DataAdapter.Update(_DataTable);
                        if (numRowsUpdated > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case DBAction.Delete:
                        string del = _DataAdapter.DeleteCommand.CommandText;
                        numRowsUpdated = _DataAdapter.Update(_DataTable);
                        if (numRowsUpdated > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                }
                return false;
            }
            catch (Exception ex)
            {


                return false;
            }

        }

        private void onWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            alarmClock.Stop();
            VlcContext.CloseAll();
        }

        public void OpenUndefinedFile(string fullpath)
        {
            if (System.IO.Path.GetExtension(fullpath) == ".rar" || System.IO.Path.GetExtension(fullpath) == ".zip")
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += uncompress;
                worker.RunWorkerCompleted += OpenCompressedVideo;
                worker.RunWorkerAsync(fullpath);
            }
            else
            {
                PlayVideo(fullpath);
            }
        }

        private void uncompress(Object sender, DoWorkEventArgs e)
        {
            try
            {
                SevenZipExtractor pumper = new SevenZipExtractor((string)e.Argument);
                List<string> listcomp = pumper.ArchiveFileNames.ToList();
                var comps = from comp in listcomp where (System.IO.Path.GetExtension(comp) == ".avi" || System.IO.Path.GetExtension(comp) == ".mkv") select comp;
                var thecomp = comps.FirstOrDefault();
                System.IO.FileStream fs = System.IO.File.Create(@"temp" + System.IO.Path.GetExtension(thecomp));
                pumper.ExtractFile(thecomp, fs);
                fs.Close();
                e.Result = @"temp" + System.IO.Path.GetExtension(thecomp);
            }
            catch
            {
                // Configure the message box to be displayed
                string messageBoxText = "No video could be extracted";
                string caption = "Video error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                // Display message box
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
        private void OpenCompressedVideo(Object sender, RunWorkerCompletedEventArgs e)
        {
            PlayVideo((string)e.Result);
        }

        private void PlayVideo(string video_file)
        {
            VideoControl.Media = new Vlc.DotNet.Core.Medias.PathMedia(video_file);
            Playing = true;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (fullscreen)
            {
                main_menu.Visibility = Visibility.Visible;
                animFadeIn.From = toolBar.Opacity;
                toolBar.BeginAnimation(Window.OpacityProperty, animFadeIn);
                MW.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                MW.WindowState = System.Windows.WindowState.Normal;
                
            }
            else
            {
                this.Cursor = Cursors.None;
                toolBar.MouseLeave += new MouseEventHandler(fw_MouseLeave);
                animFadeOut.From = toolBar.Opacity;
                toolBar.BeginAnimation(Window.OpacityProperty, animFadeOut);
                main_menu.Visibility = Visibility.Collapsed;
                MW.WindowStyle = System.Windows.WindowStyle.None;
                MW.WindowState = System.Windows.WindowState.Maximized;
                
            }
            fullscreen = !fullscreen;
        }

        private void fw_MouseEnter(object sender, MouseEventArgs e)
        {
                animFadeIn.From = toolBar.Opacity;
                toolBar.BeginAnimation(Window.OpacityProperty, animFadeIn);
        }
        private void fw_MouseLeave(object sender, MouseEventArgs e)
        {
            if (fullscreen)
            {
                animFadeOut.From = toolBar.Opacity;
                toolBar.BeginAnimation(Window.OpacityProperty, animFadeOut);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            stopWatch.Stop();
            if (stopWatch.ElapsedMilliseconds > 500)
            {
                this.Cursor = Cursors.None;
                timer.Stop();
            }
            else
            {
                stopWatch.Start();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if ((!stopWatch.IsRunning) && fullscreen)
            {
                stopWatch.Start();
                timer.Start();
                this.Cursor = Cursors.Arrow;
            }
            else if (fullscreen)
            {
                stopWatch.Reset();
            }
            else if (!fullscreen)
            {
                this.Cursor = Cursors.Arrow;
            }
        }
        private void VolumeUC_OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            VideoControl.AudioProperties.Volume = (int)e.NewValue;
        }

        private void VolumeUC_IsMuteChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            VideoControl.AudioProperties.IsMute = (bool)e.NewValue;
        }

        private void VideoControl_LengthChanged(VlcControl sender, VlcEventArgs<long> e)
        {
            time_slider.Dispatcher.BeginInvoke(new Action(
            delegate()
            {
                time_slider.GetBindingExpression(RobControls.TimeSlider.lengthProperty).UpdateTarget();
            }),
            DispatcherPriority.ApplicationIdle,
            null);
            TimeDisp.Dispatcher.BeginInvoke(new Action(
            delegate()
            {
                TimeDisp.GetBindingExpression(RobControls.TimeDisplay.lengthProperty).UpdateTarget();
            }),
            DispatcherPriority.ApplicationIdle,
            null);
        }

        private void VideoControl_TimeChanged(VlcControl sender, VlcEventArgs<TimeSpan> e)
        {
            time_slider.Dispatcher.BeginInvoke(new Action(
            delegate()
            {
                time_slider.GetBindingExpression(RobControls.TimeSlider.currentTimeProperty).UpdateTarget();
            }),
            DispatcherPriority.ApplicationIdle,
            null);
            TimeDisp.Dispatcher.BeginInvoke(new Action(
            delegate()
            {
                TimeDisp.GetBindingExpression(RobControls.TimeDisplay.currentTimeProperty).UpdateTarget();
            }),
            DispatcherPriority.ApplicationIdle,
            null);
        }

        private void toolBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (toolBar.Opacity == 1)
            {
                animFadeOut.From = toolBar.Opacity;
                toolBar.BeginAnimation(Window.OpacityProperty, animFadeOut);
            }
            else
            {
                animFadeIn.From = toolBar.Opacity;
                toolBar.BeginAnimation(Window.OpacityProperty, animFadeIn);
            }
        }

        private void MW_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                Playing = !Playing;
                
                e.Handled = true;
            }
        }

        private void ppbutton_Click(object sender, RoutedEventArgs e)
        {
            Playing = !Playing;
            e.Handled = true;
        }

        private void spbutton_Click(object sender, RoutedEventArgs e)
        {
            Playing = false;
            VideoControl.Stop();
            VideoControl.Time = TimeSpan.FromMinutes(0);
            time_slider.Dispatcher.BeginInvoke(new Action(
            delegate()
            {
                time_slider.GetBindingExpression(RobControls.TimeSlider.currentTimeProperty).UpdateTarget();
            }),
            DispatcherPriority.ApplicationIdle,
            null);
            e.Handled = true;
        }

        private void bkbutton_Click(object sender, RoutedEventArgs e)
        {
            AddBookmark();
        }
        private void AddBookmark()
        {
            DataRow newBookmark = dataSet.Tables["Bookmarks"].NewRow();
            string media_path = VideoControl.Media.MRL;
            UriBuilder uri = new UriBuilder(media_path);
            string path = Uri.UnescapeDataString(uri.Path);
            path = System.IO.Path.GetFileNameWithoutExtension(path);
            newBookmark.SetField<string>("movieFilename", path);
            newBookmark.SetField<double>("bookmarkTime", VideoControl.Duration.TotalMinutes*VideoControl.Position);
            newBookmark.SetField<DateTime>("bookmarkDate", DateTime.Now);
            dataSet.Tables["Bookmarks"].Rows.Add(newBookmark);
            CommitDBChanges(BookmarksDataAdapter, dataSet.Tables["Bookmarks"], DBAction.Insert);
        }

        public void Movies_RowChanged(Object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
            {
                RECENTFILE rf = new RECENTFILE(e.Row.Field<string>("movieFilename"), e.Row.Field<string>("movieFullFilename"));
                RecentFiles.Add(rf);
            }
        }
        public void Movies_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Delete)
            {
                RecentFiles.Remove(RecentFiles.Where(x => x.movieFilename == e.Row.Field<string>("movieFilename")).FirstOrDefault());
            }
        }
        public void MainWindow_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            CommitDBChanges(MoviesDataAdapter, dataSet.Tables["Movies"], DBAction.Delete);
        }
        public void Bookmarks_RowChanged(Object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
            {
                GenericBookmark gb = new GenericBookmark(e.Row.Field<double>("bookmarkTime"));
                Bookmarks.Add(gb);
            }
        }
        public void Bookmarks_RowDeleting(Object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Delete)
            {
                Bookmarks.Remove(Bookmarks.Where(x => x.bookmarkTime == e.Row.Field<double>("bookmarkTime")).FirstOrDefault());
            }
        }
        public void Bookmarks_RowDeleted(Object sender, DataRowChangeEventArgs e)
        {
            CommitDBChanges(BookmarksDataAdapter, dataSet.Tables["Bookmarks"], DBAction.Delete);
        }

        private void MenuItemExt_Click(object sender, RoutedEventArgs e)
        {

            MenuItem control = e.OriginalSource as MenuItem;
            RECENTFILE rf = control.Header as RECENTFILE;
            if (VideoControl.IsPlaying)
            {
                VideoControl.Stop();
                Playing = false;
            }
            if (rf != null)
            {
                string FullPath = rf.movieFullFilename;
                OpenUndefinedFile(FullPath);
            }
            else
            {
                dlg.FileName = "Video"; // Default file name
                dlg.DefaultExt = ".avi"; // Default file extension
                dlg.Filter = "All files (.*)|*.*"; // Filter files by extension

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    OpenUndefinedFile(dlg.FileName);
                }
            }
        }
        private void BookmarkOnClick(object sender, RoutedEventArgs args)
        {
            Button Control = (Button)args.OriginalSource;
            ContentPresenter LVX = DependencyObjExtensions.TryFindParent<ContentPresenter>(Control);
            GenericBookmark gb = (GenericBookmark)LVX.Content;
            VideoControl.Time = TimeSpan.FromMinutes(gb.bookmarkTime);
        }

        private void time_slider_OnDeleteBookmark(object sender, RoutedPropertyChangedEventArgs<GenericBookmark> e)
        {
            string findBK = "bookmarkTime - " + ((GenericBookmark)e.NewValue).bookmarkTime + " < 0.00001 AND bookmarkTime - " + ((GenericBookmark)e.NewValue).bookmarkTime + " > -0.00001";
            DataRow[] ToDelBK = dataSet.Tables["Bookmarks"].Select(findBK);
            if(ToDelBK.Length>0)
            {
                ToDelBK[0].Delete();                
            }
        }

        private void SubOpen_Click(object sender, RoutedEventArgs e)
        {
            dlg.FileName = "Subtitles"; // Default file name
            dlg.DefaultExt = ".srt"; // Default file extension
            dlg.Filter = "Subtitles files (.*)|*.*"; // Filter files by extension
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                VideoControl.VideoProperties.SetSubtitleFile(dlg.FileName);
                VideoControl.VideoProperties.CurrentSpuIndex = 1;
            }
        }

        private void SubDisp_Checked(object sender, RoutedEventArgs e)
        {
            if (VideoControl != null)
            {
                VideoControl.VideoProperties.CurrentSpuIndex = 1;
            }
        }

        private void SubDisp_Unchecked(object sender, RoutedEventArgs e)
        {
            VideoControl.VideoProperties.CurrentSpuIndex = 0;
        }

        private void time_slider_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            FocusManager.SetFocusedElement(MW, MW);
            Keyboard.Focus(MW);
        }

        private void rpbutton_Click(object sender, RoutedEventArgs e)
        {
            Button control = (Button)sender;
            MenuItem MI=control.TryFindParent<MenuItem>();
            RECENTFILE rf = MI.Header as RECENTFILE;
            string oldMoviesFilter = "movieFilename = '" + rf.movieFilename + "'";
            DataRow[] oldMovies = dataSet.Tables["Movies"].Select(oldMoviesFilter);
            if (oldMovies.Length > 0)
            {
                foreach (var oldMovie in oldMovies)
                {
                    dataSet.Tables["Movies"].Rows.Remove(oldMovie);
                }
            }
        }
    }
    public enum DBAction
    {
        Insert,
        Delete,
        Update
    }
}
