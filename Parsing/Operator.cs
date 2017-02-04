using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	internal class Operator
	{
		public string[] Symbols;
		public string Name;
		public int Priority;
		public bool Left;
		public bool Right;
		public bool IncludeEmptyLeft;
		public bool Assignable;
		public bool Selectable;
		public Func<string[], string[]> Func;

		public Operator(string _name, int _priority, bool _left, bool _right, params string[] _symbols)
		{
			this.Name = _name;
			this.Priority = _priority;
			this.Left = _left;
			this.Right = _right;
			this.Symbols = _symbols;
		}

		//public Operator(string _name, int _priority, bool _left, bool _right, params string[] _symbols) : this(_name, _priority, _left, _right, _symbols) { }
		//public Operator(string _name, int _priority, bool _left, params string[] _symbols) : this(_name, _priority, _left, true, _symbols) { }
		public Operator(string _name, int _priority, params string[] _symbols) : this(_name, _priority, true, true, _symbols) { }

		public virtual string[] GetParts(List<string>[] _params)
		{
			List<string> @out = new List<string>();
			@out.Add("_" + this.Name);
			@out.Add("(");
			if (!this.Left && this.IncludeEmptyLeft)
				@out.Add(",");
			for (int i = 0; i < _params.Length; i++)
			{
				if (i > 0)
					@out.Add(",");
				@out.AddRange(_params[i]);
			}
			@out.Add(")");
			return @out.ToArray();
		}
	}

	internal class ShortOperator : Operator
	{
		public ShortOperator(string _name, int _priority, string _symbol) : base(_name, _priority, _symbol) { }

		public override string[] GetParts(List<string>[] _params)
		{
			List<string> @out = new List<string>();
			@out.Add("_invoke");
			@out.Add("(");

			@out.Add("_dot");
			@out.Add("(");

			@out.AddRange(_params[0]);
			@out.Add(",");
			@out.Add(this.Name);

			@out.Add(")");

			@out.Add(",");

			@out.Add("_list");
			@out.Add("(");

			//for (int n = 1; n < _params.Length; n++)
			//{
			//	if (n > 1)
			//		ss.Add(",");
			@out.AddRange(_params[1]);
			//}

			@out.Add(")");
			@out.Add(")");

			return @out.ToArray();
		}
	}

	internal class OrOperator : Operator
	{
		public Operator[] Operators;
		public OrOperator(string _symbol, Operator _op0, Operator _op1) : base("", _op0.Priority, _op0.Left, _op0.Right, _symbol) { this.Operators = new Operator[] { _op0, _op1 }; }

		public override string[] GetParts(List<string>[] _params)
		{
			List<string> @out = new List<string>();
			@out.Add("_invoke");
			@out.Add("(");

			@out.Add("_dot");
			@out.Add("(");

			@out.AddRange(this.Operators[0].GetParts(_params));
			@out.Add(",");
			@out.Add("or");
			@out.Add(")");
			@out.Add(",");

			@out.Add("_list");
			@out.Add("(");
			@out.AddRange(this.Operators[1].GetParts(_params));
			@out.Add(")");

			@out.Add(")");

			return @out.ToArray();
		}
	}
	
	internal class NotEqualsOperator : Operator
	{
		public NotEqualsOperator(int _priority, string _symbol) : base("", _priority, _symbol) { }

		public override string[] GetParts(List<string>[] _params)
		{
			Operator equals = new ShortOperator("equals", 1, "?=");

			List<string> @out = new List<string>();
			@out.Add("_not");
			@out.Add("(");
			@out.AddRange(equals.GetParts(_params));
			@out.Add(")");

			return @out.ToArray();
		}
	}

	internal class SetNotOperator : Operator
	{
		public SetNotOperator(int _priority, string _symbol) : base("", _priority, true, false, _symbol) { }

		public override string[] GetParts(List<string>[] _params)
		{
			List<string> @out = new List<string>();

			@out.Add("_set");

			@out.Add("(");
			@out.AddRange(_params[0]);
			@out.Add(",");

			@out.Add("_not");
			@out.Add("(");
			@out.AddRange(_params[0]);
			@out.Add(")");

			@out.Add(")");

			return @out.ToArray();
		}
	}

	internal class SelectOperator : Operator
	{
		public SelectOperator(int _priority, string _symbol, string _name = "") : base(_name, _priority, _symbol) { }

		public override string[] GetParts(List<string>[] _params)
		{
			//_invoke(_dot(_arr("one","two"),select),_arr(_lamb(item,_invoke(_dot(item,add),_arr('n')

			List<string> @out = new List<string>();

			@out.AddRange("_invoke ( _dot (".Split(' '));
			@out.AddRange(_params[0]);
			@out.AddRange(", select ) , _list (".Split(' '));
			@out.AddRange(_params[1]);
			@out.Add(")");
			@out.Add(")");

			return @out.ToArray();
		}
	}

	internal class ShortSelectOperator : SelectOperator
	{
		public Operator Operator;

		public ShortSelectOperator(Operator _operator, int _priority)
			: base(_priority, "\\" + _operator.Symbols[0])
		{
			this.Operator = _operator;
		}

		public override string[] GetParts(List<string>[] _params)
		{
			List<string> @out = new List<string>();

			@out.AddRange("_lamb ( item , _invoke (".Split(' '));

			@out.Add("_dot");
			@out.Add("(");
			@out.Add("item");
			@out.Add(",");
			@out.Add(this.Operator.Name);
			@out.Add(")");

			@out.Add(",");

			@out.Add("_list");
			@out.Add("(");
			@out.AddRange(_params[1]);
			@out.Add(")");

			@out.Add(")");
			@out.Add(")");

			return base.GetParts(new List<string>[] { _params[0], @out });
		}
	}

	internal class AssignOperator : Operator
	{
		public Operator Operator;

		public AssignOperator(Operator _operator, int _priority)
			: base("", _priority, _operator.Symbols[0] + "=")
		{
			this.Operator = _operator;
		}

		public override string[] GetParts(List<string>[] _params)
		{
			List<string> @out = new List<string>();
			@out.Add("_set");
			@out.Add("(");
			@out.AddRange(_params[0]);
			@out.Add(",");

			@out.AddRange(this.Operator.GetParts(_params));

			@out.Add(")");

			return @out.ToArray();
		}
	}

	internal class NegativeOperator : Operator
	{
		public NegativeOperator(int _priority, string _symbol) : base("", _priority, false, true, _symbol) { }

		public override string[] GetParts(List<string>[] _params)
		{
			ShortOperator op = new ShortOperator("multiply", 1, "");
			return op.GetParts(new List<string>[] { _params[0], new List<string>() { "-1" } });
		}
	}
}