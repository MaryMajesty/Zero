using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public static class Helper
	{
		public static string[] ScopeOperatorSymbols
		{
			get
			{
				List<string> @out = new List<string>();
				foreach (Operator op in Parser.Operators)
				{
					if (op.Symbols.Contains(":"))
					{
						foreach (string sym in op.Symbols)
						{
							if (!@out.Contains(sym))
								@out.Add(sym);
						}
					}
				}
				return @out.ToArray();
			}
		}
		public static string[] OneLineScopeOperators
		{
			get
			{
				List<string> @out = new List<string>();
				foreach (Operator op in Parser.Operators)
				{
					for (int i = 1; i < op.Symbols.Length; i++)
					{
						if (op.Symbols[i] == ":")
						{
							string sym = op.Symbols[i - 1] + ":";
							if (!@out.Contains(sym))
								@out.Add(sym);
						}
					}
				}
				return @out.ToArray();
			}
		}
		public static string[] NoScopeOperatorSymbols
		{
			get
			{
				List<string> @out = new List<string>();
				foreach (Operator op in Parser.Operators)
				{
					string s = op.Symbols[0];
					if ((op.Left || op.Right) && (s.Length != 2 || s[0] != s[1]))
					{
						foreach (string sym in op.Symbols)
						{
							if (!@out.Contains(sym))
								@out.Add(sym);
						}
					}
				}
				return @out.ToArray();
			}
		}
		public static string[] ObjectOperators
		{
			get
			{
				List<string> @out = new List<string>() { "$" };
				foreach (Operator op in Parser.Operators.Where(item => !item.Left && !item.Right && item.Symbols[0] != "!!!"))
					@out.Add(op.Symbols[0]);
				return @out.ToArray();
			}
		}
		public static string[] FlowOperators
		{
			get
			{
				List<string> @out = new List<string>();
				foreach (Operator op in Parser.Operators)
				{
					foreach (string symbol in op.Symbols)
					{
						if (symbol.Length == 3 && symbol[0] == symbol[1] && symbol[1] == symbol[2] && !@out.Contains(symbol))
							@out.Add(symbol);
					}
				}
				return @out.ToArray();
			}
		}
		public static string[] Symbols
		{
			get
			{
				List<string> @out = new List<string>() { "$", "(", ")", "{", "}", "[", "]" };
				foreach (Operator op in Parser.Operators)
				{
					foreach (string s in op.Symbols)
					{
						if (!@out.Contains(s))
							@out.Add(s);
					}
				}
				return @out.ToArray();
			}
		}

		public static bool NeedsIndent(string _line)
		{
			List<string> ps = Parser.Split(_line, null);

			int colons = ps.Count(item => item == ":");
			int ops = ps.Count(item => ScopeOperatorSymbols.Contains(item) && item != ":");
			//int ops = 0;
			//foreach (Operator op in Parser.Operators)
			//{
			//	if (op.Symbols.Length % 2 == 0)
			//	{
			//		for (int i = 0; i < op.Symbols.Length; i += 2)
			//		{
			//			if (
			//		}
			//	}
			//}
			//int ops = ps.Count(item => Parser.Operators.Any(op => op.Symbols[0] == item && op.Symbols.Contains(":")));

			return (ops > colons);
		}

		public static int GetIndents(string _line)
		{
			int @out = 0;
			while (_line.Length > 0 && _line[0] == '\t')
			{
				_line = _line.Substring(1);
				@out++;
			}
			return @out;
		}

		public static int GetNextIndents(string _line)
		{
			return GetIndents(_line) + (NeedsIndent(_line) ? 1 : 0);
		}
	}

	//public struct Var
	//{
	//	public string Name;
	//	public Var[] Children;
	//}
}