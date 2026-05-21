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
    /// <summary>
    /// Логика взаимодействия для ChooseRoleDialog.xaml
    /// </summary>
    public partial class ChooseRoleDialog : Page
         {
            public static bool IsConfirmed { get; private set; }
            public static int TargetUserId { get; private set; }
            public static int SelectedRoleId { get; private set; }

            public ChooseRoleDialog(List<Roles> roles, int currentRoleId, int userId)
            {
                InitializeComponent();
                TargetUserId = userId;
                RoleCombo.ItemsSource = roles;
                RoleCombo.SelectedValuePath = "Id";
                RoleCombo.DisplayMemberPath = "Name";
                RoleCombo.SelectedValue = currentRoleId;
                IsConfirmed = false;
            }

            private void Save_Click(object sender, RoutedEventArgs e)
            {
                if (RoleCombo.SelectedValue != null)
                {
                    SelectedRoleId = (int)RoleCombo.SelectedValue;
                    IsConfirmed = true;
                    NavigationService.GoBack();
                }
                else
                    MessageBox.Show("Выберите роль");
            }

            private void Cancel_Click(object sender, RoutedEventArgs e)
            {
                IsConfirmed = false;
                NavigationService.GoBack();
            }
        }
    }