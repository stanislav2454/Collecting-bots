public interface IItem
{
    public string ItemId { get; }
    public string ItemName { get; }
    public bool CanBeCollected { get; }

    public void OnCollected();
    public void OnRespawn();
}