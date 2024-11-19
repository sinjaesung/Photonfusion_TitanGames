using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    Grapple,
    Glide,
    W,
    S,
    A,
    D
}

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
}