using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public enum CvarValueType
	{
		NotSet,
		Bool,
		Number,
	}

	public class CVar
	{
		//public dynamic Value { get; set; }
		public readonly string Name;



		CVarFactory factory;


		public CvarValueType ValueType { get; private set; }

		public bool HasValue => ValueType != CvarValueType.NotSet;

		bool _bool = false;
		public bool Bool
		{
			get
			{
				return _bool;
			}
			set
			{
				ValueType = CvarValueType.Bool;
				if (_bool != value)
				{
					_bool = value;
					factory.Log.Equals(Name + " changed to: " + value);
					OnChanged?.Invoke(this);
				}
			}
		}
		public static implicit operator bool(CVar cvar) => cvar.Bool;

		float _number = 0;
		public float Number
		{
			get
			{
				return _number;
			}
			set
			{
				ValueType = CvarValueType.Number;
				if (_number != value)
				{
					_number = value;
					factory.Log.Equals(Name + " changed to: " + value);
					OnChanged?.Invoke(this);
				}
			}
		}

		public string ValueAsTring
		{
			get
			{
				if (ValueType == CvarValueType.Bool) return Bool.ToString();
				if (ValueType == CvarValueType.Number) return Number.ToString();
				if (ValueType == CvarValueType.NotSet) return "not set";
				return "";
			}
		}

		public static implicit operator float(CVar cvar) => cvar.Number;
		public static implicit operator int(CVar cvar) => (int)cvar.Number;

		public OpenTK.Input.Key ToogleKey { get; set; } = OpenTK.Input.Key.Unknown;

		private event Action<CVar> OnChanged;

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
			if (ValueType == CvarValueType.Bool)
			{
				Bool = !Bool;
			}
			return this;
		}

		public CVar ToogledByKey(OpenTK.Input.Key key)
		{
			factory.Log.Info($"{key} to toggle '{Name}'");
			ToogleKey = key;
			return this;
		}

	}

}
