// Temporarily define these here until we pick up specstrings.h which
// defines these with the definitive definition.
#ifdef _Satisfies_
#error Remove this now that _Satisfies_ is officially defined
#endif

#if (_MSC_VER >= 1000) && !defined(__midl) && defined(_PREFAST_) // [

#define _Satisfies_(x) _Post_satisfies_(x)


#else // ][

#define _Satisfies_(x)

#endif // ]
