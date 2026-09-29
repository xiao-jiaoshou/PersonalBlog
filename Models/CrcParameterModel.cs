namespace PersonalBlog.Models
{
    public class CrcParameterModel
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public ulong Polynomial { get; set; }
        public ulong InitialValue { get; set; }
        public bool ReflectInput { get; set; }
        public bool ReflectOutput { get; set; }
        public ulong XorOut { get; set; }
        public string HexPolynomial => Polynomial.ToString("X");
        public string HexInitialValue => InitialValue.ToString("X");
        public string HexXorOut => XorOut.ToString("X");
    }

    public class CrcResult
    {
        public string AlgorithmName { get; set; }
        public string HexValue { get; set; }
        public string DecValue { get; set; }
        public string BinaryValue { get; set; }
    }
}
