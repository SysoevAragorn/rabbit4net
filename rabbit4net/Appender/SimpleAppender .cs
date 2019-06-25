using System;
using System.Collections.Generic;
using System.Text;
using log4net.Appender;
using log4net.Core;

namespace SimpleLoggerWithAppender {
	public class SimpleAppender : AppenderSkeleton {
		public string ConnectionString { get; set; }
		public string QueueName { get; set; }

		protected override void Append(LoggingEvent loggingEvent) {
			Console.WriteLine(ConnectionString);
			//IList<string> data = loggingEvent.MessageObject as IList<string>;
			//if (data != null) {
			//	foreach (string s in data) {
			//		// Do something with each string
			//	}
			//}
		}
	}
}
