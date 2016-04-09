#pragma once 
#include "targetver.h"
#include <stdio.h>
#include <tchar.h>
#include <Windows.h>
#include <Shlwapi.h>
#include <Winerror.h>
#include <comutil.h>

#include "DefectXmlFormat.h"
#include "JsonSerialize.h"
#include <iostream>
#include <string>
#include <algorithm>
#include <vector>
#include <unordered_map>

bool hasEnding(const _bstr_t &strValue, const std::wstring &ending);
std::wstring AddEscapeCharacters(const std::wstring &data);
std::wstring GetDefectUri(const XmlSfa &sfa);
std::wstring MakeItUri(const std::wstring &fullPath);
void GetXmlToSarifMapping(const std::wstring &prefastTag, std::wstring &sarifTag);

class SarifRegion
{
    int m_startLine;
    int m_startColumn;
public:
    json::Object m_values;
	static constexpr int BASE10 = 10;

	void SetStartLine(const std::wstring &value);
	void SetStartColumn(const std::wstring &value);
	void SetEndLine(const std::wstring &value);
	void SetEndColumn(const std::wstring &value);
	void SetCharOffset(const std::wstring &value);
	void SetByteOffset(const std::wstring &value);
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

    void SetCharOffset(int value)
    {
        m_values[L"charOffset"] = json::Value(value);
    }

    void SetByteOffset(int value)
    {
        m_values[L"byteOffset"] = json::Value(value);
    }

    void SetLength(int value)
    {
        m_values[L"length"] = json::Value(value);
    }

};

class SarifPhysicalLocationComponent
{
public:
    json::Object m_values;

    void SetURI(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void SetMimeType(const std::wstring &value)
    {
        m_values[L"mimeType"] = json::Value(value);
    }

    void SetRegion(SarifRegion region)
    {
        m_values[L"region"] = region.m_values;
    }
};

class SarifLogicalLocationComponent
{
public:
    json::Object m_values;

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
public:
    json::Object m_values;

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
};

class SarifFileChange
{
public:
    json::Object m_values;
    void SetURI(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void AddReplacement(const SarifReplacement &replacement);
};


class SarifLocation
{
public:
    json::Object m_values;

    void AddAnalysisTargetComponent(const SarifPhysicalLocationComponent &component);
    void AddIssueFileComponent(const SarifPhysicalLocationComponent &component);
    void AddLogicalLocationComponent(const std::wstring &name, const wchar_t *locationKind);
    void AddLogicalLocationComponent(const SarifLogicalLocationComponent &component);
    void SetFullyQualifiedLogicalName(const std::wstring &fqn);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};




class SarifExecutionFlowEntry
{
public:
    json::Object m_values;

	void SetMessage(const std::wstring &value);
	void AddPhysicalLocationComponent(const SarifPhysicalLocationComponent &file);
    void AddProperty(const std::wstring &key, const std::wstring &value);
};


class SarifExecutionFlowEntries
{
public:
    json::Object m_values;
    void AddExecutionFlowEntry(const SarifExecutionFlowEntry &entry);
};

class SarifFix
{
public:
    json::Object m_values;

    void SetDescription(const std::wstring &value)
    {
        m_values[L"description"] = value;
    }

    void AddFileChange(const SarifFileChange &change);
};

class SarifIssue
{
public:
    json::Object m_values;

    void SetSuppressedInSource(const std::wstring &value)
    {
        m_values[L"isSuppressedInSource"] = value;
    }

    void SetRuleId(const std::wstring &id)
    {
        m_values[L"ruleId"] = id;
    }

    void SetShortMessage(const std::wstring &message)
    {
        m_values[L"shortMessage"] = message;
    }

	void SetFullMessage(const std::wstring &message);
    void AddProperty(const std::wstring &key, const std::wstring &value);
    void AddLocation(const SarifLocation &location);
    void AddExecutionFlow(const SarifExecutionFlowEntries &exeFlow);
    void AddStack(const SarifExecutionFlowEntries &exeFlow);
    void AddFix(const SarifFix &fix);
};

class SarifHash
{
public:
    json::Object m_values;
    void SetAlgorithm(const std::wstring &algorithm)
    {
        m_values[L"algorithm"] = algorithm;
    }

    void SetValue(const std::wstring &value)
    {
        m_values[L"value"] = value;
    }
};

class SarifFileReference
{
public:
    json::Object m_values;

    void SetURI(const std::wstring &uri)
    {
        m_values[L"uri"] = uri;
    }

    void AddHash(const std::wstring &algoritm, const std::wstring &value);
    void AddHash(const SarifHash &hash);
};

class SarifRunInfo
{
public:
    json::Object m_values;
    void SetCommandLineArguments(const std::wstring &args)
    {
        m_values[L"invocation"] = args;
    }

    void AddAnalysisTarget(const SarifFileReference &target);
};

class SarifToolInfo
{
public:
    json::Object m_values;
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

    void SetFileVersion(const std::wstring &fileVersion)
    {
        m_values[L"fileVersion"] = fileVersion;
    }
};


class SarifRunLog
{
public:
    json::Object m_values;

    void SetToolInfo(const SarifToolInfo &info)
    {
        m_values[L"tool"] = info.m_values;
    }

    void SetRunInfo(const SarifRunInfo &info)
    {
        m_values[L"run"] = info.m_values;
    }

    void AddIssue(const SarifIssue &issue);
};

class SarifIssueLog
{
public:
    json::Object m_values;

    void SetVersion(const std::wstring &version)
    {
        m_values[L"version"] = version;
    }

    void AddRunLog(const SarifRunLog &log);
};

