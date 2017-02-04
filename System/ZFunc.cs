using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class ZFunc
	{
		public bool External;
		public int Parameters;
		public Func<Scope, object[], object> Func;

		public ZFunc(int _parameters, Func<Scope, object[], object> _func)
		{
			this.Parameters = _parameters;
			this.Func = _func;
		}

		public ZFunc(int _parameters, Func<object[], object> _func) : this(_parameters, (s, ps) => _func(ps)) { }

		public ZFunc(int _parameters, Func<object> _func) : this(_parameters, (s, ps) => _func()) { }

		public ZFunc(int _parameters, Action<Scope, object[]> _action)
		{
			this.Parameters = _parameters;
			this.Func = (s, ps) =>
				{
					_action(s, ps);
					return null;
				};
		}

		public ZFunc(int _parameters, Action<object[]> _action) : this(_parameters, (s, ps) => _action(ps)) { }

		public ZFunc(int _parameters, Action _action) : this(_parameters, (s, ps) => _action()) { }

		//public ZFunc(int _parameters, object _func)
		//{
		//	this.Parameters = _parameters;

		//	if (_func is Func<Scope, object[], object>)
		//		this.Func = (Func<Scope, object[], object>)_func;
		//	else if (_func is Func<object[], object>)
		//		this.Func = (Func<Scope, object[], object>)((s, ps) => ((Func<object[], object>)_func)(ps));
		//	else if (_func is Func<object>)
		//		this.Func = (Func<Scope, object[], object>)((s, ps) => ((Func<object>)_func)());
		//	else
		//	{
		//		if (_func is Action<Scope, object[]>)
		//		{
		//			this.Func = (s, ps) =>
		//				{
		//					((Action<Scope, object[]>)_func)(s, ps);
		//					return null;
		//				};
		//		}
		//		else if (_func is Action<object[]>)
		//		{
		//			this.Func = (s, ps) =>
		//				{
		//					((Action<object[]>)_func)(ps);
		//					return null;
		//				};
		//		}
		//		else if (_func is Action<object>)
		//		{
		//			this.Func = (s, ps) =>
		//			{
		//				((Action<object>)_func)(ps[0]);
		//				return null;
		//			};
		//		}
		//	}
		//}

		public object Invoke(params object[] _params) => this.Invoke(null, _params);

		public object Invoke(Scope _scope, params object[] _params)
		{
			if (this.External)
				return Convert.ToZero(this.Func.Invoke(_scope, _params.Select(item => Convert.ToNet(item)).ToArray()));
			else
				return this.Func.Invoke(_scope, _params);
		}
	}
}