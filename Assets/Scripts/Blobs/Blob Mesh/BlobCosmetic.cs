using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///     A class defining the behavior of cosmetics to be worn by blobs.
/// </summary>
[Serializable, Inspectable]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BlobCosmetic : MonoBehaviour
{
    /// <summary>
    ///     The index of the blob face that this cosmetic will be anchored to while equipped.
    /// </summary>
    public int anchorTriangle;

    /// <summary>
    ///     The triplet of atom indices that form the blob face that this cosmetic will be anchored
    ///     to while equipped.
    /// </summary>
    public Vector3Int Triangle => triangles[anchorTriangle];

    /// <summary>
    ///     <tt>True</tt> iff this cosmetic is equipped to a blob.
    /// </summary>
    public bool Equipped { get; private set; }

    /// <summary>
    ///     The triples of atom indices that form faces on the blob's icosahedron mesh.
    /// </summary>
    private static readonly Vector3Int[] triangles = new Vector3Int[]
    {
        new(1, 3, 11),  //  0
        new(1, 6, 12),  //  1
        new(1, 8, 6),   //  2
        new(1, 11, 8),  //  3
        new(1, 12, 3),  //  4
        new(3, 5, 7),   //  5
        new(6, 2, 10),  //  6
        new(8, 9, 2),   //  7
        new(11, 8, 9),  //  8
        new(12, 10, 5), //  9
        new(2, 8, 6),   // 10
        new(5, 12, 3),  // 11
        new(7, 3, 11),  // 12
        new(9, 11, 8),  // 13
        new(10, 6, 12), // 14
        new(4, 2, 10),  // 15
        new(4, 5, 7),   // 16
        new(4, 7, 9),   // 17
        new(4, 9, 2),   // 18
        new(4, 10, 5)   // 19
    };

    private Collider cosmeticCollider;
    private Rigidbody cosmeticRigidbody;

    /// <summary>
    ///     The initial scale of this cosmetic.
    /// </summary>
    private Vector3 initialScale;
    private const float FLING_POWER = 15;

    void Awake()
    {
        cosmeticCollider = GetComponent<Collider>();
        cosmeticRigidbody = GetComponent<Rigidbody>();

        initialScale = gameObject.transform.localScale;
    }

    /// <summary>
    ///     Enable/Disable the cosmetic's physics and (if unequipping and flinging) send it flying.
    /// </summary>
    /// <param name="equip">
    ///     <tt>True</tt> iff the cosmetic should become equipped.
    /// </param>
    /// <param name="fling">
    ///     <tt>True</tt> iff the cosmetic should be sent flying when unequipping.
    /// </param>
    public void SetEquipped(bool equip, bool fling = false) {
        Equipped = equip;
        cosmeticCollider.enabled = !equip;
        cosmeticRigidbody.isKinematic = equip;
        cosmeticRigidbody.useGravity = !equip;

        if (equip || !fling) return;

        Vector3 flingDirection = Random.onUnitSphere;
        flingDirection.y = Mathf.Abs(flingDirection.y);
        float flingForce = FLING_POWER * (Random.value + 0.5f);

        cosmeticRigidbody.AddForce(flingForce * flingDirection, ForceMode.Impulse);
        cosmeticRigidbody.AddTorque(flingDirection);
    }

    /// <summary>
    ///     Sets the scale of the cosmetic.
    /// </summary>
    public void SetScale(float scale)
    {
        gameObject.transform.localScale = initialScale * scale;
    }
}