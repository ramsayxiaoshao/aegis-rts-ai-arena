using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AegisPresentationPrefabGenerator
{
    private const string ArtDirectory = "Assets/_Project/Art/Generated";
    private const string PrefabDirectory = "Assets/_Project/Prefabs/Presentation";
    private const string ResourceDirectory = "Assets/_Project/Resources";
    private const string MaterialDirectory = "Assets/_Project/Materials";
    private const string AudioDirectory = "Assets/_Project/Audio/Generated";
    private const string CircleAssetPath = ArtDirectory + "/Circle.png";
    private const string PlayerInfantryPath = "Assets/_Project/Art/Units/PlayerInfantry.png";
    private const string EnemyInfantryPath = "Assets/_Project/Art/Units/EnemyInfantry.png";
    private const string PlayerBasePath = "Assets/_Project/Art/Buildings/PlayerBase.png";
    private const string EnemyBasePath = "Assets/_Project/Art/Buildings/EnemyBase.png";
    private const string FactoryPath = "Assets/_Project/Art/Buildings/InfantryFactory.png";
    private const string CatalogAssetPath = ResourceDirectory + "/PresentationPrefabCatalog.asset";
    private const string GridMaterialPath = MaterialDirectory + "/GridLine.mat";
    private const string AttackClipPath = AudioDirectory + "/Attack.wav";
    private const string HitClipPath = AudioDirectory + "/Hit.wav";
    private const string ProductionClipPath = AudioDirectory + "/ProductionComplete.wav";

    [InitializeOnLoadMethod]
    private static void ScheduleUpgradeWhenNeeded()
    {
        EditorApplication.delayCall += () =>
        {
            PresentationPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<PresentationPrefabCatalog>(
                CatalogAssetPath
            );
            SpriteRenderer playerBaseRenderer = catalog != null && catalog.PlayerBasePrefab != null
                ? catalog.PlayerBasePrefab.GetComponent<SpriteRenderer>()
                : null;
            bool usesPlaceholder = playerBaseRenderer == null ||
                playerBaseRenderer.sprite == null ||
                playerBaseRenderer.sprite.name == "Circle";

            if (catalog == null ||
                usesPlaceholder ||
                catalog.AttackClip == null ||
                catalog.HitClip == null ||
                catalog.ProductionCompleteClip == null)
            {
                Generate();
            }
        };
    }

    [MenuItem("Aegis RTS/Generate Presentation Prefabs")]
    public static void Generate()
    {
        EnsureDirectory(ArtDirectory);
        EnsureDirectory(PrefabDirectory);
        EnsureDirectory(ResourceDirectory);
        EnsureDirectory(MaterialDirectory);
        EnsureDirectory(AudioDirectory);

        Sprite circleSprite = GenerateCircleSprite();
        Sprite playerInfantrySprite = LoadArtSprite(PlayerInfantryPath, circleSprite);
        Sprite enemyInfantrySprite = LoadArtSprite(EnemyInfantryPath, circleSprite);
        Sprite playerBaseSprite = LoadArtSprite(PlayerBasePath, circleSprite);
        Sprite enemyBaseSprite = LoadArtSprite(EnemyBasePath, circleSprite);
        Sprite factorySprite = LoadArtSprite(FactoryPath, circleSprite);
        Material gridMaterial = GetOrCreateGridMaterial();
        PresentationPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<PresentationPrefabCatalog>(
            CatalogAssetPath
        );

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<PresentationPrefabCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
        }

        catalog.PlayerBasePrefab = CreateEntityPrefab(
            "PlayerBaseView",
            playerBaseSprite,
            20,
            0.012f,
            0.35f,
            1.4f
        );
        catalog.EnemyBasePrefab = CreateEntityPrefab(
            "EnemyBaseView",
            enemyBaseSprite,
            20,
            0.012f,
            0.35f,
            1.4f
        );
        catalog.FactoryPrefab = CreateEntityPrefab(
            "FactoryView",
            factorySprite,
            20,
            0.018f,
            0.5f,
            1.8f
        );
        catalog.PlayerInfantryPrefab = CreateEntityPrefab(
            "PlayerInfantryView",
            playerInfantrySprite,
            25,
            0.035f,
            1.5f,
            3.2f
        );
        catalog.EnemyInfantryPrefab = CreateEntityPrefab(
            "EnemyInfantryView",
            enemyInfantrySprite,
            25,
            0.035f,
            1.5f,
            3.2f
        );
        catalog.CircleOverlayPrefab = CreateCirclePrefab("CircleOverlayView", circleSprite);
        catalog.GridLinePrefab = CreateGridLinePrefab(gridMaterial);
        catalog.AttackClip = GenerateAttackClip();
        catalog.HitClip = GenerateHitClip();
        catalog.ProductionCompleteClip = GenerateProductionCompleteClip();

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Aegis RTS presentation prefabs generated successfully.");
    }

    private static Sprite GenerateCircleSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear
        };
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = Vector2.Distance(new Vector2(x, y), center) <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        string absolutePath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            CircleAssetPath
        );
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(CircleAssetPath, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(CircleAssetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = size;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(CircleAssetPath);
    }

    private static Sprite LoadArtSprite(string assetPath, Sprite fallback)
    {
        if (!File.Exists(GetAbsoluteAssetPath(assetPath)))
        {
            Debug.LogWarning($"Authored sprite is missing at {assetPath}; using circle fallback.");
            return fallback;
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = Mathf.Max(texture.width, texture.height);
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static AudioClip GenerateAttackClip()
    {
        return GenerateAudioClip(AttackClipPath, 0.12f, t =>
        {
            float progress = t / 0.12f;
            float envelope = Mathf.Pow(1f - progress, 2f);
            float frequency = Mathf.Lerp(920f, 260f, progress);
            float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
            return wave * envelope * 0.72f;
        });
    }

    private static AudioClip GenerateHitClip()
    {
        return GenerateAudioClip(HitClipPath, 0.16f, t =>
        {
            float progress = t / 0.16f;
            float envelope = Mathf.Pow(1f - progress, 3f);
            float low = Mathf.Sin(2f * Mathf.PI * 115f * t);
            float metal = Mathf.Sin(2f * Mathf.PI * 347f * t) * 0.32f;
            float texture = Mathf.Sin(2f * Mathf.PI * 1733f * t) * 0.12f;
            return (low + metal + texture) * envelope * 0.75f;
        });
    }

    private static AudioClip GenerateProductionCompleteClip()
    {
        float[] notes = { 523.25f, 659.25f, 783.99f };

        return GenerateAudioClip(ProductionClipPath, 0.48f, t =>
        {
            const float noteDuration = 0.16f;
            int noteIndex = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t / noteDuration));
            float localTime = t - noteIndex * noteDuration;
            float noteEnvelope = Mathf.Sin(Mathf.Clamp01(localTime / noteDuration) * Mathf.PI);
            float wave = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t);
            float harmonic = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * 2f * t) * 0.2f;
            return (wave + harmonic) * noteEnvelope * 0.58f;
        });
    }

    private static AudioClip GenerateAudioClip(
        string assetPath,
        float duration,
        Func<float, float> sampleFunction
    )
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(duration * sampleRate);
        int dataLength = sampleCount * sizeof(short);

        using (MemoryStream stream = new MemoryStream(44 + dataLength))
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataLength);

            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                short sample = (short)(Mathf.Clamp(sampleFunction(time), -1f, 1f) * short.MaxValue);
                writer.Write(sample);
            }

            File.WriteAllBytes(GetAbsoluteAssetPath(assetPath), stream.ToArray());
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
    }

    private static string GetAbsoluteAssetPath(string assetPath)
    {
        return Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            assetPath
        );
    }

    private static Material GetOrCreateGridMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(GridMaterialPath);

        if (material != null)
        {
            return material;
        }

        material = new Material(Shader.Find("Sprites/Default"));
        AssetDatabase.CreateAsset(material, GridMaterialPath);
        return material;
    }

    private static GameObject CreateEntityPrefab(
        string prefabName,
        Sprite sprite,
        int sortingOrder,
        float idleScaleAmplitude,
        float idleRotationDegrees,
        float idleSpeed
    )
    {
        GameObject root = CreateCircleTemplate(prefabName, sprite, Color.white, sortingOrder);
        RtsEntityViewAnimator animator = root.AddComponent<RtsEntityViewAnimator>();
        animator.IdleScaleAmplitude = idleScaleAmplitude;
        animator.IdleRotationDegrees = idleRotationDegrees;
        animator.IdleSpeed = idleSpeed;
        return SavePrefab(root, prefabName);
    }

    private static GameObject CreateCirclePrefab(string prefabName, Sprite sprite)
    {
        GameObject root = CreateCircleTemplate(prefabName, sprite, Color.white, 0);
        return SavePrefab(root, prefabName);
    }

    private static GameObject CreateCircleTemplate(
        string objectName,
        Sprite sprite,
        Color color,
        int sortingOrder
    )
    {
        GameObject root = new GameObject(objectName);
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return root;
    }

    private static GameObject CreateGridLinePrefab(Material material)
    {
        GameObject root = new GameObject("GridLineView");
        LineRenderer renderer = root.AddComponent<LineRenderer>();
        renderer.sharedMaterial = material;
        renderer.positionCount = 2;
        renderer.startWidth = 0.025f;
        renderer.endWidth = 0.025f;
        renderer.startColor = new Color(1f, 1f, 1f, 0.15f);
        renderer.endColor = new Color(1f, 1f, 1f, 0.15f);
        renderer.sortingOrder = -10;
        return SavePrefab(root, "GridLineView");
    }

    private static GameObject SavePrefab(GameObject root, string prefabName)
    {
        string path = PrefabDirectory + "/" + prefabName + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static void EnsureDirectory(string assetPath)
    {
        string[] parts = assetPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
