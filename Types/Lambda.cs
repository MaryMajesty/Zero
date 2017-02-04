using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Lambda
	{
		internal List<Variable> _Vars;
		internal Func _Func;
		internal Scope _Scope;

		internal Lambda(List<Variable> _vars, Func _func, Scope _scope)
		{
			//foreach (Variable var in _vars)
			//	_scope.Vars._Variables.Remove(var);
			
			this._Vars = _vars;
			this._Func = _func;
			this._Scope = _scope;
		}

		public object Execute(List _params, Scope _scope = null)
		{
			if (this._Vars.Count == _params.Count)
			{
				Scope s = this._Scope._CloneLamb();
				if (_scope != null)
				{
					s._Thread = _scope.Thread;
					s.Program = _scope.Program;
				}

				for (int i = 0; i < _params.Count; i++)
					//s.Vars[this._Vars[i].Name] = _params[i];
					this._Vars[i].SetValue(_params[i]);

				//if (this._Vars.Count > 0)
				//	throw new Exception(s.Vars._Variables.Contains(this._Vars[0]).ToString());

				object o = this._Func._Calculate(s);

				if (s.Vars.Contains("$"))
					return s.Vars["$"];
				else
					return o;
			}
			else
				throw Error.Usage.IncorrectParameterCount(this._Vars.Count);
		}
	}
}