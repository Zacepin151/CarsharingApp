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

namespace CarsharingApp
{
    /// <summary>
    /// Interaction logic for AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadUsers(); 
        }

        private void LoadUsers()
        {
            using (var context = new CarsharingdbEntities())
            {
                Users.ItemsSource = context.Users.ToList();
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.ShowDialog();

           
            LoadUsers();
        }
            private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
           
            var selectedUser = Users.SelectedItem as Users;

            if (selectedUser == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить пользователя {selectedUser.firstname} {selectedUser.lastname}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new CarsharingdbEntities())
                    {
                       
                        var userToDelete = context.Users.Find(selectedUser.id);

                        if (userToDelete != null)
                        {
                            context.Users.Remove(userToDelete);
                            context.SaveChanges();

                            MessageBox.Show("Пользователь успешно удален!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                           
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден в базе данных", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nПодробности: {ex.InnerException.Message}";
                    }

                    MessageBox.Show($"Ошибка при удалении: {errorMessage}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ISLocedUserButton_Click(object sender, RoutedEventArgs e)
        {
           
            var selectedUser = Users.SelectedItem as Users;

            if (selectedUser == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для разблокировки",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

         
            if (selectedUser.IsLoced != true)  
            {
                MessageBox.Show($"Пользователь {selectedUser.firstname} {selectedUser.lastname} не заблокирован",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

           
            MessageBoxResult result = MessageBox.Show(
                $"Разблокировать пользователя {selectedUser.firstname} {selectedUser.lastname}?",
                "Подтверждение разблокировки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new CarsharingdbEntities())
                    {
                     
                        var userToUnlock = context.Users.Find(selectedUser.id);

                        if (userToUnlock != null)
                        {
                           
                            userToUnlock.IsLoced = false;
                            userToUnlock.FailledLoginAttempts = 0; 

                            context.SaveChanges();

                            MessageBox.Show($"Пользователь {userToUnlock.firstname} {userToUnlock.lastname} успешно разблокирован!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                           
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден в базе данных", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nПодробности: {ex.InnerException.Message}";
                    }

                    MessageBox.Show($"Ошибка при разблокировке: {errorMessage}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
       