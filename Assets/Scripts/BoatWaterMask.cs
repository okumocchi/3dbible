using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ボートの内側に水面マスク（深度書き込み専用メッシュ）を生成するコンポーネント。
/// 船底付近に不可視のクワッドを配置し、透明描画される水シェーダーが
/// デプステストで失敗するようにすることでボート内側の水を非表示にする。
/// </summary>
[RequireComponent(typeof(VehicleController))]
public class BoatWaterMask : MonoBehaviour
{
    [Tooltip("ボート内部の幅と長さ（X: 幅, Y: 奥行き）")]
    [SerializeField] private Vector2 hullSize = new Vector2(3f, 6f);

    [Tooltip("ボートのローカル原点からマスク平面のYオフセット（水面より少し上に設定）")]
    [SerializeField] private float hullYOffset = 0.15f;

    [Tooltip("ボート内側の塗りつぶし色（船底の素材に合わせて調整）")]
    [SerializeField] private Color hullColor = new Color(0.25f, 0.18f, 0.12f, 1f);

    private GameObject maskObject;

    void Start()
    {
        CreateDepthMask();
    }

    private void CreateDepthMask()
    {
        var shader = Shader.Find("Custom/BoatHullDepthMask");
        if (shader == null)
        {
            Debug.LogError($"[BoatWaterMask] Custom/BoatHullDepthMask シェーダーが見つかりません。Assets/Shaders/BoatHullDepthMask.shader が存在するか確認してください。", this);
            return;
        }

        maskObject = new GameObject("_WaterDepthMask");
        maskObject.transform.SetParent(transform, worldPositionStays: false);
        maskObject.transform.localPosition = new Vector3(0f, hullYOffset, 0f);
        maskObject.transform.localEulerAngles = Vector3.zero;
        maskObject.transform.localScale = Vector3.one;

        var meshFilter = maskObject.AddComponent<MeshFilter>();
        meshFilter.mesh = BuildQuadMesh(hullSize.x, hullSize.y);

        var mat = new Material(shader);
        mat.color = hullColor;

        var meshRenderer = maskObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    private static Mesh BuildQuadMesh(float width, float length)
    {
        float hw = width * 0.5f;
        float hl = length * 0.5f;

        var mesh = new Mesh { name = "BoatHullMaskQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-hw, 0f, -hl),
            new Vector3( hw, 0f, -hl),
            new Vector3(-hw, 0f,  hl),
            new Vector3( hw, 0f,  hl),
        };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        mesh.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.RecalculateBounds();
        return mesh;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Scene viewカメラのときだけ描画し、Game viewには出さない
        if (Camera.current != SceneView.lastActiveSceneView?.camera) return;

        Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
        Vector3 center = transform.position + transform.up * hullYOffset;
        Vector3 size = new Vector3(hullSize.x, 0.02f, hullSize.y);
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
#endif
}
