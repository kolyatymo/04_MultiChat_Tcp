namespace ClientRequest
{
    public enum RequestCommand
    {
        Join, Leave, Message
    }
    public class ClientCommand
    {
        public RequestCommand Command { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public ClientCommand(){}
        public ClientCommand(RequestCommand command)
        {
            Command = command;
        }
        public ClientCommand(string message, RequestCommand command = RequestCommand.Message)
        {
            Message = message;
            Command = command;
        }
        public ClientCommand(string message, string name, DateTime time, RequestCommand command = RequestCommand.Message)
        {
            Message = message;
            Command = command;
            Name = name;
            Time = time;
        }
    }
}
