using UnityEngine;
using UnityEngine.Assertions;
[RequireComponent(typeof(MeshFilter))]

/// <summary>
///     A class that controls the mesh for a blob.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class BlobMeshController : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // EYES
    // ---------------------------------------------------------------------------------------------
    public GameObject leftEye;
    /// <summary>
    ///     The triangle of atoms that the left eye will snap to the barycenter of.
    /// </summary>
    public Vector3Int leftTriangle;
    public GameObject rightEye;
    /// <summary>
    ///     The triangle of atoms that the right eye will snap to the barycenter of.
    /// </summary>
    public Vector3Int rightTriangle;
    
    // ---------------------------------------------------------------------------------------------
    // ATOMS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The blob atoms that the mesh is attached to.
    /// </summary>
    public AtomCollection atoms;

    /// <summary>
    ///    The ratio of blob mesh radius to actual blob radius.
    /// </summary>
    public float ScaleFactor { get; set; } = 1.5f;

    // ---------------------------------------------------------------------------------------------
    // MESH
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The mesh of the blob.
    /// </summary>
    private Mesh mesh;

    /// <summary>
    ///     The renderer of the blob's mesh.
    /// </summary>
    private MeshRenderer meshRenderer;

    /// <summary>
    ///     Maps mesh vertices <tt>i</tt> to blob atoms <tt>vertexToAtomMap[i]</tt>. This is
    ///     necessary for positioning each mesh vertex at its corresponding atom, since the mesh
    ///     has more than one vertex per atom.
    /// </summary>
    private int[] vertexToAtomMap;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();

        MapVertices();
    }

    /// <summary>
    ///     Determines which atom each of the mesh's vertices is closest to.
    /// </summary>
    private void MapVertices()
    {
        vertexToAtomMap = new int[mesh.vertexCount];
        Vector3[] meshVertices = mesh.vertices;
        
        for (int i = 1; i < atoms.Count; i++)
        {
            for (int j = 0; j < mesh.vertexCount; j++)
            {
                // int arrays are initialized with 0s, and no vertex should map to the center atom
                if (vertexToAtomMap[j] > 0) continue;

                Vector3 vertexPosition = transform.TransformPoint(meshVertices[j]);

                if (atoms[i].position.Approx(vertexPosition)) vertexToAtomMap[j] = i;
            }
        }
    }

    void Update()
    {
        SnapMeshToAtoms();

        // Maintain accurate reflections and colors.
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        SnapToTriangle(leftEye, leftTriangle);
        SnapToTriangle(rightEye, rightTriangle);
    }

    /// <summary>
    ///     Transforms the given object such that it is positioned between the three given atoms,
    ///     and scaled away from the center atom.
    /// </summary>
    /// <param name="cosmetic">
    ///     The object that is being transformed.
    /// </param>
    private void SnapToTriangle(GameObject cosmetic, Vector3Int triangle)
    {
        Vector3 position = ScaledBarycenter(triangle, ScaleFactor);
        Vector3 direction = NormalVector(triangle, position);

        cosmetic.transform.position = position;
        cosmetic.transform.LookAt(position + direction, Vector3.up);
    }

    /// <summary>
    ///     Calculates the barycenter of the triangle formed by three atoms scaled away from the
    ///     center atom.
    /// </summary>
    /// <param name="scale">
    ///     The scale by which to multiply the barycenter position, relative to the center atom.
    /// </param>
    /// <returns>
    ///     The barycenter of the triangle as a Vector3, scaled away from the center.
    /// </returns>
    private Vector3 ScaledBarycenter(Vector3Int triangle, float scale)
    {
        Vector3 barycenter = (
            atoms[triangle.x].position +
            atoms[triangle.y].position +
            atoms[triangle.z].position
        ) / 3;

        return (barycenter - transform.position) * scale + transform.position;
    }

    /// <summary>
    ///     Calculates the normal vector of the triangle formed by three atoms, oriented away from
    ///     the center atom.
    /// </summary>
    /// <param name="barycenter">
    ///     The barycenter of the triangle.
    /// </param>
    /// <returns>
    ///     The normal Vector3 of the triangle.
    /// </returns>
    private Vector3 NormalVector(Vector3Int triangle, Vector3 barycenter)
    {
        Vector3 dirXY = atoms[triangle.y].position - atoms[triangle.x].position;
        Vector3 dirXZ = atoms[triangle.z].position - atoms[triangle.x].position;

        Vector3 normal = Vector3.Cross(dirXY, dirXZ).normalized;

        if (Vector3.Angle(normal, barycenter - transform.position) > 90)
        {
            return -normal;
        }
        else
        {
            return normal;
        }
    }

    /// <summary>
    ///     Sets the vertex positions for the mesh to equal the given object positions, mapped and
    ///     scaled accordingly.
    /// </summary>
    private void SnapMeshToAtoms()
    {
        Vector3[] newVertices = mesh.vertices;

        for (int i = 0; i < newVertices.Length; i++)
        {
            Assert.AreNotEqual(0, vertexToAtomMap[i], $"{i}");
            Vector3 worldPosition = atoms[vertexToAtomMap[i]].position;

            // Calculate local position of mesh vertices.
            newVertices[i] = transform.InverseTransformPoint(worldPosition) * ScaleFactor;
        }

        mesh.vertices = newVertices;
    }

    /// <summary>
    ///     Sets the materials array of the blob's mesh renderer.
    /// </summary>
    public void SetMaterials(params Material[] materials)
    {
        meshRenderer.materials = materials;
    }
}