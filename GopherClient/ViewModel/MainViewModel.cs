using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GopherClient.Model;
using GopherClient.Service;
using GopherClient.View;
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


        public RelayCommand<GopherLine> OpenLineCmd { get; set; }
        public RelayCommand OpenAddressCmd { get; set; }
        public RelayCommand GoBackCmd { get; set; }

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

            CurrentContentView = new MenuViewViewModel();

            OpenAddressCmd = new RelayCommand(OpenAddress, CanOpenAddress);
            GoBackCmd = new RelayCommand(GoBack, client.CanGoBack);
            Address = "gopher.floodgap.com";

            //Setup Messenger
            MessengerInstance.Register<GopherLine>(this, OpenLine);
            MessengerInstance.Register<string>(this, GetSearchTerm);

        }

        private async void GetSearchTerm(string s)
        {
            Debug.WriteLine(s);
            searchLine.Selector += $"\t{s}";
            try
            {
                var rawContent = await client.GetMenuContentAsync(searchLine);
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
            MenuViewViewModel menuViewViewModel = new MenuViewViewModel();
            menuViewViewModel.Menu = Parser.Parse(rawContent);
            CurrentContentView = menuViewViewModel;
        }

        private async void OpenAddress()
        {
            
            if (Address != null)
            {
                string[] checkedAddress = Parser.CheckAddress(Address);
                GopherLine destination = new GopherLine("1", "", checkedAddress[3], checkedAddress[1], checkedAddress[2].Equals("") ? "70" : checkedAddress[2]);
                var rawContent = await client.GetMenuContentAsync(destination);
                ((MenuViewViewModel) CurrentContentView).Menu = Parser.Parse(rawContent);
            }
            
        }

        private bool CanOpenAddress()
        {
            if (Address.Equals(""))
                return false;
            return true;
        }

        private async void OpenLine(GopherLine gopherLine)
        {
            client.CancelRequest();
            switch (gopherLine.Type)
            {
                case "1":
                    try
                    {
                        var rawContent = await client.GetMenuContentAsync(gopherLine);
                        Address = client.currentSite.Host + client.currentSite.Selector;
                        try
                        {
                            ((MenuViewViewModel)CurrentContentView).Menu = Parser.Parse(rawContent);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Debug.WriteLine(ex.Message);
                            return;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    break;
                case "0":
                    try
                    {
                        
                        var rawContent = await client.GetTextContentAsync(gopherLine);
                        Address = client.currentSite.Host + client.currentSite.Selector;
                        TextViewViewModel textViewViewModel = new TextViewViewModel();
                        textViewViewModel.TextContent = rawContent;
                        CurrentContentView = textViewViewModel;
                        //((TextViewViewModel)CurrentContentView).TextContent = rawContent;
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    break;
                case "7":
                    searchLine = gopherLine;
                    SearchWindow searchWindow = new SearchWindow();
                    searchWindow.Owner = App.Current.MainWindow;
                    searchWindow.ShowDialog();
                    break;
                default:
                    return;
            }
        }
    }
}