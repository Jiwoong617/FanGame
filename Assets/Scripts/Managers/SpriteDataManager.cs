using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDataManager
{
    private Dictionary<Enum, Sprite> spriteCache = new Dictionary<Enum, Sprite>();

    public void Init()
    {
        spriteCache.Clear();
    }

    public Sprite GetSprite<TEnum>(TEnum enumValue, string folderPath) where TEnum : Enum
    {
        if (spriteCache.TryGetValue(enumValue, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        string key = $"Sprites/{folderPath}/{enumValue.ToString()}";
        Sprite loadedSprite = Resources.Load<Sprite>(key);

        if (loadedSprite != null)
        {
            spriteCache.Add(enumValue, loadedSprite);
        }
        else
        {
            Debug.LogWarning($"[SpriteDataManager] 이미지를 찾을 수 없습니다: Resources/{key}");
        }

        return loadedSprite;
    }
}