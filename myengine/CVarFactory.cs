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

		bool doSaveNewCvars = false;
		public readonly ILog Log;


		struct LineHolder
		{
			public string dataPart;
			public string commentPart;
			public CVar associatedCvar;
		}
		List<LineHolder> lines = new List<LineHolder>();


		Func<Stream> configFileFactory;
		public CVarFactory(Func<Stream> configFileFactory, ILog log)
		{
			this.Log = log;
			this.configFileFactory = configFileFactory;
			TryReadConfig();
			doSaveNewCvars = true;
		}

		void TryReadConfig()
		{
			var configFile = configFileFactory();
			if (!configFile.CanRead)
			{
				Log.Fatal("can not read config file");
				return;
			}

			configFile.Position = 0;
			var reader = new StreamReader(configFile, Encoding.UTF8);
			var allText = reader.ReadToEnd();
			reader.Close();

			var textLines = allText.Split(new char[] { '\r' });

			int lineNumber = 0;
			foreach (var _line in textLines)
			{
				lineNumber++;

				CVar cvar = null;
				string dataPart = _line.Trim();
				string commentPart = "";

				var commentIndex = dataPart.IndexOfAny(new char[] { '#' }); // ab#c // # is comment
				if (commentIndex != -1) // 2
				{
					commentPart = dataPart.Trim().Substring(commentIndex); // #c
					dataPart = dataPart.Trim().Substring(0, commentIndex); // ab
				}

				if (dataPart.IsNullOrEmptyOrWhiteSpace() == false)
				{
					var dataParts = dataPart.Split(new char[] { '=' });
					if (dataParts.Length < 2)
					{
						Log.Warn("found badly formatted data on line " + lineNumber + " '" + dataPart + "'");
						continue;
					}

					var name = dataParts[0].Trim();
					cvar = GetCVar(name);

                    bool typedBoolValue;
                    float typedFloatValue;
                    OpenTK.Input.Key keyTyped;

                    var value = dataParts[1].Trim();
					if (value == "not set")
					{

					}
					else if (bool.TryParse(value, out typedBoolValue))
					{
						cvar.Bool = typedBoolValue;
					}
					else if (float.TryParse(value, out typedFloatValue))
					{
						cvar.Number = typedFloatValue;
					}

					if (dataParts.Length > 2)
					{
						var toggleKey = dataParts[2].Trim();
						if (Enum.TryParse<OpenTK.Input.Key>(toggleKey, true, out keyTyped))
							cvar.ToogledByKey(keyTyped);
						else
							Log.Warn("invalid toggle key for cvar: " + toggleKey);
					}

					Log.Info("loaded cvar: '" + ToSaveString(cvar) + "'");
				}

				lines.Add(new LineHolder() { associatedCvar = cvar, commentPart = commentPart, dataPart = dataPart });
			}

			SaveData();
		}
		private string ToSaveString(CVar cvar)
		{
			var s = cvar.Name + " = ";

			if (cvar.ValueType == CvarValueType.NotSet) s += "not set";
			else if (cvar.ValueType == CvarValueType.Bool) s += cvar.Bool.ToString().ToLower();
			else if (cvar.ValueType == CvarValueType.Number) s += cvar.Number.ToString().ToLower();

			if (cvar.ToogleKey != OpenTK.Input.Key.Unknown) s += " = " + cvar.ToogleKey.ToString().ToLower();

			return s;
		}


		private void SaveData()
		{
			var configFile = configFileFactory();
			if (!configFile.CanSeek)
			{
				Log.Fatal("can not seek config file");
				return;
			}
			if (!configFile.CanWrite)
			{
				Log.Fatal("can not write to config file");
				return;
			}

			configFile.Position = 0;
			var w = new StreamWriter(configFile, Encoding.UTF8);

			var isFirst = true;
			foreach (var line in lines)
			{
				if (isFirst == false) w.WriteLine();

				var l = "";
				if (line.associatedCvar != null) l += ToSaveString(line.associatedCvar) + " ";
				l += line.commentPart;
				w.Write(l);

				isFirst = false;
			}
			w.Flush();

			// clear the rest of the file, we cant actually decrease its size it seems
			if (w.BaseStream.Length > w.BaseStream.Position)
			{
				var c = w.BaseStream.Length - w.BaseStream.Position;
				while (c-- > 0)
					w.Write(" ");
				w.Flush();
			}
			w.Close();
		}

		private void SaveNewCvar(CVar cvar)
		{
			lines.Add(new LineHolder()
			{
				dataPart = ToSaveString(cvar),
				associatedCvar = cvar,
				commentPart = "",
			});
			SaveData();
		}

		public CVar GetCVar(string name)
		{
			CVar cvar;
			if (!nameToCVar.TryGetValue(name, out cvar))
			{
				cvar = new CVar(name, this);
				nameToCVar[name] = cvar;
				if (doSaveNewCvars) SaveNewCvar(cvar);
			}
			return cvar;
		}
		public CVar GetCVar(string name, bool defaultValue = false)
		{
			CVar cvar;
			if (!nameToCVar.TryGetValue(name, out cvar))
			{
				cvar = new CVar(name, this);
				cvar.Bool = defaultValue;
				nameToCVar[name] = cvar;
				if (doSaveNewCvars) SaveNewCvar(cvar);
			}
			return cvar;
		}
		public CVar GetCVar(string name, float defaultValue = 0)
		{
			CVar cvar;
			if (!nameToCVar.TryGetValue(name, out cvar))
			{
				cvar = new CVar(name, this);
				cvar.Number = defaultValue;
				nameToCVar[name] = cvar;
				if (doSaveNewCvars) SaveNewCvar(cvar);
			}
			return cvar;
		}



	}
}
