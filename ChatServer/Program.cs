using ClientRequest;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Xml.Serialization;

internal class Program
{
    private const string localIp = "127.0.0.1";
    private const int port = 4040;

    static TcpListener server;
    private static void Main(string[] args)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(localIp), port);
        server = new TcpListener(localEndPoint);
        Chat chat = new Chat();
        server.Start();

        while (true)
        {
            Console.WriteLine("Waiting for a request ... ");
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("\t Got a TcpClient");
            ClientCommand req = GetClientRequest(client);
            Console.WriteLine($"\t Got a {req.Command} request!");

            if(req.Command == RequestCommand.Join)
            {
                chat.AddMember(client, req.Name);
            }

            if(req.Command == RequestCommand.Leave)
            {
                chat.RemoveMember(client);
            }
        }
    }
    static ClientCommand GetClientRequest(TcpClient client)
    {
        NetworkStream ns = client.GetStream();
        StreamReader sr = new StreamReader(ns);

        return JsonSerializer.Deserialize<ClientCommand>(sr.ReadLine()!)!;
    }
}

class Chat
{
    private const int MaxPeople = 10;

    List<TcpClient> members = new List<TcpClient>();

    public void AddMember(TcpClient client, string name)
    {
        if(members.Count == MaxPeople)
        {
            Console.WriteLine(name + " Max people Sory!!!");
            return;
        }

        members.Add(client);
        Console.WriteLine($"{name} was added");
        Task.Run(() => ListenerClient(client));
    }

    public void RemoveMember(TcpClient client)
    {
        members.Remove(client);
        client.Close();
        Console.WriteLine(client.Client.AddressFamily + " Leave");
    }

    public void ListenerClient(TcpClient client)
    {
        bool isExit = false;
        while (!isExit)
        {
            ClientCommand command = GetClientCommand(client);
            switch (command.Command)
            {
                case RequestCommand.Leave:
                    RemoveMember(client);
                    isExit = true;
                    break;
                case RequestCommand.Message:
                    foreach (var member in members)
                    {
                        SendMessage(command, member);
                    }
                    break;
                default:
                    break;
            }

        }
    }
    private ClientCommand GetClientCommand(TcpClient client)
    {
        NetworkStream ns = client.GetStream();
        StreamReader sr = new StreamReader(ns);

        return JsonSerializer.Deserialize<ClientCommand>(sr.ReadLine()!)!;
    }
    private void SendMessage(ClientCommand command, TcpClient reciever)
    {
        NetworkStream ns = reciever.GetStream();
        StreamWriter sw = new StreamWriter(ns);

        sw.WriteLine(JsonSerializer.Serialize<ClientCommand>(command));
        sw.Flush();
    }
}