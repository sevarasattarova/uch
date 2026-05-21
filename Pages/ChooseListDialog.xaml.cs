using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace uch.Pages
{


        public partial class ChooseListDialog : Page
        {
            public string SelectedListType { get; private set; }
            public bool IsConfirmed { get; private set; }

            public ChooseListDialog()
            {
                InitializeComponent();
                IsConfirmed = false;
            }

            private void Select_Click(object sender, RoutedEventArgs e)
            {
                var selected = ListCombo.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    SelectedListType = selected.Tag.ToString();
                    IsConfirmed = true;
                    NavigationService.GoBack(); // возврат на предыдущую страницу
                }
                else
                    MessageBox.Show("Выберите список");
            }

            private void Cancel_Click(object sender, RoutedEventArgs e)
            {
                IsConfirmed = false;
                NavigationService.GoBack();
            }
        }
    }