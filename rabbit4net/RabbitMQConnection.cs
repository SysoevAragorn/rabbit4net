using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rabbit4net {
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	#region Class: RabbitMQConnection

	public class RabbitMQConnection: IDisposable
	{

		#region Constants: Private

		private static string _delayedPropertyKey = "x-delayed-type";

		private static string _delayedPropertyValue = "direct";

		private static string _exchangeDelayedTypeName = "x-delayed-message";

		#endregion

		#region Properties: Private

		private static readonly int _NetworkRecoveryInterval = 10;
		private ConnectionFactory RabbitMQConnectionFactory {
			get; set;
		}
		private IBasicProperties _rabbitProperties {
			get; set;
		}

		#endregion

		#region Properties: Public

		public IModel RabbitChannel {
			get; set;
		}

		public IConnection Connection {
			get; set;
		}

		public bool IsOpen {
			get {
				return Connection.IsOpen;
			}
		}

		#endregion

		#region Methods: Private


		/// <summary>
		/// Устанавливает время проверки соединения
		/// </summary>
		private void SetAutomaticRecovery() {
			RabbitMQConnectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(_NetworkRecoveryInterval);
			RabbitMQConnectionFactory.AutomaticRecoveryEnabled = true;
		}

		#endregion

		#region Methods: Public
		/// <summary>
		/// Открывает соединение с RabbitMQ
		/// </summary>
		public void OpenRabbitMQConnection() {
			Connection = RabbitMQConnectionFactory.CreateConnection();
			RabbitChannel = Connection.CreateModel();
			_rabbitProperties = RabbitChannel.CreateBasicProperties();
			_rabbitProperties.Persistent = true;
		}
		/// <summary>
		/// Конструктор
		/// </summary>
		public RabbitMQConnection(string connectionString) {
			RabbitMQConnectionFactory = new ConnectionFactory() {
				Uri = new Uri(connectionString)
			};
			SetAutomaticRecovery();
			OpenRabbitMQConnection();
		}

		
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="connectionFactory">Фабрика соединения</param>
		public RabbitMQConnection(ConnectionFactory connectionFactory) {
			RabbitMQConnectionFactory = connectionFactory;
			SetAutomaticRecovery();
			OpenRabbitMQConnection();
		}

		/// <summary>
		/// Возвращает EventingBasicConsumer
		/// </summary>
		/// <param name="queueName">Имя очереди</param>
		/// <param name="prefetchSize">Резервируемый размер</param>
		/// <param name="prefetchCount">Резервируемое кол-во</param>
		/// <returns>EventingBasicConsumer</returns>
		public EventingBasicConsumer CreateEventingBasicConsumer(string queueName,
					ushort prefetchSize, ushort prefetchCount) {
			RabbitChannel.BasicQos(prefetchSize, prefetchCount, false);
			var rabbitConsumer = new EventingBasicConsumer(RabbitChannel);
			RabbitChannel.QueueDeclare(queueName, true, false, false, null);
			RabbitChannel.BasicConsume(queueName, false, rabbitConsumer);
			return rabbitConsumer;
		}

		public void QueueDeclare(string queueName, bool durable, bool exclusive, bool autoDelete, bool value) {
			RabbitChannel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);
		}

		/// <summary>
		/// Публикация в очередь
		/// </summary>
		/// <param name="queueName">Имя очереди</param>
		/// <param name="data">Объект данных</param>
		public void PublishToQueue(string queueName, object data) {
			string message = JsonConvert.SerializeObject(data);
			PublishToQueue(queueName, message);
		}

		/// <summary>
		/// Публикация в очередь
		/// </summary>
		/// <param name="queueName">Имя очереди</param>
		/// <param name="message">Сереализированный объект</param>
		public void PublishToQueue(string queueName, string message) {
			byte[] body = Encoding.UTF8.GetBytes(message);
			//_rabbitProperties.Headers = new Dictionary<string, object>();
			RabbitChannel.BasicPublish("", queueName, _rabbitProperties, body);
		}

		/// <summary>
		/// Публикация в очередь
		/// </summary>
		/// <param name="queueName">Имя очереди</param>
		/// <param name="body">Сереализированный в байты объект</param>
		public void PublishToQueue(string queueName, byte[] body) {
			RabbitChannel.BasicPublish("", queueName, _rabbitProperties, body);
		}

		/// <summary>
		/// Публикация в Exchange
		/// </summary>
		/// <param name="exchangeName">Имя Exchange</param>
		/// <param name="routingKey">RoutingKey</param>
		/// <param name="message">Сереализированный объект</param>
		public void PublishToExchange(string exchangeName, string routingKey, string message) {
			byte[] body = Encoding.UTF8.GetBytes(message);
			_rabbitProperties.Headers = new Dictionary<string, object>();
			RabbitChannel.BasicPublish(exchangeName, routingKey, _rabbitProperties, body);
		}

		/// <summary>
		/// Публикация в Exchange
		/// </summary>
		/// <param name="exchangeName">Имя Exchange</param>
		/// <param name="routingKey">RoutingKey</param>
		/// <param name="data">Объект данных</param>
		public void PublishToExchange(string exchangeName, string routingKey, object data) {
			string message = JsonConvert.SerializeObject(data);
			PublishToExchange(exchangeName, routingKey, message);
		}

		/// <summary>
		/// Declare exchange and bind it to queue.
		/// </summary>
		public void DeclareDelayExchange(string exchangeName) {
			Dictionary<string, object> args = new Dictionary<string, object>();
			args.Add(_delayedPropertyKey, _delayedPropertyValue);
			RabbitChannel.ExchangeDeclare(exchangeName, _exchangeDelayedTypeName, true, false, args);
		}

		/// <summary>
		/// Publishes delayed message to queue.
		/// </summary>
		/// <param name="message">Message to send to queue.</param>
		/// <param name="delay">Delay(in milliseconds) before sending message.</param>
		/// <param name="headers">Collection of message headers.</param>
		public void PublishDelayedMessage(string message, int delay, string exchangeName, string routingKey) {
			var taskFactory = new TaskFactory();
			taskFactory.StartNew(() => {
				int msdelay = delay;
				byte[] body = Encoding.UTF8.GetBytes(message);
				_rabbitProperties.Headers = new Dictionary<string, object>();
				System.Threading.Thread.Sleep(msdelay);
				RabbitChannel.BasicPublish(exchangeName, routingKey, _rabbitProperties, body);
			});
		}

		public void Dispose() {
			Connection.Dispose();
			RabbitChannel.Close();
		}

		public void QueueDeclare(string queueName, bool durable, bool exclusive, bool autoDelete) {
			RabbitChannel.QueueDeclare(queueName, durable, exclusive, autoDelete);
		}

		public void BasicAck(ulong deliveryTag, bool v) {
			RabbitChannel.BasicAck(deliveryTag, v);
		}

		public void QueueDelete(string busEndpoint, bool ifUnused, bool ifEmpty) {
			RabbitChannel.QueueDelete(busEndpoint, ifUnused, ifEmpty);
		}



		#endregion

	}

	#endregion

}