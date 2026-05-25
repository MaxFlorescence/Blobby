using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
[RequireComponent(typeof(MeshFilter))]

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
    
    public AtomCollection atoms;

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    /// <summary>
    ///    The ratio of blob mesh radius to actual blob radius.
    /// </summary>
    private const float SCALE_FACTOR = 1.5f;

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
    ///     Transforms the given object such that it is positioned between the three given atom
    ///     indices. The distance between the objects and the center object (index 0) is first
    ///     scaled.
    /// </summary>
    /// <param name="eye">
    ///     The object that is being transformed.
    /// </param>
    private void SnapToTriangle(GameObject eye, Vector3Int triangle)
    {
        Vector3 position = ScaledBarycenter(triangle, SCALE_FACTOR);
        Vector3 direction = NormalVector(triangle, position);

        eye.transform.position = position;
        eye.transform.LookAt(position + direction, Vector3.up);
    }

    /// <summary>
    ///     Calculates the barycenter of the triangle formed by the tips of three vectors scaled
    ///     away from a center.
    /// </summary>
    /// <param name="scale">
    ///     The scale between the given positions and the center.
    /// </param>
    /// <returns>
    ///     The barycenter of the three positions as a Vector3, scaled away from the center.
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
    ///     Calculates the normal vector of the triangle formed by the tips of three vectors,
    ///     oriented away from the center.
    /// </summary>
    /// <param name="barycenter">
    ///     The barycenter of the triangle formed by the three positions.
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
            newVertices[i] = transform.InverseTransformPoint(worldPosition) * SCALE_FACTOR;
        }

        mesh.vertices = newVertices;
    }

    public void SetMaterial(Material material)
    {
        meshRenderer.materials = new Material[] {material};
    }
}