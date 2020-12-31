using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GopherClient.Model;
using GopherClient.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GopherClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Client client;
        public GopherLine SelectedLine { get; set; }

        private List<GopherLine> _menu;
        public List<GopherLine> Menu 
        {
            get => _menu; 
            set
            {
                _menu = value;
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
                GenerateData();
                Address = "gopher.floodgap.com";
            }
            else
            {
                client = new Client();
            }

            OpenLineCmd = new RelayCommand<GopherLine>((line) => OpenLine(line));
            OpenAddressCmd = new RelayCommand(OpenAddress, CanOpenAddress);
            GoBackCmd = new RelayCommand(GoBack, client.CanGoBack);
            Address = "gopher.floodgap.com";

        }

        private void GoBack()
        {
            var rawContent = client.GoBack();
            Menu = Parser.Parse(rawContent);
            Address = client.currentSite.Host + client.currentSite.Selector;
        }

        private async void OpenAddress()
        {
            if (Address != null)
            {
                string[] checkedAddress = Parser.CheckAddress(Address);
                GopherLine destination = new GopherLine("1", "", checkedAddress[3], checkedAddress[1], checkedAddress[2].Equals("") ? "70" : checkedAddress[2]);
                var rawContent = await client.GetMenuContentAsync(destination);
                Menu = Parser.Parse(rawContent);

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
            try
            {
                var rawContent = await client.GetMenuContentAsync(gopherLine);
                Menu = Parser.Parse(rawContent);
                Address = client.currentSite.Host + client.currentSite.Selector;

            } catch (TaskCanceledException)
            {
                return;
            }
        }

        private void GenerateData()
        {
            Menu = new List<GopherLine>()
                    {
                        new GopherLine("i", "Welcome to my gopher hole!", "", "", "70"),
                        new GopherLine("i", "", "", "", "70"),
                        new GopherLine("0", "About Me", "", "", "70"),
                        new GopherLine("i", "", "", "", "70"),
                        new GopherLine("i", "Here are some books I I've read", "", "", "70"),
                        new GopherLine("1", "Books", "", "", "70"),
                        new GopherLine("i", "", "", "", "70"),
                        new GopherLine("i", "I also like to write about movies", "", "", "70"),
                        new GopherLine("1", "Movies", "", "", "70"),
                        new GopherLine("i", "I collect knitting projects", "", "", "70"),
                        new GopherLine("1", "Knitting projects", "", "", "70")
                    };
        }
    }
}