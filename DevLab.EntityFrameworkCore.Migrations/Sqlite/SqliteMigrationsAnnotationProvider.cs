﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using System.Collections.Generic;
using System.Linq;

namespace DevLab.EntityFrameworkCore.Migrations.Sqlite
{
    /// <summary>
    /// This API supports the Entity Framework Core infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be
        /// used directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be
        /// used directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IModel model)
        {
            if (model.GetEntityTypes().SelectMany(t => t.GetProperties()).Any(
                p => SqliteTypeMappingSource.IsSpatialiteType(p.Relational().ColumnType)))
            {
                yield return new Annotation(SqliteAnnotationNames.InitSpatialMetaData, true);
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be
        /// used directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd
                && property.ClrType.UnwrapNullableType().IsInteger()
                && !HasConverter(property))
            {
                yield return new Annotation(SqliteAnnotationNames.Autoincrement, true);
            }

            var srid = property.Sqlite().Srid;
            if (srid != null)
            {
                yield return new Annotation(SqliteAnnotationNames.Srid, srid);
            }

            var dimension = property.Sqlite().Dimension;
            if (dimension != null)
            {
                yield return new Annotation(SqliteAnnotationNames.Dimension, dimension);
            }
        }

        private static bool HasConverter(IProperty property)
            => (property.FindMapping()?.Converter
                ?? property.GetValueConverter()) != null;
    }
}