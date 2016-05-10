#include <specstrings.h>

typedef __success(return >= 0) long HRESULT;

class Class
{
public:
    HRESULT MemberFunc();
    HRESULT OtherMemberFunc() { return 0; }
	Class(): buffer(nullptr), bufferSize(0) {}
    __field_bcount_opt(bufferSize) __nullterminated unsigned short * buffer;
    size_t bufferSize;
};

bool SomeFunc() { return true; }

HRESULT Class::MemberFunc()
{
    HRESULT hr = 0;

    while (0 == hr)
    {
        hr = OtherMemberFunc();

        if (0 == hr)
        {
            if (SomeFunc())
            {
                hr = OtherMemberFunc();
            }
        }
    }

    return hr;
}
