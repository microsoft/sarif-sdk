// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sarif.Sdk.Sample
{
    public class AnalysisSample : IDisposable
    {
        private readonly string[] _myStringArray;
        private int[] _myIntArray;

        public int[] MyIntArray
        {
            get { return _myIntArray; }
        }

        public int GetFirstEmptyStringIndex()
        {
            for (int i = 0; i < _myStringArray.Length; i++)
            {
                if (_myStringArray[i] == "")
                {
                    return i;
                }
            }

            return -1;
        }

        public void Dispose()
        {
        }
    }
}
