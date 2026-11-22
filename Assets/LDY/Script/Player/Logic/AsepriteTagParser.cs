using System;
using System.Collections.Generic;
using System.Text;

namespace LDY.Script {
    // .aseprite 바이너리에서 프레임 수, 프레임 시간, 태그 범위만 읽는다.
    public static class AsepriteTagParser {
        private const int HeaderSize = 128;
        private const int FrameHeaderSize = 16;
        private const int FileMagic = 0xA5E0;
        private const int FrameMagic = 0xF1FA;
        private const int TagsChunk = 0x2018;
        private const int TagFixedSize = 19;

        public static bool TryParse(byte[] data, out AsepriteTagInfo info, out string error) {
            info = null;
            error = null;
            try {
                info = Parse(data);
                return true;
            }
            catch (Exception e) when (e is IndexOutOfRangeException || e is ArgumentException || e is FormatException) {
                error = e.Message;
                return false;
            }
        }

        private static AsepriteTagInfo Parse(byte[] data) {
            if (data == null || data.Length < HeaderSize || U16(data, 4) != FileMagic)
                throw new FormatException("aseprite 헤더가 아님");

            var frameCount = U16(data, 6);
            var durations = new List<int>(frameCount);
            var tags = new List<AsepriteTag>();
            var offset = HeaderSize;
            for (var frame = 0; frame < frameCount; frame++) {
                if (U16(data, offset + 4) != FrameMagic)
                    throw new FormatException($"프레임 {frame} 헤더가 잘못됨");

                durations.Add(U16(data, offset + 8));
                ReadChunks(data, offset, tags);
                offset += (int)U32(data, offset);
            }

            return new AsepriteTagInfo(frameCount, durations, tags);
        }

        private static void ReadChunks(byte[] data, int frameOffset, List<AsepriteTag> tags) {
            var chunkCount = U32(data, frameOffset + 12);
            if (chunkCount == 0)
                chunkCount = (uint)U16(data, frameOffset + 6);

            var offset = frameOffset + FrameHeaderSize;
            for (var i = 0; i < chunkCount; i++) {
                var size = (int)U32(data, offset);
                if (U16(data, offset + 4) == TagsChunk)
                    ReadTags(data, offset, tags);

                offset += size;
            }
        }

        private static void ReadTags(byte[] data, int chunkOffset, List<AsepriteTag> tags) {
            var count = U16(data, chunkOffset + 6);
            var offset = chunkOffset + 16;
            for (var i = 0; i < count; i++) {
                var nameLength = U16(data, offset + 17);
                var name = Encoding.UTF8.GetString(data, offset + TagFixedSize, nameLength);
                tags.Add(new AsepriteTag(name, U16(data, offset), U16(data, offset + 2)));
                offset += TagFixedSize + nameLength;
            }
        }

        private static int U16(byte[] data, int offset) {
            return data[offset] | (data[offset + 1] << 8);
        }

        private static uint U32(byte[] data, int offset) {
            return (uint)(U16(data, offset) | (U16(data, offset + 2) << 16));
        }
    }
}
