using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Zero
{
	public class Func
	{
		public readonly string Name;
		internal Comp _Comp;
		private object _Constant;

		private Func[] _Parameters;
		public Func[] Parameters
		{
			get { return this._Parameters; }
			internal set { this._Parameters = value; }
		}
		private Func _Parent;
		public Func Parent
		{
			get { return this._Parent; }
			internal set { this._Parent = value; }
		}
		internal Script _Script;
		public Script Script
		{
			get
			{
				if (this._Script != null)
					return this._Script;
				else if (this.Parent != null)
					return this.Parent.Script;
				else
					return null;
			}
		}

		public Func(string _name, Func[] _parameters)
		{
			this.Name = _name;
			this.Parameters = _parameters;
		}

		public override string ToString()
		{
			if (this.Name == "_dot")
				return this.Parameters[0].ToString() + "." + this.Parameters[1].ToString();
			else if (this.Name == "_invoke")
				return this.Parameters[0].ToString() + "(" + string.Join(", ", this.Parameters[1].Parameters.Select(item => item.ToString())) + ")";
			else
				return this.Name + ((this.Parameters == null) ? "" : "(" + string.Join(", ", this.Parameters.Select(item => item.ToString())) + ")");
		}

		internal object _Calculate(Scope _scope) => this._CalculateRaw(_scope).GetValueNotVariable();

		internal object _CalculateRaw(Scope _scope)
		{
			object @out = null;

			if (_scope._Thread != null)
			{
				_scope._Thread._CurFunc = this;
				_scope._Thread._CurScope = _scope;
				_scope._Thread._Recursion++;
				if (_scope._Thread._Recursion > 300)
					throw Error.Execution.StackOverflow;
			}
			else
			{
				_scope.Program._CurFunc = this;
				_scope.Program._CurScope = _scope;
				_scope.Program._Recursion++;
				if (_scope.Program._Recursion > 300)
					throw Error.Execution.StackOverflow;
			}

			if (this._Constant != null)
				@out = this._Constant;
			else if (this.Name == null)
				@out = this.Parameters[0]._Calculate(_scope);
			else if (this.Parameters == null)
			{
				if (this.Name.IsFunction())
					@out = _scope.Vars.GetVariable(this.Name);
				else
				{
					if (this.Name[0] == '"')
					{
						this._Constant = Convert.ToZero(this.Name.Substring(1, this.Name.Length - 2));
						@out = this._Constant;
					}
					else if (this.Name[0] == '\'')
					{
						if (this.Name[this.Name.Length - 1] == '\'')
						{
							Tuple<string, char> t = EscapeCharacter.GetFirstChar(this.Name.Substring(1, this.Name.Length - 2));
							if (t.Item1.Length > 0)
								throw Error.Syntax.InvalidCharacter(this.Name.Substring(1, this.Name.Length - 2));
							else
							{
								this._Constant = (double)(ushort)t.Item2;
								@out = this._Constant;
							}
						}
						else
							throw new Exception("adslbafjgaf");
					}
					else
					{
						this._Constant = Ext.StringToDouble(this.Name);
						@out = this._Constant;
					}
				}
			}
			else
			{
				if (this.Name == "_list")
					@out = List.From(this.Parameters.Select(item => item._Calculate(_scope)));
				else if (this.Name == "_not")
				{
					object p = this.Parameters[0]._Calculate(_scope);
					if (p is double)
					{
						if ((double)p == 1)
							return 0.0;
						else if ((double)p == 0)
							return 1.0;
						else
							throw Error.Semantics.NotValidBoolean;
					}
					else
						throw Error.Semantics.IncorrectType(p, "Num");
				}
				else if (this.Name == "_comp")
				{
					Comp c = new Comp();
					foreach (Func f in this.Parameters)
					{
						if (f.Name != "_set" && f.Name != "_setprop")
							throw Error.Syntax.CompDeclarationError;
						else
						{
							f._Comp = c;
							f._Calculate(_scope);
						}
					}
					@out = c;
				}
				else if (this.Name == "_compadd")
				{
					if (this.Parameters[0] != null)
						@out = new Comp((Comp)this.Parameters[0]._Calculate(_scope), (Comp)this.Parameters[1]._Calculate(_scope));
					else
					{
						Comp c = (Comp)this.Parameters[1]._Calculate(_scope);
						foreach (Variable v in c._Variables)
							_scope.Vars.AddVariable(v);
					}
				}
				else if (this.Name == "_comphas")
				{
					List<string> vars = new List<string>();
					
					object two = this.Parameters[1]._Calculate(_scope);
					if (two is Comp)
					{
						foreach (Variable v in ((Comp)two)._Variables)
							vars.Add(v.Name);
					}
					else if (two is List)
						vars.Add(((List)two).ToLiteral());
					else
						throw Error.Semantics.IncorrectType(two, "Comp", "List");

					Comp one = (this.Parameters[0] != null) ? (Comp)this.Parameters[0]._Calculate(_scope) : _scope.Vars;
					return vars.TrueForAll(item => one.Contains(item)) ? 1.0 : 0.0;
				}
				else if (this.Name == "_wave")
					@out = new Wave(this.Parameters[0], _scope);
				else if (this.Name == "_waituntil")
				{
					//if (this.Parameters.Length == 0)
					//{
					//	//_scope._Thread._WaitingForStep = true;
					//	//while (_scope._Thread._WaitingForStep)
					//	//	Thread.Sleep(1);
					//}
					//else
					{
						object o = this.Parameters[0]._Calculate(_scope);
						Action act = () => Thread.Sleep(1);
						if (this.Parameters.Length > 1)
							act = () => this.Parameters[1]._Calculate(_scope);

						DateTime start = DateTime.UtcNow;
						if (o is Wave)
						{
							while (!((Wave)o).Finished)
								act();
						}
						else if (o is double)
						{
							double d = (double)o;
							while ((d = (double)this.Parameters[0]._Calculate(_scope)) == 0)
								act();

							if (d != 1)
								throw Error.Semantics.NotValidBoolean;
						}
						else
							throw Error.Semantics.IncorrectType(o, "Wave", "Num");

						@out = (DateTime.UtcNow - start).TotalMilliseconds;
					}
				}
				else if (this.Name == "_waittime")
				{
					double d = (double)this.Parameters[0]._Calculate(_scope);
					Action act = () => Thread.Sleep(1);
					if (this.Parameters.Length > 1)
						act = () => this.Parameters[1]._Calculate(_scope);

					DateTime start = DateTime.UtcNow;
					while ((DateTime.UtcNow - start).TotalMilliseconds < d)
						act();
				}
				else if (this.Name == "_set")
				{
					if (this._Comp != null)
						@out = this._Comp[this.Parameters[0].Name] = this.Parameters[1]._Calculate(_scope);
					else
					{
						List<Variable> vars = this.Parameters[0]._GetVars(_scope);
						
						List vals = new List();
						object val = this.Parameters[1]._Calculate(_scope);
						if (vars.Count == 1)
							vals.Add(val);
						else if (vars.Count > 1 && val is List && ((List)val).Count == vars.Count)
							vals = ((List)val).Select(item => item);
						else
							throw new Exception("akdfjbafb");

						for (int i = 0; i < vars.Count; i++)
							vars[i].SetValue(vals[i]);

						if (vars.Count == 1)
							@out = vars[0];
						else
							@out = List.From(vars);
					}
				}
				else if (this.Name == "_tryset")
				{
					Variable v = _scope.Vars.GetVariable(this.Parameters[0].Name);
					if (!v.Exists)
						v.Value = this.Parameters[1]._Calculate(_scope);
					@out = v.GetValue();
				}
				else if (this.Name == "_setprop")
				{
					Variable v = null;
					if (this._Comp != null)
						v = this._Comp.GetVariable(this.Parameters[0].Name);
					else
						v = this.Parameters[0]._GetVars(_scope)[0];

					if (this.Parameters[1].Name == "_lamb")
					{
						Lambda l = (Lambda)this.Parameters[1]._Calculate(_scope);
						if (l._Scope.Vars._Variables.Any(item => item.Name == v.Name))
						{
							l._Scope.Vars._Variables.Remove(l._Scope.Vars._Variables.First(item => item.Name == v.Name));
							l._Scope.Vars._Variables.Add(new Variable(v.Name, () => v.Value, val => v.Value = val, _external: false));
						}

						if (l._Vars.Count == 0)
						{
							if (v.Get == null)
							{
								v.Get = new ZFunc(0, (Func<object[], object>)(ps => l.Execute(new List(), _scope)));
								@out = null;
							}
							else
								throw Error.Usage.GetOverwriteAttempted(v.Name);
						}
						else if (l._Vars.Count == 1)
						{
							if (v.Set == null)
							{
								v.Set = new ZFunc(1, (Action<object[]>)(ps => l.Execute(new List(ps[0]), _scope)));
								@out = null;
							}
							else
								throw Error.Usage.SetOverwriteAttempted(v.Name);
						}
						else
							throw Error.Usage.IncorrectPropertyParameterCount;
					}
					else
					{
						if (v.Get == null)
						{
							object o = this.Parameters[1]._Calculate(_scope);
							v.Get = new ZFunc(0, (Func<object[], object>)(ps => o));
						}
						else
							throw Error.Usage.GetOverwriteAttempted(v.Name);
					}
				}
				else if (this.Name == "_br")
				{
					object[] rs = this.Parameters.Select(item => item._Calculate(_scope)).ToArray();
					if (rs.Length > 0)
						@out = rs[rs.Length - 1];
				}
				else if (this.Name == "_iter")
				{
					Func f = this.Parameters[2];

					List<Variable> vars = this.Parameters[0] != null ? this.Parameters[0]._GetVars(_scope) : new List<Variable>();
					List l = ((List)this.Parameters[1]._Calculate(_scope));

					if (vars.Count == 0)
						vars.Add(null);

					if (vars.Count > 1 && vars.Count != l.Count)
						throw Error.Usage.IterCountNotEqual;

					@out = Func._Iter(f, _scope._Clone(), vars, (vars.Count <= 1) ? new List<List>() { l } : ((List[])l.ToArray().Select(item => (List)(item)).ToArray()).ToList());
				}
				else if (this.Name == "_if")
				{
					if (this.Parameters[0]._Calculate(_scope).AsBool())
						@out = this.Parameters[1]._Calculate(_scope);
					else
					{
						if (this.Parameters.Length == 3)
							@out = this.Parameters[2]._Calculate(_scope);
					}
				}
				else if (this.Name == "_while")
				{
					if (this.Parameters.Length == 1)
					{
						while (true)
							this.Parameters[0]._Calculate(_scope);
					}
					else
					{
						while (this.Parameters[0]._Calculate(_scope).AsBool())
							this.Parameters[1]._Calculate(_scope);
					}
				}
				else if (this.Name == "_lamb")
				{
					if (this.Parameters.Length == 1)
						@out = new Lambda(new List<Variable>(), this.Parameters[0], _scope);
					else
					{
						Scope s = _scope._Clone();
						@out = new Lambda(this.Parameters[0]._GetVars(s), this.Parameters[1], s);
					}
				}
				else if (this.Name.StartsWith("_for"))
				{
					double min = 0;
					double max;
					if ((this.Name.EndsWith("s") && this.Parameters.Length == 3) || (!this.Name.EndsWith("s") && this.Parameters.Length == 2))
					{
						min = (double)this.Parameters[0]._Calculate(_scope);
						max = (double)this.Parameters[1]._Calculate(_scope);
					}
					else
						max = (double)this.Parameters[0]._Calculate(_scope);

					double step = this.Name.EndsWith("s") ? (double)this.Parameters[this.Parameters.Length - 1]._Calculate(_scope) : 1;

					List l = new List();
					for (double n = min; n < max; n += step)
						l.Add(n);
					@out = l;
				}
				else if (this.Name == "_ifelse")
					@out = this.Parameters[this.Parameters[0]._Calculate(_scope).AsBool() ? 1 : 2]._Calculate(_scope);
				else if (this.Name == "_dot")
				{
					object o = this.Parameters[0]._Calculate(_scope);
					if (!o.IsPrimitive())
					{
						if (o is Comp)
							@out = ((Comp)o).GetVariable(this.Parameters[1].Name);
						else
							throw new Exception("sfbvaskjfg");
					}
					else
					{
						Comp type = (Comp)_scope.Vars[o.GetPrimitiveName()];
						Variable function = type.GetVariable(this.Parameters[1].Name);
						ZFunc value = (ZFunc)function.Value;

						if (value.Parameters > 1)
							@out = new PrimInvoke(type.GetVariable(this.Parameters[1].Name), o);
						else
							@out = value.Invoke(o);
					}
				}
				else if (this.Name == "_invoke")
				{
					object o = this.Parameters[0]._CalculateRaw(_scope);
					if (o is PrimInvoke)
					{
						object f = ((PrimInvoke)o).Function;
						List ps = new List(((PrimInvoke)o).FirstParameter);
						ps.AddRange(((List)this.Parameters[1]._Calculate(_scope)).Select(item => item));
						@out = Func._Invoke(f, _scope, ps);
					}
					else
						@out = Func._Invoke(o, _scope, ((List)this.Parameters[1]._Calculate(_scope)).Select(item => item));
				}
				else if (this.Name == "_switch")
				{
					Func[] lines = Func._GetLines(this.Parameters[2]);

					Dictionary<object, Func<object>> cases = new Dictionary<object, Func<object>>();
					Func<object> def = null;

					foreach (Func line in lines)
					{
						Scope sc = _scope._Clone();
						cases[line.Parameters[0]._Calculate(sc)] = () => line.Parameters[1]._Calculate(sc);

						if (line.Parameters.Length > 2)
							def = () => line.Parameters[2]._Calculate(sc);
					}

					Scope s = _scope._Clone();
					if (this.Parameters[0] != null)
					{
						List l = List.From(cases.ToArray().Select(item => item.Key));
						s.Vars[this.Parameters[0].Name] = l;
					}
					object value = this.Parameters[1]._Calculate(s);

					if (cases.Any(item => item.Key.Equals(value)))
						@out = cases.First(item => item.Key.Equals(value)).Value();
					else if (def != null)
						@out = def();
				}
				else if (this.Name == "_script")
				{
					string s = _scope.Program._GetPath((List)this.Parameters[0]._Calculate(_scope));
					string r = s.Substring(_scope.Program.ScriptPath.Length);
					s += ".zr";

					if (!_scope.Program._Scripts.ContainsKey(r))
					{
						Program p = new Program(new Script(s, _external: false, _raw: false));
						p.Start(_scope.Program._Permissions);
						_scope._Thread._ExecutingProgram = p._MainThread;
						while (!p.Finished)
							Thread.Sleep(1);
						object v = p._ResultInternal;
						_scope._Thread._ExecutingProgram = null;
						_scope.Program._Scripts[r] = v;
						return v;
					}
					else
						return _scope.Program._Scripts[r];
				}
				else if (this.Name == "_ensure")
				{
					if (!this.Parameters[0]._Calculate(_scope).AsBool())
					{
						string msg = null;
						if (this.Parameters.Length == 2)
							msg = ((List)this.Parameters[1]._Calculate(_scope)).ToLiteral();
						throw Error.Usage.EnsureFailed(msg);
					}
				}
				else if (this.Name == "_try")
				{
					Scope s = _scope._Clone();

					if (this.Parameters[0] != null)
					{
						bool b = false;
						try
						{
							this.Parameters[0]._Calculate(s);
							b = true;
						}
						catch { }

						if (b)
							this.Parameters[1]._Calculate(s);
						else if (this.Parameters.Length > 2)
							this.Parameters[2]._Calculate(s);
					}
				}
				else if (this.Name == "_nopelamb")
					@out = new Lambda(new List<Variable>(), new Func("_nope", new Func[0]), _scope);
				else if (this.Name == "_error")
					throw Error.Custom(this.Parameters.Length > 0 ? ((List)this.Parameters[0]._Calculate(_scope)).ToLiteral() : "An undescribed error occurred.");
				else if (this.Name != "_nope")
					throw Error.Semantics.NameNotFound(this.Name);
			}

			if (_scope._Thread != null)
			{
				_scope._Thread._CurFunc = this.Parent;
				_scope._Thread._Recursion--;
			}
			else
			{
				_scope.Program._CurFunc = this.Parent;
				_scope.Program._Recursion--;
			}

			return @out;
		}

		internal List<Variable> _GetVars(Scope _scope)
		{
			List<Variable> @out = new List<Variable>();
			if (this.Name == "_list")
			{
				foreach (Func f in this.Parameters)
					@out.Add((Variable)f._CalculateRaw(_scope));
			}
			else
				@out.Add((Variable)this._CalculateRaw(_scope));

			return @out;
		}

		internal static Func[] _GetLines(Func _func)
		{
			List<Func> @out = new List<Func>();

			if (_func.Name == "_br")
			{
				foreach (Func f in Func._GetLines(_func.Parameters[0]))
					@out.Add(f);
				@out.Add(_func.Parameters[1]);
			}
			else
				@out.Add(_func);

			return @out.ToArray();
		}

		internal static List _Iter(Func _f, Scope _scope, List<Variable> _vs, List<List> _os)
		{
			List @out = new List();
			foreach (object o in _os[0].Objects)
			{
				if (_vs[0] != null)
				{
					Variable v = _scope.Vars.GetVariable(_vs[0].Name);
					if (o is Variable)
					{
						v.Get = new ZFunc(0, () => ((Variable)o).GetValue());
						v.Set = new ZFunc(0, ps => ((Variable)o).SetValue(ps[0]));
					}
					else
					{
						v.Get = null;
						v.Set = null;
						v.Value = o;
					}
				}

				if (_vs.Count > 1)
					@out.Add(Func._Iter(_f, _scope, _vs.GetRange(1, _vs.Count - 1), _os.GetRange(1, _os.Count - 1)));
				else
					@out.Add(_f._Calculate(_scope));
			}
			return @out;
		}

		internal static object _Invoke(object _o, Scope _scope, List _params)
		{
			string name = null;

			if (_o is Comp)
				_o = ((Comp)_o).GetVariable("GetIndex");
			if (_o is Variable)
			{
				name = ((Variable)_o).Name;
				_o = ((Variable)_o).GetValue();
			}
			
			if (_o is ZFunc)
			{
				ZFunc f = (ZFunc)_o;
				if (f.Parameters == _params.Count)
				{
					object[] ps = _params.ToArray();
					return f.Invoke(_scope, ps);
				}
				else
					throw Error.Usage.IncorrectParameterCount(name, f.Parameters);
			}

			if (_o is Lambda)
				return ((Lambda)_o).Execute(_params, _scope);
			if (_o is List)
				return ((List)_o)[_params[0].AsInt()];

			if (name != null)
				throw Error.Usage.NotInvokable(name, _o);
			else
				throw Error.Usage.NotInvokable(_o);
		}
	}
}