using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopherClient.ViewModel
{
    public class MenuViewViewModel : ViewModelBase
    {
        
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

        public GopherLine SelectedLine { get; set; }

        public RelayCommand<GopherLine> OpenLineCmd { get; set; }

        public MenuViewViewModel()
        {
            Menu = new List<GopherLine>();
            OpenLineCmd = new RelayCommand<GopherLine>((line) => OpenLine(line));
            
        }

        private void OpenLine(GopherLine line)
        {
            Debug.WriteLine(line.Host);
            MessengerInstance.Send<GopherLine>(line);
        }
    }
}
