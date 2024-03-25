#region License
//
// Copyright 2002-2017 Drew Noakes
// Ported from Java to C# by Yakov Danilov for Imazen LLC in 2014
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/metadata-extractor-dotnet
//    https://drewnoakes.com/code/exif/
//
#endregion
using JpegXmpWritePluginMDE.MetadataExtractor;
using XmpCore;
using Xunit;

namespace JpegXmpWritePluginMDE.Tests
{
	/// <summary>Unit tests for <see cref="ImageMetadataWriter"/>.</summary>
	/// <author>Michael Osthege</author>
	public sealed class ImageMetadataWriterTest
	{
		[Fact]
		public void TestWriteImageMetadata()
		{
			string writeToFile = TestDataUtil.CreateTestCopy("xmpWriting_PictureWithMicrosoftXmp.jpg");
			string xmpToWrite = TestDataUtil.GetFullTestFilePath("xmpWriting_XmpContent.xmp");
			string expectedOutputFile = TestDataUtil.GetFullTestFilePath("xmpWriting_PictureWithMicrosoftXmpReencoded.jpg");

			IXmpMeta xmp = XmpMetaFactory.ParseFromString(File.ReadAllText(xmpToWrite));
			byte[] expectedResult = TestDataUtil.GetBytes(expectedOutputFile);
			
			var metadata_objects = new object[] { xmp };
			ImageMetadataWriter.WriteMetadata(writeToFile, metadata_objects);
			byte[] actualResult = TestDataUtil.GetBytes(writeToFile);

			TestDataUtil.DeleteTestFile(writeToFile);

			Assert.True(actualResult.SequenceEqual(expectedResult));
		}
	}
}
