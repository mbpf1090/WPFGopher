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
using System.Threading;
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

        private double _gridOpacity;
        public double GridOpacity 
        {
            get => _gridOpacity;
            set
            {
                _gridOpacity = value;
                RaisePropertyChanged();
            }
        }

        private bool _isRequesting;
        public bool IsRequesting 
        {
            get => _isRequesting;
            set
            {
                _isRequesting = value;
                RaisePropertyChanged();
            }
        }

        public GopherLine CurrentLine { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        public RelayCommand ToggleMenuCmd { get; set; }
        public RelayCommand<GopherLine> OpenLineCmd { get; set; }
        public RelayCommand OpenAddressCmd { get; set; }
        public RelayCommand GoBackCmd { get; set; }
        public RelayCommand<string> NavCmd { get; set; }
        public RelayCommand QuitCmd { get; set; }
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
            GridOpacity = 1;

            // Commands
            OpenAddressCmd = new RelayCommand(OpenAddress, CanOpenAddress);
            GoBackCmd = new RelayCommand(GoBack, client.CanGoBack);
            NavCmd = new RelayCommand<string>(Navigate);
            QuitCmd = new RelayCommand(() => { App.Current.Shutdown(); });

            Address = "gopher.floodgap.com";
            IsRequesting = false;
            TokenSource = new CancellationTokenSource();

            //Setup Messenger
            MessengerInstance.Register<GopherLine>(this, OpenLine);
            MessengerInstance.Register<string>(this, GetSearchTerm);
            MessengerInstance.Register<GopherLine>(this, 1, UpdateInfoLabel);
        }

        #region Methods
        private async void Navigate(string menuItem)
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
                        await bmVM.UpdateBookmarksList();
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
            searchLine.Selector += $"\t{s}";
            try
            {
                IsRequesting = true;
                var rawContent = await client.GetMenuContentAsync(searchLine, TokenSource.Token);
                Address = client.currentSite.Host + client.currentSite.Selector;
                ((MenuViewViewModel)CurrentContentView).Menu = Parser.Parse(rawContent);
            }
            catch (TaskCanceledException)
            {
                IsRequesting = false;
                return;
            }

            IsRequesting = false;
        }

        private void GoBack()
        {
            IsRequesting = true;
            var rawContent = client.GoBack();
            Address = client.currentSite.Host + client.currentSite.Selector;

            CurrentLine = client.currentSite;

            MenuViewViewModel menuViewViewModel = SimpleIoc.Default.GetInstance<MenuViewViewModel>();
            menuViewViewModel.Menu = Parser.Parse(rawContent);
            CurrentContentView = menuViewViewModel;
            IsRequesting = false;
        }

        private async void OpenAddress()
        {
            
            if (Address != null)
            {
                IsRequesting = true;
                string[] checkedAddress = Parser.CheckAddress(Address);

                // [0]string type, [1]string userDisplay, [2]string selector, [3]string host, [4]string port
                GopherLine destination = new GopherLine("1", "", checkedAddress[2], checkedAddress[1], checkedAddress[3].Equals("") ? "70" : checkedAddress[3]);
                var rawContent = await client.GetMenuContentAsync(destination, TokenSource.Token);
                MenuViewViewModel menuViewViewModel = SimpleIoc.Default.GetInstance<MenuViewViewModel>();
                menuViewViewModel.Menu = Parser.Parse(rawContent);
                CurrentContentView = menuViewViewModel;

                CurrentLine = client.currentSite;
                IsRequesting = false;
            }
            
        }

        private bool CanOpenAddress()
        {
            return !string.IsNullOrWhiteSpace(Address);
        }

        private async void OpenLine(GopherLine gopherLine)
        {
            if (IsRequesting)
            {
                TokenSource.Cancel();
                TokenSource = new CancellationTokenSource();
            }

            CurrentLine = gopherLine;

            switch (gopherLine.Type)
            {
                // Submenu
                case "1":
                    try
                    {
                        IsRequesting = true;
                        var rawContent = await client.GetMenuContentAsync(gopherLine, TokenSource.Token);
                        Address = client.currentSite.Host + client.currentSite.Selector;
                        try
                        {
                            MenuViewViewModel menuViewViewModel = SimpleIoc.Default.GetInstance<MenuViewViewModel>();
                            menuViewViewModel.Menu = Parser.Parse(rawContent);
                            CurrentContentView = menuViewViewModel;
                            IsRequesting = false;
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Debug.WriteLine(ex.Message);
                            return;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        IsRequesting = false;
                        Debug.WriteLine("Catch MainViewModel OpenLine");
                        return;
                    }
                    break;
                // Text file
                case "0":
                    try
                    {
                        IsRequesting = true;
                        var rawContent = await client.GetTextContentAsync(gopherLine);
                        Address = client.currentSite.Host + client.currentSite.Selector;
                        
                        TextViewViewModel textViewViewModel = SimpleIoc.Default.GetInstance<TextViewViewModel>();
                        textViewViewModel.TextContent = rawContent;
                        
                        CurrentContentView = textViewViewModel;
                        IsRequesting = false;

                    }
                    catch (OperationCanceledException)
                    {
                        IsRequesting = false;
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

                    GridOpacity = 0.2;
                    searchWindow.ShowDialog();
                    GridOpacity = 1;
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
                    // TODO Bug current site not added to history
                    // TODO Add cancellation
                    IsRequesting = true;
                    ImageViewViewModel imageViewViewModel = SimpleIoc.Default.GetInstance<ImageViewViewModel>();
                    imageViewViewModel.ImageSource = client.GetImage(gopherLine);
                    CurrentContentView = imageViewViewModel;
                    break;
                default:
                    IsRequesting = false;
                    return;
            }
            IsRequesting = false;
        }
        #endregion
    }
}