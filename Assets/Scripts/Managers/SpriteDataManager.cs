using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDataManager
{
    private Dictionary<Enum, Sprite> spriteCache = new Dictionary<Enum, Sprite>();
    private Dictionary<string, Sprite> stringCache = new Dictionary<string, Sprite>();

    public void Init()
    {
        spriteCache.Clear();
        stringCache.Clear();
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

    public Sprite GetSprite(string key, string folderPath)
    {
        string cacheKey = $"{folderPath}/{key}";
        if (stringCache.TryGetValue(cacheKey, out Sprite cachedSprite))
            return cachedSprite;

        string path = $"Sprites/{folderPath}/{key}";
        Sprite loadedSprite = Resources.Load<Sprite>(path);

        if (loadedSprite != null)
            stringCache.Add(cacheKey, loadedSprite);
        else
            Debug.LogWarning($"[SpriteDataManager] 이미지를 찾을 수 없습니다: Resources/{path}");

        return loadedSprite;
    }
}