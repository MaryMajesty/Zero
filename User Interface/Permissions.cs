using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Permissions
	{
		internal Program _Program;
		internal List<string> _Perms = new List<string>();

		public Permissions(params Permission[] _permissions)
		{
			foreach (Permission p in _permissions)
			{
				foreach (string perm in p._Perms)
				{
					if (!this._Perms.Contains(perm))
						this._Perms.Add(perm);
				}
			}
		}

		public static Permissions None
		{
			get { return new Permissions(); }
		}
		public static Permissions Local
		{
			get { return new Permissions(Permission.LocalFileAccess, Permission.PermissionRequests); }
		}
		public static Permissions All
		{
			get { return new Permissions(Permission.GlobalFileAccess, Permission.DllUsage); }
		}

		internal bool _Check(Permission _permission) => _permission._Perms.TrueForAll(item => this._Perms.Contains(item));

		internal bool _Request(Permission _permission)
		{
			if (this._Check(_permission))
				return true;
			else
			{
				if (_permission._Perms.Contains("access subscripts") && this._Program.ScriptPath == null)
					throw Error.Permission.ScriptNotLocal;
				else if (this._Check(Permission.PermissionRequests) && this._Program.OnPermissionRequest(_permission))
				{
					foreach (string perm in _permission._Perms)
					{
						if (!this._Perms.Contains(perm))
							this._Perms.Add(perm);
					}
					return true;
				}
				else
					throw Error.Permission.PermissionDenied(_permission);
			}
		}
	}

	public class Permission
	{
		public readonly string Description;
		internal List<string> _Perms = new List<string>();

		internal Permission(string _description, params string[] _perms)
		{
			this.Description = _description;

			this._Perms.Add(_description);
			this._Perms.AddRange(_perms);
		}
		
		public static readonly Permission SubscriptAccess = new Permission("access subscripts");
		public static readonly Permission LocalFileAccess = new Permission("access local files", "access subscripts");
		public static readonly Permission GlobalFileAccess = new Permission("access global files", "access local files", "access subscripts");

		public static readonly Permission DllUsage = new Permission("use DLLs");
		//public static readonly Permission Multithreading = new Permission("use multiple threads");
		public static readonly Permission PermissionRequests = new Permission("request additional permissions");
	}
}