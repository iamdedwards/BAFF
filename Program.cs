
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Music.Midi;

namespace ManagedMidiRecorder
{
    class MainClass
    {

        public static void Play()
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "/usr/bin/aplay";
            process.StartInfo.Arguments = "/home/saradan/ohdear/test.wav";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (sender, data) =>
            {
                Console.WriteLine(data.Data);
            };
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (sender, data) =>
            {
                Console.WriteLine(data.Data);
            };
            process.Start();

        }

		public static Dictionary<string, string> info =new Dictionary<string, string>();
		public static int row = 0;
		static bool exit = false;
		public static void WriteCode(string code) {
			Console.WriteLine("KEY:");
			var key = Console.ReadLine();
			Console.WriteLine("key:" + key);
			exit = (key == "exit");
			info[code] = key;
		}
        public static void Main(string[] args)
        {
            string port = null;
            string outfile = null;
            foreach (var arg in args)
            {
                if (arg.StartsWith("--port:"))
                    port = arg.Substring("--port:".Length);
                else
                    outfile = arg;
            }
            Stream outStream = outfile != null ? File.OpenWrite(outfile) : null;

            var access = MidiAccessManager.Default;
            foreach (var i in access.Inputs)
                Console.WriteLine(i.Id + " : " + i.Name);
            if (!access.Inputs.Any())
            {
                Console.WriteLine("No input device found.");
                return;
            }
            var iport = access.Inputs.FirstOrDefault(i => i.Id == port) ?? access.Inputs.ElementAt(1);
            var input = access.OpenInputAsync(iport.Id).Result;
            Console.WriteLine(input.Details.Name);
            Console.WriteLine(input.Connection);
            Console.WriteLine("Using " + iport.Id);
            input.MessageReceived += (obj, e) =>
            {
                var what = string.Join("-", e.Data.Take(e.Length).Select(x => x.ToString("X")));
                Console.WriteLine($" {what} ");
                if (e.Data.ElementAt(2).ToString("X") == "0")
                {
					WriteCode(what);
                }
                if (outStream != null)
                    outStream.Write(e.Data, e.Start, e.Length);
            };
            Console.WriteLine("Type [CR] to quit...");
			while(exit == false){
				
			}
            input.CloseAsync();
            if (outStream != null)
                outStream.Close();
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
			Console.WriteLine(json);
		}
    }
}