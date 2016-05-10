#include <specstrings.h>
#pragma prefast(push)
#pragma prefast(disable: 26002 26003 26006 26007 26035 26036, "We are not interested in bugs in stdexcept")
#include <map>
#pragma prefast(pop)
#include "undefsal.h"

// Without a fix to the DFS implementation in loop header analysis in cfg.cpp,
// this file caused EspX to stack overflow in both x86fre and amd64fre builds.

typedef _Null_terminated_ const wchar_t* LPCWSTR;

typedef struct uls_perf_counter_info
{
    int iObjId;
    LPCWSTR wzCounterSetName;
    LPCWSTR wzCounterName;
} ULSPERF_COUNTER_INFO;

typedef std::map<int, ULSPERF_COUNTER_INFO> PERFMAP;
typedef std::pair<int, ULSPERF_COUNTER_INFO> PERF_PAIR;

PERFMAP g_perfIdMap;

void BigCfg()
{
  g_perfIdMap.clear();
  {
    ULSPERF_COUNTER_INFO o = {0, L"Office Web Apps - Word Web App and PowerPoint Web App viewing", 0}; if ((g_perfIdMap.insert(PERF_PAIR(186221745,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {0, o.wzCounterSetName, L"Active Viewing Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(328966139,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1, o.wzCounterSetName, L"Active Viewing Data Calls"}; if ((g_perfIdMap.insert(PERF_PAIR(234028250,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {2, o.wzCounterSetName, L"Viewing Data Calls - Item Not Found"}; if ((g_perfIdMap.insert(PERF_PAIR(149803816,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {3, o.wzCounterSetName, L"Viewing Data Calls - Conversion Is In Progress"}; if ((g_perfIdMap.insert(PERF_PAIR(1758932159,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {4, o.wzCounterSetName, L"Viewing Failed Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(789609125,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {5, o.wzCounterSetName, L"Viewing Data Calls - Service Busy"}; if ((g_perfIdMap.insert(PERF_PAIR(170074687,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {6, o.wzCounterSetName, L"Viewing Dispatched Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(610667749,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {7, o.wzCounterSetName, L"Viewing Rejected Requests - Service Busy"}; if ((g_perfIdMap.insert(PERF_PAIR(652407317,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {8, o.wzCounterSetName, L"Viewing Queued Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(1855523635,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {9, o.wzCounterSetName, L"Viewing Handled Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(986102689,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {10, o.wzCounterSetName, L"Viewing Conversions In Progress"}; if ((g_perfIdMap.insert(PERF_PAIR(1835863000,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {11, o.wzCounterSetName, L"Viewing Converted Documents"}; if ((g_perfIdMap.insert(PERF_PAIR(26167966,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {12, o.wzCounterSetName, L"Viewing Timed Out Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(1879388915,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {13, o.wzCounterSetName, L"Word Documents to PNG Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(227762213,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {14, o.wzCounterSetName, L"Word Documents to Silverlight Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(537466721,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {15, o.wzCounterSetName, L"Powerpoint Slideshow to PNG conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(1821091265,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {16, o.wzCounterSetName, L"PowerPoint Conversions to Reading View"}; if ((g_perfIdMap.insert(PERF_PAIR(866662167,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {17, o.wzCounterSetName, L"PowerPoint Conversions to Extra Small SlideShow Format"}; if ((g_perfIdMap.insert(PERF_PAIR(1809978309,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {18, o.wzCounterSetName, L"PowerPoint Conversions to Extra Large SlideShow Format"}; if ((g_perfIdMap.insert(PERF_PAIR(1876491979,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {19, o.wzCounterSetName, L"PowerPoint Conversions to Large Static Images Format"}; if ((g_perfIdMap.insert(PERF_PAIR(408647035,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {20, o.wzCounterSetName, L"PowerPoint Conversions to Static View"}; if ((g_perfIdMap.insert(PERF_PAIR(590655579,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {21, o.wzCounterSetName, L"Documents to MobileImage Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(1115441878,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {22, o.wzCounterSetName, L"Documents to Accessible Pdf Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(1201969549,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {23, o.wzCounterSetName, L"Documents to Auto Print Pdf Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(197474913,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {24, o.wzCounterSetName, L"Documents to Docx Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(328337631,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {25, o.wzCounterSetName, L"Documents to Pptx Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(1597304099,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {26, o.wzCounterSetName, L"Documents to WordEdit Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(2007270122,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {27, o.wzCounterSetName, L"Documents to Other Formats Conversions"}; if ((g_perfIdMap.insert(PERF_PAIR(386603174,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {28, o.wzCounterSetName, L"Word PNG Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(945829924,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {29, o.wzCounterSetName, L"Word Silverlight Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(1142547553,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {30, o.wzCounterSetName, L"PowerPoint Slideshow View Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(2108833598,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {31, o.wzCounterSetName, L"PowerPoint Reading View Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(961866919,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {32, o.wzCounterSetName, L"PowerPoint Extra Small View Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(858102085,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {33, o.wzCounterSetName, L"PowerPoint Extra Large View Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(88942377,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {34, o.wzCounterSetName, L"PowerPoint Large Static View Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(157604279,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {35, o.wzCounterSetName, L"PowerPoint Static View Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(1640250718,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {36, o.wzCounterSetName, L"Word MobileImage Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(879608276,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {37, o.wzCounterSetName, L"Word AccessiblePdf Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(403342746,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {38, o.wzCounterSetName, L"Word AutoPrintPdf Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(153124793,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {39, o.wzCounterSetName, L"Word Docx Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(568666702,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {40, o.wzCounterSetName, L"PowerPoint Pptx Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(2140817900,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {41, o.wzCounterSetName, L"OtherFormats Office Web Apps Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(850333561,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {42, o.wzCounterSetName, L"Word PNG Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(1763779598,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {43, o.wzCounterSetName, L"Word Silverlight Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(2113646049,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {44, o.wzCounterSetName, L"PowerPoint Slideshow View Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(947533530,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {45, o.wzCounterSetName, L"PowerPoint Reading View Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(325846014,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {46, o.wzCounterSetName, L"PowerPoint Extra Small View Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(176272833,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {47, o.wzCounterSetName, L"PowerPoint Extra Large View Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(515333202,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {48, o.wzCounterSetName, L"PowerPoint Large Static View Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(2023551332,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {49, o.wzCounterSetName, L"PowerPoint Static View Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(864483869,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {50, o.wzCounterSetName, L"Word MobileImage Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(65153516,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {51, o.wzCounterSetName, L"Word AccessiblePdf Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(956600843,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {52, o.wzCounterSetName, L"Word AutoPrintPdf Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(306852733,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {53, o.wzCounterSetName, L"Word Docx Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(1856783163,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {54, o.wzCounterSetName, L"PowerPoint Pptx Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(1983158671,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {55, o.wzCounterSetName, L"OtherFormats Backend Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(914181459,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {56, o.wzCounterSetName, L"Word Png Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(305474800,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {57, o.wzCounterSetName, L"Word Silverlight Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(1760246621,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {58, o.wzCounterSetName, L"PowerPoint Slideshow View Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(1803015678,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {59, o.wzCounterSetName, L"PowerPoint Reading View Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(1852181575,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {60, o.wzCounterSetName, L"PowerPoint Extra Small View Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(882641512,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {61, o.wzCounterSetName, L"PowerPoint Extra Large View Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(122526911,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {62, o.wzCounterSetName, L"PowerPoint Large Static View Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(1148313241,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {63, o.wzCounterSetName, L"PowerPoint Static View Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(2404647,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {64, o.wzCounterSetName, L"Word MobileImage Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(926397452,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65, o.wzCounterSetName, L"Word AccessiblePdf Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(1662369121,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {66, o.wzCounterSetName, L"Word AutoPrintPdf Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(1538575978,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {67, o.wzCounterSetName, L"Word Docx Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(835762718,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {68, o.wzCounterSetName, L"PowerPoint Pptx Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(434899766,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {69, o.wzCounterSetName, L"OtherFormats Not Cached"}; if ((g_perfIdMap.insert(PERF_PAIR(820612715,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {70, o.wzCounterSetName, L"Render Time: Word to Png - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(552307405,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {71, o.wzCounterSetName, L"ConversionCompletedTimeWordDocumentToPngBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1671005588,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {72, o.wzCounterSetName, L"Render Time: Word to Silverlight - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(399789614,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {73, o.wzCounterSetName, L"ConversionCompletedTimeWordDocumentToSilverlightBase"}; if ((g_perfIdMap.insert(PERF_PAIR(667033616,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {74, o.wzCounterSetName, L"Render Time: PowerPoint Slideshow View - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(1354528996,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {75, o.wzCounterSetName, L"ConversionCompletedTimePowerpointSlideshowToPngBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1853441294,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {76, o.wzCounterSetName, L"Render Time: PowerPoint Reading View - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(286195503,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {77, o.wzCounterSetName, L"ConversionCompletedTimePowerpointReadingViewToPngBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1222893998,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {78, o.wzCounterSetName, L"Render Time: PowerPoint Extra Small View - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(173594150,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {79, o.wzCounterSetName, L"ConversionCompletedTimePowerpointExtraSmallBase"}; if ((g_perfIdMap.insert(PERF_PAIR(320984547,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {80, o.wzCounterSetName, L"Render Time: PowerPoint Extra Large View - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(241318696,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {81, o.wzCounterSetName, L"ConversionCompletedTimePowerpointExtraLargeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1267132121,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {82, o.wzCounterSetName, L"Render Time: PowerPoint Large StaticView conversion - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(1566660362,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {83, o.wzCounterSetName, L"ConversionCompletedTimePowerpointStaticLargeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1957821561,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {84, o.wzCounterSetName, L"Render Time: PowerPoint StaticView conversion - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(1116632506,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {85, o.wzCounterSetName, L"ConversionCompletedTimePowerPointStaticViewBase"}; if ((g_perfIdMap.insert(PERF_PAIR(567267190,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {86, o.wzCounterSetName, L"Render Time: Word to MobileImage - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(304519280,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {87, o.wzCounterSetName, L"ConversionCompletedTimeWordMobileImageBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1454986492,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {88, o.wzCounterSetName, L"Render Time: Word to AccessiblePdf - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(1792832357,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {89, o.wzCounterSetName, L"ConversionCompletedTimeWordAccessiblePdfBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1717414305,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {90, o.wzCounterSetName, L"Render Time: Word to AutoPrintPdf - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(1360510289,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {91, o.wzCounterSetName, L"ConversionCompletedTimeWordAutoPrintPdfBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2014421875,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {92, o.wzCounterSetName, L"Render Time: Word to Docx - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(896590551,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {93, o.wzCounterSetName, L"ConversionCompletedTimeWordDocxBase"}; if ((g_perfIdMap.insert(PERF_PAIR(648292802,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {94, o.wzCounterSetName, L"Render Time: PowerPoint to Pptx - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(714819676,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {95, o.wzCounterSetName, L"ConversionCompletedTimePowerPointPptxBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1899296778,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {96, o.wzCounterSetName, L"Render Time: WordEdit - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(1363568866,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {97, o.wzCounterSetName, L"ConversionCompletedTimeWordEditBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1509574932,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {98, o.wzCounterSetName, L"Render Time: Word to OtherFormats - Complete"}; if ((g_perfIdMap.insert(PERF_PAIR(2107151052,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {99, o.wzCounterSetName, L"ConversionCompletedTimeOtherFormatsBase"}; if ((g_perfIdMap.insert(PERF_PAIR(230107080,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {100, o.wzCounterSetName, L"Render Time: Word to Png - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(538874264,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {101, o.wzCounterSetName, L"ConversionResponseReadyTimeWordDocumentToPngBase"}; if ((g_perfIdMap.insert(PERF_PAIR(47503659,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {102, o.wzCounterSetName, L"Render Time: Word to Silverlight - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1269174557,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {103, o.wzCounterSetName, L"ConversionResponseReadyTimeWordDocumentToSilverlightBase"}; if ((g_perfIdMap.insert(PERF_PAIR(429190890,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {104, o.wzCounterSetName, L"Render Time: PowerPoint Slideshow View - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(685771504,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {105, o.wzCounterSetName, L"ConversionResponseReadyTimePowerpointSlideshowToPngBase"}; if ((g_perfIdMap.insert(PERF_PAIR(524751762,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {106, o.wzCounterSetName, L"Render Time: PowerPoint Reading View - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1666289218,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {107, o.wzCounterSetName, L"ConversionResponseReadyTimePowerpointReadingViewToPngBase"}; if ((g_perfIdMap.insert(PERF_PAIR(506296511,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {108, o.wzCounterSetName, L"Render Time: PowerPoint Extra Small View - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1362729823,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {109, o.wzCounterSetName, L"ConversionResponseReadyTimePowerpointExtraSmallBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1803397097,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {110, o.wzCounterSetName, L"Render Time: PowerPoint Extra Large View - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1426290257,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {111, o.wzCounterSetName, L"ConversionResponseReadyTimePowerpointExtraLargeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(869962963,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {112, o.wzCounterSetName, L"Render Time: PowerPoint Large Static View Conversion- Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1009519539,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {113, o.wzCounterSetName, L"ConversionResponseReadyTimePowerpointStaticLargeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(683909964,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {114, o.wzCounterSetName, L"Render Time: PowerPoint Static View Conversion - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(435108033,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {115, o.wzCounterSetName, L"ConversionResponseReadyTimePowerPointStaticViewBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1502854526,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {116, o.wzCounterSetName, L"Render Time: Word to MobileImage - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(2055497509,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {117, o.wzCounterSetName, L"ConversionResponseReadyTimeWordMobileImageBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1306670521,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {118, o.wzCounterSetName, L"Render Time: Word to AccessiblePdf - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1781098560,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {119, o.wzCounterSetName, L"ConversionResponseReadyTimeWordAccessiblePdfBase"}; if ((g_perfIdMap.insert(PERF_PAIR(118551832,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {120, o.wzCounterSetName, L"Render Time: Word to AutoPrintPdf - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(202852460,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {121, o.wzCounterSetName, L"ConversionResponseReadyTimeWordAutoPrintPdfBase"}; if ((g_perfIdMap.insert(PERF_PAIR(594603532,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {122, o.wzCounterSetName, L"Render Time: Word to Docx - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(748496152,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {123, o.wzCounterSetName, L"ConversionResponseReadyTimeWordDocxBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1863345275,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {124, o.wzCounterSetName, L"Render Time: PowerPoint to Pptx - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1728845168,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {125, o.wzCounterSetName, L"ConversionResponseReadyTimePowerPointPptxBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2077252854,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {126, o.wzCounterSetName, L"Render Time: WordEdit - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(1219211053,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {127, o.wzCounterSetName, L"ConversionResponseReadyTimeWordEditBase"}; if ((g_perfIdMap.insert(PERF_PAIR(273548459,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {128, o.wzCounterSetName, L"Render Time: Word to OtherFormats - Incremental Response"}; if ((g_perfIdMap.insert(PERF_PAIR(875356021,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {129, o.wzCounterSetName, L"ConversionResponseReadyTimeOtherFormatsBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1354224379,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {130, o.wzCounterSetName, L"Average Wait Time on Backends"}; if ((g_perfIdMap.insert(PERF_PAIR(1098204061,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131, o.wzCounterSetName, L"ConversionRequestWaitTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1224075567,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {132, o.wzCounterSetName, L"Average Time to Download Document on Backends"}; if ((g_perfIdMap.insert(PERF_PAIR(1823806674,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {133, o.wzCounterSetName, L"ConversionRequestDownloadTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1080577420,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {134, o.wzCounterSetName, L"Average Time to Upload Rendering to Main Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(2046024966,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {135, o.wzCounterSetName, L"ConversionRequestCacheUploadTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1959553447,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {136, o.wzCounterSetName, L"Viewer Service Failures"}; if ((g_perfIdMap.insert(PERF_PAIR(1613960161,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {137, o.wzCounterSetName, L"Word Web App Viewer Session Count with PNG Backend Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(1967806958,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {138, o.wzCounterSetName, L"Word Web App Viewer Session Count with PNG Cached Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(587868368,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {139, o.wzCounterSetName, L"Total Download Size for PNG Word Web App Viewer"}; if ((g_perfIdMap.insert(PERF_PAIR(302176698,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {140, o.wzCounterSetName, L"Word Web App Viewer Session Count with Silverlight Backend Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(720401885,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {141, o.wzCounterSetName, L"Word Web App Viewer Session Count with Silverlight Cached Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(869672013,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {142, o.wzCounterSetName, L"Total Download Size for Silverlight Word Web App Viewer"}; if ((g_perfIdMap.insert(PERF_PAIR(1949378347,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {143, o.wzCounterSetName, L"Word Web App Viewer Session Count with WordMobileImage Backend Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(1807102524,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {144, o.wzCounterSetName, L"Word Web App Viewer Session Count with WordMobileImage Cached Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(1922065326,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {145, o.wzCounterSetName, L"Total Download Size for WordMobileImage Word Web App Viewer"}; if ((g_perfIdMap.insert(PERF_PAIR(897016010,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {146, o.wzCounterSetName, L"Word Web App Viewer Session Count with AccessiblePdf Backend Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(1063180011,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {147, o.wzCounterSetName, L"Word Web App Viewer Session Count with AccessiblePdf Cached Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(1856313460,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {148, o.wzCounterSetName, L"Total Download Size for AccessiblePdf Word Web App Viewer"}; if ((g_perfIdMap.insert(PERF_PAIR(1838088196,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {149, o.wzCounterSetName, L"Word Web App Viewer Session Count with AutoPrintPdf Backend Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(1044529066,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {150, o.wzCounterSetName, L"Word Web App Viewer Session Count with AutoPrintPdf Cached Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(403948564,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {151, o.wzCounterSetName, L"Total Download Size for AutoPrintPdf Word Web App Viewer"}; if ((g_perfIdMap.insert(PERF_PAIR(15667708,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {152, o.wzCounterSetName, L"Word Web App Viewer Session Count with Other Formats Backend Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(319370512,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {153, o.wzCounterSetName, L"Word Web App Viewer Session Count with Other Formats Cached Rendering"}; if ((g_perfIdMap.insert(PERF_PAIR(895012526,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {154, o.wzCounterSetName, L"Total Download Size for Other Formats Word Web App Viewer"}; if ((g_perfIdMap.insert(PERF_PAIR(765729606,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {155, o.wzCounterSetName, L"PowerPoint Broadcast Total Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(763696455,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {156, o.wzCounterSetName, L"PowerPoint Broadcast Total Viewers"}; if ((g_perfIdMap.insert(PERF_PAIR(663355155,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {157, o.wzCounterSetName, L"PowerPoint Broadcast Active Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(41726956,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {158, o.wzCounterSetName, L"PowerPoint Broadcast PutData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(736368585,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {159, o.wzCounterSetName, L"PowerPoint Broadcast GetData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(522160958,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {160, o.wzCounterSetName, L"Viewing Web Server Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(1970790410,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {161, o.wzCounterSetName, L"Viewing Web Server Cache Miss and Subsequent Write"}; if ((g_perfIdMap.insert(PERF_PAIR(539749994,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {162, o.wzCounterSetName, L"Count of Viewing Web Server Cache Items"}; if ((g_perfIdMap.insert(PERF_PAIR(869555012,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {163, o.wzCounterSetName, L"Viewing Web Server Cache Trim"}; if ((g_perfIdMap.insert(PERF_PAIR(2078440801,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {164, o.wzCounterSetName, L"Viewing Web Server Cache Item Lifetime"}; if ((g_perfIdMap.insert(PERF_PAIR(1703484366,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {165, o.wzCounterSetName, L"ConversionRequestFrontEndCacheCacheItemLifetimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1883605075,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {166, o.wzCounterSetName, L"Broadcast Page Loads Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1551459013,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {167, o.wzCounterSetName, L"Broadcast Page Refreshes Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(640387428,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {168, o.wzCounterSetName, L"Office Mobile Broadcast Page Loads Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(664772730,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {169, o.wzCounterSetName, L"Mobile WAC Broadcast Page Loads Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(938430642,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {170, o.wzCounterSetName, L"Broadcast State Cache Hits Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(317208363,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {171, o.wzCounterSetName, L"Viewing Data Calls - Item found in the cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1705795254,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {172, o.wzCounterSetName, L"Count of Document Properties in the Web Server Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1824250268,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {173, o.wzCounterSetName, L"Rate of Failed Document Properties Writes to the Web Server Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1512620285,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {174, o.wzCounterSetName, L"Count of Viewing Web Server Document Info Cache Items"}; if ((g_perfIdMap.insert(PERF_PAIR(1968245245,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {175, o.wzCounterSetName, L"Viewing Web Server Document Info Cache Trim"}; if ((g_perfIdMap.insert(PERF_PAIR(1677591794,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {176, o.wzCounterSetName, L"Average Time Spent in InsertClipart"}; if ((g_perfIdMap.insert(PERF_PAIR(1372724835,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {177, o.wzCounterSetName, L"PowerPointWeb_InsertClipartTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(100740157,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {178, o.wzCounterSetName, L"InsertClipart Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1454019936,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {179, o.wzCounterSetName, L"OneNote Service Api requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(383987185,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {180, o.wzCounterSetName, L"OneNoteApi_RequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1801405068,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {181, o.wzCounterSetName, L"OneNote Service Api GetNotebookHierarchy requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(728532758,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {182, o.wzCounterSetName, L"OneNoteApi_GetNotebookHierarchyTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1101815991,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {183, o.wzCounterSetName, L"OneNote Service Api GetSectionGroupHierarchy requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1831788713,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {184, o.wzCounterSetName, L"OneNoteApi_GetSectionGroupHierarchyTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1225748013,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {185, o.wzCounterSetName, L"OneNote Service Api GetSectionHierarchy requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(19136322,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {186, o.wzCounterSetName, L"OneNoteApi_GetSectionHierarchyTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1435967132,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {187, o.wzCounterSetName, L"OneNote Service Api GetSectionContents requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1923208102,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {188, o.wzCounterSetName, L"OneNoteApi_GetSectionContentsTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(576418084,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {189, o.wzCounterSetName, L"OneNote Service Api GetPageContents requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(890334259,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {190, o.wzCounterSetName, L"OneNoteApi_GetPageContentsTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(410671326,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {191, o.wzCounterSetName, L"OneNote Service Api GetBinaryObject requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1822022164,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {192, o.wzCounterSetName, L"OneNoteApi_GetBinaryObjectTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1379330090,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {193, o.wzCounterSetName, L"OneNote Service Api CreateSection requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1266535382,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {194, o.wzCounterSetName, L"OneNoteApi_CreateSectionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1809777021,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {195, o.wzCounterSetName, L"OneNote Service Api CreatePage requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(913916698,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196, o.wzCounterSetName, L"OneNoteApi_CreatePageTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(768500712,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {197, o.wzCounterSetName, L"OneNote Service Api UpdatePageContents requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1315923787,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {198, o.wzCounterSetName, L"OneNoteApi_UpdatePageContentsTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1932837791,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {199, o.wzCounterSetName, L"OneNote Service Api DeleteSection requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(344786071,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {200, o.wzCounterSetName, L"OneNoteApi_DeleteSectionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(234178785,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {201, o.wzCounterSetName, L"OneNote Service Api DeletePage requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1955875318,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {202, o.wzCounterSetName, L"OneNoteApi_DeletePageTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(587135005,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {203, o.wzCounterSetName, L"OneNote Service Api Info requests per second"}; if ((g_perfIdMap.insert(PERF_PAIR(1117068153,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {204, o.wzCounterSetName, L"OneNoteApi_InfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(797848932,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {205, o.wzCounterSetName, L"OneNote Service Api request count"}; if ((g_perfIdMap.insert(PERF_PAIR(937367532,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {206, o.wzCounterSetName, L"OneNote Service Api GetNotebookHierarchy request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1200231785,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {207, o.wzCounterSetName, L"OneNote Service Api GetSectionGroupHierarchy request count"}; if ((g_perfIdMap.insert(PERF_PAIR(99476688,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {208, o.wzCounterSetName, L"OneNote Service Api GetSectionHierarchy request count"}; if ((g_perfIdMap.insert(PERF_PAIR(150098095,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {209, o.wzCounterSetName, L"OneNote Service Api GetSectionContents request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1965103339,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {210, o.wzCounterSetName, L"OneNote Service Api GetPageContents request count"}; if ((g_perfIdMap.insert(PERF_PAIR(979451395,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {211, o.wzCounterSetName, L"OneNote Service Api GetBinaryObject request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1294509697,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {212, o.wzCounterSetName, L"OneNote Service Api CreateSection request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1455876937,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {213, o.wzCounterSetName, L"OneNote Service Api CreatePage request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1380848884,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {214, o.wzCounterSetName, L"OneNote Service Api UpdatePageContents request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1147802305,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {215, o.wzCounterSetName, L"OneNote Service Api DeleteSection request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1509322930,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {216, o.wzCounterSetName, L"OneNote Service Api DeletePage request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1086016265,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {217, o.wzCounterSetName, L"OneNote Service Api Info request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1105844475,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {218, o.wzCounterSetName, L"Placeholder Counter 59"}; if ((g_perfIdMap.insert(PERF_PAIR(1179362063,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {219, o.wzCounterSetName, L"Placeholder Counter 60"}; if ((g_perfIdMap.insert(PERF_PAIR(1922628464,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {220, o.wzCounterSetName, L"Placeholder Counter 61"}; if ((g_perfIdMap.insert(PERF_PAIR(2042275270,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {221, o.wzCounterSetName, L"Placeholder Counter 62"}; if ((g_perfIdMap.insert(PERF_PAIR(1692157500,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {222, o.wzCounterSetName, L"Placeholder Counter 63"}; if ((g_perfIdMap.insert(PERF_PAIR(1878923410,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {223, o.wzCounterSetName, L"Placeholder Counter 64"}; if ((g_perfIdMap.insert(PERF_PAIR(1578205640,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {224, o.wzCounterSetName, L"Placeholder Counter 65"}; if ((g_perfIdMap.insert(PERF_PAIR(1429462894,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {225, o.wzCounterSetName, L"Placeholder Counter 66"}; if ((g_perfIdMap.insert(PERF_PAIR(1213545620,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {226, o.wzCounterSetName, L"Placeholder Counter 67"}; if ((g_perfIdMap.insert(PERF_PAIR(1131897402,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {227, o.wzCounterSetName, L"Placeholder Counter 68"}; if ((g_perfIdMap.insert(PERF_PAIR(730467904,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {228, o.wzCounterSetName, L"Placeholder Counter 69"}; if ((g_perfIdMap.insert(PERF_PAIR(547901590,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {229, o.wzCounterSetName, L"Placeholder Counter 70"}; if ((g_perfIdMap.insert(PERF_PAIR(1355175399,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {230, o.wzCounterSetName, L"Placeholder Counter 71"}; if ((g_perfIdMap.insert(PERF_PAIR(1541671757,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {231, o.wzCounterSetName, L"Placeholder Counter 72"}; if ((g_perfIdMap.insert(PERF_PAIR(1182984371,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {232, o.wzCounterSetName, L"Placeholder Counter 73"}; if ((g_perfIdMap.insert(PERF_PAIR(1302378009,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {233, o.wzCounterSetName, L"Placeholder Counter 74"}; if ((g_perfIdMap.insert(PERF_PAIR(2085564239,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {234, o.wzCounterSetName, L"Placeholder Counter 75"}; if ((g_perfIdMap.insert(PERF_PAIR(2003661285,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {235, o.wzCounterSetName, L"Placeholder Counter 76"}; if ((g_perfIdMap.insert(PERF_PAIR(1779143195,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {236, o.wzCounterSetName, L"Placeholder Counter 77"}; if ((g_perfIdMap.insert(PERF_PAIR(1630129329,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {237, o.wzCounterSetName, L"Placeholder Counter 78"}; if ((g_perfIdMap.insert(PERF_PAIR(164929719,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {238, o.wzCounterSetName, L"Placeholder Counter 79"}; if ((g_perfIdMap.insert(PERF_PAIR(49741341,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {239, o.wzCounterSetName, L"Placeholder Counter 80"}; if ((g_perfIdMap.insert(PERF_PAIR(1381340993,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {240, o.wzCounterSetName, L"Placeholder Counter 81"}; if ((g_perfIdMap.insert(PERF_PAIR(1501022699,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {241, o.wzCounterSetName, L"Placeholder Counter 82"}; if ((g_perfIdMap.insert(PERF_PAIR(1141975573,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {242, o.wzCounterSetName, L"Placeholder Counter 83"}; if ((g_perfIdMap.insert(PERF_PAIR(1328772287,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {243, o.wzCounterSetName, L"Placeholder Counter 84"}; if ((g_perfIdMap.insert(PERF_PAIR(2128408041,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {244, o.wzCounterSetName, L"Placeholder Counter 85"}; if ((g_perfIdMap.insert(PERF_PAIR(1979626307,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {245, o.wzCounterSetName, L"Placeholder Counter 86"}; if ((g_perfIdMap.insert(PERF_PAIR(1754812605,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {246, o.wzCounterSetName, L"Placeholder Counter 87"}; if ((g_perfIdMap.insert(PERF_PAIR(1673137687,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {247, o.wzCounterSetName, L"Placeholder Counter 88"}; if ((g_perfIdMap.insert(PERF_PAIR(189227537,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {248, o.wzCounterSetName, L"Placeholder Counter 89"}; if ((g_perfIdMap.insert(PERF_PAIR(6634683,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {249, o.wzCounterSetName, L"Placeholder Counter 90"}; if ((g_perfIdMap.insert(PERF_PAIR(1879783880,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {250, o.wzCounterSetName, L"Placeholder Counter 91"}; if ((g_perfIdMap.insert(PERF_PAIR(2066311010,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {251, o.wzCounterSetName, L"Placeholder Counter 92"}; if ((g_perfIdMap.insert(PERF_PAIR(1716487324,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {252, o.wzCounterSetName, L"Placeholder Counter 93"}; if ((g_perfIdMap.insert(PERF_PAIR(1835915830,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {253, o.wzCounterSetName, L"Placeholder Counter 94"}; if ((g_perfIdMap.insert(PERF_PAIR(1552040800,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {254, o.wzCounterSetName, L"Placeholder Counter 95"}; if ((g_perfIdMap.insert(PERF_PAIR(1470111178,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {255, o.wzCounterSetName, L"Placeholder Counter 96"}; if ((g_perfIdMap.insert(PERF_PAIR(1254555188,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {256, o.wzCounterSetName, L"Placeholder Counter 97"}; if ((g_perfIdMap.insert(PERF_PAIR(1105502366,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {257, o.wzCounterSetName, L"Placeholder Counter 98"}; if ((g_perfIdMap.insert(PERF_PAIR(689556632,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {258, o.wzCounterSetName, L"Placeholder Counter 99"}; if ((g_perfIdMap.insert(PERF_PAIR(574329394,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {259, o.wzCounterSetName, L"Placeholder Counter 100"}; if ((g_perfIdMap.insert(PERF_PAIR(1261752097,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {1, L"Office Web Apps - PowerPoint Web App", 0}; if ((g_perfIdMap.insert(PERF_PAIR(245379272,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {65536, o.wzCounterSetName, L"Average Time Spent Rendering PowerPointFrame.aspx"}; if ((g_perfIdMap.insert(PERF_PAIR(871793148,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65537, o.wzCounterSetName, L"PowerPointWeb_Frame_RenderTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1862675293,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65538, o.wzCounterSetName, L"PowerPointFrame.aspx Reading View Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(591932422,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65539, o.wzCounterSetName, L"PowerPointFrame.aspx Editing View Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(636725812,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65540, o.wzCounterSetName, L"PowerPointFrame.aspx Slideshow View Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1277778580,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65541, o.wzCounterSetName, L"PowerPointFrame.aspx Attendee View Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(142568345,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65542, o.wzCounterSetName, L"Average Time Spent in GetPresentationInfoXml"}; if ((g_perfIdMap.insert(PERF_PAIR(2070609974,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65543, o.wzCounterSetName, L"PowerPointWeb_GetPresentationInfoXmlTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1924501695,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65544, o.wzCounterSetName, L"GetPresentationInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2081655605,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65545, o.wzCounterSetName, L"Average Time Spent in GetSlideInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(567885781,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65546, o.wzCounterSetName, L"PowerPointWeb_GetSlideInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(761460452,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65547, o.wzCounterSetName, L"GetSlideInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(648131800,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65548, o.wzCounterSetName, L"Average Time Spent in GetNotesHtmlServiceResult"}; if ((g_perfIdMap.insert(PERF_PAIR(326416638,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65549, o.wzCounterSetName, L"PowerPointWeb_GetNotesHtmlServiceResultTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(641542073,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65550, o.wzCounterSetName, L"GetNotesHtmlServiceResult Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(336344063,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65551, o.wzCounterSetName, L"Average Time Spent in GetEditPresInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(1571466379,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65552, o.wzCounterSetName, L"PowerPointWeb_GetEditPresInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(203387478,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65553, o.wzCounterSetName, L"GetEditPresInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1523722120,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65554, o.wzCounterSetName, L"Average Time Spent in GetEditSlide"}; if ((g_perfIdMap.insert(PERF_PAIR(1308418584,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65555, o.wzCounterSetName, L"PowerPointWeb_GetEditSlideTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(555726190,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65556, o.wzCounterSetName, L"GetEditSlide Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1250191637,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65557, o.wzCounterSetName, L"Average Time Spent in MoveSlide"}; if ((g_perfIdMap.insert(PERF_PAIR(1225070710,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65558, o.wzCounterSetName, L"PowerPointWeb_MoveSlideTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(67109554,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65559, o.wzCounterSetName, L"MoveSlide Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1316852599,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65560, o.wzCounterSetName, L"Average Time Spent in MoveShape"}; if ((g_perfIdMap.insert(PERF_PAIR(1146659556,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65561, o.wzCounterSetName, L"PowerPointWeb_MoveShapeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1226303853,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65562, o.wzCounterSetName, L"MoveShape Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1126181345,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65563, o.wzCounterSetName, L"Average Time Spent in ResizeShape"}; if ((g_perfIdMap.insert(PERF_PAIR(1776928576,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65564, o.wzCounterSetName, L"PowerPointWeb_ResizeShapeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1978359625,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65565, o.wzCounterSetName, L"ResizeShape Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1855015997,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65566, o.wzCounterSetName, L"Average Time Spent in InsertShape"}; if ((g_perfIdMap.insert(PERF_PAIR(50785577,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65567, o.wzCounterSetName, L"PowerPointWeb_InsertShapeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(477072324,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65568, o.wzCounterSetName, L"InsertShape Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(75392556,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65569, o.wzCounterSetName, L"Average Time Spent in UngroupShape"}; if ((g_perfIdMap.insert(PERF_PAIR(500699533,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65570, o.wzCounterSetName, L"PowerPointWeb_UngroupShapeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(102848732,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65571, o.wzCounterSetName, L"UngroupShape Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(446732944,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65572, o.wzCounterSetName, L"ArrangeShape Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1365321404,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65573, o.wzCounterSetName, L"Average Time Spent in ArrangeShape"}; if ((g_perfIdMap.insert(PERF_PAIR(1444523449,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65574, o.wzCounterSetName, L"PowerPointWeb_ArrangeShapeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(864092835,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65575, o.wzCounterSetName, L"Average Time Spent in DuplicateShape"}; if ((g_perfIdMap.insert(PERF_PAIR(1825057261,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65576, o.wzCounterSetName, L"PowerPointWeb_DuplicateShapeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1942695855,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65577, o.wzCounterSetName, L"DuplicateShape Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1806738158,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65578, o.wzCounterSetName, L"ApplyShapeFill Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(335975955,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65579, o.wzCounterSetName, L"Average Time Spent in ApplyShapeFill"}; if ((g_perfIdMap.insert(PERF_PAIR(327032082,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65580, o.wzCounterSetName, L"PowerPointWeb_ApplyShapeFillTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(576404189,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65581, o.wzCounterSetName, L"RemoveShapeFill Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(194123027,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65582, o.wzCounterSetName, L"Average Time Spent in RemoveShapeFill"}; if ((g_perfIdMap.insert(PERF_PAIR(216702480,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65583, o.wzCounterSetName, L"PowerPointWeb_RemoveShapeFillTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1195282208,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65584, o.wzCounterSetName, L"ApplyShapeOutlineColor Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1168247809,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65585, o.wzCounterSetName, L"Average Time Spent in ApplyShapeOutlineColor"}; if ((g_perfIdMap.insert(PERF_PAIR(1121617668,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65586, o.wzCounterSetName, L"PowerPointWeb_ApplyShapeOutlineColorTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(35753412,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65587, o.wzCounterSetName, L"ApplyShapeStyle Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1120383258,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65588, o.wzCounterSetName, L"Average Time Spent in ApplyShapeStyle"}; if ((g_perfIdMap.insert(PERF_PAIR(1170160155,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65589, o.wzCounterSetName, L"PowerPointWeb_ApplyShapeStyleTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1935505430,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65590, o.wzCounterSetName, L"ApplyShapeOutlineWidth Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(926966347,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65591, o.wzCounterSetName, L"Average Time Spent in ApplyShapeOutlineWidth"}; if ((g_perfIdMap.insert(PERF_PAIR(809036106,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65592, o.wzCounterSetName, L"PowerPointWeb_ApplyShapeOutlineWidthTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(454621913,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65593, o.wzCounterSetName, L"ApplyShapeOutlineDashStyle Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(76650866,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65594, o.wzCounterSetName, L"Average Time Spent in ApplyShapeOutlineDashStyle"}; if ((g_perfIdMap.insert(PERF_PAIR(65606259,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65595, o.wzCounterSetName, L"PowerPointWeb_ApplyShapeOutlineDashStyleTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1939852246,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65596, o.wzCounterSetName, L"ApplyShapeOutlineEndStyle Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2068011701,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65597, o.wzCounterSetName, L"Average Time Spent in ApplyShapeOutlineEndStyle"}; if ((g_perfIdMap.insert(PERF_PAIR(2084295096,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65598, o.wzCounterSetName, L"PowerPointWeb_ApplyShapeOutlineEndStyleTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(519300407,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65599, o.wzCounterSetName, L"RemoveShapeOutline Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1761515569,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65600, o.wzCounterSetName, L"Average Time Spent in RemoveShapeOutline"}; if ((g_perfIdMap.insert(PERF_PAIR(1871057716,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65601, o.wzCounterSetName, L"PowerPointWeb_RemoveShapeOutlineTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1349888642,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65602, o.wzCounterSetName, L"Average Time Spent in InsertSlide"}; if ((g_perfIdMap.insert(PERF_PAIR(240867263,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65603, o.wzCounterSetName, L"PowerPointWeb_InsertSlideTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1366843421,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65604, o.wzCounterSetName, L"InsertSlide Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(153345214,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65605, o.wzCounterSetName, L"Average Time Spent in DeleteAllComments"}; if ((g_perfIdMap.insert(PERF_PAIR(1471318419,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65606, o.wzCounterSetName, L"PowerPointWeb_DeleteAllCommentsTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(629190811,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65607, o.wzCounterSetName, L"DeleteAllComments Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1355416210,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65608, o.wzCounterSetName, L"Average Time Spent in ReplaceText"}; if ((g_perfIdMap.insert(PERF_PAIR(566445393,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65609, o.wzCounterSetName, L"PowerPointWeb_ReplaceTextTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(595880225,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65610, o.wzCounterSetName, L"ReplaceText Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(649842258,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65611, o.wzCounterSetName, L"Average Time Spent in ClearPlaceholder"}; if ((g_perfIdMap.insert(PERF_PAIR(1107459423,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65612, o.wzCounterSetName, L"PowerPointWeb_ClearPlaceholderTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1905188249,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65613, o.wzCounterSetName, L"ClearPlaceholder Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1165686366,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65614, o.wzCounterSetName, L"Average Time Spent in DeleteSlide"}; if ((g_perfIdMap.insert(PERF_PAIR(1729403117,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65615, o.wzCounterSetName, L"PowerPointWeb_DeleteSlideTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1946742253,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65616, o.wzCounterSetName, L"DeleteSlide Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1617695726,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65617, o.wzCounterSetName, L"Average Time Spent in DuplicateSlide"}; if ((g_perfIdMap.insert(PERF_PAIR(1637192571,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65618, o.wzCounterSetName, L"PowerPointWeb_DuplicateSlideTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1054655602,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65619, o.wzCounterSetName, L"DuplicateSlide Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1726814328,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65620, o.wzCounterSetName, L"Average Time Spent in ShowHideSlide"}; if ((g_perfIdMap.insert(PERF_PAIR(196843408,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65621, o.wzCounterSetName, L"PowerPointWeb_ShowHideSlideTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(732184318,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65622, o.wzCounterSetName, L"ShowHideSlide Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(214113421,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65623, o.wzCounterSetName, L"Average Time Spent in ReplaceNotes"}; if ((g_perfIdMap.insert(PERF_PAIR(1438508016,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65624, o.wzCounterSetName, L"PowerPointWeb_ReplaceNotesTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2106557839,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65625, o.wzCounterSetName, L"ReplaceNotes Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1388669165,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65626, o.wzCounterSetName, L"Average Time Spent in Undo"}; if ((g_perfIdMap.insert(PERF_PAIR(251221480,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65627, o.wzCounterSetName, L"PowerPointWeb_UndoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(306649852,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65628, o.wzCounterSetName, L"Undo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(159506149,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65629, o.wzCounterSetName, L"Average Time Spent in Redo"}; if ((g_perfIdMap.insert(PERF_PAIR(701560452,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65630, o.wzCounterSetName, L"PowerPointWeb_RedoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1355563402,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65631, o.wzCounterSetName, L"Redo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(782794113,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65632, o.wzCounterSetName, L"Average Time Spent in ChangePictureStyle"}; if ((g_perfIdMap.insert(PERF_PAIR(1895526905,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65633, o.wzCounterSetName, L"PowerPointWeb_ChangePictureStyleTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1494116490,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65634, o.wzCounterSetName, L"ChangePictureStyle Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2005138170,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65635, o.wzCounterSetName, L"Average Time Spent in SaveAndClose"}; if ((g_perfIdMap.insert(PERF_PAIR(636511208,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65636, o.wzCounterSetName, L"PowerPointWeb_SaveAndCloseTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2123162277,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65637, o.wzCounterSetName, L"SaveAndClose Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(579397861,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65638, o.wzCounterSetName, L"Average Time Spent in ChangeSmartArtLayout"}; if ((g_perfIdMap.insert(PERF_PAIR(183634697,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65639, o.wzCounterSetName, L"PowerPointWeb_ChangeSmartArtLayoutTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1427512630,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65640, o.wzCounterSetName, L"ChangeSmartArtLayout Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(227180554,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65641, o.wzCounterSetName, L"Average Time Spent in ChangeSmartArtColor"}; if ((g_perfIdMap.insert(PERF_PAIR(1445408204,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65642, o.wzCounterSetName, L"PowerPointWeb_ChangeSmartArtColorTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(710719364,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65643, o.wzCounterSetName, L"ChangeSmartArtColor Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1365223113,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65644, o.wzCounterSetName, L"Average Time Spent in ChangeSmartArtStyle"}; if ((g_perfIdMap.insert(PERF_PAIR(1862023662,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65645, o.wzCounterSetName, L"PowerPointWeb_ChangeSmartArtStyleTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(812506865,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65646, o.wzCounterSetName, L"ChangeSmartArtStyle Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1770304237,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65647, o.wzCounterSetName, L"Average Time Spent in InsertSmartArt"}; if ((g_perfIdMap.insert(PERF_PAIR(553459537,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65648, o.wzCounterSetName, L"PowerPointWeb_InsertSmartArtTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1792311519,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65649, o.wzCounterSetName, L"InsertSmartArt Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(663065682,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65650, o.wzCounterSetName, L"Average Time Spent in ResetSmartArt"}; if ((g_perfIdMap.insert(PERF_PAIR(1928394029,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65651, o.wzCounterSetName, L"PowerPointWeb_ResetSmartArtTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(64102443,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65652, o.wzCounterSetName, L"ResetSmartArt Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1971879470,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65653, o.wzCounterSetName, L"Average Time Spent in ReverseSmartArt"}; if ((g_perfIdMap.insert(PERF_PAIR(1911726599,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65654, o.wzCounterSetName, L"PowerPointWeb_ReverseSmartArtTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(206671685,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65655, o.wzCounterSetName, L"ReverseSmartArt Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1988765958,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65656, o.wzCounterSetName, L"BroadcastStartSession Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1982945989,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65657, o.wzCounterSetName, L"Average Time Spent in BroadcastStartSession"}; if ((g_perfIdMap.insert(PERF_PAIR(1900663240,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65658, o.wzCounterSetName, L"PowerPointWeb_BroadcastStartSessionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1081987754,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65659, o.wzCounterSetName, L"BroadcastEndSession Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1945727218,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65660, o.wzCounterSetName, L"Average Time Spent in BroadcastEndSession"}; if ((g_perfIdMap.insert(PERF_PAIR(1954675699,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65661, o.wzCounterSetName, L"PowerPointWeb_BroadcastEndSessionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(710150391,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65662, o.wzCounterSetName, L"BroadcastJoinSession Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1482507461,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65663, o.wzCounterSetName, L"Average Time Spent in BroadcastJoinSession"}; if ((g_perfIdMap.insert(PERF_PAIR(1596313544,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65664, o.wzCounterSetName, L"PowerPointWeb_BroadcastJoinSessionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(60142507,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65665, o.wzCounterSetName, L"BroadcastPutData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1718296535,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65666, o.wzCounterSetName, L"Average Time Spent in BroadcastPutData"}; if ((g_perfIdMap.insert(PERF_PAIR(1628673236,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65667, o.wzCounterSetName, L"PowerPointWeb_BroadcastPutDataTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1790823717,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65668, o.wzCounterSetName, L"BroadcastGetData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(213697481,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65669, o.wzCounterSetName, L"Average Time Spent in BroadcastGetData"}; if ((g_perfIdMap.insert(PERF_PAIR(197413066,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65670, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetDataTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(422686329,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65671, o.wzCounterSetName, L"BroadcastGetHostInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1572035240,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65672, o.wzCounterSetName, L"Average Time Spent in BroadcastGetHostInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(1523306923,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65673, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetHostInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2141445447,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65674, o.wzCounterSetName, L"BroadcastGetProtocolVersion Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(235159917,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65675, o.wzCounterSetName, L"Average Time Spent in BroadcastGetProtocolVersion"}; if ((g_perfIdMap.insert(PERF_PAIR(159173230,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65676, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetProtocolVersionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(177156108,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65677, o.wzCounterSetName, L"BroadcastGetNewUploadFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1506502945,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65678, o.wzCounterSetName, L"Average Time Spent in BroadcastGetNewUploadFile"}; if ((g_perfIdMap.insert(PERF_PAIR(1588847140,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65679, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetNewUploadFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(233483337,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65680, o.wzCounterSetName, L"BroadcastGetHostToken Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1202602390,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65681, o.wzCounterSetName, L"Average Time Spent in BroadcastGetHostToken"}; if ((g_perfIdMap.insert(PERF_PAIR(1087818389,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65682, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetHostTokenTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1539916969,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65683, o.wzCounterSetName, L"BroadcastGetAttendeeUrl Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1849722741,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65684, o.wzCounterSetName, L"Average Time Spent in BroadcastGetAttendeeUrl"}; if ((g_perfIdMap.insert(PERF_PAIR(1765280888,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65685, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetAttendeeUrlTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(59554033,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65686, o.wzCounterSetName, L"BroadcastGetStateServiceUrl Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(212424792,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65687, o.wzCounterSetName, L"Average Time Spent in BroadcastGetStateServiceUrl"}; if ((g_perfIdMap.insert(PERF_PAIR(198300507,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65688, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetStateServiceUrlTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(294324904,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65689, o.wzCounterSetName, L"BroadcastDeleteUploadFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1296566690,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65690, o.wzCounterSetName, L"Average Time Spent in BroadcastDeleteUploadFile"}; if ((g_perfIdMap.insert(PERF_PAIR(1244693155,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65691, o.wzCounterSetName, L"PowerPointWeb_BroadcastDeleteUploadFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(396746272,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65692, o.wzCounterSetName, L"BroadcastGetServerInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1624139694,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65693, o.wzCounterSetName, L"Average Time Spent in BroadcastGetServerInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(1740041391,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65694, o.wzCounterSetName, L"PowerPointWeb_BroadcastGetServerInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(922785674,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65695, o.wzCounterSetName, L"ImageHandler Requests Per Second for Reading View"}; if ((g_perfIdMap.insert(PERF_PAIR(252108164,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65696, o.wzCounterSetName, L"ImageHandler Requests Per Second for Slideshow View"}; if ((g_perfIdMap.insert(PERF_PAIR(1109414827,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65697, o.wzCounterSetName, L"ImageHandler Requests Per Second for Attendee View"}; if ((g_perfIdMap.insert(PERF_PAIR(1183751462,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65698, o.wzCounterSetName, L"ImageHandler Requests Per Second for Editing View"}; if ((g_perfIdMap.insert(PERF_PAIR(163404722,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65699, o.wzCounterSetName, L"PreviewHandler Requests Per Second "}; if ((g_perfIdMap.insert(PERF_PAIR(1192286266,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65700, o.wzCounterSetName, L"MediaHandler Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(891053127,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65701, o.wzCounterSetName, L"Average Time Spent in ImageHandler"}; if ((g_perfIdMap.insert(PERF_PAIR(669585593,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65702, o.wzCounterSetName, L"PowerPointWeb_ImageHandlerTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(798942223,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65703, o.wzCounterSetName, L"Average Time Spent in PreviewHandler"}; if ((g_perfIdMap.insert(PERF_PAIR(1080578875,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65704, o.wzCounterSetName, L"PowerPointWeb_PreviewHandlerTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(362519468,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65705, o.wzCounterSetName, L"Average Time Spent in MediaHandler"}; if ((g_perfIdMap.insert(PERF_PAIR(845475652,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65706, o.wzCounterSetName, L"PowerPointWeb_MediaHandlerTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1676616523,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65707, o.wzCounterSetName, L"PptInsertPicture Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(808622794,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65708, o.wzCounterSetName, L"Average Time Spent in PptInsertPicture"}; if ((g_perfIdMap.insert(PERF_PAIR(927601099,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65709, o.wzCounterSetName, L"PowerPointWeb_PptInsertPictureTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1676707161,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65710, o.wzCounterSetName, L"DatacenterBroadcastUploadHandler Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(979833314,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65711, o.wzCounterSetName, L"Average Time Spent in DatacenterBroadcastUploadHandler"}; if ((g_perfIdMap.insert(PERF_PAIR(1025481443,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65712, o.wzCounterSetName, L"PowerPointWeb_DatacenterBroadcastUploadHandlerTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1719705213,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65713, o.wzCounterSetName, L"CheckFileInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(218939563,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65714, o.wzCounterSetName, L"Average Time Spent in CheckFileInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(175393704,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65715, o.wzCounterSetName, L"PowerPointWeb_WSHI_CheckFileInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(23044089,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65716, o.wzCounterSetName, L"GetFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(891105818,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65717, o.wzCounterSetName, L"Average Time Spent in GetFile"}; if ((g_perfIdMap.insert(PERF_PAIR(845527323,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65718, o.wzCounterSetName, L"PowerPointWeb_WSHI_GetFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1667426999,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65719, o.wzCounterSetName, L"Error Responses Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(243786140,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65720, o.wzCounterSetName, L"Average Time an Edit Session is Active"}; if ((g_perfIdMap.insert(PERF_PAIR(2011126969,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65721, o.wzCounterSetName, L"PowerPointEdit_SessionLifeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(54588738,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65722, o.wzCounterSetName, L"Count of Active Edit Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(1962032862,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65723, o.wzCounterSetName, L"Edit Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1270271583,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65724, o.wzCounterSetName, L"Average Time to Process an Edit Request"}; if ((g_perfIdMap.insert(PERF_PAIR(1288586590,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65725, o.wzCounterSetName, L"PowerPointEdit_EditTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1877952628,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65726, o.wzCounterSetName, L"Non Edit Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2001087839,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65727, o.wzCounterSetName, L"Average Time to Process a Non Edit Request Such as Image Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(1883154014,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65728, o.wzCounterSetName, L"PowerPointEdit_NonEditTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(656992516,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65729, o.wzCounterSetName, L"Count of Queued Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(312821680,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65730, o.wzCounterSetName, L"Count of Saves to be Uploaded to Storage"}; if ((g_perfIdMap.insert(PERF_PAIR(130436696,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65731, o.wzCounterSetName, L"Average Time to Download Document on Backends"}; if ((g_perfIdMap.insert(PERF_PAIR(488876398,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65732, o.wzCounterSetName, L"PowerPointEdit_DownloadTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1639307733,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65733, o.wzCounterSetName, L"Rate That a Request Gets Routed to the Wrong Server"}; if ((g_perfIdMap.insert(PERF_PAIR(666520352,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65734, o.wzCounterSetName, L"Rate That Requests Are Rerouted Due to the Server Being Busy"}; if ((g_perfIdMap.insert(PERF_PAIR(93965222,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65735, o.wzCounterSetName, L"Rate That a Request Gets Routed to a Failed Server"}; if ((g_perfIdMap.insert(PERF_PAIR(1179062108,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65736, o.wzCounterSetName, L"Rate That a Request Cannot be Processed Due to no Available Servers"}; if ((g_perfIdMap.insert(PERF_PAIR(1528774942,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65737, o.wzCounterSetName, L"Count of Broadcast Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(69407868,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65738, o.wzCounterSetName, L"Average Time That a Broadcast Session is Active"}; if ((g_perfIdMap.insert(PERF_PAIR(699670702,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65739, o.wzCounterSetName, L"PowerPointBroadcast_SessionLifeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1814364400,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65740, o.wzCounterSetName, L"Count of Broadcast Sessions That are Finished"}; if ((g_perfIdMap.insert(PERF_PAIR(2059298161,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65741, o.wzCounterSetName, L"Rate of New Attendees Joining Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(1626715073,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65742, o.wzCounterSetName, L"Rate That a Broadcast Request Gets Routed to a Failed Server"}; if ((g_perfIdMap.insert(PERF_PAIR(758857206,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65743, o.wzCounterSetName, L"Rate That a Broadcast Request Cannot be Processed Due to No Available Servers"}; if ((g_perfIdMap.insert(PERF_PAIR(1359767624,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65744, o.wzCounterSetName, L"Average Time Spent in BinaryConvert"}; if ((g_perfIdMap.insert(PERF_PAIR(630250649,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65745, o.wzCounterSetName, L"PowerPointWeb_BinaryConvertTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1088571833,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {65746, o.wzCounterSetName, L"BinaryConvert Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(585652122,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {2, L"OneNote Web App - Common Web Editor", 0}; if ((g_perfIdMap.insert(PERF_PAIR(153712264,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {131072, o.wzCounterSetName, L"Average Convert Time for Revision from Browser to Store Format"}; if ((g_perfIdMap.insert(PERF_PAIR(1611036148,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131073, o.wzCounterSetName, L"OneNoteWebAppBrowserToStoreTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(851973920,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131074, o.wzCounterSetName, L"Average Convert Time for Revision From Store to Browser Format"}; if ((g_perfIdMap.insert(PERF_PAIR(1328296854,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131075, o.wzCounterSetName, L"OneNoteWebAppStoreToBrowserTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(165280797,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131076, o.wzCounterSetName, L"Count of Outstanding OneNote Web App Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(130106870,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131077, o.wzCounterSetName, L"Count of Outstanding Cell Storage Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(74553061,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131078, o.wzCounterSetName, L"Average Time to Query for the Root Revisions"}; if ((g_perfIdMap.insert(PERF_PAIR(875001990,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131079, o.wzCounterSetName, L"OneNoteWebAppGetCellsTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1417649500,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131080, o.wzCounterSetName, L"Average Time to Query for a Revision"}; if ((g_perfIdMap.insert(PERF_PAIR(1361057633,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131081, o.wzCounterSetName, L"OneNoteWebAppGetChangesTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1678441059,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131082, o.wzCounterSetName, L"Average Time to Update Revision/Document with Client Changes"}; if ((g_perfIdMap.insert(PERF_PAIR(1968413461,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131083, o.wzCounterSetName, L"OneNoteWebAppPutChangesTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(226197940,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131084, o.wzCounterSetName, L"Average Time to Process OneNote Web App Request"}; if ((g_perfIdMap.insert(PERF_PAIR(32631280,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131085, o.wzCounterSetName, L"OneNoteWebAppProcessRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(577783906,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131086, o.wzCounterSetName, L"Average Time to Execute a Cell Storage Request"}; if ((g_perfIdMap.insert(PERF_PAIR(318272856,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131087, o.wzCounterSetName, L"OneNoteWebAppCellStorageRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(654161153,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131088, o.wzCounterSetName, L"Average Time to Deserialize a Server Request"}; if ((g_perfIdMap.insert(PERF_PAIR(1128620868,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131089, o.wzCounterSetName, L"OneNoteWebAppRequestDeserializationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(166741500,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131090, o.wzCounterSetName, L"Average Time to Serialize a Server Request"}; if ((g_perfIdMap.insert(PERF_PAIR(1319629717,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131091, o.wzCounterSetName, L"OneNoteWebAppRequestSerializationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(173228876,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131092, o.wzCounterSetName, L"Average Time to Process a Spelling Request"}; if ((g_perfIdMap.insert(PERF_PAIR(1694058027,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131093, o.wzCounterSetName, L"WacSpellingRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(340556775,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131094, o.wzCounterSetName, L"Average Time to Process a Editors Table Request"}; if ((g_perfIdMap.insert(PERF_PAIR(153751745,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131095, o.wzCounterSetName, L"WacEditorsTableRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(674226783,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131096, o.wzCounterSetName, L"Average Time to Process a Secondary Metadata Request"}; if ((g_perfIdMap.insert(PERF_PAIR(594062507,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131097, o.wzCounterSetName, L"WacSecondaryMetadataRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(989938945,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131098, o.wzCounterSetName, L"Average Time to Process a Make Placeholders Request"}; if ((g_perfIdMap.insert(PERF_PAIR(533484176,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131099, o.wzCounterSetName, L"WacMakePlaceholdersTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1346852887,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131100, o.wzCounterSetName, L"Average Time to Compress a OneNote Web App Request"}; if ((g_perfIdMap.insert(PERF_PAIR(370458899,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131101, o.wzCounterSetName, L"OneNoteWebAppResponseCompressionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(187453042,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131102, o.wzCounterSetName, L"Average Time to Post Merge Request to Merge Service"}; if ((g_perfIdMap.insert(PERF_PAIR(508034551,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131103, o.wzCounterSetName, L"OneNoteWebAppMergeRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1408618372,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131104, o.wzCounterSetName, L"Average Time to Execute Merge"}; if ((g_perfIdMap.insert(PERF_PAIR(1263737384,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131105, o.wzCounterSetName, L"OneNoteWebAppMergeExecuteTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2027111952,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131106, o.wzCounterSetName, L"Average Time to Execute OneNote OM Merge"}; if ((g_perfIdMap.insert(PERF_PAIR(1662765389,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131107, o.wzCounterSetName, L"OneNoteWebAppMergeCallOmTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(837022422,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131108, o.wzCounterSetName, L"Merge Queue Length"}; if ((g_perfIdMap.insert(PERF_PAIR(1918332083,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131109, o.wzCounterSetName, L"Number of failures OneNote merge failures"}; if ((g_perfIdMap.insert(PERF_PAIR(2038383122,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131110, o.wzCounterSetName, L"Number of Simple Merges"}; if ((g_perfIdMap.insert(PERF_PAIR(1882120163,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131111, o.wzCounterSetName, L"Number of OneNote Merges"}; if ((g_perfIdMap.insert(PERF_PAIR(1131639606,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131112, o.wzCounterSetName, L"Number of onenote conflict pages created without calling onenote dll"}; if ((g_perfIdMap.insert(PERF_PAIR(1142767865,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131113, o.wzCounterSetName, L"Number of Conflicting OneNote Merges"}; if ((g_perfIdMap.insert(PERF_PAIR(373074356,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131114, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Query"}; if ((g_perfIdMap.insert(PERF_PAIR(1073178095,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131115, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Set Current Revision"}; if ((g_perfIdMap.insert(PERF_PAIR(725723927,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131116, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - OneNote Web App Root Request"}; if ((g_perfIdMap.insert(PERF_PAIR(536402897,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131117, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App Root Request"}; if ((g_perfIdMap.insert(PERF_PAIR(1274192838,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131118, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - OneNote Web App Update"}; if ((g_perfIdMap.insert(PERF_PAIR(1283131059,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131119, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App Update"}; if ((g_perfIdMap.insert(PERF_PAIR(877629077,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131120, o.wzCounterSetName, L"OneNote Web App File Delete Time"}; if ((g_perfIdMap.insert(PERF_PAIR(1779441418,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131121, o.wzCounterSetName, L"OneNoteWebAppDeleteFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(501249258,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131122, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App EditorsTable"}; if ((g_perfIdMap.insert(PERF_PAIR(2002480587,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131123, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App SecondaryMetadata"}; if ((g_perfIdMap.insert(PERF_PAIR(269404359,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131124, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App AmIAlone"}; if ((g_perfIdMap.insert(PERF_PAIR(1983089650,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131125, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App UpdatesAvailable"}; if ((g_perfIdMap.insert(PERF_PAIR(1959480090,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131126, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App MakePlaceholders"}; if ((g_perfIdMap.insert(PERF_PAIR(1069603827,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131127, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App CoauthStatus"}; if ((g_perfIdMap.insert(PERF_PAIR(1262948010,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131128, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Word Web App HotStoreStatus"}; if ((g_perfIdMap.insert(PERF_PAIR(1582441249,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131129, o.wzCounterSetName, L"Average Time to Process a HotStore Status Request"}; if ((g_perfIdMap.insert(PERF_PAIR(1937071880,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {131130, o.wzCounterSetName, L"WordWebAppHotStoreStatusRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(116962373,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {3, L"Office Web Apps - Logging", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1816921337,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {196608, o.wzCounterSetName, L"Average Time to Trace"}; if ((g_perfIdMap.insert(PERF_PAIR(844576197,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196609, o.wzCounterSetName, L"WacTraceTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2105335453,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196610, o.wzCounterSetName, L"Average Time to make Trace Decision"}; if ((g_perfIdMap.insert(PERF_PAIR(1026459456,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196611, o.wzCounterSetName, L"WacShouldTraceTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2033349798,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196612, o.wzCounterSetName, L"Average Time to Start Correlated Trace"}; if ((g_perfIdMap.insert(PERF_PAIR(817319235,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196613, o.wzCounterSetName, L"WacStartTraceCorrelationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1257053865,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196614, o.wzCounterSetName, L"Average Time to End Correlated Trace"}; if ((g_perfIdMap.insert(PERF_PAIR(577309410,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196615, o.wzCounterSetName, L"WacEndTraceCorrelationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1796221457,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196616, o.wzCounterSetName, L"Average Time to Add Correlated Trace"}; if ((g_perfIdMap.insert(PERF_PAIR(471844761,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196617, o.wzCounterSetName, L"WacAddTraceCorrelationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(698809704,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196618, o.wzCounterSetName, L"Average Time to Raise Debug Assert"}; if ((g_perfIdMap.insert(PERF_PAIR(625945616,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196619, o.wzCounterSetName, L"WacDebugAssertTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(805587952,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196620, o.wzCounterSetName, L"Average Time to Raise Ship Assert"}; if ((g_perfIdMap.insert(PERF_PAIR(473077814,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196621, o.wzCounterSetName, L"WacShipAssertTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(990544068,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196622, o.wzCounterSetName, L"Average Time to Report Exception"}; if ((g_perfIdMap.insert(PERF_PAIR(2093812176,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196623, o.wzCounterSetName, L"WacReportExceptionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2117213343,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196624, o.wzCounterSetName, L"Average Time to Report Watchdog Results"}; if ((g_perfIdMap.insert(PERF_PAIR(308471543,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {196625, o.wzCounterSetName, L"WacWatchdogResultReportTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2146912823,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {4, L"Office Web Apps - Storage", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1691097043,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {262144, o.wzCounterSetName, L"Average Time to Read SharePoint Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1163727380,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262145, o.wzCounterSetName, L"WssCacheReadTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(464771802,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262146, o.wzCounterSetName, L"Average Time to Write SharePoint Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1298351556,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262147, o.wzCounterSetName, L"WssCacheWriteTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2048643963,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262148, o.wzCounterSetName, L"Average Time to Read SharePoint Storage"}; if ((g_perfIdMap.insert(PERF_PAIR(153808567,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262149, o.wzCounterSetName, L"WssStorageReadTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(675947414,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262150, o.wzCounterSetName, L"Average Time to Write SharePoint Storage"}; if ((g_perfIdMap.insert(PERF_PAIR(983543387,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262151, o.wzCounterSetName, L"WssStorageWriteTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2082307684,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262152, o.wzCounterSetName, L"Average Time to Refresh Directory Expiration on WLS"}; if ((g_perfIdMap.insert(PERF_PAIR(2060154957,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262153, o.wzCounterSetName, L"WacWLSRefreshDirectoryExpirationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1612445466,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262154, o.wzCounterSetName, L"Average Time to Save File to WLS"}; if ((g_perfIdMap.insert(PERF_PAIR(129780684,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262155, o.wzCounterSetName, L"WacWLSPutFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(325558067,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262156, o.wzCounterSetName, L"Average Time to Write Cache on WLS"}; if ((g_perfIdMap.insert(PERF_PAIR(113755369,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262157, o.wzCounterSetName, L"WacWLSCacheWriteTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1938866654,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262158, o.wzCounterSetName, L"Average Time to Get File from WLS"}; if ((g_perfIdMap.insert(PERF_PAIR(1835767766,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262159, o.wzCounterSetName, L"WacWLSGetFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1626078319,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262160, o.wzCounterSetName, L"Average Time to Read Cache on WLS"}; if ((g_perfIdMap.insert(PERF_PAIR(15288643,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262161, o.wzCounterSetName, L"WacWLSCacheReadTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2101718636,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262162, o.wzCounterSetName, L"Average Time to Create Cache Dir on WLS"}; if ((g_perfIdMap.insert(PERF_PAIR(1672475643,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262163, o.wzCounterSetName, L"WacWLSCacheDirCreationTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2077537119,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262164, o.wzCounterSetName, L"WLS Cache Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(386242723,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262165, o.wzCounterSetName, L"WLS Cache Miss"}; if ((g_perfIdMap.insert(PERF_PAIR(1637709279,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262166, o.wzCounterSetName, L"WLS Direct Read Hit"}; if ((g_perfIdMap.insert(PERF_PAIR(825377838,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262167, o.wzCounterSetName, L"WLS Direct Read Miss"}; if ((g_perfIdMap.insert(PERF_PAIR(1447135304,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262168, o.wzCounterSetName, L"WLS Reads"}; if ((g_perfIdMap.insert(PERF_PAIR(1558488614,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262169, o.wzCounterSetName, L"WLS Read Failures"}; if ((g_perfIdMap.insert(PERF_PAIR(1892192196,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262170, o.wzCounterSetName, L"WLS Writes"}; if ((g_perfIdMap.insert(PERF_PAIR(1750091556,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262171, o.wzCounterSetName, L"WLS Write Failures"}; if ((g_perfIdMap.insert(PERF_PAIR(542941901,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262172, o.wzCounterSetName, L"WLS Directory Creations"}; if ((g_perfIdMap.insert(PERF_PAIR(726949049,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262173, o.wzCounterSetName, L"WLS Directory Creation Failures"}; if ((g_perfIdMap.insert(PERF_PAIR(470519165,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262174, o.wzCounterSetName, L"WLS Expiration Updates"}; if ((g_perfIdMap.insert(PERF_PAIR(98081375,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {262175, o.wzCounterSetName, L"WLS Expiration Update Failures"}; if ((g_perfIdMap.insert(PERF_PAIR(1210683110,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {5, L"Office Web Apps - Service Hosting Interface", 0}; if ((g_perfIdMap.insert(PERF_PAIR(890393053,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {327680, o.wzCounterSetName, L"Number of times host is contacted to to check file information"}; if ((g_perfIdMap.insert(PERF_PAIR(1517745967,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327681, o.wzCounterSetName, L"Average time to check file information"}; if ((g_perfIdMap.insert(PERF_PAIR(1756451130,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327682, o.wzCounterSetName, L"WacWSHICheckFileInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(16920917,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327683, o.wzCounterSetName, L"Average time to get file"}; if ((g_perfIdMap.insert(PERF_PAIR(1143712909,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327684, o.wzCounterSetName, L"WacWSHIGetFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2083551105,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327685, o.wzCounterSetName, L"Average time to put file"}; if ((g_perfIdMap.insert(PERF_PAIR(788295827,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327686, o.wzCounterSetName, L"WacWSHIPutFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(263984349,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327687, o.wzCounterSetName, L"Average time to lock"}; if ((g_perfIdMap.insert(PERF_PAIR(741131482,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327688, o.wzCounterSetName, L"WacWSHILockTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(658904677,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327689, o.wzCounterSetName, L"Average time to unlock"}; if ((g_perfIdMap.insert(PERF_PAIR(233505427,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327690, o.wzCounterSetName, L"WacWSHIUnlockTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(389328488,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327691, o.wzCounterSetName, L"Average time to refresh lock"}; if ((g_perfIdMap.insert(PERF_PAIR(623386099,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327692, o.wzCounterSetName, L"WacWSHIRefreshLockTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(225878088,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327693, o.wzCounterSetName, L"Average time to unlock relock"}; if ((g_perfIdMap.insert(PERF_PAIR(190864258,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327694, o.wzCounterSetName, L"WacWSHIUnlockAndRelockTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1508278721,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327695, o.wzCounterSetName, L"Average time to execute cell storage request"}; if ((g_perfIdMap.insert(PERF_PAIR(1333612148,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327696, o.wzCounterSetName, L"WacWSHIExecuteCellStorageRequestTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(678767967,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327697, o.wzCounterSetName, L"Average time to execute relative cell storage request"}; if ((g_perfIdMap.insert(PERF_PAIR(1683252359,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327698, o.wzCounterSetName, L"WacWSHIExecuteCellStorageRelativeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1364426819,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327699, o.wzCounterSetName, L"Average time to enumerate siblings"}; if ((g_perfIdMap.insert(PERF_PAIR(1716964536,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327700, o.wzCounterSetName, L"WacWSHIEnumerateSiblingsTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(239765331,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327701, o.wzCounterSetName, L"Average time to enumerate folder"}; if ((g_perfIdMap.insert(PERF_PAIR(123991916,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327702, o.wzCounterSetName, L"WacWSHIEnumerateFolderTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1672328776,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327703, o.wzCounterSetName, L"Average time to create relative folder"}; if ((g_perfIdMap.insert(PERF_PAIR(322734673,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327704, o.wzCounterSetName, L"WacWSHICreateRelativeFolderTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(144387250,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327705, o.wzCounterSetName, L"Average time to delete file"}; if ((g_perfIdMap.insert(PERF_PAIR(1741823867,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {327706, o.wzCounterSetName, L"WacWSHIDeleteFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(201212550,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {6, L"Office Web Apps Browser SQM HTTP Handler", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1693987140,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {393216, o.wzCounterSetName, L"Browser CEIP data files received"}; if ((g_perfIdMap.insert(PERF_PAIR(1936856165,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {393217, o.wzCounterSetName, L"Browser CEIP data files discarded"}; if ((g_perfIdMap.insert(PERF_PAIR(844865380,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {393218, o.wzCounterSetName, L"Browser CEIP data files overwritten"}; if ((g_perfIdMap.insert(PERF_PAIR(1828381802,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {393219, o.wzCounterSetName, L"Browser CEIP data files received rate"}; if ((g_perfIdMap.insert(PERF_PAIR(64629076,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {393220, o.wzCounterSetName, L"Browser CEIP data files discarded rate"}; if ((g_perfIdMap.insert(PERF_PAIR(2092830345,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {393221, o.wzCounterSetName, L"Browser CEIP data files overwritten rate"}; if ((g_perfIdMap.insert(PERF_PAIR(1807648603,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {7, L"Office Web Apps - Proofing", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1420925608,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {458752, o.wzCounterSetName, L"OneNote.ashx ProcessRequest - Spelling"}; if ((g_perfIdMap.insert(PERF_PAIR(1775939233,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458753, o.wzCounterSetName, L"Successful Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(1008371201,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458754, o.wzCounterSetName, L"Unauthenticated Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(393998032,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458755, o.wzCounterSetName, L"Requests Over Set Character Limit"}; if ((g_perfIdMap.insert(PERF_PAIR(1540638887,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458756, o.wzCounterSetName, L"Failed Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(2061886289,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458757, o.wzCounterSetName, L"Language Recycling"}; if ((g_perfIdMap.insert(PERF_PAIR(1918317965,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458758, o.wzCounterSetName, L"Unsupported Language Requests"}; if ((g_perfIdMap.insert(PERF_PAIR(846986523,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458759, o.wzCounterSetName, L"Request Queue Length"}; if ((g_perfIdMap.insert(PERF_PAIR(149542767,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458760, o.wzCounterSetName, L"Processing Time"}; if ((g_perfIdMap.insert(PERF_PAIR(1721040845,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458761, o.wzCounterSetName, L"WacSpellingProcessingTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1934251532,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458762, o.wzCounterSetName, L"Queue Wait Time"}; if ((g_perfIdMap.insert(PERF_PAIR(237863791,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {458763, o.wzCounterSetName, L"WacSpellingQueueTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1715346093,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {8, L"Office Web Apps - Meetings", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1218768196,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {524288, o.wzCounterSetName, L"Count of Broadcast Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(121660243,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524289, o.wzCounterSetName, L"Average Time That a Broadcast Session is Active"}; if ((g_perfIdMap.insert(PERF_PAIR(125616037,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524290, o.wzCounterSetName, L"MetBroadcast_SessionLifeTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1866889183,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524291, o.wzCounterSetName, L"Count of Broadcast Sessions That are Finished"}; if ((g_perfIdMap.insert(PERF_PAIR(227584928,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524292, o.wzCounterSetName, L"Rate of New Attendees Joining Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(1469395678,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524293, o.wzCounterSetName, L"Rate That a Broadcast Request Gets Routed to a Failed Server"}; if ((g_perfIdMap.insert(PERF_PAIR(442219753,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524294, o.wzCounterSetName, L"Rate That a Broadcast Request Cannot be Processed Due to No Available Servers"}; if ((g_perfIdMap.insert(PERF_PAIR(639805081,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524295, o.wzCounterSetName, L"DatacenterBroadcastUploadHandler Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1929125432,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524296, o.wzCounterSetName, L"Average Time Spent in DatacenterBroadcastUploadHandler"}; if ((g_perfIdMap.insert(PERF_PAIR(1971623221,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524297, o.wzCounterSetName, L"MetWeb_DatacenterBroadcastUploadHandlerTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2021667499,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524298, o.wzCounterSetName, L"CheckFileInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(980173236,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524299, o.wzCounterSetName, L"Average Time Spent in CheckFileInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(1024772791,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524300, o.wzCounterSetName, L"MetWeb_WSHI_CheckFileInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1646833928,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524301, o.wzCounterSetName, L"GetFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(905834206,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524302, o.wzCounterSetName, L"Average Time Spent in GetFile"}; if ((g_perfIdMap.insert(PERF_PAIR(847603165,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524303, o.wzCounterSetName, L"MetWeb_WSHI_GetFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(179559916,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524304, o.wzCounterSetName, L"BroadcastCopyFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2115047947,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524305, o.wzCounterSetName, L"Average Time Spent in BroadcastCopyFile"}; if ((g_perfIdMap.insert(PERF_PAIR(2036963592,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524306, o.wzCounterSetName, L"MetWeb_BroadcastCopyFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(766717594,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524307, o.wzCounterSetName, L"BroadcastGetNewUploadFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1758599333,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524308, o.wzCounterSetName, L"Average Time Spent in BroadcastGetNewUploadFile"}; if ((g_perfIdMap.insert(PERF_PAIR(1873449894,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524309, o.wzCounterSetName, L"MetWeb_BroadcastGetNewUploadFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1107708343,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524310, o.wzCounterSetName, L"BroadcastGetHostToken Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1154299577,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524311, o.wzCounterSetName, L"Average Time Spent in BroadcastGetHostToken"}; if ((g_perfIdMap.insert(PERF_PAIR(1135984058,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524312, o.wzCounterSetName, L"MetWeb_BroadcastGetHostTokenTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1792029997,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524313, o.wzCounterSetName, L"BroadcastGetAttendeeUrl Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(426658214,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524314, o.wzCounterSetName, L"Average Time Spent in BroadcastGetAttendeeUrl"}; if ((g_perfIdMap.insert(PERF_PAIR(504808103,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524315, o.wzCounterSetName, L"MetWeb_BroadcastGetAttendeeUrlTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1920202573,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524316, o.wzCounterSetName, L"BroadcastDeleteUploadFile Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2085713956,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524317, o.wzCounterSetName, L"Average Time Spent in BroadcastDeleteUploadFile"}; if ((g_perfIdMap.insert(PERF_PAIR(2066346791,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524318, o.wzCounterSetName, L"MetWeb_BroadcastDeleteUploadFileTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1481194464,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524319, o.wzCounterSetName, L"BroadcastGetServerInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(62474589,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524320, o.wzCounterSetName, L"Average Time Spent in BroadcastGetServerInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(79807072,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524321, o.wzCounterSetName, L"MetWeb_BroadcastGetServerInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(779184304,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524322, o.wzCounterSetName, L"BroadcastProvisionNotes Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(2074364822,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524323, o.wzCounterSetName, L"Average Time Spent in BroadcastProvisionNotes"}; if ((g_perfIdMap.insert(PERF_PAIR(2094842007,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524324, o.wzCounterSetName, L"MetWeb_BroadcastProvisionNotesTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1947512696,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524325, o.wzCounterSetName, L"BroadcastGetData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1701704850,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524326, o.wzCounterSetName, L"Average Time Spent in BroadcastGetData"}; if ((g_perfIdMap.insert(PERF_PAIR(1645576081,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524327, o.wzCounterSetName, L"MetWeb_BroadcastGetDataTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1741501095,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524328, o.wzCounterSetName, L"BroadcastJoinSession Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(648140827,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524329, o.wzCounterSetName, L"Average Time Spent in BroadcastJoinSession"}; if ((g_perfIdMap.insert(PERF_PAIR(567890712,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524330, o.wzCounterSetName, L"MetWeb_BroadcastJoinSessionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(760960310,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524331, o.wzCounterSetName, L"BroadcastGetHostInfo Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(592430712,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524332, o.wzCounterSetName, L"Average Time Spent in BroadcastGetHostInfo"}; if ((g_perfIdMap.insert(PERF_PAIR(607604085,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524333, o.wzCounterSetName, L"MetWeb_BroadcastGetHostInfoTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1366113242,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524334, o.wzCounterSetName, L"BroadcastPutData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(263858316,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524335, o.wzCounterSetName, L"Average Time Spent in BroadcastPutData"}; if ((g_perfIdMap.insert(PERF_PAIR(146973583,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524336, o.wzCounterSetName, L"MetWeb_BroadcastPutDataTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(339825147,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524337, o.wzCounterSetName, L"BroadcastStartSession Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1968319980,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524338, o.wzCounterSetName, L"Average Time Spent in BroadcastStartSession"}; if ((g_perfIdMap.insert(PERF_PAIR(1915397865,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524339, o.wzCounterSetName, L"MetWeb_BroadcastStartSessionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1902420784,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524340, o.wzCounterSetName, L"BroadcastEndSession Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(326661037,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524341, o.wzCounterSetName, L"Average Time Spent in BroadcastEndSession"}; if ((g_perfIdMap.insert(PERF_PAIR(335607984,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524342, o.wzCounterSetName, L"MetWeb_BroadcastEndSessionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1568327206,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524343, o.wzCounterSetName, L"GetAppCapabilities Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(750564610,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524344, o.wzCounterSetName, L"Average Time Spent in GetAppCapabilities"}; if ((g_perfIdMap.insert(PERF_PAIR(734281217,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524345, o.wzCounterSetName, L"MetWeb_BroadcastGetAppCapabilitiesTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(116986797,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524346, o.wzCounterSetName, L"S4 Broadcast Total Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(249068674,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524347, o.wzCounterSetName, L"S4 Broadcast Total Viewers"}; if ((g_perfIdMap.insert(PERF_PAIR(950706806,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524348, o.wzCounterSetName, L"S4 Broadcast Active Sessions"}; if ((g_perfIdMap.insert(PERF_PAIR(1595237115,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524349, o.wzCounterSetName, L"S4 Broadcast PutData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(452814413,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524350, o.wzCounterSetName, L"S4 Broadcast GetData Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(772209336,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524351, o.wzCounterSetName, L"Broadcast State Cache Hits Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1906252252,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {524352, o.wzCounterSetName, L"Error Responses Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(296008953,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {9, L"Office Marketplace Experience Storage Agent", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1210974250,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {589824, o.wzCounterSetName, L"OmexStorageAgent service Api Put request count"}; if ((g_perfIdMap.insert(PERF_PAIR(1955187193,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {589825, o.wzCounterSetName, L"OmexStorageAgent service Api Get request count"}; if ((g_perfIdMap.insert(PERF_PAIR(75887678,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {589826, o.wzCounterSetName, L"OmexStorageAgent service Api Query request count"}; if ((g_perfIdMap.insert(PERF_PAIR(770050041,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {589827, o.wzCounterSetName, L"OmexStorageAgent service Api Delete request count"}; if ((g_perfIdMap.insert(PERF_PAIR(999840855,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {10, L"Office.com Service Core", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1117757394,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {655360, o.wzCounterSetName, L"Average Request Execution Time"}; if ((g_perfIdMap.insert(PERF_PAIR(236763738,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {655361, o.wzCounterSetName, L"ODCServiceCore_AverageRequestExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2040265815,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {655362, o.wzCounterSetName, L"Requests Per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1189220771,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {655363, o.wzCounterSetName, L"Unexpected Errors per Second"}; if ((g_perfIdMap.insert(PERF_PAIR(1391723423,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {11, L"Roaming Settings API return codes", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1919165552,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {720896, o.wzCounterSetName, L"Rate of returning ServerUnavailable from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1292750386,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720897, o.wzCounterSetName, L"Count of returning ServerUnavailable from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1581804157,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720898, o.wzCounterSetName, L"Rate of returning Success from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1318494348,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720899, o.wzCounterSetName, L"Count of returning Success from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(987310708,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720900, o.wzCounterSetName, L"Rate of returning Throttled from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1897372298,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720901, o.wzCounterSetName, L"Count of returning Throttled from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1142737577,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720902, o.wzCounterSetName, L"Rate of returning WriteFailed from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(431015257,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720903, o.wzCounterSetName, L"Count of returning WriteFailed from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(482139407,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720904, o.wzCounterSetName, L"Rate of returning UserFlaggedAsDisabled from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(28125803,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720905, o.wzCounterSetName, L"Count of returning UserFlaggedAsDisabled from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(22274183,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720906, o.wzCounterSetName, L"Rate of returning UserNotFlaggedAsDisabled from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1930919980,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720907, o.wzCounterSetName, L"Count of returning UserNotFlaggedAsDisabled from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1141247854,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720908, o.wzCounterSetName, L"Rate of returning UserNotFound from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1232554505,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720909, o.wzCounterSetName, L"Count of returning UserNotFound from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(617370561,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720910, o.wzCounterSetName, L"Rate of returning UnsupportedClient from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(680459774,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720911, o.wzCounterSetName, L"Count of returning UnsupportedClient from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(579102130,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720912, o.wzCounterSetName, L"Rate of returning UnsupportedProtocol from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1706005557,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720913, o.wzCounterSetName, L"Count of returning UnsupportedProtocol from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(65922688,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720914, o.wzCounterSetName, L"Rate of returning TypeMismatch from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(945107590,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720915, o.wzCounterSetName, L"Count of returning TypeMismatch from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(112496165,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720916, o.wzCounterSetName, L"Rate of returning MalformedRequest from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(103275310,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720917, o.wzCounterSetName, L"Count of returning MalformedRequest from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1952268207,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720918, o.wzCounterSetName, L"Rate of returning InvalidSettingId from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(268784018,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720919, o.wzCounterSetName, L"Count of returning InvalidSettingId from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1456966333,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720920, o.wzCounterSetName, L"Rate of returning SettingTooLarge from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(580599152,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720921, o.wzCounterSetName, L"Count of returning SettingTooLarge from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(927241485,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720922, o.wzCounterSetName, L"Rate of returning MustNotSpecifyContext from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(93583081,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720923, o.wzCounterSetName, L"Count of returning MustNotSpecifyContext from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1066360891,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720924, o.wzCounterSetName, L"Rate of returning MustSpecifyContext from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1157397131,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720925, o.wzCounterSetName, L"Count of returning MustSpecifyContext from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1342393807,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720926, o.wzCounterSetName, L"Rate of returning DuplicateWrites from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(901872393,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720927, o.wzCounterSetName, L"Count of returning DuplicateWrites from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1828888363,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720928, o.wzCounterSetName, L"Rate of returning ReconstructServer from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1656419220,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720929, o.wzCounterSetName, L"Count of returning ReconstructServer from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(378159821,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720930, o.wzCounterSetName, L"Rate of returning ReconstructClient from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2101699001,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720931, o.wzCounterSetName, L"Count of returning ReconstructClient from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(384277240,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720932, o.wzCounterSetName, L"Rate of returning InvalidListItemData from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2010386005,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720933, o.wzCounterSetName, L"Count of returning InvalidListItemData from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2021124533,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720934, o.wzCounterSetName, L"Rate of returning InvalidListItemKey from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1339374263,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720935, o.wzCounterSetName, L"Count of returning InvalidListItemKey from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2034464070,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720936, o.wzCounterSetName, L"Rate of returning InvalidDataValue from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1688754854,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720937, o.wzCounterSetName, L"Count of returning InvalidDataValue from the WriteSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(69433135,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720938, o.wzCounterSetName, L"Rate of returning ServerUnavailable from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(438240600,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720939, o.wzCounterSetName, L"Count of returning ServerUnavailable from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1283662336,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720940, o.wzCounterSetName, L"Rate of returning Success from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1505448737,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720941, o.wzCounterSetName, L"Count of returning Success from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(388665039,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720942, o.wzCounterSetName, L"Rate of returning Throttled from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2102184703,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720943, o.wzCounterSetName, L"Count of returning Throttled from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(525131000,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720944, o.wzCounterSetName, L"Rate of returning WriteFailed from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1235671931,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720945, o.wzCounterSetName, L"Count of returning WriteFailed from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(178436772,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720946, o.wzCounterSetName, L"Rate of returning UserFlaggedAsDisabled from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2004602323,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720947, o.wzCounterSetName, L"Count of returning UserFlaggedAsDisabled from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(676040779,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720948, o.wzCounterSetName, L"Rate of returning UserNotFlaggedAsDisabled from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(61818681,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720949, o.wzCounterSetName, L"Count of returning UserNotFlaggedAsDisabled from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(54440921,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720950, o.wzCounterSetName, L"Rate of returning UserNotFound from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1600728484,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720951, o.wzCounterSetName, L"Count of returning UserNotFound from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(984323402,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720952, o.wzCounterSetName, L"Rate of returning UnsupportedClient from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2140944020,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720953, o.wzCounterSetName, L"Count of returning UnsupportedClient from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(810495437,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720954, o.wzCounterSetName, L"Rate of returning UnsupportedProtocol from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1360534885,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720955, o.wzCounterSetName, L"Count of returning UnsupportedProtocol from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1268461641,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720956, o.wzCounterSetName, L"Rate of returning TypeMismatch from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(776688943,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720957, o.wzCounterSetName, L"Count of returning TypeMismatch from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(416499884,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720958, o.wzCounterSetName, L"Rate of returning MalformedRequest from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(410044907,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720959, o.wzCounterSetName, L"Count of returning MalformedRequest from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(592442565,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720960, o.wzCounterSetName, L"Rate of returning InvalidSettingId from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(240272215,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720961, o.wzCounterSetName, L"Count of returning InvalidSettingId from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(29704663,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720962, o.wzCounterSetName, L"Rate of returning SettingTooLarge from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(461728360,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720963, o.wzCounterSetName, L"Count of returning SettingTooLarge from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(689145802,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720964, o.wzCounterSetName, L"Rate of returning MustNotSpecifyContext from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1933906257,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720965, o.wzCounterSetName, L"Count of returning MustNotSpecifyContext from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(378999031,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720966, o.wzCounterSetName, L"Rate of returning MustSpecifyContext from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1446458104,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720967, o.wzCounterSetName, L"Count of returning MustSpecifyContext from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1689568415,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720968, o.wzCounterSetName, L"Rate of returning DuplicateWrites from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(215952385,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720969, o.wzCounterSetName, L"Count of returning DuplicateWrites from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1934986734,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720970, o.wzCounterSetName, L"Rate of returning ReconstructServer from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(900217082,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720971, o.wzCounterSetName, L"Count of returning ReconstructServer from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(71371440,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720972, o.wzCounterSetName, L"Rate of returning ReconstructClient from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(710388435,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720973, o.wzCounterSetName, L"Count of returning ReconstructClient from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(70046347,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720974, o.wzCounterSetName, L"Rate of returning InvalidListItemData from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1130596101,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720975, o.wzCounterSetName, L"Count of returning InvalidListItemData from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(805388158,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720976, o.wzCounterSetName, L"Rate of returning InvalidListItemKey from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1562276556,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720977, o.wzCounterSetName, L"Count of returning InvalidListItemKey from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1307900952,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720978, o.wzCounterSetName, L"Rate of returning InvalidDataValue from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(2063550563,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720979, o.wzCounterSetName, L"Count of returning InvalidDataValue from the ReadSettings API"}; if ((g_perfIdMap.insert(PERF_PAIR(1395768389,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720980, o.wzCounterSetName, L"Rate of returning ServerUnavailable from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1423744847,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720981, o.wzCounterSetName, L"Count of returning ServerUnavailable from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1894180774,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720982, o.wzCounterSetName, L"Rate of returning Success from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(875419261,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720983, o.wzCounterSetName, L"Count of returning Success from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(2119177338,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720984, o.wzCounterSetName, L"Rate of returning Throttled from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(131473555,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720985, o.wzCounterSetName, L"Count of returning Throttled from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(2052660069,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720986, o.wzCounterSetName, L"Rate of returning WriteFailed from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1549890634,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720987, o.wzCounterSetName, L"Count of returning WriteFailed from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(854752953,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720988, o.wzCounterSetName, L"Rate of returning UserFlaggedAsDisabled from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1351844069,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720989, o.wzCounterSetName, L"Count of returning UserFlaggedAsDisabled from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1073303322,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720990, o.wzCounterSetName, L"Rate of returning UserNotFlaggedAsDisabled from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(594457490,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720991, o.wzCounterSetName, L"Count of returning UserNotFlaggedAsDisabled from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1026945735,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720992, o.wzCounterSetName, L"Rate of returning UserNotFound from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1731848639,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720993, o.wzCounterSetName, L"Count of returning UserNotFound from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1785722555,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720994, o.wzCounterSetName, L"Rate of returning UnsupportedClient from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(828353667,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720995, o.wzCounterSetName, L"Count of returning UnsupportedClient from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(204138601,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720996, o.wzCounterSetName, L"Rate of returning UnsupportedProtocol from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(886949164,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720997, o.wzCounterSetName, L"Count of returning UnsupportedProtocol from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1908201503,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720998, o.wzCounterSetName, L"Rate of returning TypeMismatch from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(370905396,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {720999, o.wzCounterSetName, L"Count of returning TypeMismatch from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1209477983,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721000, o.wzCounterSetName, L"Rate of returning MalformedRequest from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1363020383,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721001, o.wzCounterSetName, L"Count of returning MalformedRequest from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1837919956,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721002, o.wzCounterSetName, L"Rate of returning InvalidSettingId from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1193050339,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721003, o.wzCounterSetName, L"Count of returning InvalidSettingId from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1325849538,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721004, o.wzCounterSetName, L"Rate of returning SettingTooLarge from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(2016664321,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721005, o.wzCounterSetName, L"Count of returning SettingTooLarge from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1616725118,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721006, o.wzCounterSetName, L"Rate of returning MustNotSpecifyContext from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1420647527,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721007, o.wzCounterSetName, L"Count of returning MustNotSpecifyContext from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(19255206,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721008, o.wzCounterSetName, L"Rate of returning MustSpecifyContext from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1783822162,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721009, o.wzCounterSetName, L"Count of returning MustSpecifyContext from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(24190160,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721010, o.wzCounterSetName, L"Rate of returning DuplicateWrites from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1869192552,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721011, o.wzCounterSetName, L"Count of returning DuplicateWrites from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(974868058,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721012, o.wzCounterSetName, L"Rate of returning ReconstructServer from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(2070651631,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721013, o.wzCounterSetName, L"Count of returning ReconstructServer from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(941936406,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721014, o.wzCounterSetName, L"Rate of returning ReconstructClient from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1687433414,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721015, o.wzCounterSetName, L"Count of returning ReconstructClient from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(944314159,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721016, o.wzCounterSetName, L"Rate of returning InvalidListItemData from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(648437578,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721017, o.wzCounterSetName, L"Count of returning InvalidListItemData from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(170317612,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721018, o.wzCounterSetName, L"Rate of returning InvalidListItemKey from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1635497838,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721019, o.wzCounterSetName, L"Count of returning InvalidListItemKey from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(674309209,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721020, o.wzCounterSetName, L"Rate of returning InvalidDataValue from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(867273687,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721021, o.wzCounterSetName, L"Count of returning InvalidDataValue from the EnableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(502408786,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721022, o.wzCounterSetName, L"Rate of returning ServerUnavailable from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1458185711,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721023, o.wzCounterSetName, L"Count of returning ServerUnavailable from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1180443233,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721024, o.wzCounterSetName, L"Rate of returning Success from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(620717366,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721025, o.wzCounterSetName, L"Count of returning Success from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1394387574,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721026, o.wzCounterSetName, L"Rate of returning Throttled from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1265129603,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721027, o.wzCounterSetName, L"Count of returning Throttled from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1004452870,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721028, o.wzCounterSetName, L"Rate of returning WriteFailed from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(607764953,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721029, o.wzCounterSetName, L"Count of returning WriteFailed from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1783085706,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721030, o.wzCounterSetName, L"Rate of returning UserFlaggedAsDisabled from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(304957651,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721031, o.wzCounterSetName, L"Count of returning UserFlaggedAsDisabled from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(2127473784,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721032, o.wzCounterSetName, L"Rate of returning UserNotFlaggedAsDisabled from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1069465770,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721033, o.wzCounterSetName, L"Count of returning UserNotFlaggedAsDisabled from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(314128397,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721034, o.wzCounterSetName, L"Rate of returning UserNotFound from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1066151310,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721035, o.wzCounterSetName, L"Count of returning UserNotFound from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(542407318,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721036, o.wzCounterSetName, L"Rate of returning UnsupportedClient from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(862524963,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721037, o.wzCounterSetName, L"Count of returning UnsupportedClient from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(982527406,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721038, o.wzCounterSetName, L"Rate of returning UnsupportedProtocol from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1910998973,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721039, o.wzCounterSetName, L"Count of returning UnsupportedProtocol from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1274963660,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721040, o.wzCounterSetName, L"Rate of returning TypeMismatch from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1320047873,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721041, o.wzCounterSetName, L"Count of returning TypeMismatch from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(36480884,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721042, o.wzCounterSetName, L"Rate of returning MalformedRequest from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1405338395,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721043, o.wzCounterSetName, L"Count of returning MalformedRequest from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1874515058,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721044, o.wzCounterSetName, L"Rate of returning InvalidSettingId from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1172392359,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721045, o.wzCounterSetName, L"Count of returning InvalidSettingId from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1295016288,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721046, o.wzCounterSetName, L"Rate of returning SettingTooLarge from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(198105448,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721047, o.wzCounterSetName, L"Count of returning SettingTooLarge from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1654689082,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721048, o.wzCounterSetName, L"Rate of returning MustNotSpecifyContext from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(370381905,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721049, o.wzCounterSetName, L"Count of returning MustNotSpecifyContext from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1074932932,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721050, o.wzCounterSetName, L"Rate of returning MustSpecifyContext from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1558721175,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721051, o.wzCounterSetName, L"Count of returning MustSpecifyContext from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1145790023,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721052, o.wzCounterSetName, L"Rate of returning DuplicateWrites from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(479502081,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721053, o.wzCounterSetName, L"Count of returning DuplicateWrites from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(954631966,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721054, o.wzCounterSetName, L"Rate of returning ReconstructServer from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(2036144207,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721055, o.wzCounterSetName, L"Count of returning ReconstructServer from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(245238481,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721056, o.wzCounterSetName, L"Rate of returning ReconstructClient from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1721928292,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721057, o.wzCounterSetName, L"Count of returning ReconstructClient from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(250860268,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721058, o.wzCounterSetName, L"Rate of returning InvalidListItemData from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1671175645,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721059, o.wzCounterSetName, L"Count of returning InvalidListItemData from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(811887097,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721060, o.wzCounterSetName, L"Rate of returning InvalidListItemKey from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1472295595,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721061, o.wzCounterSetName, L"Count of returning InvalidListItemKey from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(1829463758,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721062, o.wzCounterSetName, L"Rate of returning InvalidDataValue from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(827081363,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {721063, o.wzCounterSetName, L"Count of returning InvalidDataValue from the DisableUser API"}; if ((g_perfIdMap.insert(PERF_PAIR(532978932,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {12, L"Roaming API Timers", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1309476910,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {786432, o.wzCounterSetName, L"Execution time for the WriteSettings api"}; if ((g_perfIdMap.insert(PERF_PAIR(1794151716,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786433, o.wzCounterSetName, L"WriteSettings_ServiceExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1947466807,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786434, o.wzCounterSetName, L"Execution time for the ReadSettings api"}; if ((g_perfIdMap.insert(PERF_PAIR(1408156204,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786435, o.wzCounterSetName, L"ReadSettings_ServiceExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1084519783,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786436, o.wzCounterSetName, L"Execution time for the EnableUser api"}; if ((g_perfIdMap.insert(PERF_PAIR(811103051,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786437, o.wzCounterSetName, L"EnableUser_ServiceExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(627158312,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786438, o.wzCounterSetName, L"Execution time for the DisableUser api"}; if ((g_perfIdMap.insert(PERF_PAIR(1134915884,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786439, o.wzCounterSetName, L"DisableUser_ServiceExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1616637887,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786440, o.wzCounterSetName, L"Execution time for the WriteSettings api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(1631797473,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786441, o.wzCounterSetName, L"WriteSettings_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1274667231,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786442, o.wzCounterSetName, L"Execution time for the ReadSettings api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(1482495977,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786443, o.wzCounterSetName, L"ReadSettings_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(2135845263,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786444, o.wzCounterSetName, L"Execution time for the EnableUser api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(1005299342,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786445, o.wzCounterSetName, L"EnableUser_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(445362626,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786446, o.wzCounterSetName, L"Execution time for the DisableUser api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(1209443561,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786447, o.wzCounterSetName, L"DisableUser_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1605497687,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786448, o.wzCounterSetName, L"Execution time for the InitializeFanoutCache api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(39278730,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786449, o.wzCounterSetName, L"InitializeFanoutCache_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1833273428,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786450, o.wzCounterSetName, L"Execution time for the GetUserDataLocator api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(146265287,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786451, o.wzCounterSetName, L"GetUserDataLocator_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(582344378,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786452, o.wzCounterSetName, L"Execution time for the DeleteUserData api in storage"}; if ((g_perfIdMap.insert(PERF_PAIR(2004344236,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {786453, o.wzCounterSetName, L"DeleteUserData_StorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1141894748,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {13, L"Roaming Settings Storage", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1009061919,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {851968, o.wzCounterSetName, L"Execution time for the call to Azure to write settings"}; if ((g_perfIdMap.insert(PERF_PAIR(436111864,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {851969, o.wzCounterSetName, L"WriteSettings_TableStorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1401201728,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {851970, o.wzCounterSetName, L"Execution time for the call to Azure to read settings"}; if ((g_perfIdMap.insert(PERF_PAIR(1367891761,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {851971, o.wzCounterSetName, L"ReadSettings_TableStorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(591160147,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {851972, o.wzCounterSetName, L"Execution time for the call to Azure to read user status"}; if ((g_perfIdMap.insert(PERF_PAIR(2134319117,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {851973, o.wzCounterSetName, L"GetUserStatus_TableStorageExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(397126015,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {14, L"Roaming Settings Service", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1908170842,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {917504, o.wzCounterSetName, L"Execution time in the identity adapter for client authentication"}; if ((g_perfIdMap.insert(PERF_PAIR(240660622,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917505, o.wzCounterSetName, L"ClientIdentityAdapterExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1353985627,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917506, o.wzCounterSetName, L"Execution time in the identity adapter for service authentication"}; if ((g_perfIdMap.insert(PERF_PAIR(875905611,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917507, o.wzCounterSetName, L"ServiceIdentityAdapterExecutionTimeBase"}; if ((g_perfIdMap.insert(PERF_PAIR(1607127153,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917508, o.wzCounterSetName, L"Rate of requests for WriteSettings API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(517690763,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917509, o.wzCounterSetName, L"Count of requests for WriteSettings API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1999375428,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917510, o.wzCounterSetName, L"Rate of requests for ReadSettings API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(711858437,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917511, o.wzCounterSetName, L"Count of requests for ReadSettings API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1033831563,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917512, o.wzCounterSetName, L"Rate of requests for EnableUser API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1220564719,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917513, o.wzCounterSetName, L"Count of requests for EnableUser API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1815038610,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917514, o.wzCounterSetName, L"Rate of requests for DisableUser API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(1512937125,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917515, o.wzCounterSetName, L"Count of requests for DisableUser API from O15Cache"}; if ((g_perfIdMap.insert(PERF_PAIR(2115040448,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917516, o.wzCounterSetName, L"Rate of requests for WriteSettings API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(490169583,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917517, o.wzCounterSetName, L"Count of requests for WriteSettings API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(1692571213,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917518, o.wzCounterSetName, L"Rate of requests for ReadSettings API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(965872292,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917519, o.wzCounterSetName, L"Count of requests for ReadSettings API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(1047737839,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917520, o.wzCounterSetName, L"Rate of requests for EnableUser API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(47299267,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917521, o.wzCounterSetName, L"Count of requests for EnableUser API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(2109950328,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917522, o.wzCounterSetName, L"Rate of requests for DisableUser API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(1270977859,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917523, o.wzCounterSetName, L"Count of requests for DisableUser API from ODCWeb"}; if ((g_perfIdMap.insert(PERF_PAIR(1844242273,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917524, o.wzCounterSetName, L"Rate of requests for WriteSettings API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(597283186,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917525, o.wzCounterSetName, L"Count of requests for WriteSettings API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(900263225,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917526, o.wzCounterSetName, L"Rate of requests for ReadSettings API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(1319937062,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917527, o.wzCounterSetName, L"Count of requests for ReadSettings API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(14516340,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917528, o.wzCounterSetName, L"Rate of requests for EnableUser API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(1015558783,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917529, o.wzCounterSetName, L"Count of requests for EnableUser API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(608929895,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917530, o.wzCounterSetName, L"Rate of requests for DisableUser API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(306795604,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917531, o.wzCounterSetName, L"Count of requests for DisableUser API from WACOneNote"}; if ((g_perfIdMap.insert(PERF_PAIR(449989091,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917532, o.wzCounterSetName, L"Rate of requests for WriteSettings API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(857613667,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917533, o.wzCounterSetName, L"Count of requests for WriteSettings API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(1302530359,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917534, o.wzCounterSetName, L"Rate of requests for ReadSettings API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(1841046470,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917535, o.wzCounterSetName, L"Count of requests for ReadSettings API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(274363491,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917536, o.wzCounterSetName, L"Rate of requests for EnableUser API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(1431826127,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917537, o.wzCounterSetName, L"Count of requests for EnableUser API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(73963765,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917538, o.wzCounterSetName, L"Rate of requests for DisableUser API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(845825218,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {917539, o.wzCounterSetName, L"Count of requests for DisableUser API from Other"}; if ((g_perfIdMap.insert(PERF_PAIR(969068035,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {15, L"Roaming Settings Usage", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1681674455,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {983040, o.wzCounterSetName, L"Rate of WriteSettings requests for untracked settings"}; if ((g_perfIdMap.insert(PERF_PAIR(461650315,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983041, o.wzCounterSetName, L"Rate of ReadSettings requests for untracked settings"}; if ((g_perfIdMap.insert(PERF_PAIR(711673449,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983042, o.wzCounterSetName, L"Rate of WriteSettings requests for ridUserName"}; if ((g_perfIdMap.insert(PERF_PAIR(462869886,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983043, o.wzCounterSetName, L"Rate of ReadSettings requests for ridUserName"}; if ((g_perfIdMap.insert(PERF_PAIR(1701532842,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983044, o.wzCounterSetName, L"Rate of WriteSettings requests for ridUserInitials"}; if ((g_perfIdMap.insert(PERF_PAIR(1433236972,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983045, o.wzCounterSetName, L"Rate of ReadSettings requests for ridUserInitials"}; if ((g_perfIdMap.insert(PERF_PAIR(215634088,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983046, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruAccessPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(907117561,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983047, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruAccessPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(2126064655,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983048, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruAccessUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(266720272,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983049, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruAccessUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(931080073,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983050, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruOneNotePinned"}; if ((g_perfIdMap.insert(PERF_PAIR(817103496,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983051, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruOneNotePinned"}; if ((g_perfIdMap.insert(PERF_PAIR(524318287,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983052, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruOneNoteUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1896642054,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983053, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruOneNoteUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1828834832,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983054, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruPowerPointPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1943343300,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983055, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruPowerPointPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(184751957,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983056, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruPowerPointUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(874341874,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983057, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruPowerPointUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(826597179,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983058, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruPowerPointPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1060964557,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983059, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruPowerPointPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(2142240992,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983060, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruPowerPointUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1243872501,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983061, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruPowerPointUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1211818387,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983062, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruPublisherPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(487458108,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983063, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruPublisherPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(194130879,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983064, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruPublisherUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1992838699,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983065, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruPublisherUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(687470050,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983066, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruVisioPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(679334173,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983067, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruVisioPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(312950886,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983068, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruVisioUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1561167860,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983069, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruVisioUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1529728995,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983070, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruWordPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(863115731,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983071, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruWordPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1111732340,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983072, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruWordUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1025631455,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983073, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruWordUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1773280261,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983074, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruWordPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(906070496,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983075, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruWordPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(658949088,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983076, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruWordUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(888458463,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983077, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruWordUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1958463814,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983078, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruInfoPathDesignerPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(638848392,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983079, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruInfoPathDesignerPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(262744501,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983080, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruInfoPathDesignerUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(2121259517,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983081, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruInfoPathDesignerUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(854676089,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983082, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruInfoPathFillerPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(71041007,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983083, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruInfoPathFillerPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(965412415,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983084, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruInfoPathFillerUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1938388045,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983085, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruInfoPathFillerUnpinned"}; if ((g_perfIdMap.insert(PERF_PAIR(486755164,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983086, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruExcelPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1183867508,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983087, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruExcelPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(617196975,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983088, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruExcelUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1185959713,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983089, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruExcelUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1949725242,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983090, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruExcelPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1301291550,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983091, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruExcelPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(985837027,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983092, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruExcelUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1623537662,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983093, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruExcelUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(451934435,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983094, o.wzCounterSetName, L"Rate of WriteSettings requests for ridTestMruPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1848284037,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983095, o.wzCounterSetName, L"Rate of ReadSettings requests for ridTestMruPinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(669063007,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983096, o.wzCounterSetName, L"Rate of WriteSettings requests for ridTestMruUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(1219388521,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983097, o.wzCounterSetName, L"Rate of ReadSettings requests for ridTestMruUnpinnedDoc"}; if ((g_perfIdMap.insert(PERF_PAIR(825781342,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983098, o.wzCounterSetName, L"Rate of WriteSettings requests for ridTestMruPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1133381976,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983099, o.wzCounterSetName, L"Rate of ReadSettings requests for ridTestMruPinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(2144121733,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983100, o.wzCounterSetName, L"Rate of WriteSettings requests for ridTestMruUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(1273978383,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983101, o.wzCounterSetName, L"Rate of ReadSettings requests for ridTestMruUnpinnedPlace"}; if ((g_perfIdMap.insert(PERF_PAIR(80007908,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983102, o.wzCounterSetName, L"Rate of WriteSettings requests for ridTheme"}; if ((g_perfIdMap.insert(PERF_PAIR(1694183712,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983103, o.wzCounterSetName, L"Rate of ReadSettings requests for ridTheme"}; if ((g_perfIdMap.insert(PERF_PAIR(1729378637,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983104, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateWord"}; if ((g_perfIdMap.insert(PERF_PAIR(1646448259,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983105, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateWord"}; if ((g_perfIdMap.insert(PERF_PAIR(1307385656,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983106, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplatePowerPoint"}; if ((g_perfIdMap.insert(PERF_PAIR(1759360598,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983107, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplatePowerPoint"}; if ((g_perfIdMap.insert(PERF_PAIR(1659363411,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983108, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateExcel"}; if ((g_perfIdMap.insert(PERF_PAIR(2067233433,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983109, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateExcel"}; if ((g_perfIdMap.insert(PERF_PAIR(1007416611,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983110, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateVisio"}; if ((g_perfIdMap.insert(PERF_PAIR(1071065340,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983111, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateVisio"}; if ((g_perfIdMap.insert(PERF_PAIR(1269885625,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983112, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplatePublisher"}; if ((g_perfIdMap.insert(PERF_PAIR(1873394730,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983113, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplatePublisher"}; if ((g_perfIdMap.insert(PERF_PAIR(652252272,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983114, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateProject"}; if ((g_perfIdMap.insert(PERF_PAIR(601855340,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983115, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateProject"}; if ((g_perfIdMap.insert(PERF_PAIR(1934826977,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983116, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateAccess"}; if ((g_perfIdMap.insert(PERF_PAIR(1787905102,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983117, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateAccess"}; if ((g_perfIdMap.insert(PERF_PAIR(236606311,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983118, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateWordPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1647069741,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983119, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateWordPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1609372087,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983120, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplatePowerPointPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1139189599,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983121, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplatePowerPointPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(187901062,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983122, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateExcelPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1188878490,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983123, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateExcelPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(154541726,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983124, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateVisioPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1003187226,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983125, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateVisioPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1991571073,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983126, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplatePublisherPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1463010567,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983127, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplatePublisherPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1663562207,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983128, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateProjectPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1512779961,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983129, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateProjectPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1093331262,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983130, o.wzCounterSetName, L"Rate of WriteSettings requests for ridMruTemplateAccessPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(2136151197,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {983131, o.wzCounterSetName, L"Rate of ReadSettings requests for ridMruTemplateAccessPinned"}; if ((g_perfIdMap.insert(PERF_PAIR(1829736712,c))).second == false) return;}
  }
  {
    ULSPERF_COUNTER_INFO o = {16, L"Roaming setting storage account tracking", 0}; if ((g_perfIdMap.insert(PERF_PAIR(1204059195,o))).second == false) return;
    {ULSPERF_COUNTER_INFO c = {1048576, o.wzCounterSetName, L"Count of requests for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1046907544,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048577, o.wzCounterSetName, L"Rate of ServerUnavailable results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1550190581,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048578, o.wzCounterSetName, L"Rate of Success results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(2102046070,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048579, o.wzCounterSetName, L"Rate of Throttled results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(2090202469,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048580, o.wzCounterSetName, L"Rate of WriteFailed results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1983645470,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048581, o.wzCounterSetName, L"Rate of UserFlaggedAsDisabled results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1685964352,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048582, o.wzCounterSetName, L"Rate of UserNotFlaggedAsDisabled results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1683119696,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048583, o.wzCounterSetName, L"Rate of UserNotFound results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(977972345,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048584, o.wzCounterSetName, L"Rate of InconsistentKnowledge results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1188021475,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048585, o.wzCounterSetName, L"Rate of WriteConflict results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(694001114,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048586, o.wzCounterSetName, L"Rate of Unknown results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(1214870203,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048587, o.wzCounterSetName, L"Rate of OfficeStorageException results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(562855681,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048588, o.wzCounterSetName, L"Rate of Requests results for storage account 0"}; if ((g_perfIdMap.insert(PERF_PAIR(388993629,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048589, o.wzCounterSetName, L"Count of requests for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1179764927,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048590, o.wzCounterSetName, L"Rate of ServerUnavailable results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(2072604532,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048591, o.wzCounterSetName, L"Rate of Success results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1959617998,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048592, o.wzCounterSetName, L"Rate of Throttled results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1272416598,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048593, o.wzCounterSetName, L"Rate of WriteFailed results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(175607577,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048594, o.wzCounterSetName, L"Rate of UserFlaggedAsDisabled results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1496055962,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048595, o.wzCounterSetName, L"Rate of UserNotFlaggedAsDisabled results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1057537673,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048596, o.wzCounterSetName, L"Rate of UserNotFound results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1908604664,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048597, o.wzCounterSetName, L"Rate of InconsistentKnowledge results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(2073952827,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048598, o.wzCounterSetName, L"Rate of WriteConflict results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1592829686,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048599, o.wzCounterSetName, L"Rate of Unknown results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1106164227,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048600, o.wzCounterSetName, L"Rate of OfficeStorageException results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1883954145,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048601, o.wzCounterSetName, L"Rate of Requests results for storage account 1"}; if ((g_perfIdMap.insert(PERF_PAIR(1863897206,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048602, o.wzCounterSetName, L"Count of requests for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(838332823,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048603, o.wzCounterSetName, L"Rate of ServerUnavailable results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(330880600,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048604, o.wzCounterSetName, L"Rate of Success results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(1850031110,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048605, o.wzCounterSetName, L"Rate of Throttled results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(303107502,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048606, o.wzCounterSetName, L"Rate of WriteFailed results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(1901389893,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048607, o.wzCounterSetName, L"Rate of UserFlaggedAsDisabled results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(517884765,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048608, o.wzCounterSetName, L"Rate of UserNotFlaggedAsDisabled results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(756572222,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048609, o.wzCounterSetName, L"Rate of UserNotFound results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(1386608180,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048610, o.wzCounterSetName, L"Rate of InconsistentKnowledge results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(1013729790,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048611, o.wzCounterSetName, L"Rate of WriteConflict results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(972559486,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048612, o.wzCounterSetName, L"Rate of Unknown results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(1533469643,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048613, o.wzCounterSetName, L"Rate of OfficeStorageException results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(2113849748,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048614, o.wzCounterSetName, L"Rate of Requests results for storage account 2"}; if ((g_perfIdMap.insert(PERF_PAIR(415164764,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048615, o.wzCounterSetName, L"Count of requests for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(1237365694,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048616, o.wzCounterSetName, L"Rate of ServerUnavailable results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(878165713,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048617, o.wzCounterSetName, L"Rate of Success results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(1740829886,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048618, o.wzCounterSetName, L"Rate of Throttled results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(626225565,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048619, o.wzCounterSetName, L"Rate of WriteFailed results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(220160064,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048620, o.wzCounterSetName, L"Rate of UserFlaggedAsDisabled results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(596610437,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048621, o.wzCounterSetName, L"Rate of UserNotFlaggedAsDisabled results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(1984069881,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048622, o.wzCounterSetName, L"Rate of UserNotFound results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(422421691,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048623, o.wzCounterSetName, L"Rate of InconsistentKnowledge results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(20811560,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048624, o.wzCounterSetName, L"Rate of WriteConflict results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(1314205522,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048625, o.wzCounterSetName, L"Rate of Unknown results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(1390488435,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048626, o.wzCounterSetName, L"Rate of OfficeStorageException results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(741895538,c))).second == false) return;}
    {ULSPERF_COUNTER_INFO c = {1048627, o.wzCounterSetName, L"Rate of Requests results for storage account 3"}; if ((g_perfIdMap.insert(PERF_PAIR(1619635061,c))).second == false) return;}
  }
}


/*
#include <specstrings.h>


typedef unsigned short wchar_t;
typedef unsigned short USHORT;
typedef wchar_t WCHAR;
typedef WCHAR* PWCH;
typedef _Null_terminated_ PWCH PWSTR, LPCWSTR;
typedef const PWSTR PCWSTR;
typedef unsigned char UCHAR, *PUCHAR;
typedef size_t SIZE_T;
typedef void VOID, *PVOID;
typedef long LONG;
typedef unsigned long ULONG, DWORD;
typedef unsigned short RTL_STRING_LENGTH_TYPE;
typedef _Return_type_success_(return >= 0) LONG NTSTATUS;

#define UNICODE_STRING_MAX_BYTES ((USHORT) 65534) // winnt
#define UNICODE_STRING_MAX_CHARS (32767) // winnt

#define STATUS_SUCCESS                          ((NTSTATUS)0x00000000L) // ntsubauth
#define STATUS_NAME_TOO_LONG             ((NTSTATUS)0xC0000106L)
#define STATUS_NO_MEMORY                 ((DWORD   )0xC0000017L)    

#define NT_SUCCESS(Status) (((NTSTATUS)(Status)) >= 0)
#define NULL 0
#define MAX_PATH 260


typedef struct _UNICODE_STRING {
    USHORT Length;
    USHORT MaximumLength;
    _Field_size_bytes_part_(MaximumLength, Length) PWCH   Buffer;
} UNICODE_STRING, *PUNICODE_STRING;

typedef struct _RTL_BUFFER {
    PUCHAR    Buffer;
    PUCHAR    StaticBuffer;
    SIZE_T    Size;
    SIZE_T    StaticSize;
    SIZE_T    ReservedForAllocatedSize; // for future doubling
    PVOID     ReservedForIMalloc; // for future pluggable growth
} RTL_BUFFER, *PRTL_BUFFER;

struct _RTL_UNICODE_STRING_BUFFER;

typedef struct _RTL_UNICODE_STRING_BUFFER {
    UNICODE_STRING String;
    RTL_BUFFER     ByteBuffer;
    UCHAR          MinimumStaticBufferForTerminalNul[sizeof(WCHAR)];
} RTL_UNICODE_STRING_BUFFER, *PRTL_UNICODE_STRING_BUFFER;


_At_(DestinationString->Buffer, _Post_equal_to_(SourceString))  
_At_(DestinationString->Length, _Post_equal_to_(_String_length_(SourceString) * sizeof(WCHAR)))  
_At_(DestinationString->MaximumLength, _Post_equal_to_((_String_length_(SourceString)+1) * sizeof(WCHAR)))  
VOID  
RtlInitUnicodeString(  
    _Out_ PUNICODE_STRING DestinationString,  
    _In_opt_z_ PCWSTR SourceString  
    );  

PVOID 
RtlMoveMemory(
    _Out_writes_bytes_all_opt_(_Size) PVOID _Dst,
    _In_reads_bytes_opt_(_Size) const PVOID _Src,
    _In_ size_t _Size
    );
    
NTSTATUS
SdbEnsureBufferSizeFunction(
    _In_                    ULONG       Flags,
    _Inout_updates_bytes_(Size)    PRTL_BUFFER Buffer,
    _In_                    SIZE_T      Size
    );

char _RTL_CONSTANT_STRING_type_check(const void *s);
#define _RTL_CONSTANT_STRING_remove_const_macro(s) (s)

#define RTL_CONSTANT_STRING(s) \
{ \
    sizeof( s ) - sizeof( (s)[0] ), \
    sizeof( s ) / sizeof(_RTL_CONSTANT_STRING_type_check(s)), \
    _RTL_CONSTANT_STRING_remove_const_macro(s) \
}    

static const UNICODE_STRING AppcompatKeyPathCustomKey = RTL_CONSTANT_STRING(L"Custom\\");
  
#define RtlInitBuffer(Buff, StatBuff, StatSize) \
    do {                                        \
        (Buff)->Buffer       = (StatBuff);      \
        (Buff)->Size         = (StatSize);      \
        (Buff)->StaticBuffer = (StatBuff);      \
        (Buff)->StaticSize   = (StatSize);      \
    } while (0)

#define RtlEnsureBufferSize(Flags, Buff, NewSizeBytes) \
    (   ((Buff) != NULL && (NewSizeBytes) <= (Buff)->Size) \
        ? STATUS_SUCCESS \
        : SdbEnsureBufferSizeFunction((Flags), (Buff), (NewSizeBytes)) \
    )

#define RtlInitUnicodeStringBuffer(Buff, StatBuff, StatSize)      \
    do {                                                          \
        SIZE_T TempStaticSize = (StatSize);                       \
        PUCHAR TempStaticBuff = (StatBuff);                       \
        TempStaticSize &= ~(sizeof((Buff)->String.Buffer[0]) - 1);  \
        if (TempStaticSize > UNICODE_STRING_MAX_BYTES) {          \
            TempStaticSize = UNICODE_STRING_MAX_BYTES;            \
        }                                                         \
        if (TempStaticSize < sizeof(WCHAR)) {                     \
            TempStaticBuff = (Buff)->MinimumStaticBufferForTerminalNul; \
            TempStaticSize = sizeof(WCHAR);                       \
        }                                                         \
        RtlInitBuffer(&(Buff)->ByteBuffer, TempStaticBuff, TempStaticSize); \
        (Buff)->String.Buffer = (WCHAR*)TempStaticBuff;           \
        if ((Buff)->String.Buffer != NULL)                        \
            (Buff)->String.Buffer[0] = 0;                         \
        (Buff)->String.Length = 0;                                \
        (Buff)->String.MaximumLength = (RTL_STRING_LENGTH_TYPE)TempStaticSize;    \
    } while (0)
    
#define RtlAssignUnicodeStringBuffer(Buff, Str) \
    (((Buff)->String.Length = 0), (RtlAppendUnicodeStringBuffer((Buff), (Str))))
    
#define RtlAppendUnicodeStringBuffer(Dest, Source)                            \
    ( ( ( (Dest)->String.Length + (Source)->Length + sizeof((Dest)->String.Buffer[0]) ) > UNICODE_STRING_MAX_BYTES ) \
        ? STATUS_NAME_TOO_LONG                                                \
        : (!NT_SUCCESS(                                                       \
                RtlEnsureBufferSize(                                          \
                    0,                                                        \
                    &(Dest)->ByteBuffer,                                          \
                    (Dest)->String.Length + (Source)->Length + sizeof((Dest)->String.Buffer[0]) ) ) \
                ? STATUS_NO_MEMORY                                            \
                : ( ( (Dest)->String.Buffer = (PWSTR)(Dest)->ByteBuffer.Buffer ), \
                    ( RtlMoveMemory(                                          \
                        (Dest)->String.Buffer + (Dest)->String.Length / sizeof((Dest)->String.Buffer[0]), \
                        (Source)->Buffer,                                     \
                        (Source)->Length) ),                                  \
                    ( (Dest)->String.MaximumLength = (RTL_STRING_LENGTH_TYPE)((Dest)->String.Length + (Source)->Length + sizeof((Dest)->String.Buffer[0]))), \
                    ( (Dest)->String.Length = (USHORT) ((Dest)->String.Length + (Source)->Length )),            \
                    ( (Dest)->String.Buffer[(Dest)->String.Length / sizeof((Dest)->String.Buffer[0])] = 0 ), \
                    ( STATUS_SUCCESS ) ) ) )

void foo(_In_ LPCWSTR pwszPath)
{
    NTSTATUS Status;
    RTL_UNICODE_STRING_BUFFER KeyPath = {0}; // buffer to store exe path
    UCHAR BufferKeyPath[260 * 2];
    UNICODE_STRING ustrPath;
    UNICODE_STRING customKey = RTL_CONSTANT_STRING(L"Custom\\");
    
//    RtlInitUnicodeString(&ustrPath, pwszPath);
        
    
//    RtlInitUnicodeStringBuffer(&KeyPath, BufferKeyPath, sizeof(BufferKeyPath));
        SIZE_T TempStaticSize = 520;
        PUCHAR TempStaticBuff = BufferKeyPath;
        TempStaticSize &= ~(sizeof(UCHAR) - 1);
//        if (TempStaticSize > UNICODE_STRING_MAX_BYTES) {
//            TempStaticSize = UNICODE_STRING_MAX_BYTES;
//        }
        if (TempStaticSize < sizeof(WCHAR)) {
            TempStaticBuff = KeyPath.MinimumStaticBufferForTerminalNul;
            TempStaticSize = sizeof(WCHAR);
        }                                                         
//        RtlInitBuffer(&KeyPath.ByteBuffer, TempStaticBuff, TempStaticSize);
        (&KeyPath.ByteBuffer)->Buffer       = TempStaticBuff;
        (&KeyPath.ByteBuffer)->Size         = TempStaticSize;
        (&KeyPath.ByteBuffer)->StaticBuffer = TempStaticBuff;
        (&KeyPath.ByteBuffer)->StaticSize   = TempStaticSize;
        KeyPath.String.Buffer = (WCHAR*)TempStaticBuff;
        if (KeyPath.String.Buffer != NULL)
            KeyPath.String.Buffer[0] = 0;                         
        KeyPath.String.Length = 0;
        KeyPath.String.MaximumLength = (RTL_STRING_LENGTH_TYPE)TempStaticSize;

//    Status = RtlAssignUnicodeStringBuffer(&KeyPath, &customKey);
    Status = (&KeyPath)->String.Length = 0,    
        (!NT_SUCCESS(
            if (!RtlEnsureBufferSize(0, &(&KeyPath)->ByteBuffer, (&KeyPath)->String.Length + (Source)->Length + sizeof((&KeyPath)->String.Buffer[0])))
                 (&KeyPath)->String.Buffer = (PWSTR)(&KeyPath)->ByteBuffer.Buffer ), \
                    ( RtlMoveMemory(                                          \
                        (&KeyPath)->String.Buffer + (&KeyPath)->String.Length / sizeof((&KeyPath)->String.Buffer[0]), \
                        (Source)->Buffer,                                     \
                        (Source)->Length) ),                                  \
                    ( (&KeyPath)->String.MaximumLength = (RTL_STRING_LENGTH_TYPE)((&KeyPath)->String.Length + (Source)->Length + sizeof((&KeyPath)->String.Buffer[0]))), \
                    ( (&KeyPath)->String.Length = (USHORT) ((&KeyPath)->String.Length + (Source)->Length )),            \
                    ( (&KeyPath)->String.Buffer[(&KeyPath)->String.Length / sizeof((&KeyPath)->String.Buffer[0])] = 0 ), \
                    ( STATUS_SUCCESS ) ) ) )
    
    if (!NT_SUCCESS(Status)) {
        return;
    }

//    Status = RtlAppendUnicodeStringBuffer(&KeyPath, &ustrPath);    
    
}
*/

/*
// 3. Try annotation on typedef.
//     No warnings expected in this function.
void espRepro(ReproType3* someType)
{   
    int localArray[SIZE];
    int ci = someType->indices[0];
    localArray[ci] = 0;
    globalArray[ci] = 0;
}



typedef unsigned long ULONG;
typedef unsigned char UCHAR;

extern "C" void memset(_Out_writes_bytes_(count) void *dst, int val, size_t count);
extern "C" __bcount(size) void *malloc(size_t size);

typedef struct _MYSTRUCT {
   ULONG something;
   ULONG Length;
    __field_bcount_opt(Length) UCHAR Data[1];
} MYSTRUCT, *PMYSTRUCT;

template <class myType>
class MyClass {
public:
    void MyFunc(myType &pobject);
};

void ErrorTestFunc()
{
    PMYSTRUCT pObj1;
    MyClass<MYSTRUCT *> myclassobj;

    pObj1 = (PMYSTRUCT)malloc(20);
    pObj1->Length = 20;

    myclassobj.MyFunc(pObj1);   // expecting warning here
}



__bcount(cnt) void* malloc(int cnt);
void memcpy(__out_bcount(cb) void *dst, __in_bcount(cb) void *src, size_t cb);

typedef struct _FLEXARRAY {
    unsigned int count;
    __ecount(count) int arr[1];
} FLEXARRAY;

void bad1(__in_ecount(cnt) int *buf, size_t cnt, __inout FLEXARRAY *f)
{
    memcpy(f->arr, buf, cnt * sizeof(int));
}


//Tests for references
void ref1(__out_ecount(n) char *p, int &n)
{
	p[n] = 1;	
}

typedef unsigned char BYTE;
typedef unsigned long ULONG;

bool IsEqualGUID(int*, int*);

typedef struct _RegEntry { int ProviderGuid; } RegEntryType;

#define MAX_REGISTRATIONS 12
#define min(a,b)            (((a) < (b)) ? (a) : (b))


void foo(_In_reads_bytes_(sizeof(int) * MAX_REGISTRATIONS) RegEntryType* entries, 
    _Out_writes_bytes_opt_(OutBufferSize) void* OutBuffer,
    _In_ ULONG OutBufferSize
)
{
    ULONG MaxGuidsToCopy = OutBufferSize / sizeof(int);
    ULONG GuidCount = 0;
    int* GuidArray = (int*)OutBuffer;
    int* Guids;
    bool GuidNotIncluded;
    
    for (ULONG Index = 0; Index < MAX_REGISTRATIONS; Index++)
    {

        RegEntryType* RegEntry = &entries[Index];       
        if (RegEntry == 0) continue;
        
        GuidCount += 1;
        if (GuidCount <=  MaxGuidsToCopy) {                
            *GuidArray++ = 1; 
        }      
    }
}


extern "C"
_Post_equal_to_(dst)
_At_buffer_(dst, _I_, count, _Post_satisfies_(((BYTE*)dst)[_I_] == ((BYTE*)src)[_I_]))
void* mymemcpy(
    _Out_writes_bytes_all_(count) void* dst,
    _In_reads_bytes_(count) const void* src,
    _In_ size_t count
);

extern "C" unsigned int GetColorLength();
extern "C" char* GetColorName();

template < typename T > class SafeInt
{
public:
    SafeInt() throw()
    {
        m_int = 0;
    }  
    
    
    SafeInt( const T& i ) throw()
    {
        m_int = i;
    }
    
    operator __int32() const
    {
        return (__int32)m_int;
    }

    operator unsigned __int32() const
    {
        return (unsigned __int32)m_int;
    }
   
private:
    T m_int;
};


template < typename T, typename U >
bool operator ==( U lhs, SafeInt< T > rhs ) throw()
{
    return (T)rhs == lhs;
}


template < typename T, typename U >
SafeInt< T > operator +( U lhs, SafeInt < T > rhs )
{
    T ret( 0 );
    ret = (T)rhs + lhs;
    return SafeInt< T >( ret );
}

void foo(_Out_cap_(cchOut) char *szOut, unsigned int &ichOut, int cchOut)
{
    //unsigned int nameLength = GetColorLength();
    SafeInt<UINT_PTR> nameLength(GetColorLength());
    unsigned int length = ichOut + nameLength;
    _Analysis_assume_(length == ichOut + nameLength);
    if (length < cchOut)
    {
        mymemcpy(szOut + ichOut, GetColorName(), length - ichOut);
    }
}

typedef char* LPSTR;
typedef const char* LPCSTR;
typedef unsigned long ULONG;
typedef unsigned short WCHAR;
typedef WCHAR* PWSTR;
typedef unsigned char BYTE;
typedef unsigned int UINT;
typedef unsigned long DWORD;
typedef long LONG;

bool IsPatternSpecial(char c);

_Success_(return == true)
bool
QuotePatternSpecials(__in_ecount(InChars) LPCSTR In,
                     __in ULONG InChars,
                     __out_ecount(OutChars) LPSTR Out,
                     __in ULONG OutChars)
{
    //
    // Make a pass over the input string and
    // produce an output string that will be interpreted
    // literally by the dbghelp pattern matcher
    // (i.e. all regex chars are quoted).
    //

    while (InChars--)
    {
        if (IsPatternSpecial(*In))
        {
            if (OutChars < 1)
            {
               return false;
            }
            *Out = '\\';  //Warning 26010 here
            Out++;    
            OutChars--;
        }

        if (OutChars < 1)
        {
            return false;
        }

        *Out = *In++;
        Out++;
        OutChars--;
    }

    if (OutChars < 1)
    {
        return false;
    }

    *Out = 0;
    return true;
}

PWSTR GetSomeText(LONG *pcchValid);

extern "C"
_Post_equal_to_(dst)
_At_buffer_(dst, _I_, count, _Post_satisfies_(((BYTE*)dst)[_I_] == ((BYTE*)src)[_I_]))
void* CopyMemory(
    _Out_writes_bytes_all_(count) void* dst,
    _In_reads_bytes_(count) const void* src,
    _In_ size_t count
);

void MyTest(LONG cch, __out_ecount(cch) PWSTR pch)
{
	LONG cchValid;
	const WCHAR *pchRead;
	while( cch > 0 )
	{
		pchRead = GetSomeText(&cchValid);
		if(!pchRead)					// No more text
			break;

		if (cchValid <= cch)
			CopyMemory(pch, pchRead, cchValid*sizeof(WCHAR));

		pch += cchValid;
		cch -= cchValid;
	}
}
*/
/*

extern void Convert(
    _Out_writes_bytes_(*pWritten) LPSTR szDest,
    _In_reads_(*pRead) LPCSTR szSource,
    _Out_ ULONG* pRead,
    _Out_ ULONG* pWritten);

extern bool IsLeadByte(char c);



typedef unsigned long DWORD;
typedef unsigned short WCHAR;
typedef _Null_terminated_ WCHAR* PWSTR;


extern "C" _Ret_range_(==, _String_length_(str)) size_t wcslen(_In_ const PWSTR str);

extern "C" void memcpy(_Out_writes_bytes_(size) void *dst, _In_reads_bytes_(size) const void *src, unsigned int size);
extern "C" void widecopy(_Out_writes_(size) WCHAR* dst, _In_reads_(size) const WCHAR* src, unsigned int size);
extern "C" int LoadString(  
    _In_ DWORD uID,  
    _Out_writes_to_(cchBufferMax, return + 1) PWSTR lpBuffer,  
    _In_ int cchBufferMax);
_Success_(return != 0)  
extern "C" DWORD FormatMessage(  
    _In_     DWORD dwFlags,  
    _Outptr_    PWSTR* lpBuffer);

void bad(_In_ DWORD error)
{
    WCHAR pwszErrorMessage[1024] = {0};
    PWSTR pwszWin32Error = 0;
    
    LoadString(error, pwszErrorMessage, 1024);
    DWORD rc = FormatMessage(0, &pwszWin32Error);
    
    if (!rc) return;
    
    if (wcslen(pwszErrorMessage) + wcslen(pwszWin32Error) + 1 < 1024)
    {
    
        widecopy(pwszErrorMessage + wcslen(pwszErrorMessage),
            pwszWin32Error,
            wcslen(pwszWin32Error) + 1);
        
        memcpy
        (
            pwszErrorMessage + wcslen(pwszErrorMessage),
            pwszWin32Error,
            (wcslen(pwszWin32Error) + 1) * sizeof(WCHAR)
        );
        
    }
}


typedef unsigned short wchar_t;
typedef _Null_terminated_ const wchar_t* PCWSTR;

void bar(
    __deref_out_z PCWSTR*name, 
);

void foo()
{
    wchar_t* p;
    bar(&p);
}
*/

/*
class MyClass
{
public:   
    MyClass(){}
    
    _Field_range_(0, 100) int size;
    _Field_size_(size) char* buf;
};

MyClass MakeClass()  
{
   return MyClass();
}

MyClass* MakeClassPtr()  
{
   return new MyClass();
}

void foo(_In_ MyClass a)
{
  if (a.size > 1)
    a.buf[0] = 'h';
}

void ConsumeClass(_In_ char c)
{
  MyClass a(MakeClass());
  if (a.size > 1)
    a.buf[0] = c;
}

void ConsumeClassAssignment(_In_ char c)
{
    MyClass a = MakeClass();
    if (a.size > 1)
        a.buf[0] = 'h';
}

void ConsumeClassPtr(_In_ char c)
{
    MyClass* a = MakeClassPtr();
    if (a->size > 1)
        a->buf[4] = 'h';
}
*/
/*
#define NULL 0
typedef unsigned long DWORD;
typedef unsigned short WCHAR;
typedef _Null_terminated_ WCHAR* PWSTR;


extern "C" _Ret_range_(==, _String_length_(str)) size_t wcslen(_In_z_ const WCHAR* str);

extern "C" void memcpy(_Out_writes_bytes_(size) void *dst, _In_reads_bytes_(size) const void *src, unsigned int size);

extern "C" int LoadString(  
    _In_ DWORD uID,  
    _Out_writes_to_(cchBufferMax, return + 1) PWSTR lpBuffer,  
    _In_ int cchBufferMax);

_Success_(return != 0)  
extern "C" DWORD FormatMessage(  
    _In_     DWORD dwFlags,  
    _Out_    PWSTR lpBuffer);

void bad(_In_ DWORD uErrorStringID, _In_ DWORD dwErrorText)
{
    WCHAR pwszErrorMessage[1024] = {0};
    PWSTR pwszWin32Error = NULL;
    
    LoadString(uErrorStringID, pwszErrorMessage, 1024);

    DWORD retVal = FormatMessage
    (
        0,
        (PWSTR)&pwszWin32Error
    );
    
    if (!retVal) return;

    if (wcslen(pwszErrorMessage) + wcslen(pwszWin32Error) + 1 < 1024)
    {
        memcpy
        (
            pwszErrorMessage + wcslen(pwszErrorMessage),
            pwszWin32Error,
            (wcslen(pwszWin32Error) + 1) * sizeof(WCHAR)
        );
    }
}
*/
