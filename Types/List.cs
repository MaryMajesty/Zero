using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class List
	{
		public List<object> Objects;
		//public bool IsString;

		public int Count
		{
			get { return this.Objects.Count; }
		}
		public object this[int _index]
		{
			get { return this.Objects[_index]; }
			set { this.Objects[_index] = value; }
		}
		public bool MightBeString
		{
			get { return this.Objects.TrueForAll(item => item is double); }
		}

		public List(params object[] _objects) { this.Objects = _objects.ToList(); }

		public void Add(object _object)
		{
			this.Objects.Add(_object);
			//if (!(_object is double) || (double)_object >= 256 * 256)
			//	this.IsString = false;
		}

		public void AddRange(List _list)
		{
			this.Objects.AddRange(_list.Objects);
			//foreach (object o in _list.Objects)
			//	this.Add(o);
		}

		public void Remove(object _object)
		{
			this.Objects.Remove(_object);
		}

		public static List From(IEnumerable<object> _list/*, bool _isstring = false*/) { return new List() { Objects = new List<object>(_list)/*, IsString = _isstring*/ }; }
		public static List From(string _string) { return List.From(_string.Select(item => Convert.ToZero(item))/*, _isstring: true*/); }

		public List Reverse() { return List.From(this.Objects.Reverse<object>().ToArray()); }
		public bool Any(Func<object, bool> _func) { return this.Objects.Any(item => _func(item)); }
		public bool All(Func<object, bool> _func) { return this.Objects.TrueForAll(item => _func(item)); }
		public List Select(Func<object, object> _func) { return List.From(this.Objects.Select(item => _func(item)).ToList()); }
		public List Where(Func<object, bool> _func) { return List.From(this.Objects.Where(item => _func(item)).ToList()); }
		public bool Contains(object _item) { return this.Objects.Contains(_item); }
		public List Sub(int _start, int _length) { return List.From(this.Objects.GetRange(_start, _length)); }
		public List OrderBy(Func<object, double> _func) { return List.From(this.Objects.OrderBy(item => _func(item))); }
		public object[] ToArray() { return this.Objects.ToArray(); }
		public int CountNumber(Func<object, bool> _func) => this.Objects.Count(item => _func(item));

		public bool ContainsRange(List _range)
		{
			if (_range.Count == 0)
				return true;
			else
			{
				for (int i = 0; i <= this.Count - _range.Count; i++)
				{
					for (int x = 0; x < _range.Count; x++)
					{
						if (!this[i + x].Equals(_range[x]))
							break;
						else if (x == _range.Count - 1)
							return true;
					}
				}
				return false;
			}
		}

		public List Clone()
		{
			object[] os = new object[this.Count];
			this.Objects.CopyTo(os);
			return List.From(os);
		}

		public bool EndsWith(List _list)
		{
			if (this.Count >= _list.Count)
				return this.Sub(this.Count - _list.Count, _list.Count).Equals(_list);
			else
				return false;
		}

		public bool StartsWith(List _list)
		{
			if (this.Count >= _list.Count)
				return this.Sub(0, _list.Count).Equals(_list);
			else
				return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is List)
			{
				List l = (List)obj;
				if (this.Count == l.Count)
				{
					for (int i = 0; i < this.Count; i++)
					{
						if (!this[i].Equals(l[i]))
							return false;
					}
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		public string ToLiteral() => string.Join("", this.Select(item => item.AsChar()).ToArray());

		public string Escape()
		{
			string s = this.ToLiteral();
			string @out = "";
			while (s.Length > 0)
			{
				Tuple<string, char> t = EscapeCharacter.GetFirstChar(s);
				s = t.Item1;
				@out += t.Item2;
			}
			return @out;
		}

		public string Insert(Comp _vars)
		{
			string @out = "";
			string name = "";
			bool inname = false;

			foreach (char c in this.ToLiteral())
			{
				if (!inname)
				{
					if (c == '{')
						inname = true;
					else
						@out += c;
				}
				else
				{
					if (c == '}')
					{
						@out += ((List)_vars[name]).ToLiteral();
						inname = false;
						name = "";
					}
					else
						name += c;
				}
			}

			return @out;
		}
	}
}