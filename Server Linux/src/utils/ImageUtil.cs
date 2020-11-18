using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux.src.utils
{
	class ImageUtil
	{
		public static byte[] ImageToByte(Image img, ImageFormat format)
		{
			using (var stream = new MemoryStream())
			{
				img.Save(stream, format);
				return stream.ToArray();
			}
		}
	}
}
