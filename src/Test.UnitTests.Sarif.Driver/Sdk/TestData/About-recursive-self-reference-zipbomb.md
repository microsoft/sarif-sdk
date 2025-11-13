# Recursive Self-Reference Zip Bomb

## Overview

This is a **self-referencing zip bomb** - a ZIP file that contains itself. When you extract the file `r/r.zip` from inside, you get an identical 440-byte ZIP file, creating infinite recursion.

## Creator & History

- **Created by**: [Russ Cox](https://swtch.com/~rsc/)
- **Date**: March 18, 2010
- **Original article**: [Zip Files All The Way Down](https://research.swtch.com/zip)

## Technical Details

- **Size**: 440 bytes
- **Internal filename**: `r/r.zip`
- **External filename**: `recursive_self_reference_zipbomb.zip` (renamed for clarity)
- **Compression**: Hand-crafted DEFLATE stream using uncompressed blocks and backreferences
- **SHA-256**: `d152d368ca2da00de987cfe58c868534410ab270773d59d0d7956e671d3adafd`

## How It Works

The file uses a carefully constructed 328-byte DEFLATE stream that decompresses to the entire 440-byte file. The key technique:
- Uses **uncompressed blocks** for literal data
- Uses **LZ77 backreferences** to repeat previously decompressed data
- The output "catches up to and overtakes" the input at precisely the right moment

This creates a mathematical fixed-point where `decompress(file) = file`.

## Purpose

This file is used for **testing archive enumeration code** to ensure proper handling of:
- Recursive archive structures
- Infinite extraction loops
- Depth limits and resource protection

## Warning

⚠️ **Do not extract recursively without depth protection!** This file will create infinite copies of itself until disk space or memory is exhausted.

---

*Note: The original file was named `r.zip` by Russ Cox. We renamed it to `recursive_self_reference_zipbomb.zip` for clarity in this test suite.*