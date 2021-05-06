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
                    string itemPath = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ItemPath"));
                    string itemPathUri = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ItemPathUri"));
                    string organizationName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "OrganizationName"));
                    string projectName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ProjectName"));
                    string projectId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ProjectId"));
                    string repositoryName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RepositoryName"));
                    string repositoryId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RepositoryId"));
                    string regionSnippet = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RegionSnippet"));
                    string validationFingerprint = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ValidationFingerprint"));
                    string validationFingerprintHash = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ValidationFingerprintHash"));
                    string globalFingerprint = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "GlobalFingerprint"));
                    string ruleId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RuleId"));
                    string ruleName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "RuleName"));
                    string resultKind = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ResultKind"));
                    string level = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "Level"));
                    string resultMessageText = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ResultMessageText"));
                    string result = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "Result"));
                    string etlEntity = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "EtlEntity"));

                    string contextRegionSnippet = null;
                    if (GetIndex(dataReader, dataReaderIndex, "ContextRegionSnippet") != -1)
                    {
                        contextRegionSnippet = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "ContextRegionSnippet"));
                    }

                    string assetFingerprint = null;
                    if (GetIndex(dataReader, dataReaderIndex, "AssetFingerprint") != -1)
                    {
                        assetFingerprint = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "AssetFingerprint"));
                    }

                    bool? onDemandValidated = null;
                    if (GetIndex(dataReader, dataReaderIndex, "OnDemandValidated") != -1)
                    {
                        onDemandValidated = (sbyte)dataReader.GetValue(GetIndex(dataReader, dataReaderIndex, "OnDemandValidated")) == 1;
                    }

                    string subscriptionId = null;
                    if (GetIndex(dataReader, dataReaderIndex, "SubscriptionId") != -1)
                    {
                        subscriptionId = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "SubscriptionId"));
                    }

                    string subscriptionName = null;
                    if (GetIndex(dataReader, dataReaderIndex, "SubscriptionName") != -1)
                    {
                        subscriptionName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "SubscriptionName"));
                    }

                    if (!string.IsNullOrEmpty(subscriptionId) && !string.IsNullOrEmpty(subscriptionName))
                    {
                        resultMessageText += $" The resource is in the '[{subscriptionName}](https://portal.azure.com/#resource/subscriptions/{subscriptionId}/overview)' subscription.";
                    }

                    string serviceName = null;
                    if (GetIndex(dataReader, dataReaderIndex, "STServiceName") != -1)
                    {
                        serviceName = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "STServiceName"));
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            var owners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            resultMessageText += $" The subscription backing this Azure resource is associated with the '{serviceName}'";

                            string serviceOwner = null;
                            if (GetIndex(dataReader, dataReaderIndex, "STOwner") != -1)
                            {
                                serviceOwner = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "STOwner"));
                                if (!string.IsNullOrEmpty(serviceOwner))
                                {
                                    owners.Add(serviceOwner);
                                }
                            }

                            string STAllOwners = null;
                            if (GetIndex(dataReader, dataReaderIndex, "STAllOwners") != -1)
                            {
                                STAllOwners = dataReader.GetString(GetIndex(dataReader, dataReaderIndex, "STAllOwners"));
                                if (!string.IsNullOrEmpty(STAllOwners))
                                {
                                    List<string> ownersList = JsonConvert.DeserializeObject<List<string>>(STAllOwners);
                                    ownersList.ForEach((o) => owners.Add(o));
                                }
                            }

                            if (owners.Count > 0)
                            {
                                string emails = string.Join(";", owners.Select(o => $"{o}@microsoft.com"));
                                resultMessageText += $" which is owned by '[{string.Join(";", owners)}](mailto:{emails})'.";
                            }
                        }
                    }

                    if (etlEntity == "Build" || etlEntity == "BuildDefinition" ||
                        etlEntity == "Release" || etlEntity == "ReleaseDefinition" ||
                        etlEntity == "WorkItem")
                    {
                        itemPath = itemPath.Replace("vsrm.dev.azure.com", "dev.azure.com");
                        itemPath = itemPath.Replace("_apis/wit/workItems/", "_workitems/edit/");
                        itemPath = itemPath.Replace("vsrm.visualstudio.com", "visualstudio.com");
                        itemPath = itemPath.Replace("_apis/build/Definitions/", "_build?definitionId=");
                        itemPath = itemPath.Replace("_apis/Release/definitions/", "_release?_a=releases&view=mine&definitionId=");

                        resultMessageText += $" The raw data that was scanned for this finding can be viewed [here]({itemPathUri}).";

                        itemPathUri = itemPath;
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

                    if (onDemandValidated.HasValue)
                    {
                        // If on demand is true, it means that we searched validated that credential
                        // and that returned that the credential is not valid anymore.
                        resultObj.BaselineState = onDemandValidated.Value
                            ? BaselineState.Absent
                            : BaselineState.New;
                    }

                    if (resultObj?.Locations.Count > 0)
                    {
                        resultObj.Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri(itemPathUri);
                        resultObj.Locations[0].PhysicalLocation.Region.Snippet = new ArtifactContent
                        {
                            Text = regionSnippet
                        };

                        if (!string.IsNullOrEmpty(contextRegionSnippet))
                        {
                            resultObj.Locations[0].PhysicalLocation.ContextRegion.Snippet = new ArtifactContent
                            {
                                Text = contextRegionSnippet
                            };
                        }
                    }
                    else
                    {
                        int regionStartLine = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionStartLine"));
                        int regionEndLine = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionEndLine"));
                        int regionStartColumn = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionStartColumn"));
                        int regionEndColumn = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionEndColumn"));
                        int regionCharOffset = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionCharOffset"));
                        int regionCharLength = dataReader.GetInt32(GetIndex(dataReader, dataReaderIndex, "RegionCharLength"));
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
                    }

                    resultObj.SetProperty("organizationName", organizationName);
                    resultObj.SetProperty("projectName", projectName);
                    resultObj.SetProperty("projectId", projectId);
                    resultObj.SetProperty("repositoryName", repositoryName);
                    resultObj.SetProperty("repositoryId", repositoryId);
                    resultObj.Fingerprints.Add("AssetFingerprint/v1", assetFingerprint);
                    resultObj.Fingerprints.Add("GlobalFingerprint/v1", globalFingerprint);
                    resultObj.Fingerprints.Add("ValidationFingerprint/v1", validationFingerprint);
                    resultObj.Fingerprints.Add("ValidationFingerprintHash/v1", validationFingerprintHash);

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
