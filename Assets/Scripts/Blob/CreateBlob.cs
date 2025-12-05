using UnityEngine;
using UnityEngine.Assertions;

// TODO: CreateBlob -> BlobFactory, BlobController handles mesh stuff

[RequireComponent(typeof(MeshFilter))]
/// <summary>
///     This class (1) creates the blob character and (2) maintains its appearance as the game is played.
///     <para/>
///     The blob character is composed of "atoms", spheres interconnected by spring joints.
///     There is one atom at each surface vertex (for the default icosahedron shape, that makes 12 atoms),
///     plus one extra atom at the center of the blob (13 total).
///     The surface atoms ensure that the overall shape of the blob is maintained, while the center
///     atom ensures that the blob doesn't collapse on itself.
/// </summary>
public class CreateBlob : MonoBehaviour
{
    // PUBLIC MEMBERS
    /// <summary>
    ///     Spring constant for the springs maintaining the blob's shape.
    /// </summary>
    public float springForce = 100f;
    /// <summary>
    ///     How big each atom of the blob is.
    /// </summary>
    public float atomScale = 0.5f;
    /// <summary>
    ///    TODO: id strings for multiplayer
    /// </summary>
    public const int BLOB_ID = 0;

    // TODO: can/should these be found automatically?
    public GameObject leftEye;
    public GameObject rightEye;

    // PRIVATE MEMBERS
    // Spawning
    /// <summary>
    ///     Initial position of the blob.
    /// </summary>
    private Vector3 spawnPoint;
    private BlobController blobController;

    // Atoms
    /// <summary>
    ///     Number of vertices in an icosahedron, plus 1 for the center point.
    /// </summary>
    private const int NUM_ATOMS = 13;
    /// <summary>
    ///     Custom epsilon for floating point comparisons. Used to determine if two atoms are adjacent.
    /// </summary>
    private const float EPSILON = 1E-3f;
    private const string JELLY_PHYSIC_MATERIAL = "Materials/Blob Materials/JellyPhysic";
    private const string IGNORE_CAMERA_LAYER = "Ignore Camera";
    private GameObject[] blobAtoms;
    private Rigidbody[] atomRigidbodies;
    /// <summary>
    ///     Quick reference for <tt>blobAtoms[0]</tt>.
    /// </summary>
    private GameObject centerAtom;

    // Springs
    /// <summary>
    ///    Number of edges in an icosahedron, plus 12 for radial connections (one per vertex).
    /// </summary>
    private const int NUM_SPRINGS = 42;
    /// <summary>
    ///     Distance between blob's surface atoms. Equal to side length of radius 1 icosahedron: csc(2/5 * pi).
    /// </summary>
    private const float SIDE_LENGTH = 1.051462224f;
    /// <summary>
    ///     Scalar for each spring's length. 1 corresponds to spring lengths equal to <tt>SIDE_LENGTH</tt>.
    /// </summary>
    private float springLengthFactor = 1f;
    private SpringJoint[] springJoints;
    private Vector3[] connectedAnchors;

    // Mesh
    /// <summary>
    ///    Ratio of blob mesh radius to actual blob radius.
    /// </summary>
    private float meshScale = 1.5f;
    private Mesh blobMesh;
    /// <summary>
    ///     Maps mesh vertices <tt>i</tt> to blob atoms <tt>meshToAtomMap[i]</tt>.
    ///     Necessary for positioning each mesh vertex at its corresponding atom,
    ///     since the mesh might have more than one vertex per atom.
    /// </summary>
    private int[] meshToAtomMap;

    /// <summary>
    ///     Create the blob. This assumes the script is a component of an icosahedron mesh, which may change later.
    /// </summary>
    void Start()
    {
        spawnPoint = transform.position;
        blobMesh = GetComponent<MeshFilter>().mesh;

        // Build the blob
        blobAtoms = MakeAtomsFromMesh(blobMesh, 0, NUM_ATOMS, out meshToAtomMap, spawnPoint);
        centerAtom = blobAtoms[0];

        atomRigidbodies = AddRigidBodies(blobAtoms);

        PhysicMaterial jellyPhysic = Resources.Load(JELLY_PHYSIC_MATERIAL, typeof(PhysicMaterial)) as PhysicMaterial;
        AddPhysicMaterials(blobAtoms, jellyPhysic);

        springJoints = ConnectAtoms(blobAtoms, springForce, NUM_SPRINGS, SIDE_LENGTH, false);

        blobController = AttachController(centerAtom);
    }

    /// <summary>
    ///     Spawns one atom (sphere) at the given center, and one at each vertex of the given mesh.
    /// </summary>
    /// <param name="mesh">
    ///     The mesh to build from.
    /// </param>
    /// <param name="ID">
    ///     The ID of this mesh.
    /// </param>
    /// <param name="expectedCount">
    ///     How many atoms the caller expects there to be when this method returns.
    /// </param>
    /// <param name="vertexToAtomMap">
    ///     A list mapping which mesh verices correspond to which atoms.
    ///     vertexToAtomMap[i] = j iff mesh.vertices[i] corresponds to the returned array's j-th element. 
    /// </param>
    /// <param name="center">
    ///     The center of the mesh.
    ///     If not given, defaults to the average position of the mesh's vertices.
    /// </param>
    /// <returns>
    ///     An array of GameObjects containing the spawned atoms.
    ///     The first atom (index 0) is the atom spawned at the center.
    /// </returns>
    private GameObject[] MakeAtomsFromMesh(Mesh mesh, int ID, int expectedCount, out int[] vertexToAtomMap, Vector3? center = null)
    {

        GameObject[] atoms = new GameObject[expectedCount];
        vertexToAtomMap = new int[mesh.vertexCount];
        int newIndex = 1; // reserve index 0 for the center atom
        Vector3 positionSum = Vector3.zero; // used to calculate the center, if needed

        // Add enough atoms to account for every mesh vertex.
        for (int i = 0; i < mesh.vertexCount; i++)
        {

            Vector3 newPosition = transform.TransformPoint(mesh.vertices[i]); // working in world coordinates is easier
            positionSum += newPosition;

            // Search for an existing atom that was spawned at this new atom's position.
            vertexToAtomMap[i] = -1;
            for (int j = 1; j < newIndex; j++)
            {
                if (atoms[j].transform.position == newPosition)
                { // accounts for floating point errors
                    vertexToAtomMap[i] = j;
                    break;
                }
            }

            // If no existing atom was found, add a new one.
            if (vertexToAtomMap[i] == -1)
            {
                Assert.IsTrue(
                    newIndex < expectedCount,
                    string.Format("Number of necessary atoms is greater than expected. ({0} >= {1})", newIndex, expectedCount)
                );
                vertexToAtomMap[i] = newIndex;
                atoms[newIndex] = SpawnAtom(
                    newPosition,
                    atomScale,
                    newIndex, ID,
                    transform.parent.transform // ew
                );
                newIndex++;
            }
        }
        Assert.IsTrue(
            newIndex == expectedCount,
            string.Format("Number of atoms created is less than expected. ({0} < {1})", newIndex, expectedCount)
        );

        Vector3 centerPosition = (center == null) ? positionSum / (expectedCount - 1) : (Vector3)center;
        atoms[0] = SpawnAtom(
            centerPosition,
            atomScale,
            0, ID,
            transform.parent.transform // ew x2
        );

        return atoms;
    }

    /// <summary>
    ///     Instantiates one atom (sphere) at the given position, parented to the given parent's transform.
    /// </summary>
    /// <param name="position">
    ///     The position at which to spawn this atom.
    /// </param>
    /// <param name="scale">
    ///     The scale to give this atom.
    /// </param>
    /// <param name="ID">
    ///     The ID of this atom.
    /// </param>
    /// <param name="parentID">
    ///     The ID of this atom's parent.
    /// </param>
    /// <param name="parentTransform">
    ///     The transform of this atom's parent.
    /// </param>
    /// <returns>
    ///     The atom GameObject.
    /// </returns>
    private GameObject SpawnAtom(Vector3 position, float scale, int ID, int parentID, Transform parentTransform)
    {
        GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atom.GetComponent<MeshRenderer>().enabled = false; // make it invisible

        atom.name = string.Format("Mesh {0} Atom {1}", parentID, ID);
        atom.layer = LayerMask.NameToLayer(IGNORE_CAMERA_LAYER);
        atom.tag = "Atom";

        atom.transform.localScale = scale * Vector3.one;
        atom.transform.position = position;
        atom.transform.SetParent(parentTransform);

        return atom;
    }

    /// <summary>
    ///     Adds a Rigidbody component to every object in an array.
    /// </summary>
    /// <param name="objects">
    ///     The array of objects.
    /// </param>
    /// <returns>
    ///     The array of Rigidbodies added.
    ///     objects[i] has the Rigidbody at index i.
    /// </returns>
    private Rigidbody[] AddRigidBodies(GameObject[] objects)
    {
        Rigidbody[] rigidBodies = new Rigidbody[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            rigidBodies[i] = objects[i].AddComponent<Rigidbody>();
        }

        return rigidBodies;
    }

    /// <summary>
    ///     Adds a physic material to the collider of every object in an array.
    /// </summary>
    /// <param name="objects">
    ///     The array of objects.
    /// </param>
    /// <param name="material">
    ///     The material to apply.
    /// </param>
    private void AddPhysicMaterials(GameObject[] objects, PhysicMaterial physicMaterial)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].GetComponent<Collider>().material = physicMaterial;
        }
    }

    /// <summary>
    ///     Inter-connects the objects with spring joints.
    ///     <br/>
    ///     Note: For object A's spring joint connected to object B, the anchor (relative to A)
    ///     "wants" to rest at the connectedAnchor (relative to B).
    ///     e.g. anchor=(0,0,0), connectedAnchor=(0,1,0) => A's origin "wants" to rest one unit
    ///     above B's origin.
    /// </summary>
    /// <param name="objects">
    ///     The array of objects to connect.
    /// </param>
    /// <param name="springForce">
    ///     The spring constant to apply to each spring joint.
    /// </param>
    /// <param name="expectedCount">
    ///     How many spring joints are expected to be created
    /// </param>
    /// <param name="distance">
    ///     The threshold that determines which pairs of objects will be connected.
    ///     The center object (index 0) will always be connected to all other objects.
    /// </param>
    /// <param name="ballAdjacency">
    ///     Option for deciding how to interpret the `distance` threshold.
    ///     `true` indicates that objects are connected iff they are at most `distance` apart.
    ///     `false` indicates that objects are connected iff they are approximately `distance` apart.
    /// </param>
    /// <returns>
    ///     The array of SpringJoints added, in no particular order.
    /// </returns>
    private SpringJoint[] ConnectAtoms(GameObject[] objects, float springForce, int expectedCount, float distance, bool ballAdjacency)
    {
        SpringJoint[] springJoints = new SpringJoint[expectedCount];
        connectedAnchors = new Vector3[expectedCount];
        int newIndex = 0;

        // For each unique pair of objects, try to connect them.
        for (int i = 1; i < objects.Length; i++)
        {
            for (int j = 0; j < i; j++)
            {
                bool isCenterObject = (j == 0);
                if (isCenterObject || AreAdjacent(objects, i, j, distance, ballAdjacency))
                {
                    Assert.IsTrue(
                        newIndex < expectedCount,
                        string.Format("Number of necessary springs is greater than expected. ({0} >= {1})", newIndex, expectedCount)
                    );

                    Rigidbody from = objects[i].GetComponent<Rigidbody>();
                    Rigidbody to = objects[j].GetComponent<Rigidbody>();

                    springJoints[newIndex] = objects[i].AddComponent<SpringJoint>();
                    springJoints[newIndex].connectedBody = to;

                    springJoints[newIndex].enableCollision = true;
                    springJoints[newIndex].spring = springForce;

                    // manually set anchor positions
                    springJoints[newIndex].autoConfigureConnectedAnchor = false;
                    springJoints[newIndex].anchor = Vector3.zero;
                    connectedAnchors[newIndex] = to.transform.InverseTransformPoint(from.position);
                    springJoints[newIndex].connectedAnchor = connectedAnchors[newIndex];

                    newIndex++;
                }
            }
        }

        Assert.IsTrue(
            newIndex == expectedCount,
            string.Format("Number of springs created is less than expected. ({0} < {1})", newIndex, expectedCount)
        );

        return springJoints;
    }

    /// <summary>
    ///     Tests if objects i and j are adjacent by the given parameters.
    /// </summary>
    /// <param name="objects">
    ///     The array of objects
    /// </param>
    /// <param name="i">
    ///     The index of the first object.
    /// </param>
    /// <param name="j">
    ///     The index of the second object.
    /// </param>
    /// <param name="distance">
    ///     The threshold that determines if the two objects are adjacent.
    /// </param>
    /// <param name="ballAdjacency">
    ///     Option for deciding how to interpret the `distance` threshold.
    ///     `true` indicates the objects are adjacent iff they are at most `distance` apart.
    ///     `false` indicates the objects are adjacent iff they are approximately `distance` apart.
    /// </param>
    /// <returns>
    ///     A boolean indicating if i and j are adjacent.
    /// </returns>
    private bool AreAdjacent(GameObject[] objects, int i, int j, float distance, bool ballAdjacency)
    {
        float separation = (objects[i].transform.position - objects[j].transform.position).magnitude;

        if (ballAdjacency)
        {
            return distance < separation;
        }
        else
        {
            // Mathf.Approximately() is seemingly too strict.
            return Mathf.Abs(separation - distance) < EPSILON;
        }
    }

    /// <summary>
    ///     Attaches a blob controller component to the given gameObject.
    ///     This controller handles player input, motion, and world interaction.
    /// </summary>
    /// <param name="gameObject">
    ///     The object to attach the blob controller to.
    /// </param>
    /// <returns>
    ///     The attached BlobController.
    /// </returns>
    private BlobController AttachController(GameObject gameObject)
    {
        BlobController controller = gameObject.AddComponent<BlobController>();

        // Inherit class member. Is there a better way to do this?
        controller.SetCreateBlob(this);

        return controller;
    }

    /// <summary>
    ///     Maintains the blob's appearance by keeping the eyes and mesh in place and looking as they should.
    /// </summary>
    void Update()
    {
        // Move the blob mesh and snap its vertices to the atoms.
        transform.position = centerAtom.transform.position;
        SnapMeshToAtoms(blobMesh, blobAtoms, meshToAtomMap, meshScale);

        // Maintain accurate reflections and colors.
        blobMesh.RecalculateNormals();
        blobMesh.RecalculateBounds();

        SnapEyes();
    }

    ///<summary>
    ///     Helper function to snap both the left and right eyes.
    ///</summary>
    private void SnapEyes()
    {
        SnapToTriangle(leftEye, blobAtoms, 1, 2, 3, meshScale);
        SnapToTriangle(rightEye, blobAtoms, 1, 3, 4, meshScale);
    }

    /// <summary>
    ///     Transforms the `objectToSnap` such that it is positioned between the three given objects.
    ///     The distance between the objects and the center object (index 0) is first scaled.
    /// </summary>
    /// <param name="objectToSnap">
    ///     The object that is being transformed.
    /// </param>
    /// <param name="objects">
    ///     The array of objects.
    /// </param>
    /// <param name="i">
    ///     The index of the first object.
    /// </param>
    /// <param name="j">
    ///     The index of the second object.
    /// </param>
    /// <param name="k">
    ///     The index of the third object.
    /// </param>
    /// <param name="scale">
    ///     The scale between the given objects and the center object.
    /// </param>
    private void SnapToTriangle(GameObject objectToSnap, GameObject[] objects, int i, int j, int k, float scale)
    {
        Vector3 center = objects[0].transform.position;
        Vector3 pos1 = objects[i].transform.position;
        Vector3 pos2 = objects[j].transform.position;
        Vector3 pos3 = objects[k].transform.position;

        Vector3 position = ScaledBarycenter(center, pos1, pos2, pos3, scale);
        Vector3 direction = NormalVector(center, pos1, pos2, pos3, position);

        objectToSnap.transform.position = position;
        objectToSnap.transform.LookAt(position + direction, Vector3.up);
    }

    /// <summary>
    ///     Calculates the barycenter of the triangle formed by the tips of three vectors scaled away from a center.
    /// </summary>
    /// <param name="center">
    ///     The center position.
    /// </param>
    /// <param name="pos1">
    ///     The first position.
    /// </param>
    /// <param name="pos2">
    ///     The second position.
    /// </param>
    /// <param name="pos3">
    ///     The third position.
    /// </param>
    /// <param name="scale">
    ///     The scale between the given positions and the center.
    /// </param>
    /// <returns>
    ///     The barycenter of the three positions as a Vector3, scaled away from the center.
    /// </returns>
    private Vector3 ScaledBarycenter(Vector3 center, Vector3 pos1, Vector3 pos2, Vector3 pos3, float scale)
    {
        Vector3 barycenter = (pos1 + pos2 + pos3) / 3;

        return (barycenter - center) * scale + center;
    }

    /// <summary>
    ///     Calculates the normal vector of the triangle formed by the tips of three vectors, oriented away from the center.
    /// </summary>
    /// <param name="center">
    ///     The center position.
    /// </param>
    /// <param name="pos1">
    ///     The first position.
    /// </param>
    /// <param name="pos2">
    ///     The second position.
    /// </param>
    /// <param name="pos3">
    ///     The third position.
    /// </param>
    /// <param name="barycenter">
    ///     The barycenter of the triangle.
    /// </param>
    /// <returns>
    ///     The normal Vector3 of the triangle.
    /// </returns>
    private Vector3 NormalVector(Vector3 center, Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 barycenter)
    {
        Vector3 dir12 = pos2 - pos1;
        Vector3 dir13 = pos3 - pos1;

        Vector3 normal = Vector3.Cross(dir12, dir13).normalized;

        if (Vector3.Angle(normal, barycenter - center) > 90)
        {
            return -normal;
        }
        else
        {
            return normal;
        }
    }

    /// <summary>
    ///     Sets the vertex positions for the mesh to equal the given object positions, mapped and scaled accordingly.
    /// </summary>
    /// <param name="mesh">
    ///     The mesh whose vertices will be snapped.
    /// </param>
    /// <param name="objects">
    ///     The objects to snap the mesh to.
    /// </param>
    /// <param name="vertexToObjectMap">
    ///     The mapping between mesh vertices and object indices.
    ///     vertexToObjectMap[i] = j indicates that vertex i corresponds to object j.
    /// </param>
    /// <param name="scale">
    ///     The scaling to apply to the mesh.
    /// </param>
    private void SnapMeshToAtoms(Mesh mesh, GameObject[] objects, int[] vertexToObjectMap, float scale)
    {
        Vector3[] newVertices = mesh.vertices;

        for (int i = 0; i < newVertices.Length; i++)
        {
            Vector3 worldPosition = objects[vertexToObjectMap[i]].transform.position;

            // Calculate local position of mesh vertices.
            newVertices[i] = transform.InverseTransformPoint(worldPosition) * scale;
        }

        mesh.vertices = newVertices;
    }

    // Getters and setters
    public GameObject[] GetAtoms()
    {
        return blobAtoms;
    }

    public float GetSpringLengthFactor()
    {
        return springLengthFactor;
    }

    /// <summary>
    ///     Modify each spring's connectedAnchor to be <tt>factor</tt> times the original value.
    /// </summary>
    /// <param name="factor">
    ///     The new spring length factor.
    /// </param>
    public void SetSpringLengthFactor(float factor = 1)
    {
        springLengthFactor = factor;
        for (int i = 0; i < NUM_SPRINGS; i++)
        {
            springJoints[i].connectedAnchor = factor * connectedAnchors[i];
            springJoints[i].spring = factor * springForce;
        }

        meshScale = 1f + atomScale/factor;
    }
}