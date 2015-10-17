using System;
using TagLib;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using ExtensionMethods;
using Mono.Options;

namespace BPMPlaylist
{
	class MainClass
	{
		static Random rnd = new Random();

		public static List<T>[] Split<T>(List<T> source, int items)
		{
			int fileCount = source.Count(),
			addCounter = 0;
			List<T>[] output = new List<T>[items];

			for (int i = 0; i < items; i++) {
				output[i] = new List<T>();
				for (int j = 0; j < fileCount / items; j++) {
					output[i].Add (source [addCounter++]);
				}
			}
			return output;
		}

		public static List<FileInfo> GetMusicFiles(string directory, string searchPattern)
		{
			DirectoryInfo d = new DirectoryInfo (directory);
			FileInfo[] filesArray = d.GetFiles(searchPattern, SearchOption.AllDirectories);
			return filesArray.Cast<FileInfo>().ToList();
		}

		public static List<FileInfo> ChooseNormalizedFiles(List<FileInfo>[] fileGroups, TimeSpan timespan, int divider)
		{
			TimeSpan listDuration = new TimeSpan();
			List<FileInfo> musicList = new List<FileInfo>();
			int counter = 0;
			while (listDuration < timespan) {
				var currentPart = fileGroups[counter++ % divider];

				//If one is empty, all the other groups will be empty too
				if (currentPart.Count () == 0)
					break;
				
				int r = rnd.Next(currentPart.Count);
				FileInfo curSong = currentPart[r];
				currentPart.Remove(curSong);
				musicList.Add (curSong);
				listDuration += curSong.Duration();
			}
			return musicList;
		}

		public static List<FileInfo> SortByBMP(List<FileInfo> files)
		{
			return files.OrderBy (file => file.BeatsPerMinute()).ToList();
		}

		public static void SaveToFile(List<FileInfo> list, string outputFile)
		{
			if (Directory.Exists(outputFile) || System.IO.File.Exists(outputFile)) {
				FileAttributes attr = System.IO.File.GetAttributes(outputFile);
				if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
					outputFile = outputFile + "playlist.m3u";
			}

			using (System.IO.StreamWriter playlist = new System.IO.StreamWriter (outputFile)) {
				foreach (var file in list) {
					playlist.WriteLine (file.FullName);
				}
			}
		}

		public static void PrintPlaylist(List<FileInfo> list)
		{
			uint sum = 0;
			TimeSpan timesum = new TimeSpan();
			foreach (var file in list) {
				sum += file.BeatsPerMinute();
				timesum += file.Duration();
				Console.WriteLine ("{0,4} {1:hh\\:mm\\:ss} {2}", file.BeatsPerMinute(), file.Duration(), file.FullName);
			}
			Console.WriteLine ("{0,4} {1:hh\\:mm\\:ss}", sum, timesum);
		}

		public static List<FileInfo> RemoveLongSongs(List<FileInfo> files, TimeSpan maxLength)
		{
			return files.Where(file => file.Duration () < maxLength).ToList();
		}

		static void ShowHelp (OptionSet p)
		{
			Console.WriteLine ("Usage: bpmplaylist -d directory [OPTIONS]");
			Console.WriteLine ("Creates a pseudorandom playlist based on your music's BPM and playlist lenght.");
			Console.WriteLine ("If no options has been set, a playlist spanning 45 minutes will be created.");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			p.WriteOptionDescriptions (Console.Out);
		}

		public static void Main (string[] args)
		{
			bool showHelp = false;
			bool silent = false;
			int divider = 4;
			TimeSpan songMaxLength = new TimeSpan (0, 5, 0);
			TimeSpan timeSchedule = new TimeSpan (0, 45, 0);
			string musicDirectory = Directory.GetCurrentDirectory();
			string playlistFile	  = Directory.GetCurrentDirectory() + "playlist.m3u";

			var p = new OptionSet () {
				{ "d|directory=", "the {DIRECTORY} with your music files.\n" +
					"it is run in the current directory if none is given.",
					v => musicDirectory = v },
				{ "O|output=", "the output {FILENAME}, should be a m3u file, to work with mpd.",
					v => playlistFile = v },
				{ "t|timespan=", "the timespan of the playlist.",
					v => TimeSpan.TryParse(v, out timeSchedule) },
				{ "l|maxlength=", "the max length of chosen songs.",
					v => TimeSpan.TryParse(v, out songMaxLength) },
				{ "s|silent", "silent or quiet mode.",
					v => { if (v != null) silent = true; }  },
				{ "g|granularity=", "the granularity of the BPM groups.\n" +
					"more granularity gives less randomness.\n" +
					"this must be an integer.",
					(int v) => divider = v },
				{ "h|help",  "show this message and exit", 
					v => showHelp = v != null
				},
			};

			List<string> extra;
			try {
				extra = p.Parse (args);
			}
			catch (OptionException e) {
				Console.Write ("bpmplaylist: ");
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try `bpmplaylist --help' for more information.");
				return;
			}

			if (showHelp) {
				ShowHelp(p);
				return;
			}


			List<FileInfo> files = GetMusicFiles(musicDirectory, "*.mp3");
			files = RemoveLongSongs(files, songMaxLength);

			//Sort into groups ordered by BPM
			files = SortByBMP(files);
			List<FileInfo>[] fileGroups = Split<FileInfo>(files, divider);

			//Choose random files from the different BPM groups
			List<FileInfo> playlist = ChooseNormalizedFiles(fileGroups, timeSchedule, divider);
			playlist = SortByBMP(playlist);

			//Output
			SaveToFile(playlist, playlistFile);
			if (!silent) {
				PrintPlaylist (playlist);
			}
		}
	}
}