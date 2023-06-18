using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using kate.shared.Helpers;
using Newtonsoft.Json.Linq;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Models;

public class BaseChannel : Clientable, IBaseChannel
{
    /// <summary>
    /// Unique Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// Id of the user this channel belongs to
    /// </summary>
    [JsonPropertyName("channel_type")]
    public string ChannelType { get; set; }

    public virtual Task<bool> Fetch(Client client)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Fetch()
        => Fetch(Client);
    protected async Task<T?> GetGeneric<T>(string id, Client client) where T : BaseChannel
    {
        var response = await client.GetAsync($"/channels/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch channel {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<T>(stringContent, Client.SerializerOptions);
        return data;
    }

    public event GenericDelegate<string> MessageDeleted;

    internal void OnMessageDeleted(string messageId)
    {
        if (MessageDeleted != null)
            MessageDeleted?.Invoke(messageId);
    }

    /// <summary>
    /// Invoked when a user starts typing in this channel.
    /// </summary>
    public event ChannelIdDelegate StartTyping;
    /// <summary>
    /// Invoke <see cref="StartTyping"/>
    /// </summary>
    internal void OnStartTyping(string userId)
    {
        StartTyping?.Invoke(userId);
    }
    /// <summary>
    /// Invoked when a user stops typing in this channel.
    /// </summary>
    public event ChannelIdDelegate StopTyping;
    /// <summary>
    /// Invoke <see cref="StopTyping"/>
    /// </summary>
    internal void OnStopTyping(string userId)
    {
        StopTyping?.Invoke(userId);
    }

    public event VoidDelegate Deleted;
    /// <summary>
    /// Invoke <see cref="Deleted"/>
    /// </summary>
    internal void OnDeleted()
    {
        Deleted?.Invoke();
    }

    protected Task<T?> GetGeneric<T>(string id) where T : BaseChannel
        => GetGeneric<T>(id, Client);
    protected Task<T?> GetGeneric<T>(Client client) where T : BaseChannel
        => GetGeneric<T>(Id, client);

    protected Task<T?> GetGeneric<T>() where T : BaseChannel
        => GetGeneric<T>(Id, Client);

    public Task<bool> DeleteMessage(Client client, string messageId)
    {
        return Message.Delete(client, Id, messageId);
    }

    public Task<bool> DeleteMessage(string messageId)
        => DeleteMessage(Client, messageId);
    public Task<bool> DeleteMessage(Client client, Message message)
    {
        return Message.Delete(client, Id, message.Id);
    }
    public Task<bool> DeleteMessage(Message message)
        => DeleteMessage(Client, message);
    public async Task<Message?> GetMessage(Client client, string id)
    {
        var message = new Message
        {
            Id = id,
            ChannelId = Id
        };
        if (await message.Fetch(client))
            return message;
        return null;
    }

    public enum MessageSortDirection
    {
        Relevance,
        Latest,
        Oldest
    }

    public class BulkMessagesResponse
    {
        public LinkedList<Message> Messages { get; set; }
        public LinkedList<User>? Users { get; set; }
        public LinkedList<Member>? Members { get; set; }

        public static async Task<BulkMessagesResponse> Parse(Client client, string content)
        {
            var instance = new BulkMessagesResponse();
            var messagesJson = "[]";
            var usersJson = "[]";
            var membersJson = "[]";
            bool isJustMessages = false;
            if (content.StartsWith("["))
            {
                messagesJson = content;
                isJustMessages = true;
            }
            else if (content.StartsWith("{"))
            {
                var jojb = JObject.Parse(content);
                messagesJson = jojb["messages"].ToString();
                usersJson = jojb["users"].ToString();
                membersJson = jojb["members"].ToString();
            }
        
        
            var msgs = Message.ParseMultiple(messagesJson);
            var lmsgs = new LinkedList<Message>();
            foreach (var m in msgs)
            {
                client.AddToCache(m);
                var cm = await client.GetMessage(m.ChannelId, m.Id);
                lmsgs.AddLast(cm);
            }
            instance.Messages = lmsgs;

            if (!isJustMessages)
            {
                return instance;
            }

            var usrs = JsonSerializer.Deserialize<User[]>(usersJson, Client.SerializerOptions);
            var usrsL = new LinkedList<User>();
            foreach (var u in usrs)
            {
                client.AddToCache(u);
                var ur = await client.GetUser(u.Id, false);
                if (ur != null)
                    usrsL.AddLast(ur);
            }

            instance.Users = usrsL;

            var mbrs = JsonSerializer.Deserialize<Member[]>(membersJson, Client.SerializerOptions);
            var mbrsL = new LinkedList<Member>();
            foreach (var m in mbrs)
            {
                client.AddToCache(m);
                var mr = await client.GetMember(m.Id.ServerId, m.Id.UserId, false);
                if (mr != null)
                    mbrsL.AddLast(mr);
            }

            instance.Members = mbrsL;
        
        
            return instance;
        }
    }
    public async Task<BulkMessagesResponse> FetchMessages(
        Client client,
        ulong? limit=null,
        string? before=null,
        string?after=null,
        MessageSortDirection? sort = null,
        string?nearby = null,
        bool?includeUsers = null)
    {
        var paramDict = new Dictionary<string, object>()
        {
            {"limit", limit},
            {"before", before},
            {"after", after},
            {"sort", sort},
            {"nearby", nearby},
            {"include_users", includeUsers}
        };
        var paramList = new List<string>();
        foreach (var pair in paramDict)
        {
            if (pair.Value != null)
            {
                paramList.Add($"{pair.Key}={WebUtility.UrlEncode(pair.Value.ToString())}");
            }
        }

        var url = Client.SEndpoint.ChannelMessages(Id);
        if (paramList.Count > 0)
        {
            url += "?" + string.Join("&", paramList);
        }

        var res = await client.GetAsync(url);
        if (res.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = res.Content.ReadAsStringAsync().Result;
        var instance = await BulkMessagesResponse.Parse(client, stringContent);

        return instance;
    }

    public Task<BulkMessagesResponse> FetchMessages(
        ulong? limit = null,
        string? before = null,
        string? after = null,
        MessageSortDirection? sort = null,
        string? nearby = null,
        bool? includeUsers = null) =>
        FetchMessages(Client, limit, before, after, sort, nearby, includeUsers);
    
    public Task<Message?> SendMessage(
        Client client,
        string? content,
        Reply[]? replies,
        SendableEmbed[]? embeds,
        Masquerade? masquerade,
        Interactions[]? interactions,
        string[]? attachments)
    {
        var data = new DataMessageSend()
        {
            Content = content,
            Attachments = attachments,
            Replies = replies,
            Embeds = embeds,
            Masquerade = masquerade,
            Interactions = interactions
        };
        return SendMessage(client, data);
    }

    public Task<Message?> 
        SendMessage(
        string? content,
        Reply[]? replies,
        SendableEmbed[]? embeds,
        Masquerade? masquerade,
        Interactions[]? interactions,
        string[]? attachments)
        => SendMessage(Client, content, replies, embeds, masquerade, interactions, attachments);
    public Task<Message?> SendMessage(
        Client client,
        DataMessageSend data)
    {
        return Message.Send(client, Id, data);
    }

    public Task<Message?> SendMessage(
        Client client,
        SendableEmbed embed)
    {
        return SendMessage(
            Client, new DataMessageSend()
            {
                Content = "",
                Embeds = new []
                {
                    embed
                }
            });
    }

    public Task<Message?> SendMessage(SendableEmbed embed) => SendMessage(Client, embed);
    public Task<Message?> SendMessage(DataMessageSend data)
        => SendMessage(Client, data);

    public async Task BeginTyping(Client client)
    {
        if (client.WSClient == null)
            throw new Exception("Websocket Client not created");
        await client.WSClient.SendMessage(new TypingSendEvent(Id));
    }

    public Task BeginTyping()
        => BeginTyping(Client);

    public async Task EndTyping(Client client)
    {
        if (client.WSClient == null)
            throw new Exception("Websocket Client not created");
        await client.WSClient.SendMessage(new TypingSendEvent(Id));
    }

    public Task EndTyping()
        => EndTyping(Client);

    public async Task<ChannelInvite> CreateInvite(Client client)
    {
        var response = await client.PostAsync($"/channels/{Id}/invites", new StringContent(""));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to create invite, server responded with {response.StatusCode}");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<ChannelInvite>(stringContent, Client.SerializerOptions);
        if (data == null)
            throw new Exception("Failed to deserialize invite");
        return data;
    }
    
    public BaseChannel()
        : this(null, "")
    {}

    public BaseChannel(string id)
        : this(null, id)
    {
    }

    internal BaseChannel(Client? client, string id)
        : base(client)
    {
        Id = id;
        ChannelType = "Unknown";
    }

    protected override void ClientInit()
    {
        if (Client == null)
            return;
        Client.MessageReceived += OnMessageReceived;
    }

    public event MessageDelegate MessageReceived;

    private void OnMessageReceived(Message message)
    {
        MessageReceived?.Invoke(message);
    }
}