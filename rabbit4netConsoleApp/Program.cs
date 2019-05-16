using System;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;

namespace rabbit4netConsoleApp {
	class Program {

		//protected static readonly ILog log =
	 // LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//static Program() {
		//	XmlConfigurator.Configure();
		//}
		static void Main(string[] args) {
			var log4netConfig = new XmlDocument();
			log4netConfig.Load(File.OpenRead("log4net.config"));
			var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
					   typeof(log4net.Repository.Hierarchy.Hierarchy));
			XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
			ILog log = LogManager.GetLogger(typeof(Program));
			log.Debug("hello world");
		}
	}
}
