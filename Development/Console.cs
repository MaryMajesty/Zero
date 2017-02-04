using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Console
	{
		private List<string> _Lines = new List<string>();
		private bool _LineFinished = true;

		public void Write(string _text)
		{
			lock (this._Lines)
			{
				this._LineFinished = true;
				this._Lines.Add(" " + _text);
			}
		}

		public void WriteShort(string _text)
		{
			lock (this._Lines)
			{
				if (this._LineFinished)
				{
					this._LineFinished = false;
					this._Lines.Add(" " + _text);
				}
				else
					this._Lines[this._Lines.Count - 1] += _text;
			}
		}

		public void EndLine() => this._LineFinished = true;

		public void WriteOver(string _text)
		{
			lock (this._Lines)
			{
				if (this._LineFinished)
				{
					this._LineFinished = false;
					this._Lines.Add(" " + _text);
				}
				else
					this._Lines[this._Lines.Count - 1] = " " + _text;
			}
		}

		public void WriteUserInput(string _text)
		{
			lock (this._Lines)
			{
				this._LineFinished = true;
				this._Lines.Add(">" + _text);
			}
		}

		public void WriteUserInput(char _char)
		{
			lock (this._Lines)
			{
				this._LineFinished = true;
				this._Lines.Add(":" + _char.ToString());
			}
		}

		public string[] GetLines(string _outputindicator = "", string _inputindicator = "", string _charinputindicator = "")
		{
			lock (this._Lines)
			{
				List<string> @out = new List<string>();
				foreach (string line in this._Lines)
				{
					string l = line.Substring(1);
					if (line[0] == ' ')
						@out.Add(_outputindicator + l);
					else if (line[0] == '>')
						@out.Add(_inputindicator + l);
					else
						@out.Add(_charinputindicator + l);
				}
				return @out.ToArray();
			}
		}
	}
}