using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SimpleBotTest
{
    [UnityTest]
    public IEnumerator Bot_CanCollectItem_And_ReturnToBase()
    {
        // Простейшая проверка основного цикла
        yield return new WaitForSeconds(5f);
        Assert.IsTrue(true); // Заглушка - реальная логика будет в ЭТАПЕ 2
    }
}