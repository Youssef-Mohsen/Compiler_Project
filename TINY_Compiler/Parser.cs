using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TINY_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        private int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;
        private Boolean MainFunctionExecuted = false;
        public Node StartParsing(List<Token> TokenStream)
        {
            this.TokenStream = TokenStream;
            root = new Node("Root Node");
            root.Children.Add(Program());

            if (!MainFunctionExecuted)
            {
                Errors.Error_List.Add("The code misses the main function !!!!");
            }

            return root;
        }
        private Node FunctionStatements(Node functionStatements)
        {
            if ((IsvalidToken(Token_Class.Int) || IsvalidToken(Token_Class.Float) || IsvalidToken(Token_Class.String))
                && TokenStream[InputPointer + 1].token_type == Token_Class.Idenifier)
            {
                functionStatements.Children.Add(FunctionStatement());
                FunctionStatements(functionStatements);
            }
            else if ((IsvalidToken(Token_Class.Int) || IsvalidToken(Token_Class.Float) || IsvalidToken(Token_Class.String))
                && TokenStream[InputPointer + 1].token_type == Token_Class.Main)
            {
                MainFunctionExecuted = true;
            }             
            else if (IsvalidToken(Token_Class.Comment))
            {
                    functionStatements.Children.Add(match(Token_Class.Comment));
                    FunctionStatements(functionStatements);
            }
            
            return functionStatements;
        }
        private Node Program()
        {
            Node program = new Node("Program");
            Node functionStatements = new Node("Function_Satements");
            program.Children.Add(FunctionStatements(functionStatements));
            Node mainFunction = new Node("Main_Function");
            if (MainFunctionExecuted)
            {
                program.Children.Add(MainFunction(mainFunction));
            }
            else
            {
                program.Children.Add(mainFunction);
            }
            return program;
        }
        private Node MainFunction(Node mainFunction)
        {          
            mainFunction.Children.Add(Datatype());
            mainFunction.Children.Add(match(Token_Class.Main));
            mainFunction.Children.Add(match(Token_Class.LParanthesis));
            mainFunction.Children.Add(match(Token_Class.RParanthesis));
            mainFunction.Children.Add(FunctionBody());
            return mainFunction;
        }
        private Node FunctionStatement()
        {
            Node functionStatement = new Node("Function_Statement");
            functionStatement.Children.Add(FunctionDeclaration());
            functionStatement.Children.Add(FunctionBody());
            return functionStatement;
        }
        private Node FunctionBody()
        {
            Node functionBody = new Node("Function_Body");
            functionBody.Children.Add(match(Token_Class.LCurlyBraces));
            Node statements = new Node("Statements");
            functionBody.Children.Add(Statements(statements));
            functionBody.Children.Add(ReturnStatement());
            functionBody.Children.Add(match(Token_Class.RCurlyBraces));
            return functionBody;
        }
        private Node FunctionDeclaration()
        {
            Node functionDeclaration = new Node("Function_Declaration");
            functionDeclaration.Children.Add(Datatype());          
            functionDeclaration.Children.Add(FunctionName());
            functionDeclaration.Children.Add(match(Token_Class.LParanthesis));
            functionDeclaration.Children.Add(ParameterList());
            functionDeclaration.Children.Add(match(Token_Class.RParanthesis));           
            return functionDeclaration;
        }
        private Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());
            parameter.Children.Add(match(Token_Class.Idenifier));
            return parameter;
        }
        private Node ParameterList()
        {
            Node parameterList = new Node("Parameter_List");
            if (IsvalidToken(Token_Class.Int) || IsvalidToken(Token_Class.Float) || IsvalidToken(Token_Class.String))
            {
                parameterList.Children.Add(Parameter());
                Node moreParameters = new Node("More_Parameters");
                parameterList.Children.Add(MoreParameters(moreParameters));
            }
            return parameterList;
        }
        private Node MoreParameters(Node moreParameters)
        {
            if (IsvalidToken(Token_Class.Comma))
            {
                moreParameters.Children.Add(match(Token_Class.Comma));
                moreParameters.Children.Add(Parameter());
                MoreParameters(moreParameters);
            }
            return moreParameters;
        }
        private Node FunctionName()
        {
            Node functionName = new Node("Function_Name");
            functionName.Children.Add(match(Token_Class.Idenifier));
            return functionName;
        }
       
        private Node RepeatStatement()
        {
            Node repeatStatement = new Node("Repeat_Statement");
            repeatStatement.Children.Add(match(Token_Class.Repeat));
            Node statements = new Node("Statements");
            repeatStatement.Children.Add(Statements(statements));
            repeatStatement.Children.Add(match(Token_Class.Until));
            repeatStatement.Children.Add(ConditionStatement());
            return repeatStatement;
        }
        private Node IfStatement()
        {
            Node ifStatement = new Node("If_Statement");
            ifStatement.Children.Add(match(Token_Class.If));
            ifStatement.Children.Add(ConditionStatement());
            if (IsvalidToken(Token_Class.Comment))
            {
                 Node commentStatements = new Node("Comment_Statements");
                 ifStatement.Children.Add(CommentStatements(commentStatements));
            }
            ifStatement.Children.Add(match(Token_Class.Then));
            Node statements = new Node("Statements");
            ifStatement.Children.Add(Statements(statements));
            ifStatement.Children.Add(ElseClose());
            return ifStatement;
        }
        private Node ElseIfStatment()
        {
            Node elseIfStatment = new Node("Else_If_Statment");
            elseIfStatment.Children.Add(match(Token_Class.Elseif));
            elseIfStatment.Children.Add(ConditionStatement());
            if (IsvalidToken(Token_Class.Comment))
            {
                Node commentStatements = new Node("Comment_Statements");
                elseIfStatment.Children.Add(CommentStatements(commentStatements));
            }
            elseIfStatment.Children.Add(match(Token_Class.Then));
            Node statements = new Node("Statements");
            elseIfStatment.Children.Add(Statements(statements));
            elseIfStatment.Children.Add(ElseClose());
            return elseIfStatment;
        }
        private Node ElseStatment()
        {
            Node elseStatment = new Node("Else_Statment");
            elseStatment.Children.Add(match(Token_Class.Else));
            Node statements = new Node("Statements");
            elseStatment.Children.Add(Statements(statements));
            elseStatment.Children.Add(match(Token_Class.End));
            return elseStatment;
        }
        private Node ElseClose()
        {
            Node elseClose = new Node("Else_Close");
            if (IsvalidToken(Token_Class.Elseif))
            {
                elseClose.Children.Add(ElseIfStatment());
            }
            else if (IsvalidToken(Token_Class.Else))
            {
                elseClose.Children.Add(ElseStatment());
            }
            else
            {
                elseClose.Children.Add(match(Token_Class.End));
            }
            return elseClose;
        }
        private Node CommentStatements(Node commentStatements)
        {         
            if (IsvalidToken(Token_Class.Comment))
            {
                commentStatements.Children.Skip(1);
                CommentStatements(commentStatements);
            }           
            return commentStatements;
        }
        private Node ConditionStatement()
        {
            Node conditionStatement = new Node("Condition_Statement");
            conditionStatement.Children.Add(Condition());
            Node conditionEnd = new Node("Condition_End");
            conditionStatement.Children.Add(ConditionEnd(conditionEnd));
            return conditionStatement;
        }
        private Node BooleanOperator()
        {
            Node booleanOperator = new Node("Boolean_Operator");
            if (IsvalidToken(Token_Class.OrOp))
            {
                booleanOperator.Children.Add(match(Token_Class.OrOp));
            }
            else
            {
                booleanOperator.Children.Add(match(Token_Class.AndOp));
            }
            return booleanOperator;
        }
        private Node ConditionEnd(Node conditionEnd)
        {
            if(IsvalidToken(Token_Class.OrOp) || IsvalidToken(Token_Class.AndOp))
            {
                conditionEnd.Children.Add(BooleanOperator());
                conditionEnd.Children.Add(Condition());
                ConditionEnd(conditionEnd);
            }
            return conditionEnd;
        }


        private Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(Token_Class.Idenifier));
            condition.Children.Add(ConditionOperator());
            condition.Children.Add(Term());
            return condition;
        }
        private Node ConditionOperator()
        {
            Node conditionOperator = new Node("Condition_Operator");
            if (IsvalidToken(Token_Class.EqualOp))
            {
                conditionOperator.Children.Add(match(Token_Class.EqualOp));
            }
            else if (IsvalidToken(Token_Class.LessThanOp))
            {
                conditionOperator.Children.Add(match(Token_Class.LessThanOp));
            }
            else if (IsvalidToken(Token_Class.GreaterThanOp))
            {
                conditionOperator.Children.Add(match(Token_Class.GreaterThanOp));
            }

            else if (IsvalidToken(Token_Class.NotEqualOp))
            {
                conditionOperator.Children.Add(match(Token_Class.NotEqualOp));
            }

            return conditionOperator;
        }
        private Node ReturnStatement()
        {
            Node returnStatement = new Node("Return_Statement");
            returnStatement.Children.Add(match(Token_Class.Return));
            returnStatement.Children.Add(Expression());
            returnStatement.Children.Add(match(Token_Class.semicolon));
            return returnStatement;
        }
        private Node ReadStatement()
        {
            Node readStatement = new Node("Read_Statement");
            readStatement.Children.Add(match(Token_Class.Read));
            readStatement.Children.Add(match(Token_Class.Idenifier));
            readStatement.Children.Add(match(Token_Class.semicolon));
            return readStatement;
        }
        private Node WriteStatement()
        {
            Node writeStatement = new Node("Write_Statement");
            writeStatement.Children.Add(match(Token_Class.Write));
            writeStatement.Children.Add(WriteExpression());
            writeStatement.Children.Add(match(Token_Class.semicolon));
            return writeStatement;
        }
        Node WriteExpression()
        {
            Node writeExpression = new Node("Write_Expression");
            if (IsvalidToken(Token_Class.Endl))
            {
                writeExpression.Children.Add(match(Token_Class.Endl));
            }
            else
            {
                writeExpression.Children.Add(Expression());
            }
            return writeExpression;
        }
        private Node DeclarationStatement()
        {
            Node declarationStatement = new Node("Declaration_Statement");
            declarationStatement.Children.Add(Datatype());           
            declarationStatement.Children.Add(DeclareRest());
            declarationStatement.Children.Add(match(Token_Class.semicolon));
            return declarationStatement;
        }
        private Node DeclareRest()
        {

            Node declareRest = new Node("Declare_Rest");
            if (IsvalidToken(Token_Class.Idenifier)
               && (TokenStream[InputPointer + 1].token_type == Token_Class.Comma
               || TokenStream[InputPointer + 1].token_type == Token_Class.semicolon))
            {
                declareRest.Children.Add(match(Token_Class.Idenifier));
            }
            if (IsvalidToken(Token_Class.Comma))
            {
                Node declareIdentifiers = new Node("Declare_Identifiers");
                declareRest.Children.Add(DeclareIdentifiers(declareIdentifiers));
            }
            else if(IsvalidToken(Token_Class.Idenifier) && TokenStream[InputPointer + 1].token_type == Token_Class.AssignOp)
            {
                declareRest.Children.Add(AssignmentStatement());
                if (IsvalidToken(Token_Class.Comma))
                {
                    Node declareIdentifiers = new Node("Declare_Identifiers");
                    declareRest.Children.Add(DeclareIdentifiers(declareIdentifiers));
                }
            }
            return declareRest;
        }
        private Node DeclareIdentifiers(Node declareIdentifiers)
        {
            
            if (IsvalidToken(Token_Class.Comma))
            {
                declareIdentifiers.Children.Add(match(Token_Class.Comma));
                if(IsvalidToken(Token_Class.Idenifier) && TokenStream[InputPointer + 1].token_type == Token_Class.AssignOp)
                {
                    declareIdentifiers.Children.Add(AssignmentStatement());
                }
                else
                {
                    declareIdentifiers.Children.Add(match(Token_Class.Idenifier));
                }
                DeclareIdentifiers(declareIdentifiers);
            }
            return declareIdentifiers;
        }
        private Node Datatype()
        {
            Node datatype = new Node("Datatype");
            if (IsvalidToken(Token_Class.Int) )
            {
                datatype.Children.Add(match(Token_Class.Int));
            }
            else if (IsvalidToken(Token_Class.Float))
            {
                datatype.Children.Add(match(Token_Class.Float));
            }
            else if (IsvalidToken(Token_Class.String))
            {
                datatype.Children.Add(match(Token_Class.String));
            }

            return datatype;
        }
        private Node AssignmentStatement()
        {
            Node assignmentStatement = new Node("Assignment_Statement");            
            assignmentStatement.Children.Add(match(Token_Class.Idenifier));            
            assignmentStatement.Children.Add(match(Token_Class.AssignOp));
            assignmentStatement.Children.Add(Expression());
            return assignmentStatement;
        }
        private Node Expression()
        {
            Node expression = new Node("Expression");
            if (IsvalidToken(Token_Class.StringLiteral))
            {
                expression.Children.Add(match(Token_Class.StringLiteral));
            }
            else if (IsvalidToken(Token_Class.LParanthesis))
            {

                expression.Children.Add(match(Token_Class.LParanthesis));
                expression.Children.Add(Equation());
                expression.Children.Add(match(Token_Class.RParanthesis));
                Node equationEnd = new Node("Equation_End");
                expression.Children.Add(EquationEnd(equationEnd));
            }
            else
            {
                expression.Children.Add(Term());
                Node equationEnd = new Node("Equation_End");
                expression.Children.Add(EquationEnd(equationEnd));
            }
            return expression;
        }
        private Node Equation()
        {
            Node equation = new Node("Equation");
            if (IsvalidToken(Token_Class.Number) || IsvalidToken(Token_Class.Idenifier))
            {
                equation.Children.Add(Term());
                Node equationEnd = new Node("Equation_End");
                equation.Children.Add(EquationEnd(equationEnd));
            }
            else if (IsvalidToken(Token_Class.LParanthesis))
            {
                equation.Children.Add(match(Token_Class.LParanthesis));
                equation.Children.Add(Equation());
                equation.Children.Add(match(Token_Class.RParanthesis));
                Node equationEnd = new Node("Equation_End");
                equation.Children.Add(EquationEnd(equationEnd));
            }
            
            return equation;
        }
        private Node EquationEnd(Node equationEnd)
        {
            if (IsvalidToken(Token_Class.PlusOp) || IsvalidToken(Token_Class.MinusOp) ||
                IsvalidToken(Token_Class.MultiplyOp) || IsvalidToken(Token_Class.DivideOp))
            {
                equationEnd.Children.Add(ArithmaticOperator());
                equationEnd.Children.Add(Equation());
                EquationEnd(equationEnd);
            }
            return equationEnd;
        }

        private Node ArithmaticOperator()
        {
            Node arithmaticOperator = new Node("Arithmatic_Operator");
            if (IsvalidToken(Token_Class.PlusOp))
            {
                arithmaticOperator.Children.Add(match(Token_Class.PlusOp));
            }
            else if (IsvalidToken(Token_Class.MinusOp))
            {
                arithmaticOperator.Children.Add(match(Token_Class.MinusOp));
            }
            else if (IsvalidToken(Token_Class.MultiplyOp))
            {
                arithmaticOperator.Children.Add(match(Token_Class.MultiplyOp));
            }

            else if (IsvalidToken(Token_Class.DivideOp))
            {
                arithmaticOperator.Children.Add(match(Token_Class.DivideOp));
            }
            return arithmaticOperator;
        }
        private Node Term()
        {
            Node term = new Node("Term");
            if (IsvalidToken(Token_Class.Idenifier) && TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
            {
                    term.Children.Add(FunctionCall());
            }
            else if (IsvalidToken(Token_Class.Idenifier))
            {
                term.Children.Add(match(Token_Class.Idenifier));
            }
            else if (IsvalidToken(Token_Class.Number))
            {
                term.Children.Add(match(Token_Class.Number));
            }
            return term;
        }
        private Node FunctionCall()
        {
            Node functionCall = new Node("Function_Call");
            functionCall.Children.Add(match(Token_Class.Idenifier));
            functionCall.Children.Add(match(Token_Class.LParanthesis));
            functionCall.Children.Add(IdentifierList());
            functionCall.Children.Add(match(Token_Class.RParanthesis));
            return functionCall;
        }
        private Node IdentifierList()
        {
            Node identifierList = new Node("Identifier_List");
            if (IsvalidToken(Token_Class.Idenifier))
            {
                identifierList.Children.Add(match(Token_Class.Idenifier));
                Node moreIdentifiers = new Node("More_Identifiers");
                identifierList.Children.Add(MoreIdentifiers(moreIdentifiers));
            }
            return identifierList;
        }
        private Node MoreIdentifiers(Node moreIdentifiers)
        {
            if (IsvalidToken(Token_Class.Comma))
            {
                moreIdentifiers.Children.Add(match(Token_Class.Comma));
                moreIdentifiers.Children.Add(match(Token_Class.Idenifier));
                MoreIdentifiers(moreIdentifiers);
            }
            return moreIdentifiers;
        }
        private Node Statements(Node statements)
        {
             //Node statements = new Node("Statements");
            if (IsvalidToken(Token_Class.Idenifier)
             || IsvalidToken(Token_Class.String) || IsvalidToken(Token_Class.Int) || IsvalidToken(Token_Class.Float)
             || IsvalidToken(Token_Class.If) || IsvalidToken(Token_Class.Repeat)
             || IsvalidToken(Token_Class.Read) || IsvalidToken(Token_Class.Write) 
             || IsvalidToken(Token_Class.Comment))
            {
                statements.Children.Add(Statement());
                Statements(statements);
            }
            return statements;
        }
        private Node Statement()
        {
            Node statement = new Node("Statement");
            if (IsvalidToken(Token_Class.If))
            {
                statement.Children.Add(IfStatement());
            }
            else if (IsvalidToken(Token_Class.Repeat))
            {
                statement.Children.Add(RepeatStatement());
            }
            else if (IsvalidToken(Token_Class.Idenifier) && TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
            {
                statement.Children.Add(FunctionCall());
            }
            else if (IsvalidToken(Token_Class.Read))
            {
                statement.Children.Add(ReadStatement());
            }
            else if (IsvalidToken(Token_Class.Write))
            {
                statement.Children.Add(WriteStatement());
            }
            else if (IsvalidToken(Token_Class.Int) || IsvalidToken(Token_Class.Float) || IsvalidToken(Token_Class.String))
            {
                statement.Children.Add(DeclarationStatement());
            }
            else if (IsvalidToken(Token_Class.Idenifier) && TokenStream[InputPointer+1].token_type == Token_Class.AssignOp)
            {
                statement.Children.Add(AssignmentStatement());
                statement.Children.Add(match(Token_Class.semicolon));
            }
            else if (IsvalidToken(Token_Class.Comment))
            {
                //statement.Children.Add(match(Token_Class.Comment));
                statement.Children.Skip(1);
            }
            return statement; 
        }
        private bool IsvalidToken(Token_Class token)
        {
            return (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == token);
        }
        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
