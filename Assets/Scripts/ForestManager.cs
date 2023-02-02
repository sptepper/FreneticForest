using UnityEngine;
using UnityEngine.AI;

public class ForestManager : MonoBehaviour
{
    public const float SECONDS_BETWEEN_REBUILD = 20f;
    public ForestSettings ForestSettings;

    public GameObject Tree_Prefab;
    public GameObject Critter_Prefab;

    public GameObject HomeTree{ get; private set; }

    public float ArenaSize => ForestSettings.forestScale * 5f;

    public static ForestManager Instance{ get; private set; }

    private NavMeshSurface _navSurface;
    private MeshRenderer _meshRenderer;

    private float _timeSinceRebuild = 0;
    private float max_pos;

    public const float TRANSITION_RATE = 0.5f;
    public const float SLOW_THRESHOLD = 0.95f; // Lowerbound of slow transition
    private float _alpha = 1;
    private bool _inTransition;
    private int _transDir = 1;

    void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _navSurface = GetComponent<NavMeshSurface>();
        _meshRenderer = GetComponent<MeshRenderer>();
        transform.localScale = new Vector3(ForestSettings.forestScale,
                                         1f, ForestSettings.forestScale);

        max_pos = ArenaSize - ForestSettings.edgeBufferSize;
        HomeTree = Instantiate(Tree_Prefab, Vector3.zero, Quaternion.identity);

        SpawnTrees();
        RebuildNavMesh();

        SpawnCritters();
    }

    void Update()
    {
        _timeSinceRebuild += Time.deltaTime;

        if(_timeSinceRebuild >= SECONDS_BETWEEN_REBUILD)
            RebuildNavMesh();

        if(_inTransition)
            UpdateSurfaceOpacity();
    }

    public void ToggleSurface()
    {
        //_meshRenderer.enabled = !_meshRenderer.enabled;
        _transDir = -_transDir;
        _inTransition = true;
    }

    private void UpdateSurfaceOpacity()
    {
        var delta = _transDir * TRANSITION_RATE * Time.deltaTime;
        if(_alpha > SLOW_THRESHOLD)
            delta *= 0.1f;

        _alpha += delta;
        if(_alpha > 1)
        {
            _alpha = 1;
            _inTransition = false;
        }
        else if(_alpha < 0)
        {
            _alpha = 0;
            _inTransition = false;
        }

        _meshRenderer.material.color = new Color(_meshRenderer.material.color.r,
         _meshRenderer.material.color.g, _meshRenderer.material.color.b, _alpha);
    }

    private void SpawnTrees()
    {
        var TreeHolder = new GameObject("Trees").transform;

        for(int i=1;i<ForestSettings.numberOfTrees;i++)
        {
            Vector3 spawn_pos = new Vector3(Random.Range(-max_pos,max_pos),
                                        0, Random.Range(-max_pos, max_pos));
            var new_tree = Instantiate(Tree_Prefab, spawn_pos, Quaternion.identity);
            new_tree.transform.SetParent(TreeHolder);
        }
    }

    private void SpawnCritters()
    {
        SpawnCritterFromData(ForestSettings.critters.patherData, ForestSettings.numberOfPathers);
        SpawnCritterFromData(ForestSettings.critters.diggieData, ForestSettings.numberOfDiggies);
        SpawnCritterFromData(ForestSettings.critters.chopData, ForestSettings.numberOfChopChops);
    }

    private void SpawnCritterFromData(CritterTypeData data, int num)
    {
        var Holder = new GameObject(data.critterName).transform;

        for(int i=0;i<num;i++)
        {
            Vector3 spawn_pos = new Vector3(Random.Range(-max_pos,max_pos),
                                        0, Random.Range(-max_pos, max_pos));
            var new_pather = Instantiate(Critter_Prefab, spawn_pos, Quaternion.identity);
            new_pather.GetComponent<CritterPod>().CritterData = data;
            new_pather.transform.SetParent(Holder);
        }
    }

    private void RebuildNavMesh()
    {
        _navSurface.BuildNavMesh();
        _timeSinceRebuild = 0;
    }

    public struct TreeNode
    {
        public int Id;
        public Vector3 Position;
    }

    public struct TreeConnection
    {
        public float Strength;
        public TreeNode NodeA;
        public TreeNode NodeB;
    }
}