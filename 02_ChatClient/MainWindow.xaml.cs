using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace _02_ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // адреса віддаленого хоста
        private static string remoteIPAddress = "127.0.0.1";
        // порт віддаленого хоста
        private static int remotePort = 8080;
        // створення об'єкту UdpClient для відправки даних
        UdpClient client = new UdpClient(0);

        ObservableCollection<MessageInfo> messages = new ObservableCollection<MessageInfo>();
        string MyName = "Andriy";
        public MainWindow()
        {
            InitializeComponent();           
            list.ItemsSource = messages;

        }

        private void Listen()
        {
            IPEndPoint iPEndPoint = null;
            while (true)
            {
                try
                {
                    byte[] data = client.Receive(ref iPEndPoint);

                    string msg = Encoding.UTF8.GetString(data);

                    _ = Dispatcher.BeginInvoke(new Action(() =>
                      {
                          MessageInfo.TypeMessage typeMessage;
                          string text = "";
                          string user = "";
                          if (msg[0] == '@')
                          {
                              text = msg.Substring(1);
                              typeMessage = MessageInfo.TypeMessage.InfoMessage;
                          }
                          else
                          {
                              user = msg.Substring(0, msg.IndexOf('#'));
                              if (MyName == user)
                              {
                                  typeMessage = MessageInfo.TypeMessage.MyMessage;
                              }
                              else
                              {
                                  typeMessage = MessageInfo.TypeMessage.TextMessage;
                              }
                              text = msg.Substring(msg.IndexOf('#')+1);
                          }
                          messages.Insert(0, new MessageInfo()
                          {
                              User = user,
                              Time = DateTime.Now.ToShortTimeString(),
                              Text = text,
                              @Type = typeMessage
                          });
                      }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void SendMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);

            byte[] data = Encoding.UTF8.GetBytes(msg);
            client.Send(data, data.Length, iPEndPoint);
        }

        private void LeaveClick(object sender, RoutedEventArgs e)
        {
            SendMessage("#2");
        }

        private void JoinClick(object sender, RoutedEventArgs e)
        {
            bool UniqueNickname = false;
            while (!UniqueNickname)
            {
                Nickname nickname = new Nickname();
                nickname.ShowDialog();
                SendMessage($"#1{MyName}");
                IPEndPoint iPEndPoint = null;
                byte[] data = client.Receive(ref iPEndPoint);
                string msg = Encoding.UTF8.GetString(data);
                if (msg== "NotUnique#")
                {
                    MessageBox.Show("Is not a unique nickname");
                    continue;
                }
                messages.Add(new MessageInfo()
                {
                    Time = DateTime.Now.ToShortTimeString(),
                    Text = msg,
                    Type = MessageInfo.TypeMessage.InfoMessage
                });
            }
            Task.Run(() => Listen());
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            SendMessage(txtBox.Text);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SendMessage("#2");
            client.Close();
        }
    }
}
