
#include "PatriciaSet.h"
#include "PatriciaMap.h"
#include <iostream>
#include <set>

using namespace Esp::Utility;

typedef PSet<UIntSet>       UIntSetSet;
typedef PSet<UIntSetSet>    UIntSetSetSet;
typedef PSet<UIntSetSetSet> UIntSetSetSetSet;

typedef PSet<IntSet>        IntSetSet;
typedef PSet<IntSetSet>     IntSetSetSet;
typedef PSet<IntSetSetSet>  IntSetSetSetSet;

void main()
{
#define TEST_SET
#ifdef TEST_SET
#ifdef TEST_UINT
    UIntSet x;
    UIntSet y = x.insert(1023);
	UIntSet y1 = y.insert(342);
	UIntSet z = y1.insert(23432);
    UIntSet w = z.remove(342).insert(56).insert(1000);
    UIntSet u = w.remove(56).insert(342).remove(200).remove(1000).remove(23432);
    UIntSet v = w.insert(56).insert(57).insert(58).insert(1022).insert(1024);

    UIntSetSet xx = UIntSetSet().insert(x).insert(y).insert(w).remove(UIntSet().insert(1023).insert(56).insert(1000).insert(23432));
    UIntSetSetSet ufo = UIntSetSetSet(xx).insert(UIntSetSet(z).insert(w)).insert(UIntSetSet(w).insert(z));
    UIntSetSetSetSet ufo1 = UIntSetSetSetSet(ufo).insert(ufo).insert(UIntSetSetSet(xx));
#else
    IntSet x;
    IntSet y = x.insert(1023);
	IntSet y1 = y.insert(-342);
	IntSet z = y1.insert(-23432);
    IntSet w = z.remove(342).insert(56).insert(-1000);
    IntSet u = w.remove(56).insert(342).remove(-200).remove(1000).remove(23432);
    IntSet v = w.insert(56).insert(57).insert(-58).insert(1022).insert(1024);

    IntSetSet xx = IntSetSet().insert(x).insert(y).insert(w).remove(IntSet().insert(1023).insert(56).insert(1000).insert(23432));
    IntSetSetSet ufo = IntSetSetSet(xx).insert(IntSetSet(z).insert(w)).insert(IntSetSet(w).insert(z));
    IntSetSetSetSet ufo1 = IntSetSetSetSet(ufo).insert(ufo).insert(IntSetSetSet(xx));
#endif
    IntSet y_y1, y1_y;
    std::cout << "disjoint(y, y1) = " << IntSet::diff(y, y1, y_y1, y1_y);
    std::cout << ", y - y1 = " << y_y1 << ", y1 - y = " << y1_y << std::endl;
    std::cout << ", y + y1 = " << y + y1 << std::endl;

    IntSet u_v, v_u;
    std::cout << "disjoint(u, v) = " << IntSet::diff(u, v, u_v, v_u);
    std::cout << ", u - v = " << u_v << ", v - u = " << v_u << std::endl;
    std::cout << "u + v = " << u + v << std::endl;
#else
    // test map
    typedef PMap<int, int> Int2Int;
    typedef PSet<Int2Int>  Int2Int_Set;
    typedef PMap<Int2Int, Int2Int> Int2Int_2_Int2Int;
    typedef PMap<int, PSet<int> > Int2ManyInts;

    Int2Int x = Int2Int(56, 78).insert(-56, -34);
    Int2Int y = Int2Int().insert(56, 90).insert(57, 100).
        insert(100000, 23423).insert(0, 3);
    Int2Int y1 = y.remove(0).insert(56, 78).remove(57);
    Int2Int z = x.insert(100000, 23423).remove(-56);
    Int2Int_Set w = Int2Int_Set().insert(y).insert(y1).insert(x).insert(z);
    Int2Int_2_Int2Int u = Int2Int_2_Int2Int().insert(x,y).insert(z,y1)
        .insert(y1, x);
    Int2ManyInts v = Int2ManyInts().insert(2, PSet<int>(5).insert(6)).
        insert(3, PSet<int>(1000).insert(11432));
#endif
    /*
	IntSet y1 = y.insert(-342);
	IntSet z = y1.insert(-23432);
    IntSet w = z.remove(342).insert(56).insert(-1000);
    IntSet u = w.remove(56).insert(342).remove(-200).remove(1000).remove(23432);
    IntSet v = w.insert(56).insert(57).insert(-58).insert(1022).insert(1024);

    IntSetSet xx = IntSetSet().insert(x).insert(y).insert(w).remove(IntSet().insert(1023).insert(56).insert(1000).insert(23432));
    IntSetSetSet ufo = IntSetSetSet(xx).insert(IntSetSet(z).insert(w)).insert(IntSetSet(w).insert(z));
    IntSetSetSetSet ufo1 = IntSetSetSetSet(ufo).insert(ufo).insert(IntSetSetSet(xx));

    */
    std::cout << "x = " << x << "\n"; 
   
	std::cout << "y = " << y << "\n";

    std::cout << "y1 = " << y1 << "\n";

    std::cout << "z = " << z << "\n";

    std::cout << "w = " << w << "\n";

    std::cout << "u = " << u << "\n";

    std::cout << "v = " << v << "\n";

#ifndef TEST_SET
    try 
    {
        
        std::cout << "v(2) = " << v(2) << "\n";
        
        std::cout << "v(3) = " << v(3) << "\n";
        
        std::cout << "v(0) = " << v(0) << "\n";
    }
    catch (int& x) 
    {
        std::cout << "exception # " << x << "\n";
    }
#endif
    /*
      std::cout << "xx = " << xx << "\n";

      std::cout << "ufo = " << ufo << "\n";
    
      std::cout << "ufo1 = " << ufo1 << "\n";
    */
}
