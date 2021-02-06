using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GopherClient.Model;
using GopherClient.Service;
using GopherClient.View;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GopherClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Client client;
        private GopherLine searchLine;

        #region Properties
        private ViewModelBase _currentContentView;
        public ViewModelBase CurrentContentView
        { 
            get => _currentContentView;
            set
            {
                _currentContentView = value;
                RaisePropertyChanged();
            }
        }


        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                RaisePropertyChanged();
            }
        }

        private string _infoLabel;
        public string InfoLabel
        {
            get => _infoLabel;
            set
            {
                _infoLabel = value;
                RaisePropertyChanged();
            }
        }


        private int _progress;
        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }

        public GopherLine CurrentLine { get; set; }

        public RelayCommand ToggleMenuCmd { get; set; }
        public RelayCommand<GopherLine> OpenLineCmd { get; set; }
        public RelayCommand OpenAddressCmd { get; set; }
        public RelayCommand GoBackCmd { get; set; }
        public RelayCommand<string> NavCmd { get; set; }
        #endregion

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                Address = "gopher.floodgap.com";
            }
            else
            {
                client = new Client();
            }

            CurrentContentView = SimpleIoc.Default.GetInstance<MenuViewViewModel>();

            // Commands
            OpenAddressCmd = new RelayCommand(OpenAddress, CanOpenAddress);
            GoBackCmd = new RelayCommand(GoBack, client.CanGoBack);
            NavCmd = new RelayCommand<string>(Navigate);

            Address = "gopher.floodgap.com";

            //Setup Messenger
            MessengerInstance.Register<GopherLine>(this, OpenLine);
            MessengerInstance.Register<string>(this, GetSearchTerm);
            MessengerInstance.Register<GopherLine>(this, 1, UpdateInfoLabel);

            // Create bookmarks file
            Bookmarker.CreateInitialBookmarks();
        }

        #region Methods
        private void Navigate(string menuItem)
        {
            switch (menuItem)
            {
                case "bookmarks":
                    BookmarksViewModel bookmarksViewModel = SimpleIoc.Default.GetInstance<BookmarksViewModel>();
                    //menuViewViewModel.Menu = Bookmarker.LoadBookmarks();
                    CurrentContentView = bookmarksViewModel;
                    break;
                case "addbookmark":
                    if (CurrentLine != null)
                    {
                        using (var db = new GopherDB())
                        {
                            Bookmark b = new Bookmark(CurrentLine);
                            db.Insert(b);
                        }
                        BookmarksViewModel bmVM = SimpleIoc.Default.GetInstance<BookmarksViewModel>();
                        bmVM.UpdateBookmarksList();
                    }
                    break;
                default:
                    break;
            }
        }


        private void UpdateInfoLabel(GopherLine line)
        {
            if (line != null)
                InfoLabel = $"{line.Host}{line.Selector}:{line.Port}";
        }

        private async void GetSearchTerm(string s)
        {
            Debug.WriteLine(s);
            searchLine.Selector += $"\t{s}";
            try
            {
                ResetProgressBar();
                var progress = new Progress<int>((value) =>
                {
                    Progress = value;
                });

                var rawContent = await client.GetMenuContentAsync(searchLine, progress);
                Address = client.currentSite.Host + client.currentSite.Selector;
                ((MenuViewViewModel)CurrentContentView).Menu = Parser.Parse(rawContent);
            }
            catch (TaskCanceledException)
            {
                return;
            }


        }

        private void GoBack()
        {
            var rawContent = client.GoBack();
            Address = client.currentSite.Host + client.currentSite.Selector;

            CurrentLine = client.currentSite;

            MenuViewViewModel menuViewViewModel = SimpleIoc.Default.GetInstance<MenuViewViewModel>();
            menuViewViewModel.Menu = Parser.Parse(rawContent);
            CurrentContentView = menuViewViewModel;
        }

        private async void OpenAddress()
        {
            
            if (Address != null)
            {
                ResetProgressBar();
                var progress = new Progress<int>((value) =>
                {
                    Progress = value;
                });

                string[] checkedAddress = Parser.CheckAddress(Address);
                // [0]string type, [1]string userDisplay, [2]string selector, [3]string host, [4]string port
                GopherLine destination = new GopherLine("1", "", checkedAddress[2], checkedAddress[1], checkedAddress[3].Equals("") ? "70" : checkedAddress[3]);
                var rawContent = await client.GetMenuContentAsync(destination, progress);
                MenuViewViewModel menuViewViewModel = SimpleIoc.Default.GetInstance<MenuViewViewModel>();
                menuViewViewModel.Menu = Parser.Parse(rawContent);
                CurrentContentView = menuViewViewModel;

                CurrentLine = client.currentSite;
            }
            
        }

        private bool CanOpenAddress()
        {
            return !string.IsNullOrWhiteSpace(Address);
        }

        private async void OpenLine(GopherLine gopherLine)
        {
            ResetProgressBar();
            var progress = new Progress<int>((value) =>
            {
                Progress = value;
            });

            CurrentLine = gopherLine;


            switch (gopherLine.Type)
            {
                // Submenu
                case "1":
                    try
                    {
                        var rawContent = await client.GetMenuContentAsync(gopherLine, progress);
                        Address = client.currentSite.Host + client.currentSite.Selector;
                        try
                        {
                            MenuViewViewModel menuViewViewModel = SimpleIoc.Default.GetInstance<MenuViewViewModel>();
                            menuViewViewModel.Menu = Parser.Parse(rawContent);
                            CurrentContentView = menuViewViewModel;
                            //((MenuViewViewModel)CurrentContentView).Menu = Parser.Parse(rawContent);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Debug.WriteLine(ex.Message);
                            return;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Catch MainViewModel OpenLine");
                        return;
                    }
                    break;
                // Text file
                case "0":
                    try
                    {
                        
                        var rawContent = await client.GetTextContentAsync(gopherLine);
                        Address = client.currentSite.Host + client.currentSite.Selector;
                        //SimpleIoc.Default.GetInstance<TextViewViewModel>();
                        TextViewViewModel textViewViewModel = SimpleIoc.Default.GetInstance<TextViewViewModel>();
                        textViewViewModel.TextContent = rawContent;
                        
                        CurrentContentView = textViewViewModel;
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    break;
                // Search
                case "7":
                    searchLine = gopherLine;
                    SearchWindow searchWindow = new SearchWindow
                    {
                        Owner = App.Current.MainWindow
                    };
                    searchWindow.ShowDialog();
                    break;
                // HTML link
                case "h":
                    // Remove leading "URL:"
                    string url = gopherLine.Selector.Remove(0, 4);
                    // Basic URL checking
                    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                        Process.Start(url);
                    break;
                // Image
                case "g":
                case "I":
                    // TODO: Bug current site not added to history
                    ImageViewViewModel imageViewViewModel = SimpleIoc.Default.GetInstance<ImageViewViewModel>();
                    imageViewViewModel.ImageSource = client.GetImage(gopherLine);
                    CurrentContentView = imageViewViewModel;
                    break;
                default:
                    return;
            }
        }

        private void ResetProgressBar()
        {
            Progress = 0;
        }
        #endregion
    }
}