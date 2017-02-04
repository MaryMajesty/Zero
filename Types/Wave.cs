using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zero
{
	public class Wave
	{
		//private List<Variable> _Parameters = new List<Variable>();
		//public Variable[] Parameters
		//{
		//	get { return this._Parameters.ToArray(); }
		//}
		private ZThread _Thread;
		public bool Finished
		{
			get { return this._Thread._Finished; }
		}

		internal Wave(Func _func, Scope _scope/*, List _parameters*/)
		{
			Scope s = _scope._Clone();
			Lambda l = new Lambda(new List<Variable>(), _func, s);
			this._Thread = new ZThread(l, _scope.Program);
			s._Thread = this._Thread;
			s.Program._Threads.Add(this._Thread);
			this._Thread._Start();
		}
	}
}