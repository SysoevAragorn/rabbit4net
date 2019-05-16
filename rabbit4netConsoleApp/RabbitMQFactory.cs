using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AtmOfferService.ConnectionManagers;

namespace rabbit4netConsoleApp {
	public class RabbitMQFactory {
		private static readonly object Lock = new object();
	
		private readonly RabbitMQConnection _connection;
		private double _failedConnectionRetryTimeoutInSeconds = 10;

		public RabbitMQFactory(string connectionString) {
			_connection = new RabbitMQConnection(connectionString);
		}

		public RabbitMQConnection GetConnection() {
			InitializeConnection();
			return _connection;
		}

		private void InitializeConnection() {
			if (_connection.IsOpen) {
				return;
			}

			lock (Lock) {
				try {
					OpenConnection();

					if (!_connection.IsOpen) {
						Thread.Sleep(TimeSpan.FromSeconds(_failedConnectionRetryTimeoutInSeconds));
						OpenConnection();
					}
				} catch {
					// Nothing to do if this fails
				}
			}
		}

		private void OpenConnection() {
			if (!_connection.IsOpen) {
				_connection.OpenRabbitMQConnection();
			}
		}
	}
}
