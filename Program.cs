// See https://aka.ms/new-console-template for more information
using Microsoft.SqlServer.TransactSql.ScriptDom;

internal class Program
{
    private static void Main(string[] args)
    {
        var parser = new TSql150Parser(true);

        var reader = new StreamReader(File.OpenRead("sample.sql"));

        IList<ParseError> errors;

        var tokenList = parser.Parse(reader, out errors);

        var visitor = new MyVisitor();

        tokenList.Accept(visitor);
    }
}

public class MyVisitor : TSqlFragmentVisitor
{
    int? headerEndsAtOffset = null;

    public override void Visit(TSqlStatement node)
    {
        var createProcedureStatement = node as CreateProcedureStatement;

        if (createProcedureStatement != null)
        {
            headerEndsAtOffset = createProcedureStatement.ScriptTokenStream
            .Where(t => t?.TokenType == TSqlTokenType.As)
            .Select(t => t.Offset + t.Text.Length)
            .Last();

            Console.WriteLine($"CREATE PROCEDURE {createProcedureStatement.ProcedureReference.Name.Identifiers.Single().Value}");

            Console.Write(String.Join(",\n", createProcedureStatement.Parameters.Select(p => $"  {p.VariableName.Value} {p.DataType.Name.Identifiers[0].Value}")));

            Console.WriteLine("\nAS");

            foreach (var statement in createProcedureStatement.StatementList.Statements)
            {
                foreach (var token in statement.ScriptTokenStream.Where(t => t.Offset > headerEndsAtOffset))
                {
                    Console.Write(token.Text);
                }
            }
        }

        base.Visit(node);
    }

}