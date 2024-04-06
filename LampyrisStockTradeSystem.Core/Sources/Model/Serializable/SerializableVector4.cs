using CsvHelper.Configuration;
using System.Numerics;
using System.Runtime.Serialization;

namespace LampyrisStockTradeSystem;

[Serializable]
public class SerializableVector4 : ISerializable
{
    private Vector4 vector;

    public float X { get => vector.X; set => vector.X = value; }
    public float Y { get => vector.Y; set => vector.Y = value; }
    public float Z { get => vector.Z; set => vector.Z = value; }
    public float W { get => vector.W; set => vector.W = value; }

    public SerializableVector4(Vector4 vector)
    {
        this.vector = vector;
    }

    public SerializableVector4(float x, float y, float z, float w)
    {
        this.vector = new Vector4(x,y,z,w);
    }

    protected SerializableVector4(SerializationInfo info, StreamingContext context)
    {
        float x = info.GetSingle("X");
        float y = info.GetSingle("Y");
        float z = info.GetSingle("Z");
        float w = info.GetSingle("W");
        vector = new Vector4(x, y, z, w);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("X", vector.X);
        info.AddValue("Y", vector.Y);
        info.AddValue("Z", vector.Z);
        info.AddValue("W", vector.W);
    }

    public Vector4 Vector
    {
        get { return vector; }
    }

    // 定义从ClassA到ClassB的显式转换
    public static implicit operator Vector4(SerializableVector4 vec4)
    {
        return new Vector4(vec4.X, vec4.Y, vec4.Z, vec4.W);
    }
}
