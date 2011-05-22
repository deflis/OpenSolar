using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xaml;
using Ignition;
using Ionic.Zip;

namespace Solar.Models
{
#pragma warning disable 1591
	
	public class UpdateInfo
	{
		const string UpdateInfoUri = "http://star2.glasscore.net/Content/Tools/Solar/Update/update.xaml";

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Version Version
		{
			get;
			set;
		}

		public string VersionString
		{
			get
			{
				return this.Version.ToString();
			}
			set
			{
				this.Version = new Version(value);
			}
		}

		public string Description
		{
			get;
			set;
		}

		public DateTime DateTime
		{
			get;
			set;
		}

		public Uri PackageUri
		{
			get;
			set;
		}

		public UpdateInfo()
		{
			this.Version = App.AssemblyVersion;
			this.Description = @"";
			this.DateTime = DateTime.Now;
			this.PackageUri = new Uri(string.Format("http://star2.glasscore.net/Content/Tools/Solar/Download/solar{0}{1}.zip", this.Version.Major, this.Version.Minor));
		}

		public static UpdateInfo Load()
		{
			using (var wc = new WebClient())
			using (var ns = wc.OpenRead(UpdateInfoUri))
				return (UpdateInfo)XamlServices.Load(ns);
		}

		public bool Update(Func<UpdateInfo, bool> confirm)
		{
			if (this.Version <= App.AssemblyVersion)
				return false;

			var tmp = Path.GetTempFileName();
			var bat = Path.GetTempFileName();

			if (File.Exists(tmp))
				File.Delete(tmp);

			if (File.Exists(bat))
				File.Delete(bat);

			Directory.CreateDirectory(tmp);
			bat = Path.ChangeExtension(bat, ".bat");

			using (var wc = new WebClient())
			using (var ns = wc.OpenRead(this.PackageUri))
			using (var ms = ns.Freeze())
			using (var zip = ZipFile.Read(ms))
				zip.ExtractAll(tmp, ExtractExistingFileAction.OverwriteSilently);

			File.WriteAllLines(bat, new[]
			{
				"@echo off",
				"ping localhost -n 2 > nul",
				string.Format("xcopy /c /s /y \"{0}\\*\" \"{1}\"", tmp, App.StartupPath),
				string.Format("start \"\" \"{0}\"", App.ExecutablePath),
				string.Format("rmdir /q /s \"{0}\"", tmp),
				string.Format("del \"{0}\"", bat),
			}, Encoding.GetEncoding(932));

			AppDomain.CurrentDomain.ProcessExit += (sender, e) => Process.Start(new ProcessStartInfo(Environment.GetEnvironmentVariable("comspec"), "/c \"" + bat + "\"")
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = App.StartupPath,
			});

			if ((bool)App.Current.Dispatcher.Invoke(confirm, this))
				App.Current.Dispatcher.Invoke((Action)App.Current.Shutdown);

			return true;
		}

		public void Save()
		{
			XamlServices.Save(Path.Combine(App.StartupPath, "update.xaml"), this);
		}
	}

#pragma warning restore 1591
}
