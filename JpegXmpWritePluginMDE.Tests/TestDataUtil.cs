// Copyright (c) Drew Noakes and contributors. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Reflection;

namespace JpegXmpWritePluginMDE.Tests
{
	/// <summary>Utility functions for working with unit tests data files.</summary>
	/// <author>Drew Noakes https://drewnoakes.com</author>
	internal static class TestDataUtil
	{
		/// <summary>
		/// Traditionally, NUnit and xUnit on desktops have run tests such that the current directory
		/// was the project folder. xUnit on .NET Core uses the bin/Debug folder. This method tries both.
		/// </summary>
		public static string GetPath(string filePath) => File.Exists(filePath) ? filePath : Path.Combine("../..", filePath);

		public static Stream OpenRead(string filePath) => new FileStream(GetPath(filePath), FileMode.Open, FileAccess.Read, FileShare.Read);

		public static byte[] GetBytes(string filePath) => File.ReadAllBytes(GetPath(filePath));
		/// <summary>
		/// Gets path of the executing assembly
		/// </summary>
		/// <returns></returns>
		public static string GetTestExecutionPath() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		/// <summary>
		/// Gets the path to the test file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetFullTestFilePath(string fileName)
		{
			return Path.Combine(GetTestExecutionPath(), "Data", fileName);
		}
		/// <summary>
		/// Creates copy of a test file to avoid multiple unit tests accessing the same file for writing
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string CreateTestCopy(string fileName)
		{
			string destinationFilePath = GetTestExecutionPath() + "\\" + "Data\\"+ fileName + Guid.NewGuid().ToString();
			string originalFilePath = GetFullTestFilePath(fileName);
			File.Copy(originalFilePath, destinationFilePath);
			return destinationFilePath;
		}
		/// <summary>
		/// Delete the test file
		/// </summary>
		/// <param name="fileName"></param>
		public static void DeleteTestFile (string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

		}
	}
}
