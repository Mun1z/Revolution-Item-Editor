﻿using System;
using System.Collections.Generic;
using System.IO;

namespace RevoIE.Domain.Entities
{
    public class Sprite
	{
		public static bool LoadSprites(string filename, ref Dictionary<UInt16, Sprite> sprites, UInt32 signature)
		{
			FileStream fileStream = new FileStream(filename, FileMode.Open);
			try
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					UInt32 sprSignature = reader.ReadUInt32();
					if (signature != 0 && signature != sprSignature)
					{
						return false;
					}

					UInt16 totalPics = reader.ReadUInt16();

					List<UInt32> spriteIndexes = new List<UInt32>();
					for (uint i = 0; i < totalPics; ++i)
					{
						UInt32 index = reader.ReadUInt32();
						spriteIndexes.Add(index);
					}

					UInt16 id = 1;
					foreach (UInt32 element in spriteIndexes)
					{
						UInt32 index = element + 3;
						reader.BaseStream.Seek(index, SeekOrigin.Begin);
						UInt16 size = reader.ReadUInt16();

                        if (sprites.TryGetValue(id, out Sprite sprite))
                        {
                            if (sprite != null && size > 0)
                            {
                                if (sprite.size > 0)
                                {
                                    //generate warning
                                }
                                else
                                {
                                    sprite.id = id;
                                    sprite.size = size;
                                    sprite.dump = reader.ReadBytes(size);

                                    sprites[id] = sprite;
                                }
                            }
                        }
                        else
                        {
                            reader.BaseStream.Seek(size, SeekOrigin.Current);
                        }

                        ++id;
					}
				}
			}
			finally
			{
				fileStream.Close();
			}

			return true;
		}

        public static bool LoadSprites(string filename, ref Dictionary<UInt32, Sprite> sprites, UInt32 signature)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            try
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    UInt32 sprSignature = reader.ReadUInt32();
                    if (signature != 0 && signature != sprSignature)
                    {
                        return false;
                    }

                    UInt32 totalPics = reader.ReadUInt32();

                    List<UInt32> spriteIndexes = new List<UInt32>();
                    for (uint i = 0; i < totalPics; ++i)
                    {
                        UInt32 index = reader.ReadUInt32();
                        spriteIndexes.Add(index);
                    }

                    UInt32 id = 1;
                    foreach (UInt32 element in spriteIndexes)
                    {
                        UInt32 index = element + 3;
                        reader.BaseStream.Seek(index, SeekOrigin.Begin);
                        UInt16 size = reader.ReadUInt16();

                        if (sprites.TryGetValue(id, out Sprite sprite))
                        {
                            if (sprite != null && size > 0)
                            {
                                if (sprite.size > 0)
                                {
                                    //generate warning
                                }
                                else
                                {
                                    sprite.id = id;
                                    sprite.size = size;
                                    sprite.dump = reader.ReadBytes(size);

                                    sprites[id] = sprite;
                                }
                            }
                        }
                        else
                        {
                            reader.BaseStream.Seek(size, SeekOrigin.Current);
                        }

                        ++id;
                    }
                }
            }
            finally
            {
                fileStream.Close();
            }

            return true;
        }

		public Sprite()
		{
			id = 0;
			size = 0;
			dump = null;
		}

		public UInt32 id;
		public UInt32 size;
		public byte[] dump;

		public byte[] GetRGBData()
		{
			const byte transparentColor = 0x11;
			return GetRGBData(transparentColor);
		}

		public byte[] GetRGBData(byte transparentColor)
		{
			byte[] rgb32x32x3 = new byte[32 * 32 * 3];
			UInt32 bytes = 0;
			UInt32 x = 0;
			UInt32 y = 0;
			Int32 chunkSize;

			while (bytes < size)
			{
				chunkSize = dump[bytes] | dump[bytes + 1] << 8;
				bytes += 2;

				for (int i = 0; i < chunkSize; ++i)
				{
					// Transparent pixel
					rgb32x32x3[96 * y + x * 3 + 0] = transparentColor;
					rgb32x32x3[96 * y + x * 3 + 1] = transparentColor;
					rgb32x32x3[96 * y + x * 3 + 2] = transparentColor;
					x++;
					if (x >= 32)
					{
						x = 0;
						++y;
					}
				}

				if (bytes >= size) break; // We're done
				// Now comes a pixel chunk, read it!
				chunkSize = dump[bytes] | dump[bytes + 1] << 8;
				bytes += 2;
				for (int i = 0; i < chunkSize; ++i)
				{
					byte red = dump[bytes + 0];
					byte green = dump[bytes + 1];
					byte blue = dump[bytes + 2];
					rgb32x32x3[96 * y + x * 3 + 0] = red;
					rgb32x32x3[96 * y + x * 3 + 1] = green;
					rgb32x32x3[96 * y + x * 3 + 2] = blue;

					bytes += 3;

					x++;
					if (x >= 32)
					{
						x = 0;
						++y;
					}
				}
			}

			// Fill up any trailing pixels
			while (y < 32 && x < 32)
			{
				rgb32x32x3[96 * y + x * 3 + 0] = transparentColor;
				rgb32x32x3[96 * y + x * 3 + 1] = transparentColor;
				rgb32x32x3[96 * y + x * 3 + 2] = transparentColor;
				x++;
				if (x >= 32)
				{
					x = 0;
					++y;
				}
			}

			return rgb32x32x3;
		}

		public byte[] GetRGBAData()
		{
			byte[] rgb32x32x3 = GetRGBData();
			byte[] rgbx32x32x4 = new byte[32 * 32 * 4];

			//reverse rgb
			for (int y = 0; y < 32; ++y)
			{
				for (int x = 0; x < 32; ++x)
				{
					rgbx32x32x4[128 * y + x * 4 + 0] = rgb32x32x3[(32 - y - 1) * 96 + x * 3 + 2]; //blue
					rgbx32x32x4[128 * y + x * 4 + 1] = rgb32x32x3[(32 - y - 1) * 96 + x * 3 + 1]; //green
					rgbx32x32x4[128 * y + x * 4 + 2] = rgb32x32x3[(32 - y - 1) * 96 + x * 3 + 0]; //red
					rgbx32x32x4[128 * y + x * 4 + 3] = 0;
				}
			}

			return rgbx32x32x4;
		}
	}
}
