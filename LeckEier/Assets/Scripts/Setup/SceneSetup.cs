#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Run via: Right-click SceneSetup component in Inspector → "Build REDMATCH Scene"
// Builds the entire playable scene from scratch.
[ExecuteInEditMode]
public class SceneSetup : MonoBehaviour
{
    [Header("Map")]
    public Vector3 arenaSize = new Vector3(60f, 20f, 60f);
    public int platformCount = 12;
    public int botCount = 4;

    [ContextMenu("Build REDMATCH Scene")]
    public void BuildScene()
    {
        ClearScene();
        SetupLayers();
        BuildArena();
        CreatePlayer();
        CreateBots();
        CreateManagers();
        CreateHUD();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[SceneSetup] REDMATCH scene built! Press Play to test.");
#endif
    }

    void ClearScene()
    {
        // Remove old setup objects (keep this SceneSetup object)
        var oldPlayer = GameObject.Find("Player");
        if (oldPlayer != null) DestroyImmediate(oldPlayer);

        var oldArena = GameObject.Find("Arena");
        if (oldArena != null) DestroyImmediate(oldArena);

        var oldManagers = GameObject.Find("Managers");
        if (oldManagers != null) DestroyImmediate(oldManagers);

        var oldHUD = GameObject.Find("HUD");
        if (oldHUD != null) DestroyImmediate(oldHUD);

        foreach (var bot in FindObjectsByType<BotController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            DestroyImmediate(bot.gameObject);
    }

    void SetupLayers()
    {
        // Layers must be configured manually in Edit > Project Settings > Tags and Layers
        // Layer 6: Ground, Layer 7: Player, Layer 8: Enemy
        Debug.Log("[SceneSetup] Reminder: Set up layers in Project Settings:\n" +
                  "  Layer 6 = Ground\n  Layer 7 = Player\n  Layer 8 = Enemy");
    }

    // ─── Arena ───────────────────────────────────────────────────────────────

    void BuildArena()
    {
        var arena = new GameObject("Arena");

        // Floor
        CreateBox(arena.transform, "Floor",
            new Vector3(0f, 0f, 0f),
            new Vector3(arenaSize.x, 0.5f, arenaSize.z),
            new Color(0.2f, 0.2f, 0.25f));

        // Walls
        float hw = arenaSize.x / 2f + 0.5f;
        float hd = arenaSize.z / 2f + 0.5f;
        float wh = arenaSize.y;
        CreateBox(arena.transform, "WallN", new Vector3(0f, wh/2f, hd), new Vector3(arenaSize.x + 1f, wh, 1f), Color.gray);
        CreateBox(arena.transform, "WallS", new Vector3(0f, wh/2f,-hd), new Vector3(arenaSize.x + 1f, wh, 1f), Color.gray);
        CreateBox(arena.transform, "WallE", new Vector3( hw, wh/2f, 0f), new Vector3(1f, wh, arenaSize.z + 1f), Color.gray);
        CreateBox(arena.transform, "WallW", new Vector3(-hw, wh/2f, 0f), new Vector3(1f, wh, arenaSize.z + 1f), Color.gray);

        // Platforms (randomized layout)
        var rng = new System.Random(42);
        for (int i = 0; i < platformCount; i++)
        {
            float x = (float)(rng.NextDouble() * arenaSize.x - arenaSize.x / 2f) * 0.8f;
            float z = (float)(rng.NextDouble() * arenaSize.z - arenaSize.z / 2f) * 0.8f;
            float y = (float)(rng.NextDouble() * arenaSize.y * 0.6f) + 1.5f;
            float w = (float)(rng.NextDouble() * 6f) + 3f;
            float d = (float)(rng.NextDouble() * 6f) + 3f;

            float hue = (float)i / platformCount;
            Color c = Color.HSVToRGB(hue, 0.3f, 0.45f);
            CreateBox(arena.transform, $"Platform_{i}", new Vector3(x, y, z), new Vector3(w, 0.4f, d), c);
        }

        // Central tower
        CreateBox(arena.transform, "Tower_Base",  new Vector3(0, 2,  0), new Vector3(6, 4, 6),  new Color(0.3f,0.15f,0.15f));
        CreateBox(arena.transform, "Tower_Mid",   new Vector3(0, 6,  0), new Vector3(4, 4, 4),  new Color(0.35f,0.1f,0.1f));
        CreateBox(arena.transform, "Tower_Top",   new Vector3(0, 10, 0), new Vector3(3, 4, 3),  new Color(0.4f,0.05f,0.05f));
    }

    GameObject CreateBox(Transform parent, string boxName, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = boxName;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.layer = LayerMask.NameToLayer("Default");

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        go.GetComponent<Renderer>().material = mat;

        return go;
    }

    // ─── Player ──────────────────────────────────────────────────────────────

    // Player is 2.5× real-world scale (5 units = ~2m, feels right in this arena)
    const float PlayerScale = 2.5f;

    void CreatePlayer()
    {
        // Root at scale (1,1,1) so CC dimensions and child positions are in world space
        var player = new GameObject("Player");
        player.layer = 7;
        player.transform.position = new Vector3(0f, PlayerScale, -20f);

        // Visual capsule body as child — purely cosmetic, no collider
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(player.transform);
        body.transform.localPosition = new Vector3(0f, PlayerScale, 0f); // match CC center
        body.transform.localScale    = Vector3.one * PlayerScale;
        DestroyImmediate(body.GetComponent<CapsuleCollider>());
        var bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bodyMat.color = new Color(0.15f, 0.35f, 0.8f);
        body.GetComponent<Renderer>().material = bodyMat;

        var cc = player.AddComponent<CharacterController>();
        cc.height     = 2f   * PlayerScale;
        cc.radius     = 0.4f * PlayerScale;
        cc.center     = Vector3.up * PlayerScale;
        cc.skinWidth  = 0.08f * PlayerScale;
        cc.stepOffset = 0.4f  * PlayerScale;

        var ctrl = player.AddComponent<PlayerController>();
        ctrl.groundMask        = LayerMask.GetMask("Default");
        ctrl.normalHeight      = 2f   * PlayerScale;
        ctrl.crouchHeight      = 1f   * PlayerScale;
        ctrl.normalCenter      = 1f   * PlayerScale;
        ctrl.crouchCenter      = 0.5f * PlayerScale;
        ctrl.groundCheckRadius = 0.35f * PlayerScale;
        ctrl.walkSpeed         = 9f   * PlayerScale;
        ctrl.jumpForce         = 4f   * PlayerScale;
        ctrl.gravity           = -30f * PlayerScale;

        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerStats>();

        // Camera rig – eye/face height (top of CC is at 2×PlayerScale, eye at 1.7×)
        var camRig = new GameObject("CameraRig");
        camRig.transform.SetParent(player.transform);
        camRig.transform.localPosition = new Vector3(0f, 1.7f * PlayerScale, 0f);

        var camGo = new GameObject("MainCamera");
        camGo.transform.SetParent(camRig.transform);
        camGo.transform.localPosition = Vector3.zero;
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 90f;
        cam.nearClipPlane = 0.05f;
        camGo.AddComponent<AudioListener>();

        var look = camGo.AddComponent<PlayerLook>();
        look.playerBody   = player.transform;
        look.cameraHolder = camRig.transform;

        var grapple = camGo.AddComponent<GrappleHook>();
        grapple.grappleMask  = LayerMask.GetMask("Default");
        grapple.maxDistance  = 50f * PlayerScale;
        grapple.pullSpeed    = 18f * PlayerScale;

        var primaryFP = new GameObject("PrimaryFirePoint");
        primaryFP.transform.SetParent(camGo.transform);
        primaryFP.transform.localPosition = new Vector3(0.2f, -0.1f, 0.5f);
        grapple.primaryFirePoint = primaryFP.transform;

        var secondaryFP = new GameObject("SecondaryFirePoint");
        secondaryFP.transform.SetParent(camGo.transform);
        secondaryFP.transform.localPosition = new Vector3(-0.2f, -0.1f, 0.5f);
        grapple.secondaryFirePoint = secondaryFP.transform;

        // Weapon holder – camera-relative, so no scale needed
        var weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(camGo.transform);
        weaponHolder.transform.localPosition = new Vector3(0.28f, -0.28f, 0.45f);
        weaponHolder.transform.localRotation = Quaternion.identity;

        var wm = player.AddComponent<WeaponManager>();
        wm.weapons    = new WeaponBase[4];
        wm.weapons[0] = CreateWeapon<AssaultRifle>(weaponHolder.transform, "AssaultRifle", WeaponVisuals.WeaponType.AssaultRifle);
        wm.weapons[1] = CreateWeapon<Shotgun>(weaponHolder.transform,      "Shotgun",      WeaponVisuals.WeaponType.Shotgun);
        wm.weapons[2] = CreateWeapon<SniperRifle>(weaponHolder.transform,  "SniperRifle",  WeaponVisuals.WeaponType.SniperRifle);
        wm.weapons[3] = CreateWeapon<MeleeWeapon>(weaponHolder.transform,  "Melee",        WeaponVisuals.WeaponType.Melee);
    }

    T CreateWeapon<T>(Transform parent, string wName, WeaponVisuals.WeaponType visualType) where T : WeaponBase
    {
        var go = new GameObject(wName);
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        // Barrel was built along local +X; rotate so barrel faces camera-forward (+Z)
        go.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        // Scale weapons to be clearly visible in first-person view
        go.transform.localScale    = new Vector3(1.4f, 1.4f, 1.4f);

        var visuals = go.AddComponent<WeaponVisuals>();
        visuals.weaponType = visualType;
        visuals.Build();

        go.SetActive(false);
        return go.AddComponent<T>();
    }

    // ─── Bots ────────────────────────────────────────────────────────────────

    void CreateBots()
    {
        string[] names = { "BotAlpha", "BotBeta", "BotGamma", "BotDelta",
                           "BotEpsilon", "BotZeta", "BotEta", "BotTheta" };

        for (int i = 0; i < Mathf.Min(botCount, names.Length); i++)
        {
            float angle = (360f / botCount) * i;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * 18f;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * 18f;

            CreateBot(names[i], new Vector3(x, PlayerScale, z));
        }
    }

    void CreateBot(string botName, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = botName;
        // Spawn so capsule bottom (pos.y - PlayerScale) sits on floor top (0.25)
        go.transform.position  = new Vector3(pos.x, PlayerScale + 0.25f, pos.z);
        go.transform.localScale = Vector3.one * PlayerScale;
        go.layer = 8;
        DestroyImmediate(go.GetComponent<CapsuleCollider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.8f, 0.15f, 0.15f);
        go.GetComponent<Renderer>().material = mat;

        // CC values are in local space; Unity scales them by localScale (PlayerScale)
        // so height=2 → world 5, radius=0.4 → world 1
        var cc = go.AddComponent<CharacterController>();
        cc.height     = 2f;
        cc.radius     = 0.4f;
        cc.center     = Vector3.zero;
        cc.skinWidth  = 0.08f;
        cc.stepOffset = 0.3f;

        var dmg = go.AddComponent<DamageReceiver>();
        dmg.maxHealth = 100f;

        var bot = go.AddComponent<BotController>();
        bot.botName        = botName;
        bot.attackRange    = 15f * PlayerScale;
        bot.detectionRange = 25f * PlayerScale;
        bot.moveSpeed      = 5f  * PlayerScale;
        bot.gravity        = -30f * PlayerScale;
    }

    // ─── Managers ────────────────────────────────────────────────────────────

    void CreateManagers()
    {
        var managers = new GameObject("Managers");

        var gm = managers.AddComponent<GameManager>();
        gm.killLimit = 20;
        gm.infiniteTime = false;

        var sm = managers.AddComponent<SpawnManager>();
        var spawnRoot = new GameObject("SpawnPoints");
        spawnRoot.transform.SetParent(managers.transform);
        sm.spawnPoints = CreateSpawnPoints(spawnRoot.transform);
    }

    Transform[] CreateSpawnPoints(Transform parent)
    {
        var pts = new Transform[8];
        for (int i = 0; i < 8; i++)
        {
            float angle = (360f / 8) * i;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * 22f;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * 22f;
            var sp = new GameObject($"Spawn_{i}");
            sp.transform.SetParent(parent);
            sp.transform.position = new Vector3(x, PlayerScale + 0.5f, z);
            pts[i] = sp.transform;
        }
        return pts;
    }

    // ─── HUD ─────────────────────────────────────────────────────────────────

    void CreateHUD()
    {
        var canvasGo = new GameObject("HUD");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        var hud = canvasGo.AddComponent<HUDManager>();
        canvasGo.AddComponent<KillFeedUI>();
        canvasGo.AddComponent<StatUpgradeUI>();

        BuildCrosshair(canvasGo.transform, hud);

        // Weapon name – bottom right
        hud.weaponNameText = CreateHUDText(canvasGo.transform, "WeaponName", "Assault Rifle",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15f, 70f), 22f, TextAlignmentOptions.Right);

        // Ammo – bottom right, below weapon name
        hud.ammoText = CreateHUDText(canvasGo.transform, "AmmoText", "30 / 90",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15f, 38f), 18f, TextAlignmentOptions.Right);

        // Speedometer – bottom left
        hud.speedText = CreateHUDText(canvasGo.transform, "SpeedText", "0 u/s",
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(15f, 20f), 14f, TextAlignmentOptions.Left);

        // Kill points – top right
        hud.killPointsText = CreateHUDText(canvasGo.transform, "KPText", "KP: 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15f, -30f), 18f, TextAlignmentOptions.Right);

        Debug.Log("[SceneSetup] HUD built.");
    }

    void BuildCrosshair(Transform parent, HUDManager hud)
    {
        var root = new GameObject("Crosshair");
        root.transform.SetParent(parent);
        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = rt.sizeDelta = Vector2.zero;

        // Center dot: black background + white fill
        CreateXhairImg(root.transform, "DotBG", Vector2.zero, new Vector2(8f, 8f), Color.black);
        hud.crosshairDot = CreateXhairImg(root.transform, "Dot", Vector2.zero, new Vector2(4f, 4f), Color.white);

        // 4 lines: each has a black outline layer behind the white fill
        var lines = new Image[4];
        CreateXhairImg(root.transform, "TopBG",   new Vector2( 0f,  11f), new Vector2(4f, 10f), Color.black);
        lines[0] = CreateXhairImg(root.transform, "Top",   new Vector2( 0f,  11f), new Vector2(2f, 8f), Color.white);
        CreateXhairImg(root.transform, "BotBG",   new Vector2( 0f, -11f), new Vector2(4f, 10f), Color.black);
        lines[1] = CreateXhairImg(root.transform, "Bot",   new Vector2( 0f, -11f), new Vector2(2f, 8f), Color.white);
        CreateXhairImg(root.transform, "LeftBG",  new Vector2(-11f,  0f), new Vector2(10f, 4f), Color.black);
        lines[2] = CreateXhairImg(root.transform, "Left",  new Vector2(-11f,  0f), new Vector2(8f, 2f), Color.white);
        CreateXhairImg(root.transform, "RightBG", new Vector2( 11f,  0f), new Vector2(10f, 4f), Color.black);
        lines[3] = CreateXhairImg(root.transform, "Right", new Vector2( 11f,  0f), new Vector2(8f, 2f), Color.white);

        hud.crosshairLines = lines;
    }

    Image CreateXhairImg(Transform parent, string n, Vector2 pos, Vector2 size, Color col)
    {
        var go = new GameObject(n);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = col;
        return img;
    }

    TextMeshProUGUI CreateHUDText(Transform parent, string n, string text,
        Vector2 anchor, Vector2 pivot, Vector2 pos, float size, TextAlignmentOptions align)
    {
        var go = new GameObject(n);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300f, 40f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = Color.white;
        tmp.alignment = align;
        return tmp;
    }
}
