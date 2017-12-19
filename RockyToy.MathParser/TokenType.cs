namespace RockyToy.MathParser
{
	internal enum TokenType
	{
		LiteralNumber,

		Identifier,

		Func,
		FuncArgs,

		UnaryNoOp,

		UnaryPlus,
		UnaryMinus,
		UnaryNot,

		BinaryPow,

		BinaryMul,
		BinaryDiv,
		BinaryDivInt,

		BinaryPlus,
		BinaryMinus,

		BinaryLt,
		BinaryLe,
		BinaryGt,
		BinaryGe,

		BinaryEq,
		BinaryNe,

		BinaryAnd,
		BinaryOr,

		TrinayCondition,
	}
}