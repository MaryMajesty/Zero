using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Zero
{
	/// <summary>
	/// A representation of one or multiple scripts working together to form one cohesive functional program.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The main script's path.
		/// </summary>
		public readonly string ScriptPath;
		/// <summary>
		/// The main script's name.
		/// </summary>
		public readonly string ScriptName;

		/// <summary>
		/// The action to be executed on a crash.
		/// </summary>
		public Action<CrashReport> OnCrash = cr => { throw cr.Error; };
		/// <summary>
		/// The func to be executed when an additional permission is requested.
		/// </summary>
		public Func<Permission, bool> OnPermissionRequest = p => false;
		/// <summary>
		/// A list for tracking the program's currently operating console objects. To be used by developers for implementing a basic user interface based on console functionality.
		/// </summary>
		public List<object> Consoles = new List<object>();

		internal Permissions _Permissions = Permissions.None;
		internal ZThread _MainThread;
		internal List<ZThread> _Threads;
		internal Dictionary<string, object> _Scripts;
		internal Func _CurFunc;
		internal Scope _CurScope;
		internal int _Recursion;
		internal Lambda _Lambda;
		internal Func _StartFunc;

		/// <summary>
		/// Gets whether the program has crashed.
		/// </summary>
		public bool Crashed
		{
			get { return this._Crashed; }
		}
		private bool _Crashed;
		/// <summary>
		/// Gets whether the program has stopped executing.
		/// </summary>
		public bool Finished
		{
			get { return this._Started && (this._Crashed || this._Threads.TrueForAll(item => item._Finished)); }
		}
		/// <summary>
		/// Gets whether the program is currently being executed.
		/// </summary>
		public bool Executing
		{
			get { return this._Started && !this.Finished; }
		}
		private bool _Started;
		/// <summary>
		/// Gets the program's end result.
		/// </summary>
		public object Result
		{
			get
			{
				if (this.Finished && !this.Crashed)
					return Convert.ToNet(this._MainThread._Result);
				else
					throw new Exception("The program hasn't finished yet.");
			}
		}
		internal object _ResultInternal
		{
			get { return this._MainThread._Result; }
		}

		/// <summary>
		/// Initializes a program from a script and an optional path to overwrite the script's path.
		/// </summary>
		/// <param name="_script">The script that serves as the program's entry point.</param>
		/// <param name="_path">The (folder) path to overwrite the script's path, for example if the script is supposed to have a location but is executed in memory.</param>
		public Program(Script _script, string _path = null)
		{
			this._StartFunc = _script._Func;
			this.ScriptPath = _path ?? _script.Path;
			this.ScriptName = _script.FileName;
		}

		/// <summary>
		/// Starts the program with the specified permissions and includes the specified variables in its scope prior to execution.
		/// </summary>
		/// <param name="_permissions">The permissions the program starts with.</param>
		/// <param name="_vars">The variables to be added to the program's scope prior to its execution.</param>
		public void StartCustom(Permissions _permissions, params Variable[] _vars)
		{
			if (this._Started && !this.Finished)
				throw new Exception("A program can't run twice at the same time. Creating another program might be the solution you're looking for.");
			else
			{
				this._Started = false;
				this._Crashed = false;
				this._Permissions = _permissions;
				_permissions._Program = this;
				
				foreach (Variable var in _vars.Where(item => item._External))
					var.Value = Convert.ToZero(var.Value, _allowwrap: _permissions._Check(Permission.DllUsage));

				Scope s = new Scope(Libraries._GetComp(_vars), this);
				Lambda l = (Lambda)this._StartFunc._Calculate(s);
				this._Lambda = l;

				ZThread t = new ZThread(this._Lambda, this, _vars);
				this._MainThread = t;
				this._Threads = new List<ZThread>() { t };
				this._Scripts = new Dictionary<string, object>();
				
				this._Started = true;
				this._MainThread._Start();
			}
		}

		/// <summary>
		/// Starts the program with the specified permissions and includes the specified variables in its scope prior to execution in addition to the default variables.
		/// </summary>
		/// <param name="_permissions">The permissions the program starts with.</param>
		/// <param name="_vars">The variables to be added to the program's scope prior to its execution in addition to the default variables.</param>
		public void Start(Permissions _permissions, params Variable[] _vars)
		{
			List<Variable> vs = new List<Variable>(_vars);
			vs.AddRange(Libraries.Default.All);
			this.StartCustom(_permissions, vs.ToArray());
		}

		/// <summary>
		/// Starts the program with the specified permissions and includes the specified variables in its scope prior to execution in addition to the default variables and waits until the program has finished executing.
		/// </summary>
		/// <param name="_permissions">The permissions the program starts with.</param>
		/// <param name="_vars">The variables to be added to the program's scope prior to its execution in addition to the default variables.</param>
		/// <returns></returns>
		public object Execute(Permissions _permissions, params Variable[] _vars)
		{
			this.Start(_permissions, _vars);
			while (!this.Finished)
				Thread.Sleep(1);
			return this.Result;
		}

		internal void _Abort(Error _error) => this._Crash(null, _error);
		internal void _Abort(string _error, bool _external) => this._Crash(null, _external ? Error.Execution.AbortedExternally(_error) : Error.Execution.AbortedViaCode(_error));
		/// <summary>
		/// Aborts the program with a specified error message.
		/// </summary>
		/// <param name="_error">The error message to be included in the crash report.</param>
		public void Abort(string _error) => this._Abort(_error, _external: true);
		/// <summary>
		/// Aborts the program.
		/// </summary>
		public void Abort() => this._Abort("The program was aborted.", _external: true);
		/// <summary>
		/// Aborts the program as a result of an action taken by the user.
		/// </summary>
		public void UserAbort() => this._Abort(Error.Execution.AbortedByUser);

		internal void _Crash(ZThread _thread, Error _error)
		{
			this._Crashed = true;

			foreach (ZThread t in this._Threads.Where(item => item != _thread))
			{
				t._CrashHandled = true;
				t._Thread.Abort();
			}

			this.OnCrash?.Invoke(this._GetCrashReport(_thread, _error));
		}

		internal CrashReport _GetCrashReport(ZThread _thread, Error _error)
		{
			if (_thread != null)
				return new CrashReport(_thread._GetCurFunc(), _thread._GetCurScope(), _error);
			else
				return new CrashReport(this._CurFunc, this._CurScope, _error);
		}

		internal string _GetPath(List _path)
		{
			if (this.ScriptPath == null)
				throw Error.Permission.ScriptNotLocal;
			else
			{
				string path = _path.ToLiteral();
				if (!path.Contains(":"))
					this._Permissions._Request(Permission.LocalFileAccess);
				else
				{
					if (this.ScriptPath != null && path.StartsWith(this.ScriptPath))
						this._Permissions._Request(Permission.LocalFileAccess);
					else
						this._Permissions._Request(Permission.GlobalFileAccess);
				}

				if (path.Contains(':'))
					return path;
				else
					return this.ScriptPath + "\\" + path;
			}
		}
	}
}