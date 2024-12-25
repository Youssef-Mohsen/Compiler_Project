using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Int, Float, String, Read, Write, Main,
    Repeat, Until, If, Elseif, Else, Then, Return, Endl, End, Dot,
    semicolon, Comma, LParanthesis, RParanthesis,
    EqualOp, NotEqualOp, LessThanOp, GreaterThanOp, AndOp, OrOp,
    PlusOp, MinusOp, MultiplyOp, DivideOp, AssignOp, Idenifier, Number,FloatNumber, Comment, LCurlyBraces, RCurlyBraces, constant, StringLiteral
}

namespace TINY_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();
        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.Elseif);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("main", Token_Class.Main);

            Operators.Add(".", Token_Class.Dot);
            Operators.Add(";", Token_Class.semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LCurlyBraces);
            Operators.Add("}", Token_Class.RCurlyBraces);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("||", Token_Class.OrOp);
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add(":=", Token_Class.AssignOp);
        }
        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();

            for (int i = 0; i < SourceCode.Length;)
            {
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = string.Empty;
                int j = i + 1;

                // Ignore whitespace and newline characters
                if (char.IsWhiteSpace(CurrentChar))
                {
                    i++;
                    continue;
                }

                // Handling identifiers and keywords
                if (char.IsLetter(CurrentChar))
                {
                    CurrentLexeme += CurrentChar.ToString();
                    while (j < SourceCode.Length && (isLetter(SourceCode[j]) || isDigit(SourceCode[j])))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                }
                // Handling numbers (including floats)
                else if (isDigit(CurrentChar))
                {
                    CurrentLexeme += CurrentChar.ToString();
                    while (j < SourceCode.Length && (isDigit(SourceCode[j]) || (SourceCode[j] == '.')  || isLetter(SourceCode[j])))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                }
                // Handling comments
                else if (CurrentChar == '/')
                {
                    CurrentLexeme += CurrentChar.ToString();
                    if (j < SourceCode.Length && SourceCode[j] == '*')
                    {
                        CurrentLexeme += SourceCode[j++];
                        while (j < SourceCode.Length - 1 && !(SourceCode[j] == '*' && SourceCode[j + 1] == '/'))
                        {
                            CurrentLexeme += SourceCode[j++];
                        }
                        if (j < SourceCode.Length - 1)
                        {
                            CurrentLexeme += "*/";
                            j += 2;
                        }
                    }
                }
                // Handling string literals
                else if (CurrentChar == '"')
                {
                    CurrentLexeme += CurrentChar.ToString();
                    while (j < SourceCode.Length && SourceCode[j] != '"')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    if (j < SourceCode.Length && SourceCode[j] == '"')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                }
                // Handling operators
                else
                {
                    if (j < SourceCode.Length && ((CurrentChar == '&' && SourceCode[j] == '&') ||
                                                  (CurrentChar == '|' && SourceCode[j] == '|') ||
                                                  (CurrentChar == '<' && SourceCode[j] == '>') ||
                                                  (CurrentChar == ':' && SourceCode[j] == '=')))
                    {
                        CurrentLexeme += CurrentChar.ToString();
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    else if((j < SourceCode.Length && (CurrentChar == '<' && SourceCode[j] == '=') ||
                                                      (CurrentChar == '>' && SourceCode[j] == '=')))
                    {
                        Errors.Error_List.Add("Invalid Lexeme " + CurrentChar + SourceCode[j] + "\n");
                        j++; 
                    }
                    else
                    {
                        CurrentLexeme += CurrentChar.ToString();
                    }
                }

                FindTokenClass(CurrentLexeme);
                i = j;
            }

            TINY_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            // empty lexeme
            if (Lex.Length == 0)
                return;

            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Tok.lex))
            {
                TC = ReservedWords[Tok.lex];
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it an operator?
            else if (Operators.ContainsKey(Tok.lex))
            {
                TC = Operators[Tok.lex];
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            //Is it an identifier?
            else if (IsIdentifier(Tok.lex))
            {
                TC = Token_Class.Idenifier;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }

            //Is it a Constant?
            else if (isNumber(Tok.lex))
            {
                TC = Token_Class.Number;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            // Is it a string
            else if (isStringLiteral(Tok.lex))
            {
                TC = Token_Class.StringLiteral;
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            // Is it a comment
            else if (IsComment(Tok.lex))
            {
                TC = Token_Class.Comment;
                Tok.token_type = TC;               
            }
            // Is it a error
            else
            {
                Errors.Error_List.Add("Invalid Lexeme " + Tok.lex + "\n");
                
            }
        }
        public bool IsIdentifier(string lex)
        {
            Regex reg = new Regex(@"^([a-zA-Z])([0-9a-zA-Z])*$", RegexOptions.Compiled);
            return reg.IsMatch(lex);
        }
        bool isNumber(string lex)
        {
            var rx = new Regex(@"^\d+(\.\d+)?$", RegexOptions.Compiled);
            bool isValid = rx.IsMatch(lex);
            return isValid;
        }
      
        bool isStringLiteral(string lex)
        {
            bool isValid = true;
            int len = lex.Length;
            if (!(lex[0] == '"' && lex[len - 1] == '"'))
                isValid = false;
            if(lex.Length <= 1 )
                isValid = false;
            return isValid;
        }
        public bool IsComment(string lex)
        {
            return (lex.Length >= 4 && lex.StartsWith("/*") && lex.EndsWith("*/"));
        }
        bool isLetter(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
        }
        public bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}