using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace Zero
{
	public static class Libraries
	{
		public static class Types
		{
			public static List<Variable> All
			{
				get { return new List<Variable>() { List, Num, Wave, Func }; }
			}

			public static Variable List
			{
				get
				{
					Comp @out = new Comp
						(
							new Variable("Add", 2, (s, ps) =>
								{
									List l = ((List)ps[0]).Clone();
									l.Add(ps[1]);
									return l;
								}),
							new Variable("Subtract", 2, (s, ps) =>
								{
									List l = ((List)ps[0]).Clone();
									l.Remove(ps[1]);
									return l;
								}),
							new Variable("Multiply", 2, (s, ps) =>
								{
									List l = new List();
									for (int i = 1; i < (double)ps[1]; i++)
										l.AddRange(((List)ps[0]));
									return l;
								}),

							new Variable("And", 2, (s, ps) =>
								{
									List l = ((List)ps[0]).Clone();
									l.AddRange((List)ps[1]);
									return l;
								}),
							new Variable("Or", 2, (s, ps) =>
								{
									List l = ((List)ps[0]).Clone();
									l.AddRange(((List)ps[1]).Where(item => !l.Contains(item)));
									return l;
								}),

							new Variable("Amount", 1, (s, ps) => (double)((List)ps[0]).Count),
							new Variable("Count", 2, (s, ps) => (double)((List)ps[0]).CountNumber(item => ((Lambda)ps[1]).Execute(new List(item)).AsBool())),
							new Variable("Last", 1, (s, ps) => ((List)ps[0])[((List)ps[0]).Count - 1]),
							new Variable("Sub", 3, (s, ps) => ((List)ps[0]).Sub(ps[1].AsInt(), ps[2].AsInt())),

							new Variable("Equals", 2, (s, ps) => ps[0].Equals(ps[1]).AsDouble()),
							new Variable("Contains", 2, (s, ps) => ((List)ps[0]).ContainsRange((List)ps[1]).AsDouble()),
							new Variable("All", 2, (s, ps) => ((List)ps[0]).All(item => (((Lambda)ps[1]).Execute(new List(item))).AsBool()).AsDouble()),
							new Variable("Any", 2, (s, ps) => ((List)ps[0]).Any(item => (((Lambda)ps[1]).Execute(new List(item))).AsBool()).AsDouble()),

							new Variable("Select", 2, (s, ps) => ((List)ps[0]).Select(item => ((Lambda)ps[1]).Execute(new List(item)))),
							new Variable("Where", 2, (s, ps) => ((List)ps[0]).Where(item => (((Lambda)ps[1]).Execute(new List(item))).AsBool())),
							new Variable("OrderBy", 2, (s, ps) => ((List)ps[0]).OrderBy(item => (double)((Lambda)ps[1]).Execute(new List(item)))),
							new Variable("OrderByDescending", 2, (s, ps) => ((List)ps[0]).OrderBy(item => (double)((Lambda)ps[1]).Execute(new List(item))).Reverse()),
							new Variable("Reverse", 1, (s, ps) => ((List)ps[0]).Reverse()),

							new Variable("Sum", 2, (s, ps) => ((List)((List)ps[0]).Select(item => ((Lambda)ps[1]).Execute(new List(item)))).Objects.Sum(item => (double)item)),

							new Variable("Remove", 2, (s, ps) =>
								{
									List l = ((List)ps[0]).Clone();
									l.Remove(ps[1]);
									return l;
									//List<object> os = ((List)ps[0]).Objects.ToArray().ToList();
									//os.Remove(ps[1]);
									//return global::Zero.List.From(os);
								}),
							new Variable("Remove_At", 2, (s, ps) =>
								{
									List l = (List)ps[0];
									int p = ps[1].AsInt();

									List nl = new List();
									nl.AddRange(l.Sub(0, p));
									nl.AddRange(l.Sub(p + 1, l.Count - p - 1));
									return nl;
								}),

							new Variable("StartsWith", 2, (s, ps) => ((List)ps[0]).StartsWith((List)ps[1]).AsDouble()),
							new Variable("EndsWith", 2, (s, ps) => ((List)ps[0]).EndsWith((List)ps[1]).AsDouble()),

							new Variable("Escaped", 1, (s, ps) => Zero.List.From(((List)ps[0]).Escape())),
							new Variable("Insert", 2, (s, ps) => Zero.List.From(((List)ps[0]).Insert((Comp)ps[1]))),
							new Variable("Is", 1, (s, ps) => (ps[0] is List).AsDouble())
						);

					return new Variable("List", @out, _external: false);
				}
			}

			public static Variable Num
			{
				get
				{
					Comp @out = new Comp
						(
							new Variable("Add", 2, (s, ps) => (double)ps[0] + (double)ps[1]),
							new Variable("Subtract", 2, (s, ps) => (double)ps[0] - (double)ps[1]),
							new Variable("Multiply", 2, (s, ps) => (double)ps[0] * (double)ps[1]),
							new Variable("Divide", 2, (s, ps) => (double)ps[0] / (double)ps[1]),
							new Variable("Modulate", 2, (s, ps) => (double)ps[0] % (double)ps[1]),
							new Variable("Exponentiate", 2, (s, ps) => System.Math.Pow((double)ps[0], (double)ps[1])),

							new Variable("And", 2, (s, ps) => (ps[0].AsBool() && ps[1].AsBool()).AsDouble()),
							new Variable("Or", 2, (s, ps) => (ps[0].AsBool() || ps[1].AsBool()).AsDouble()),

							new Variable("Equals", 2, (s, ps) => ((double)ps[0] == (double)ps[1]).AsDouble()),
							new Variable("LessThan", 2, (s, ps) => ((double)ps[0] < (double)ps[1]).AsDouble()),
							new Variable("GreaterThan", 2, (s, ps) => ((double)ps[0] > (double)ps[1]).AsDouble()),

							new Variable("Parse", 1, (s, ps) => Ext.StringToDouble(((List)ps[0]).ToLiteral())),
							new Variable("AsString", 1, (s, ps) => global::Zero.List.From(ps[0].ToString())),
							new Variable("AsPercent", 1, (s, ps) =>
								{
									double v = (double)ps[0] * 100;
									return global::Zero.List.From(((int)Math.Round(v)).ToString("D3") + "%");
								}),

							new Variable("Is", 1, (s, ps) => (ps[0] is double).AsDouble())
						);

					return new Variable("Num", @out, _external: false);
				}
			}

			public static Variable Wave
			{
				get
				{
					Comp @out = new Comp
						(
							new Variable("Finished", 1, (s, ps) => ((Wave)ps[0]).Finished ? 1.0 : 0.0),
							new Variable("Is", 1, (s, ps) => (ps[0] is Wave).AsDouble())
						);
					return new Variable("Wave", @out, _external: false);
				}
			}

			public static Variable Func
			{
				get
				{
					Comp @out = new Comp
						(
							//new Variable("Add", 2, (s, ps) =>
							//	{
							//		Lambda l0 = (Lambda)ps[0];
							//		Lambda l1 = (Lambda)ps[1];
							//		return new Lambda(
							//	})
							new Variable("Is", 1, (s, ps) => (ps[0] is Lambda).AsDouble())
						);
					return new Variable("Func", @out, _external: false);
				}
			}
		}

		//public static Variable Zero
		//{
		//	get
		//	{
		//		Comp @out = new Comp
		//			(
		//				new Variable("var", new Comp
		//					(
		//						new Variable("new", 2, (s, ps) => new Variable(((List)ps[0]).ToLiteral(), ps[1], _external: false))
		//					), _external: false),
		//				new Variable("null", () => new Null(), null, _external: false)
		//			);
		//		return new Variable("zero", @out, _external: false);
		//	}
		//}

		public static class Default
		{
			public static List<Variable> All
			{
				get { return new List<Variable>() { Math, IO, new Variable("time", 0, (s, ps) => (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds) }; }
			}

			public static Variable Math
			{
				get
				{
					Comp @out = new Comp
						(
							new Variable("Sin", 1, (s, ps) => System.Math.Sin((double)ps[0])),
							new Variable("Cos", 1, (s, ps) => System.Math.Cos((double)ps[0])),
							new Variable("Sqrt", 1, (s, ps) => System.Math.Sqrt((double)ps[0])),
							new Variable("Abs", 1, (s, ps) => System.Math.Abs((double)ps[0])),
							new Variable("Tau", () => System.Math.PI * 2, null, _external: false),
							new Variable("Round", 1, (s, ps) => System.Math.Round((double)ps[0])),
							new Variable("Round_Down", 1, (s, ps) => System.Math.Floor((double)ps[0])),
							new Variable("Round_Up", 1, (s, ps) => System.Math.Ceiling((double)ps[0]))
						);

					return new Variable("Math", @out, _external: false) { _Internal = true };
				}
			}

			public static Variable IO
			{
				get
				{
					Comp @out = new Comp
						(
							new Variable("Read_Text", 1, (s, ps) => global::Zero.List.From(System.IO.File.ReadAllText(s.Program._GetPath((List)ps[0])))),
							new Variable("Read_Lines", 1, (s, ps) =>
							{
								List l = new List();
								foreach (string st in System.IO.File.ReadAllLines(s.Program._GetPath((List)ps[0])))
									l.Add(global::Zero.List.From(st));
								return l;
							}),
							new Variable("GetFiles", 1, (s, ps) => global::Zero.List.From(System.IO.Directory.GetFiles(s.Program._GetPath((List)ps[0])).Select(item => global::Zero.List.From(item)))),
							new Variable("GetFiles_All", 1, (s, ps) => global::Zero.List.From(System.IO.Directory.GetFiles(s.Program._GetPath((List)ps[0]), "*", System.IO.SearchOption.AllDirectories).Select(item => global::Zero.List.From(item)))),
							new Variable("Write_Text", 2, (s, ps) => System.IO.File.WriteAllText(((List)ps[0]).ToLiteral(), ((List)ps[1]).ToLiteral()))
						);
					return new Variable("IO", @out, _external: false) { _Internal = true };
				}
			}
		}



		internal static Comp _GetComp(params Variable[] _vars)
		{
			Comp @out = new Comp();

			foreach (Variable var in _vars.Where(item => !item._Internal))
				var.MakeExternal();
			foreach (Variable var in _vars)
				@out.AddVariable(var);
			foreach (Variable var in Types.All)
				@out.AddVariable(var);

			return @out;
		}
	}
}