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

    void SetCharOffset(int value)
    {
        m_values[L"charOffset"] = json::Value(value);
    }

    void SetOffset(int value)
    {
        m_values[L"offset"] = json::Value(value);
    }

    void SetLength(int value)
    {
        m_values[L"length"] = json::Value(value);
    }

};

class SarifPhysicalLocation
{
public:
	json::Object m_values;

	void SetURI(const std::wstring &uri)
	{
		m_values[L"uri"] = uri;
	}

	void SetRegion(SarifRegion region)
	{
		m_values[L"region"] = region.m_values;
	}
};

class SarifAnnotatedCodeLocation
{
public:
	json::Object m_values;

	void SetPhysicalLocation(const SarifPhysicalLocation &physicalLocation)
	{
		m_values[L"physicalLocation"] = physicalLocation.m_values;
	}

	void SetMessage(const std::wstring &msg)
	{
		m_values[L"message"] = AddEscapeCharacters(msg);
	}

	void AddProperty(const std::wstring &key, const std::wstring &value);
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


	void SarifLocation::SetFullyQualifiedLogicalName(const std::wstring &fqn)
	{
		m_values[L"fullyQualifiedLogicalName"] = fqn;
	}

	void SarifLocation::SetAnalysisTarget(const SarifPhysicalLocation &physicalLocation)
	{
		m_values[L"analysisTarget"] = physicalLocation.m_values;
	}

	void SarifLocation::SetResultFile(const SarifPhysicalLocation &physicalLocation)
	{
		m_values[L"resultFile"] = physicalLocation.m_values;
	}

	void AddLogicalLocationComponent(const std::wstring &name, const wchar_t *locationKind);
    void AddLogicalLocationComponent(const SarifLogicalLocationComponent &component);
	void AddProperty(const std::wstring &key, const std::wstring &value);
};

class SarifCodeFlow
{
public:
    json::Array m_values;
    void AddAnnotatedCodeLocation(const SarifAnnotatedCodeLocation &location);
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

class SarifResult
{
public:
    json::Object m_values;

    void SetSuppressedInSource(const std::wstring &value)
    {
        m_values[L"SuppressionStates"] = value;
    }

    void SetRuleId(const std::wstring &id)
    {
        m_values[L"ruleId"] = id;
    }

    void SetShortMessage(const std::wstring &message)
    {
        m_values[L"shortMessage"] = AddEscapeCharacters(message);
    }

	void SetFullMessage(const std::wstring &message)
	{
		m_values[L"fullMessage"] = AddEscapeCharacters(message);
	}

    void AddProperty(const std::wstring &key, const std::wstring &value);
    void AddLocation(const SarifLocation &location);
    void AddCodeFlow(const SarifCodeFlow &codeFlow);
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

class SarifFile
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

class SarifTool
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


class SarifRun
{
public:
    json::Object m_values;

	void SetCommandLineArguments(const std::wstring &args)
	{
		m_values[L"invocation"] = args;
	}

    void SetTool(const SarifTool &info)
    {
        m_values[L"tool"] = info.m_values;
    }

    void AddResult(const SarifResult &result);

	void AddFile(const std::wstring &key, const SarifFile &file);
};

class SarifLog
{
public:
    json::Object m_values;

    void SetVersion(const std::wstring &version)
    {
        m_values[L"version"] = version;
    }

    void AddRun(const SarifRun &run);
};

