using System;
using System.Collections.Generic;
using System.Text;
using AtmOfferService.ConnectionManagers;
using log4net.Appender;
using log4net.Core;

namespace rabbit4netConsoleApp.Appender {
	public class RabbitAppender : AppenderSkeleton {

		private RabbitMQFactory rabbitFactory;
		public string ConnectionString { get; set; }
		public string QueueName { get; set; }


		public override void ActivateOptions() {
			base.ActivateOptions();
			rabbitFactory = new RabbitMQFactory(ConnectionString);
			DeclareQueu();
		}
		protected override void Append(LoggingEvent loggingEvent) {
			Console.WriteLine(ConnectionString);
			RabbitMQConnection rabbitConnection = rabbitFactory.GetConnection();
			rabbitConnection.PublishToQueue(QueueName, loggingEvent.MessageObject);

		}

		private void DeclareQueu() {
			RabbitMQConnection rabbitConnection = rabbitFactory.GetConnection();
			rabbitConnection.QueueDeclare(QueueName, true, false, false);
		}
	}
}
