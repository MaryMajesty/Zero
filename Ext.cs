using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public static class Ext
	{
		public static string DecimalDigits = "0123456789";
		public static string Digits = "0123456789abcdefghijklmnopqrstuvwxyz";



		public static object GetValueNotVariable(this object @this)
		{
			if (@this is Variable)
			{
				Variable v = (Variable)@this;
				if (!v.Exists)
					throw Error.Semantics.VariableNotExistent(v);

				return v.GetValue();
			}
			else
				return @this;
		}



		public static bool IsFunction(this string @this) { return @this[0] == '$' || (@this.ToLower().ToList().TrueForAll(item => "abcdefghijklmnopqrstuvwxyz_0123456789".Contains(item)) && !"0123456789".Contains(@this[0])); }

		public static bool IsDouble(this string @this)
		{
			@this = @this.ToLower();

			string @base = "10";
			string @int = "0";
			string @double = "0";

			if (@this.Length > 0 && @this[0] == '-')
				@this = @this.Substring(1);

			if (@this.Contains('_'))
			{
				@base = @this.Substring(0, @this.IndexOf('_'));
				@this = @this.Substring(@this.IndexOf('_') + 1);
			}

			if (@this.Contains('.'))
			{
				@int = @this.Substring(0, @this.IndexOf('.'));
				@double = @this.Substring(@this.IndexOf('.') + 1);
			}
			else
				@int = @this;

			if (@base.Length > 0 && @base.ToList().TrueForAll(item => DecimalDigits.Contains(item)))
			{
				int b = int.Parse(@base);

				return b >= 2 && b <= Digits.Length
					&& @int.ToList().TrueForAll(item => Digits.Contains(item)) && @int.ToList().TrueForAll(item => Digits.IndexOf(item) < b)
					&& @double.Length > 0 && @double.ToList().TrueForAll(item => Digits.Contains(item)) && @double.ToList().TrueForAll(item => Digits.IndexOf(item) < b)
					&& (@int.Length > 0 || @double.Length > 0);
			}
			else
				return false;
		}

		public static bool IsExpression(this string @this) { return @this.IsFunction() || @this.IsDouble(); }

		public static bool IsPrimitive(this object @this) { return @this is double || @this is List || @this is Lambda || @this is Wave; }



		public static string GetPrimitiveName(this object @this)
		{
			if (@this is double)
				return "num";
			else if (@this is List)
				return "list";
			else if (@this is Lambda)
				return "func";
			else if (@this is Wave)
				return "wave";
			else
				throw new Exception("The type " + @this.GetType().Name + " is not a primitive.");
		}



		public static double AsDouble(this object @this)
		{
			if (@this is bool)
				return (bool)@this ? 1 : 0;
			else if (@this is char)
				return (double)(ushort)((char)@this);
			else
				throw new Exception("akfdgaigf");
		}

		public static int AsInt(this object @this) { return (int)Math.Floor((double)@this); }

		public static bool AsBool(this object @this)
		{
			double v = (double)@this;
			if (v == 1)
				return true;
			else if (v == 0)
				return false;
			else
				throw Error.Semantics.NotValidBoolean;
		}

		public static char AsChar(this object @this) { return (char)((ushort)((double)@this)); }

		//public static string AsString(this object @this) { return string.Join("", ((List)@this).Select(item => item.AsChar()).ToArray()); }

		
		
		public static double DoubleFromBase(string _text, int _base, bool _isdecimal = false)
		{
			_text = _text.ToLower();

			double @out = 0;
			for (int i = 0; i < _text.Length; i++)
				@out += Digits.IndexOf(_text[_text.Length - i - 1]) * Math.Pow(_base, i);

			if (_isdecimal)
				return @out / Math.Pow(_base, _text.Length);

			return @out;
		}

		public static double StringToDouble(string _string)
		{
			if (_string.IsDouble())
			{
				int @base = 10;
				int sign = 1;

				if (_string[0] == '-')
				{
					sign = -1;
					_string = _string.Substring(1);
				}

				if (_string.Contains('_'))
				{
					@base = int.Parse(_string.Substring(0, _string.IndexOf('_')));
					_string = _string.Substring(_string.IndexOf('_') + 1);
				}

				if (!_string.Contains('.'))
					return DoubleFromBase(_string, @base) * sign;
				else
					return DoubleFromBase(_string.Substring(0, _string.IndexOf('.')), @base) + DoubleFromBase(_string.Substring(_string.IndexOf('.') + 1), @base, _isdecimal: true) * sign;
			}
			else
				throw Error.Semantics.NotParseable(_string);
		}



		public static string ToString(this object @this, bool _getstrings, int _layers)
		{
			if (@this is List)
			{
				if (_getstrings && ((List)@this).MightBeString)
					return '"' + ((List)@this).ToLiteral() + '"';
				else if (_layers > 0)
					return "[" + string.Join(", ", ((List)@this).Objects.Select(item => Ext.ToString(item, _getstrings, _layers - 1))) + "]";
				else
					return "[ {red" + ((List)@this).Count.ToString() + "} ]";
			}
			else if (@this is double)
				return @this.ToString();
			else if (@this is Zero.Lambda)
				return "{func}";
			else if (@this is Comp)
			{
				if (((Comp)@this)._Variables.Count == 0)
					return "{ }";
				else if (_layers > 0)
					return "{ " + string.Join(", ", ((Comp)@this)._Variables.Select(item => item.Name + " = " + ToString(item.GetValue(), _getstrings, _layers - 1))) + " }";
				else
					return "{ {red" + ((Comp)@this)._Variables.Count.ToString() + "} }";
			}
			else if (@this is Variable)
				return ToString(((Variable)@this).Value, _getstrings, _layers);
			else if (@this == null)
				return "{null}";
			else
				throw new Exception("Unknown type.");
			//else
			//	return "{ext: " + @this.GetType().ToString() + "}";
		}

		public static void MakeExternal(this Variable @this)
		{
			object v = @this.Value;
			if (v is Comp)
			{
				foreach (Variable var in ((Comp)v)._Variables)
					var.MakeExternal();
			}
			else if (v is ZFunc)
				((ZFunc)v).External = true;
		}
	}
}