// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using Hemocode.Common.Implementations;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.DebugPanel.JsonModels;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class JsonKnownIssuesDatabase : IKnownIssuesDatabase
    {
        private readonly IFileSystemScanner _scanner;
        private readonly IJsonParser _jsonParser;

        public JsonKnownIssuesDatabase(IFileSystemScanner scanner, IJsonParser jsonParser)
        {
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
            ThrowHelper.ThrowIfNull(jsonParser);
            _jsonParser = jsonParser;
        }

        public string DatabaseVersion
        {
            get
            {
                var data = LoadData();

                return data != null ? data.Version ?? string.Empty : string.Empty;
            }
        }

        public IList<KnownIssueItem> GetKnownIssues()
        {
            var data = LoadData();
            if (data == null || data.Issues == null)
            {
                return new List<KnownIssueItem>();
            }

            return new List<KnownIssueItem>(data.Issues);
        }

        private KnownIssuesData LoadData()
        {
            try
            {
                var path = _scanner.GetExternalDataPath() + "/known-issues.json";
                if (!_scanner.FileExists(path))
                {
                    return null;
                }

                var json = _scanner.ReadAllText(path);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return _jsonParser.FromJson<KnownIssuesData>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
