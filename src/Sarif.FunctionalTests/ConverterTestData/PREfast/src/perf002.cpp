/// ----------------------------------------------------------------------------------------/
/// 
/// Include most of the standard C++ headers.
/// A small set of test to verify STL container/algorithm, stream, regex, thread just work.
///
/// ----------------------------------------------------------------------------------------/

#include <algorithm>
#include <allocators>
#include <array>
#include <atomic>
#include <bitset>
#include <cassert>
#include <ccomplex>
#include <cctype>
#include <cerrno>
#include <cfenv>
#include <cfloat>
#include <chrono>
#include <cinttypes>
#include <ciso646>
#include <climits>
#include <clocale>
#include <cmath>
#include <codecvt>
#include <complex>
#include <condition_variable>
#include <csetjmp>
#include <csignal>
#include <cstdarg>
#include <cstdbool>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctgmath>
#include <ctime>
#include <cuchar>
#include <cwchar>
#include <cwctype>
#include <deque>
#include <exception>
#include <filesystem>
#include <forward_list>
#include <fstream>
#include <functional>
#include <future>
#include <initializer_list>
#include <iomanip>
#include <ios>
#include <iosfwd>
#include <iostream>
#include <istream>
#include <iterator>
#include <limits>
#include <list>
#include <locale>
#include <map>
#include <memory>
#include <mutex>
#include <new>
#include <numeric>
#include <ostream>
#include <queue>
#include <random>
#include <ratio>
#include <regex>
#include <scoped_allocator>
#include <set>
#include <sstream>
#include <stack>
#include <stdexcept>
#include <streambuf>
#include <string>
#include <strstream>
#include <system_error>
#include <thread>
#include <tuple>
#include <type_traits>
#include <typeindex>
#include <typeinfo>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <valarray>
#include <vector>
using namespace std;


// Basic container
// IO Stream
// typeinfo
template<class T> void test_container_iostream()
{
	T cont;
	cont.push_back(0);
	cont.push_back(1);
	cout << typeid(cont).name() 
		<< " " 
		<< *cont.begin() 
		<< endl;
}


// Associative Container
// string
void test_map_string()
{
	map<string, int> _mp;
	_mp.insert(pair<string, int>("abc", 0));
}

// array
// Iterator
// Algorithm
void test_array_algorithm()
{
	array<int, 3> ar;
	for_each(ar.begin(), ar.end(), [](const int &i)
	{
		cout << i << ',';
	});

	cout << endl;
}

// Regex
// String
void test_regex()
{
	string str = "Hello world";
	regex rx("ello");
	cout << "regex:" << regex_search(str.begin(), str.end(), rx) << endl;
}

// <filesystem>
void test_filesystem()
{
	::std::tr2::sys::path p("Abc");
}

void test_thread()
{
	mutex m;

	{
		lock_guard<mutex> lck(m);
		cout << "In critical section" <<endl;
	}

	thread t1([]()
	{
		cout << "Thread 1" <<endl;
	});

	thread t2([]()
	{
		cout << "Thread 2" <<endl;
	});

	t1.join();
	t2.join();
}

void test_filestream()
{
	fstream f("hello.txt");
	f<<"hello.txt"<<endl;
}

void test_exception()
{
	try
	{
		throw runtime_error("Error");
	}
	catch(exception& ex)
	{
		cout << "Exception message: " << ex.what() << endl;
	}
}

int main()
{
	test_container_iostream<vector<int>>();
	test_container_iostream<list<int>>();
	test_map_string();
	test_array_algorithm();
	test_regex();
	test_filesystem();
	test_thread();
	test_filestream();
	test_exception();

	return 0;
}
