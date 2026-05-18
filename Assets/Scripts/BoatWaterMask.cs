using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ボートの内側にステンシルマスク用の不可視クワッドを生成するコンポーネント。
/// Geometry-1 キューでステンシルバッファに 1 を書き込み、
/// 水シェーダー（_StencilComp = NotEqual 1）がその領域をスキップすることで
/// ボート内側の水を非表示にする。
/// </summary>
[RequireComponent(typeof(VehicleController))]
public class BoatWaterMask : MonoBehaviour
{
    [Tooltip("ボート内部の幅と長さ（X: 幅, Y: 奥行き）")]
    [SerializeField] private Vector2 hullSize = new Vector2(3f, 6f);

    [Tooltip("ボートのローカル原点からマスク平面のYオフセット（水面より少し上に設定）")]
    [SerializeField] private float hullYOffset = 0.15f;

    private GameObject maskObject;

    void Start()
    {
        CreateStencilMask();
    }

    private void CreateStencilMask()
    {
        var shader = Shader.Find("Custom/BoatHullStencilMask");
        if (shader == null)
        {
            Debug.LogError("[BoatWaterMask] Custom/BoatHullStencilMask シェーダーが見つかりません。Assets/Shaders/BoatHullDepthMask.shader が存在するか確認してください。", this);
            return;
        }

        maskObject = new GameObject("_WaterStencilMask");
        maskObject.transform.SetParent(transform, worldPositionStays: false);
        maskObject.transform.localPosition = new Vector3(0f, hullYOffset, 0f);
        maskObject.transform.localEulerAngles = Vector3.zero;
        maskObject.transform.localScale = Vector3.one;

        var meshFilter = maskObject.AddComponent<MeshFilter>();
        meshFilter.mesh = BuildQuadMesh(hullSize.x, hullSize.y);

        var meshRenderer = maskObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(shader);
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    private static Mesh BuildQuadMesh(float width, float length)
    {
        float hw = width * 0.5f;
        float hl = length * 0.5f;

        var mesh = new Mesh { name = "BoatHullStencilQuad" };
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
