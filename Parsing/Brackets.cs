using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	internal class Brackets
	{
		public string Name;
		public string Start;
		public string End;

		public Brackets(string _name, string _start, string _end)
		{
			this.Name = _name;
			this.Start = _start;
			this.End = _end;
		}
	}
}