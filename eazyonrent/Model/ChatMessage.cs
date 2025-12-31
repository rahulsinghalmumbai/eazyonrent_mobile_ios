using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public enum MessageStatus
    {
        Sent = 0,
        Delivered = 1,
        Read = 2
    }
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;


        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
