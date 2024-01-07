using OnlineChatMvc.Data;

namespace OnlineChatMvc.Services
{
    public class ChatService
    {
        private readonly ChatContext _context;
        public ChatService(ChatContext context) 
        {
            _context = context;
        }

        public async Task DeleteOldMessages()
        {
            var messages = _context.Messages.Where(x => x.Data < DateTime.Now.AddMinutes(-1)).ToList();

            _context.Messages.RemoveRange(messages);
          await  _context.SaveChangesAsync();

        }
    }
}
