using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
	public class NameAttribute : Attribute
	{
		public string Name;
		public NameAttribute(string _name) { this.Name = _name; }
	}
}