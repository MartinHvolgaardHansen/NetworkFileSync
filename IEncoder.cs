namespace NetworkFileSync
{
    interface IEncoder<T>
    {
        byte[] Encode(T entity);
        T Decode(byte[] data);
    }
}