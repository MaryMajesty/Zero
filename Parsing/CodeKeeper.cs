using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	class CodeKeeper
	{
		public string Source;
		public int[] Relations;
		public List<Insert> Inserts = new List<Insert>();
		public bool Enabled = false;

		public CodeKeeper(string _source)
		{
			this.Source = _source;
			this.Relations = new int[_source.Length];
			for (int i = 0; i < _source.Length; i++)
				this.Relations[i] = i;
		}
		
		public void InsertLatent(int _position, int _length) => this.Inserts.Add(new Insert(_position, _length));
		public void RemoveLatent(int _position, int _length) => this.Inserts.Add(new Insert(_position, _length, _remove: true));

		public void UpdateLatents()
		{
			if (this.Enabled)
			{
				int p = 0;
				foreach (Insert i in this.Inserts.OrderBy(item => item.Position))
				{
					List<int> nr = new List<int>();
					for (int x = 0; x < p + i.Position; x++)
						nr.Add(this.Relations[x]);

					if (i.Remove)
					{
						for (int x = p + i.Position + i.Length; x < this.Relations.Length; x++)
							nr.Add(this.Relations[x]);

						p -= i.Length;
					}
					else
					{
						for (int x = 0; x < i.Length; x++)
							nr.Add(-1);
						for (int x = p + i.Position; x < this.Relations.Length; x++)
							nr.Add(this.Relations[x]);

						p += i.Length;
					}

					this.Relations = nr.ToArray();
				}
				this.Inserts.Clear();
			}
		}

		public void Insert(int _position, int _length)
		{
			this.InsertLatent(_position, _length);
			this.UpdateLatents();
		}

		public void Remove(int _position, int _length)
		{
			this.RemoveLatent(_position, _length);
			this.UpdateLatents();
		}

		public void Check(string _code)
		{
			if (this.Enabled)
			{
				string[] lines = this.Source.Split('\n');
				string f = this.Source + "\n\n";

				Func<int, Tuple<int, int>> getpos = pos =>
					{
						int y = 0;
						int x = 0;

						for (int i = 0; i < pos; i++)
						{
							if (x < lines[y].Length)
								x++;
							else
							{
								y++;
								x = 0;
							}
						}

						return new Tuple<int, int>(x, y);
					};

				for (int i = 0; i < this.Relations.Length; i++)
				{
					if (this.Relations[i] >= 0)
					{
						Tuple<int, int> p = getpos(this.Relations[i]);
						f += _code[i] + "(" + p.Item1.ToString() + "|" + p.Item2.ToString() + ") ";
					}
					else
						f += "_";
				}

				f += "\n\n" + _code;

				System.IO.File.WriteAllText("output.txt", f);
			}
		}
	}

	struct Insert
	{
		public bool Remove;
		public int Position;
		public int Length;

		public Insert(int _position, int _length, bool _remove = false)
		{
			this.Remove = _remove;
			this.Position = _position;
			this.Length = _length;
		}
	}
}