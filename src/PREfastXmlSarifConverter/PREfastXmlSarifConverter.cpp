// PREfastXmlSarifConverter.cpp : Defines the entry point for the console application.
//

#include <fstream>
#include "SarifFormat.h"
#include "DefectXmlFormat.h"

extern "C" {__declspec(dllexport) BSTR __stdcall ConvertToSarif(BSTR bstrFilePath); }
extern "C" {__declspec(dllexport) BSTR __stdcall ConvertToSarifFromXml(BSTR bstrXmlText); }

HRESULT __stdcall ConvertToSarifHelperFromText(BSTR bstrXmlText, BSTR* pbstrSarifText);
HRESULT __stdcall ConvertToSarifHelperFromFile(BSTR bstrInputFile, BSTR bstrOutputFile, BSTR* pbstrSarifText);

class XmlToSarifConverter
{
private:

    bool ReadProperty(IXmlReader *pReader, std::map<bstr_t, bstr_t>  &category)
    {
        while (!pReader->IsEOF())
        {
            XmlNodeType nodeType;
            HRESULT hr = pReader->Read(&nodeType);
            if (hr != S_OK)
                break;

            if (nodeType == XmlNodeType_Element)
            {
                LPCWSTR wszLocalName, wszValue;
                UINT cchLocalName, cchValue;

                pReader->GetLocalName(&wszLocalName, &cchLocalName);
                pReader->GetValue(&wszValue, &cchValue);

                if (wcslen(wszLocalName) > 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    category[wszLocalName] = wszValue;
                }
                pReader->Read(&nodeType);
            }
            else if (nodeType == XmlNodeType_EndElement)
            {
                break;
            }
        }

        return true;
    }

    bool ReadKeyEvent(IXmlReader *pReader, XmlKeyEvent &event)
    {
        while (!pReader->IsEOF())
        {
            XmlNodeType nodeType;
            HRESULT hr = pReader->Read(&nodeType);
            if (hr != S_OK)
                break;

            if (nodeType == XmlNodeType_Element)
            {
                LPCWSTR wszLocalName, wszValue;
                UINT cchLocalName, cchValue;

                pReader->GetLocalName(&wszLocalName, &cchLocalName);
                pReader->GetValue(&wszValue, &cchValue);

                if (wcscmp(wszLocalName, L"ID") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    event.SetId(wszValue);
                    event.SetIsValid(true);
                }
                else if (wcscmp(wszLocalName, L"KIND") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    event.SetKind(wszValue);
                }
                else if (wcscmp(wszLocalName, L"IMPORTANCE") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    event.SetImportance(wszValue);
                }
                else if (wcscmp(wszLocalName, L"MESSAGE") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    event.SetMessage(wszValue);
                }
                pReader->Read(&nodeType);
            }
            else if (nodeType == XmlNodeType_EndElement)
            {
                break;
            }
        }

        return true;
    }

    bool ReadSFA(IXmlReader *pReader, XmlSfa &sfa)
    {
        while (!pReader->IsEOF())
        {
            XmlNodeType nodeType;
            HRESULT hr = pReader->Read(&nodeType);
            if (hr != S_OK)
                break;

            if (nodeType == XmlNodeType_Element)
            {
                LPCWSTR wszLocalName, wszValue;
                UINT cchLocalName, cchValue;

                pReader->GetLocalName(&wszLocalName, &cchLocalName);
                pReader->GetValue(&wszValue, &cchValue);

                if (wcscmp(wszLocalName, L"FILEPATH") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    sfa.SetFilePath(wszValue);
                }
                else if (wcscmp(wszLocalName, L"FILENAME") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    sfa.SetFileName(wszValue);
                }
                else if (wcscmp(wszLocalName, L"COLUMN") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    sfa.SetColumnNo(wszValue);

                }
                else if (wcscmp(wszLocalName, L"LINE") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    sfa.SetLineNo(wszValue);
                }
                else if (wcscmp(wszLocalName, L"KEYEVENT") == 0)
                {
                    XmlKeyEvent keyEvent;
                    ReadKeyEvent(pReader, keyEvent);
                    sfa.SetKeyEvent(keyEvent);
                }
                pReader->Read(&nodeType); // Covers an end element tag
            }
            else if (nodeType == XmlNodeType_EndElement)
            {
                break;
            }
        }

        return true;
    }

    bool ReadPath(IXmlReader *pReader, std::vector<XmlSfa> &path)
    {
        while (!pReader->IsEOF())
        {
            XmlNodeType nodeType;
            HRESULT hr = pReader->Read(&nodeType);
            if (hr != S_OK)
                break;

            if (nodeType == XmlNodeType_Element)
            {
                LPCWSTR wszLocalName, wszValue;
                UINT cchLocalName, cchValue;

                pReader->GetLocalName(&wszLocalName, &cchLocalName);
                pReader->GetValue(&wszValue, &cchValue);

                if (wcscmp(wszLocalName, L"SFA") == 0)
                {
                    XmlSfa sfa;
                    ReadSFA(pReader, sfa);
                    path.push_back(sfa);
                }
            }
            else if (nodeType == XmlNodeType_EndElement)
            {
                break;
            }
        }

        return true;
    }

    bool ReadDefect(IXmlReader *pReader, XmlDefect &defect)
    {
        while (!pReader->IsEOF())
        {
            XmlNodeType nodeType;
            HRESULT hr = pReader->Read(&nodeType);
            if (hr != S_OK)
                break;

            if (nodeType == XmlNodeType_Whitespace)
                continue;

            if (nodeType == XmlNodeType_Element)
            {
                LPCWSTR wszLocalName, wszValue;
                UINT cchLocalName, cchValue;

                pReader->GetLocalName(&wszLocalName, &cchLocalName);
                pReader->GetValue(&wszValue, &cchValue);

                if (wcscmp(wszLocalName, L"SFA") == 0)
                {
                    ReadSFA(pReader, defect.m_sfa);
                }
                else if (wcscmp(wszLocalName, L"PATH") == 0)
                {
                    ReadPath(pReader, defect.m_path);
                }
                // probability and rank
                else if (wcscmp(wszLocalName, L"DEFECTCODE") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetDefectCode(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"DESCRIPTION") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetDescription(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"FUNCTION") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetFunction(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"DECORATED") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetDecorated(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"FUNCLINE") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetFunctionLine(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"PROBABILITY") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetProbability(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"RANK") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    defect.SetRank(wszValue);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"CATEGORY") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    ReadProperty(pReader, defect.m_category);
                    pReader->Read(&nodeType);
                }
                else if (wcscmp(wszLocalName, L"ADDITIONALINFO") == 0)
                {
                    HRESULT hr = pReader->Read(&nodeType);
                    pReader->GetValue(&wszValue, &cchValue);
                    ReadProperty(pReader, defect.m_additionalInfo);
                    pReader->Read(&nodeType);
                }
            }
            else if (nodeType == XmlNodeType_EndElement)
            {
                break;
            }
        }

        return true;
    }

    bool InternalLoadXmlDefects(IStream *pInStream, std::deque<XmlDefect> &defectList)
    {
        IXmlReader *pReader = NULL;
        HRESULT hr;
        if (FAILED(hr = CreateXmlReader(__uuidof(IXmlReader), (void**)&pReader, NULL)))
        {
            wprintf(L"Error creating xml reader, error is %08.8lx", hr);
            return false;
        }

        IXmlReaderInput *pReaderInput = NULL;
        pReader->SetInput(pInStream);

        pReader->SetProperty(XmlReaderProperty_DtdProcessing, DtdProcessing_Prohibit);

        while (!pReader->IsEOF())
        {
            XmlNodeType nodeType;
            HRESULT hr = pReader->Read(&nodeType);
            if (hr != S_OK)
                break;
            if (nodeType == XmlNodeType_Whitespace)
                continue;

            if (nodeType == XmlNodeType_Element)
            {
                LPCWSTR wszLocalName, wszValue;
                UINT cchLocalName, cchValue;

                pReader->GetLocalName(&wszLocalName, &cchLocalName);
                pReader->GetValue(&wszValue, &cchValue);

                if (wcscmp(wszLocalName, L"DEFECT") == 0)
                {
                    XmlDefect defect;
                    if (ReadDefect(pReader, defect))
                        defectList.push_back(defect);
                }
            }
        }
        return true;
    }

public:

    static bool LoadXmlDefectFromString(const std::wstring xmlText, std::deque<XmlDefect> &defectList)
    {
        _bstr_t inpString = xmlText.c_str();
        IStream *pInStream = SHCreateMemStream((BYTE*)inpString.operator char *(), inpString.length());

        XmlToSarifConverter converter;
        return converter.InternalLoadXmlDefects(pInStream, defectList);
    }

    static bool LoadXmlDefects(const std::wstring &xmlFile, std::deque<XmlDefect> &defectList)
    {
        IStream *pInFileStream = NULL;
        HRESULT hr;
        if (FAILED(hr = SHCreateStreamOnFile(xmlFile.c_str(), STGM_READ, &pInFileStream)))
        {
            wprintf(L"Error creating file reader, error is %08.8lx", hr);
            return false;
        }

        XmlToSarifConverter converter;
        return converter.InternalLoadXmlDefects(pInFileStream, defectList);

    }

};

#if BUILD_CONSOLE
int main(int argc, char * argv[])
{
    if (argc != 2 && argc != 3)
    {
        std::wcout << "Usage: PREfastSarifConverter.exe <XmlFilename> [output.sarif] \n";
        return -1;
    }

    BSTR bstrInputFile = _bstr_t(argv[1]);
    BSTR bstrOutputFile = NULL;
    if (argc == 3)
    {
        bstrOutputFile = _bstr_t(argv[2]);
    }

    ConvertToSarifHelperFromFile(bstrInputFile, bstrOutputFile, NULL);
}
#endif

extern "C"
{
    __declspec(dllexport) BSTR __stdcall ConvertToSarif(BSTR bstrFilePath)
    {
        BSTR bstrResult;
        ConvertToSarifHelperFromFile(bstrFilePath, NULL, &bstrResult);
        return bstrResult;
    }

    __declspec(dllexport) BSTR __stdcall ConvertToSarifFromXml(BSTR bstrXmlText)
    {
        BSTR bstrResult;
        ConvertToSarifHelperFromText(bstrXmlText, &bstrResult);
        return bstrResult;
    }
}

HRESULT __stdcall Convert(const std::deque<XmlDefect> defectList, BSTR bstrOutputFile, BSTR* pbstrSarifText)
{
    SarifLog issueLog;
    issueLog.SetVersion(L"1.0.0-beta.5");
    issueLog.SetSchema(L"http://json.schemastore.org/sarif-1.0.0-beta.5");

    // Set Tool
    SarifTool tool;
    tool.SetName(L"PREfast");
    tool.SetFullName(L"PREfast Code Analysis");
    tool.SetVersion(L"14.0.0");

    // Set Run
    SarifRun run;
    run.SetTool(tool);

    for (const XmlDefect &defect : defectList)
    {
        SarifRegion region;
        region.SetStartColumn(defect.m_sfa.GetColumnNo());
        region.SetStartLine(defect.m_sfa.GetLineNo());

        std::wstring uriResultFile = GetDefectUri(defect.m_sfa);
        SarifPhysicalLocation resultFile;
        SarifLocation location;

        resultFile.SetURI(uriResultFile);
        if (region.IsValid())
        {
            resultFile.SetRegion(region);
        }
        location.SetResultFile(resultFile);

        location.SetFullyQualifiedLogicalName(defect.GetFunction());

        SarifLogicalLocation logicalLocation;
        logicalLocation.AddLogicalLocationComponent(defect.GetFunction(), L"method");

        location.SetLogicalLocationKey(defect.GetDecorated());
        run.AddLogicalLocation(defect.GetDecorated(), logicalLocation);

        location.AddProperty(L"decorated", defect.GetDecorated());
        location.AddProperty(L"funcline", defect.GetFunctionLine());

        // Result
        SarifResult result;
        result.SetRuleId(defect.GetDefectCode());
        result.SetMessage(defect.GetDescription());
        result.AddLocation(location);

        if (wcslen(defect.GetProbability()) > 0)
            result.AddProperty(L"probability", defect.GetProbability());

        if (wcslen(defect.GetRank()) > 0)
        {
            result.AddProperty(L"rank", defect.GetRank());
        }

        if (defect.m_category.size() > 0)
        {
            for (auto mit : defect.m_category)
            {
                std::wstring key;
                GetXmlToSarifMapping(std::wstring(mit.first), key);
                result.AddProperty(key, std::wstring(mit.second));
            }
        }

        if (defect.m_additionalInfo.size() > 0)
        {
            for (auto mit : defect.m_additionalInfo)
            {
                std::wstring key;
                GetXmlToSarifMapping(std::wstring(mit.first), key);
                result.AddProperty(key, std::wstring(mit.second));
            }
        }

        if (defect.m_path.size() > 0)
        {
            SarifCodeFlow codeFlow;

            for (const XmlSfa &sfa : defect.m_path)
            {
                SarifPhysicalLocation fileLocation;

                fileLocation.SetURI(GetDefectUri(sfa));

                SarifRegion region;
                region.SetStartColumn(sfa.GetColumnNo());
                region.SetStartLine(sfa.GetLineNo());
                if (region.IsValid())
                {
                    fileLocation.SetRegion(region);
                }

                SarifAnnotatedCodeLocation annotation;
                annotation.SetPhysicalLocation(fileLocation);

                const XmlKeyEvent &keyEvent = sfa.GetKeyEvent();

                if (keyEvent.IsValid())
                {
                    annotation.AddProperty(L"id", keyEvent.GetId());
                    annotation.AddProperty(L"kind", keyEvent.GetKind());
                    annotation.AddProperty(L"importance", keyEvent.GetImportance());

                    annotation.SetMessage(keyEvent.GetMessage());
                }
                codeFlow.AddAnnotatedCodeLocation(annotation);
            }
            result.AddCodeFlow(codeFlow);
        }
        run.AddResult(result);
    }
    issueLog.AddRun(run);

    std::wstring out = json::Serialize(issueLog.m_values);

    if (pbstrSarifText != NULL)
    {
        *pbstrSarifText = SysAllocString(out.c_str());
    }
    else if (bstrOutputFile != NULL)
    {
        std::wstring filepath = bstrOutputFile;
        std::wofstream ofs(filepath);
        if (ofs.is_open())
        {
            ofs.write(out.c_str(), out.size());
            if (ofs.fail())
            {
                ofs.close();
                return E_FAIL;
            }
            ofs.close();
        }
    }
    else
    {
        wprintf_s(L"%s", out.c_str());
    }
    return S_OK;
}

HRESULT __stdcall ConvertToSarifHelperFromText(BSTR bstrXmlText, BSTR* pbstrSarifText)
{
    std::wstring xmlText = bstrXmlText;
    std::deque<XmlDefect> defectList;
    XmlToSarifConverter::LoadXmlDefectFromString(xmlText, defectList);

    return Convert(defectList, NULL, pbstrSarifText);
}

HRESULT __stdcall ConvertToSarifHelperFromFile(BSTR bstrInputFile, BSTR bstrOutputFile, BSTR* pbstrSarifText)
{
    std::wstring filename = bstrInputFile;
    std::deque<XmlDefect> defectList;
    XmlToSarifConverter::LoadXmlDefects(filename, defectList);

    return Convert(defectList, bstrOutputFile, pbstrSarifText);
}