using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero
{
	public class Error : Exception
	{
		public readonly string ErrorType;
		public readonly int ErrorNumber;
		public readonly string ErrorMessage;

		public string ErrorCode
		{
			get { return this.ErrorType + " " + this.ErrorNumber.ToString(); }
		}

		public Error(string _type, int _errornumber, string _message)
			: base("[" + _type + " Error #" + _errornumber.ToString() + "] " + _message)
		{
			this.ErrorType = _type;
			this.ErrorNumber = _errornumber;
			this.ErrorMessage = _message;
		}

		public static class Types
		{
			public class Usage : Error { public Usage(int _errornumber, string _message) : base("Usage", _errornumber, _message) { } }
			public class Semantics : Error { public Semantics(int _errornumber, string _message) : base("Semantics", _errornumber, _message) { } }
			public class Syntax : Error { public Syntax(int _errornumber, string _message) : base("Syntax", _errornumber, _message) { } }
			public class Execution : Error { public Execution(int _errornumber, string _message) : base("Execution", _errornumber, _message) { } }
			public class Permission : Error { public Permission(int _errornumber, string _message) : base("Permission", _errornumber, _message) { } }
			public class Custom : Error { public Custom(int _errornumber, string _message) : base("Custom", _errornumber, _message) { } }
		}

		public static class Usage
		{
			public static Error.Types.Usage IterCountNotEqual => new Error.Types.Usage(0, "The number of variables and objects to be iterated upon has to be equal.");
			public static Error.Types.Usage IncorrectPropertyParameterCount => new Error.Types.Usage(1, "Getters and setters can only have 0 and 1 parameters each.");
			public static Error.Types.Usage EnsureFailed(string _message = null) => new Error.Types.Usage(2, "The specified condition was not met." + ((_message == null) ? "" : " Condition: " + _message));
			public static Error.Types.Usage NotInvokable(object _value) => new Error.Types.Usage(3, "An object of type " + _value.GetType().Name + " cannot be invoked.");
			public static Error.Types.Usage NotInvokable(string _name, object _value) => new Error.Types.Usage(3, "The variable \"" + _name + "\" of type " + _value.GetType().Name + " cannot be invoked.");
			public static Error.Types.Usage IncorrectParameterCount(string _name, int _params) => new Error.Types.Usage(4, "Incorrect number of parameters for function \"" + _name + "\". Expected: " + _params.ToString());
			public static Error.Types.Usage IncorrectParameterCount(int _params) => new Error.Types.Usage(5, "Incorrect number of parameters. Expected: " + _params.ToString());
			public static Error.Types.Usage GetOverwriteAttempted(string _name) => new Error.Types.Usage(6, "The variable \"" + _name + "\" already has a getter.");
			public static Error.Types.Usage SetOverwriteAttempted(string _name) => new Error.Types.Usage(7, "The variable \"" + _name + "\" already has a setter.");
		}

		public static class Semantics
		{
			public static Error.Types.Semantics NotValidBoolean => new Error.Types.Semantics(0, "Only 0 and 1 are valid values for a boolean.");
			public static Error.Types.Semantics NotAVariable => new Error.Types.Semantics(1, "The specified object is not a variable.");
			public static Error.Types.Semantics NameNotFound(string _name) => new Error.Types.Semantics(2, "The name \"" + _name + "\" was not found in the current scope.");
			public static Error.Types.Semantics IncorrectType(object _object, params string[] _types) => new Error.Types.Semantics(3, '"' + _object.GetType().Name + "\" is not the correct type. Expected: " + string.Join(", ", _types));
			public static Error.Types.Semantics VariableNotExistent(Variable _variable) => new Error.Types.Semantics(4, "Variable " + '"' + ((Variable)_variable).Name + '"' + " does not exist.");
			public static Error.Types.Semantics NotParseable(string _string) => new Error.Types.Semantics(5, "The string \"" + _string + "\" has an incorrect format and can't be parsed as a num.");
		}

		public static class Syntax
		{
			public static Error.Types.Syntax CompDeclarationError => new Error.Types.Syntax(0, "Comp declarations can only contain set- and setprop-operators.");
			public static Error.Types.Syntax InvalidEscapeCharacter(char _char) => new Error.Types.Syntax(1, "The character " + '\'' + _char.ToString() + '\'' + " is not a valid escape character.");
			public static Error.Types.Syntax InvalidEscapeSequence(string _sequence) => new Error.Types.Syntax(2, "The sequence " + '"' + _sequence + '"' + " is not a valid num and can't be parsed as an escape sequence.");
			public static Error.Types.Syntax InvalidCharacter(string _character) => new Error.Types.Syntax(3, "The sequence " + '"' + _character + '"' + " does not represent a valid character.");
		}

		public static class Execution
		{
			public static Error.Types.Execution AbortedByUser => new Error.Types.Execution(0, "The user aborted the program.");
			public static Error.Types.Execution AbortedViaCode(string _message = null) => new Error.Types.Execution(1, "The program was aborted via code." + ((_message == null) ? "" : " Reason: " + _message));
			public static Error.Types.Execution AbortedExternally(string _message) => new Error.Types.Execution(2, "The program was aborted externally. Reason: " + _message);
			public static Error.Types.Execution External(Exception _exception) => new Error.Types.Execution(3, "An external error occurred. Message: " + _exception.Message);
			public static Error.Types.Execution StackOverflow => new Types.Execution(4, "The stack has overflown.");
		}

		public static class Permission
		{
			public static Error.Types.Permission PermissionDenied(Zero.Permission _permission) => new Error.Types.Permission(0, "The permission to " + _permission.Description + " was denied.");
			public static Error.Types.Permission ScriptNotLocal => new Error.Types.Permission(1, "The script is not local and thus can't access any local files.");
		}
		
		public static Error.Types.Custom Custom(string _message) => new Types.Custom(0, _message);
	}
}