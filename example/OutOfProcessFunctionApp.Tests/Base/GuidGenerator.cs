using System.Numerics;

namespace OutOfProcessFunctionApp.Tests.Base;

internal class GuidGenerator
{
    private Guid _currentGuid;

    public GuidGenerator()
    {
        _currentGuid = new Guid(new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 });
    }

    public Guid GetGuid()
    {
        var guid = _currentGuid;

        var bigInt = new BigInteger(_currentGuid.ToByteArray());
        bigInt++;

        var byteArray = bigInt.ToByteArray();
        var paddedArray = new byte[16];
        Array.Copy(byteArray, paddedArray, byteArray.Length);

        _currentGuid = new Guid(paddedArray);

        return guid;
    }
}
