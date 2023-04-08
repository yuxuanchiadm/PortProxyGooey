#region + -- NAMESPACE IMPORTS -- +

    using NStandard;
    using SQLib.Sqlite;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

#endregion

namespace PortProxyGUI.Data
{
    public class ApplicationDbScope : SqliteScope<ApplicationDbScope>
    {
        public static readonly string AppDbDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%ALLUSERSPROFILE%"), "PortProxyGUI");
        public static readonly string AppDbFile = Path.Combine(AppDbDirectory, "config.db");

        public static ApplicationDbScope FromFile(string file)
        {
            string dir = Path.GetDirectoryName(file);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(file))
            {
#if NETCOREAPP3_0_OR_GREATER
#else
                System.Data.SQLite.SQLiteConnection.CreateFile(file);
#endif
            }

            ApplicationDbScope scope = new($"Data Source=\"{file}\"");
            scope.Migrate();
            return scope;
        }

        public ApplicationDbScope(string connectionString) : base(connectionString)
        {
        }

        public override void Initialize()
        {
        }

        #region + -- APP: MIGRATION -- +

        public void Migrate() => new MigrationUtil(this).MigrateToLast();

        public Migration GetLastMigration()
        {
            return SqlQuery<Migration>($"SELECT * FROM __history ORDER BY MigrationId DESC LIMIT 1;").First();
        }

        #endregion

        public IEnumerable<Rule> Rules => SqlQuery<Rule>($"SELECT * FROM Rules;");

        public Rule GetRule(string type, string listenOn, int listenPort)
        {
            return SqlQuery<Rule>($"SELECT * FROM Rules WHERE Type={type} AND ListenOn={listenOn} AND ListenPort={listenPort} LIMIT 1;").FirstOrDefault();
        }

        #region + -- RULE: ADD -- +

        /// <summary>
        /// Add a new proxy rule into the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void Add<T>(T obj) where T : class
        {
            string newid = Guid.NewGuid().ToString();
            if (obj is Rule rule)
            {
                Sql($"INSERT INTO Rules (Id, Type, ListenOn, ListenPort, ConnectTo, ConnectPort, Comment, `Group`) VALUES ({newid}, {rule.Type}, {rule.ListenOn}, {rule.ListenPort}, {rule.ConnectTo}, {rule.ConnectPort}, {rule.Comment ?? ""}, {rule.Group ?? ""});");
                rule.Id = newid;
            }
            else throw new NotSupportedException($"Adding {obj.GetType().FullName} is not supported.");
        }
        public void AddRange<T>(IEnumerable<T> objs) where T : class
        {
            foreach (T obj in objs) Add(obj);
        }

        #endregion

        #region + -- RULE: MODIFY -- +

        /// <summary>
        /// Modify proxy rule already in the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void Update<T>(T obj) where T : class
        {
            if (obj is Rule rule)
            {
                Sql($"UPDATE Rules SET Type={rule.Type}, ListenOn={rule.ListenOn}, ListenPort={rule.ListenPort}, ConnectTo={rule.ConnectTo}, ConnectPort={rule.ConnectPort} WHERE Id={rule.Id};");
            }
            else throw new NotSupportedException($"Updating {obj.GetType().FullName} is not supported.");
        }

        public void UpdateRange<T>(IEnumerable<T> objs) where T : class
        {
            foreach (T obj in objs) Update(obj);
        }

        #endregion

        #region + -- RULE: REMOVE -- +

        /// <summary>
        /// Delete a proxy rule from the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void Remove<T>(T obj) where T : class
        {
            if (obj is Rule rule)
            {
                Sql($"DELETE FROM Rules WHERE Id={rule.Id};");
            }
            else throw new NotSupportedException($"Removing {obj.GetType().FullName} is not supported.");
        }

        public void RemoveRange<T>(IEnumerable<T> objs) where T : class
        {
            foreach (T obj in objs) Remove(obj);
        }

        #endregion

        #region + -- APP: CONFIG -- +

        /// <summary>
        /// Read config from db
        /// </summary>
        /// <returns></returns>
        public AppConfig GetAppConfig()
        {
            Config[] configRows = SqlQuery<Config>($"SELECT * FROM Configs;");
            AppConfig appConfig = new(configRows);
            return appConfig;
        }

        /// <summary>
        ///  Save current app config to the db
        /// </summary>
        /// <param name="appConfig"></param>
        public void SaveAppConfig(AppConfig appConfig)
        {
            // Store Window dimensions
            Sql($"UPDATE Configs SET Value = {appConfig.MainWindowSize.Width} WHERE Item = 'MainWindow' AND `Key` = 'Width';");
            Sql($"UPDATE Configs SET Value = {appConfig.MainWindowSize.Height} WHERE Item = 'MainWindow' AND `Key` = 'Height';");

            // Store column widths
            string s_portProxyColumnWidths = $"[{appConfig.PortProxyColumnWidths.Select(x => x.ToString()).Join(", ")}]";
            Sql($"UPDATE Configs SET Value = {s_portProxyColumnWidths} WHERE Item = 'PortProxy' AND `Key` = 'ColumnWidths';");
            
            // Store column sorting
            Sql($"UPDATE Configs SET Value = {appConfig.SortColumn} WHERE Item = 'PortProxy' AND `Key` = 'Column';");
            Sql($"UPDATE Configs SET Value = {appConfig.SortOrder} WHERE Item = 'PortProxy' AND `Key` = 'Order';");
        }

#endregion

    }
}
