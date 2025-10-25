using System.Collections.Generic;
using UnityEngine;

public class ResourceAssignmentManager
{
    private Dictionary<Item, Bot> _itemToBotMap = new Dictionary<Item, Bot>();
    private Dictionary<Bot, Item> _botToItemMap = new Dictionary<Bot, Item>();

    public bool TryAssignResourceToBot(Item resource, Bot bot)
    {
        Debug.Log($"🔧 ПЫТАЕМСЯ НАЗНАЧИТЬ РЕСУРС {resource?.name} БОТУ {bot?.name}");

        if (resource == null || bot == null)
        {
            Debug.Log($"❌ Невалидные параметры: ресурс={resource != null}, бот={bot != null}");
            return false;
        }

        if (_itemToBotMap.ContainsKey(resource))
        {
            Debug.Log($"❌ Ресурс {resource.name} уже назначен боту {_itemToBotMap[resource]?.name}");
            return false;
        }

        if (_botToItemMap.ContainsKey(bot))
        {
            Debug.Log($"❌ Бот {bot.name} уже имеет назначенный ресурс {_botToItemMap[bot]?.name}");
            return false;
        }

        Debug.Log($"🔍 Проверяем возможность резервирования ресурса {resource.name}...");
        if (ResourceManager.Instance.TryReserveResource(resource))
        {
            _itemToBotMap[resource] = bot;
            _botToItemMap[bot] = resource;
            Debug.Log($"✅ УСПЕШНО: Ресурс {resource.name} назначен боту {bot.name}");
            return true;
        }
        else
        {
            Debug.Log($"❌ Не удалось зарезервировать ресурс {resource.name} через ResourceManager");
            return false;
        }
    }

    public void CompleteAssignment(Bot bot, bool success)
    {
        Debug.Log($"🔧 ЗАВЕРШАЕМ НАЗНАЧЕНИЕ: Бот {bot?.name}, Успех: {success}");

        if (_botToItemMap.TryGetValue(bot, out Item resource))
        {
            _botToItemMap.Remove(bot);
            _itemToBotMap.Remove(resource);

            if (success)
            {
                Debug.Log($"✅ Ресурс {resource?.name} отмечен как собранный");
                ResourceManager.Instance.MarkAsCollected(resource);
            }
            else
            {
                Debug.Log($"🔄 Ресурс {resource?.name} освобожден");
                ResourceManager.Instance.ReleaseResource(resource);
            }

            Debug.Log($"🏁 Назначение завершено: бот {bot.name}, ресурс {resource?.name}");
        }
        else
        {
            Debug.Log($"⚠️ Бот {bot?.name} не найден в активных назначениях");
        }
    }

    public void ForceReleaseBotAssignment(Bot bot)
    {
        Debug.Log($"🔧 ПРИНУДИТЕЛЬНОЕ ОСВОБОЖДЕНИЕ БОТА: {bot?.name}");

        if (_botToItemMap.TryGetValue(bot, out Item resource))
        {
            _botToItemMap.Remove(bot);
            _itemToBotMap.Remove(resource);
            ResourceManager.Instance.ReleaseResource(resource);
            Debug.Log($"✅ Бот {bot.name} освобожден от ресурса {resource?.name}");
        }
        else
        {
            Debug.Log($"ℹ️ Бот {bot?.name} не имел активных назначений");
        }
    }

    public Item GetAssignedResource(Bot bot)
    {
        _botToItemMap.TryGetValue(bot, out Item resource);
        Debug.Log($"🔍 Запрошен назначенный ресурс для бота {bot?.name}: {resource?.name ?? "НЕТ"}");
        return resource;
    }

    public bool IsBotAssigned(Bot bot)
    {
        bool assigned = _botToItemMap.ContainsKey(bot);
        Debug.Log($"🔍 Бот {bot?.name} назначен: {assigned}");
        return assigned;
    }

    public bool IsResourceAssigned(Item resource)
    {
        bool assigned = _itemToBotMap.ContainsKey(resource);
        Debug.Log($"🔍 Ресурс {resource?.name} назначен: {assigned}");
        return assigned;
    }

    // Дополнительный метод для отладки
    public void DebugState()
    {
        Debug.Log($"=== ResourceAssignmentManager State ===");
        Debug.Log($"Активных назначений: {_botToItemMap.Count}");

        foreach (var kvp in _botToItemMap)
        {
            Debug.Log($"- Бот: {kvp.Key?.name} -> Ресурс: {kvp.Value?.name}");
        }
        Debug.Log($"=====================================");
    }
}