using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Comp
	{
		public List<Variable> _Variables = new List<Variable>();
		public object WrappedItem;

		public object this[string _name]
		{
			get
			{
				if (this._Variables.Any(item => item.Name.ToLower() == _name.ToLower()))
					return this._Variables.First(item => item.Name.ToLower() == _name.ToLower()).GetValue();
				else
				{
					Variable v = new Variable(_name, (object)null, _external: false);
					this._Variables.Add(v);
					return v.GetValue();
				}
			}

			set
			{
				if (this._Variables.Any(item => item.Name.ToLower() == _name.ToLower()))
					this._Variables.First(item => item.Name.ToLower() == _name.ToLower()).SetValue(value);
				else
				{
					Variable v = new Variable(_name, value, _external: false);
					this._Variables.Add(v);
				}
			}
		}

		public Comp() { }

		public Comp(params Variable[] _vars)
		{
			//foreach (Variable var in _vars)
			//	var.Name = var.Name.ToLower();
			this._Variables = _vars.ToList();
		}

		public Comp(params Comp[] _comps)
		{
			this._Variables = new List<Variable>();
			foreach (Comp c in _comps)
			{
				foreach (Variable v in c._Variables)
					this.AddVariable(v);
			}
		}

		public Variable GetVariable(string _name)
		{
			if (!this._Variables.Any(item => item.Name.ToLower() == _name.ToLower()))
				this[_name] = null;

			return this._Variables.First(item => item.Name.ToLower() == _name.ToLower());
		}

		public void AddVariable(Variable _var)
		{
			if (this.Contains(_var.Name))
				this._Variables.Remove(this.GetVariable(_var.Name));
			this._Variables.Add(_var);
		}

		public bool Contains(string _name) { return this._Variables.Any(item => item.Name.ToLower() == _name.ToLower() && item.Exists); }

		public Comp Clone() => new Comp(this);
		//{
		//	Comp @out = new Comp();
		//	foreach (Variable v in this._Variables)
		//		@out._Variables.Add(v);
		//	return @out;
		//}

		public Comp CloneLamb()
		{
			Comp @out = new Comp();
			foreach (Variable v in this._Variables.Where(item => item.Name != "$"))
				@out._Variables.Add(v);
			return @out;
		}
	}
}