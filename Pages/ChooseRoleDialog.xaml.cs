using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace uch.Pages
{
    public partial class ChooseRoleDialog : Page
    {
        public static bool IsConfirmed { get; private set; }
        public static int TargetUserId { get; private set; }
        public static int SelectedRoleId { get; private set; }

        // Исправлено: Roles вместо Role (если у вас класс называется Roles)
        public ChooseRoleDialog(List<Roles> roles, int currentRoleId, int userId)
        {
            InitializeComponent();

            // Сбрасываем статические поля
            IsConfirmed = false;
            TargetUserId = userId;
            SelectedRoleId = currentRoleId;

            RoleCombo.ItemsSource = roles;
            RoleCombo.SelectedValuePath = "Id";
            RoleCombo.DisplayMemberPath = "Name";
            RoleCombo.SelectedValue = currentRoleId;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (RoleCombo.SelectedValue != null)
            {
                SelectedRoleId = (int)RoleCombo.SelectedValue;
                IsConfirmed = true;  // Устанавливаем флаг ПЕРЕД возвратом
                NavigationService.GoBack();
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            NavigationService.GoBack();
        }
    }
}