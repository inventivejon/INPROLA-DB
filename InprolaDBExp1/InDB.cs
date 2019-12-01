using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace InprolaDBExp1
{
    public enum InDBSourceType
    {
        AKTIONEN,
        VORLAGEN,
        BEZUEGE,
        SPRACHE
    }

    public class InDB
    {
        private readonly ConcurrentDictionary<InDBSourceType, ConcurrentBag<string>> _allDBSources;
        private readonly string _dBFolder;
        private readonly List<string> _supportedLanguages;

        public InDB(string dB_Folder, string[] supportedLanguages)
        {
            this._dBFolder = dB_Folder.TrimEnd('\\') + "\\";
            this._supportedLanguages = new List<string>(supportedLanguages);
            this._allDBSources = new ConcurrentDictionary<InDBSourceType, ConcurrentBag<string>>();

            foreach (InDBSourceType singleInDBSourceType in (InDBSourceType[])Enum.GetValues(typeof(InDBSourceType)))
            {
                this._allDBSources.TryAdd(singleInDBSourceType, new ConcurrentBag<string>());
            }
        }

        public bool StartEngine()
        {
            return this.RegisterFullDBNode(this._dBFolder);
        }

        public bool RegisterFullDBNode(string rootSourcePath)
        {
            bool returnValue = true;

            returnValue = returnValue && this.RegisterDBSource(InDBSourceType.AKTIONEN, rootSourcePath + "\\Aktionen");
            returnValue = returnValue && this.RegisterDBSource(InDBSourceType.VORLAGEN, rootSourcePath + "\\Vorlagen");
            returnValue = returnValue && this.RegisterDBSource(InDBSourceType.BEZUEGE, rootSourcePath + "\\Bezüge");

            foreach(var singleLang in this._supportedLanguages)
                returnValue = returnValue && this.RegisterDBSource(InDBSourceType.SPRACHE, rootSourcePath + "\\Sprache", singleLang);

            return returnValue;
        }

        public bool RegisterDBSource(InDBSourceType sourceType, string sourcePath, params string[] additionalParams)
        {
            string absoluteSourcePath;

            /* Check if the folder Path is relative or absolute */
            if(Path.IsPathFullyQualified(sourcePath))
            {
                /* absolute path */
                absoluteSourcePath = sourcePath;
            }
            else
            {
                /* relative path */
                absoluteSourcePath = this._dBFolder + sourcePath.TrimStart('\\');
            }

            return this._RegisterDBSource(sourceType, absoluteSourcePath, additionalParams);
        }

        private bool _RegisterDBSource(InDBSourceType sourceType, string absoluteSourcePath, params string[] additionalParams)
        {
            bool returnValue = true;

            switch (sourceType)
            {
                default:
                    this._allDBSources[sourceType].Add(absoluteSourcePath);
                    try
                    {
                        Directory.CreateDirectory(absoluteSourcePath);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                        returnValue = false;
                    }
                    break;

                case InDBSourceType.SPRACHE:
                    string subPath = (additionalParams?.Length > 0) ? ("\\"+additionalParams[0].TrimStart('\\')) : "";
                    this._allDBSources[sourceType].Add(absoluteSourcePath + subPath);

                    try
                    {
                        Directory.CreateDirectory(absoluteSourcePath);
                        if (!string.IsNullOrEmpty(subPath))
                        {
                            Directory.CreateDirectory(absoluteSourcePath + subPath);
                            Directory.CreateDirectory(absoluteSourcePath + subPath + "\\Keywords");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        returnValue = false;
                    }
                    
                    break;
            }

            return returnValue;
        }
    }
}