using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class ConditionEvaluator
{
    public static bool Evaluate(string expression, Dictionary<string, object> context)
    {
        return false;
    }

    #region 1. 词法分析
    enum TokenType
    {
        Operator, LeftParen, RightParen,
        Identifier, Boolean, Number, String, End
    }

    record Token(TokenType type, string value, object val, int pos);
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
                        else if (char.IsLetter(c) || c == '_') ;
                        else
                            throw new System.Exception($"Unexpected character: {c} at {_p}");
                        break;
                }//switch end
            }//while end
            tokens.Add(NewToken(TokenType.End, ""));
            return tokens;

            bool Peek(char except) => _p + 1 < _src.Length && _src[_p + 1] == except;//栈操作三原语：push（压入）、pop（弹出）、peek（查看栈顶）队列同理：enqueue、dequeue、peek
            Token NewToken(TokenType type, string text, object value = null) => new Token(type, text, value, _p);

            void ReadString()
            {
                //"content"
                //p-------p
                char quote = _src[_p];
                int start = _p;
                while (_p < _src.Length && _src[_p] != quote)
                {
                    _p++;
                }
                if (_p >= _src.Length) throw new System.Exception("Unclosed string");
                string content = _src.Substring(start + 1, _p - start - 1);
                _p++;//skip closing quote,exit the string area
                tokens.Add(NewToken(TokenType.String, content));
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
                tokens.Add(NewToken(TokenType.Number, number, double.Parse(number)));
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
                    tokens.Add(NewToken(TokenType.Boolean, identifier, bool.Parse(identifier)));
                else
                    tokens.Add(NewToken(TokenType.Identifier, identifier));
            }
        }//List function end
    }
    #endregion

    #region 2.语法解析 -> AST
    abstract record Node
    {
        public abstract bool Evaluate();
        protected IReadOnlyDictionary<string, object> Value;
        protected Node(IReadOnlyDictionary<string, object> value) => Value = value;
    }

    record BinaryExpressionNode(Node Left, string Operator, Node Right) : Node(new Dictionary<string, object>())
    {
        //冒号继承：继承父类的初始化方法
        public override bool Evaluate()
        {
            //实现二元表达式的求值逻辑
            var l = Left.Evaluate();
            var r = Right.Evaluate();
            return Operator switch
            {
                "&&" => l && r,
                "||" => l || r,
                //"==" => Obj
            };
            return false;
        }
    }
    #endregion

    #region 3.工具
    static bool CompareObject(object? left, object? right)
    {
        if (left is null || right is null) return false;
        if (ReferenceEquals(left, right)) return true;
        if (left.GetType() == right.GetType())
            if (left is IComparable cl)
                return cl.CompareTo(right) == 0;
        if (IsNumeric(left) && IsNumeric(right))
            return CompareNumeric(left, right) == 0;
        return object.Equals(left, right);
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
    #endregion
}