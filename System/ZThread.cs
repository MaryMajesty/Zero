using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Zero
{
	public class ZThread
	{
		internal Thread _Thread;
		internal Func _CurFunc;
		internal Scope _CurScope;
		internal int _Recursion;
		internal object _Result;

		internal bool _CrashHandled;
		internal bool _Finished;
		internal ZThread _ExecutingProgram;
		//public bool Finished
		//{
		//	get { return this._Finished; }
		//}
		//internal bool _WaitingForStep;
		//public bool WaitingForStep
		//{
		//	get { return this._WaitingForStep; }
		//}
		internal Script _Script;
		public Script Script
		{
			get { return this._Script; }
		}

		internal Func _GetCurFunc()
		{
			if (this._ExecutingProgram != null)
				return this._ExecutingProgram._GetCurFunc();
			else
				return this._CurFunc;
		}

		internal Scope _GetCurScope()
		{
			if (this._ExecutingProgram != null)
				return this._ExecutingProgram._GetCurScope();
			else
				return this._CurScope;
		}

		internal ZThread(Lambda _lambda, Program _program, params Variable[] _vars)
		{
			this._Script = _lambda._Func._Script;
			//_lambda._Scope._Thread = this;

			this._Thread = new Thread(() =>
				{
					try
					{
						this._Result = _lambda.Execute(new List(), new Scope(null, _program) { _Thread = this });
						this._Finished = true;
					}
					catch (Exception _ex)
					{
						if (!this._CrashHandled)
						{
							if (_ex is Error)
								_program._Crash(this, (Error)_ex);
							else
								_program._Crash(this, Error.Execution.External(_ex));
						}
					}
				});
		}

		//public void Step()
		//{
		//	this._WaitingForStep = false;
		//}

		internal void _Start() => this._Thread.Start();
	}
}