using System;
using UnityEngine;
using UnityEngine.Assertions;

public class CreateBlob : MonoBehaviour
{
    /// <summary>
    ///     This class (1) creates the blob character and (2) maintains its appearance as the game is played.
    /// </summary>

    // Public members
    public float springForce;
    public float atomScale;
    public const int BLOB_ID = 0; // TODO: id strings

    // TODO: can these be found automatically?
    public GameObject leftEye;
    public GameObject rightEye;
    public GameObject mainCamera;
    public FinishMenu finishMenu;
    public CheatMenu cheatMenu;
    public PauseMenu pauseMenu;
    public ExperimenterScript experimenter;

    // Private members
    private const string JELLY_PHYSIC_MATERIAL = "Materials/JellyPhysic";
    private const string IGNORE_CAMERA_LAYER = "Ignore Camera";
    private const int NUM_ATOMS = 13;
    private const int NUM_SPRINGS = 42;
    private const float SIDE_LENGTH = 1.051462224f; // == csc(2/5 * pi);
    private const float EPSILON = 1E-3f;
    private const float MESH_SCALE = 1.5f;
    private GameObject[] blobAtoms;
    private GameObject centerAtom;
    private Rigidbody[] rigidBodies;
    private SpringJoint[] springJoints;
    private Vector3 spawnPoint;
    private Mesh blobMesh;
    private BlobController blobController;
    private int[] meshToAtomMap;

    void Start()
    {
        /// <summary>
        ///     Create the blob. This assumes the script is a component of an icosahedron mesh, which may change later.
        /// </summary>

        spawnPoint = transform.position;
        blobMesh = GetComponent<MeshFilter>().mesh;

        // Build the blob
        blobAtoms = MakeAtomsFromMesh(blobMesh, 0, NUM_ATOMS, out meshToAtomMap, spawnPoint);
        centerAtom = blobAtoms[0];
        experimenter.blob = centerAtom.transform;

        rigidBodies = AddRigidBodies(blobAtoms);
        
        PhysicMaterial jellyPhysic = Resources.Load(JELLY_PHYSIC_MATERIAL, typeof(PhysicMaterial)) as PhysicMaterial;
        AddPhysicMaterials(blobAtoms, jellyPhysic);

        springJoints = ConnectAtoms(blobAtoms, springForce, NUM_SPRINGS, SIDE_LENGTH, false);

        blobController = AttachController(centerAtom);
    }

    private GameObject[] MakeAtomsFromMesh(Mesh mesh, int ID, int expectedCount, out int[] vertexToAtomMap, Vector3? center = null) {
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
        ///     (GameObject[]) An array containing the spawned atoms.
        ///     The first atom (index 0) is the atom spawned at the center.
        /// </returns>
        
        GameObject[] atoms = new GameObject[expectedCount];
        vertexToAtomMap = new int[mesh.vertexCount];
        int newIndex = 1; // reserve index 0 for the center atom
        Vector3 positionSum = Vector3.zero; // used to calculate the center, if needed

        // Add enough atoms to account for every mesh vertex.
        for (int i = 0; i < mesh.vertexCount; i++) {

            Vector3 newPosition = transform.TransformPoint(mesh.vertices[i]); // working in world coordinates is easier
            positionSum += newPosition;

            // Search for an existing atom that was spawned at this new atom's position.
            vertexToAtomMap[i] = -1;
            for (int j = 1; j < newIndex; j++) {
                if (atoms[j].transform.position == newPosition) { // accounts for floating point errors
                    vertexToAtomMap[i] = j;
                    break;
                }
            }

            // If no existing atom was found, add a new one.
            if (vertexToAtomMap[i] == -1) {
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

        Vector3 centerPosition = (center == null) ? positionSum/(expectedCount-1) : (Vector3)center;
        atoms[0] = SpawnAtom(
            centerPosition,
            atomScale,
            0, ID,
            transform.parent.transform // ew x2
        );

        return atoms;
    }

    private GameObject SpawnAtom(Vector3 position, float scale, int ID, int parentID, Transform parentTransform) {
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
        ///     (GameObject) The atom.
        /// </returns>

        GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atom.GetComponent<MeshRenderer>().enabled = false; // make it invisible

        atom.name = string.Format("Mesh {0} Atom {1}", parentID, ID);
        atom.layer = LayerMask.NameToLayer(IGNORE_CAMERA_LAYER);

        atom.transform.localScale = scale * Vector3.one;
        atom.transform.position = position;
        atom.transform.SetParent(parentTransform);

        return atom;
    }

    private Rigidbody[] AddRigidBodies(GameObject[] objects) {
        /// <summary>
        ///     Adds a Rigidbody component to every object in an array.
        /// </summary>
        /// <param name="objects">
        ///     The array of objects.
        /// </param>
        /// <returns>
        ///     (Rigidbody[]) The array of Rigidbodies added.
        ///     objects[i] has the Rigidbody at index i.
        /// </returns>

        Rigidbody[] rigidBodies = new Rigidbody[objects.Length];
        for (int i = 0; i < objects.Length; i++) {
            rigidBodies[i] = objects[i].AddComponent<Rigidbody>();
        }

        return rigidBodies;
    }

    private void AddPhysicMaterials(GameObject[] objects, PhysicMaterial physicMaterial) {
        /// <summary>
        ///     Adds a physic material to the collider of every object in an array.
        /// </summary>
        /// <param name="objects">
        ///     The array of objects.
        /// </param>
        /// <param name="material">
        ///     The material to apply.
        /// </param>

        for (int i = 0; i < objects.Length; i++) {
            objects[i].GetComponent<Collider>().material = physicMaterial;
        }
    }

    private SpringJoint[] ConnectAtoms(GameObject[] objects, float springForce, int expectedCount, float distance, bool ballAdjacency) {
        /// <summary>
        ///     Inter-connects the objects with spring joints.
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
        ///     (SpringJoint[]) The array of SpringJoints added, in no particular order.
        /// </returns>

        SpringJoint[] springJoints = new SpringJoint[expectedCount];
        int newIndex = 0;

        // For each unique pair of objects, try to connect them.
        for (int i = 1; i < objects.Length; i++) {
            for (int j = 0; j < i; j++) {
                bool isCenterObject = (j == 0);
                if (isCenterObject || AreAdjacent(objects, i, j, distance, ballAdjacency)) {
                    Assert.IsTrue(
                        newIndex < expectedCount,
                        string.Format("Number of necessary springs is greater than expected. ({0} >= {1})", newIndex, expectedCount)
                    );

                    springJoints[newIndex] = objects[i].AddComponent<SpringJoint>();
                    springJoints[newIndex].connectedBody = objects[j].GetComponent<Rigidbody>();

                    springJoints[newIndex].enableCollision = true;
                    springJoints[newIndex].spring = springForce;

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

    private bool AreAdjacent(GameObject[] objects, int i, int j, float distance, bool ballAdjacency) {
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
        ///     (bool) Are objects i and j adjacent?
        /// </returns>

        float separation = (objects[i].transform.position - objects[j].transform.position).magnitude;

        if (ballAdjacency) {
            return distance < separation;
        } else {
            // Mathf.Approximately() is seemingly too strict.
            return Mathf.Abs(separation - distance) < EPSILON;
        }
    }

    private BlobController AttachController(GameObject gameObject) {
        /// <summary>
        ///     Attaches a blob controller component to the given gameObject.
        ///     This controller handles player input, motion, and world interaction.
        /// </summary>
        /// <param name="gameObject">
        ///     The object to attach the blob controller to.
        /// </param>
        /// <returns>
        ///     (BlobController) The attached blob controller.
        /// </returns>

        BlobController controller = gameObject.AddComponent<BlobController>();

        // Inherit some class members. Is there a better way to do this?
        controller.mainCamera = mainCamera;
        controller.blobAtoms = blobAtoms;
        controller.rigidBodies = rigidBodies;
        controller.N = NUM_ATOMS;
        controller.finishMenu = finishMenu;
        controller.cheatMenu = cheatMenu;
        controller.pauseMenu = pauseMenu;

        // Allow the camera to track the blob.
        controller.cameraDistance = 10;
        mainCamera.GetComponent<CameraController>().TrackObject(gameObject, controller.cameraDistance);

        // Allow the cheats menu to teleport the blob.
        cheatMenu.blobController = controller;

        return controller;
    }


    void Update() {
        /// <summary>
        ///     Maintains the blob's appearance by keeping the eyes and mesh in place and looking as they should.
        /// </summary>

        // Move the blob mesh and snap its vertices to the atoms.
        transform.position = centerAtom.transform.position;
        SnapMeshToAtoms(blobMesh, blobAtoms, meshToAtomMap, MESH_SCALE);

        // Maintain accurate reflections and colors.
        blobMesh.RecalculateNormals();
        blobMesh.RecalculateBounds();
        
        SnapEyes();
    }

    private void SnapEyes() {
        ///<summary>
        ///     Helper function to snap both the left and right eyes.
        ///</summary>
        
        SnapToTriangle(leftEye, blobAtoms, 1, 2, 3, MESH_SCALE);
        SnapToTriangle(rightEye, blobAtoms, 1, 3, 4, MESH_SCALE);
    }

    private void SnapToTriangle(GameObject objectToSnap, GameObject[] objects, int i, int j, int k, float scale) {
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
        
        Vector3 center = objects[0].transform.position;
        Vector3 pos1 = objects[i].transform.position;
        Vector3 pos2 = objects[j].transform.position;
        Vector3 pos3 = objects[k].transform.position;
        
        Vector3 position = ScaledBarycenter(center, pos1, pos2, pos3, scale);
        Vector3 direction = NormalVector(center, pos1, pos2, pos3, position);

        objectToSnap.transform.position = position;
        objectToSnap.transform.LookAt(position + direction, Vector3.up);
    }

    private Vector3 ScaledBarycenter(Vector3 center, Vector3 pos1, Vector3 pos2, Vector3 pos3, float scale) {
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
        ///     (Vector3) The barycenter of the three positions, scaled away from the center.
        /// </returns>
        
        Vector3 barycenter = (pos1 + pos2 + pos3) / 3;

        return (barycenter - center)*scale + center;
    }

    private Vector3 NormalVector(Vector3 center, Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 barycenter) {
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
        ///     (Vector3) The normal vector of the triangle.
        /// </returns>
        
        Vector3 dir12 = pos2 - pos1;
        Vector3 dir13 = pos3 - pos1;

        Vector3 normal = Vector3.Cross(dir12, dir13).normalized;

        if (Vector3.Angle(normal, barycenter - center) > 90) {
            return -normal;
        } else {
            return normal;
        }
    }

    private void SnapMeshToAtoms(Mesh mesh, GameObject[] objects, int[] vertexToObjectMap, float scale) {
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

        Vector3[] newVertices = mesh.vertices;

        for (int i = 0; i < newVertices.Length; i++) {
            Vector3 worldPosition = objects[vertexToObjectMap[i]].transform.position;

            // Calculate local position of mesh vertices.
            newVertices[i] = transform.InverseTransformPoint(worldPosition) * scale;
        }

        mesh.vertices = newVertices;
    }
}