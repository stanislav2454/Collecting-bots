//using UnityEngine;
//using System;

//public interface IItemService
//{//после рефакторинга УДАЛИТЬ!
//    public event Action<Item> ItemReserved;
//    public event Action<Item> ItemReleased;
//    public event Action<Item> ItemCollected;
//    public event Action<Item> ItemSpawned;

//    public Item[] GetAllItems();
//    public Item[] GetAvailableItems();
//    public Item FindNearestAvailableItem(Vector3 position, float radius);
//    public Item FindBestItemForBot(Vector3 position, float radius, BotController bot);
//    public bool TryReserveItem(Item item, BotController requester);
//    public void ReleaseItem(Item item);
//    public bool IsItemReserved(Item item);
//    public void ReleaseAllReservations();
//    public int GetTotalItemsCount();
//    public int GetAvailableItemsCount();
//    public int GetReservedItemsCount();
//}