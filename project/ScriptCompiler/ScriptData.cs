using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompiler
{
	public enum RemoteFunction
	{
		OutputText,
		OutputValue
	}

	public enum Operation
	{
		OpVal,
		OpPush,
		OpPop,
		OpJumpIf,
		OpJumpNotIf,
		OpJump,
		OpFunc,
		OpAssign,
		OpGetVar,
		OpAdd,
		OpSub,
		OpDiv,
		OpMul,
		OpAnd,
		OpOr,
		OpInc,
		OpDec,
		OpEquals,
		OpNotEquals,
		OpGreaterThan,
		OpLessThan,
		OpGreaterOrEquals,
		OpLessThanOrEquals,
	}
}
