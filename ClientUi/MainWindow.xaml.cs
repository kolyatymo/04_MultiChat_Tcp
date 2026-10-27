using ClientRequest;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string remoteIP = "127.0.0.1";
        private int port = 4040;
        TcpClient client = new TcpClient();
        ObservableCollection<string> messages;
        private string clientName = "";
        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<string>();
            this.DataContext = messages;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            NameMembers name = new NameMembers();
            name.ShowDialog();

            clientName = name.MyName.Text;

            if (clientName != "")
            {
                if (client.Connected) return;

                await client.ConnectAsync(IPAddress.Parse(remoteIP), port);
                SendRequest(new ClientCommand("", clientName, DateTime.Now, RequestCommand.Join));
                ServerListener();
            }
        }

        private async void SendRequest(ClientCommand command)
        {
            NetworkStream ns = client.GetStream();

            StreamWriter sw = new StreamWriter(ns);
            string json = JsonSerializer.Serialize<ClientCommand>(command);
            await sw.WriteLineAsync(json);
            await sw.FlushAsync();
        }

        private async void ServerListener()
        {
            while (true)
            {
                ClientCommand command = await GetResponse();
                if(command.Command == RequestCommand.Message)
                {
                    messages.Add($"{command.Name}, [{command.Time}] --> {command.Message}");
                }
            }
        }

        private Task<ClientCommand> GetResponse()
        {
            return Task.Run(() =>
            {
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                string resp = sr.ReadLine()!;

                return JsonSerializer.Deserialize<ClientCommand>(resp)!;
            });
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            SendRequest(new ClientCommand(RequestCommand.Leave));
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            SendRequest(new ClientCommand(msgTextBox.Text, clientName, DateTime.Now, RequestCommand.Message));
            msgTextBox.Clear();
        }

        private void Close_Window(object sender, EventArgs e)
        {
            SendRequest(new ClientCommand(RequestCommand.Leave));
        }
    }
}