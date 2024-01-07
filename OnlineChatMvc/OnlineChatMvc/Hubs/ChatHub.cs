using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineChatMvc.Data;
using OnlineChatMvc.Models;

namespace OnlineChatMvc.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatContext _context;

        public ChatHub(ChatContext context) 
        {
            _context = context;
        }

        [Authorize]
        public async Task Send(string message)
        {

            var newMessage = new Message
            {
                UserId = GetUserId(),
                Text = message,
                Data = DateTime.Now

            };

            await _context.Messages.AddAsync(newMessage);
            await _context.SaveChangesAsync();


            var messageDto = new MessageDto
            {
                Id = newMessage.Id,
                Name = Context.User.Identity.Name,
                Data = DateTime.Now.ToString("dd.MM HH:mm"),
                Message = message
            };

            await Clients.All.SendAsync("Receive", messageDto );
        }

        [Authorize]
        public async Task DeleteMessage(int id)
        {
            var message = _context.Messages.FirstOrDefault(x => x.Id == id);

            if (message != null)
          {
                if (message.UserId == GetUserId() || Context.User.IsInRole(UserRole.Admin.ToString()))
            {
                _context.Messages.Remove(message);
                _context.SaveChanges();
            }
         }

            await Clients.All.SendAsync("HideMessage", message.Id);
        }

        private int GetUserId()
        {
            var userIdstr = Context.User.FindFirst("Id")?.Value;
            var userId = Convert.ToInt32(userIdstr);

            return userId;
        }
    }

}
