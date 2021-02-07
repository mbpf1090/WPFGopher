/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:GopherClient"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;

namespace GopherClient.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MenuViewViewModel>();
            SimpleIoc.Default.Register<TextViewViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<ImageViewViewModel>();
            SimpleIoc.Default.Register<BookmarksViewModel>();
            SimpleIoc.Default.Register<BookmarkEditViewModel>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public SearchViewModel Search
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SearchViewModel>();
            }
        }

        public ImageViewViewModel ImageViewViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ImageViewViewModel>();
            }
        }

        public BookmarksViewModel BookmarksViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BookmarksViewModel>();
            }
        }

        public BookmarkEditViewModel BookmarkEdit
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BookmarkEditViewModel>();
            }
        }

        public static void Cleanup()
        {
        }
    }
}