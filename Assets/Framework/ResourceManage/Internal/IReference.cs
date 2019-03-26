namespace Framework.ResourceManage
{
    internal interface IReference
    {
        void Retain();
        void Release();
    }
}
