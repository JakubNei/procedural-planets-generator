using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neitri;

namespace MyEngine
{
	public class CVarFactory
	{

		Dictionary<string, CVar> nameToCVar = new Dictionary<string, CVar>();

		public IReadOnlyDictionary<string, CVar> NameToCvar => nameToCVar;

		bool doSaveChangesOnNewCvar = false;
		public readonly ILog Log;

		Func<Stream> configFileFactory;
		public CVarFactory(Func<Stream> configFileFactory, ILog log)
		{
			this.Log = log;
			this.configFileFactory = configFileFactory;
			TryReadConfig();
			doSaveChangesOnNewCvar = true;
		}

		void TryReadConfig()
		{
			var configFile = configFileFactory();
			if (!configFile.CanRead)
			{
				Log.Fatal("can not read config file");
				return;
			}


			var reader = new StreamReader(configFile, Encoding.UTF8);
			var text = reader.ReadToEnd();
			reader.Close();

			var lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			int i = 0;
			foreach (var eLine in lines)
			{
				i++;
				var line = eLine.Trim();
				if (line.StartsWith("/") || line.StartsWith("#")) continue; // comment
				var parts = line.Split(new char[] { '=' });
				if (parts.Length < 2)
				{
					Log.Warn("found badly formatted line #" + i + " = " + line);
					continue;
				}

				var name = parts[0].Trim();
				var value = parts[1].Trim();

				bool.TryParse(value, out bool typedValue);
 
				var cvar = GetCVar(name, typedValue);

				if (parts.Length > 2)
				{
					var key = parts[2].Trim();
					if (Enum.TryParse<OpenTK.Input.Key>(key, out OpenTK.Input.Key keyTyped))
						cvar.ToogledByKey(keyTyped);
				}

				Log.Info("loaded cvar: '" + cvar.ToSaveString() + "'");
			}
		}


		private void SaveNewCvar(CVar cvar)
		{
			var configFile = configFileFactory();
			if (!configFile.CanWrite)
			{
				Log.Fatal("can not save new cvar");
				return;
			}
			if (!configFile.CanSeek)
			{
				Log.Fatal("can not seek");
				return;
			}
			if (configFile.Length > 0)
				configFile.Position = configFile.Length - 1; // to the end
			var writer = new StreamWriter(configFile, Encoding.UTF8);

			var line = cvar.ToSaveString();
			writer.WriteLine();
			writer.Write(line);

			writer.Flush();
			writer.Close();

			Log.Info("saved cvar: '" + line + "'");
		}

		public CVar GetCVar(string name, bool defaultValue = false)
		{
			CVar cvar;
			if (!nameToCVar.TryGetValue(name, out cvar))
			{
				cvar = new CVar(name, this);
				cvar.Bool = defaultValue;
				nameToCVar[name] = cvar;
				if (doSaveChangesOnNewCvar) SaveNewCvar(cvar);
			}
			return cvar;
		}

		public CVar GetCVar(string name)
		{
			CVar cvar;
			if (!nameToCVar.TryGetValue(name, out cvar))
			{
				cvar = new CVar(name, this);
				nameToCVar[name] = cvar;
				if (doSaveChangesOnNewCvar) SaveNewCvar(cvar);
			}
			return cvar;
		}

	}
}
