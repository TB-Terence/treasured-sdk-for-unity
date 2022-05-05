namespace Treasured.UnitySdk
{
    public interface IExportHandler
    {
        public void OnPreExport();
        public void Export();
        public void OnPostExport();
    }
}
