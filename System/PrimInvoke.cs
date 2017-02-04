using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	class PrimInvoke
	{
		public object Function;
		public object FirstParameter;

		public PrimInvoke(object _function, object _firstparameter)
		{
			this.Function = _function;
			this.FirstParameter = _firstparameter;
		}
	}
}