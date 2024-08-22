using Microsoft.AspNetCore.SignalR;

public class CommentHub : Hub
{
    public async Task SendCommentUpdate()
    {
        await Clients.All.SendAsync("ReceiveCommentUpdate");
    }
}