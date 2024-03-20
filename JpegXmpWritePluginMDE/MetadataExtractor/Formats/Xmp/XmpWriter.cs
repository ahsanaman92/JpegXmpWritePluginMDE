using JetBrains.Annotations;
using JpegXmpWritePluginMDE.MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Xmp;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XmpCore;
using XmpCore.Impl;
using XmpCore.Options;

#if NET35
using FragmentList = System.Collections.Generic.IList<MetadataExtractor.Formats.Jpeg.JpegFragment>;
#else
using FragmentList = System.Collections.Generic.IReadOnlyList<JpegXmpWritePluginMDE.MetadataExtractor.Formats.Jpeg.JpegFragment>;
#endif

namespace JpegXmpWritePluginMDE.MetadataExtractor.Formats.Xmp
{
	public sealed class XmpWriter : IJpegFragmentMetadataWriter
	{
		/// <summary>
		/// Specifies the type of metadata that this MetadataWriter can handle.
		/// </summary>
		Type IJpegFragmentMetadataWriter.MetadataXmpMetaType => typeof(XmpMeta);

		/// <summary>
		/// Updates a list of JpegFragments with new metadata.
		/// <para>
		/// An existing App1 Xmp fragment will be updated. If none is found, a new segment will be
		/// inserted before the first fragment that is not one of {Soi, App0, App1}
		/// </para>
		/// </summary>
		/// <param name="fragments">Original file fragmets</param>
		/// <param name="metadata">The Xmp metadata that shall be written</param>
		/// <returns>A new list of JpegFragments</returns>
		public List<JpegFragment> UpdateFragments([NotNull] FragmentList fragments, [NotNull] object metadata)
		{
			JpegFragment metadataFragment;
			List<JpegFragment> output = new List<JpegFragment>();
			bool wroteData = false;
			int insertPosition = 0;

			if (metadata is IXmpMeta xmpMeta)
			{
				byte[] payloadBytes = WritePreambleToXmpBytes(xmpMeta);
				JpegSegmentPlugin metadataSegment = new JpegSegmentPlugin(JpegSegmentType.App1, payloadBytes, offset: 0);
				metadataFragment = JpegFragment.FromJpegSegment(metadataSegment);
			}
			else
			{
				throw new ArgumentException($"XmpWriter expects metadata to be of type IXmpMeta, but was given {metadata.GetType()}.");
			}

			// First look for any potential Xmp fragment, insert only if none is found

			// Walk over existing fragment and replace or insert the new metadata fragment
			for (int i = 0; i < fragments.Count; i++)
			{
				JpegFragment currentFragment = fragments[i];

				if (!wroteData && currentFragment.IsSegment)
				{
					JpegSegmentType currentType = currentFragment.Segment.Type;

					// if this is an existing App1 XMP fragment, overwrite it with the new fragment
					if (currentType == JpegSegmentType.App1 && currentFragment.Segment.Bytes.Length > XmpReader.JpegSegmentPreamble.Length)
					{
						// This App1 segment could be a candidate for overwriting.
						// Read the encountered segment payload to check if it contains the Xmp preamble
						string potentialPreamble = Encoding.UTF8.GetString(currentFragment.Segment.Bytes, 0, XmpReader.JpegSegmentPreamble.Length);
						if (potentialPreamble.Equals(Encoding.UTF8.GetString(XmpReader.JpegSegmentPreamble.ToArray()), StringComparison.OrdinalIgnoreCase))
						{
							// The existing Xmp App1 fragment will be replaced with the new fragment
							currentFragment = metadataFragment;
							wroteData = true;
						}
					}
					else if (insertPosition == 0 && currentType != JpegSegmentType.Soi && currentType != JpegSegmentType.App0)
					{
						// file begins with Soi (App0) (App1) ...
						// At this point we have encountered a segment that should not be earlier than an App1.
						// But there could be another Xmp segment, so we just make a note of this position
						insertPosition = i;
					}
				}
				output.Add(currentFragment);
			}

			if (!wroteData)
			{
				// The files does not contain an App1-Xmp segment yet.
				// Therefore we must insert a new App1-Xmp segment at the previously determined position.
				output.Insert(insertPosition, metadataFragment);
				wroteData = true;
			}

			return output;
		}
		/// <summary>
		/// Calls SerializeToBuffer on IXmpMeta object to encode bytes to be used as the payload of an App1 segment.
		/// </summary>
		/// <param name="xmpMeta">Xmp document to be encoded</param>
		/// <returns>App1 segment payload</returns>
		public static byte[] WritePreambleToXmpBytes([NotNull] IXmpMeta xmpMeta)
		{
			// xmp preamble
			byte[] preamble = XmpReader.JpegSegmentPreamble.ToArray();
			// xmpMeta object serialized
			byte[] xmpMetaBytes = XmpMetaFactory.SerializeToBuffer(xmpMeta, new SerializeOptions());

			MemoryStream xmpMS = new MemoryStream();
			// 1. preamble "http://ns.adobe.com/xap/1.0/\0"
			xmpMS.Write(preamble, 0, preamble.Length);

			// 2. xmpMeta 
			xmpMS.Write(xmpMetaBytes, 0, xmpMetaBytes.Length);

			return xmpMS.ToArray();
		}
	}
}
