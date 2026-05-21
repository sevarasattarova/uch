using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace uch.Pages
{
    /// <summary>
    /// Логика взаимодействия для MoveBookDialog.xaml
    /// </summary>
    public partial class MoveBookDialog : Window
    {
            private int _bookId;
            private string _currentListType;

            public MoveBookDialog(int bookId, string currentListType)
            {
                InitializeComponent();
                _bookId = bookId;
                _currentListType = currentListType;
                // удаляем из комбобокса текущий список
                foreach (ComboBoxItem item in TargetListCombo.Items)
                {
                    if (item.Tag.ToString() == currentListType)
                    {
                        item.IsEnabled = false;
                        break;
                    }
                }
            }

            private void Move_Click(object sender, RoutedEventArgs e)
            {
                var selected = TargetListCombo.SelectedItem as ComboBoxItem;
                if (selected == null)
                {
                    MessageBox.Show("Выберите целевой список");
                    return;
                }
                string newListType = selected.Tag.ToString();

                var userBook = Core.Context.UserBookLists.FirstOrDefault(ub => ub.UserId == Core.CurrentUser.Id && ub.BookId == _bookId);
                if (userBook != null)
                {
                    userBook.ListType = newListType;
                    Core.Context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }

            private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        }
    }