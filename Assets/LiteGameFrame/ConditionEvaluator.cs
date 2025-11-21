using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Unity.VisualScripting;

public static class ConditionEvaluator
{
    public static bool Evaluate(string expression, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(expression)) return false;
        var compiled = Compile(expression);
        var result = compiled.Root.Evaluate(context ?? _emptyContext);
        return ToBool(result);
    }

    // 编译缓存（进程内存活期）：声明到销毁前只做一次解析
    private static readonly ConcurrentDictionary<string, CompiledExpression> _cache = new();
    private static readonly Dictionary<string, object> _emptyContext = new();

    public sealed class CompiledExpression
    {
        public string Expression { get; }
        internal Node Root { get; }
        internal CompiledExpression(string expr, Node root)
        {
            Expression = expr;
            Root = root;
        }
        public bool Evaluate(Dictionary<string, object> context)
        {
            var result = Root.Evaluate(context ?? _emptyContext);
            return ToBool(result);
        }
    }

    public static CompiledExpression Compile(string expression)
    {
        if (_cache.TryGetValue(expression, out var existed)) return existed;

        var lexer = new Lexer(expression);
        var tokens = lexer.GenerateTokens();
        var parser = new Parser(tokens);
        var root = parser.ParseExpression();
        var compiled = new CompiledExpression(expression, root);
        _cache[expression] = compiled;
        return compiled;
    }

    public static void ClearCache() => _cache.Clear();

    #region 1. 词法分析
    enum TokenType{Operator, LeftParen, RightParen, Value, End}

    // 用于区分字符串字面量与标识符的包装类型（Unity 2021 使用 C# 9，不用 record struct）
    readonly struct StringLiteral
    {
        public readonly string Value;
        public StringLiteral(string value) { Value = value; }
        public override string ToString() => Value;
    }

    record Token(TokenType type, object value, int pos);
    class Lexer
    {
        private readonly string _src;
        private int _p;
        public Lexer(string src) => _src = src;
        public List<Token> GenerateTokens()
        {
            var tokens = new List<Token>();
            while (_p < _src.Length)
            {
                char c = _src[_p];
                if (char.IsWhiteSpace(c)) { _p++; continue; }
                switch (c)//逻辑运算符：&& || ! == != > >= < <= 括号 引号 运算符(暂时不实现)：+-*/
                {
                    case '(':
                        tokens.Add(NewToken(TokenType.LeftParen, "("));
                        _p++; break;
                    case ')':
                        tokens.Add(NewToken(TokenType.RightParen, ")"));
                        _p++; break;
                    case '&' when Peek('&'):
                        tokens.Add(NewToken(TokenType.Operator, "&&"));
                        _p += 2; break;
                    case '|' when Peek('|'):
                        tokens.Add(NewToken(TokenType.Operator, "||"));
                        _p += 2; break;
                    case '=' when Peek('='):
                        tokens.Add(NewToken(TokenType.Operator, "=="));
                        _p += 2; break;
                    case '!' when Peek('='):
                        tokens.Add(NewToken(TokenType.Operator, "!="));
                        _p += 2; break;
                    case '!':
                        tokens.Add(NewToken(TokenType.Operator, "!"));
                        _p++; break;
                    case '>' when Peek('='):
                        tokens.Add(NewToken(TokenType.Operator, ">="));
                        _p += 2; break;
                    case '<' when Peek('='):
                        tokens.Add(NewToken(TokenType.Operator, "<="));
                        _p += 2; break;
                    case '>':
                        tokens.Add(NewToken(TokenType.Operator, ">"));
                        _p++; break;
                    case '<':
                        tokens.Add(NewToken(TokenType.Operator, "<"));
                        _p++; break;
                    case '"':
                        ReadString();
                        break;
                    default:
                        if (char.IsNumber(c) || c == '.') ReadNumber();
                        else if (char.IsLetter(c) || c == '_') ReadIdent();
                        else
                            throw new System.Exception($"Unexpected character: {c} at {_p}");
                        break;
                }//switch end
            }//while end
            tokens.Add(NewToken(TokenType.End, null));
            return tokens;

            bool Peek(char except) => _p + 1 < _src.Length && _src[_p + 1] == except;//栈操作三原语：push（压入）、pop（弹出）、peek（查看栈顶）队列同理：enqueue、dequeue、peek
            Token NewToken(TokenType type, object value) => new Token(type, value, _p);

            void ReadString()
            {
                //"content"
                //-p------p
                _p++;
                char quote = _src[_p];
                int start = _p;
                while (_p < _src.Length && _src[_p] != quote)
                {
                    _p++;
                }
                if (_p >= _src.Length) throw new System.Exception("Unclosed string");
                string content = _src.Substring(start, _p - start);
                _p++;//skip closing quote,exit the string area
                tokens.Add(NewToken(TokenType.Value, new StringLiteral(content)));
                /*var stringBuilder = new StringBuilder();
                char quote = _src[_p++];
                while (_p < _src.Length && _src[_p] != quote)
                {
                    stringBuilder.Append(_src[_p++]);
                }
                if (_p >= _src.Length) throw new System.Exception("Unclosed string");
                _p++;//skip closing quote,exit the string area
                tokens.Add(NewToken(TokenType.String, stringBuilder.ToString()));*/
            }

            void ReadNumber()
            {
                //number
                //p-----p
                int start = _p;
                while (_p < _src.Length && (char.IsDigit(_src[_p]) || _src[_p] == '.'))
                {
                    _p++;
                }//move to the first char which is not a number,exit the number area
                string number = _src.Substring(start, _p - start);
                if (!double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    throw new Exception($"Invalid number at {start}");
                tokens.Add(NewToken(TokenType.Value, d));
            }

            void ReadIdent()
            {
                //identifier
                //p---------p
                int start = _p;
                while (_p < _src.Length && (char.IsLetterOrDigit(_src[_p]) || _src[_p] == '_'))
                {
                    _p++;
                }//move to the first char which is not a part of identifier,exit the identifier area
                string identifier = _src.Substring(start, _p - start);
                if (identifier is "true" or "false")
                    tokens.Add(NewToken(TokenType.Value, bool.Parse(identifier)));
                else
                {
                    tokens.Add(NewToken(TokenType.Value, identifier));
                }
            }
        }//List function end
    }
    #endregion

    #region 2.语法解析
    internal abstract record Node
    {
        public abstract object Evaluate(Dictionary<string, object> context);
    }
    internal record LiteralNode(object Value) : Node
    {
        public override object Evaluate(Dictionary<string, object> context) => Value is StringLiteral s ? s.Value : Value;
    }
    internal record IdentifierNode(string Name) : Node
    {
        public override object Evaluate(Dictionary<string, object> context)
        {
            if (context != null && context.TryGetValue(Name, out var v)) return v;
            throw new Exception($"Identifier '{Name}' not found in context");
        }
    }
    internal record SingleExpressionNode(string Operator, Node Operand) : Node
    {
        public override object Evaluate(Dictionary<string, object> context)
        {
            var val = Operand.Evaluate(context);
            return SingleEvaluate(val, Operator);
        }
    }
    internal record BinaryExpressionNode(Node Left, string Operator, Node Right) : Node
    {
        //冒号继承：继承父类的初始化方法
        public override object Evaluate(Dictionary<string, object> context)
        {
            var left = Left.Evaluate(context);
            var right = Right.Evaluate(context);
            return BinaryEvaluate(left, right, Operator);
        }
    }

    static object SingleEvaluate(object value, string Operator)
    {
        switch (Operator)
        {
            case "!":
                if (value is bool b)
                    return !b;
                else if (IsNumeric(value))
                    return 1 / Convert.ToDouble(value, CultureInfo.InvariantCulture);
                else
                    return value;
            default:
                throw new Exception($"Unknown unary operator: {Operator}");
        }
    }

    static object BinaryEvaluate(object left, object right, string Operator)
    {
        switch (Operator)
        {
            case "^":
                if (AllIsNumeric(left, right))
                    return NumberCalculation(left, right, (a, b) => (decimal)Math.Pow((double)a, (double)b));
                if (ExistType(typeof(string), left, right))
                    return left.ToString() + right.ToString();
                if (ExistType(typeof(bool), left, right))
                    return (bool)left ^ (bool)right;
                else
                    throw new Exception($"Cannot apply operator '^' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            case "*":
                if (ExistType(typeof(string), left, right))
                    return left.ToString() + right.ToString();
                else if (AllIsNumeric(left, right))
                    return NumberCalculation(left, right, (a, b) => a * b);
                else if (ExistType(typeof(bool), left, right))
                    return (bool)left && (bool)right;
                else
                    throw new Exception($"Cannot apply operator '*' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            case "/":
                if (AllIsNumeric(left, right))
                    return NumberCalculation(left, right, (a, b) => a / b);
                else
                    throw new Exception($"Cannot apply operator '/' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            case "+":
                if (ExistType(typeof(string), left, right))
                    return left.ToString() + right.ToString();
                else if (AllIsNumeric(left, right))
                    return NumberCalculation(left, right, (a, b) => a + b);
                else if (ExistType(typeof(bool), left, right))
                    return (bool)left || (bool)right;
                else
                    throw new Exception($"Cannot apply operator '+' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            case "-":
                if (AllIsNumeric(left, right))
                    return NumberCalculation(left, right, (a, b) => a - b);
                else
                    throw new Exception($"Cannot apply operator '-' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            case "==":
                return IsEuqal(left, right);
            case "!=":
                return !IsEuqal(left, right);
            case ">":
                return IsGreater(left, right);
            case "<":
                return IsGreater(right, left);
            case ">=":
                return IsGreater(left, right) || IsEuqal(left, right);
            case "<=":
                return IsGreater(right, left) || IsEuqal(left, right);
            case "&&":
                if (ExistType(typeof(bool), left, right))
                    return (bool)left && (bool)right;
                if (AllIsNumeric(left, right))
                {
                    var dl = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                    var dr = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                    return Math.Min(dl, dr);//数值“与”
                }
                if (ExistType(typeof(string), left, right))
                    return left.ToString() + right.ToString();
                throw new Exception($"Cannot apply operator '&&' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            case "||":
                if (ExistType(typeof(bool), left, right))
                    return (bool)left || (bool)right;
                if (AllIsNumeric(left, right))
                {
                    var dl2 = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                    var dr2 = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                    return Math.Max(dl2, dr2);//数值“或”
                }
                if (ExistType(typeof(string), left, right))
                    return left.ToString() + right.ToString();
                throw new Exception($"Cannot apply operator '||' to operands of type '{left.GetType()}' and '{right.GetType()}'");
            default:
                throw new Exception($"Unknown binary operator: {Operator}");
        }
    }
    #endregion

    #region 3.工具
    static bool ExistType(Type t, object l, object r)
    {
        return l.GetType() == t || r.GetType() == t;
    }
    static bool AllIsNumeric(object l, object r)
    {
        return IsNumeric(l) && IsNumeric(r);
    }
    static bool AllType(Type t, object l, object r)
    {
        return l.GetType() == t && r.GetType() == t;
    }
    static bool IsEuqal(object left, object right)
    {
        if (left is null || right is null) return false;
        if (ReferenceEquals(left, right)) return true;
        if (IsNumeric(left) && IsNumeric(right))
            return CompareNumeric(left, right) == 0;
        if (left.GetType() == right.GetType())
            if (left is IComparable cl)
                return cl.CompareTo(right) == 0;
        return object.Equals(left, right);
    }
    static bool IsGreater(object left, object right)
    {
        if (left is null || right is null) return false;
        if (IsNumeric(left) && IsNumeric(right))
            return CompareNumeric(left, right) > 0;
        if (left.GetType() == right.GetType())
            if (left is IComparable cl)
                return cl.CompareTo(right) > 0;
        if (AllIsNumeric(left, right))
            return CompareNumeric(left, right) > 0;
        if (ExistType(typeof(bool), left, right))
            return (bool)left && !(bool)right;
        throw new Exception($"Cannot compare greater between types '{left.GetType()}' and '{right.GetType()}'");
    }

    private static bool IsNumeric(object o) => o is byte or sbyte or short or ushort
    or int or uint or long or ulong or float or double or decimal;

    /// <summary>
    /// 比较两个数字对象的大小，小于返回-1，等于返回0，大于返回1
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    private static int CompareNumeric(object left, object right)
    {
        decimal dl = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
        decimal dr = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
        return dl.CompareTo(dr);
    }

    static object NumberCalculation(object left, object right, Func<decimal, decimal, decimal> operation)
    {
        decimal dl = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
        decimal dr = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
        return operation(dl, dr);
    }

    #endregion

    #region 4.AST构建
    static int GetPrecedence(string op)
    {
        return op switch
        {
            "!" => 14,
            "^" => 12,
            "*" or "/" => 11,
            "+" or "-" => 10,
            "==" or "!=" or ">" or "<" or ">=" or "<=" => 6,
            "&&" => 2,
            "||" => 1,
            _ => -1,
        };
    }

    static bool IsGreaterOperator(string targetOperator, string compareTo)
    {
        return GetPrecedence(targetOperator) > GetPrecedence(compareTo);
    }

    class Parser
    {
        List<Token> _tokens;
        int _p;
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _p = 0;
        }

        public Node ParseExpression(int minPrecedence = 0)
        {
            var left = ParseUnary();
            while (true)
            {
                var tok = Peek();
                if (tok.type != TokenType.Operator) break;
                string op = (string)tok.value;
                int prec = GetPrecedence(op);
                if (prec < minPrecedence) break;

                // 处理右结合运算符（例如 ^）
                int nextMinPrec = prec + (IsRightAssociative(op) ? 0 : 1);
                Next(); // consume operator
                var right = ParseExpression(nextMinPrec);
                left = new BinaryExpressionNode(left, op, right);
            }
            return left;
        }

        Node ParseUnary()
        {
            var tok = Peek();
            if (tok.type == TokenType.Operator && tok.value is string s && s == "!")
            {
                Next();
                var operand = ParseUnary();
                return new SingleExpressionNode("!", operand);
            }
            return ParsePrimary();
        }

        Node ParsePrimary()
        {
            var tok = Peek();
            switch (tok.type)
            {
                case TokenType.LeftParen:
                    Next();
                    var expr = ParseExpression();
                    Expect(TokenType.RightParen, ")");
                    return expr;
                case TokenType.Value:
                    Next();
                    var val = tok.value;
                    if (val is StringLiteral || val is bool || val is double || val is float || val is int || val is long || val is decimal)
                        return new LiteralNode(val);
                    if (val is string name)
                        return new IdentifierNode(name);
                    return new LiteralNode(val);
                default:
                    throw new Exception($"Unexpected token at {tok.pos}: {tok.type} {tok.value}");
            }
        }

        Token Peek() => _tokens[_p];
        Token Next() => _tokens[_p++];
        void Expect(TokenType t, string display)
        {
            var tok = Next();
            if (tok.type != t) throw new Exception($"Expected '{display}' at {tok.pos}");
        }

        static bool IsRightAssociative(string op) => op == "^";
    }
    #endregion

    #region 5. 布尔转换与辅助
    static bool ToBool(object value)
    {
        if (value is bool b) return b;
        if (IsNumeric(value))
        {
            decimal d = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return d != 0m;
        }
        if (value is string s) return !string.IsNullOrEmpty(s);
        if (value is null) return false;
        // 其他对象视为 true（有对象即真）
        return true;
    }
    #endregion
}