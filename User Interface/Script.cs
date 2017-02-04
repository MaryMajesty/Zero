using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zero
{
	/// <summary>
	/// A parsed representation of a script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The path at which the script's source code file is located.
		/// </summary>
		public readonly string Path;
		/// <summary>
		/// The name of the script's source code file.
		/// </summary>
		public readonly string FileName;

		internal Func _Func;
		private bool _External;
		private string _SourceCode;

		/// <summary>
		/// The script's source code.
		/// </summary>
		public string SourceCode
		{
			get { return this._SourceCode; }
		}

		internal Script(Func _func) { this._Func = _func; }

		/// <summary>
		/// Creates a new script using the source code file located at a certain path.
		/// </summary>
		/// <param name="_path">The source code file's path.</param>
		public Script(string _path)
		{
			this.Path = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(_path));
			this.FileName = System.IO.Path.GetFileName(_path);

			this._SourceCode = System.IO.File.ReadAllText(_path);
			this._Func = Parser.Parse(this._SourceCode);
			this._Func._Script = this;
		}

		internal Script(string _path, bool _external, bool _raw, bool _parse = true)
		{
			if (!_raw)
			{
				this.Path = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(_path));
				this.FileName = System.IO.Path.GetFileName(_path);
			}

			if (_raw)
				this._SourceCode = _path;
			else
				this._SourceCode = System.IO.File.ReadAllText(_path);

			if (_parse)
			{
				this._Func = Parser.Parse(this._SourceCode);
				this._Func._Script = this;
			}

			this._External = _external;
		}

		/// <summary>
		/// Creates a new script from source code.
		/// </summary>
		/// <param name="_code">The script's source code.</param>
		public static Script Parse(string _code) { return new Script(_code, _external: true, _raw: true); }

		/// <summary>
		/// Tries to parse a script from source code. In case of a parsing error, the error is returned.
		/// </summary>
		/// <param name="_code">The script's source code.</param>
		/// <param name="_script">The variable that the resulting script will be assigned to.</param>
		public static Error TryParse(string _code, out Script _script)
		{
			try
			{
				_script = Script.Parse(_code);
				return null;
			}
			catch (Exception ex)
			{
				_script = null;
				if (ex is Error)
					return Error.Execution.External(ex);
				else
					return (Error)ex;
			}
		}

		/// <summary>
		/// Creates a new script that does not get parsed. It simply stores the specified source code and does nothing else.
		/// </summary>
		/// <param name="_code">The source code to be stored.</param>
		public static Script Store(string _code) => new Script(_code, _external: true, _raw: true, _parse: false);

		//internal object Execute(Permissions _permissions)
		//{
		//	return this.Execute(_permissions,
		//	//Lambda l = (Lambda)this._Func._Calculate(new Scope(new Comp(Libraries.Default.All.ToArray()), _scope.Program) { _Thread = _scope._Thread }).GetValue();
		//	//return l.Execute(new List());
		//}

		/// <summary>
		/// Executes the script without any permissions and with optional custom variables.
		/// </summary>
		/// <param name="_vars">The custom variables that will get added to the script's scope prior to its execution.</param>
		public object Execute(params Variable[] _vars) => this.Execute(Permissions.None, _vars);

		/// <summary>
		/// Executes the script with the specified permissions and with optional custom variables.
		/// </summary>
		/// <param name="_permissions">The permissions the script has.</param>
		/// <param name="_vars">The custom variables that will get added to the script's scope prior to its execution.</param>
		public object Execute(Permissions _permissions, params Variable[] _vars)
		{
			Program p = new Program(this);
			p.Start(_permissions, _vars);
			while (!p.Finished)
				Thread.Sleep(1);

			//if (this._External)
			//	return Convert.ToNet(p._MainThread._Result);
			//else

			return p._MainThread._Result;
		}
		
		/// <summary>
		/// Returns the script's source code.
		/// </summary>
		public override string ToString() => this._SourceCode;
	}
}