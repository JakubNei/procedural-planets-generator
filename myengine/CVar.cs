using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{

	public class CVar
	{
		//public dynamic Value { get; set; }
		public string name;

		bool _bool;

		Debug debug;

		public bool Bool
		{
			get
			{
				return _bool;
			}
			set
			{
				if (_bool != value)
				{
					_bool = value;
					OnChanged?.Invoke(this);
				}
			}
		}


		public OpenTK.Input.Key toogleKey = OpenTK.Input.Key.Unknown;
		public bool hasDefaultValue = false;

		public event Action<CVar> OnChanged;

		public CVar(Debug debug)
		{
			this.debug = debug;
		}

		public bool EatBoolIfTrue()
		{
			if(Bool) {
				Bool = false;
				return true;
			}
			return false;
		}

		public CVar ToogledByKey(OpenTK.Input.Key key)
		{
			debug.Info($"{key} to toggled {name}");
			toogleKey = key;
			return this;
		}

		public static implicit operator bool(CVar cvar) => cvar.Bool;
	}

}
