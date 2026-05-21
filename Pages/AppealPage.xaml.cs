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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace uch.Pages
{
    /// <summary>
    /// Логика взаимодействия для AppealPage.xaml
    /// </summary>
    public partial class AppealPage : Page
    {
        public AppealPage()
        {
            InitializeComponent();
            
                LoadFreezeInfo();
            }

            private void LoadFreezeInfo()
            {
                if (Core.CurrentUser != null && Core.CurrentUser.IsFrozen)
                {
                    ReasonText.Text = $"Причина заморозки: {Core.CurrentUser.FreezeReason}";
                }
                else
                {
                    ReasonText.Text = "Ваш аккаунт не заморожен.";
                    SendAppealBtn.IsEnabled = false;
                }
            }

            private void SendAppeal_Click(object sender, RoutedEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(AppealReason.Text))
                {
                    MessageBox.Show("Укажите причину оспаривания.");
                    return;
                }
                var existingAppeal = Core.Context.FreezeAppeals
                    .FirstOrDefault(a => a.UserId == Core.CurrentUser.Id && a.TargetType == "User" && a.TargetId == Core.CurrentUser.Id && a.Status == "Pending");
                if (existingAppeal != null)
                {
                    MessageBox.Show("Заявка уже отправлена и ожидает рассмотрения.");
                    return;
                }
                var appeal = new FreezeAppeals
                {
                    UserId = Core.CurrentUser.Id,
                    TargetType = "User",
                    TargetId = Core.CurrentUser.Id,
                    Reason = AppealReason.Text,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };
                Core.Context.FreezeAppeals.Add(appeal);
                Core.Context.SaveChanges();
                MessageBox.Show("Заявка на разморозку отправлена администратору.");
            }
        }
    }

