using System;
using System.Text;

namespace Microsoft.Sarif.Viewer.Models
{
    public class FileDetailsModel
    {
        /// <summary>
        /// Contents of file. May or may not be Base64 encoded.
        /// </summary>
        private string _contents;

        /// <summary>
        /// Tells whether or not the _contents are Base64 encoded.
        /// </summary>
        private bool _encoded;

        /// <summary>
        /// Creates a new object of the <see cref="FileDetailsModel" /> class.
        /// </summary>
        /// <param name="hash">SHA256 hash for file.</param>
        /// <param name="contents">Base64 encoded contents of the file.</param>
        public FileDetailsModel(string hash, string contents)
        {
            Hash = hash;
            _contents = contents;
            _encoded = true;
        }

        /// <summary>
        /// SHA256 hash for file.
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Returns the decoded file contents.
        /// </summary>
        public string Contents
        {
            get
            {
                // If the contents are encoded, decode them.
                if (_encoded)
                {
                    DecodeContents();
                }

                return _contents;
            }
        }

        /// <summary>
        /// Decodes the contents.
        /// </summary>
        private void DecodeContents()
        {
            var data = Convert.FromBase64String(_contents);
            _contents = Encoding.UTF8.GetString(data);
            _encoded = false;
        }
    }
}
