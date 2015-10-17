using System;
using TagLib;
using System.IO;
using System.Diagnostics;

namespace ExtensionMethods
{
	public static class MyExtensions
	{
		public static TimeSpan Duration(this FileInfo file)
		{
			return TagLib.File.Create(file.FullName).Properties.Duration;
		}

		public static uint BeatsPerMinute(this FileInfo file)
		{
			TagLib.File f = TagLib.File.Create (file.FullName);
			if (f.Tag.BeatsPerMinute != 0) {
				return f.Tag.BeatsPerMinute;
			}
			Process _bmpProcess = new Process ();
			_bmpProcess.StartInfo.UseShellExecute = false;
			_bmpProcess.StartInfo.CreateNoWindow = true;
			_bmpProcess.StartInfo.FileName = "bpm-tag";	
			_bmpProcess.StartInfo.Arguments = "\"" + file.FullName + "\"";
			_bmpProcess.Start();
			_bmpProcess.WaitForExit();

			f = TagLib.File.Create (file.FullName);
			return f.Tag.BeatsPerMinute;
		}
	}
}

