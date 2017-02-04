using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	internal static class EscapeCharacter
	{
		public static Dictionary<char, char> Table = new Dictionary<char, char>() { { 't', '\t' }, { 'q', '"' }, { 'c', '\'' }, { 'n', '\n' }, { '\\', '\\' }, { '|', '|' } };

		public static Tuple<string, char> GetFirstChar(string _text)
		{
			if (_text[0] == '\\')
			{
				if (Table.ContainsKey(_text[1]))
					return new Tuple<string, char>(_text.Substring(2), Table[_text[1]]);
				else
				{
					string cs = "0123456789";
					string ncs = cs + "_";

					string num = "";
					bool b = false;

					_text = _text.Substring(1);
					if (cs.Contains(_text[0]))
					{
						while (_text.Length > 0 && ncs.Contains(_text[0]))
						{
							num += _text[0];
							_text = _text.Substring(1);
						}
						if (num.Length > 0)
						{
							b = true;

							if (_text.Length > 0 && _text[0] == '|')
								_text = _text.Substring(1);
						}
					}

					if (b)
					{
						try { return new Tuple<string, char>(_text, (char)(ushort)Ext.StringToDouble(num)); }
						catch { throw Error.Syntax.InvalidEscapeSequence(num); }
					}
					else
						throw Error.Syntax.InvalidEscapeCharacter(_text[1]);
				}
			}
			else
				return new Tuple<string, char>(_text.Substring(1), _text[0]);
		}
	}
}