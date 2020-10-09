using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
[DataContract(Name = "SerializableColor32", Namespace = "", IsReference = true)]
public class SerializableColor32
{
    [DataMember(Name = "r", Order = 0)]
    public byte r
    {
        get;
        set;
    }

    [DataMember(Name = "g", Order = 1)]
    public byte g
    {
        get;
        set;
    }

    [DataMember(Name = "b", Order = 2)]
    public byte b
    {
        get;
        set;
    }

    [DataMember(Name = "a", Order = 3)]
    public byte a
    {
        get;
        set;
    }

    public static implicit operator Color32(SerializableColor32 color)
    {
        return new Color32(color.r, color.g, color.b, color.a);
    }

}
