using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MikuBot.Commands;
using MikuBot.Modules;

namespace MikuBot.ExtraPlugins
{
	public class SysInfo : MsgCommandModuleBase
	{
		private string CollapseWhitespace(string str)
		{
			var res = Regex.Replace(str, @"\s{2,}", " ");

			return res;
		}

#if OS_WINDOWS
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private class MEMORYSTATUSEX
		{
			public uint dwLength;
			public uint dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;

			public MEMORYSTATUSEX()
			{
				this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
			}
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool GlobalMemoryStatusEx([In, Out]MEMORYSTATUSEX lpBuffer);
#endif

		private string MemoryString
		{
			get
			{
				string freeRam = "unknown", totalRam = freeRam;
#if OS_WINDOWS
				var memStatus = new MEMORYSTATUSEX();
				if (GlobalMemoryStatusEx(memStatus))
				{
					totalRam = (memStatus.ullTotalPhys / 1024 / 1024).ToString();
					freeRam = (memStatus.ullAvailPhys / 1024 / 1024).ToString();
				}
#elif OS_LINUX
				try
				{
					using (var sr = new StreamReader("/proc/meminfo"))
					{
						string? line;
						line = sr.ReadLine();
						while (line != null)
						{
							var fields = line.Split(':');
							var item = fields[0];
							switch (item)
							{
							case "MemTotal":
								totalRam = (ulong.Parse(fields[1].Trim().Split(' ')[0]) / 1024).ToString();
								break;
							case "MemFree":
								freeRam = (ulong.Parse(fields[1].Trim().Split(' ')[0]) / 1024).ToString();
								break;
							}
							line = sr.ReadLine();
						}
					}
				} catch (Exception e) when (e is FileNotFoundException || e is IOException)
				{}
#endif
				return "RAM: " + freeRam + "MB free, " + totalRam + " MB total";
			}
		}

		private string ProcString
		{
			get
			{
				var procCount = Environment.ProcessorCount;

				var procInfo = "39000";

				return procCount + "x " + procInfo;
			}
		}

		public override int CooldownChannelMs
		{
			get { return 10000; }
		}

		public override int CooldownUserMs
		{
			get { return 30000; }
		}

		public override string CommandDescription
		{
			get { return "Displays information about the environement this bot is running in."; }
		}

		public override string Name
		{
			get { return "SysInfo"; }
		}

		public override void HandleCommand(MsgCommand chat, IBotContext bot)
		{
			if (!CheckCall(chat, bot))
				return;

			var os = Environment.OSVersion.ToString();
			var clrVer = Environment.Version.ToString();
			var arch = Environment.Is64BitOperatingSystem ? "x86-64" : "x86";

			bot.Writer.Msg(chat.ChannelOrSenderNick,
				os + " " + arch
				+ " | .NET Core " + clrVer
				+ " | " + ProcString
				+ " | " + MemoryString);
		}
	}
}
