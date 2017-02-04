using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Zero
{
	public static class Wrapper
	{
		private static Dictionary<string, string> _Operators = new Dictionary<string, string>()
			{
				{ "op_Equality", "equals" },
				{ "op_Addition", "add" },
				{ "op_Subtraction", "subtract" },
				{ "op_LessThan", "lessthan" },
				{ "op_GreaterThan", "greaterthan" },
				{ "op_Multiply", "multiply" },
				{ "op_Division", "divide" },
				{ "op_Modulus", "modulate" },
				{ "op_ExclusiveOr", "exponentiate" },
				{ "op_LogicalAnd", "and" },
				{ "op_LogicalOr", "or" }
			};

		public static Comp WrapDll(string _path) { return WrapAssembly(Assembly.LoadFile(System.IO.Path.GetFullPath(_path))); }

		public static Comp WrapAssembly(Assembly _assembly) { return new Comp(_assembly.GetTypes().Select(item => new Variable(item.Name, WrapType(item), _external: false)).ToArray()); }

		public static Comp WrapType(Type _type)
		{
			Comp @out = new Comp() { WrappedItem = _type };

			foreach (ConstructorInfo c in _type.GetConstructors())
				//@out.AddVariable(new Variable("new" + string.Join("", c.GetParameters().Select(item => item.Name.StartsWith("_") ? item.Name : "_" + item.Name)), c.GetParameters().Length, ps => WrapObject(c.Invoke(ToParams(c.GetParameters(), ps)))));
				@out.AddVariable(new Variable(GetName(c), c.GetParameters().Length, (s, ps) => WrapObject(c.Invoke(ToParams(c.GetParameters(), ps)), _onlyzero: false)));

			foreach (FieldInfo f in _type.GetFields().Where(item => item.IsStatic))
				@out.AddVariable(new Variable(GetName(f), () => Convert.ToZero(f.GetValue(null), _allowwrap: true), obj => f.SetValue(null, Convert.FromZeroTo(f.FieldType, obj)), _external: false));
			foreach (PropertyInfo p in _type.GetProperties(BindingFlags.Static | BindingFlags.Public))
				@out.AddVariable(new Variable(GetName(p), () => Convert.ToZero(p.GetValue(null, null), _allowwrap: true), obj => p.SetValue(null, Convert.FromZeroTo(p.PropertyType, obj))));
			foreach (MethodInfo m in _type.GetMethods().Where(item => item.IsStatic && !Wrapper._Operators.ContainsKey(item.Name)))
				@out.AddVariable(new Variable(GetName(m), m.GetParameters().Length, (s, ps) => Convert.ToZero(m.Invoke(null, ToParams(m.GetParameters(), ps)), _allowwrap: true)));

			return @out;
		}

		public static object[] ToParams(ParameterInfo[] _infos, object[] _params)
		{
			List<object> @out = new List<object>();
			for (int i = 0; i < _params.Length; i++)
				@out.Add(Convert.FromZeroTo(_infos[i].ParameterType, _params[i]));
			return @out.ToArray();
		}

		public static string GetName(MemberInfo _info)
		{
			NameAttribute[] nas = _info.GetCustomAttributes<NameAttribute>().ToArray();
			if (nas.Length > 0)
				return nas[0].Name.ToLower();
			else
			{
				if (_info is ConstructorInfo)
					return "new" + string.Join("", ((ConstructorInfo)_info).GetParameters().Select(item => (item.Name.StartsWith("_") ? "" : "_") + item.Name.ToLower()));
				else
				{
					string name = _info.Name.ToLower();
					if (_info is MethodInfo)
					{
						
						//if (ops.ContainsKey(_info.Name))
						//	return ops[_info.Name];
						//else
							return name + string.Join("", ((MethodInfo)_info).GetParameters().Select(item => (item.Name.StartsWith("_") ? "" : "_") + item.Name.ToLower()));
					}
					else
						return name;
				}
			}
		}

		public static Comp WrapObject(object _object, bool _onlyzero/*, Action<object> _onchange*/)
		{
			Type t = _object.GetType();
			Comp @out = new Comp() { WrappedItem = _object };

			foreach (FieldInfo f in t.GetFields().Where(item => _onlyzero ? item.GetCustomAttributes<ZeroAttribute>().Any() : true))
			{
				Action<object> set = obj => f.SetValue(_object, Convert.FromZeroTo(f.FieldType, obj));
				Func<object> get = () => Convert.ToZero(f.GetValue(_object), _allowwrap: true/*, _onchange: f.FieldType.IsValueType ? set : null*/);

				//if (f.FieldType.IsValueType)
				//	@out.AddVariable(new Variable(GetName(f), get, set, _external: false));
				//else
				@out.AddVariable(new Variable(GetName(f), get, set, _external: false));
			}
			foreach (PropertyInfo p in t.GetProperties().Where(item => _onlyzero ? item.GetCustomAttributes<ZeroAttribute>().Any() : true))
			{
				if (p.GetIndexParameters().Length > 0)
				{
					int ix = p.GetIndexParameters().Length;
					@out.AddVariable(new Variable("GetIndex", ix, (s, ps) =>
						{
							List<object> indexes = new List<object>();
							for (int i = 0; i < ps.Length; i++)
								indexes.Add(Convert.FromZeroTo(p.GetIndexParameters()[i].ParameterType, ps[i]));
							return Convert.ToZero(p.GetValue(_object, indexes.ToArray()), _allowwrap: true);
						}));
				}
				else
					@out.AddVariable(new Variable(GetName(p), () => Convert.ToZero(p.GetValue(_object), _allowwrap: true), obj => p.SetValue(_object, Convert.FromZeroTo(p.PropertyType, obj)), _external: false));
			}
			foreach (MethodInfo m in t.GetMethods().Where(item => _onlyzero ? item.GetCustomAttributes<ZeroAttribute>().Any() : true))
			{
				if (!Wrapper._Operators.ContainsKey(m.Name))
					@out.AddVariable(new Variable(GetName(m), m.GetParameters().Length, (s, ps) => Convert.ToZero(m.Invoke(_object, ToParams(m.GetParameters(), ps)), _allowwrap: true)));
				else
					@out.AddVariable(new Variable(Wrapper._Operators[m.Name], 1, (s, ps) => Convert.ToZero(m.Invoke(null, new object[] { _object, Convert.FromZeroTo(m.GetParameters()[1].ParameterType, ps[0]) }), _allowwrap: true)));
				//@out.AddVariable(new Variable(m.Name + string.Join("", m.GetParameters().Select(item => item.Name.StartsWith("_") ? item.Name : "_" + item.Name)), m.GetParameters().Length, ps => Convert.ToZero(m.Invoke(_object, ToParams(m.GetParameters(), ps)))));

			}
			if (!t.IsValueType)
				@out.AddVariable(new Variable("equals", 1, (s, ps) => (_object == ((Comp)ps[0]).WrappedItem) ? 1.0 : 0.0));

			return @out;
		}
	}
}