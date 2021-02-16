namespace Mirage.Cloud.ListServerService
{
    public abstract class ListServerBaseApi : BaseApi
    {
        protected ListServerBaseApi(ICoroutineRunner runner, IRequestCreator requestCreator) : base(runner, requestCreator)
        {
        }
    }
}
