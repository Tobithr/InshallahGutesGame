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

    // ─── Arena (Dust-2 inspired layout) ─────────────────────────────────────
    //
    // Map overview (top = north / CT side):
    //
    //  Z=+100  ┌────────────────────────────────────────┐
    //          │            CT SPAWN                    │
    //  Z=+62   │  [ct→A open]  │wall│  [ct→B open]     │
    //          │   A  SITE     │A/B │   B  SITE         │
    //  Z=+20   ├───────────────┤sep ├───────────────────┤
    //          │ [LongA] ▓▓▓▓▓ │MID │ ▓▓▓▓▓ [BTunnel]  │
    //  Z=-50   ├───────────────┴────┴───────────────────┤
    //          │              T SPAWN                   │
    //  Z=-100  └────────────────────────────────────────┘
    //         -65  -45  -20       +20  +45             +65
    //
    // Long A  : X[-65,-45]   Mid : X[-20,+20]   B Tunnel : X[+45,+65]
    // Divider blocks (▓) fill X[-45,-20] and X[+20,+45] for Z[-50,+20]

    void BuildArena()
    {
        var arena = new GameObject("Arena");
        const float WH = 14f;   // wall height – taller than max jump (~10 u)

        Color sand    = new Color(0.78f, 0.68f, 0.48f);
        Color dSand   = new Color(0.52f, 0.44f, 0.28f);
        Color stone   = new Color(0.50f, 0.48f, 0.44f);
        Color crate   = new Color(0.66f, 0.58f, 0.42f);
        Color ctFloor = new Color(0.40f, 0.48f, 0.56f);

        // ── GROUND ──────────────────────────────────────────────────────────
        CreateBox(arena.transform, "Ground",
            Vector3.zero, new Vector3(132f, 1f, 202f), sand);
        // CT area gets a distinct floor colour
        CreateBox(arena.transform, "Ground_CT",
            new Vector3(10f, 0.06f, +80f), new Vector3(100f, 0.6f, 40f), ctFloor);

        // ── OUTER WALLS ─────────────────────────────────────────────────────
        CreateBox(arena.transform, "OW_S", new Vector3(  0f, WH/2,-101f), new Vector3(134f,WH,  2f), stone);
        CreateBox(arena.transform, "OW_N", new Vector3(  0f, WH/2,+101f), new Vector3(134f,WH,  2f), stone);
        CreateBox(arena.transform, "OW_W", new Vector3(-66f, WH/2,   0f), new Vector3(  2f,WH,204f), stone);
        CreateBox(arena.transform, "OW_E", new Vector3(+66f, WH/2,   0f), new Vector3(  2f,WH,204f), stone);

        // ── CORRIDOR DIVIDER BLOCKS ──────────────────────────────────────────
        // Solid block left  (between Long A and Mid):  X[-45,-20]
        CreateBox(arena.transform, "Div_L",
            new Vector3(-32.5f, WH/2, -15f), new Vector3(25f, WH, 70f), dSand);
        // Solid block right (between Mid and B Tunnel): X[+20,+45]
        CreateBox(arena.transform, "Div_R",
            new Vector3(+32.5f, WH/2, -15f), new Vector3(25f, WH, 70f), dSand);

        // ── A / B SITE SEPARATOR ────────────────────────────────────────────
        // Thick wall between sites: X[+5,+35], Z[+20,+70]
        CreateBox(arena.transform, "AB_Sep",
            new Vector3(+20f, WH/2, +45f), new Vector3(30f, WH, 50f), dSand);

        // ── A SITE CT BOUNDARY ──────────────────────────────────────────────
        // Opening X[-65,-38] for CT→A short push (27 units wide)
        // Wall from X[-38, +5]  →  center=-16.5, width=43
        CreateBox(arena.transform, "A_CT_Wall",
            new Vector3(-16.5f, WH/2, +62f), new Vector3(43f, WH, 2f), dSand);

        // ── CT SPAWN WEST WALL ───────────────────────────────────────────────
        // Runs from A_CT_Wall west end (-38) to north outer wall
        CreateBox(arena.transform, "CT_W",
            new Vector3(-38f, WH/2, +81f), new Vector3(2f, WH, 38f), stone);

        // ── COVER – A SITE ───────────────────────────────────────────────────
        CreateBox(arena.transform, "A_BigCrate",  new Vector3(-50f, 3f,   +40f), new Vector3( 8f,6f, 8f), crate);
        CreateBox(arena.transform, "A_SmCrate",   new Vector3(-38f, 2.5f, +33f), new Vector3( 5f,5f, 5f), crate);
        CreateBox(arena.transform, "A_Crate2",    new Vector3(-22f, 2.5f, +52f), new Vector3( 5f,5f, 5f), crate);
        CreateBox(arena.transform, "A_Truck",     new Vector3(-12f, 3.5f, +44f), new Vector3(14f,7f, 6f), stone);
        CreateBox(arena.transform, "A_Barrel",    new Vector3(-35f, 2.5f, +55f), new Vector3( 4f,5f, 4f), crate);

        // ── COVER – B SITE ───────────────────────────────────────────────────
        CreateBox(arena.transform, "B_BigCrate",  new Vector3(+50f, 2.5f, +38f), new Vector3( 8f,5f, 8f), crate);
        CreateBox(arena.transform, "B_Plat",      new Vector3(+58f, 3.5f, +54f), new Vector3(14f,7f,14f), dSand);
        CreateBox(arena.transform, "B_SmCrate1",  new Vector3(+44f, 2.5f, +50f), new Vector3( 5f,5f, 5f), crate);
        CreateBox(arena.transform, "B_SmCrate2",  new Vector3(+58f, 2.5f, +40f), new Vector3( 5f,5f, 5f), crate);

        // ── COVER – CORRIDORS ────────────────────────────────────────────────
        // Long A
        CreateBox(arena.transform, "LA_Box",      new Vector3(-56f, 2.5f, -15f), new Vector3(6f,5f,6f), crate);
        CreateBox(arena.transform, "LA_Wall",     new Vector3(-56f, 2f,   +10f), new Vector3(8f,4f,2f), dSand);
        // Mid
        CreateBox(arena.transform, "Mid_Box",     new Vector3(  0f, 2.5f, -10f), new Vector3(5f,5f,5f), crate);
        CreateBox(arena.transform, "Mid_Ledge",   new Vector3( -8f, 2f,   + 8f), new Vector3(3f,4f,6f), dSand);
        // B Tunnel
        CreateBox(arena.transform, "BT_Box",      new Vector3(+56f, 2.5f, -15f), new Vector3(6f,5f,6f), crate);
        // B Tunnel divider (lower / upper split at Z=-50 end)
        CreateBox(arena.transform, "BT_Div",      new Vector3(+55f, WH/2, -58f), new Vector3(22f,WH, 2f), dSand);

        // ── COVER – T SPAWN ──────────────────────────────────────────────────
        CreateBox(arena.transform, "T_Box1",      new Vector3(-22f, 2.5f, -78f), new Vector3(6f,5f,6f), crate);
        CreateBox(arena.transform, "T_Box2",      new Vector3(+22f, 2.5f, -78f), new Vector3(6f,5f,6f), crate);
        CreateBox(arena.transform, "T_Box3",      new Vector3(  0f, 2.5f, -88f), new Vector3(6f,5f,4f), crate);

        // ── COVER – CT SPAWN ─────────────────────────────────────────────────
        CreateBox(arena.transform, "CT_Box1",     new Vector3(-18f, 2.5f, +80f), new Vector3(6f,5f,6f), crate);
        CreateBox(arena.transform, "CT_Box2",     new Vector3(+28f, 2.5f, +80f), new Vector3(6f,5f,6f), crate);
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
        player.transform.position = new Vector3(0f, PlayerScale, -80f); // T spawn

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

        // Spread bots across the map; positions match the map areas
        Vector3[] positions =
        {
            new Vector3(-56f, PlayerScale, +40f),   // A site
            new Vector3(+52f, PlayerScale, +44f),   // B site
            new Vector3(  0f, PlayerScale, +82f),   // CT spawn
            new Vector3(-56f, PlayerScale, -15f),   // Long A corridor
            new Vector3(+56f, PlayerScale, -15f),   // B tunnel
            new Vector3(  0f, PlayerScale,  -8f),   // Mid
            new Vector3(-22f, PlayerScale, -78f),   // T spawn left
            new Vector3(+22f, PlayerScale, -78f),   // T spawn right
        };

        for (int i = 0; i < Mathf.Min(botCount, positions.Length); i++)
            CreateBot(names[i], positions[i]);
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
        // Spread spawn points across the map (T side, sites, CT)
        Vector3[] spawnPos =
        {
            new Vector3(  0f, PlayerScale, -80f),   // T centre
            new Vector3(-25f, PlayerScale, -75f),   // T left
            new Vector3(+25f, PlayerScale, -75f),   // T right
            new Vector3(-56f, PlayerScale, +40f),   // A site
            new Vector3(+52f, PlayerScale, +44f),   // B site
            new Vector3(  0f, PlayerScale, +82f),   // CT centre
            new Vector3(-25f, PlayerScale, +82f),   // CT left
            new Vector3(+25f, PlayerScale, +82f),   // CT right
        };

        var pts = new Transform[spawnPos.Length];
        for (int i = 0; i < spawnPos.Length; i++)
        {
            var sp = new GameObject($"Spawn_{i}");
            sp.transform.SetParent(parent);
            sp.transform.position = spawnPos[i];
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
