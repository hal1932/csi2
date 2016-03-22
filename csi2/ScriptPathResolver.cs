using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System;

namespace Csi2
{
    class ScriptPathResolver : MetadataReferenceResolver, IEquatable<ScriptPathResolver>
    {
        public static ScriptPathResolver Default = new ScriptPathResolver();

        public ScriptPathResolver WithSearchPaths(params string[] paths)
        {
            var resolver = new ScriptPathResolver();
            resolver._searchPaths = _searchPaths.AddRange(paths);
            return resolver;
        }

        public bool Equals(ScriptPathResolver other)
        {
            return ReferenceEquals(this, other) ||
                other != null &&
                Equals(_defaultResolver, other._defaultResolver);
        }

        public override bool ResolveMissingAssemblies
            => _defaultResolver.ResolveMissingAssemblies;

        public override bool Equals(object other)
            => Equals(other as ScriptPathResolver);

        public override int GetHashCode()
            => _defaultResolver.GetHashCode();

        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
            => _defaultResolver.ResolveMissingAssembly(definition, referenceIdentity);

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            var resolvedReferences = _defaultResolver.ResolveReference(reference, baseFilePath, properties);
            if (resolvedReferences.Length > 0)
            {
                return resolvedReferences;
            }

            foreach (var searchPath in _searchPaths)
            {
                resolvedReferences = resolvedReferences.AddRange(
                    _defaultResolver.ResolveReference(
                        Path.Combine(searchPath, reference),
                        baseFilePath,
                        properties));
            }
            return resolvedReferences;
        }

        private MetadataReferenceResolver _defaultResolver = ScriptOptions.Default.MetadataResolver;
        private ImmutableArray<string> _searchPaths = ImmutableArray<string>.Empty;
    }
}
