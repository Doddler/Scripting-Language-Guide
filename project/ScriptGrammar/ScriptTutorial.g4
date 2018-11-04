grammar ScriptTutorial;

/* Parser Rules */

scriptfile : IDENTIFIER LPAREN RPAREN LBRACKET statement* RBRACKET EOF;

statementblock : statement									#SingleStatement
			   | LBRACKET statement* RBRACKET				#StatementGroup
			   ;

statement : VAR (IDENTIFIER (COMMA IDENTIFIER)*)? SEMI													# VarDeclaration 
		  | IF LPAREN expression RPAREN block1=statementblock (ELSE block2=statementblock)?				# IfStatement
		  | FOR LPAREN asgn=expression SEMI comp=expression SEMI inc=expression RPAREN statementblock	# ForLoop
		  | expression SEMI																				# StatementExpression
		  ;

functionparam : expression (COMMA expression)*
			  ;

comparison_operator: GT | GE | LT | LE | EQ | NE;

entity : DECIMAL	#NumericConst
	   | STRING		#StringEntity
	   | IDENTIFIER	#Variable
	   ;

expression : IDENTIFIER type=(INC|DEC)											#ExpressionUnary
		   | left=expression type=(MULT|DIV) right=expression					#MathExpression
		   | left=expression type=(PLUS|MINUS) right=expression					#MathExpression
		   | left=expression comparison_operator right=expression				#MathExpression
		   | left=expression type=(BAND|BOR) right=expression					#MathExpression
		   | left=expression type=(AND|OR) right=expression						#MathExpression
		   | LPAREN expression RPAREN											#ArithmeticParens
		   | IDENTIFIER LPAREN functionparam? RPAREN							#FunctionCall
		   | IDENTIFIER EQUALS expression										#VarAssignment
		   | entity																#ExpressionEntity
		   ;

/* Lexer Rules */

IF   : 'if' ;
ELSE : 'else';
VAR  : 'var';
FOR : 'for';

AND : '&&' ;
OR  : '||' ;

BOR : '|' ;
BAND : '&' ;

TRUE  : 'true' ;
FALSE : 'false' ;

INC : '++';
DEC : '--';

MULT  : '*' ;
DIV   : '/' ;
PLUS  : '+' ;
MINUS : '-' ;

GT : '>' ;
GE : '>=' ;
LT : '<' ;
LE : '<=' ;
EQ : '==' ;
NE : '!=' ;

EQUALS : '=' ;

LPAREN : '(' ;
RPAREN : ')' ;

LBRACKET : '{' ;
RBRACKET : '}' ;

LSQUARE : '[' ;
RSQUARE : ']' ;

COMMA : ',';
SEMI : ';';

STRING	:  '"' ( '\\"' | ~('"') )* '"' ;

DECIMAL : '-'?[0-9]+('.'[0-9]+)? ;
IDENTIFIER : [a-zA-Z_][a-zA-Z_0-9]* ;

COMMENT : '//' .+? ('\n'|EOF) -> skip ;
WHITESPACE : [ \r\t\u000C\n]+ -> skip ;