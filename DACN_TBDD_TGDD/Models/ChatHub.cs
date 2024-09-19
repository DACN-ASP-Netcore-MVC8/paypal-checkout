using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(string senderId, string receiverId, string message)
    {
        // Logic to send a message to the specified receiver
        // Optionally, check if the message is being sent by the correct user
        var isSender = Context.User.Identity.Name == senderId;
        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, isSender);
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, message, true); // Send back to sender
    }
}

