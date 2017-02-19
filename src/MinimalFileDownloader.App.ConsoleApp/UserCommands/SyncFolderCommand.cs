﻿using MinimalFileDownloader.Common.FTP;
using MinimalFileDownloader.Common.SSH;
using System.Collections.Generic;
using System.Linq;

namespace MinimalFileDownloader.App.ConsoleApp
{
    internal class SyncFolderCommand : BaseUserCommand
    {
        public SyncFolderCommand(AppSettings appSettings, IDownloadManager downloadManager, IFtpService ftpService)
            : base(appSettings, downloadManager, ftpService)
        {
        }

        public override string Name
        {
            get
            {
                return "Sync File or Folder";
            }
        }

        public override string Shortcut
        {
            get
            {
                return "sync";
            }
        }

        public override void Run()
        {
            foreach (var folder in FtpService.ListItemsAsync("/", true, false, true).GetAwaiter().GetResult())
            {
                ConsoleUtils.WriteOption(folder.Path);
            }

            ConsoleUtils.DoCommandsWhileNotEscape("Path to download:", path =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    ConsoleUtils.WriteLine("Path cannot be empty!");
                    return;
                }

                DownloadFilesFromFolder(path);
            });
        }

        private void DownloadFilesFromFolder(string path)
        {
            path = path.AddInitialSlash();

            var files = FtpService.ListItemsAsync(path, true, true, false).GetAwaiter().GetResult();

            var downloadInfos = files
                .Select(file => new DownloadInfo($"ftp://{AppSettings.Ftp.UserName.FtpEscape()}:{AppSettings.Ftp.Password.FtpEscape()}@{AppSettings.Ftp.Url}/{file.Path.RemoveInitialSlash().FtpEscape()}", file.Path.RemoveInitialSlash()))
                .ToArray();

            DownloadManager.StartDownloadingFiles(downloadInfos);
        }

        public override void RunSilent(IReadOnlyDictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.ContainsKey("path"))
            {
                return;
            }

            string path = parameters["path"];

            DownloadFilesFromFolder(path);
        }
    }
}