using System;
using System.Text;

namespace Microsoft.Sarif.Viewer.Models
{
    public class FileDetailsModel
    {
        // Contents of file. May or may not be Base64 encoded.
        private string _contents;

        // Tells whether or not the _contents are Base64 encoded.
        private bool _encoded;

        public FileDetailsModel(string hash, string contents)
        {
            Hash = hash;
            _contents = contents;
            _encoded = true;
        }

        public string Hash { get; }

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

        private void DecodeContents()
        {
            var data = Convert.FromBase64String(_contents);
            _contents = Encoding.UTF8.GetString(data);
            _encoded = false;
        }
    }
}
