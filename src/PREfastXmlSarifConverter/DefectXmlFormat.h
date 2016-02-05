/////////////////////////////////////////////////////////////////////////////
// Copyright © 2015 Microsoft Corporation. All rights reserved.
// xmlfmt.h : Declaration of the PREfast XML formated STRUCT
//

#pragma once
#include <string>
#include <vector>
#include <map>
#include <queue>

#include <ole2.h>
#include <xmllite.h>
#include <shlwapi.h>
#include <stdio.h>

class XmlKeyEvent
{
    bool m_valid;
    std::wstring m_id;
    std::wstring m_kind;
    std::wstring m_importance;
    std::wstring m_message;
public:
    XmlKeyEvent() : m_valid(false)
    {
    }

    XmlKeyEvent(const wchar_t *id,
        const wchar_t *kind,
        const wchar_t *importance,
        const wchar_t *message) :
        m_valid(true),
        m_id(id),
        m_kind(kind),
        m_importance(importance),
        m_message(message)
    {
    }

	const wchar_t* GetId() const { return m_id.c_str(); }
	const wchar_t* GetKind() const { return m_kind.c_str(); }
	const wchar_t* GetImportance() const { return m_importance.c_str(); }
	const wchar_t* GetMessage() const { return m_message.c_str(); }
	bool IsValid() const { return m_valid; }

    void SetId(const wchar_t *id) { m_id = id;  }
    void SetKind(const wchar_t *kind) { m_kind = kind; }
    void SetImportance(const wchar_t *importance) { m_importance = importance; }
    void SetMessage(const wchar_t *message) { m_message = message; }
    void SetIsValid(bool bValid) { m_valid = bValid; }
};

class XmlSfa
{
    std::wstring m_filePath;
    std::wstring m_fileName;
    std::wstring m_lineNo;
    std::wstring m_columnNo;
    XmlKeyEvent m_keyEvent;
public:
    XmlSfa() = default;

    const wchar_t* GetFilePath() const { return m_filePath.c_str(); }
    const wchar_t* GetFileName() const { return m_fileName.c_str(); }
    const wchar_t* GetLineNo() const { return m_lineNo.c_str(); }
    const wchar_t* GetColumnNo() const { return m_columnNo.c_str(); }
    const XmlKeyEvent& GetKeyEvent() const { return m_keyEvent; }

    void SetFilePath(const wchar_t *filePath) { m_filePath = filePath; }
    void SetFileName(const wchar_t *fileName) { m_fileName = fileName; }
    void SetLineNo(const wchar_t *lineNo) { m_lineNo = lineNo; }
    void SetColumnNo(const wchar_t *columnNo) { m_columnNo = columnNo; }
    void SetKeyEvent(const XmlKeyEvent &keyEvent) { m_keyEvent = keyEvent; }
};

class XmlDefect
{
    std::wstring m_defectcode;
    std::wstring m_description;
    std::wstring m_function;
    std::wstring m_decorated;

    std::wstring m_functionLine;
    std::wstring m_probability;
    std::wstring m_rank;

public:

    XmlSfa m_sfa;
    std::map<bstr_t, bstr_t> m_category;
    std::map<bstr_t, bstr_t> m_additionalInfo;
    std::vector<XmlSfa> m_path;

    XmlDefect() = default;

    const wchar_t* GetDefectCode() const { return m_defectcode.c_str(); }
    const wchar_t* GetDescription() const { return m_description.c_str(); }
    const wchar_t* GetFunction() const { return m_function.c_str(); }
    const wchar_t* GetDecorated() const { return m_decorated.c_str(); }

    const wchar_t* GetFunctionLine() const { return m_functionLine.c_str(); }
    const wchar_t* GetProbability() const { return m_probability.c_str(); }
    const wchar_t* GetRank() const { return m_rank.c_str(); }

    void SetDefectCode(const wchar_t *code) { m_defectcode = code; }
    void SetDescription(const wchar_t *description) { m_description = description; }
    void SetFunction(const wchar_t *function) { m_function = function; }
    void SetDecorated(const wchar_t *decorated) { m_decorated = decorated; }

    void SetFunctionLine(const wchar_t *functionLine) { m_functionLine = functionLine; }
    void SetProbability(const wchar_t *probability) { m_probability = probability; }
    void SetRank(const wchar_t *rank) { m_rank = rank; }
};
