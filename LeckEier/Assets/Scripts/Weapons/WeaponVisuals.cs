using UnityEngine;

// Builds a first-person weapon model from Unity primitives.
// Add this component to each weapon GameObject and set the type.
public class WeaponVisuals : MonoBehaviour
{
    public enum WeaponType { AssaultRifle, Shotgun, SniperRifle, Melee }
    public WeaponType weaponType;

    // Colors
    static readonly Color ColMetal      = new Color(0.20f, 0.20f, 0.22f);
    static readonly Color ColMetalLight = new Color(0.45f, 0.45f, 0.48f);
    static readonly Color ColSteel      = new Color(0.60f, 0.60f, 0.62f);
    static readonly Color ColWood       = new Color(0.38f, 0.22f, 0.10f);
    static readonly Color ColBlack      = new Color(0.10f, 0.10f, 0.10f);
    static readonly Color ColRed        = new Color(0.80f, 0.10f, 0.10f);
    static readonly Color ColScope      = new Color(0.05f, 0.05f, 0.08f);
    static readonly Color ColLens       = new Color(0.10f, 0.20f, 0.50f);
    static readonly Color ColBlade      = new Color(0.75f, 0.75f, 0.78f);
    static readonly Color ColHandle     = new Color(0.25f, 0.15f, 0.08f);
    static readonly Color ColOrange     = new Color(0.90f, 0.40f, 0.05f);

    void Awake()
    {
        Build();
    }

    public void Build()
    {
        // Remove old visuals
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("Visual_"))
                DestroyImmediate(child.gameObject);
        }

        switch (weaponType)
        {
            case WeaponType.AssaultRifle: BuildAR();      break;
            case WeaponType.Shotgun:      BuildShotgun(); break;
            case WeaponType.SniperRifle:  BuildSniper();  break;
            case WeaponType.Melee:        BuildMelee();   break;
        }
    }

    // ─── Assault Rifle ───────────────────────────────────────────────────────

    void BuildAR()
    {
        // Receiver / body
        AddBox("Visual_Body",     new Vector3( 0.00f,  0.00f, 0f), new Vector3(0.46f, 0.09f, 0.08f), ColMetal);
        // Barrel
        AddCylinder("Visual_Barrel", new Vector3( 0.28f,  0.02f, 0f), new Vector3(0.035f, 0.14f, 0.035f), ColMetalLight, Quaternion.Euler(0f, 0f, 90f));
        // Muzzle flash guard
        AddBox("Visual_Muzzle",   new Vector3( 0.44f,  0.02f, 0f), new Vector3(0.04f, 0.055f, 0.055f), ColBlack);
        // Hand guard
        AddBox("Visual_Guard",    new Vector3( 0.16f, -0.01f, 0f), new Vector3(0.14f, 0.06f, 0.075f), ColBlack);
        // Magazine
        AddBox("Visual_Mag",      new Vector3( 0.02f, -0.10f, 0f), new Vector3(0.055f, 0.13f, 0.052f), ColBlack);
        // Pistol grip
        AddBox("Visual_Grip",     new Vector3(-0.06f, -0.08f, 0f), new Vector3(0.044f, 0.10f, 0.048f), ColMetal);
        // Stock
        AddBox("Visual_Stock",    new Vector3(-0.28f, -0.01f, 0f), new Vector3(0.18f, 0.075f, 0.065f), ColMetal);
        // Stock cheek rest
        AddBox("Visual_Cheek",    new Vector3(-0.30f,  0.03f, 0f), new Vector3(0.12f, 0.030f, 0.058f), ColMetalLight);
        // Ejection port detail
        AddBox("Visual_Port",     new Vector3( 0.05f,  0.05f, 0f), new Vector3(0.06f, 0.012f, 0.082f), ColBlack);
        // Red dot sight base
        AddBox("Visual_SightBase",new Vector3( 0.08f,  0.055f,0f), new Vector3(0.08f, 0.022f, 0.040f), ColMetalLight);
        // Red dot lens
        AddBox("Visual_SightLens",new Vector3( 0.08f,  0.080f,0f), new Vector3(0.018f,0.020f, 0.030f), ColLens);
    }

    // ─── Shotgun ─────────────────────────────────────────────────────────────

    void BuildShotgun()
    {
        // Main body
        AddBox("Visual_Body",     new Vector3( 0.00f,  0.00f, 0f), new Vector3(0.40f, 0.10f, 0.09f), ColWood);
        // Double barrel
        AddCylinder("Visual_Barrel1", new Vector3(0.26f, 0.035f, 0.025f), new Vector3(0.038f, 0.17f, 0.038f), ColMetalLight, Quaternion.Euler(0f, 0f, 90f));
        AddCylinder("Visual_Barrel2", new Vector3(0.26f, 0.035f,-0.025f), new Vector3(0.038f, 0.17f, 0.038f), ColMetalLight, Quaternion.Euler(0f, 0f, 90f));
        // Barrel rib (top connection)
        AddBox("Visual_Rib",      new Vector3( 0.26f,  0.075f, 0f), new Vector3(0.22f, 0.008f, 0.048f), ColMetal);
        // Pump / forend
        AddBox("Visual_Pump",     new Vector3( 0.14f, -0.02f, 0f), new Vector3(0.14f, 0.065f, 0.100f), ColWood);
        // Trigger guard
        AddBox("Visual_Guard",    new Vector3(-0.02f, -0.07f, 0f), new Vector3(0.10f, 0.030f, 0.055f), ColMetal);
        // Pistol grip
        AddBox("Visual_Grip",     new Vector3(-0.06f, -0.09f, 0f), new Vector3(0.048f, 0.11f, 0.055f), ColWood);
        // Stock
        AddBox("Visual_Stock",    new Vector3(-0.26f, -0.01f, 0f), new Vector3(0.20f, 0.090f, 0.075f), ColWood);
        // Shell indicator (orange detail)
        AddBox("Visual_Shell",    new Vector3( 0.05f,  0.055f, 0f), new Vector3(0.04f, 0.020f, 0.025f), ColOrange);
    }

    // ─── Sniper Rifle ─────────────────────────────────────────────────────────

    void BuildSniper()
    {
        // Long receiver
        AddBox("Visual_Body",        new Vector3( 0.00f,  0.00f, 0f), new Vector3(0.52f, 0.080f, 0.070f), ColBlack);
        // Long thin barrel
        AddCylinder("Visual_Barrel", new Vector3( 0.36f,  0.01f, 0f), new Vector3(0.022f, 0.26f, 0.022f), ColSteel, Quaternion.Euler(0f, 0f, 90f));
        // Muzzle brake
        AddBox("Visual_Muzzle",      new Vector3( 0.52f,  0.01f, 0f), new Vector3(0.04f, 0.038f, 0.038f), ColMetalLight);
        // Scope body
        AddBox("Visual_ScopeBody",   new Vector3( 0.05f,  0.07f, 0f), new Vector3(0.22f, 0.042f, 0.042f), ColScope);
        // Scope adjustment turrets
        AddBox("Visual_TurretTop",   new Vector3( 0.04f,  0.097f,0f), new Vector3(0.022f,0.028f, 0.022f), ColScope);
        AddBox("Visual_TurretSide",  new Vector3( 0.04f,  0.07f, 0.035f), new Vector3(0.022f,0.022f,0.028f), ColScope);
        // Scope lenses
        AddBox("Visual_LensFront",   new Vector3( 0.15f,  0.070f,0f), new Vector3(0.014f,0.040f, 0.040f), ColLens);
        AddBox("Visual_LensRear",    new Vector3(-0.06f,  0.070f,0f), new Vector3(0.014f,0.032f, 0.032f), ColLens);
        // Magazine
        AddBox("Visual_Mag",         new Vector3( 0.02f, -0.095f,0f), new Vector3(0.050f, 0.120f, 0.048f), ColBlack);
        // Pistol grip
        AddBox("Visual_Grip",        new Vector3(-0.05f, -0.08f, 0f), new Vector3(0.042f, 0.100f, 0.048f), ColBlack);
        // Thumbhole stock
        AddBox("Visual_Stock",       new Vector3(-0.28f, -0.01f, 0f), new Vector3(0.22f, 0.070f, 0.060f), ColBlack);
        AddBox("Visual_StockHole",   new Vector3(-0.26f,  0.00f, 0f), new Vector3(0.12f, 0.030f, 0.065f), new Color(0,0,0,0)); // cutout (invisible)
        // Cheek rest
        AddBox("Visual_Cheek",       new Vector3(-0.24f,  0.04f, 0f), new Vector3(0.14f, 0.025f, 0.055f), ColBlack);
        // Bipod legs
        AddBox("Visual_BipodArm",    new Vector3( 0.30f, -0.04f, 0f), new Vector3(0.012f,0.050f, 0.060f), ColMetal);
        AddBox("Visual_BipodFoot1",  new Vector3( 0.30f, -0.065f, 0.025f), new Vector3(0.010f,0.010f,0.040f), ColMetal);
        AddBox("Visual_BipodFoot2",  new Vector3( 0.30f, -0.065f,-0.025f), new Vector3(0.010f,0.010f,0.040f), ColMetal);
    }

    // ─── Melee / Knife ────────────────────────────────────────────────────────

    void BuildMelee()
    {
        // Blade
        AddBox("Visual_Blade",       new Vector3( 0.15f,  0.015f, 0f), new Vector3(0.26f, 0.032f, 0.010f), ColBlade);
        // Blade edge bevel
        AddBox("Visual_BladeEdge",   new Vector3( 0.28f,  0.002f, 0f), new Vector3(0.040f, 0.012f, 0.009f), ColSteel);
        // Blood groove
        AddBox("Visual_Groove",      new Vector3( 0.12f,  0.018f, 0f), new Vector3(0.18f, 0.006f, 0.003f), ColMetalLight);
        // Guard / crossguard
        AddBox("Visual_Guard",       new Vector3( 0.00f,  0.010f, 0f), new Vector3(0.030f, 0.085f, 0.018f), ColMetal);
        // Handle
        AddBox("Visual_Handle",      new Vector3(-0.09f,  0.00f,  0f), new Vector3(0.140f, 0.040f, 0.030f), ColHandle);
        // Handle wrapping detail
        AddBox("Visual_Wrap1",       new Vector3(-0.06f,  0.00f,  0f), new Vector3(0.015f, 0.042f, 0.032f), ColBlack);
        AddBox("Visual_Wrap2",       new Vector3(-0.10f,  0.00f,  0f), new Vector3(0.015f, 0.042f, 0.032f), ColBlack);
        AddBox("Visual_Wrap3",       new Vector3(-0.14f,  0.00f,  0f), new Vector3(0.015f, 0.042f, 0.032f), ColBlack);
        // Pommel
        AddBox("Visual_Pommel",      new Vector3(-0.17f,  0.00f,  0f), new Vector3(0.030f, 0.050f, 0.028f), ColMetal);
        // Red accent stripe
        AddBox("Visual_Accent",      new Vector3( 0.10f,  0.036f, 0f), new Vector3(0.08f, 0.006f, 0.011f), ColRed);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    GameObject AddBox(string partName, Vector3 localPos, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = partName;
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;
        ApplyColor(go, color);
        RemoveCollider(go);
        return go;
    }

    GameObject AddCylinder(string partName, Vector3 localPos, Vector3 localScale, Color color, Quaternion localRot)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = partName;
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale = localScale;
        ApplyColor(go, color);
        RemoveCollider(go);
        return go;
    }

    void ApplyColor(GameObject go, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        // Make metals slightly metallic
        mat.SetFloat("_Metallic", color == ColBlade || color == ColSteel || color == ColMetalLight ? 0.7f : 0.2f);
        mat.SetFloat("_Smoothness", 0.4f);
        go.GetComponent<Renderer>().material = mat;
    }

    void RemoveCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
    }
}
