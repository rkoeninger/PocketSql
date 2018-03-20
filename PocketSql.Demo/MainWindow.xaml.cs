using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using PocketSql;

namespace PocketSql.Demo
{
    public partial class MainWindow : Window
    {
        private readonly Engine engine = new Engine(140);

        public MainWindow()
        {
            InitializeComponent();

            using (var connection = engine.GetConnection())
            {
                connection.Execute(File.ReadAllText("Init.sql"));
            }

            UpdateSchemaTreeView();
        }

        private void RunButtonClick(object sender, RoutedEventArgs e)
        {
            using (var connection = engine.GetConnection())
            {
                var results = connection.ExecuteReader(ScriptTextBox.Text);
                UpdateResultsDataGrid(results);
                UpdateSchemaTreeView();
            }
        }

        private void UpdateResultsDataGrid(IDataReader results)
        {
            var output = new StringBuilder();

            if (!results.IsClosed)
            {
                output.AppendLine(string.Join(" | ",
                    Enumerable.Range(0, results.FieldCount)
                        .Select(results.GetName)));
                output.AppendLine();

                while (results.Read())
                {
                    output.AppendLine(string.Join(" | ",
                        Enumerable.Range(0, results.FieldCount)
                            .Select(results.GetValue)));
                }
            }

            ResultsTextBox.Text = output.ToString();
        }

        private void UpdateSchemaTreeView()
        {
            SchemaTreeView.Items.Clear();
            SchemaTreeView.Items.Add(
                Node("Databases", engine.Databases.Select(d =>
                    Node(d.Name, d.Schemas.Select(s =>
                        Node(s.Name, new[] {
                            Node("Tables", s.Tables.Select(t =>
                                Node(t.Name, t.Columns.Select(c =>
                                    Node($"{c.Name.Last()} {TypeName(c.Type)} {(c.Nullable ? "null" : "")}")).Concat(new []{
                                        Node($"RowCount = {t.Rows.Count}")})))),
                            Node("Views", s.Views.Select(v =>
                                Node(v.Name))),
                            Node("Procedures", s.Procedures.Select(p =>
                                Node(p.Name.Last()))),
                            Node("Functions", s.Functions.Select(f =>
                                Node(f.Name.Last()))) }))))));
        }

        private TreeViewItem Node(string text, params TreeViewItem[] children) =>
            Node(text, children.AsEnumerable());

        private TreeViewItem Node(string text, IEnumerable<TreeViewItem> children)
        {
            var node = new TreeViewItem
            {
                Header = text
            };

            foreach (var child in children)
            {
                node.Items.Add(child);
            }

            return node;
        }

        private static string TypeName(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    return "varchar";
                case DbType.Int32:
                    return "int";
                default:
                    return type.ToString();
            }
        }
    }
}
