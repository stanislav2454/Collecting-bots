using UnityEngine;
using System;

public interface IItemService
{
    // Основные операции с предметами
    Item[] GetAllItems();
    Item[] GetAvailableItems();
    Item FindNearestAvailableItem(Vector3 position, float radius);
    Item FindBestItemForBot(Vector3 position, float radius, BotController bot);

    // Резервирование предметов (координация между ботами)
    bool TryReserveItem(Item item, BotController requester);
    void ReleaseItem(Item item);
    bool IsItemReserved(Item item);
    void ReleaseAllReservations();

    // Статистика
    int GetTotalItemsCount();
    int GetAvailableItemsCount();
    int GetReservedItemsCount();

    // События
    event Action<Item> OnItemReserved;
    event Action<Item> OnItemReleased;
    event Action<Item> OnItemCollected;
    event Action<Item> OnItemSpawned;
}