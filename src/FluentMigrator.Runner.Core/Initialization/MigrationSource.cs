#region License
// Copyright (c) 2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    public class MigrationSource : IMigrationSource
    {
        [NotNull]
        private readonly IAssemblySource _source;

        [NotNull]
        private readonly IMigrationRunnerConventions _conventions;

        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly ConcurrentDictionary<Type, IMigration> _instanceCache = new ConcurrentDictionary<Type, IMigration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileSource"/> class.
        /// </summary>
        /// <param name="source">The assembly source</param>
        /// <param name="conventions">The migration runner conventios</param>
        /// <param name="serviceProvider">The service provider</param>
        public MigrationSource(
            [NotNull] IAssemblySource source,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IServiceProvider serviceProvider)
        {
            _source = source;
            _conventions = conventions;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileSource"/> class.
        /// </summary>
        /// <param name="source">The assembly source</param>
        /// <param name="conventions">The migration runner conventios</param>
        [Obsolete]
        public MigrationSource(
            [NotNull] IAssemblySource source,
            [NotNull] IMigrationRunnerConventions conventions)
        {
            _source = source;
            _conventions = conventions;
        }

        /// <inheritdoc />
        public IEnumerable<IMigration> GetMigrations()
        {
            var instances =
                from type in _source.Assemblies.SelectMany(a => a.GetExportedTypes())
                where !type.IsAbstract && typeof(IMigration).IsAssignableFrom(type)
                where _conventions.TypeIsMigration(type)
                select _instanceCache.GetOrAdd(type, CreateInstance);
            return instances;
        }

        private IMigration CreateInstance(Type type)
        {
            if (_serviceProvider == null)
            {
                return (IMigration)Activator.CreateInstance(type);
            }

            return (IMigration)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
