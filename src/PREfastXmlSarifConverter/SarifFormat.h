#pragma once 
#include <comutil.h>

#include "DefectXmlFormat.h"
#include "JsonSerialize.h"

bool hasEnding(const _bstr_t &strValue, const std::wstring &ending);
std::wstring AddEscapeCharacters(const std::wstring &data);
std::wstring GetDefectUri(const XmlSfa &sfa);
std::wstring MakeItUri(const std::wstring &fullPath);
void GetXmlToSarifMapping(const std::wstring &prefastTag, std::wstring &sarifTag);

class SarifRegion
{
    static constexpr int BASE10 = 10;
    json::Object m_values;

public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetStartLine(const std::wstring &value);
    void SetStartColumn(const std::wstring &value);
    void SetEndLine(const std::wstring &value);
    void SetEndColumn(const std::wstring &value);
    void SetOffset(const std::wstring &value);
    void SetLength(const std::wstring &value);

    bool IsValid();
    void SetStartLine(int value);
    void SetStartColumn(int value);

    void SetEndLine(int value)
    {
        m_values[L"endLine"] = json::Value(value);
    }

    void SetEndColumn(int value)
    {
        m_values[L"endColumn"] = json::Value(value);
    }

    void SetOffset(int value)
    {
        m_values[L"offset"] = json::Value(value);
    }

    void SetLength(int value)
    {
        m_values[L"length"] = json::Value(value);
    }

    int GetStartLine() const;
    int GetStartColumn() const;
};

class SarifPhysicalLocation
{
    json::Object m_values;
public:
    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetURI(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void SetURIBaseId(const std::wstring &uriBaseId)
    {
        m_values[L"uriBaseId"] = uriBaseId;
    }

    void SetRegion(const SarifRegion &region)
    {
        m_values[L"region"] = region.GetJsonObject();
    }
};

class SarifAnnotatedCodeLocation
{
    static constexpr int BASE10 = 10;
    json::Object m_values;

public:
    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetId(const std::wstring &id)
    {
        m_values[L"id"] = id;
    }

    void SetPhysicalLocation(const SarifPhysicalLocation &physicalLocation)
    {
        m_values[L"physicalLocation"] = physicalLocation.GetJsonObject();
    }

    void SetModule(const std::wstring &module)
    {
        m_values[L"module"] = module;
    }

    void SetMessage(const std::wstring &msg)
    {
        m_values[L"message"] = AddEscapeCharacters(msg);
    }

    void SetKind(const std::wstring &kind)
    {
        m_values[L"kind"] = kind;
    }

    void SetEssential(bool essential)
    {
        m_values[L"essential"] = json::Value(essential);
    }

    void SetThreadId(int threadId)
    {
        m_values[L"threadId"] = json::Value(threadId);
    }

    void SetThreadId(const std::wstring &time);
    void SetEssential(const std::wstring &essential);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifLogicalLocationComponent
{
    json::Object m_values;
public:
    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetLocationKind(const std::wstring &value)
    {
        m_values[L"kind"] = value;
    }

    void SetName(const std::wstring &value)
    {
        m_values[L"name"] = value;
    }
};

class SarifReplacement
{
    static constexpr int BASE10 = 10;
    json::Object m_values;

public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetOffset(int value)
    {
        m_values[L"offset"] = json::Value(value);
    }

    void SetDeletedLength(int value)
    {
        m_values[L"deletedLength"] = json::Value(value);
    }

    void SetInsertedBytes(const std::wstring &value)
    {
        m_values[L"insertedBytes"] = value;
    }

    void SetOffset(const std::wstring &value);
    void SetDeletedLength(const std::wstring &value);
};

class SarifFileChange
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetURI(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void SetURIBaseId(const std::wstring &uriBaseId)
    {
        m_values[L"uriBaseId"] = uriBaseId;
    }

    void AddReplacement(const SarifReplacement &replacement);
};

class SarifLocation
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetFullyQualifiedLogicalName(const std::wstring &fqn)
    {
        m_values[L"fullyQualifiedLogicalName"] = fqn;
    }

    void SetLogicalLocationKey(const std::wstring &logicalLocationKey)
    {
        m_values[L"logicalLocationKey"] = logicalLocationKey;
    }

    void SetAnalysisTarget(const SarifPhysicalLocation &physicalLocation)
    {
        m_values[L"analysisTarget"] = physicalLocation.GetJsonObject();
    }

    void SetResultFile(const SarifPhysicalLocation &physicalLocation)
    {
        m_values[L"resultFile"] = physicalLocation.GetJsonObject();
    }

    void SetDecoratedName(const std::wstring &decoratedName)
    {
        m_values[L"decoratedName"] = decoratedName;
    }

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifCodeFlow
{
    json::Object m_values;
public:

    SarifCodeFlow()
    {
        SetMessage(L"");
    }

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetMessage(const std::wstring &message)
    {
        m_values[L"message"] = message;
    }

    void AddAnnotatedCodeLocation(const SarifAnnotatedCodeLocation &location);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifFix
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetDescription(const std::wstring &value)
    {
        m_values[L"description"] = value;
    }

    void AddFileChange(const SarifFileChange &change);
};

class SarifFormattedMessage
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetFormatId(const std::wstring &formatId)
    {
        m_values[L"formatId"] = AddEscapeCharacters(formatId);
    }

    void AddArgument(const std::wstring &argument);
};

class SarifStackFrame
{
    static constexpr int BASE10 = 10;
    json::Object m_values;

public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetMessage(const std::wstring &msg)
    {
        m_values[L"message"] = AddEscapeCharacters(msg);
    }

    void SetUri(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void SetLine(const std::wstring &line)
    {
        m_values[L"line"] = line;
    }

    void SetColumn(const std::wstring &column)
    {
        m_values[L"column"] = column;
    }

    void SetModule(const std::wstring &module)
    {
        m_values[L"module"] = module;
    }

    void SetFullyQualifiedLogicalName(const std::wstring &fqln)
    {
        m_values[L"fullyQualifiedLogicalName"] = fqln;
    }

    void SetAddress(const std::wstring &address)
    {
        m_values[L"address"] = address;
    }

    void SetOffset(const std::wstring &offset)
    {
        m_values[L"offset"] = offset;
    }

    void SetThreadId(int threadId)
    {
        m_values[L"threadId"] = json::Value(threadId);
    }

    void SetThreadId(const std::wstring &time);

    void AddParameter(const std::wstring &parameter);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifStack
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetMessage(const std::wstring &msg)
    {
        m_values[L"message"] = AddEscapeCharacters(msg);
    }

    void AddFrame(const SarifStackFrame &stackFrame);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifResult
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetSuppressedInSource(const std::wstring &value)
    {
        m_values[L"suppressionStates"] = value;
    }

    void SetRuleId(const std::wstring &id)
    {
        m_values[L"ruleId"] = id;
    }

    void SetRuleKey(const std::wstring &ruleKey)
    {
        m_values[L"ruleKey"] = ruleKey;
    }

    void SetLevel(const std::wstring &level)
    {
        m_values[L"level"] = level;
    }

    void SetMessage(const std::wstring &message)
    {
        m_values[L"message"] = AddEscapeCharacters(message);
    }

    void SetFormattedRuleMessage(const SarifFormattedMessage &formattedRuleMessage)
    {
        m_values[L"formattedRuleMessage"] = formattedRuleMessage.GetJsonObject();
    }

    void SetToolFingerprint(const std::wstring &toolFingerprint)
    {
        m_values[L"toolFingerprint"] = AddEscapeCharacters(toolFingerprint);
    }

    void SetCodeSnippet(const std::wstring &codeSnippet)
    {
        m_values[L"codeSnippet"] = AddEscapeCharacters(codeSnippet);
    }

    void SetId(const std::wstring &id)
    {
        m_values[L"id"] = id;
    }

    void SetBaselineState(const std::wstring &baselineState)
    {
        m_values[L"baselineState"] = baselineState;
    }

    void AddFix(const SarifFix &fix);
    void AddStack(const SarifStack &stack);
    void AddLocation(const SarifLocation &location);
    void AddCodeFlow(const SarifCodeFlow &codeFlow);
    void AddRelatedLocation(const SarifAnnotatedCodeLocation &relatedLocation);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifHash
{
    json::Object m_values;
public:
    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetAlgorithm(const std::wstring &algorithm)
    {
        m_values[L"algorithm"] = algorithm;
    }

    void SetValue(const std::wstring &value)
    {
        m_values[L"value"] = value;
    }
};

class SarifFile
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetURI(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void SetRelativeTo(const std::wstring &relativeTo)
    {
        m_values[L"relativeTo"] = relativeTo;
    }

    void SetOffset(const std::wstring &offset)
    {
        m_values[L"offset"] = offset;
    }

    void SetLength(const std::wstring &length)
    {
        m_values[L"length"] = length;
    }

    void SetMimeType(const std::wstring &mimeType)
    {
        m_values[L"mimeType"] = mimeType;
    }

    void AddHash(const SarifHash &hash);
    void AddHash(const std::wstring &algoritm, const std::wstring &value);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifTool
{
    json::Object m_values;
public:
    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetName(const std::wstring &name)
    {
        m_values[L"name"] = name;
    }

    void SetFullName(const std::wstring &fullName)
    {
        m_values[L"fullName"] = fullName;
    }

    void SetVersion(const std::wstring &version)
    {
        m_values[L"version"] = version;
    }

    void SetSemanticVersion(const std::wstring &semanticVersion)
    {
        m_values[L"semanticVersion"] = semanticVersion;
    }

    void SetFileVersion(const std::wstring &fileVersion)
    {
        m_values[L"fileVersion"] = fileVersion;
    }

    void SetSarifLoggerVersion(const std::wstring &sarifLoggerVersion)
    {
        m_values[L"sarifLoggerVersion"] = sarifLoggerVersion;
    }

    void SetLanguage(const std::wstring &language)
    {
        m_values[L"language"] = language;
    }

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifLogicalLocation
{
    json::Array m_values;
public:
    const json::Array& GetJsonArray() const
    {
        return m_values;
    }

    void AddLogicalLocationComponent(const std::wstring &name, const wchar_t *locationKind);
    void AddLogicalLocationComponent(const SarifLogicalLocationComponent &logicalLocationComponent);
};

class SarifInvocation
{
    json::Object m_values;
public:
    const json::Object& GetJsonObject() const
    {
        return m_values;
    }
    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifRule
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetId(const std::wstring &id)
    {
        m_values[L"id"] = AddEscapeCharacters(id);
    }

    void SetName(const std::wstring &name)
    {
        m_values[L"name"] = AddEscapeCharacters(name);
    }

    void SetShortDescription(const std::wstring &shortDescription)
    {
        m_values[L"shortDescription"] = shortDescription;
    }

    void SetFullDescription(const std::wstring &fullDescription)
    {
        m_values[L"fullDescription"] = fullDescription;
    }

    void SetDefaultLevel(const std::wstring &defaultLevel)
    {
        m_values[L"defaultLevel"] = defaultLevel;
    }

    void SetHelpUri(const std::wstring &helpUri)
    {
        m_values[L"helpUri"] = helpUri;
    }

    void AddMessageFormat(const std::wstring &key, const std::wstring &messageFormat);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifException
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetKind(const std::wstring &kind)
    {
        m_values[L"kind"] = kind;
    }

    void SetMessage(const std::wstring &message)
    {
        m_values[L"message"] = AddEscapeCharacters(message);
    }

    void SetStack(const SarifStack &stack)
    {
        m_values[L"stack"] = stack.GetJsonObject();
    }

    void AddInnerException(const SarifException &innerException);
};

class SarifNotification
{
    static constexpr int BASE10 = 10;
    json::Object m_values;

public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetId(const std::wstring &id)
    {
        m_values[L"id"] = id;
    }

    void SetRuleId(const std::wstring &ruleId)
    {
        m_values[L"ruleId"] = ruleId;
    }

    void SetRuleKey(const std::wstring &ruleKey)
    {
        m_values[L"ruleKey"] = ruleKey;
    }

    void SetAnalysisTarget(const SarifPhysicalLocation &analysisTarget)
    {
        m_values[L"analysisTarget"] = analysisTarget.GetJsonObject();
    }

    void SetMessage(const std::wstring &message)
    {
        m_values[L"message"] = message;
    }

    void SetLevel(const std::wstring &level)
    {
        m_values[L"level"] = level;
    }

    void SetTime(const std::wstring &time)
    {
        m_values[L"time"] = time;
    }

    void SetException(const SarifException &exception)
    {
        m_values[L"exception"] = exception.GetJsonObject();
    }

    void SetThreadId(int threadId)
    {
        m_values[L"threadId"] = json::Value(threadId);
    }

    void SetThreadId(const std::wstring &time);

    void AddTag(const std::wstring &tag);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifRun
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetId(const std::wstring &id)
    {
        m_values[L"id"] = id;
    }

    void SetCorrelationId(const std::wstring &correlationId)
    {
        m_values[L"correlationId"] = correlationId;
    }

    void SetTool(const SarifTool &info)
    {
        m_values[L"tool"] = info.GetJsonObject();
    }

    void SetInvocation(const SarifInvocation &invocation)
    {
        m_values[L"invocation"] = invocation.GetJsonObject();
    }

    void SetAnalysisTarget(const SarifPhysicalLocation &physicalLocation)
    {
        m_values[L"analysisTarget"] = physicalLocation.GetJsonObject();
    }

    void AddRule(const std::wstring &key, const SarifRule &rule);
    void AddResult(const SarifResult &result);
    void AddFile(const std::wstring &key, const SarifFile &file);
    void AddToolNotification(const SarifNotification &notification);
    void AddConfigurationNotification(const SarifNotification &notification);
    void AddLogicalLocation(const std::wstring &key, const SarifLogicalLocation &logicalLocationComponents);
};

class SarifLog
{
    json::Object m_values;
public:

    const json::Object& GetJsonObject() const
    {
        return m_values;
    }

    void SetVersion(const std::wstring &version)
    {
        m_values[L"version"] = version;
    }

    void SetSchema(const std::wstring &schema)
    {
        m_values[L"$schema"] = schema;
    }

    void AddRun(const SarifRun &run);
};
