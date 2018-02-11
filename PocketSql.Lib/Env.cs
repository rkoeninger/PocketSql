using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql
{
    public class Env
    {
        public static Env Of(EngineConnection connection, IDataParameterCollection parameters)
        {
            var env = new Env();
            env.AddAll(parameters);
            env.Engine = connection.Engine;
            env.DefaultDatabase = connection.Database;
            return env;
        }

        public Namespace<object> Vars { get; private set; } = new Namespace<object>();
        public object ReturnValue { get; set; }
        public int FetchStatus { get; set; } = -1;
        public int RowCount { get; set; } = -1;
        public Engine Engine { get; private set; }
        public string DefaultDatabase { get; set; } = "master";
        public string DefaultSchema { get; set; } = "dbo";

        public Env Fork() => new Env
        {
            Engine = Engine,
            DefaultDatabase = DefaultDatabase,
            DefaultSchema = DefaultSchema,
            Vars = Vars.Copy()
        };

        public void AddAll(IDataParameterCollection parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                Vars.Declare(Naming.Parameter(parameter.ParameterName), parameter.Value);
            }
        }

        public Database Database => Engine.Databases[DefaultDatabase];
        public Schema Schema => Database.Schemas[DefaultSchema];
        public Namespace<Function> Functions => Schema.Functions;
        public Namespace<Procedure> Procedures => Schema.Procedures;
        public Namespace<DataTable> Tables => Schema.Tables;
        public Namespace<View> Views => Schema.Views;

        public Database GetDatabase(Identifier id) =>
            id?.Value == null ? Engine.Databases[DefaultDatabase] : Engine.Databases[id.Value];

        public Schema GetSchema(Database database, Identifier id) =>
            id?.Value == null ? database.Schemas[DefaultSchema] : database.Schemas[id.Value];

        public Schema GetSchema(SchemaObjectName id) =>
            GetSchema(GetDatabase(id.DatabaseIdentifier), id.SchemaIdentifier);

        public Function GetFunction(Schema schema, Identifier id) =>
            schema.Functions[id.Value];

        public Procedure GetProcedure(Schema schema, Identifier id) =>
            schema.Procedures[id.Value];

        public DataTable GetTable(Schema schema, Identifier id) =>
            schema.Tables[id.Value];

        public View GetView(Schema schema, Identifier id) =>
            schema.Views[id.Value];

        public Function GetFunction(SchemaObjectName id) =>
            GetFunction(GetSchema(id), id.BaseIdentifier);

        public Procedure GetProcedure(SchemaObjectName id) =>
            GetProcedure(GetSchema(id), id.BaseIdentifier);

        public DataTable GetTable(SchemaObjectName id) =>
            GetTable(GetSchema(id), id.BaseIdentifier);

        public View GetView(SchemaObjectName id) =>
            GetView(GetSchema(id), id.BaseIdentifier);

        public object GetGlobal(string name)
        {
            switch (name.ToLower())
            {
                case "@@fetch_status": return FetchStatus;
                case "@@rowcount": return RowCount;
                default: throw new Exception($"Global \"{name}\" not defined");
            }
        }
    }
}
