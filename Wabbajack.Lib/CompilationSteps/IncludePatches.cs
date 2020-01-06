﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using Newtonsoft.Json;
using Wabbajack.Common;
using Wabbajack.VirtualFileSystem;

namespace Wabbajack.Lib.CompilationSteps
{
    public class IncludePatches : ACompilationStep
    {
        private readonly Dictionary<string, IGrouping<string, VirtualFile>> _indexed;

        public IncludePatches(ACompiler compiler) : base(compiler)
        {
            _indexed = _compiler.IndexedFiles.Values
                .SelectMany(f => f)
                .GroupBy(f => Path.GetFileName(f.Name).ToLower())
                .ToDictionary(f => f.Key);
        }

        public override async ValueTask<Directive> Run(RawSourceFile source)
        {
            var name = Path.GetFileName(source.File.Name.ToLower());
            string nameWithoutExt = name;
            if (Path.GetExtension(name) == ".mohidden")
                nameWithoutExt = Path.GetFileNameWithoutExtension(name);
            
            if (!_indexed.TryGetValue(Path.GetFileName(name), out var choices))
                if (!_indexed.TryGetValue(Path.GetFileName(nameWithoutExt), out choices)) 
                    return null;

            var mod_ini = ((MO2Compiler)_compiler).ModMetas.FirstOrDefault(f => source.Path.StartsWith(f.Key));
            var installationFile = mod_ini.Value?.General?.installationFile;

            var found = choices.FirstOrDefault(
                f => Path.GetFileName(f.FilesInFullPath.First().Name) == installationFile);

            if (found == null)
            {
                found = choices.OrderBy(f => f.NestingFactor)
                               .ThenByDescending(f => (f.FilesInFullPath.First() ?? f).LastModified)
                               .First();
            }

            var e = source.EvolveTo<PatchedFromArchive>();
            e.FromHash = found.Hash;
            e.ArchiveHashPath = found.MakeRelativePaths();
            e.To = source.Path;
            e.Hash = source.File.Hash;

            Utils.TryGetPatch(found.Hash, source.File.Hash, out var data);

            if (data != null)
                e.PatchID = _compiler.IncludeFile(data);

            return e;
        }

        public override IState GetState()
        {
            return new State();
        }

        [JsonObject("IncludePatches")]
        public class State : IState
        {
            public ICompilationStep CreateStep(ACompiler compiler)
            {
                return new IncludePatches(compiler);
            }
        }
    }
}
