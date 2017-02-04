using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Scope
	{
		public Comp Vars;
		public Program Program;
		internal ZThread _Thread;
		public ZThread Thread
		{
			get { return this._Thread; }
		}

		internal Scope(Comp _vars, Program _program)
		{
			this.Vars = _vars;
			this.Program = _program;
		}

		internal Scope _Clone() => new Scope(this.Vars.Clone(), this.Program) { _Thread = this._Thread };
		internal Scope _CloneLamb() => new Scope(this.Vars.CloneLamb(), this.Program) { _Thread = this._Thread };
	}
}