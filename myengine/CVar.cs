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
		public readonly string Name;

		bool _bool;

		CVarFactory factory;

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
					factory.Log.Equals(Name + " changed to: " + value);
					OnChanged?.Invoke(this);
				}
			}
		}


		public OpenTK.Input.Key toogleKey = OpenTK.Input.Key.Unknown;

		public event Action<CVar> OnChanged;

		public CVar(string name, CVarFactory factory)
		{
			this.Name = name;
			this.factory = factory;
		}

		public CVar OnChangedAndNow(Action<CVar> action)
		{
			if (action != null)
			{
				OnChanged += action;
				action.Invoke(this);
			}
			return this;
		}


		public bool EatBoolIfTrue()
		{
			if (Bool)
			{
				Bool = false;
				return true;
			}
			return false;
		}

		public CVar Toogle()
		{
			Bool = !Bool;
			return this;
		}

		public CVar ToogledByKey(OpenTK.Input.Key key)
		{
			factory.Log.Info($"{key} to toggle {Name}");
			toogleKey = key;
			return this;
		}

		public static implicit operator bool(CVar cvar) => cvar.Bool;
	}

}
