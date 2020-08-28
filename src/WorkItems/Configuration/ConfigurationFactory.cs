using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.CodeAnalysis.WorkItems.Configuration
{
    public static class ConfigurationFactory
    {
        public static SurffConfigurationDocument LoadFromSourceControl()
        {
            return LoadFromText(Example);
        }

        public static SurffConfigurationDocument LoadFromFilePath(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return LoadFromText(text);
        }

        public static SurffConfigurationDocument LoadFromText(string text)
        {
            IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            IParser parser = new MergingParser(new Parser(new StringReader(text)));
            SurffConfigurationDocument configuration = deserializer.Deserialize<SurffConfigurationDocument>(parser);

            return configuration;
        }

        public static SurffConfiguration Load(string organization, string project, string tool)
        {
            IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            IParser parser = new MergingParser(new Parser(new StringReader(Example)));
            SurffConfigurationDocument configuration = deserializer.Deserialize<SurffConfigurationDocument>(parser);

            if (!string.IsNullOrEmpty(tool))
            {
                SurffConfiguration toolConfiguration = null;
                if (configuration.Tools?.TryGetValue(tool, out toolConfiguration) == true)
                {
                    return toolConfiguration;
                }
            }

            return configuration.Default;
        }

        private const string Example = @"---
Default: &default
  IsEnabled: true
  # The list of extensions/transformers to load and apply
  Extensions:
    - # The name of the extension. this is unused and is just a label for readability
      Name: 'Default Transformers'
      # The path to the assembly that defines the transformer.
      # Dependencies will be loaded from the same directory as the assembly
      # Should this path be rooted? 
      #    .\path1\assembly.dll is ambiguous. 
      #    Similarly, E:\path1\assembly.dll is bad. 
      #    But \path1\assembly.dll implies the path is relative to the root of the configuration repo
      AssemblyPath: '.\1ES.Surff.dll'
      # The fully qualified type name of the class that defines the transformer.
      FullyQualifiedTypeName: 'Microsoft.CodeAnalysis.Sarif.WorkItems.Plugins.Transformers, 1ES.Surff, Version=2.2.3.0, Culture=neutral, PublicKeyToken=21a5e83f6f5bb844'
    - Name: '1ES Transformers'
      AssemblyPath: '.\1ES.Surff.dll'
      FullyQualifiedTypeName: 'Microsoft.OneES.OneESTransformer, 1ES.Surff, Version=2.2.3.0, Culture=neutral, PublicKeyToken=21a5e83f6f5bb844'
      # Settings for the transformer are defined as part of the transformer configuration
      # Issue, is there a way to serialize these, since the settings will be different for each transformer?
      # OwnershipTransformer settings
      Properties:
        AADClientId: 'asdfasfsasdfasdfd'
        AADKey: 'asdfasdfasfasdfasdf'
        KustoCluster: 'https://1es.kusto.windows.net'
        KustoDatabase: 'TSESecurity'
        Account: 'asdfasdfasfasdfasdf'
        Project: 'asdfasdfasfasdfasdf'
        Repository: 'asdfasdfasfasdfasdf'
        # SnippetFilterTransformer settings
        SnippetContains: 'SnippetContainsPLACEHOLDER'
        # WorkItemOperationTransformer settings
        WorkItemClosedStates: 'Closed'
        DoNotFileTags: 'falsepositive'
  # The settings for the work items that are created    
  WorkItem: &defaultWorkItem
    # The token to use to access the work item store. This should be a key vault setting instead?
    PersonalAccessToken: 'salsdjfafdsiw409sldfasljfadsfljiwoi'
    # Sync the state of the work item before filing.
    # This is used to determine the current status of the work item to determine if
    # the existing work item should be updated or created new.
    SyncWorkItemMetadata: true
    # File work items for results who's baseline state is Unchanged.
    # Usually this is false to avoid filing duplicate work items.
    ShouldFileUnchanged: false
    # The splitting/grouping strategy to use. This determines which resuts are grouped together and filed as one work item.
    SplittingStrategy: 'PerRunPerOrgPerEntityType'
    # The footer for work items filed to AzureDevOps.
    AzureDevOpsDescriptionFooter: 'footer text'
    # The footer for issues filed to GitHub.
    GitHubDescriptionFooter: 'footer text'
    # Custom tags to add to the work items
    Tags:
      - 'tag1'
      - 'tag2'
Tools:
  binskim:
    <<: *default
    IsEnabled: false
  credentialscanner:
    <<: *default
    WorkItem:
      <<: *defaultWorkItem
      # Overrides the organization and project for the work item
      # This is typically used for testing, so we don't create work items in a production account
      Organization: 'secretscantest'
      Project: 'thesecretproject'";
    }
}
