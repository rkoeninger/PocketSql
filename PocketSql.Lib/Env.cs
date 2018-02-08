using System.Data;

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
    }
}
