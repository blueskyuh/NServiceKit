﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NServiceKit.IO;
using NServiceKit.WebHost.Endpoints;

namespace NServiceKit.VirtualPath
{
    /// <summary>
    /// <see cref="IVirtualPathProvider" /> implementation which looks at embedded resources.
    /// Resources will have the assembly name stripped off and subsequent namespace levels will be treated as directories.
    /// For example, if you embed A.B.C.D.txt in an assembly named A, it will be provided as: /B/C/D.txt
    /// </summary>
    public class EmbeddedResourceVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        private readonly List<Assembly> _assemblies;
        private InMemoryVirtualDirectory _root;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceVirtualPathProvider"/> class.
        /// </summary>
        /// <param name="appHost"></param>
        public EmbeddedResourceVirtualPathProvider(IAppHost appHost) : base(appHost)
        {
            _root = new InMemoryVirtualDirectory(this){FlattenFileEnumeration = false};
            _assemblies = new List<Assembly>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblies"></param>
        public void IncludeAssemblies(params Assembly[] assemblies)
        {
            _assemblies.AddRange(assemblies);
            _initialized = false;
        }

        /// <summary>
        /// If set, will run on all discovered resources. Returning true will cause the resource to be excluded.
        /// </summary>
        public Func<IVirtualFile, bool> FileExcluder { get; set; }

        public override IVirtualDirectory RootDirectory
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                    _initialized = true;
                }
                return _root;
            }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        /// <summary>
        /// </summary>
        protected override void Initialize()
        {
            PopulateFromEmbeddedResources();
        }

        /// <summary>
        /// Populates the root directory from embedded resources.
        /// </summary>
        public void PopulateFromEmbeddedResources()
        {
            _root = new InMemoryVirtualDirectory(this) { FlattenFileEnumeration = false };
            foreach (var assembly in _assemblies)
            {
                string baseName = assembly.GetName().Name + ".";

                foreach (var resource in assembly.GetManifestResourceNames())
                {
                    // Most embedded resources will start with the assembly name (e.g. Foo.txt in Bar.dll will be named Bar.Foo.txt by default)
                    // Strip that assembly name off if it's set
                    string relativeName = resource;
                    if (relativeName.StartsWith(baseName))
                    {
                        relativeName = relativeName.Remove(0, baseName.Length);
                    }

                    string fileName;
                    string[] directoryStructure;
                    // Figure out which portion of the path represents the file name, and what directory structure it's in (if any)
                    InferFileNameAndDirectoryPath(relativeName, out fileName, out directoryStructure);

                    // Loop over the directory structure to figure out which directory this file is supposed to go in
                    InMemoryVirtualDirectory destinationDirectory = _root;
                    foreach (var directory in directoryStructure)
                    {
                        var nextLevel = (InMemoryVirtualDirectory) destinationDirectory.GetDirectory(directory);
                        // If our expected directory doesn't exist, add it
                        if (nextLevel == null)
                        {
                            nextLevel = new InMemoryVirtualDirectory(this, destinationDirectory) {FlattenFileEnumeration = false, DirName = directory};
                            destinationDirectory.dirs.Add(nextLevel);
                        }
                        destinationDirectory = nextLevel;
                    }
                    var file = new StreamBasedVirtualFile(this, destinationDirectory, assembly.GetManifestResourceStream(resource), fileName, DateTime.Now);

                    // Give people the opportunity to exclude this file
                    if (FileExcluder != null && FileExcluder(file))
                    {
                        continue;
                    }

                    destinationDirectory.files.Add(file);
                }
            }
        }

        /// <summary>
        /// Takes in a relative path and figures out which portions represent the file name and any directories in between.
        /// e.g. A.B.C.txt would produce "C.txt" and ["A", "B"]
        /// </summary>
        /// <param name="relativeName"></param>
        /// <param name="fileName"></param>
        /// <param name="directoryPath"></param>
        internal static void InferFileNameAndDirectoryPath(string relativeName, out string fileName, out string[] directoryPath)
        {
            string[] pieces = relativeName.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
            // If there are no dots, treat this as a file in the root
            if (pieces.Length == 1)
            {
                fileName = relativeName;
                directoryPath = new string[0];
                return;
            }

            int length = pieces.Length;
            fileName = pieces[length - 2] + "." + pieces[length - 1];

            // Only 2 parts means a file name, but no directories
            if (pieces.Length == 2)
            {
                directoryPath = new string[0];
                return;
            }

            // Copy any other pieces verbatim, except for the last two parts used to compose the file
            directoryPath = new string[length - 2];
            Array.Copy(pieces, directoryPath, directoryPath.Length);
        }

        /// <summary>
        /// Virtual file that wraps a stream. Doesn't get unwrapped until it's resolved.
        /// </summary>
        private class StreamBasedVirtualFile : InMemoryVirtualFile
        {
            private readonly Stream _contents;
            private readonly string _name;
            private readonly DateTime _lastModified;

            public StreamBasedVirtualFile(IVirtualPathProvider owningProvider, IVirtualDirectory directory, Stream contents, string name, DateTime lastModified) : base(owningProvider, directory)
            {
                _contents = contents;
                _name = name;
                _lastModified = lastModified;
                FilePath = name;
            }

            public override string Name
            {
                get { return _name; }
            }

            public override DateTime LastModified
            {
                get { return _lastModified; }
            }

            public override Stream OpenRead()
            {
                return _contents;
            }
        }
    }
}
