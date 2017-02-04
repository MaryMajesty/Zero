using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public static class Convert
	{
		//public static string ToString(object _object)
		//{
		//	if (_object is object[])
		//		return string.Join("", ((object[])_object).Select(item => item.AsChar()));
		//	else
		//		return _object.ToString();
		//}

		public static T To<T>(object _object) { return (T)FromZeroTo(typeof(T), _object); }

		public static object FromZeroTo(Type _type, object _object)
		{
			if (_object is Null)
				return null;
			else if (_type == typeof(string))
				return ((List)_object).ToLiteral();
			else if (_type == typeof(bool))
				return _object.AsBool();
			else if (_type == typeof(char))
				return _object.AsChar();
			else if (_type == typeof(byte))
				return (byte)((double)_object);
			else if (_type == typeof(int))
				return (int)((double)_object);
			else if (_type.IsArray)
			{
				List l = (List)_object;
				Array a = Array.CreateInstance(_type.GetElementType(), l.Count);
				for (int i = 0; i < l.Count; i++)
					a.SetValue(Convert.FromZeroTo(_type.GetElementType(), l[i]), i);
				return a;
			}
			else if (_type.IsGenericType)
			{
				List l = (List)_object;
				Type a = _type.GetGenericArguments()[0];
				Type t = typeof(List<>).MakeGenericType(a);
				object @out = Activator.CreateInstance(t);
				foreach (object o in l.Objects)
					t.GetMethod("Add").Invoke(@out, new object[] { Convert.FromZeroTo(a, o) });
				return @out;
			}
			else if (_type == typeof(List))
				return _object;
			else
				return Convert.ToNet(_object);
		}

		public static object ToNet(object @this)
		{
			//if (@this is List)
			//{
			//	//List l = (List)@this;
			//	return ((List)@this).ToArray();

			//	//if (l.IsString)
			//	//	return string.Join("", l.Objects);
			//	//else
			//	//	return l.Select(item => Convert.ToNet(item)).ToArray();
			//	//if (l.Objects.TrueForAll(item => item is char))
			//	//	return string.Join("", l.Objects);
			//	//else
			//	//	return l.Select(item => Convert.ToNet(item)).ToArray();
			//}
			/*else */
			if (@this is Comp)
			{
				Comp c = (Comp)@this;
				if (c.WrappedItem != null)
					return c.WrappedItem;
				else
				{
					Dictionary<string, object> @out = new Dictionary<string, object>();
					foreach (Variable v in c._Variables)
						@out[v.Name] = v.GetValue();
					return @out;
				}
			}
			else if (@this is Lambda)
				return (Func<object[], object>)(ps => Convert.ToNet(((Lambda)@this).Execute(List.From(ps), null)));
			else if (@this is Null)
				return null;
			else
				return @this;
		}

		public static object ToZero(object @this, bool _allowwrap = false/*, Action<object> _onchange = null*/)
		{
			if (@this == null)
				return null;
			else if (@this.GetType().IsPrimitive)
			{
				if (@this is double)
					return @this;
				else if (@this is bool)
					return (bool)@this ? 1.0 : 0.0;
				else if (@this is int)
					return (double)((int)@this);
				else if (@this is byte)
					return (double)((byte)@this);
				else if (@this is char)
					return (double)((ushort)((char)@this));
				else
					throw new Exception("Type " + @this.GetType().Name + " can't be converted.");
			}
			else
			{
				if (@this is string)
					return List.From((string)@this);
				else if (@this is List)
					return @this;
				else if (@this is Comp)
					return @this;
				else if (@this is Dictionary<string, object>)
					return new Comp() { _Variables = ((Dictionary<string, object>)@this).ToArray().Select(item => new Variable(item.Key, Convert.ToZero(item.Value), _external: false)).ToList() };
				else if (_allowwrap)
					return Wrapper.WrapObject(@this, _onlyzero: false/*, _onchange: _onchange*/);
				else
					throw Error.Permission.PermissionDenied(Permission.DllUsage);
			}
		}
	}
}