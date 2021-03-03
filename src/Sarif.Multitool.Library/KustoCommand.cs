// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class KustoCommand : CommandBase
    {
        private ICslQueryProvider _kustoClient;
        private KustoOptions _options;

        public int Run(KustoOptions options)
        {
            try
            {
                if (!options.Validate())
                {
                    return FAILURE;
                }

                _options = options;

                InitializeKustoClient();

                (List<Result>, List<ReportingDescriptor>) sarifResults = RetrieveResultsFromKusto();
                var sarifLog = new SarifLog
                {
                    Runs = new[]
                    {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "spam",
                                Rules = sarifResults.Item2,
                            }
                        },
                        Results = sarifResults.Item1,
                    }
                }
                };

                WriteSarifFile(FileSystem, sarifLog, options.OutputFilePath, options.Minify);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }
            finally
            {
                _kustoClient?.Dispose();
            }
            return SUCCESS;
        }

        private void InitializeKustoClient()
        {
            KustoConnectionStringBuilder connection = new KustoConnectionStringBuilder(_options.HostAddress)
                .WithAadApplicationKeyAuthentication(
                    Environment.GetEnvironmentVariable("AppClientId"),
                    Environment.GetEnvironmentVariable("AppSecret"),
                    Environment.GetEnvironmentVariable("AuthorityId"));

            _kustoClient = KustoClientFactory.CreateCslQueryProvider(connection);
        }

        private (List<Result>, List<ReportingDescriptor>) RetrieveResultsFromKusto()
        {
            var dataReaderIndex = new Dictionary<string, int>();
            var results = new List<Result>();
            var rules = new Dictionary<string, ReportingDescriptor>();

            using IDataReader dataReader = _kustoClient.ExecuteQuery(
                _options.Database,
                _options.Query,
                new ClientRequestProperties { ClientRequestId = Guid.NewGuid().ToString() });
            while (dataReader.Read())
            {
                try
                {
                    string itemPathUri = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ItemPathUri"));
                    string organizationName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "OrganizationName"));
                    string projectName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ProjectName"));
                    string projectId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ProjectId"));
                    string repositoryName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RepositoryName"));
                    string repositoryId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RepositoryId"));
                    string regionSnippet = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RegionSnippet"));
                    string validationFingerprint = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ValidationFingerprint"));
                    string globalFingerprint = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "GlobalFingerprint"));
                    string ruleId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RuleId"));
                    string ruleName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RuleName"));
                    int regionStartLine = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionStartLine"));
                    int regionEndLine = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionEndLine"));
                    int regionStartColumn = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionStartColumn"));
                    int regionEndColumn = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionEndColumn"));
                    int regionCharOffset = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionCharOffset"));
                    int regionCharLength = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionCharLength"));
                    string resultKind = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ResultKind"));
                    string level = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "Level"));
                    string resultMessageText = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ResultMessageText"));
                    string result = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "Result"));

                    string contextRegionSnippet = null;

                    if (GetIndex(dataReader, dataReaderIndex, "ContextRegionSnippet") != -1)
                    {
                        contextRegionSnippet = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ContextRegionSnippet"));
                    }

                    Result resultObj = JsonConvert.DeserializeObject<Result>(result);
                    resultObj.Message = new Message
                    {
                        Text = resultMessageText
                    };
                    // Removing this, because the ruleIndex might not be in the correct place.
                    resultObj.RuleIndex = -1;
                    resultObj.Level = (FailureLevel)Enum.Parse(typeof(FailureLevel), level);
                    resultObj.Kind = (ResultKind)Enum.Parse(typeof(ResultKind), resultKind);
                    resultObj.Locations.Add(new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                CharLength = regionCharLength,
                                CharOffset = regionCharOffset,
                                StartColumn = regionStartColumn,
                                StartLine = regionStartLine,
                                EndColumn = regionEndColumn,
                                EndLine = regionEndLine,
                                Snippet = new ArtifactContent
                                {
                                    Text = regionSnippet
                                }
                            },
                            ContextRegion = string.IsNullOrEmpty(contextRegionSnippet)
                                ? null
                                : new Region
                                {
                                    Snippet = new ArtifactContent
                                    {
                                        Text = contextRegionSnippet
                                    }
                                },
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(itemPathUri)
                            }
                        },
                    });
                    resultObj.SetProperty("organizationName", organizationName);
                    resultObj.SetProperty("projectName", projectName);
                    resultObj.SetProperty("projectId", projectId);
                    resultObj.SetProperty("repositoryName", repositoryName);
                    resultObj.SetProperty("repositoryId", repositoryId);
                    resultObj.Fingerprints.Add("ValidationFingerprint", validationFingerprint);
                    resultObj.Fingerprints.Add("GlobalFingerprint", globalFingerprint);

                    if (!rules.ContainsKey(ruleId))
                    {
                        rules.Add(ruleId, new ReportingDescriptor
                        {
                            Id = ruleId,
                            Name = ruleName,
                        });
                    }

                    results.Add(resultObj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return (results, rules.Values.ToList());
        }

        private static int GetIndex(IDataReader dataReader, Dictionary<string, int> dataReaderIndex, string key)
        {
            int index;
            try
            {
                if (!dataReaderIndex.TryGetValue(key, out index))
                {
                    dataReaderIndex[key] = index = dataReader.GetOrdinal(key);
                }
            }
            catch (ArgumentException)
            {
                // When we don't find, let's set to -1.
                dataReaderIndex[key] = index = -1;
            }

            return index;
        }
    }
}
