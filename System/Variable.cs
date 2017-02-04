using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Variable
	{
		public string Name;
		public object Value;
		public ZFunc Get;
		public ZFunc Set;
		internal bool _External;
		internal bool _Internal;

		public bool IsProperty
		{
			get { return this.Get != null || this.Set != null; }
		}
		public bool Exists
		{
			get { return this.IsProperty || this.Value != null; }
		}

		public Variable(string _name, object _value, bool _external)
		{
			this.Name = _name;
			this.Value = _value;
			this._External = _external;
			//this.Value = _external ? Convert.ToZero(_value) : _value;
		}

		public Variable(string _name, object _value) : this(_name, _value, _external: true) { }

		public Variable(string _name, int _params, Action<Scope, object[]> _action)
		{
			this.Name = _name;
			this.Value = new ZFunc(_params, _action);
		}

		public Variable(string _name, int _params, Func<Scope, object[], object> _func)
		{
			this.Name = _name;
			this.Value = new ZFunc(_params, _func);
		}

		public Variable(string _name, Func<object> _get, Action<object> _set, bool _external)
		{
			this.Name = _name;
			this.Get = new ZFunc(0, /*(Func<object[], object>)(ps => _get())*/ _get) { External = _external };
			this.Set = new ZFunc(1, /*(Action<object[]>)(ps => _set(ps[0]))*/ ps => _set(ps[0])) { External = _external };
		}

		public Variable(string _name, Func<object> _get, Action<object> _set) : this(_name, _get, _set, _external: true) { }

		public object GetValue()
		{
			object @out = null;

			if (this.IsProperty)
			{
				if (this.Get != null)
					@out = this.Get.Invoke();
				else
					throw new Exception("The variable \"" + this.Name + "\" can't be gotten.");
			}
			else
				@out = this.Value;

			if (@out == null)
				throw Error.Semantics.VariableNotExistent(this);
			else
				return @out;
		}

		public void SetValue(object _value)
		{
			if (_value is List)
				_value = new List(((List)_value).Objects.ToArray());

			if (this.IsProperty)
			{
				if (this.Set != null)
					this.Set.Invoke(_value);
				else
					throw new Exception("The variable \"" + this.Name + "\" can't be set.");
			}
			else
				this.Value = _value;
		}

		//internal object Execute(object[] _params, Comp _vars)
		//{
		//	if (this.Value is Action<object[]>)
		//	{
		//		if (this.Params == _params.Length)
		//		{
		//			((Action<object[]>)this.Value).Invoke(_params);
		//			return null;
		//		}
		//		else
		//			throw Exceptions.Execution(this.Name, Exceptions.ParameterCount(this.Params));
		//	}
		//	else if (this.Value is Func<object[], object>)
		//	{
		//		if (this.Params == _params.Length)
		//			return ((Func<object[], object>)this.Value)(_params);
		//		else
		//			throw Exceptions.Execution(this.Name, Exceptions.ParameterCount(this.Params));
		//	}
		//	else if (this.Value is Lambda)
		//	{
		//		try { return ((Lambda)this.Value).Execute(_params.ToList()); }
		//		catch (Exception _ex) { throw Exceptions.Execution(this.Name, _ex); }
		//	}
		//	else if (this.Value is List)
		//		return ((List)this.Value)[_params[0].ToInt()];
		//	else
		//		throw new Exception("Variable " + '"' + this.Name + '"' + " is neither a function nor a list and thus can't be invoked.");
		//}
	}
}