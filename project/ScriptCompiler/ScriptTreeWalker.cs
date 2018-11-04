using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptTutorial;
using static ScriptTutorial.ScriptTutorialParser;

namespace ScriptCompiler
{
	class ScriptTreeWalker
	{
		private ScriptBuilder builder;

		public ScriptTreeWalker(ScriptBuilder scriptBuilder, ScriptTutorialParser parser)
		{
			builder = scriptBuilder;

			var data = parser.scriptfile();
			if (data.IDENTIFIER().GetText() != "main")
				throw new Exception("Top level function is not named main as expected.");

			foreach (var statement in data.statement())
				VisitStatement(statement);
		}

		public void VisitStatement(StatementContext context)
		{
			if (context is VarDeclarationContext varcontext)
				VisitVarDeclaration(varcontext);
			if (context is IfStatementContext ifcontext)
				VisitIfStatement(ifcontext);
			if (context is ForLoopContext forloopcontext)
				VisitForLoop(forloopcontext);
			if (context is StatementExpressionContext expressioncontext)
				VisitExpression(expressioncontext.expression());
		}

		public void VisitVarDeclaration(VarDeclarationContext context)
		{
			var ids = context.IDENTIFIER();

			foreach (var id in ids)
				builder.DeclareVariable(id.GetText());
		}

		public void VisitIfStatement(IfStatementContext context)
		{
			var elseLabel = builder.CreateLabel();
			var endLabel = builder.CreateLabel();

			VisitExpression(context.expression());

			builder.OutputOperation(Operation.OpJumpNotIf, elseLabel);

			VisitStatementBlock(context.block1);

			builder.OutputOperation(Operation.OpJump, endLabel);
			builder.SetLabelPosition(elseLabel);

			if (context.block2 != null)
				VisitStatementBlock(context.block2);

			builder.SetLabelPosition(endLabel);
		}

		public void VisitStatementBlock(StatementblockContext context)
		{
			if (context is SingleStatementContext single)
				VisitStatement(single.statement());
			if (context is StatementGroupContext group)
			{
				foreach (var s in group.statement())
					VisitStatement(s);
			}
		}

		public void VisitForLoop(ForLoopContext context)
		{
			VisitExpression(context.asgn);

			var startLabel = builder.CreateLabelAndSet();
			var endLabel = builder.CreateLabel();

			VisitExpression(context.comp);

			builder.OutputOperation(Operation.OpJumpNotIf, endLabel);

			VisitStatementBlock(context.statementblock());

			VisitExpression(context.inc);

			builder.OutputOperation(Operation.OpJump, startLabel);

			builder.SetLabelPosition(endLabel);
		}

		public void VisitExpression(ExpressionContext context)
		{
			if (context is ExpressionUnaryContext unarycontext)
				VisitUnaryExpression(unarycontext);
			if (context is MathExpressionContext mathcontext)
				VisitMathExpression(mathcontext);
			if (context is ArithmeticParensContext parencontext)
				VisitExpression(parencontext.expression());
			if (context is FunctionCallContext functioncontext)
				VisitFunctionExpression(functioncontext);
			if (context is VarAssignmentContext varcontext)
				VisitVarAssignment(varcontext);
			if (context is ExpressionEntityContext entitycontext)
				VisitEntity(entitycontext.entity());
		}

		public void VisitUnaryExpression(ExpressionUnaryContext context)
		{
			var varid = builder.GetVariableId(context.IDENTIFIER().GetText());

			if (context.type.Text == "++")
				builder.OutputOperation(Operation.OpInc, varid);
			else
				builder.OutputOperation(Operation.OpSub, varid);
		}

		public void VisitMathExpression(MathExpressionContext context)
		{
			VisitExpression(context.left);
			builder.OutputOperation(Operation.OpPush, 0);
			VisitExpression(context.right);
			builder.OutputOperation(Operation.OpPop, 1);

			var symbol = "";
			if (context.type != null)
				symbol = context.type.Text;
			if (context.comparison_operator() != null)
				symbol = context.comparison_operator().GetText();

			switch (symbol)
			{
				case "+":
					builder.OutputOperation(Operation.OpAdd);
					break;
				case "-":
					builder.OutputOperation(Operation.OpSub);
					break;
				case "*":
					builder.OutputOperation(Operation.OpMul);
					break;
				case "/":
					builder.OutputOperation(Operation.OpDiv);
					break;
				case "&&":
					builder.OutputOperation(Operation.OpAnd);
					break;
				case "||":
					builder.OutputOperation(Operation.OpOr);
					break;
				case "==":
					builder.OutputOperation(Operation.OpEquals);
					break;
				case "<":
					builder.OutputOperation(Operation.OpLessThan);
					break;
				case ">":
					builder.OutputOperation(Operation.OpGreaterThan);
					break;
				case "<=":
					builder.OutputOperation(Operation.OpLessThanOrEquals);
					break;
				case ">=":
					builder.OutputOperation(Operation.OpGreaterOrEquals);
					break;
				default:
					throw new Exception($"Unhandled math operation {symbol}");
			}
		}

		public void VisitFunctionExpression(FunctionCallContext context)
		{
			var name = context.IDENTIFIER().GetText();
			if(!Enum.TryParse(name, out RemoteFunction id))
				throw new Exception($"Could not find remote function with the name of {name}");

			var paramcount = 0;

			if (context.functionparam() != null)
			{
				foreach (var e in context.functionparam().expression())
				{
					VisitExpression(e);
					builder.OutputOperation(Operation.OpPush);
					paramcount++;
				}
			}

			builder.OutputOperation(Operation.OpVal, paramcount);
			builder.OutputOperation(Operation.OpFunc, (int)id);
		}

		public void VisitVarAssignment(VarAssignmentContext context)
		{
			var name = context.IDENTIFIER().GetText();
			var id = builder.GetVariableId(name);

			VisitExpression(context.expression());

			builder.OutputOperation(Operation.OpAssign, id);
		}

		public void VisitEntity(EntityContext context)
		{
			if (context is NumericConstContext num)
			{
				if(!int.TryParse(num.GetText(), out var val))
					throw new Exception($"Unable to parse {num.GetText()} as a number.");
				
				builder.OutputOperation(Operation.OpVal, val);
			}

			if (context is StringEntityContext str)
			{
				var text = str.GetText();
				text = text.Substring(1, text.Length - 2).Replace("\\\"", "\"");

				builder.OutputOperation(Operation.OpVal, builder.AddStringReference(text));
			}

			if (context is VariableContext v)
			{
				var name = v.GetText();
				var id = builder.GetVariableId(name);

				builder.OutputOperation(Operation.OpGetVar, id);
			}
		}
	}
}
