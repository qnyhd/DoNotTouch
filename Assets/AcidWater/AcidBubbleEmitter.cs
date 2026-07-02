using System.Collections.Generic;
using UnityEngine;

public class AcidBubbleEmitter : MonoBehaviour
{
    [Header("Surface")]
    public Transform acidSurface;
    public Vector2 surfaceSize = new Vector2(8f, 8f);
    public float surfaceYOffset = 0.03f;

    [Header("Materials")]
    public Material bubbleMaterial;
    public Material ringMaterial;

    [Header("Spawn")]
    public float spawnRate = 3.5f;
    public int maxActiveBubbles = 30;

    [Header("Bubble Size")]
    public Vector2 bubbleSizeRange = new Vector2(0.12f, 0.38f);
    public Vector2 bubbleLifetimeRange = new Vector2(1.0f, 2.0f);

    [Header("Bubble Motion")]
    public float riseHeight = 0.08f;
    public float domeHeightScale = 0.45f;

    [Header("Pop Ring")]
    public float ringLifetime = 0.45f;
    public float ringStartScale = 0.5f;
    public float ringEndScale = 1.8f;

    private float spawnTimer;
    private readonly List<BubbleInstance> bubbles = new List<BubbleInstance>();
    private readonly List<RingInstance> rings = new List<RingInstance>();

    private Mesh ringMesh;
    private MaterialPropertyBlock block;

    private class BubbleInstance
    {
        public GameObject obj;
        public Renderer renderer;
        public Vector3 startPosition;
        public float baseSize;
        public float lifetime;
        public float age;
        public bool popped;
    }

    private class RingInstance
    {
        public GameObject obj;
        public Renderer renderer;
        public Vector3 startPosition;
        public float startScale;
        public float endScale;
        public float lifetime;
        public float age;
    }

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        ringMesh = CreateHorizontalQuadMesh();

        if (acidSurface == null)
            acidSurface = transform;
    }

    private void Update()
    {
        SpawnUpdate();
        BubbleUpdate();
        RingUpdate();
    }

    private void SpawnUpdate()
    {
        if (bubbleMaterial == null || ringMaterial == null || acidSurface == null)
            return;

        spawnTimer += Time.deltaTime * spawnRate;

        while (spawnTimer >= 1f)
        {
            spawnTimer -= 1f;

            if (bubbles.Count < maxActiveBubbles)
                SpawnBubble();
        }
    }

    private void SpawnBubble()
    {
        Vector3 localPos = new Vector3(
            Random.Range(-surfaceSize.x * 0.5f, surfaceSize.x * 0.5f),
            surfaceYOffset,
            Random.Range(-surfaceSize.y * 0.5f, surfaceSize.y * 0.5f)
        );

        Vector3 worldPos = acidSurface.TransformPoint(localPos);

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "Acid_Bubble";

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = bubbleMaterial;

        float size = Random.Range(bubbleSizeRange.x, bubbleSizeRange.y);
        float lifetime = Random.Range(bubbleLifetimeRange.x, bubbleLifetimeRange.y);

        obj.transform.position = worldPos;
        obj.transform.rotation = acidSurface.rotation;
        obj.transform.localScale = Vector3.zero;

        bubbles.Add(new BubbleInstance
        {
            obj = obj,
            renderer = renderer,
            startPosition = worldPos,
            baseSize = size,
            lifetime = lifetime,
            age = 0f,
            popped = false
        });
    }

    private void BubbleUpdate()
    {
        for (int i = bubbles.Count - 1; i >= 0; i--)
        {
            BubbleInstance b = bubbles[i];

            if (b.obj == null)
            {
                bubbles.RemoveAt(i);
                continue;
            }

            b.age += Time.deltaTime;

            float t = Mathf.Clamp01(b.age / b.lifetime);

            // 先长大，后消失
            float grow = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.45f));
            float fadeOut = 1f - Mathf.SmoothStep(0.72f, 1f, t);
            float fade = grow * fadeOut;

            // 泡泡形状：扁半球感
            float sizeXZ = b.baseSize * Mathf.Lerp(0.25f, 1f, grow);
            float sizeY = sizeXZ * domeHeightScale;

            b.obj.transform.localScale = new Vector3(sizeXZ, sizeY, sizeXZ);

            // 让泡泡从水面微微鼓起
            Vector3 up = acidSurface != null ? acidSurface.up : Vector3.up;
            b.obj.transform.position = b.startPosition + up * (sizeY * 0.45f + Mathf.Sin(t * Mathf.PI) * riseHeight);

            b.renderer.GetPropertyBlock(block);
            block.SetFloat("_Fade", fade);
            b.renderer.SetPropertyBlock(block);

            if (t >= 0.86f && !b.popped)
            {
                b.popped = true;
                SpawnRing(b.startPosition, b.baseSize);
            }

            if (t >= 1f)
            {
                Destroy(b.obj);
                bubbles.RemoveAt(i);
            }
        }
    }

    private void SpawnRing(Vector3 position, float bubbleSize)
    {
        GameObject obj = new GameObject("Acid_Bubble_Pop_Ring");

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        mf.sharedMesh = ringMesh;
        mr.sharedMaterial = ringMaterial;

        obj.transform.position = position + acidSurface.up * 0.012f;
        obj.transform.rotation = acidSurface.rotation;

        float start = bubbleSize * ringStartScale;
        float end = bubbleSize * ringEndScale;

        obj.transform.localScale = Vector3.one * start;

        rings.Add(new RingInstance
        {
            obj = obj,
            renderer = mr,
            startPosition = obj.transform.position,
            startScale = start,
            endScale = end,
            lifetime = ringLifetime,
            age = 0f
        });
    }

    private void RingUpdate()
    {
        for (int i = rings.Count - 1; i >= 0; i--)
        {
            RingInstance r = rings[i];

            if (r.obj == null)
            {
                rings.RemoveAt(i);
                continue;
            }

            r.age += Time.deltaTime;

            float t = Mathf.Clamp01(r.age / r.lifetime);

            float scale = Mathf.Lerp(r.startScale, r.endScale, Mathf.SmoothStep(0f, 1f, t));
            float fade = 1f - Mathf.SmoothStep(0.1f, 1f, t);

            r.obj.transform.localScale = Vector3.one * scale;

            r.renderer.GetPropertyBlock(block);
            block.SetFloat("_Fade", fade);
            r.renderer.SetPropertyBlock(block);

            if (t >= 1f)
            {
                Destroy(r.obj);
                rings.RemoveAt(i);
            }
        }
    }

    private Mesh CreateHorizontalQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Horizontal_Ring_Quad";

        Vector3[] vertices =
        {
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f, -0.5f),
            new Vector3(-0.5f, 0f,  0.5f),
            new Vector3( 0.5f, 0f,  0.5f)
        };

        Vector2[] uvs =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        int[] triangles =
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}