# Overview
JSON Map is a JSON-based format which describes part of the structure of another JSON document. Maps inform clients of the sizes of parts of the JSON to allow constant time construction of subsets of the document.

For example, suppose you have a 20GB JSON file which contains an array with one million elements along with some surrounding document structure. You can construct a map with a 0.1% target size (20 MB). The map will identify the path to the huge array, the number of elements it has, where each nth array element begins, and where the array itself starts and ends. 

To write a copy of the file with only the first 1,000 array elements, copy the file content up to the array start, copy the range from element 0 to element 999, omit the last comma, and copy the range from the array end to the end of the file.


# JSON Map Structure
A JSON Map consists of a hierarchy of **nodes** which mirror the hierarchical structure of the underlying JSON document. A map is constructed with a specific **target size ratio** (ex: 1%) which determines a size threshold for including items in the map.

Each **node** contains:
* A "start" integer, which is the absolute byte position in the file of the start of the object or array (the index of the '[' or '{')
* An "end" integer, which is the absolute byte position in the file of the end of the object or array (the index of the ']' or '}')
* A "count" integer containing the number of elements (for arrays) or properties (for objects) within.
* A "nodes" object, containing a nested **node** for each child over the size threshold. The property name for each nested **node** is the property name or array index of the nested object in the real object. The **nodes** are in order matching their order within the containing object.

If the object is a JSON array, the **node** also contains the start position of every nth array element. If it fits within the target size ratio, every element start is included. If not, N is chosen so that the size of the array starts fits within the target size ratio.

Arrays therefore also have:
* An "every" integer indicating what portion of elements have positions. ("every": 1 means every element, "every": 2 means every other element, etc).
* An "arrayStarts" integer array. The first value is an absolute byte position in the file of the start of the first value; each subsequent value is added to the sum so far to get the absolute byte offset in the file of the start of array`[i * every]` above array`[(i - 1) * every]`.

To find the element with JSON path "array`[15]`.snippet" in a JSON map, search the map for "nodes`["array"]`.nodes`[15]`.nodes`["snippet"]`. If the snippet was too small to have a node, "nodes`["array"]`.nodes`[15]`" or "nodes`["array"]`" can be used to get close to "snippet".

# Example
For the following JSON document:
```
{ "version": "1.0.0", "schema": "https://schema.org/someFancySchema", "results": [ 0, 1000, 2000, 3000, [ 4000 ], 5000, { "v": 6000 }, 7000, 8000, 9000 ] }
```

Here is a particular JSON map:
```
{
  "start": 0,
  "end": 154,
  "count": 3,
  "nodes": {
    "version": {
      "start": 12,
      "end": 19,
      "count": 0
    },
    "schema": {
      "start": 31,
      "end": 67,
      "count": 0
    },
    "results": {
      "start": 81,
      "end": 152,
      "count": 10,
      "every": 1,
      "arrayStarts": [
        82,
        3,
        6,
        6,
        7,
        9,
        7,
        14,
        6,
        6
      ]
    }
  }
}
```

This map has been configured to be up to 10x the size of the document; in most cases, a JSON document this small wouldn't be large enough for any map to be created.

# Uses
JSON Maps can be used to:

### Construct subsets of JSON documents
A map can be used to find any large object or array in a JSON document. One can then parse that subset of the JSON individually, make a new JSON document containing only that item, or make a JSON document like the original which excludes that item.


### Inform clients of JSON size and structure
A map can be sent to a client (for example, a web viewer) to allow it to determine whether to request the whole file or only subsets. A viewer can request a subset of the file with large portions excluded and then, if needed, can request only those subsets to reconstruct the full document. A viewer can request "pages" of elements from large arrays, allowing it to show only the first 100 elements, for example, and then retrieve additional pages as a user scrolls. The exact count is available in the map, so the viewer can display the total number of items available and size scroll bars and similar interface elements appropriately.


### Enable as-you-go JSON parsing
In contexts where a JSON document is loaded into an in-memory object model and is too large to fully load, a client can create an object model where a large array can be "skipped" when reading the JSON and a "placeholder" object put into the object model instead. When the "placeholder" array is enumerated it can then parse the JSON elements one by one and convert them to object model objects. If the placeholder provides random access, it can use the **arrayStarts** to find the desired element (or one closeby) and avoid parsing objects which aren't accessed.


# Size Thresholds
The typical size of a **node** and an arrayStart is used to determine what to include in the map.

**nodes** are estimated to be 90 bytes, which includes:

| Property | Size |
| ------------- | ------------- |
| Property Name outside Node | 10 |
| "start":`[long]`, | 15 |
| "end":`[long]`, | 15 |
| "count":`[int]`, | 15 |
| "nodes":{}, | 10 |
| "every":`[byte]`, | 10 |
| "arrayStarts":`[]`, | 15 |

If the target size ratio is 1%, then any object or array in the JSON which is larger than 9,000 bytes (90 / 1%) will have a **node** included in the map.

**arrayStarts** are each a single number and comma, and are estimated to be five bytes each (four digits and a comma). This estimate is correct for array elements between 1,000 and 10,000 bytes in size.

If the target size ratio is 1%, then every array start will be included if the array elements average 500 bytes (5 / 1%) in size. If the elements are 100 bytes each, every 5th element start will be indexed. In this case, a client trying to find the start of array`[4]` would seek to array`[0]` and then parse the four preceeding elements, which on average would total under 400 bytes.

These estimates are not exact, and a large array may consume twice the desired space - once for the **node** metadata and once for the **arrayStarts**. The ratio is therefore an estimate on the map size rather than a strict limit.


# Offset Details
All offsets in JSON Map are **absolute byte offsets within the file** containing the JSON. Byte offsets are used so that clients can use file system APIs to quickly seek to the position containing a given element or quickly copy a given range to another file. Absolute offsets minimize the need for calculation. **arrayStarts** are an exception, where relative offsets are used to significantly reduce the space needed to store array element positions.

The exact positions stored are important to ensure that the regions they describe are themselves valid JSON. Positions **must** therefore be either the byte offset of the first character of the element described or whitespace immediately preceding it. Positions **should** refer to the first character of the element when possible. For **nodes**, the **start** is the position of the '{' or '[' of the object or array referenced. The **end** is the position of the '}' or ']' of the object or array referenced.

For **nodes** which are properties in a containing object, this means the Start and End refer to the value and don't include the range of the property name in the container. A client wishing to omit a property can either copy the property name and replace the value with the null literal, or can search backward from the position where the value begins to determine where the property name begins to exclude it as well.

For **arrayStarts**, only the start positions are included. A client can copy from the start of one element to the start of the next, but this will include the comma and whitespace after the earlier element. When a client is copying an array slice, it must search backward to remove any trailing whitespace and comma and exclude those bytes so that the copied slice is a properly closed array.