using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///     A class that controls the mesh for a blob.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BlobMeshController : MonoBehaviour, IBusyable
{
    // ---------------------------------------------------------------------------------------------
    // COSMETICS
    // --------------------------------------------------------------------------------------------- 
    public BlobCosmetic[] Cosmetics;

    /// <summary>
    ///     The minimum size at which cosmetics can be displayed normally.
    /// </summary>
    private const float LOWER_COSMETIC_THRESHOLD = 0.75f;
    
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
    public float ScaleFactor { get; set; }

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
    private float[] atomOffsets;

    // ---------------------------------------------------------------------------------------------
    // MATERIAL TRANSITIONS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The default amount of time it takes for the mesh to transition between materials.
    /// </summary>
    private const float DEFAULT_TRANSITION_TIME = 1f;

    /// <summary>
    ///     The default alpha values for the materials currently used by the mesh.
    /// </summary>
    private readonly float[] materialAlphas = new float[2];

    /// <summary>
    ///     The timer for controlling materials fading in and out during transitions.
    /// </summary>
    private readonly StagedTimer transitionTimer = new(
        DEFAULT_TRANSITION_TIME, 2,
        new string[] {FADE_NEW_MATERIAL, FADE_OLD_MATERIAL}
    );
    private const string FADE_NEW_MATERIAL = "Fade New Material";
    private const string FADE_OLD_MATERIAL = "Fade Old Material";

    public bool Busy { get => transitionTimer.Running; }

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        transitionTimer.Skip(false);

        MapVertices();
        atomOffsets = new float[atoms.Count];
        Jolt(0);

        foreach (BlobCosmetic cosmetic in Cosmetics)
        {
            cosmetic.SetEquipped(true);
        }
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

        foreach (BlobCosmetic cosmetic in Cosmetics)
        {
            if (cosmetic.Equipped) SnapToTriangle(cosmetic.gameObject, cosmetic.Triangle);
        }

        if (atomOffsets[0] > 0) atomOffsets[0] -= .05f;

        if (transitionTimer.Update(mode: TimerMode.Pulse)) SimplifyMaterials();

        if (transitionTimer.Running)
        {
            StagedTimerState timerState = transitionTimer.State;

            if (timerState.stageName == FADE_NEW_MATERIAL)
            {
                meshRenderer.materials[0].SetFloat("_Alpha",
                    timerState.progress * materialAlphas[0]
                );
            }
            else if (timerState.stageName == FADE_OLD_MATERIAL)
            {
                meshRenderer.materials[1].SetFloat("_Alpha",
                    timerState.RemainingProgress * materialAlphas[1]
                );
            }
        }
    }

    void FixedUpdate()
    {
        atoms.ClearAllVertexCaches();
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
        (float[] offset, Vector3[] vertex) triangleAtoms = (
            new float[]
            {
                GetOffset(triangle.x),
                GetOffset(triangle.y),
                GetOffset(triangle.z)
            },
            new Vector3[]
            {
                atoms.GetVertex(triangle.x),
                atoms.GetVertex(triangle.y),
                atoms.GetVertex(triangle.z)
            }
        );
        transform.InverseTransformPoints(new Span<Vector3>(triangleAtoms.vertex));

        Vector3 position = ScaledBarycenter(triangleAtoms);
        Vector3 direction = NormalVector(triangleAtoms, position);

        cosmetic.transform.position = position;
        cosmetic.transform.LookAt(position + direction, Vector3.up);
    }

    /// <summary>
    ///     Calculates the barycenter of the triangle formed by three atoms scaled away from the
    ///     center atom.
    /// </summary>
    /// <returns>
    ///     The barycenter of the triangle as a Vector3, scaled away from the center.
    /// </returns>
    private Vector3 ScaledBarycenter((float[] offset, Vector3[] vertex) triangle)
    {
        Vector3 localBarycenter = (
            triangle.offset[0] * triangle.vertex[0] +
            triangle.offset[1] * triangle.vertex[1] +
            triangle.offset[2] * triangle.vertex[2]
        ) / 3;

        return transform.TransformPoint(localBarycenter * ScaleFactor);
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
    private Vector3 NormalVector((float[] offset, Vector3[] vertex) triangle, Vector3 barycenter)
    {
        Vector3 dirXY = triangle.offset[1] * triangle.vertex[1]
                      - triangle.offset[0] * triangle.vertex[0];
        Vector3 dirXZ = triangle.offset[2] * triangle.vertex[2]
                      - triangle.offset[0] * triangle.vertex[0];

        Vector3 normal = transform.TransformDirection(Vector3.Cross(dirXY, dirXZ).normalized);

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
            int atomIndex = vertexToAtomMap[i];
            Vector3 worldPosition = atoms.GetVertex(atomIndex);

            // Calculate local position of mesh vertices.
            newVertices[i] = GetOffset(atomIndex) * ScaleFactor * transform.InverseTransformPoint(
                worldPosition
            );
        }

        mesh.vertices = newVertices;
    }

    /// <summary>
    ///     Sets the material of the blob's mesh renderer. If the given duration is positive, then
    ///     transitions smoothly from the old material to the new material.
    /// </summary>
    /// <param name="bodyData">
    ///     The pair that defines the new material and its default alpha value.
    /// </param>
    /// <param name="duration">
    ///     How long to take during the transition from the mesh's old material to the new one.
    /// </param>
    public void SetMaterial((Material material, float alpha) bodyData,
                            float duration = DEFAULT_TRANSITION_TIME)
    {
        if (transitionTimer.Running) return;
        
        if (duration > 0 && !meshRenderer.materials[0].name.HasPrefix(bodyData.material.name))
        {
            transitionTimer.Reset(duration.Approx(transitionTimer.Interval) ? 0 : duration);

            meshRenderer.materials = new Material[] {
                bodyData.material,
                meshRenderer.materials[0]
            };
            meshRenderer.materials[0].SetFloat("_Alpha", 0);

            materialAlphas[1] = materialAlphas[0];
        }
        else
        {
            meshRenderer.materials = new Material[] {bodyData.material};
        }

        materialAlphas[0] = bodyData.alpha;
    }

    /// <summary>
    ///     Removes all but the first material defined in the mesh's material array, setting its
    ///     alpha value to the stored default.
    /// </summary>
    private void SimplifyMaterials()
    {
        meshRenderer.materials = new Material[] {meshRenderer.materials[0]};
        meshRenderer.materials[0].SetFloat("_Alpha", materialAlphas[0]);
        materialAlphas[1] = 0;
    }

    /// <summary>
    ///     Determines what the mesh's scale should be to cover all of the blob's atoms,
    ///     and modifies cosmetics if necessary.
    /// </summary>
    /// <param name="lengthScaleFactor">
    ///     The scale factor currently applied to the blob's joints.
    /// </param>
    public void Rescale(float lengthScaleFactor)
    {
        ScaleFactor = 1 + 3 * atoms.AtomScale / (2 * lengthScaleFactor * atoms.DefaultLength);
        
        foreach (BlobCosmetic cosmetic in Cosmetics)
        {
            cosmetic.SetScale(lengthScaleFactor);
        }
    }

    /// <summary>
    ///     Apply a temporary random offset to the mesh's vertices.
    /// </summary>
    /// <param name="maxOffset">
    ///     The maximum offset as a proportion of the normal offset that can be applied.
    /// </param>
    public void Jolt(float maxOffset = 0.5f)
    {
        for (int i = 1; i < atoms.Count; i++)
        {
            atomOffsets[i] = maxOffset * (2 * Random.value - 1);
        }

        atomOffsets[0] = maxOffset.Approx(0) ? 0 : 1;
    }

    /// <param name="atomIndex">
    ///     The index of the atom to get the offset of.
    /// </param>
    /// <returns>
    ///     The offset associated with the given atom index.
    /// </returns>
    private float GetOffset(int atomIndex)
    {
        if (atomOffsets[0] < FloatExtensions.EPSILON) atomOffsets[0] = 0;

        return 1 + atomOffsets[atomIndex] * atomOffsets[0];
    }

    /// <summary>
    ///     Unequip all cosmetics currently on the blob.
    /// </summary>
    public void DropCosmetics()
    {
        foreach (BlobCosmetic cosmetic in Cosmetics)
        {
            cosmetic.SetEquipped(false, fling: true);
        }
    }
}