using eazyonrent.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace eazyonrent.Pages;

public partial class UserChatPage : ContentPage
{
    public ObservableCollection<ChatMessageViewModel> Messages { get; set; }
    private string currentUserId = "user1"; // Aapka current user ID
    private string receiverId = "user2"; // Receiver ka ID

    public  UserChatPage()
    {
        InitializeComponent();
        Messages = new ObservableCollection<ChatMessageViewModel>();
        LoadSampleMessages();
        BindingContext = this;
    }

    private async void LoadSampleMessages()
    {
        // Sample messages jo aapki image mein dikhaye gaye hain
        Messages.Add(new ChatMessageViewModel
        {
            Id = 1,
            SenderId = currentUserId,
            ReceiverId = receiverId,
            Message = "As long as it is a payment option with money transactions, it is highly safe...",
            SentAt = DateTime.Now.AddMinutes(-10),
            Status = MessageStatus.Read,
            IsSentByMe = true
        });

        Messages.Add(new ChatMessageViewModel
        {
            Id = 2,
            SenderId = receiverId,
            ReceiverId = currentUserId,
            Message = "I think you are quite right.\nThe first method of credit cards is important for property security.",
            SentAt = DateTime.Now.AddMinutes(-8),
            Status = MessageStatus.Read,
            IsSentByMe = false
        });

        Messages.Add(new ChatMessageViewModel
        {
            Id = 3,
            SenderId = currentUserId,
            ReceiverId = receiverId,
            Message = "Ok...",
            SentAt = DateTime.Now.AddMinutes(-5),
            Status = MessageStatus.Delivered,
            IsSentByMe = true
        });

        Messages.Add(new ChatMessageViewModel
        {
            Id = 4,
            SenderId = receiverId,
            ReceiverId = currentUserId,
            Message = "Thank you for your reminder, let me learn a lot.",
            SentAt = DateTime.Now.AddMinutes(-2),
            Status = MessageStatus.Read,
            IsSentByMe = false
        });

        // Typing indicator
        Messages.Add(new ChatMessageViewModel
        {
            Id = 5,
            IsTyping = true,
            IsSentByMe = false
        });
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageEntry.Text))
            return;

        var message = new ChatMessageViewModel
        {
            Id = Messages.Count + 1,
            SenderId = currentUserId,
            ReceiverId = receiverId,
            Message = MessageEntry.Text.Trim(),
            SentAt = DateTime.Now,
            Status = MessageStatus.Sent,
            IsSentByMe = true
        };

        Messages.Add(message);
        MessageEntry.Text = string.Empty;

        // Scroll to bottom
        await Task.Delay(100);
        MessagesCollectionView.ScrollTo(Messages.Count - 1, position: ScrollToPosition.End, animate: true);

        // Simulate message delivery and read status
        await Task.Delay(1000);
        message.Status = MessageStatus.Delivered;

        await Task.Delay(2000);
        message.Status = MessageStatus.Read;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

// ViewModel for binding
public class ChatMessageViewModel : INotifyPropertyChanged
{
    private MessageStatus _status;

    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.Now;

    public MessageStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSent));
            OnPropertyChanged(nameof(IsDelivered));
            OnPropertyChanged(nameof(IsRead));
        }
    }

    public bool IsSentByMe { get; set; }
    public bool IsTyping { get; set; } = false;

    public bool IsSent => Status == MessageStatus.Sent && !IsTyping;
    public bool IsDelivered => Status == MessageStatus.Delivered && !IsTyping && !IsRead;
    public bool IsRead => Status == MessageStatus.Read && !IsTyping;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Template Selector
public class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate SentMessageTemplate { get; set; }
    public DataTemplate ReceivedMessageTemplate { get; set; }
    public DataTemplate TypingTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var message = (ChatMessageViewModel)item;

        if (message.IsTyping)
            return TypingTemplate;

        return message.IsSentByMe ? SentMessageTemplate : ReceivedMessageTemplate;
    }
}