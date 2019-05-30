// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    internal class OverflowCorrector
    {
        // Note: Consider sbyte overflow: Range is -128...127, so 129 will overflow and become -127. 
        // To correct it, add 256. (-127 + 256 = 129). The int equivalent is therefore 2^32.
        private const long PerOverflowCorrection = 4294967296;

        private int _overflowLineNumber;
        private long _overflowCorrection;
        private bool _currentlyNegative;

        public OverflowCorrector()
        {
            _overflowLineNumber = -1;
        }

        public long CorrectForOverflow(int lineNumber, long charInLine)
        {
            if (lineNumber != _overflowLineNumber)
            {
                // Reset on new line number
                _overflowLineNumber = lineNumber;
                _overflowCorrection = 0;
            }
            else
            {
                // On each overflow, add a correction
                bool isNegative = (charInLine < 0);
                if (_currentlyNegative == false && isNegative == true)
                {

                    _overflowCorrection += PerOverflowCorrection;
                }
                _currentlyNegative = isNegative;
            }

            return charInLine + _overflowCorrection;
        }
    }
}
