using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OrleansSimpleQueueCacheTest.Migrations
{
    internal static class MigrationBuilderExtensions
    {
        public static MigrationBuilder RunSqlScripts(this MigrationBuilder migrationBuilder, params string[] scripts)
        {
            foreach (var script in scripts)
            {
                var sql = GetResource(script);
                migrationBuilder.Sql(sql, true);
            }

            return migrationBuilder;
        }

        private static string GetResource(string resourceName)
        {
            using (var stream = typeof(MigrationBuilderExtensions).Assembly.GetManifestResourceStream($"{typeof(MigrationBuilderExtensions).Namespace}.Sql.{resourceName}"))
            using (var reader = new StreamReader(stream))
            {
                var resource = reader.ReadToEnd();
                return resource;
            }
        }
    }
}
