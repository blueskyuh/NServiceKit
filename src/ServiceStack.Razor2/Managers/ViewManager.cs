﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;
using ServiceStack.Common.Extensions;
using ServiceStack.IO;
using ServiceStack.Razor2.Compilation;
using ServiceStack.Razor2.Managers.RazorGen;
using ServiceStack.WebHost.Endpoints;

namespace ServiceStack.Razor2.Managers
{
    /// <summary>
    /// This view manager is responsible for keeping track of all the 
    /// available Razor views and states of Razor pages.
    /// </summary>
    public class ViewManager
    {
        /// <summary>
        /// The purpose of the FileSystemWatcher is to ensure razor pages are
        /// consistent with the code generated by the razor engine. The file
        /// system watcher will invalidate pages and queue them for recompilation.
        /// </summary>
        protected FileSystemWatcher FileSystemWatcher;

        protected Dictionary<string, RazorPage> Pages = new Dictionary<string, RazorPage>(StringComparer.InvariantCultureIgnoreCase);

        protected ViewConfig Config { get; set; }

        protected string RootPath = null;
        protected IAppHost AppHost { get; set; }

        protected IVirtualPathProvider PathProvider = null;

        public ViewManager(IAppHost appHost, ViewConfig viewConfig)
        {
            this.AppHost = appHost;
            this.Config = viewConfig;
            this.RootPath = appHost.Config.WebHostPhysicalPath;
            this.PathProvider = appHost.VirtualPathProvider;

            ScanForRazorPages();

            //setup the file system watcher for page invalidation
            this.FileSystemWatcher = new FileSystemWatcher(this.AppHost.Config.WebHostPhysicalPath, "*.*")
                {
                    IncludeSubdirectories = true,
                    //NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite
                };

            this.FileSystemWatcher.Changed += FileSystemWatcher_Changed;
            this.FileSystemWatcher.Created += FileSystemWatcher_Created;
            this.FileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            this.FileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            this.FileSystemWatcher.EnableRaisingEvents = true;
        }

        private void ScanForRazorPages()
        {
            var pattern = Path.ChangeExtension("*", this.Config.RazorFileExtension);

            var files = this.PathProvider.GetAllMatchingFiles(pattern)
                            .Where(IsWatchedFile);
            // you can override IsWatchedFile to filter

            files.ForEach(TrackRazorPage);
        }

        protected virtual void TrackRazorPage(IVirtualFile file)
        {
            //get the base type.
            var pageBaseType = this.Config.PageBaseType;

            var transformer = new RazorViewPageTransformer(pageBaseType);

            //create a RazorPage
            var page = new RazorPage
                {
                    PageHost = new RazorPageHost(PathProvider, file, transformer, new CSharpCodeProvider(), new Dictionary<string, string>()),
                    IsValid = false,
                    File = file
                };

            //add it to our pages dictionary.
            AddRazorPage(page);
        }

        protected virtual void AddRazorPage(RazorPage page)
        {
            var pagePath = GetDictionaryPagePath(page.PageHost.File);

            this.Pages.Add(pagePath, page);
        }

        public virtual RazorPage GetRazorView(string path)
        {
            RazorPage page;
            this.Pages.TryGetValue(path, out page);
            return page;
        }

        protected virtual bool IsWatchedFile(IVirtualFile file)
        {
            return this.Config.RazorFileExtension.EndsWith(file.Extension, StringComparison.InvariantCultureIgnoreCase);
        }


        protected virtual string GetDictionaryPagePath(string relativePath)
        {
            if (relativePath.ToLowerInvariant().StartsWith("/views/"))
            {
                //re-write the /views path
                //so we can uniquely get views by
                //ResponseDTO/RequestDTO type.
                //PageResolver:NormalizePath()
                //knows how to resolve DTO views.
                return "/views/" + Path.GetFileName(relativePath);
            }
            return relativePath;
        }
        protected virtual string GetDictionaryPagePath(IVirtualFile file)
        {
            return GetDictionaryPagePath(file.VirtualPath);
        }

        #region FileSystemWatcher Handlers

        protected virtual string GetRelativePath(string ospath)
        {
            var relative = ospath
                .Replace(this.AppHost.Config.WebHostPhysicalPath, "")
                .Replace(this.PathProvider.RealPathSeparator, "/");
            return relative;
        }
        protected virtual IVirtualFile GetVirutalFile(string ospath)
        {
            var relative = GetRelativePath(ospath);
            return this.PathProvider.GetFile(relative);
        }


        protected virtual void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            var oldPagePath = this.GetDictionaryPagePath(this.GetRelativePath(e.OldFullPath));

            if (!this.Pages.Remove(oldPagePath))
            {
                Debugger.Break();
            }

            var newFile = this.GetVirutalFile(e.FullPath);
            if (!IsWatchedFile(newFile)) return;

            this.TrackRazorPage(newFile);
        }

        protected virtual void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            var file = this.GetVirutalFile(e.FullPath);
            if (!IsWatchedFile(file)) return;

            var pathPage = GetDictionaryPagePath(file);

            this.Pages.Remove(pathPage);
        }

        protected virtual void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            var file = this.GetVirutalFile(e.FullPath);
            if (!IsWatchedFile(file)) return;

            this.TrackRazorPage(file);
        }

        protected virtual void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            var file = this.GetVirutalFile(e.FullPath);
            if (!IsWatchedFile(file)) return;

            var pagePath = GetDictionaryPagePath(file);

            RazorPage page;
            if (this.Pages.TryGetValue(pagePath, out page) && page.IsValid)
            {
                page.IsValid = false;
            }
        }

        #endregion
    }


    public class ViewConfig
    {
        public ViewConfig()
        {
            this.RazorFileExtension = ".cshtml";

            this.PageBaseType = typeof(ViewPage);
        }

        protected virtual void ReadBasePageTypeFromConfig()
        {

        }

        public virtual string RazorFileExtension { get; set; }

        public virtual Type PageBaseType { get; set; }
    }

}