using AppUTH.Views.Alumno;
using AppUTH.Views.Amigos;
using AppUTH.Views.Menu_Grupos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Menu
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageMenuFlyout : ContentPage
    {
        public ListView ListView;

        public PageMenuFlyout()
        {
            InitializeComponent();

            BindingContext = new PageMenuFlyoutViewModel();
            ListView = MenuItemsListView;
        }

        private class PageMenuFlyoutViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<PageMenuFlyoutMenuItem> MenuItems { get; set; }

            public PageMenuFlyoutViewModel()
            {
                MenuItems = new ObservableCollection<PageMenuFlyoutMenuItem>(new[]
                {
                    new PageMenuFlyoutMenuItem { Id = 0, Title = "Amigos", TargetType = typeof(PageMenuAmigos)},
                    new PageMenuFlyoutMenuItem { Id = 1, Title = "Grupos", TargetType = typeof(PageMenuGrupos) },
                    new PageMenuFlyoutMenuItem { Id = 2, Title = "Perfil", TargetType = typeof(PagePerfilAlumno) },
                    //new PageMenuFlyoutMenuItem { Id = 3, Title = "" },
                    //new PageMenuFlyoutMenuItem { Id = 4, Title = ""},
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}