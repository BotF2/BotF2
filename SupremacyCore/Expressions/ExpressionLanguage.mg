module Supremacy.Expressions
{
	import Language { Base, Grammar };
    
	export Common;
    
	language Common
	{
		token NewLines = NewLineCharacter;
        
		syntax List(element) 
			= n:element => [n] 
			| l:List(element) n:element => [valuesof(l), n];
            
		syntax DelimitedList(element, delimiter) 
			= e:element => [e] 
			| e:element delimiter l:DelimitedList(element, delimiter) => [e, valuesof(l)];
            
		/*
		 * Common Literal Types
		 */
        
		@{Classification["Numeric"]}
		syntax Integer 
			= text:IntegerLiteral suffix:IntegralTypeSuffix
				=> LiteralExpression {
					   Kind => "Integer",
					   Text => text,
					   Type => TypeName { 
								   Name => suffix,
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				   }
			| text : IntegerLiteral
				=> LiteralExpression {
					   Kind => "Integer",
					   Text => text,
					   Type => TypeName { 
								   Name => "Int32",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				   }
			;
        
		@{Classification["Numeric"]}
		syntax Real
			= text:RealLiteral suffix:RealTypeSuffix
				=> LiteralExpression {
					   Kind => "Real",
					   Text => text,
					   Type => TypeName { 
								   Name => suffix,
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			| text:RealLiteral
				=> LiteralExpression {
					   Kind => "Real",
					   Text => text,
					   Type => TypeName { 
								   Name => "Double",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			;

            
		syntax TextValue = 
			text : TextLiteral
				=> LiteralExpression {
					   Kind => "Text",
					   Text => text,
					   Type => TypeName { 
								   Name => "String",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			;
        
		syntax CharacterValue = 
			text : CharacterLiteral
				=> LiteralExpression {
					   Kind => "Character",
					   Text => text,
					   Type => TypeName { 
								   Name => "Char",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			;
                
		syntax Logical
			= True
				=> LiteralExpression {
					   Kind => "Logical",
					   Text => "true",
					   Type => TypeName { 
								   Name => "Boolean",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			| False
				=> LiteralExpression {
					   Kind => "Logical",
					   Text => "false",
					   Type => TypeName { 
								   Name => "Boolean",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			;
        
		syntax Null 
			= "null"
				=> NullLiteral {
					   Kind => "Null",
					   Text => "null",
					   Type => TypeName { 
								   Name => "Null",
								   //TypeArguments => null,
								   IsBuiltinType => true
							   }
				}
			;
        
		@{Classification["Unknown"]}
		token ErrorErrorError = "__Error";
        
		@{Classification["Numeric"]}
		token HexadecimalIntegerLiteral 
			= "0x" HexDigit+
			| "0X" HexDigit+
			;
            
		token HexadecimalEscapeSequence 
			= HexadecimalEscapeSequencePrefix HexDigit 
			| HexadecimalEscapeSequencePrefix HexDigit HexDigit
			| HexadecimalEscapeSequencePrefix HexDigit HexDigit HexDigit
			| HexadecimalEscapeSequencePrefix HexDigit HexDigit HexDigit HexDigit;
		token HexadecimalEscapeSequencePrefix = "\\x" | "\\X";
		token CharacterEscapeSimple = '\u005C' CharacterEscapeSimpleCharacter;
		token CharacterEscapeSimpleCharacter 
			= "'"      // Single Quote
			| '"'      // Double Quote
			| '\u005C' // Backslash
			| '0'      // Null
			| 'a'      // Alert
			| 'b'      // Backspace
			| 'f'      // Form Feed
			| 'n'      // New Line
			| 'r'      // Carriage Return
			| 't'      // Horizontal Tab
			| 'v';     // Vertical Tab
		token UnicodeEscapeSequence
			= "\\u"  HexDigit  HexDigit  HexDigit  HexDigit
			| "\\U"  HexDigit  HexDigit  HexDigit  HexDigit HexDigit  HexDigit  HexDigit  HexDigit;
        
		@{Classification["Comment"]}
		token CommentToken 
			= CommentLine
			| DelimitedComment;
		token CommentLine = "//" CommentLineContent*;
		token CommentLineContent
			= ^(
				 '\u000A' // New Line
			  |  '\u000D' // Carriage Return
			  |  '\u0085' // Next Line
			  |  '\u2028' // Line Separator
			  |  '\u2029' // Paragraph Separator
			);
		token EndOfCommentLineToken
			= (
				 '\u000A' // New Line
			  |  '\u000D' // Carriage Return
			  |  '\u0085' // Next Line
			  |  '\u2028' // Line Separator
			  |  '\u2029' // Paragraph Separator
			);
            
		token Asterisks = "*"+;
		token OptionalAsterisks = "*"*;
		token NotSlashOrAsterisk
			= ^(
				  "/"
				| "*"
			   );
		token DelimitedCommentSection
			= "/"
			| OptionalAsterisks NotSlashOrAsterisk;
		token DelimitedComment
			= "/*" DelimitedCommentSection* Asterisks "/"
			; 
        
		token DecimalDigits = DecimalDigit+;
		token DecimalDigit =  '0'..'9';
        
		token HexDigit = '0'..'9' | 'a'..'f' | 'A'..'F';
		token HexDigits = HexDigit+;
        
		token Sign = "+" | "-";
        
		@{Classification["Numeric"]}
		token RealLiteral
			= DecimalDigit+ Dot DecimalDigit+ ExponentPart? //RealTypeSuffix?
			| Dot DecimalDigit+ ExponentPart? //RealTypeSuffix?
			| DecimalDigit+ ExponentPart //RealTypeSuffix?
			| DecimalDigit+ //RealTypeSuffix?
			;
		token ExponentPart
			= "e" Sign? DecimalDigit+
			| "E" Sign? DecimalDigit+
			;
		token RealTypeSuffix 
			= "F" => "Single"
			| "f" => "Single" 
			| "D" => "Double"
			| "d" => "Double"
			| "M" => "Decimal"
			| "m" => "Decimal"
			;
            
		@{Classification["Numeric"]}
		token IntegerLiteral
			= DecimalIntegerLiteral
			| HexadecimalIntegerLiteral
			;
            
		token DecimalIntegerLiteral = DecimalDigits;
		token IntegralTypeSuffix 
			= "U" => "UInt32"
			| "u" => "UInt32" 
			| "L" => "Int64"
			| "l" => "Int64"
			| "UL" => "UInt64"
			| "Ul" => "UInt64"
			| "uL" => "UInt64"
			| "ul" => "UInt64"
			| "LU" => "UInt64"
			| "Lu" => "UInt64"
			| "lU" => "UInt64"
			| "lu" => "UInt64"
			;
        
		token Letter = 'a'..'z' | 'A'..'Z';
                
		token NewLineCharacter 
			= SimpleNewLineCharacter
			| ('\u000D' '\u000A') // Carriage Return + New Line
			;
            
		token SimpleNewLineCharacter
			= '\u000A' // New Line
			| '\u000D' // Carriage Return
			| '\u0085' // Next Line
			| '\u2028' // Line Separator
			| '\u2029' // Paragraph Separator
			;

		@{Classification["String"]}
		token TextLiteral
			= '"' TextCharacter* '"' 
			| '@' '"' VerbatimStringLiteralCharacter* '"';
            
		@{Classification["String"]}
		token CharacterLiteral
			= "'" Character "'"
			;
        
		token Character
			= SingleCharacter
			| SimpleEscapeSequence
			| HexadecimalEscapeSequence
			| UnicodeEscapeSequence
			;
            
		token SingleCharacter
			= ^(
				 '\u0027' // Apostrophe
			  |  '\u005c' // Backslash
			  |  SimpleNewLineCharacter
			  )
			;
            
		token BackSlash = '\u005c';
        
		token SimpleEscapeSequence
			= BackSlash '\u0027'
			| BackSlash '\u0022'
			| BackSlash BackSlash
			| BackSlash '0'
			| BackSlash 'a' //\b  \f  \n  \r  \t  \v
			| BackSlash 'b'
			| BackSlash 'f'
			| BackSlash 'n'
			| BackSlash 'r'
			| BackSlash 't'
			| BackSlash 'v'
			;
            
		token TextCharacter 
			= TextSimple 
			| HexadecimalEscapeSequence 
			| CharacterEscapeSimple 
			| UnicodeEscapeSequence
            | OpenBraceEscape
            | CloseBraceEscape;
        token OpenBraceEscape = "{{";
        token CloseBraceEscape = "}}";
		token TextCharacters = TextCharacter+;
		token TextSimple
			= ^(
				'"'
              | '{'
              | '}'
			  | '\u005C' // Backslash
			  | '\u000A' // New Line
			  | '\u000D' // Carriage Return
			  | '\u0085' // Next Line
			  | '\u2028' // Line Separator
			  | '\u2029' // Paragraph Separator
			);
		token SingleVerbatimStringLiteralCharacter
			= ^('"');
		token VerbatimStringLiteralCharacter
			= SingleVerbatimStringLiteralCharacter
			| VerbatimQuoteEscapeSequence;
		token VerbatimQuoteEscapeSequence = '"' '"';
		token SingleVerbatimStringLiteralCharacters = SingleVerbatimStringLiteralCharacter+;
                
		@{Classification["Whitespace"]}
		token WhitespaceToken = WhitespaceCharacter+;
		token WhitespaceCharacter 
			= '\u0009'   // Horizontal Tab
			| '\u000B' // Vertical Tab
			| '\u000C' // Form Feed
			| '\u0020' // Space
			| NewLineCharacter
			;
           
            
		/*****************
		 * Common Tokens *
		 *****************/
    
		@{Classification["Operator"]} token Ampersand = "&";
		@{Classification["Operator"]} token AmpersandEqualsSign = "&=";
		@{Classification["Operator"]} token AmpersandAmpersand = "&&";
		@{Classification["Operator"]} token Asterisk = "*";
		@{Classification["Operator"]} token CircumflexAccent = "^";
		@{Classification["Operator"]} token CircumflexAccentEqualsSign = "^=";
		@{Classification["Operator"]} token EqualsSign = "=";
		@{Classification["Operator"]} token EqualsSignEqualsSign = "==";
		@{Classification["Operator"]} token ExclamationMark = "!";
		@{Classification["Operator"]} token ExclamationMarkEqualsSign = "!=";
		@{Classification["Operator"]} token LessThanSignGreaterThanSign = "<>";
		@{Classification["Operator"]} token GreaterThanSign = ">";
		@{Classification["Operator"]} token GreaterThanSignEqualsSign = ">=";
		@{Classification["Operator"]} token HyphenMinus = "-";
		@{Classification["Operator"]} token LessThanSign = "<";
		@{Classification["Operator"]} token LessThanSignEqualsSign = "<=";
		@{Classification["Operator"]} token LessThanSignLessThanSign = "<<";
		@{Classification["Operator"]} token LessThanSignLessThanSignEqualsSign = "<<=";
		@{Classification["Operator"]} token GreaterThanSignGreaterThanSign = ">>";
		@{Classification["Operator"]} token GreaterThanSignGreaterThanSignEqualsSign = ">>=";
		@{Classification["Operator"]} token PercentSign = "%";
		@{Classification["Operator"]} token PercentSignEqualsSign = "%=";
		@{Classification["Operator"]} token PlusSign = "+";
		@{Classification["Operator"]} token PlusSignEqualsSign = "+=";
		@{Classification["Operator"]} token MinusSignEqualsSign = "-=";
		@{Classification["Operator"]} token AsteriskEqualsSign = "*=";
		@{Classification["Operator"]} token SolidusEqualsSign = "/=";
		@{Classification["Operator"]} token QuestionMark = "?";
		@{Classification["Operator"]} token QuestionMarkQuestionMark = "??";
		@{Classification["Operator"]} token Solidus = "/";
		@{Classification["Operator"]} token Tilde = "~";
		@{Classification["Operator"]} token VerticalLine = "|";
		@{Classification["Operator"]} token VerticalLineEqualsSign = "|";
		@{Classification["Operator"]} token VerticalLineVerticalLine = "||";
        
		@{Classification["Delimiter"]} token Colon = ":";
		@{Classification["Delimiter"]} token Comma = ",";
		@{Classification["Delimiter"]} token Dot = ".";
		@{Classification["Delimiter"]} token LeftCurlyBracket = "{";
		@{Classification["Delimiter"]} token LeftParentheses = "(";
		@{Classification["Delimiter"]} token LeftSquareBracket = "[";
		@{Classification["Delimiter"]} token RightCurlyBracket = "}";
		@{Classification["Delimiter"]} token RightParentheses = ")";
		@{Classification["Delimiter"]} token RightSquareBracket = "]";
		@{Classification["Delimiter"]} token Semicolon = ";";
        
		@{Classification["Keyword"]} token True = "true";
		@{Classification["Keyword"]} token False = "false";
	}
}

module Supremacy.Expressions
{
	import Language { Base, Grammar };
	import Common as C;
    
	language ExpressionLanguage
	{
		interleave Skippable = Whitespace | Comment | NewLines;
		interleave Whitespace = C.WhitespaceToken;
		interleave Comment = CommentToken;
		interleave NewLines = C.NewLines;
        
		syntax Main
			= e:ConditionScopeExpression => e
			| e:Expression => e
			;
        
		syntax ConditionScopeExpression
			= locals:C.List(LocalDeclaration) query:QueryExpression
				=> ConditionScopeExpression {
					   Locals => locals,
					   Query => query
				   }
			| locals:C.List(LocalDeclaration) query:SimpleQueryExpression
				=> ConditionScopeExpression {
					   Locals => locals,
					   Query => query
				   }
			;
        
		syntax LocalDeclaration
			= Local name:LocalVariableName C.EqualsSign value:NonAssignmentExpression
				=> id("LocalDeclaration") {
					   VariableName => name,
					   Initializer => value
				   }
			;
        
		syntax SimpleQueryExpression
			= Select projection:Expression
				=> SimpleQueryExpression {
					   Projection => projection
				   }
			;
            
		syntax PrimaryExpression
			= e:ArrayCreationExpression => e
			| e:PrimaryNoArrayCreationExpression => e
			;
        
		syntax PrimaryNoArrayCreationExpression
			= e:Literal => e
			| e:NameExpression => e
			| e:ParenthesizedExpression => e
			| e:MemberAccessExpression => e
			| e:InvocationExpression => e
			| e:ElementAccessExpression => e
			| e:PostIncrementExpression => e
			| e:PostDecrementExpression => e
			| e:ObjectCreationExpression => e
			| e:AnonymousObjectCreationExpression => e
			| e:TypeofExpression => e
			| e:DefaultValueExpression => e
			;

		syntax NameExpression
			= precedence 2: name:TypeName => name
			| precedence 1: name:IdentifierName
				=> NameExpression {
					   Name => name
					   //TypeArguments => null
				   }
			;
        
		syntax TypeName
			= precedence 2: name:BaseIdentifierName arguments:TypeArgumentList
				=> TypeName {
					   Name => name,
					   TypeArguments => [valuesof(arguments)]
				   }
			| precedence 1: name:BaseIdentifierName
				=> TypeName {
					   Name => name
					   //TypeArguments => null
				   }
			;
        
		syntax TypeArgumentList
			= "<" arguments:C.DelimitedList(Type, C.Comma) ">"
				=> [valuesof(arguments)]
			;
            
		syntax TypeArguments
			= arguments:C.DelimitedList(Type, ",")
				=> [valuesof(arguments)]
			;
            
		syntax TypeArgument
			= type:Type
				=> TypeArgument {
					   TypeName => type
				   }
			;
        
		syntax TypeParameter
			= name:BaseIdentifierName
				=> TypeParameter {
					   TypeName { name }
				}
			;
            
		syntax Expression
			= e:NonAssignmentExpression => e
			| e:Assignment => e
			;
            
		token AssignmentOperator
			= C.EqualsSign
			| C.PlusSignEqualsSign
			| C.MinusSignEqualsSign
			| C.AsteriskEqualsSign
			| C.SolidusEqualsSign
			| C.PercentSignEqualsSign
			| C.AmpersandEqualsSign
			| C.VerticalLineEqualsSign
			| C.CircumflexAccentEqualsSign
			| C.LessThanSignLessThanSignEqualsSign
			| C.GreaterThanSignGreaterThanSignEqualsSign
			;
            
		syntax Assignment
			= UnaryExpression AssignmentOperator Expression
			;
            
		syntax UnaryExpression
			= e:PrimaryExpression => e
			| operator:(Add => "UnaryPlus") operand:UnaryExpression
				=> UnaryExpression {
					   Operator => operator,
					   Operand => operand
				   }
			| operator:(Subtract => "Negate") operand:UnaryExpression
				=> UnaryExpression {
					   Operator => operator,
					   Operand => operand
				   }
			| operator:Not operand:UnaryExpression
				=> UnaryExpression {
					   Operator => operator,
					   Operand => operand
				   }
			| operator:Complement operand:UnaryExpression
				=> UnaryExpression {
					   Operator => operator,
					   Operand => operand
				   }
			| e:PreIncrementExpression => e
			| e:PreDecrementExpression => e
			| e:CastExpression => e
			;
            
		syntax PreIncrementExpression
			= "++" operand:UnaryExpression
				=> PreIncrementExpression {
					   Operand { operand }
				   }
			;
        
		syntax PreDecrementExpression
			= "--" operand:UnaryExpression
				=> PreDecrementExpression {
					   Operand { operand }
				   }
			;
        
		syntax PostIncrementExpression
			= operand:PrimaryExpression "++"
				=> PostIncrementExpression {
					   Operand { operand }
				   }
			;
        
		syntax PostDecrementExpression
			= operand:PrimaryExpression "--"
				=> PostDecrementExpression {
					   Operand { operand }
				   }
			;
        
		syntax CastExpression
			= "(" type:Type ")" operand:UnaryExpression
				=> CastExpression {
					   DetinationType => type,
					   Operand => operand
				   }
			;
        
		syntax DefaultValueExpression
			= Default "(" type:Type ")"
				=> DefaultValueExpression {
					   TypeName => type
				   }
			;
        
		syntax TypeofExpression
			= Typeof "(" type:Type ")"
				=> TypeofExpression {
					   TypeName => type
				   }
			| Typeof "(" type:UnboundTypeName ")"
				=> TypeofExpression {
					   TypeName => type
				   }
			;
        
		syntax UnboundTypeName
			= name:BaseIdentifierName genericDimension:GenericDimensionSpecifier?
				=> TypeName {
					   Name => name,
					   //TypeArguments => null,
					   IsUnboundGenericType => true,
					   UnboundGenericDimensions => genericDimension
				   }
			;
            
		syntax GenericDimensionSpecifier
			= "<" commas:C.List(C.Comma)? ">"
				=> [valuesof(commas)]
			;
        
		syntax MultiplicativeExpression
			= e:UnaryExpression => e
			| leftArg:MultiplicativeExpression operator:Multiply rightArg:UnaryExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:MultiplicativeExpression operator:Divide rightArg:UnaryExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:MultiplicativeExpression operator:Modulo rightArg:UnaryExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
            
		syntax AdditiveExpression
			= e:MultiplicativeExpression => e
			| leftArg:AdditiveExpression operator:Add rightArg:MultiplicativeExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:AdditiveExpression operator:Subtract rightArg:MultiplicativeExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax ShiftExpression
			= e:AdditiveExpression => e
			| leftArg:ShiftExpression operator:LeftShift rightArg:AdditiveExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:ShiftExpression operator:RightShift rightArg:AdditiveExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax RelationalExpression
			= e:ShiftExpression => e
			| leftArg:RelationalExpression operator:LessThan rightArg:ShiftExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:RelationalExpression operator:GreaterThan rightArg:ShiftExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:RelationalExpression operator:LessThanOrEqual rightArg:ShiftExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:RelationalExpression operator:GreaterThanOrEqual rightArg:ShiftExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:RelationalExpression operator:TypeIs rightArg:Type
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:RelationalExpression operator:TypeAs rightArg:Type
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax EqualityExpression
			= e:RelationalExpression => e
			| leftArg:EqualityExpression operator:Equal rightArg:RelationalExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			| leftArg:EqualityExpression operator:NotEqual rightArg:RelationalExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax AndExpression
			= e:EqualityExpression => e
			| leftArg:AndExpression operator:And rightArg:EqualityExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax ExclusiveOrExpression
			= e:AndExpression => e
			| leftArg:ExclusiveOrExpression operator:ExclusiveOr rightArg:AndExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax OrExpression
			= e:ExclusiveOrExpression => e
			| leftArg:OrExpression operator:Or rightArg:ExclusiveOrExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax AndAlsoExpression
			= e:OrExpression => e
			| leftArg:AndAlsoExpression operator:AndAlso rightArg:OrExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax OrElseExpression
			= e:AndAlsoExpression => e
			| leftArg:OrElseExpression operator:OrElse rightArg:AndAlsoExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
        
		syntax NullCoalescingExpression
			= e:OrElseExpression => e
			| leftArg:OrElseExpression operator:NullCoalesce rightArg:NullCoalescingExpression
				=> BinaryOperationExpression {
					  Left => leftArg,
					  Right => rightArg,
					  Operator => operator
				   }
			;
            
		syntax ConditionalExpression
			= e:NullCoalescingExpression => e
			| test:NullCoalescingExpression "?" ifTrue:Expression ":" ifFalse:Expression
				=> ConditionalExpression {
					   Test => test,
					   IfTrue => ifTrue,
					   IfFalse => ifFalse
				   }
			;
        
		syntax InvocationExpression
			= precedence 2: target:MemberAccessExpression "(" arguments:ArgumentList? ")"
				=> InvokeMemberExpression {
					   Target => target,
					   Arguments => [valuesof(arguments)]
				   }
			| precedence 1: target:PrimaryExpression "(" arguments:ArgumentList? ")"
				=> InvokeExpression {
					   Target => target,
					   Arguments => [valuesof(arguments)]
				   }
			;
/*
		member-access:
			primary-expression   .   identifier  type-argument-listopt
			predefined-type   .   identifier  type-argument-listopt
			qualified-alias-member   .   identifier
*/
		syntax MemberAccessExpression
			= leftArg:PrimaryExpression C.Dot memberName:BaseIdentifierName typeArguments:TypeArgumentList?
				=> MemberAccessExpression {
					   Target => leftArg,
					   MemberName => memberName,
					   TypeArguments => typeArguments
				   }
			| leftArg:SimpleType C.Dot memberName:BaseIdentifierName typeArguments:TypeArgumentList?
				=> MemberAccessExpression {
					   Target => leftArg,
					   MemberName => memberName,
					   TypeArguments => typeArguments
				   }
			;
        
		syntax ElementAccessExpression
			= leftArg:PrimaryExpression "[" arguments:C.DelimitedList(Expression, C.Comma) "]"
				=> ElementAccessExpression {
					   LeftArgument => leftArg,
					   Arguments => [valuesof(arguments)]
				   }
			;
        
		syntax ObjectCreationExpression
			= New type:Type arguments:("(" al:ArgumentList? ")" => al)? initializer:ObjectOrCollectionInitializer?
				=> ObjectCreationExpression {
					   ObjectType => type,
					   Arguments => [valuesof(arguments)],
					   Initializer => initializer
				   }
			;
        
		syntax ObjectOrCollectionInitializer
			= i:ObjectInitializer => i
			| i:CollectionInitializer => i
			;
        
		syntax ObjectInitializer
			= "{" memberInitializers:(mi:C.DelimitedList(MemberInitializer, C.Comma)? C.Comma? => mi) "}"
				=> ObjectInitializerExpression {
					   MemberInitializers => [valuesof(memberInitializers)]
				   }
			;
        
		syntax CollectionInitializer
			= "{" initializerValues:(iv:C.DelimitedList(NonAssignmentExpression, C.Comma)? C.Comma? => iv) "}"
				=> CollectionInitializerExpression {
					   InitializerValues => initializerValues
				   }
			;
        
		syntax MemberInitializer
			= memberName:BaseIdentifierName "=" value:InitializerValue
				=> MemberInitializerExpression {
					   MemberName => memberName,
					   Value => value
				   }
			;
        
		syntax InitializerValue
			= e:Expression => e
			| i:ObjectOrCollectionInitializer => i
			;
/*
		object-creation-expression:
			new   type   (   argument-listopt   )   object-or-collection-initializeropt 
			new   type   object-or-collection-initializer
		object-or-collection-initializer:
			object-initializer
			collection-initializer
		object-initializer:
			{   member-initializer-listopt   }
			{   member-initializer-list   ,   }
		member-initializer-list:
			member-initializer
			member-initializer-list   ,   member-initializer
		member-initializer:
			identifier   =   initializer-value
		initializer-value:
			expression
			object-or-collection-initializer
		collection-initializer:
			{   element-initializer-list   }
			{   element-initializer-list   ,   }
		initializer-value:
			expression
			object-or-collection-initializer

*/

		/********************************************************************************
		 * QUERY EXPRESSIONS
		 ****************************************************************************** */
         
		syntax QueryExpression
			= into:IntoExpression => into
			| into:FromExpression => into
			/*| from:FromClause clauses:QueryBody
				=> QueryExpression {
					   FromClause => from,
					   BodyClauses => clauses
				   }*/
			;
        
		syntax RangeDeclaration
			= name:LocalVariableName
				=> RangeDeclaration {
					   VariableName => name
				   }
			;
        
		syntax QueryDeclaration
			= name:LocalVariableName
				=> QueryDeclaration {
					   VariableName => name
				   }
			;
        
		syntax FromClause
			= From itemType:Type itemIdentifier:RangeDeclaration In itemSource:Expression
				=> FromClause {
					   VariableName => itemIdentifier,
					   Initializer => itemSource,
					   VariableType => itemType
				   }
			| From itemIdentifier:RangeDeclaration In itemSource:Expression
				=> FromClause {
					   VariableName => itemIdentifier,
					   Initializer => itemSource
				   }
			;
        
		syntax FromExpression 
		= From fname:QueryDeclaration In init:ConditionalExpression fbody:QueryBody
			=> FromExpression {
				VariableName => fname,
				Initializer => init,
				Body => fbody 
			}
		| From fname:QueryDeclaration In init:ConditionalExpression error
			=> FromExpression {
				VariableName => fname,
				Initializer => init 
			};
        
		syntax IntoExpression 
			= From fname:RangeDeclaration In init:ConditionalExpression fbody:QueryBody Into iname:RangeDeclaration ibody:QueryBody
				=> IntoExpression {
					VariableName => iname,
					Initializer =>
						FromExpression {
							VariableName => fname,
							Initializer => init,
							Body => fbody              
					},
					Body => ibody 
				}
			| From fname:RangeDeclaration In init:ConditionalExpression fbody:QueryBody Into iname:RangeDeclaration error ibody:QueryBody
				=> IntoExpression {
					VariableName => iname,
					Initializer =>
						FromExpression {
							VariableName => fname,
							Initializer => init,
							Body => fbody 
					},
					Body => ibody 
				}                
			;
        
	syntax QueryBody 
		= f:FromExpression
			=> f
		| j:JoinIntoExpression
			=> j
		| j:JoinExpression
			=> j
		| w:WhereClause
			=> w
		| o:OrderbyExpression
			=> o
		| l:LetExpression
			=> l
		| q:QueryConstructor
			=> q;

	syntax QueryGenerator 
		= q:QueryBody
			=> q;
    
	syntax JoinExpression 
		= Join name:QueryDeclaration In inExpr:ConditionalExpression On onExpr:Expression Equals eqExpr:ConditionalExpression body:QueryGenerator
			=> JoinExpression {
				VariableName => name,
				Initializer => inExpr,
				Body => body,
				LeftKey => onExpr,
				RightKey => eqExpr 
			}
		| Join name:QueryDeclaration In inExpr:ConditionalExpression error
			=> JoinExpression {
				VariableName => name,
				Initializer => inExpr,
				Body => [],
				LeftKey => [],
				RightKey => [] 
			}
		| Join name:LocalVariableName In inExpr:ConditionalExpression On onExpr:Expression error
			=> JoinExpression {
				VariableName => name,
				Initializer => inExpr,
				Body => [],
				LeftKey => [],
				RightKey => [] 
			}
		| Join name:QueryDeclaration In inExpr:ConditionalExpression On onExpr:Expression Equals eqExpr:ConditionalExpression error
			=> JoinExpression {
				VariableName => name,
				Initializer => inExpr,
				Body => [],
				LeftKey => onExpr,
				RightKey => eqExpr 
			};
        
	syntax JoinIntoExpression 
		= Join name:QueryDeclaration In inExpr:ConditionalExpression On onExpr:Expression Equals eqExpr:ConditionalExpression
		  Into intoName:RangeDeclaration body:QueryGenerator
			=> JoinIntoExpression {
				VariableName => name,
				Initializer => inExpr,
				Body => body,
				LeftKey => onExpr,
				RightKey => eqExpr,
				IntoName => intoName
			}
		| Join name:QueryDeclaration In inExpr:ConditionalExpression On onExpr:Expression Equals eqExpr:ConditionalExpression
		  Into intoName:RangeDeclaration error
			=> JoinIntoExpression {
				VariableName => name,
				Initializer => inExpr,
				Body => [],
				LeftKey => onExpr,
				RightKey => eqExpr,
				IntoName => intoName
			};
    
	syntax LetExpression 
		= Let name:QueryDeclaration C.EqualsSign expr:ConditionalExpression body:QueryGenerator
			=> LetExpression {
				VariableName => name,
				Initializer => expr,
				Body => body 
			};

	syntax WhereClause 
		= Where predicate:Expression body:QueryGenerator
			=> WhereClause {
				Predicate => predicate,
				Body => body 
			}
		| Where predicate:Expression error
			=> WhereClause {
				Predicate => predicate,
				Body => null
			};

	syntax QueryConstructor 
		= g:GroupByClause
			=> g
		| s:SelectClause
			=> s;
    
	syntax GroupByClause 
		= Group projection:Expression By discriminator:ConditionalExpression
			=> GroupByClause {
				Projection => projection,
				Discriminator => discriminator 
			}
		| Group projection:Expression error
			=> GroupByClause {
				Projection => projection,
				Discriminator => null
			};
    
	syntax QueryBody2
			= clauses:C.List(QueryBodyClause)? selectOrGroupClause:SelectOrGroupClause //continuation:QueryContinuation?
				=> QueryBody {
					   Clauses => [valuesof(clauses)],
					   SelectOrGroupClause => selectOrGroupClause//,
					   //Continuation => continuation
				   }
			;
        
		syntax QueryBodyClause
			= c:FromClause => c
			| c:LetClause => c
			| c:WhereClause => c
			| c:JoinClause => c
			| c:JoinIntoClause => c
			| c:OrderbyClause => c
			;
        
		syntax LetClause
			= Let name:RangeDeclaration "=" value:Expression
				=> LetClause {
					   VariableName => name,
					   Value => value
				   }
			;
        
		syntax WhereClause2
			= Where condition:Expression
				=> WhereClause {
					   Condition => condition
				   }
			;
        
		syntax JoinClause
			= Join type:(Type?) name:RangeDeclaration In inner:Expression On leftComparand:Expression Equals rightComparand:Expression
				=> JoinClause {
					   Type => type,
					   InnerItemIndentifer => name,
					   InnerItemSource => inner,
					   InnerKey => leftComparand,
					   OuterKey => rightComparand
				   }
			;
        
		syntax JoinIntoClause
			= Join type:(Type?) name:RangeDeclaration In source:Expression On leftComparand:Expression Equals rightComparand:Expression Into targetVariable:RangeDeclaration
				=> JoinIntoClause {
					   Type => type,
					   InnerItemIndentifer => name,
					   InnerItemSource => source,
					   OuterKey => leftComparand,
					   InnerKey => rightComparand,
					   GroupIdentifier => targetVariable
				   }
			;
        
		syntax SelectClause
			= Select value:Expression
				=> SelectClause {
					   Projection => value
				   }
			;
        
		syntax GroupClause
			= Group rangeVariableName:RangeDeclaration By selection:Expression
				=> GroupClause {
					   RangeVariableName => rangeVariableName,
					   Key => selection
				   }
			;
        
		syntax SelectOrGroupClause
			= c:SelectClause => c
			| c:GroupClause => c
			;
        
		syntax QueryContinuation
			= Into rangeVariableName:RangeDeclaration continuation:QueryBody
				=> QueryContinuation {
					   RangeVariableName => rangeVariableName,
					   Continuation => continuation
				   }
			;
        
		syntax OrderbyClause
			= Orderby orderings:C.DelimitedList(Ordering, C.Comma)
				=> OrderbyClause {
					   Orderings => orderings
				   }
			;
        
		syntax OrderbyExpression
			= Orderby orderings:C.DelimitedList(Ordering, C.Comma) body:QueryGenerator
				=> OrderbyExpression {
					   Orderings => orderings,
					   Body => body
				   }
			;
            
		syntax Ordering
			= expression:Expression direction:OrderingDirection
				=> Ordering {
					   Expression => expression,
					   Direction => direction
				   }
			| expression:Expression
				=> Ordering {
					   Expression => expression,
					   Direction => "Ascending"
				   }
			;
            
		@{Classification["Keyword"]}
		final token OrderingDirection
			= Ascending => "Ascending"
			| Descending => "Descending"
			;

		/********************************************************************************
		 * ARRAY CREATION EXPRESSIONS
		 ****************************************************************************** */

/*
		array-creation-expression:
			new   non-array-type   [   expression-list   ]   rank-specifiersopt   array-initializeropt
			new   array-type   array-initializer 
			new   rank-specifier   array-initializer
            
		array-initializer:
			{   variable-initializer-listopt   }
			{   variable-initializer-list   ,   }

		variable-initializer-list:
			variable-initializer
			variable-initializer-list   ,   variable-initializer

		variable-initializer:
			expression
			array-initializer
*/
        
		syntax ExpressionList
			= expressions:C.DelimitedList(Expression, C.Comma)
			;
        
		syntax ArrayCreationExpression
			= New type:NonArrayType "[" dimensionExpressions: ExpressionList "]" RankSpecifiers? ArrayInitializerExpression?
			| New type:ArrayType ArrayInitializerExpression
			| New RankSpecifier ArrayInitializerExpression
			;
        
		syntax ArrayInitializerExpression
			= "{" variableInitializers:C.DelimitedList(VariableInitializer, C.Comma)? "}"
				=> ArrayInitializer {
					   Values => [valuesof(variableInitializers)]
				   }
			;
        
		syntax VariableInitializer
			= e:Expression => e
			| i:ArrayInitializerExpression => i
			;
        
		syntax AnonymousObjectCreationExpression
			= New initializer:AnonymousObjectInitializer
				=> AnonymousObjectCreationExpression {
					   Initializer => initializer
				   }
			;
        
		syntax AnonymousObjectInitializer
			= "{" memberDeclarators:C.DelimitedList(MemberDeclarator, C.Comma) "}"
				=> AnonymousObjectCreationExpression {
					   MemberDeclarators => [valuesof(memberDeclarators)]
				   }
			;
        
		syntax MemberDeclarator
			= name:BaseIdentifierName
				=> NameExpression {
					   Name => name
				   }
			| memberAccess:MemberAccessExpression => memberAccess
			| memberName:BaseIdentifierName "=" value:Expression
				=> MemberDeclaratorExpression {
					   MemberName => memberName,
					   Value => value
				   }
			;
/*
anonymous-object-creation-expression:
	new   anonymous-object-initializer
anonymous-object-initializer:
	{   member-declarator-listopt   }
	{   member-declarator-list   ,   }
member-declarator-list:
	member-declarator
	member-declarator-list   ,   member-declarator
member-declarator:
	simple-name
	member-access
	identifier   =   expression
*/   
		/********************************************************************************
		 * LAMBDA EXPRESSIONS
		 ****************************************************************************** */
        
		syntax LambdaExpression
			= signature:LambdaSignature "=>" body:Expression
				=> LambdaExpression {
					   Parameters => [valuesof(signature)],
					   Body => body
				   }
			;
        
		syntax LambdaSignature
			= precedence 1: s:ImplicitLambdaSignature => s
			| precedence 2: s:ExplicitLambdaSignature => s
			;
        
		syntax ImplicitLambdaSignature
			= "(" parameters:(c:C.DelimitedList(ImplicitLambdaParameter, C.Comma)? => c) ")"
				=> Parameters[valuesof(parameters)]
			| parameter:ImplicitLambdaParameter
				=> Parameters[parameter]
			;
        
		syntax ExplicitLambdaSignature
			= "(" parameters:(c:C.DelimitedList(ExplicitLambdaParameter, C.Comma)? => c) ")"
				=> Parameters[valuesof(parameters)]
			;
        
		syntax ImplicitLambdaParameter
			= name:BaseIdentifierName
				=> LambdaParameter {
					   Name => name
				   }
			;
        
		syntax ExplicitLambdaParameter
			= Ref type:Type name:BaseIdentifierName
				=> LambdaParameter {
					   Name => name,
					   Type => type,
					   IsRef => true
				   }
			| Out type:Type name:BaseIdentifierName
				=> LambdaParameter {
					   Name => name,
					   Type => type,
					   IsOut => true
				   }
			| type:Type name:BaseIdentifierName
				=> LambdaParameter {
					   Name => name,
					   Type => type
				   }
			;
/*
lambda-expression:
	anonymous-function-signature   =>   anonymous-function-body
anonymous-function-signature:
	explicit-anonymous-function-signature 
	implicit-anonymous-function-signature
explicit-anonymous-function-signature:
	(   explicit-anonymous-function-parameter-listopt   )
	explicit-anonymous-function-parameter-list
	explicit-anonymous-function-parameter
	explicit-anonymous-function-parameter-list   ,   explicit-anonymous-function-parameter
explicit-anonymous-function-parameter:
	anonymous-function-parameter-modifieropt   type   identifier
anonymous-function-parameter-modifier: 
	ref
	out
implicit-anonymous-function-signature:
	(   implicit-anonymous-function-parameter-listopt   )
	implicit-anonymous-function-parameter
	implicit-anonymous-function-parameter-list
	implicit-anonymous-function-parameter
	implicit-anonymous-function-parameter-list   ,   implicit-anonymous-function-parameter
implicit-anonymous-function-parameter:
	identifier
anonymous-function-body:
	expression
	block
*/
         
		/********************************************************************************
		 * TYPES
		 ****************************************************************************** */
         
		 syntax Type
			= precedence 1: t:TypeName => t
			| precedence 2: t:SimpleType => t
			//= t:ReferenceType => t
			//| t:ValueType => t
			//| t:TypeParameter => t
			;
         
		 syntax ValueType
			= t:StructType => t
			| t:EnumType => t
			;
         
		 syntax StructType
			= t:TypeName => t
			| t:SimpleType => t
			| t:NullableType => t
			;
         
		 syntax SimpleType
			= t:NumericType => t
			| t:Bool
				=> TypeName { Name => "Boolean", IsBuiltinType => true }
			;
         
		 syntax NumericType
			= t:IntegralType => t
			| t:FloatingPointType => t
			| t:Decimal
				=> TypeName { Name => "Decimal", IsBuiltinType => true }
			;
         
		 syntax IntegralType
			= t:SByte
				=> TypeName { Name => "SByte", IsBuiltinType => true }
			| t:Byte
				=> TypeName { Name => "Byte", IsBuiltinType => true }
			| t:Short
				=> TypeName { Name => "Int16", IsBuiltinType => true }
			| t:UShort
				=> TypeName { Name => "UInt16", IsBuiltinType => true }
			| t:Int
				=> TypeName { Name => "Int32", IsBuiltinType => true }
			| t:UInt
				=> TypeName { Name => "UInt32", IsBuiltinType => true }
			| t:Long
				=> TypeName { Name => "Int64", IsBuiltinType => true }
			| t:ULong
				=> TypeName { Name => "UInt16", IsBuiltinType => true }
			| t:Char
				=> TypeName { Name => "Char", IsBuiltinType => true }
			;
         
		 syntax FloatingPointType
			= t:Float
				=> TypeName { Name => "Single", IsBuiltinType => true }
			| t:Double
				=> TypeName { Name => "Double", IsBuiltinType => true }
			;
         
		 syntax NullableType
			= t:NonNullableValueType "?"
				=> TypeName { Name => t, IsNullableType => true }
			;
            
		 syntax NonNullableValueType
			= t:Type => t
			;
         
		 syntax EnumType
			= t:TypeName => t
			;
         
		 syntax ReferenceType
			= t:ClassType => t
			| t:InterfaceType => t
			| t:ArrayType => t
			| t:DelegateType => t
			;
         
		syntax ClassType
			= t:TypeName => t
			| t:Object
				=> TypeName { Name => t }
			| t:String
				=> TypeName { Name => t }
			;
         
		syntax InterfaceType
			= t:TypeName => t
			;
        
		syntax DelegateType
			= t:TypeName => t
			;
         
		syntax ArrayType
			= type:NonArrayType rank:RankSpecifiers
				=> ArrayType {
					   Type => type,
					   RankSpecifiers => rank
				   }
			;
         
		syntax NonArrayType
			= t:Type => t
			;
         
		syntax DimensionSeparators
			= comma:C.Comma
				=> [comma]
			| separators:DimensionSeparators comma:C.Comma
				=> [valuesof(separators), comma]
			;
        
		syntax RankSpecifier
			= "[" DimensionSeparators* "]"
			;
        
		syntax RankSpecifiers
			= specifier:RankSpecifier
				=> [specifier]
			| specifiers:RankSpecifiers specifier:RankSpecifier
				=> [valuesof(specifiers), specifier]
			;
        
		syntax NonAssignmentExpression
			= c:ConditionalExpression => c
			| l: LambdaExpression => l
			| q:QueryExpression => q
			;
            
		syntax ConstantExpression
			= e:Expression => e
			;
            
		syntax ParenthesizedExpression
			= "(" e:Expression ")" => e
			;
        
		syntax Literal 
			= precedence 2: d:C.Real
				=> d
			| precedence 1: i:C.Integer
				=> i
			| t:C.TextValue
				=> t
			| c:C.CharacterValue
				=> c
			| l:C.Logical
				=> l
			| n:C.Null
				=> n
			;
            
		syntax NamedReference
			= IdentifierName
			;
            
		syntax Argument
			= value:Expression
				=> value
			;
        
		syntax ArgumentList 
			= arguments:C.DelimitedList(Argument, ',')
				=> [valuesof(arguments)]
			;
            
		@{Classification["Comment"]}
		token CommentToken = C.CommentToken;
        
		token BaseIdentifierName = IdentifierBegin (IdentifierCharacter* IdentifierEnd)?;
        
		token IdentifierBegin = C.Letter;
		token IdentifierEnd = '_' | C.Letter | C.DecimalDigit;
		token IdentifierCharacter 
			= IdentifierEnd
			| '$';
		token IdentifierCharacters = IdentifierCharacter+;
        
		@{Classification["Identifier"]} token LocalVariableName = BaseIdentifierName;
		@{Classification["Identifier"]} token SpecialParameterName = Dollar BaseIdentifierName;
		@{Classification["Identifier"]} token UserParameterName = Hash BaseIdentifierName;
        
		@{Classification["Identifier"]} token IdentifierName = LocalVariableName | SpecialParameterName | UserParameterName;
        
		final token Hash = "#";
		final token Dollar = "$";
        
		@{Classification["Keyword"]} final token Local = "local";
		@{Classification["Keyword"]} final token New = "new";
		@{Classification["Keyword"]} final token Let = "let";
		@{Classification["Keyword"]} final token From = "from";
		@{Classification["Keyword"]} token In = "in";
		@{Classification["Keyword"]} final token Into = "into";
		@{Classification["Keyword"]} final token Where = "where";
		@{Classification["Keyword"]} final token Join = "join";
		@{Classification["Keyword"]} final token On = "on";
		@{Classification["Keyword"]} final token Equals = "equals";
		@{Classification["Keyword"]} final token Select = "select";
		@{Classification["Keyword"]} final token Ascending = "ascending" => "Ascending";
		@{Classification["Keyword"]} final token Descending = "descending" => "Descending";
		@{Classification["Keyword"]} final token Orderby = "orderby";
		@{Classification["Keyword"]} final token Group = "group";
		@{Classification["Keyword"]} final token By = "by";
		@{Classification["Keyword"]} final token For = "for";
		@{Classification["Keyword"]} final token Yield = "yield";
		@{Classification["Keyword"]} final token Return = "return";
		@{Classification["Keyword"]} final token Ref = "ref";
		@{Classification["Keyword"]} final token Out = "out";
		@{Classification["Keyword"]} final token Default = "default";
		@{Classification["Keyword"]} final token Typeof = "typeof";
		@{Classification["Keyword"]} final token String = "string" => String;
		@{Classification["Keyword"]} final token Object = "object" => "Object";
		@{Classification["Keyword"]} final token Float = "float" => "Single";
		@{Classification["Keyword"]} final token Double = "double" => "Double";
		@{Classification["Keyword"]} final token Decimal = "decimal" => "Decimal";
		@{Classification["Keyword"]} final token SByte = "sbyte" => "SByte";
		@{Classification["Keyword"]} final token Byte = "byte" => "Byte";
		@{Classification["Keyword"]} final token Short = "short" => "Int16";
		@{Classification["Keyword"]} final token UShort = "ushort" => "UInt16";
		@{Classification["Keyword"]} final token Int = "int" => "Int32";
		@{Classification["Keyword"]} final token UInt = "uint" => "UInt32";
		@{Classification["Keyword"]} final token Long = "long" => "Int64";
		@{Classification["Keyword"]} final token ULong = "ulong" => "UInt64";
		@{Classification["Keyword"]} final token Char = "char" => "Char";
		@{Classification["Keyword"]} final token Bool = "bool" => "Boolean";
        
		@{Classification["Keyword"]} token TypeAs = "as" => "TypeAs";
		@{Classification["Keyword"]} token TypeIs = "is" => "TypeIs";
        
		@{Classification["Operator"]} token And = C.Ampersand => "And";
		@{Classification["Operator"]} token AndAlso = C.Ampersand C.Ampersand => "AndAlso";
		@{Classification["Operator"]} token Or = C.VerticalLine => "Or";
		@{Classification["Operator"]} token ExclusiveOr = C.CircumflexAccent => "ExclusiveOr";
		@{Classification["Operator"]} token OrElse = C.VerticalLine C.VerticalLine => "OrElse";
		@{Classification["Operator"]} token Complement = C.Tilde => "Not";
		@{Classification["Operator"]} token Not = C.ExclamationMark => "IsFalse";
		@{Classification["Operator"]} token Add = C.PlusSign => "Add";
		@{Classification["Operator"]} token Subtract = C.HyphenMinus => "Subtract";
		@{Classification["Operator"]} token Multiply = C.Asterisk => "Multiply";
		@{Classification["Operator"]} token Modulo = C.PercentSign => "Modulo";
		@{Classification["Operator"]} token Divide = C.Solidus => "Divide";
		@{Classification["Operator"]} token Equal = C.EqualsSignEqualsSign => "Equal";
		@{Classification["Operator"]} token NotEqual = C.ExclamationMarkEqualsSign => "NotEqual";
		@{Classification["Operator"]} token LessThan = C.LessThanSign => "LessThan";
		@{Classification["Operator"]} token GreaterThan = C.GreaterThanSign => "GreaterThan";
		@{Classification["Operator"]} token LessThanOrEqual = C.LessThanSignEqualsSign => "LessThanOrEqual";
		@{Classification["Operator"]} token GreaterThanOrEqual = C.GreaterThanSignEqualsSign => "GreaterThanOrEqual";
		@{Classification["Operator"]} token LeftShift = C.LessThanSignLessThanSign => "LeftShift";
		@{Classification["Operator"]} token RightShift = C.GreaterThanSignGreaterThanSign => "RightShift";
		@{Classification["Operator"]} token NullCoalesce = C.QuestionMarkQuestionMark => "NullCoalesce";
	}
}