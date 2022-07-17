using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MikuBot.Services;

namespace MikuBot
{
	class Program
	{
		private static readonly string exnErr = "Unhandled exception";
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
		private static Bot bot;

		public static Bot Bot
		{
			get { return bot; }
		}

		static void Main(string[] args)
		{
			Thread.CurrentThread.Name = "Boot";
			log4net.Config.XmlConfigurator.Configure();

			log.Info("Running in standalone mode.");
			RunBot();
		}

		public static void RunBot()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
			TaskScheduler.UnobservedTaskException += UnobservedTaskException;
			if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
				Thread.CurrentThread.Name = "Main";

			var config = new Config();

			try
			{
				Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(config.Culture);
			}
			catch (ArgumentException x)
			{
				log.Error("Unable to set culture", x);
			}

			bot = new Bot(config);
			bot.Run();
		}

		private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exn = new Exception(e.ExceptionObject.ToString());
			log.Fatal(exnErr, exn);
		}

		private static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			log.Error(exnErr, e.Exception);
		}
	}
}
