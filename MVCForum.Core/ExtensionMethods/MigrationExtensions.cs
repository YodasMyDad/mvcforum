namespace MvcForum.Core.ExtensionMethods
{
    using System;
    using System.Data.Entity.Migrations.Infrastructure;

    public static class MigrationExtensions
    {
        //public static IFluentSyntax CreateColIfNotExists(this MigratorBase self, string tableName, string colName,
        //    Func<IAlterTableColumnAsTypeSyntax, IFluentSyntax> constructColFunction, string schemaName = "dbo")
        //{
        //    if (!self.Schema.Schema(schemaName).Table(tableName).Column(col‌​Name).Exists())
        //    {
        //        return constructColFunction(self.Alter.Table(tableName).AddColumn(c‌​olName));
        //    }
        //    return null;
        //}

        //public static IFluentSyntax CreateTableIfNotExists(this MigrationBase self, string tableName, Func<ICreateTableWithColumnOrSchemaOrDescriptionSyntax, IFluentSyntax> constructTableFunction, string schemaName = "dbo")
        //{
        //    if (!self.Schema.Schema(schemaName).Table(tableName).Exists())
        //    {
        //        return constructTableFunction(self.Create.Table(tableName));
        //    }
        //    else
        //        return null;
        //}
    }
}