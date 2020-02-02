using Microsoft.EntityFrameworkCore.Migrations;

namespace OrleansSimpleQueueCacheTest.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RunSqlScripts(
                "0001 SQLServer-Main.sql",
                "0002 SQLServer-Clustering.sql",
                "0003 SQLServer-Persistence.sql",
                "0004 SQLServer-Reminders.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
