using InstagramClone.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace InstagramClone.Hubs
{
	public class AuthHub(UserConnectionManager connections) : Hub
	{
		private readonly UserConnectionManager _connections = connections;
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connections.DeleteConnection(Context.ConnectionId);
			Console.WriteLine("Deleted " + Context.ConnectionId);
			return Task.CompletedTask;
		}
		public void RegisterUserWithEmail(string email)
		{
			Console.WriteLine(email + " registered with id:" + Context.ConnectionId);
			_connections.AddConnection(email, Context.ConnectionId);
		}
	}
}
