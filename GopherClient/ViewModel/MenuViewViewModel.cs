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
        #region Properties
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

        private GopherLine _selectedLine;
        public GopherLine SelectedLine
        {
            get => _selectedLine;
            set
            {
                _selectedLine = value;
                SelectionChanged(_selectedLine);
            }
        }

        public RelayCommand<GopherLine> OpenLineCmd { get; set; }
        #endregion

        public MenuViewViewModel()
        {
            Menu = new List<GopherLine>();
            OpenLineCmd = new RelayCommand<GopherLine>((line) => OpenLine(line));
            
        }

        #region Methods
        private void OpenLine(GopherLine line)
        {
            Debug.WriteLine(line.Host);
            MessengerInstance.Send<GopherLine>(line);
        }

        private void SelectionChanged(GopherLine line)
        {
            MessengerInstance.Send<GopherLine>(line, 1);
        }
        #endregion
    }
}
