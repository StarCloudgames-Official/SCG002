// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("klko7IL1aIpNxk6ot4FVRpljjPHMKYZ4L0mp3UUH33MPbaeAsPy3Huf++KwGxB3NaqmVt9xmoxCh/c/D62hmaVnraGNr62hoadEH2eibnJpDutMFkUvCnslVaIHhK+5SpyOSDexjWbN1iIRxBHqOg0ma5iSLpym9WetoS1lkb2BD7yHvnmRoaGhsaWrU5Hjih2f8YUTLSjbIqhXtjVH2bkn9QhTr04f4I76hsx0keE+LzSEXOT1EAtL0N4QkP/O2R8pzjy05U8nCJxIx4MxiKpLgZgwE8ZPKhBlqOP1PzbyMH0X7Fr4f1XV4oLrKmTSRagvPIyQgn8Tv3leiL7K+HMs774AQYe2E1KnS0FaaV6nfqEMLUdwuBDIkWNeGFF2g9mtqaGlo");
        private static int[] order = new int[] { 5,13,6,13,6,9,9,12,8,10,12,12,13,13,14 };
        private static int key = 105;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
