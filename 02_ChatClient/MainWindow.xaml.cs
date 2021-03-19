using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
        UdpClient client;
        ObservableCollection<MessageInfo> messages = new ObservableCollection<MessageInfo>();
        string MyName = null;
        Thread ThreadListen;
        public MainWindow()
        {
            InitializeComponent();
            list.ItemsSource = messages;
            ThreadListen = new Thread(() => Listen());
            ThreadListen.IsBackground = true;
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
                              text = msg.Substring(msg.IndexOf('#') + 1);
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

        [Obsolete]
        private void LeaveClick(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                ThreadListen.Suspend();
                SendMessage("#2");
                client.Close();
            }
        }

        [Obsolete]
        private void JoinClick(object sender, RoutedEventArgs e)
        {
            client = new UdpClient();
            bool UniqueNickname = false;
            while (!UniqueNickname)
            {
                Nickname nickname = new Nickname();
                if (nickname.ShowDialog() != true)
                {
                    return;
                }
                SendMessage($"#1{nickname.GetNickname}");
                IPEndPoint iPEndPoint = null;
                byte[] data = client.Receive(ref iPEndPoint);
                string msg = Encoding.UTF8.GetString(data);
                if (msg == "NotUnique#")
                {
                    MessageBox.Show("Is not a unique nickname");
                    continue;
                }
                UniqueNickname = true;
                MyName = nickname.GetNickname;
                messages.Add(new MessageInfo()
                {
                    Time = DateTime.Now.ToShortTimeString(),
                    Text = msg.Substring(1),
                    Type = MessageInfo.TypeMessage.InfoMessage
                });
            }
            if (ThreadListen.ThreadState == (ThreadState.Background | ThreadState.Unstarted))
            {
                ThreadListen.Start();
            }
            else
            {
                ThreadListen.Resume();
            }
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                SendMessage(txtBox.Text);
                txtBox.Text = "";
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (client != null)
            {
                ThreadListen.Abort();
                SendMessage("#2");
                client.Close();
            }
        }
    }
}
