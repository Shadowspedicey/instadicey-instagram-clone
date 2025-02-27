using System.Collections.Concurrent;

namespace InstagramClone.Services
{
	public class UserConnectionManager
	{
		private readonly ConcurrentDictionary<string, string> _connections = new();

		public void AddConnection(string email, string connectionID) => _connections.TryAdd(email, connectionID);
		public void DeleteConnection(string connectionID)
		{
			var pair = _connections.FirstOrDefault(p => p.Value == connectionID);
			_connections.TryRemove(pair);
		}
		public string? GetConnection(string email)
		{
			_connections.TryGetValue(email, out string? connectionID);
			return connectionID;
		}
	}
}
