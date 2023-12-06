// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{

    public class FileEncodingTests
    {
        [Fact]
        public void FileEncoding_NullBytesRaisesException()
        {
            Assert.Throws<ArgumentNullException>(() => FileEncoding.IsTextualData(null, 1, 1));
        }

        [Fact]
        public void FileEncoding_StartExceedsBufferLength()
        {
            // Start argument exceeds buffer size.
            Assert.Throws<ArgumentException>(() => FileEncoding.IsTextualData(new byte[1], 1, 1));
        }

        [Fact]
        public void FileEncoding_AllowCountToExceedBufferLength()
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            FileEncoding.IsTextualData(new byte[1], 0, 1024).Should().BeTrue();
        }

        [Fact]
        public void FileEncoding_FileEncoding_IsBinary()
        {
            ValidateIsBinary("Sarif.dll");
            ValidateIsBinary("Sarif.pdb");
        }

        private void ValidateIsBinary(string fileName)
        {
            fileName = Path.Combine(Environment.CurrentDirectory, fileName);
            using FileStream reader = File.OpenRead(fileName);
            int bufferSize = 1024;
            byte[] bytes = new byte[bufferSize];
            reader.Read(bytes, 0, bufferSize);
            FileEncoding.IsTextualData(bytes).Should().BeFalse();
        }

        [Fact]
        public void FileEncoding_TestOneOffFiles()
        {
            string fileName = @"E:\src\os.2020\src\clientcore\termsrv\newsvc\core\lagcounterdef.ini";
            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(fileName)
            };

            Console.WriteLine("Textual : " + artifact.Bytes == null);
        }


        [Fact]
        public void FileEncoding_CrawlDirectory()
        {
            var textExtensions = new HashSet<string>(new[] { "_port_64_", ".1", ".3dmdef", ".acf", ".acp", ".adef", ".adml", ".admx", ".AFM", ".ait", ".alz", ".API", ".appxmanifest", ".appxsources", ".asax", ".ascx", ".asm", ".aspx", ".awk", ".bamo", ".bas", ".bat", ".bbtf", ".bms", ".byu", ".c", ".C", ".cd", ".chi", ".clang-format", ".cmd", ".cmdfile", ".cod", ".config", ".Config", ".cpp", ".Cpp", ".CPP", ".cs", ".csh", ".csproj", ".css", ".csv", ".cvss", ".cxx", ".d", ".dat", ".DC", ".ddk", ".def", ".Def", ".DEF", ".definition", ".dep", ".deployment", ".dif", ".disco", ".discomap", ".djt", ".dlg", ".dos", ".DotSettings", ".dsp", ".dsw", ".dtd", ".dui", ".duih", ".editorconfig", ".filters", ".fn", ".fx", ".fxh", ".gen", ".genxfg", ".gitignore", ".gpd", ".gsh", ".h", ".H", ".hhc", ".hhk", ".hhp", ".hlsl", ".hpj", ".hpp", ".htm", ".html", ".hxx", ".i", ".idl", ".in", ".inc", ".INC", ".inf", ".INF", ".ini", ".inl", ".ite", ".js", ".json", ".jsproj", ".kbom", ".Kbom", ".kbomdirectories", ".kml", ".l", ".lci", ".Lci", ".legacytest", ".lnk", ".log", ".lrf", ".lst", ".m", ".m4", ".mak", ".man", ".manifest", ".Manifest", ".mas", ".master", ".mbt", ".mc", ".mcp", ".md", ".mh", ".mk", ".mock", ".mock2", ".mode1v3", ".mof", ".mst", ".mum", ".natvis", ".notloc", ".nt", ".nuspec", ".OUT", ".pbxproj", ".pbxuser", ".pch", ".pck", ".pdf", ".pfb", ".pict", ".pl", ".plist", ".pluginlist", ".pm", ".ppm", ".prep", ".prf", ".privsym", ".pro", ".PRO", ".profiles", ".proj", ".ps", ".ps1", ".psh", ".psm1", ".publish)", ".publishproj", ".pubxml", ".py", ".ql", ".qll", ".rc", ".rc2", ".rcv", ".reg", ".resx", ".resX", ".rgs", ".ribbon", ".rtf", ".rts", ".s", ".sbx", ".scopeproj", ".scr", ".script", ".seq", ".set", ".settings", ".sh", ".sitemap", ".skl", ".sln", ".smc", ".smd", ".smr", ".snippet", ".sql", ".src", ".stb", ".strings", ".SUP", ".tab", ".template", ".testenv", ".testlist", ".testpasses", ".tlh", ".tpl", ".tru", ".tst", ".txt", ".TXT", ".unx", ".user", ".usp", ".USP", ".vbs", ".vcproj", ".vcxproj", ".vdproj", ".ver", ".vs", ".vsh", ".vspscc", ".vssscc", ".w", ".webinfo", ".wnt", ".wpaProfile", ".wprp", ".wsdl", ".wsf", ".wxs", ".x", ".xaml", ".xib", ".xml", ".Xml", ".XML", ".xsd", ".XSD", ".xsl", ".xslt", ".xtc", ".y", ".yml", "clip1", "configure", "COPYING", "cube_obj", "ddksources", "dirs", "DIRS", "dirs_stop", "dirs-ansi", "dirs-original", "dirs-unicode", "gcoord", "gdc", "gdraw", "gmapmode", "gmeta", "grgn", "INSTALL", "iosfwd", "log", "makefile", "Makefile", "MAKEFILE", "makesdk", "mdipal", "mkinstalldirs", "mkwin9x", "monkey_obj", "NEWS", "readme", "README", "rops", "sources", "sourceS", "Sources", "SOURCES", "sources_chfngen", "sphere_obj", "testlist", "tiny_obj", "tsproxyrpcsources", "width" });
            var binaryExtensions = new HashSet<string>(new[] { ".932", ".936", ".949", ".950", ".ani", ".API", ".aps", ".avi", ".bin", ".blg", ".bmp", ".bsh", ".cab", ".cs", ".cur", ".dds", ".dib", ".DIB", ".djx", ".dle", ".dll", ".DLL", ".doc", ".docx", ".emf", ".EMF", ".eot", ".etl", ".EUF", ".exe", ".EXE", ".fnt", ".fon", ".fxo", ".gif", ".GIF", ".glb", ".grp", ".gso", ".hlp", ".icc", ".icm", ".icns", ".ico", ".ICO", ".ilk", ".imp", ".ipch", ".jpg", ".JPG", ".jps", ".lib", ".lnk", ".md2", ".mdp", ".mmm", ".mp4", ".MP4", ".mrdp", ".mui", ".onnx", ".otf", ".pdb", ".pfb", ".PFb", ".PFB", ".pfm", ".PFM", ".pfx", ".png", ".ppm", ".ppt", ".pptx", ".prm", ".prn", ".pso", ".rat", ".ReaderTestInput", ".res", ".resources", ".rmp", ".scr", ".sdf", ".sdkmesh", ".sdkmesh_anim", ".suo", ".tag", ".tex", ".tga", ".tif", ".tiff", ".ttc", ".TTC", ".tte", ".TTE", ".ttf", ".TTF", ".txt", ".usp", ".USP", ".vso", ".wan", ".wav", ".wmf", ".wmv", ".xlsx", ".xpu", ".xso", ".xvu", ".zip", "CompiledShader_BC", "CompiledShader_Simple", "MSRC55690_fontPackageBuffer", "MSRC55690_mergeFontBuffer", "MSRC59209_fontPackageBuffer", "MSRC59209_mergeFontBuffer" });
            var bothExtensions = new HashSet<string>(new[] { ".API", ".cs", ".lnk", ".pfb", ".ppm", ".scr", ".txt", ".USP" });

            string directoryName = @"E:\src\os.2020\src\base\";

            using var writer = new StreamWriter(@"d:\repros\WindowsFiles.csv");
            writer.WriteLine("Path,Extension,Classification");

            var specifier = new OrderedFileSpecifier(directoryName, recurse: true);
            {
                foreach (EnumeratedArtifact artifact in specifier.Artifacts)
                {
                    string filePath = artifact.Uri.LocalPath;
                    string extension = Path.GetExtension(filePath);
                    extension = !string.IsNullOrEmpty(extension) ? extension : Path.GetFileNameWithoutExtension(filePath);
                    string classification = artifact.Contents != null ? "Text" : "Binary";
                    writer.WriteLine($"{filePath},{extension},{classification}");

                    if (classification == "Text")
                    {
                        if (binaryExtensions.Contains(extension))
                        {
                            bothExtensions.Add(extension);
                        }

                        textExtensions.Add(extension);
                    }
                    else
                    {
                        if (bothExtensions.Contains(extension))
                        {
                            Console.WriteLine("here");
                        }
                        
                        if (textExtensions.Contains(extension))
                        {
                            bothExtensions.Add(extension);
                        }

                        binaryExtensions.Add(extension);
                    }
                }
            }

            string[] text = textExtensions.ToArray();
            Array.Sort(text);

            string[] binary = binaryExtensions.ToArray();
            Array.Sort(binary);

            string[] both = bothExtensions.ToArray();
            Array.Sort(both);

            writer.WriteLine("var textExtensions = new HashSet<string>(new[] { \"" + string.Join("\", \"", text) + "\" });");
            writer.WriteLine("var binaryExtensions = new HashSet<string>(new[] { \"" + string.Join("\", \"", binary) + "\" });");
            writer.WriteLine("var bothExtensions = new HashSet<string>(new[] { \"" + string.Join("\", \"", both) + "\" });");
        }


        [Fact]
        public void FileEncoding_UnicodeDataIsTextual()
        {
            var sb = new StringBuilder();
            string unicodeText = "американец";

            foreach (Encoding encoding in new[] { Encoding.Unicode, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.UTF32 })
            {
                byte[] input = encoding.GetBytes(unicodeText);
                if (!FileEncoding.IsTextualData(input))
                {
                    sb.AppendLine($"\tThe '{encoding.EncodingName}' encoding classified unicode text '{unicodeText}' as binary data.");
                }
            }

            sb.Length.Should().Be(0, because: $"all unicode strings should be classified as textual:{Environment.NewLine}{sb}");
        }

        [Fact]
        public void FileEncoding_BinaryDataIsBinary()
        {
            var sb = new StringBuilder();

            foreach (string binaryName in new[] { "Certificate.cer", "Certificate.der", "PasswordProtected.pfx" })
            {
                string resourceName = $"Test.UnitTests.Sarif.TestData.FileEncoding.{binaryName}";
                Stream resource = typeof(FileEncodingTests).Assembly.GetManifestResourceStream(resourceName);

                var artifact = new EnumeratedArtifact(new FileSystem())
                {
                    Uri = new Uri(resourceName, UriKind.Relative),
                    Stream = resource,
                };

                if (artifact.Bytes == null)
                {
                    sb.AppendLine($"\tBinary file '{binaryName}' was classified as textual data.");
                }
            }

            sb.Length.Should().Be(0, because: $"all unicode strings should be classified as textual:{Environment.NewLine}{sb}");
        }

        [Fact]
        public void FileEncoding_AllWindows1252EncodingsAreTextual()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ValidateEncoding("Windows-1252", Encoding.GetEncoding(1252));
        }

        [Fact]
        public void FileEncoding_AllUtf8EncodingsAreTextual()
        {
            ValidateEncoding("Utf8", Encoding.UTF8);
        }

        private static void ValidateEncoding(string encodingName, Encoding encoding)
        {
            var sb = new StringBuilder(65536 * 100);
            Stream resource = typeof(FileEncodingTests).Assembly.GetManifestResourceStream($"Test.UnitTests.Sarif.TestData.FileEncoding.{encodingName}.txt");
            using var reader = new StreamReader(resource, Encoding.UTF8);

            int current;
            while ((current = reader.Read()) != -1)
            {
                char ch = (char)current;
                byte[] input = encoding.GetBytes(new[] { ch });
                if (!FileEncoding.IsTextualData(input))
                {
                    string unicodeText = "\\u" + ((int)ch).ToString("d4");                    
                    sb.AppendLine($"\t{encodingName} character '{unicodeText}' ({encoding.GetString(input)}) was classified as binary data.");
                }
            }

            sb.Length.Should().Be(0, because: $"all {encodingName} encodable character should be classified as textual:{Environment.NewLine}{sb}");
        }
    }
}
