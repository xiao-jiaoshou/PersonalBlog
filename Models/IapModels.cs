using System;

namespace PersonalBlog.Models
{
    public class IapSettings
    {
        public int TimeoutMs { get; set; } = 10000; // 默认 10s
        public int MaxRetries { get; set; } = 3;
        public bool ChecksumLittleEndian { get; set; } = true; // 默认小端
    }

    public class IapProgress
    {
        public int CurrentFrame { get; set; }
        public int TotalFrames { get; set; }
        public long SentBytes { get; set; }
        public long TotalBytes { get; set; }
    }

    public class IapAck
    {
        public byte Cmd { get; set; }
        public byte Ack { get; set; }
        public byte Seq { get; set; }
    }
}
