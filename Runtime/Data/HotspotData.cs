namespace Treasured.UnitySdk
{
    public sealed class HotspotData : TreasuredObjectData
    {
        public HotspotData(Hotspot hotspot)
        {
            this._id = hotspot.Id;
            this._description = hotspot.Description;
            this._onSelected = hotspot.OnSelected;
        }
    }
}
