#include "stdafx.h"
#include "SarifFormat.h"
#include <iterator>
#include <array>

void
GetXmlToSarifMapping(const std::wstring &prefastTag, std::wstring &sarifTag)
{
    // Full list for additional info is not available, so far it is only RULECATEGORY
    static std::map<std::wstring, std::wstring> mapping =
    {
        { L"RULECATEGORY", L"ruleCategory" }
    };

    _ASSERTE(mapping.find(prefastTag) != mapping.end());

    std::map<std::wstring, std::wstring>::iterator it = mapping.find(prefastTag);
    if (it != mapping.end())
        sarifTag = it->second;
}

bool
hasEnding(const _bstr_t &strValue, const std::wstring &ending)
{
    std::wstring fullString = strValue;
    if (fullString.length() <= ending.length())
        return false;

    size_t extPos = fullString.length() - ending.length();
    for (size_t i = 0, j = extPos; i < ending.length(); i++, j++)
    {
        if (towlower(fullString[j]) != towlower(ending[i]))
            return false;
    }
    return true;
}

std::wstring
AddEscapeCharacters(const std::wstring &data)
{
    static const std::array<const wchar_t, 10> src_chars = {
        '\a', L'\b', L'\f', L'\n', L'\r', L'\t', '\v',
        L'\\', L'"', L'/'
    };

    static const std::array<const wchar_t, 10> mapped_chars = {
        L'a', L'b', L'f', L'n', L'r', L't', L'v',
        L'\\', L'"', L'/'
    };

    std::wstring result;
    result.reserve(data.length() * 2);

    // Stripping off legacy PREFAST_NEWLINE markers from SARIF descriptions
    // Replacing control characters with literal characters by prepending '\\'
    for (std::wstring::const_iterator it = data.begin(); it != data.end(); ++it)
    {
        if (*it == L'\n')
        {
            auto beg = std::distance(data.begin(), it);
            auto end = data.find(L"PREFAST_NEWLINE\n", beg);
            if (end != std::wstring::npos)
            {
                // If end != std::wstring::npos, then it is safe to add iterator by countof(L"PREFAST_NEWLINE\n") - 1. 
                it += _countof(L"PREFAST_NEWLINE\n") - 1;
            }
        }

        auto mapit = std::find(src_chars.begin(), src_chars.end(), *it);
        if (mapit != src_chars.end())
        {
            result += '\\';
            auto pos = std::distance(src_chars.begin(), mapit);
            result += mapped_chars[pos];
        }
        else
        {
            result += *it;
        }

    }
    result.shrink_to_fit();

    return result;
}


std::wstring
MakeItUri(const std::wstring &path)
{
    std::wstring uri = L"file:///";
    uri += path;
    std::replace(uri.begin(), uri.end(), L'\\', L'/');
    return uri;
}

std::wstring
GetDefectUri(const XmlSfa &sfa)
{
    std::wstring path = sfa.GetFilePath();
    path += sfa.GetFileName();
    return MakeItUri(path);
}

bool
SarifRegion::IsValid()
{
    //This is handling a special case.PREfast indicates an internal error by emitting a "defect" whose line and column are 0, 0. 
    //That is, PREfast is saying that this defect is not associated with any particular region in the file.
    //The IsValid method returns false precisely when line, column = 0, 0.
    //SARIF indicates that a defect is not associated with a region by omitting the region property from the physicalLocationComponent object.
    //So here, we only set the region property when the defect is associated with a region.

    int startLine = GetStartLine();
    int startColumn = GetStartColumn();
    if (startLine > 0 && startColumn >= 0)
    {
        // Sarif uses a 1-indexed startColumn. Now that we know
        // this region is valid, we will increment here
        SetStartColumn(++startColumn);
        return true;
    }
    else if (startLine == 0 && startColumn == 0)
        return false;
    std::string error = "Invalid region specified.";
    error += "Start Line : " + std::to_string(startLine);
    error += "Start Column : " + std::to_string(startColumn);
    throw std::exception(error.c_str());
}

int
SarifRegion::GetStartLine() const
{
    json::Object::ValueMap::const_iterator it = m_values.find(L"startLine");
    if (it != m_values.end())
        return it->second.ToInt();
    return -1;
}

int
SarifRegion::GetStartColumn() const
{
    json::Object::ValueMap::const_iterator it = m_values.find(L"startColumn");
    if (it != m_values.end())
        return it->second.ToInt();
    return -1;
}

void
SarifRegion::SetStartLine(int value)
{
    m_values[L"startLine"] = json::Value(value);
}

void
SarifRegion::SetStartColumn(int value)
{
    m_values[L"startColumn"] = json::Value(value);
}

void
SarifRegion::SetStartLine(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Start Line specified.");
    SetStartLine(intVal);
}

void
SarifRegion::SetStartColumn(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Start Column specified.");
    SetStartColumn(intVal);
}

void
SarifRegion::SetEndLine(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid End Line specified.");
    SetEndLine(intVal);
}

void
SarifRegion::SetEndColumn(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid End Column specified.");
    SetEndColumn(intVal);
}

void
SarifRegion::SetOffset(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Offset specified.");
    SetOffset(intVal);
}

void
SarifRegion::SetLength(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Length specified.");
    SetLength(intVal);
}

void
SarifFileChange::AddReplacement(const SarifReplacement &replacement)
{
    m_values.GetArrayElement(L"replacements").push_back(replacement.GetJsonObject());
}

void
SarifLogicalLocation::AddLogicalLocationComponent(const std::wstring &name, const wchar_t *locationKind)
{
    SarifLogicalLocationComponent location;
    location.SetLocationKind(locationKind);
    location.SetName(name);
    AddLogicalLocationComponent(location);
}

void
SarifLogicalLocation::AddLogicalLocationComponent(const SarifLogicalLocationComponent &component)
{
    m_values.push_back(component.GetJsonObject());
}

void
SarifLocation::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifLocation::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifInvocation::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifInvocation::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();
    m_values[L"properties"][key] = value;
}

void
SarifCodeFlow::AddAnnotatedCodeLocation(const SarifAnnotatedCodeLocation &location)
{
    m_values.GetArrayElement(L"locations").push_back(location.GetJsonObject());
}

void
SarifCodeFlow::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();
    m_values[L"properties"][key] = value;
}

void
SarifAnnotatedCodeLocation::SetThreadId(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Offset specified.");
    SetThreadId(intVal);
}

void
SarifAnnotatedCodeLocation::SetEssential(const std::wstring &value)
{
    const wchar_t* essential = value.c_str();

    if (wcscmp(essential, L"TRUE") == 0 ||
        wcscmp(essential, L"True") == 0 ||
        wcscmp(essential, L"true") == 0)
    {
        SetEssential(true);
        return;
    }

    if (wcscmp(essential, L"FALSE") == 0 ||
        wcscmp(essential, L"False") == 0 ||
        wcscmp(essential, L"false") == 0)
    {
        SetEssential(false);
    }

    throw std::exception("Value could not be converted to bool.");
}

void
SarifAnnotatedCodeLocation::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifAnnotatedCodeLocation::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();
    m_values[L"properties"][key] = value;
}

void
SarifFix::AddFileChange(const SarifFileChange &change)
{
    m_values.GetArrayElement(L"fileChanges").push_back(change.GetJsonObject());
}

void
SarifResult::AddLocation(const SarifLocation &location)
{
    m_values.GetArrayElement(L"locations").push_back(location.GetJsonObject());
}

void
SarifResult::AddRelatedLocation(const SarifAnnotatedCodeLocation &location)
{
    m_values.GetArrayElement(L"relatedLocations").push_back(location.GetJsonObject());
}

void
SarifResult::AddStack(const SarifStack &stack)
{
    m_values.GetArrayElement(L"stacks").push_back(stack.GetJsonObject());
}

void
SarifResult::AddCodeFlow(const SarifCodeFlow &codeFlow)
{
    m_values.GetArrayElement(L"codeFlows").push_back(codeFlow.GetJsonObject());
}

void
SarifResult::AddFix(const SarifFix &fix)
{
    m_values.GetArrayElement(L"fixes").push_back(fix.GetJsonObject());
}

void
SarifResult::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifResult::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifFile::AddHash(const std::wstring &algoritm, const std::wstring &value)
{
    SarifHash hash;
    hash.SetAlgorithm(algoritm);
    hash.SetValue(value);
    AddHash(hash);
}

void
SarifFile::AddHash(const SarifHash &hash)
{
    m_values.GetArrayElement(L"hashes").push_back(hash.GetJsonObject());
}

void
SarifFile::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifFile::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifRun::AddRule(const std::wstring &key, const SarifRule &rule)
{
    if (m_values.find(L"rules") == m_values.end())
        m_values[L"rules"] = json::Object();

    m_values[L"rules"][key] = rule.GetJsonObject();
}

void
SarifRun::AddFile(const std::wstring &key, const SarifFile &file)
{
    if (m_values.find(L"files") == m_values.end())
        m_values[L"files"] = json::Object();

    m_values[L"files"][key] = file.GetJsonObject();
}

void
SarifRun::AddToolNotification(const SarifNotification &toolNotification)
{
    m_values.GetArrayElement(L"toolNotification").push_back(toolNotification.GetJsonObject());
}

void
SarifRun::AddConfigurationNotification(const SarifNotification &configurationNotification)
{
    m_values.GetArrayElement(L"configurationNotifications").push_back(configurationNotification.GetJsonObject());
}

void
SarifRun::AddLogicalLocation(const std::wstring &key, const SarifLogicalLocation &location)
{
    if (m_values.find(L"logicalLocations") == m_values.end())
        m_values[L"logicalLocations"] = json::Object();

    m_values[L"logicalLocations"][key] = location.GetJsonArray();
}

void
SarifRun::AddResult(const SarifResult &result)
{
    m_values.GetArrayElement(L"results").push_back(result.GetJsonObject());
}

void
SarifLog::AddRun(const SarifRun &run)
{
    m_values.GetArrayElement(L"runs").push_back(run.GetJsonObject());
}

void
SarifTool::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifTool::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifException::AddInnerException(const SarifException &exception)
{
    m_values.GetArrayElement(L"innerException").push_back(exception.GetJsonObject());
}

void
SarifFormattedMessage::AddArgument(const std::wstring &argument)
{
    m_values.GetArrayElement(L"arguments").push_back(argument);
}

void
SarifNotification::SetThreadId(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Offset specified.");
    SetThreadId(intVal);
}

void
SarifNotification::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifNotification::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifRule::AddMessageFormat(const std::wstring &key, const std::wstring &messageFormat)
{
    if (m_values.find(L"messageFormats") == m_values.end())
        m_values[L"messageFormats"] = json::Object();

    m_values[L"messageFormats"][key] = messageFormat;
}

void
SarifRule::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifRule::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifStack::AddFrame(const SarifStackFrame &stackFrame)
{
    m_values.GetArrayElement(L"frames").push_back(stackFrame.GetJsonObject());
}

void
SarifStack::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifStack::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifStackFrame::SetThreadId(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Offset specified.");
    SetThreadId(intVal);
}

void
SarifStackFrame::AddParameter(const std::wstring &parameter)
{
    m_values.GetArrayElement(L"parameters").push_back(parameter);
}

void
SarifStackFrame::AddTag(const std::wstring &tag)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    ((json::Object)m_values[L"properties"]).GetArrayElement(L"tags").push_back(tag);
}

void
SarifStackFrame::AddProperty(const std::wstring &key, const std::wstring &value)
{
    if (m_values.find(L"properties") == m_values.end())
        m_values[L"properties"] = json::Object();

    m_values[L"properties"][key] = value;
}

void
SarifReplacement::SetOffset(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Offset specified.");
    SetOffset(intVal);
}

void
SarifReplacement::SetDeletedLength(const std::wstring &value)
{
    wchar_t * pEnd;
    int intVal = wcstol(value.c_str(), &pEnd, BASE10);
    if (*pEnd)
        throw std::exception("Invalid Deleted Length specified.");
    SetDeletedLength(intVal);
}
