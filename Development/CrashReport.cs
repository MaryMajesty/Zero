using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class CrashReport
	{
		public readonly Func Func;
		public readonly Scope Scope;
		public readonly Error Error;

		internal CrashReport(Func _func, Scope _scope, Error _error)
		{
			this.Func = _func;
			this.Scope = _scope;
			this.Error = _error;
		}

		internal static string[] GetVarListing(List<Variable> _vars)
		{
			List<string> @out = new List<string>();
			foreach (Variable var in _vars)
				@out.Add(var.Name + GetVarListing(var.Value));
			return @out.ToArray();
		}

		internal static string GetVarListing(object _object)
		{
			string value = null;

			if (_object is Comp)
				value = "\n" + string.Join("\n", CrashReport.GetVarListing(((Comp)_object)._Variables.Select(item => item.Value).ToList()).Select(item => "\t" + item));
			else if (_object is List)
			{
				object[] os = ((List)_object).ToArray();
				if (os.ToList().TrueForAll(item => item is double && (double)item >= 0 && (double)item <= ushort.MaxValue && (double)item % 1 == 0))
					value = '"' + ((List)_object).ToLiteral() + '"';
				else
					value = "";

				string[] ss = os.Select(item => CrashReport.GetVarListing(item)).ToArray();
				for (int i = 0; i < ss.Length; i++)
					ss[i] = "\t" + i.ToString() + ss[i];

				value += "\n" + string.Join("\n", ss);
			}
			else if (_object != null)
				value = _object.ToString();
			
			return ((value == null) ? ";" : (": " + value));
		}
	}
}