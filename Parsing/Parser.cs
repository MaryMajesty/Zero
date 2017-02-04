using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	internal static class Parser
	{
		private static bool _OperatorsSet;

		internal static List<Brackets> Brackets = new List<Brackets>()
			{
				new Brackets("list", "[", "]"),
				new Brackets("comp", "{", "}"),
			};

		private static Operator[] _Operators = new Operator[]
			{
				new Operator("br", 0, ";"),
				
				new Operator("setprop", 1, ":="),

				new Operator("waituntil", 2, false, true, ".."),
				new Operator("waituntil", 2, false, true, "..", ":"),
				new Operator("waittime", 2, true, false, ".."),
				new Operator("waittime", 2, true, true, "..", ":"),
				new Operator("error", 2, false, true, "!!!"),
				new Operator("error", 2, false, false, "!!!"),

				new Operator("set", 3, "="),
				new Operator("tryset", 3, "~="),

				

				new Operator("iter", 5, "<<", ":"),
				new Operator("iter", 5, false, true, "<<", ":") { IncludeEmptyLeft = true },
				new Operator("if", 5, false, true, "??", ":"),
				new Operator("if", 5, false, true, "??", ":", "!!", ":"),
				new Operator("while", 5, false, true, "%%", ":"),
				new Operator("switch", 5, false, true, @"\\", ":") { IncludeEmptyLeft = true },
				new Operator("switch", 5, true, true, @"\\", ":"),
				new Operator("case", 5, false, true, "//", ":"),
				new Operator("case", 5, false, true, "//", ":", "!!", ":"),
				new Operator("wave", 5, false, true, "|||", ":"),
				new Operator("ensure", 5, false, true, "###"),
				new Operator("ensure", 5, false, true, "###", ":"),
				new Operator("try", 5, false, true, "~~", ":"),
				new Operator("try", 5, false, true, "~~", ":", "!!", ":"),

				new SelectOperator(6, "\\") { Assignable = true },
				new ShortOperator("where", 6, "?\\"),
				new ShortOperator("sum", 6, "+\\"),
				new ShortOperator("all", 6, "&\\"),
				new ShortOperator("any", 6, "|\\"),
				new ShortOperator("orderby", 6, "<\\"),
				new ShortOperator("orderbydescending", 6, ">\\"),

				new Operator("lamb", 7, ">>", ":"),
				new Operator("lamb", 7, false, true, ">>", ":"),
				
				new Operator("ifelse", 8, "?", "!"),

				new Operator("for", 9, "~"),
				new Operator("for", 9, false, true, "~"),
				new Operator("fors", 9, "~", "~+"),
				new Operator("fors", 9, false, true, "~", "~+"),
				
				new ShortOperator("and", 10, "&") { Assignable = true, Selectable = true },
				new ShortOperator("or", 10, "|") { Assignable = true, Selectable = true },
				
				new ShortOperator("equals", 11, "?="),
				new NotEqualsOperator(11, "!="),
				new ShortOperator("greaterthan", 11, ">"),
				new OrOperator(">=", new ShortOperator("equals", 11, "?="), new ShortOperator("greaterthan", 10, ">")),
				new ShortOperator("lessthan", 11, "<"),
				new OrOperator("<=", new ShortOperator("equals", 11, "?="), new ShortOperator("lessthan", 10, "<")),
				new Operator("compadd", 11, "<+") { Assignable = true, Selectable = true },
				new Operator("compadd", 11, false, true, "<+") { IncludeEmptyLeft = true },
				new Operator("comphas", 11, "<?"),
				new Operator("comphas", 11, false, true, "<?") { IncludeEmptyLeft = true },
				
				new ShortOperator("add", 12, "+") { Assignable = true, Selectable = true },
				new ShortOperator("subtract", 12, "-") { Assignable = true, Selectable = true },

				new ShortOperator("multiply", 13, "*") { Assignable = true, Selectable = true },
				new ShortOperator("divide", 13, "/") { Assignable = true, Selectable = true },
				new ShortOperator("modulate", 13, "%") { Assignable = true, Selectable = true },

				new ShortOperator("exponentiate", 14, "^") { Assignable = true, Selectable = true },
				new Operator("not", 14, false, true, "!"),
				new SetNotOperator(14, "!"),
				new NegativeOperator(14, "-"),
				new Operator("script", 14, false, true, "@")
			};

		internal static Operator[] Operators
		{
			get
			{
				if (!Parser._OperatorsSet)
				{
					Parser._OperatorsSet = true;

					List<Operator> ops = new List<Operator>(Parser._Operators);
					foreach (Operator op in Parser._Operators)
					{
						SelectOperator so = null;

						if (op.Assignable)
							ops.Add(new AssignOperator(op, 3));
						if (op.Selectable)
							ops.Add(so = new ShortSelectOperator(op, 6));

						if (op.Assignable && op.Selectable)
							ops.Add(new AssignOperator(so, 3));
					}

					Parser._Operators = ops.ToArray();
				}

				return Parser._Operators;
			}
		}

		internal static List<int> GetUnquotedChars(string _text)
		{
			List<int> @out = new List<int>();

			char? quote = null;
			for (int i = 0; i < _text.Length; i++)
			{
				if (quote == null)
				{
					if (_text[i] == '\'' || _text[i] == '"')
						quote = _text[i];
					else
						@out.Add(i);
				}
				else if (_text[i] == quote)
					quote = null;
			}

			return @out;
		}
		
		internal static string[] RemoveComments(string[] _lines, CodeKeeper _keeper)
		{
			List<string> @out = new List<string>();

			int cx = 0;
			for (int i = 0; i < _lines.Length; i++)
			{
				string l = _lines[i];
				string n = "";

				bool comment = false;
				int skip = 0;
				int last = 0;
				int rlast = 0;
				foreach (int x in GetUnquotedChars(l))
				{
					if (skip > 0)
						skip--;
					else if (l[x] == '#')
					{
						if (x < l.Length - 2 && l[x + 1] == '#' && l[x + 2] == '#')
							skip = 2;
						else
						{
							if (!comment)
							{
								n += l.Substring(last, x - last);
								_keeper.RemoveLatent(cx + rlast, last - rlast);
								rlast = x;
							}
							else
								last = x + 1;

							comment = !comment;
						}
					}
				}

				if (last != l.Length)
				{
					if (!comment)
					{
						n += l.Substring(last, l.Length - last);
						_keeper.RemoveLatent(cx + rlast, last - rlast);
					}
					else
						_keeper.RemoveLatent(cx + rlast, l.Length - rlast);
				}

				if (n != "" && !n.ToList().TrueForAll(item => (byte)item <= 32))
					@out.Add(n);
				else
					_keeper.RemoveLatent(cx, l.Length + 1);

				cx += l.Length + 1;
			}

			_keeper.UpdateLatents();

			return @out.ToArray();
		}

		internal static string CollapseLines(string[] _lines, CodeKeeper _keeper)
		{
			string @out = "";

			int tabs = 0;
			int cx = 0;
			for (int i = 0; i < _lines.Length; i++)
			{
				int ncx = cx + _lines[i].Length + 1;

				if (_lines[i].Any(item => (byte)item > 32))
				{
					int t = 0;
					while (_lines[i][0] == '\t')
					{
						_lines[i] = _lines[i].Substring(1);
						_keeper.RemoveLatent(cx, 1);
						t++;
						cx += 1;
					}
					if (tabs < t)
					{
						while (tabs < t)
						{
							@out += ":(";
							_keeper.InsertLatent(cx, 2);
							tabs++;
						}
					}
					else if (tabs == t && i > 0 && !Operators.Any(item => item.Symbols.Length >= 3 && _lines[i].StartsWith(item.Symbols[2])))
					{
						@out += ";";
						_keeper.InsertLatent(cx, 1);
					}

					bool b = tabs > t;
					while (tabs > t)
					{
						@out += ")";
						_keeper.InsertLatent(cx, 1);
						tabs--;
					}

					if (b && !Operators.Any(item => item.Symbols.Length >= 3 && _lines[i].StartsWith(item.Symbols[2])))
					{
						@out += ";";
						_keeper.InsertLatent(cx, 1);
					}
					
					@out += _lines[i];
				}

				cx = ncx;

				if (i < _lines.Length - 1)
					_keeper.RemoveLatent(cx - 1, 1);
			}
			while (tabs > 0)
			{
				@out += ")";
				_keeper.InsertLatent(cx - 1, 1);
				tabs--;
			}

			_keeper.UpdateLatents();

			return @out;
		}

		internal static string ConvertDots(string _code, CodeKeeper _keeper)
		{
			while (true)
			{
				bool b = false;

				foreach (int i in GetUnquotedChars(_code))
				{
					if (_code[i] == '.' && _code[i - 1] != '.' && _code[i + 1].ToString().IsFunction())
					{
						int ps = 0;

						int start = 0;

						foreach (int x in GetUnquotedChars(_code.Substring(0, i)).Reverse<int>())
						{
							if (_code[x] == ')' || _code[x] == ']')
								ps++;
							else if (_code[x] == '(' || _code[x] == '[')
							{
								ps--;
								if (ps < 0)
								{
									start = x + 1;
									break;
								}
							}
							else
							{
								if (ps == 0 && !_code[x].ToString().IsExpression())
								{
									start = x + 1;
									break;
								}
							}
						}

						int end = _code.Length - 1;
						for (int x = i + 1; x < _code.Length; x++)
						{
							if (!_code[x].ToString().IsFunction())
							{
								end = x - 1;
								ps = 0;
								break;
							}
						}

						string p1 = _code.Substring(start, i - start);
						string p2 = _code.Substring(i + 1, end - i);
						_code = _code.Substring(0, start) + "_dot(" + p1 + "," + p2 + ")" + _code.Substring(end + 1);

						_keeper.Insert(end + 1, 1);
						_keeper.Remove(i, 1);
						_keeper.Insert(i, 1);
						_keeper.Insert(start, 5);

						b = true;
						break;
					}
				}
				if (!b)
					break;
			}
			
			return _code;
		}

		internal static string ConvertInvokes(string _code, CodeKeeper _keeper)
		{
			while (true)
			{
				bool b = false;

				foreach (int i in GetUnquotedChars(_code))
				{
					if (_code[i] == '(')
					{
						int ll = 0;
						int ps = 0;
						
						foreach (int x in GetUnquotedChars(_code.Substring(0, i)).Reverse<int>())
						{
							if (_code[x] == ')' || _code[x] == ']')
								ps++;
							else if (_code[x] == '(' || _code[x] == '[')
							{
								ps--;
								if (ps < 0)
								{
									ll = i - x - 1;
									break;
								}
							}
							else if (ps == 0 && !_code[x].ToString().IsFunction())
							{
								ll = i - x - 1;
								break;
							}
						}

						string l = _code.Substring(i - ll, ll);
						if (ll > 0 && (l.Contains('(') || !l.StartsWith("_")))
						{
							int rl = 0;
							ps = 0;
							foreach (int x in GetUnquotedChars(_code.Substring(i + 1)))
							{
								if (_code[x + i + 1] == '(')
									ps++;
								else if (_code[x + i + 1] == ')')
								{
									ps--;
									if (ps < 0)
									{
										rl = x;
										break;
									}
								}
							}

							string r = _code.Substring(i + 1, rl);
							
							_code = _code.Substring(0, i - ll) + "_invoke(" + l + ",[" + r + "])" + _code.Substring(i + r.Length + 2);

							_keeper.Remove(i + 1 + rl, 1);
							_keeper.Insert(i + 1 + rl, 2);
							_keeper.Remove(i, 1);
							_keeper.Insert(i, 2);
							_keeper.Insert(i - ll, 8);

							b = true;
							break;
						}
					}
				}

				if (!b)
					break;
			}
			
			return _code;
		}
		
		internal static string Prepare(string _code, out CodeKeeper _keeper)
		{
			List<string> ls = new List<string>() { "[]>>" };
			foreach (string l in _code.Split('\n'))
				ls.Add("\t" + l.Replace("\r", ""));

			_keeper = new CodeKeeper(string.Join("\n", ls));

			string[] lines = ls.ToArray();
			lines = RemoveComments(ls.ToArray(), _keeper);
			_code = CollapseLines(lines, _keeper);
			
			_code = ConvertDots(_code, _keeper);
			_code = ConvertInvokes(_code, _keeper);

			return _code;
		}

		internal static List<string> Split(string _line, CodeKeeper _keeper)
		{
			List<string> sps = new List<string>() { "(", ")", "," };
			foreach (Brackets br in Brackets)
			{
				sps.Add(br.Start);
				sps.Add(br.End);
			}
			foreach (Operator op in Operators)
			{
				foreach (string symbol in op.Symbols)
				{
					if (!sps.Contains(symbol))
						sps.Add(symbol);
				}
			}

			string ol = _line;
			string l = "";
			
			char? quote = null;
			int cpos = 0;
			foreach (char c in ol)
			{
				if (c == '"' || c == '\'')
				{
					if (quote == null)
						quote = c;
					else if (quote == c)
						quote = null;
				}
				if (quote.HasValue || c != ' ')
					l += c;

				if (_keeper != null && !quote.HasValue && c == ' ')
					_keeper.Remove(cpos, 1);
				else
					cpos++;
			}

			List<string> @out = new List<string>();
			string cur = "";
			for (int x = 0; x < l.Length; x++)
			{
				if (l[x] == '"' || l[x] == '\'')
				{
					for (int n = x + 1; n < l.Length; n++)
					{
						if (l[n] == l[x])
						{
							@out.Add(l.Substring(x, n - x + 1));
							x += n - x;
							break;
						}
					}
				}
				else
				{
					string sp = null;
					int lg = 0;
					foreach (string s in sps)
					{
						if (x + s.Length - 1 < l.Length && l.Substring(x, s.Length) == s && s.Length > lg)
						{
							sp = s;
							lg = s.Length;
						}
					}

					if (sp != null)
					{
						if (cur != sp && cur != "")
							@out.Add(cur);
						@out.Add(sp);
						cur = "";

						x += lg - 1;
					}
					else
						cur += l[x];
				}
			}
			if (cur != "")
				@out.Add(cur);
			
			return @out;
		}

		internal static List<string> DeBracketize(List<string> _parts, CodeKeeper _keeper)
		{
			List<string> starts = Parser.Brackets.Select(item => item.Start).ToList();
			List<string> ends = Parser.Brackets.Select(item => item.End).ToList();
			
			while (true)
			{
				bool b = true;
				int pos = -1;
				int bs = 0;

				for (int i = 0; i < _parts.Count; i++)
				{
					if (starts.Contains(_parts[i]))
					{
						pos = i;
						bs++;
					}
					else if (ends.Contains(_parts[i]))
					{
						bs--;
						if (pos != -1)
						{
							if (Brackets.Any(item => item.End == _parts[i]))
							{
								Brackets br = Brackets.First(item => item.End == _parts[i]);

								List<string> ps = new List<string>();

								List<string> left = _parts.GetRange(0, pos);
								List<string> middle = _parts.GetRange(pos + 1, i - pos - 1);
								List<string> right = _parts.GetRange(i + 1, _parts.Count - i - 1);

								int sumleft = left.Sum(item => item.Length);
								int summiddle = middle.Sum(item => item.Length);
								int sumright = right.Sum(item => item.Length);

								ps.AddRange(left);

								ps.Add("_" + br.Name);
								ps.Add("(");
								ps.AddRange(middle);
								ps.Add(")");

								ps.AddRange(right);

								_keeper.Remove(sumleft + 1 + summiddle, 1);
								_keeper.Insert(sumleft + 1 + summiddle, 1);
								_keeper.Remove(sumleft, 1);
								_keeper.Insert(sumleft, 2 + br.Name.Length);

								_parts = ps;

								b = false;
								bs = 0;
								break;
							}
						}
						else
							throw new Exception("Too many brackets of type \"" + _parts[i] + "\".");
					}
				}

				if (b)
				{
					if (bs > 0)
						throw new Exception("Too many brackets of type \"" + _parts[pos] + "\".");
					break;
				}
			}
			
			return _parts;
		}

		internal static List<string> ToFuncs(List<string> _parts)
		{
			List<string> ps = _parts;

			for (int m = Operators.Max(item => item.Priority); m >= 0; m--)
			{
				while (true)
				{
					bool b = true;

					for (int i = 0; i < ps.Count; i++)
					{
						b = true;
						if (Operators.Any(item => item.Symbols[0] == ps[i] && item.Priority == m))
						{
							List<List<string>> pms = new List<List<string>>();

							List<Operator> opers = Operators.Where(item => item.Symbols[0] == ps[i] && item.Priority == m).ToList();
							bool hasleft = i > 0 && !Operators.Where(item => !opers.Contains(item)).Any(item => item.Symbols.Any(sym => sym == ps[i - 1])) && ps[i - 1] != "," && ps[i - 1] != "(" && ps[i - 1] != ";";
							bool hasright = i < ps.Count - 1 && !Operators.Any(item => !item.Left && item.Symbols[0] == ps[i + 1]) && ps[i + 1] != ")" && ps[i + 1] != ";";

							if (opers.Any(item => item.Left == hasleft && item.Right == hasright))
							{
								List<string> left = new List<string>();
								if (hasleft)
								{
									if (ps[i - 1] != ")")
										left.Add(ps[i - 1]);
									else
									{
										int c = 0;
										for (int n = i - 1; n >= 0; n--)
										{
											if (ps[n] == ")")
												c++;
											if (ps[n] == "(")
												c--;

											if (c == 0)
											{
												int s;
												if (n > 0 && ps[n - 1].IsExpression())
													s = n - 1;
												else
													s = n;

												for (int k = s; k < i; k++)
													left.Add(ps[k]);
												break;
											}
										}
									}
									pms.Add(left);
								}

								int right = 0;

								Action<int> readright = x =>
									{
										List<string> @out = new List<string>();
										if (ps[x + 1] != ":")
										{
											bool ls = Operators.Any(item => !item.Left && item.Symbols[0] == ps[x + 1]);
											if ((ps[x + 1] != "(" && !ps[x + 1].IsExpression() && !ls) || (ps[x + 1].IsExpression() && (x + 2 >= ps.Count || ps[x + 2] != "(")))
												@out.Add(ps[x + 1]);
											else
											{
												int c = 0;
												for (int n = x + 1; n < ps.Count; n++)
												{
													if (ps[n] == "(")
														c++;
													if (ps[n] == ")")
														c--;

													if (c == 0 && (ps[n] == ")" || (ls && (ps[n].IsFunction() || !Operators.Any(item => item.Symbols[0] == ps[n])))))
													{
														for (int k = x + 1; k <= n; k++)
															@out.Add(ps[k]);
														break;
													}
												}
											}
											right += @out.Count;
											pms.Add(@out);
										}
									};

								if (hasright)
									readright(i);

								opers = opers.Where(item => item.Left == hasleft && item.Right == hasright).ToList();

								int cur = 1;
								List<Operator> secs = opers;
								while (secs.Any(item => item.Symbols.Length >= cur))
								{
									secs = secs.Where(item => i + right + 1 < ps.Count && item.Symbols.Length > cur && item.Symbols[cur] == ps[i + right + 1]).ToList();
									if (secs.Count > 0)
									{
										opers = secs;
										readright(i + right + 1);
										right++;
										cur++;
									}
								}

								List<string> l = new List<string>();
								for (int n = 0; n < i - left.Count; n++)
									l.Add(ps[n]);


								l.AddRange(opers[0].GetParts(pms.ToArray()));


								for (int n = i + right + 1; n < ps.Count; n++)
									l.Add(ps[n]);
								ps = l;

								b = false;
							}
						}
						if (!b)
							break;
					}
					
					if (b)
						break;
				}
			}

			while (true)
			{
				bool b = false;
				for (int i = 0; i < ps.Count; i++)
				{
					if (ps[i] == "(" && (i == 0 || !ps[i - 1].IsExpression()))
					{
						b = true;

						List<string> ss = new List<string>();
						for (int n = 0; n < i; n++)
							ss.Add(ps[n]);

						int tabs = 0;
						for (int n = i + 1; n < ps.Count; n++)
						{
							if (ps[n] == "(")
								tabs++;
							if (ps[n] == ")")
								tabs--;

							if (tabs >= 0)
								ss.Add(ps[n]);
							else
							{
								for (int x = n + 1; x < ps.Count; x++)
									ss.Add(ps[x]);
								break;
							}
						}

						ps = ss;
						break;
					}
				}
				if (!b)
					break;
			}
			
			return ps;
		}

		internal static Func Parse(List<string> _parts, int _tabs, Func _parent)
		{
			if (_parts.Count == 1)
				return new Func(_parts[0], null) { Parent = _parent };
			else if (_parts.Count == 0)
				return null;
			else
			{
				int o = 0;
				string name = null;
				if (_parts[0] != "(")
				{
					name = _parts[0];
					o++;
				}
				
				List<List<string>> param = new List<List<string>>();

				int start = o + 1;
				int c = 0;
				for (int i = o + 1; i < _parts.Count; i++)
				{
					if (_parts[i] == "(")
						c++;
					if (_parts[i] == ")")
						c--;

					if (_parts[i] == "," && c == 0)
					{
						List<string> p = new List<string>();
						for (int n = start; n < i; n++)
							p.Add(_parts[n]);
						param.Add(p);
						start = i + 1;
					}

					if (_parts[i] == ")" && c == -1)
					{
						if (start != i)
						{
							List<string> p = new List<string>();
							for (int n = start; n < i; n++)
								p.Add(_parts[n]);
							param.Add(p);
						}
					}
				}

				Func f = new Func(name, new Func[0]) { Parent = _parent };
				f.Parameters = param.Select(item => Parse(item, _tabs + 1, f)).ToArray();
				return f;
			}
		}

		internal static Func Parse(string _code)
		{
			if (_code != null && _code != "")
			{
				CodeKeeper keeper;
				string c = Prepare(_code, out keeper);

				List<string> fs = ToFuncs(DeBracketize(Split(c, keeper), keeper));
				return Parse(fs, 0, null);
			}
			else
				return new Func("_nopelamb", new Func[0]);
		}
	}
}